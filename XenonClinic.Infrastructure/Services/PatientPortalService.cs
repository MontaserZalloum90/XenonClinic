using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Patient Portal operations
/// </summary>
public class PatientPortalService : IPatientPortalService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<PatientPortalService> _logger;

    public PatientPortalService(
        ClinicDbContext context,
        ILogger<PatientPortalService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Authentication

    public async Task<PortalRegistrationResponseDto> RegisterAsync(int branchId, PortalRegistrationDto dto)
    {
        // Use transaction to ensure data consistency
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate email format
            if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
            {
                return new PortalRegistrationResponseDto
                {
                    Success = false,
                    Message = "Please provide a valid email address."
                };
            }

            // Validate password strength
            if (!IsPasswordStrong(dto.Password))
            {
                return new PortalRegistrationResponseDto
                {
                    Success = false,
                    Message = "Password must be at least 8 characters with uppercase, lowercase, number, and special character."
                };
            }

            // Find patient by MRN and date of birth
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.BranchId == branchId &&
                    (p.MRN == dto.MRN || p.Email == dto.Email) &&
                    p.DateOfBirth.Date == dto.DateOfBirth.Date);

            if (patient == null)
            {
                return new PortalRegistrationResponseDto
                {
                    Success = false,
                    Message = "Patient not found. Please verify your MRN, email, and date of birth."
                };
            }

            // Check if already registered (use locking to prevent race conditions)
            var existingAccount = await _context.PortalAccounts
                .FirstOrDefaultAsync(pa => pa.PatientId == patient.Id || pa.Email == dto.Email);

            if (existingAccount != null)
            {
                return new PortalRegistrationResponseDto
                {
                    Success = false,
                    Message = existingAccount.PatientId == patient.Id
                        ? "An account already exists for this patient."
                        : "This email is already registered."
                };
            }

            // Create portal account with hashed password
            var passwordHash = HashPassword(dto.Password);
            var verificationToken = GenerateToken();

            var portalAccount = new PortalAccount
            {
                PatientId = patient.Id,
                Email = dto.Email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                EmailVerificationToken = HashTokenForStorage(verificationToken), // Store hashed token
                IsEmailVerified = false,
                IsActive = true,
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.PortalAccounts.Add(portalAccount);

            // Update patient email if different
            if (!string.Equals(patient.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                patient.Email = dto.Email.Trim();
                patient.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Patient portal account created for PatientId: {PatientId}", patient.Id);

            // Return the plain verification token (not hashed) for sending via email
            return new PortalRegistrationResponseDto
            {
                Success = true,
                PatientId = patient.Id,
                Message = "Registration successful. Please check your email to verify your account.",
                RequiresVerification = true,
                VerificationToken = verificationToken // For email sending (would normally be sent via email service)
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error registering patient portal account");
            return new PortalRegistrationResponseDto
            {
                Success = false,
                Message = "An error occurred during registration. Please try again."
            };
        }
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check password strength requirements
    /// </summary>
    private static bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    public async Task<PortalLoginResponseDto> LoginAsync(PortalLoginDto dto)
    {
        try
        {
            var account = await _context.PortalAccounts
                .Include(pa => pa.Patient)
                .FirstOrDefaultAsync(pa => pa.Email == dto.Email && pa.IsActive);

            if (account == null || !VerifyPassword(dto.Password, account.PasswordHash))
            {
                // Log failed attempt for security monitoring
                _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
                return new PortalLoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            if (!account.IsEmailVerified)
            {
                return new PortalLoginResponseDto
                {
                    Success = false,
                    Message = "Please verify your email before logging in."
                };
            }

            // Upgrade password hash if using legacy format
            if (NeedsPasswordUpgrade(account.PasswordHash))
            {
                account.PasswordHash = HashPassword(dto.Password);
                _logger.LogInformation("Upgraded password hash for PatientId: {PatientId}", account.PatientId);
            }

            // Generate tokens
            var sessionToken = GenerateToken();
            var refreshToken = GenerateToken();
            var expiresAt = DateTime.UtcNow.AddHours(dto.RememberMe ? 168 : 24);

            // Store session token hash for validation (don't store plaintext)
            account.SessionTokenHash = HashTokenForStorage(sessionToken);
            account.SessionTokenExpiresAt = expiresAt;
            account.LastLoginAt = DateTime.UtcNow;
            account.RefreshToken = HashTokenForStorage(refreshToken);
            account.RefreshTokenExpiresAt = expiresAt.AddDays(7);
            account.FailedLoginAttempts = 0; // Reset failed attempts on successful login
            await _context.SaveChangesAsync();

            var profile = await GetProfileAsync(account.PatientId);

            _logger.LogInformation("Successful login for PatientId: {PatientId}", account.PatientId);

            return new PortalLoginResponseDto
            {
                Success = true,
                Token = sessionToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                Profile = profile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during patient portal login");
            return new PortalLoginResponseDto
            {
                Success = false,
                Message = "An error occurred during login. Please try again."
            };
        }
    }

    /// <summary>
    /// Hash token for secure storage (one-way hash)
    /// </summary>
    private static string HashTokenForStorage(string token)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Verify token against stored hash
    /// </summary>
    private static bool VerifyTokenHash(string token, string storedHash)
    {
        var computedHash = HashTokenForStorage(token);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(storedHash));
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var account = await _context.PortalAccounts
            .FirstOrDefaultAsync(pa => pa.EmailVerificationToken == token);

        if (account == null)
            return false;

        account.IsEmailVerified = true;
        account.EmailVerificationToken = null;
        account.EmailVerifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var account = await _context.PortalAccounts
            .FirstOrDefaultAsync(pa => pa.Email == email && pa.IsActive);

        if (account == null)
            return true; // Don't reveal if account exists

        account.PasswordResetToken = GenerateToken();
        account.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        // TODO: Send password reset email
        _logger.LogInformation("Password reset requested for PatientId: {PatientId}", account.PatientId);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var account = await _context.PortalAccounts
            .FirstOrDefaultAsync(pa => pa.PasswordResetToken == token &&
                pa.PasswordResetTokenExpiresAt > DateTime.UtcNow);

        if (account == null)
            return false;

        account.PasswordHash = HashPassword(newPassword);
        account.PasswordResetToken = null;
        account.PasswordResetTokenExpiresAt = null;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ChangePasswordAsync(int patientId, string currentPassword, string newPassword)
    {
        var account = await _context.PortalAccounts
            .FirstOrDefaultAsync(pa => pa.PatientId == patientId && pa.IsActive);

        if (account == null || !VerifyPassword(currentPassword, account.PasswordHash))
            return false;

        account.PasswordHash = HashPassword(newPassword);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PortalLoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var account = await _context.PortalAccounts
            .Include(pa => pa.Patient)
            .FirstOrDefaultAsync(pa => pa.RefreshToken == refreshToken &&
                pa.RefreshTokenExpiresAt > DateTime.UtcNow &&
                pa.IsActive);

        if (account == null)
        {
            return new PortalLoginResponseDto
            {
                Success = false,
                Message = "Invalid or expired refresh token."
            };
        }

        var newToken = GenerateToken();
        var newRefreshToken = GenerateToken();
        var expiresAt = DateTime.UtcNow.AddHours(24);

        account.RefreshToken = newRefreshToken;
        account.RefreshTokenExpiresAt = expiresAt.AddDays(7);
        await _context.SaveChangesAsync();

        var profile = await GetProfileAsync(account.PatientId);

        return new PortalLoginResponseDto
        {
            Success = true,
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            Profile = profile
        };
    }

    #endregion

    #region Profile & Dashboard

    public async Task<PatientPortalProfileDto> GetProfileAsync(int patientId)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
            return new PatientPortalProfileDto();

        var account = await _context.PortalAccounts
            .FirstOrDefaultAsync(pa => pa.PatientId == patientId);

        return new PatientPortalProfileDto
        {
            PatientId = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Address = patient.Address,
            City = patient.City,
            Country = patient.Country,
            MRN = patient.MRN,
            BloodType = patient.BloodType,
            Allergies = patient.Allergies,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            ProfilePhotoUrl = patient.ProfilePhotoUrl,
            IsEmailVerified = account?.IsEmailVerified ?? false,
            IsSmsOptIn = patient.IsSmsOptIn,
            IsEmailOptIn = patient.IsEmailOptIn,
            LastLoginAt = account?.LastLoginAt
        };
    }

    public async Task<PatientPortalProfileDto> UpdateProfileAsync(int patientId, UpdatePatientProfileDto dto)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
            return new PatientPortalProfileDto();

        if (dto.PhoneNumber != null) patient.PhoneNumber = dto.PhoneNumber;
        if (dto.Address != null) patient.Address = dto.Address;
        if (dto.City != null) patient.City = dto.City;
        if (dto.Country != null) patient.Country = dto.Country;
        if (dto.EmergencyContactName != null) patient.EmergencyContactName = dto.EmergencyContactName;
        if (dto.EmergencyContactPhone != null) patient.EmergencyContactPhone = dto.EmergencyContactPhone;
        if (dto.IsSmsOptIn.HasValue) patient.IsSmsOptIn = dto.IsSmsOptIn.Value;
        if (dto.IsEmailOptIn.HasValue) patient.IsEmailOptIn = dto.IsEmailOptIn.Value;

        patient.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetProfileAsync(patientId);
    }

    public async Task<PatientPortalDashboardDto> GetDashboardAsync(int patientId)
    {
        var profile = await GetProfileAsync(patientId);
        var upcomingAppointments = await GetUpcomingAppointmentsAsync(patientId);
        var activeMedications = await GetActiveMedicationsAsync(patientId);
        var unreadMessages = await GetUnreadMessageCountAsync(patientId);
        var outstandingBalance = await GetOutstandingBalanceAsync(patientId);

        var pendingInvoices = await _context.Invoices
            .Where(i => i.PatientId == patientId &&
                (i.Status == "Pending" || i.Status == "Overdue"))
            .CountAsync();

        var notifications = await GetNotificationsAsync(patientId, true);

        return new PatientPortalDashboardDto
        {
            Profile = profile,
            UpcomingAppointments = upcomingAppointments.Count(),
            PendingPrescriptions = 0, // TODO: Implement pending prescription count
            UnreadMessages = unreadMessages,
            PendingInvoices = pendingInvoices,
            OutstandingBalance = outstandingBalance,
            NextAppointments = upcomingAppointments.Take(3).ToList(),
            ActiveMedications = activeMedications.Take(5).ToList(),
            RecentNotifications = notifications.Take(5).ToList()
        };
    }

    // File upload security constants
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };
    private const int MaxProfilePhotoSizeBytes = 5 * 1024 * 1024; // 5MB

    public async Task<string> UploadProfilePhotoAsync(int patientId, byte[] photoData, string fileName)
    {
        // Validate patient exists
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
        {
            _logger.LogWarning("Profile photo upload attempted for non-existent patient: {PatientId}", patientId);
            return string.Empty;
        }

        // Validate file data
        if (photoData == null || photoData.Length == 0)
        {
            _logger.LogWarning("Empty file upload attempted for patient: {PatientId}", patientId);
            throw new InvalidOperationException("No file data provided");
        }

        // Validate file size
        if (photoData.Length > MaxProfilePhotoSizeBytes)
        {
            _logger.LogWarning("File too large for patient {PatientId}: {Size} bytes", patientId, photoData.Length);
            throw new InvalidOperationException($"File size exceeds maximum allowed ({MaxProfilePhotoSizeBytes / 1024 / 1024}MB)");
        }

        // Validate and sanitize filename (prevent path traversal)
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("Invalid filename");

        var sanitizedFileName = Path.GetFileName(fileName); // Remove any path components
        var extension = Path.GetExtension(sanitizedFileName)?.ToLowerInvariant();

        // Validate extension
        if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
        {
            _logger.LogWarning("Invalid file extension attempted for patient {PatientId}: {Extension}", patientId, extension);
            throw new InvalidOperationException($"Invalid file type. Allowed types: {string.Join(", ", AllowedImageExtensions)}");
        }

        // Check for double extension attack (e.g., image.jpg.exe)
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitizedFileName);
        if (nameWithoutExtension.Contains('.'))
        {
            _logger.LogWarning("Possible double extension attack for patient {PatientId}: {FileName}", patientId, fileName);
            throw new InvalidOperationException("Invalid filename format");
        }

        // Validate file signature (magic bytes) to ensure it's a real image
        if (!ValidateImageSignature(photoData, extension))
        {
            _logger.LogWarning("File signature mismatch for patient {PatientId}", patientId);
            throw new InvalidOperationException("File content does not match declared file type");
        }

        // Generate secure unique file name (no user input in final name)
        var uniqueFileName = $"profile_{patientId}_{Guid.NewGuid():N}{extension}";

        // TODO: Upload to storage service (Azure Blob, S3, etc.)
        var photoUrl = $"/uploads/profiles/{uniqueFileName}";

        patient.ProfilePhotoUrl = photoUrl;
        patient.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Profile photo uploaded for patient {PatientId}", patientId);

        return photoUrl;
    }

    /// <summary>
    /// Validate image file signature (magic bytes)
    /// </summary>
    private static bool ValidateImageSignature(byte[] data, string extension)
    {
        if (data.Length < 8)
            return false;

        return extension switch
        {
            ".jpg" or ".jpeg" => data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
            ".png" => data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47,
            ".gif" => data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46,
            ".webp" => data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                       data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50,
            _ => false
        };
    }

    #endregion

    #region Appointments

    public async Task<IEnumerable<PortalAppointmentSummaryDto>> GetUpcomingAppointmentsAsync(int patientId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Where(a => a.PatientId == patientId &&
                a.AppointmentDate >= DateTime.Today &&
                a.Status != "Cancelled")
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Take(10)
            .ToListAsync();

        return appointments.Select(a => new PortalAppointmentSummaryDto
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            DoctorName = $"Dr. {a.Doctor?.FirstName} {a.Doctor?.LastName}",
            DoctorSpecialty = a.Doctor?.Specialty,
            AppointmentType = a.AppointmentType ?? "Regular",
            Status = a.Status ?? "Scheduled",
            IsTelemedicine = a.IsTelemedicine
        });
    }

    public async Task<IEnumerable<PortalAppointmentSummaryDto>> GetPastAppointmentsAsync(int patientId, int limit = 20)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Where(a => a.PatientId == patientId &&
                a.AppointmentDate < DateTime.Today)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.StartTime)
            .Take(limit)
            .ToListAsync();

        return appointments.Select(a => new PortalAppointmentSummaryDto
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            DoctorName = $"Dr. {a.Doctor?.FirstName} {a.Doctor?.LastName}",
            DoctorSpecialty = a.Doctor?.Specialty,
            AppointmentType = a.AppointmentType ?? "Regular",
            Status = a.Status ?? "Completed",
            IsTelemedicine = a.IsTelemedicine
        });
    }

    public async Task<PortalAppointmentDto?> GetAppointmentAsync(int patientId, int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);

        if (appointment == null)
            return null;

        var canModify = appointment.AppointmentDate > DateTime.Today.AddDays(1) &&
                        appointment.Status != "Cancelled" &&
                        appointment.Status != "Completed";

        return new PortalAppointmentDto
        {
            Id = appointment.Id,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            DoctorName = $"Dr. {appointment.Doctor?.FirstName} {appointment.Doctor?.LastName}",
            DoctorSpecialty = appointment.Doctor?.Specialty,
            DoctorPhotoUrl = appointment.Doctor?.ProfilePhotoUrl,
            Department = appointment.Department,
            Location = appointment.Location,
            AppointmentType = appointment.AppointmentType ?? "Regular",
            Status = appointment.Status ?? "Scheduled",
            Reason = appointment.Reason,
            Notes = appointment.Notes,
            CanCancel = canModify,
            CanReschedule = canModify,
            IsTelemedicine = appointment.IsTelemedicine,
            TelemedicineLink = appointment.IsTelemedicine ? appointment.TelemedicineLink : null,
            PreVisitInstructions = appointment.PreVisitInstructions
        };
    }

    public async Task<IEnumerable<PortalAppointmentSlotDto>> GetAvailableSlotsAsync(
        int branchId, int doctorId, DateTime startDate, DateTime endDate)
    {
        var slots = new List<PortalAppointmentSlotDto>();

        // Get doctor's schedule
        var schedule = await _context.DoctorSchedules
            .Where(ds => ds.DoctorId == doctorId && ds.BranchId == branchId && ds.IsActive)
            .ToListAsync();

        // Get existing appointments
        var existingAppointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                a.AppointmentDate >= startDate &&
                a.AppointmentDate <= endDate &&
                a.Status != "Cancelled")
            .Select(a => new { a.AppointmentDate, a.StartTime })
            .ToListAsync();

        var doctor = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == doctorId);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var daySchedule = schedule.FirstOrDefault(s => s.DayOfWeek == date.DayOfWeek);
            if (daySchedule == null) continue;

            // Generate slots based on schedule
            var slotDuration = daySchedule.SlotDurationMinutes > 0 ? daySchedule.SlotDurationMinutes : 30;
            var startTime = TimeSpan.Parse(daySchedule.StartTime ?? "09:00");
            var endTime = TimeSpan.Parse(daySchedule.EndTime ?? "17:00");

            while (startTime < endTime)
            {
                var slotEndTime = startTime.Add(TimeSpan.FromMinutes(slotDuration));
                var slotTimeStr = startTime.ToString(@"hh\:mm");

                var isBooked = existingAppointments.Any(a =>
                    a.AppointmentDate.Date == date.Date &&
                    a.StartTime == slotTimeStr);

                slots.Add(new PortalAppointmentSlotDto
                {
                    Date = date,
                    StartTime = slotTimeStr,
                    EndTime = slotEndTime.ToString(@"hh\:mm"),
                    DoctorId = doctorId,
                    DoctorName = $"Dr. {doctor?.FirstName} {doctor?.LastName}",
                    IsAvailable = !isBooked,
                    IsTelemedicineAvailable = daySchedule.AllowsTelemedicine
                });

                startTime = slotEndTime;
            }
        }

        return slots;
    }

    public async Task<PortalAppointmentDto> BookAppointmentAsync(int patientId, PortalBookAppointmentDto dto)
    {
        // Use transaction to prevent race conditions
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                throw new InvalidOperationException("Patient not found");

            // Validate doctor exists and is active
            var doctor = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.DoctorId && u.Role == "Doctor" && u.IsActive);

            if (doctor == null)
                throw new InvalidOperationException("Doctor not found or not available");

            // Validate appointment date is in the future
            if (dto.PreferredDate.Date < DateTime.Today)
                throw new InvalidOperationException("Cannot book appointments in the past");

            // Check for double-booking (same doctor, same date, same time)
            var existingAppointment = await _context.Appointments
                .AnyAsync(a => a.DoctorId == dto.DoctorId &&
                    a.AppointmentDate.Date == dto.PreferredDate.Date &&
                    a.StartTime == dto.PreferredTime &&
                    a.Status != "Cancelled");

            if (existingAppointment)
                throw new InvalidOperationException("This time slot is no longer available. Please select another time.");

            // Check if patient already has an appointment at this time
            var patientConflict = await _context.Appointments
                .AnyAsync(a => a.PatientId == patientId &&
                    a.AppointmentDate.Date == dto.PreferredDate.Date &&
                    a.StartTime == dto.PreferredTime &&
                    a.Status != "Cancelled");

            if (patientConflict)
                throw new InvalidOperationException("You already have an appointment scheduled at this time.");

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = dto.DoctorId,
                BranchId = patient.BranchId,
                AppointmentDate = dto.PreferredDate,
                StartTime = dto.PreferredTime,
                AppointmentType = dto.AppointmentType ?? "Regular",
                Reason = SanitizeInput(dto.Reason),
                Notes = SanitizeInput(dto.Notes),
                IsTelemedicine = dto.IsTelemedicine,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Create notification (outside transaction)
            await CreateNotificationAsync(patientId, "Appointment Booked",
                $"Your appointment has been scheduled for {dto.PreferredDate:MMM dd, yyyy}",
                "Appointment", $"/appointments/{appointment.Id}");

            _logger.LogInformation("Appointment booked: PatientId={PatientId}, DoctorId={DoctorId}, Date={Date}",
                patientId, dto.DoctorId, dto.PreferredDate);

            return (await GetAppointmentAsync(patientId, appointment.Id))!;
        }
        catch (InvalidOperationException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error booking appointment for PatientId: {PatientId}", patientId);
            throw new InvalidOperationException("An error occurred while booking your appointment. Please try again.");
        }
    }

    /// <summary>
    /// Sanitize user input to prevent XSS
    /// </summary>
    private static string? SanitizeInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Basic sanitization - remove script tags and encode HTML entities
        return System.Net.WebUtility.HtmlEncode(input.Trim());
    }

    public async Task<bool> CancelAppointmentAsync(int patientId, int appointmentId, string? reason)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);

        if (appointment == null || appointment.Status == "Cancelled")
            return false;

        appointment.Status = "Cancelled";
        appointment.CancellationReason = reason;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancelledByPatient = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PortalAppointmentDto> RescheduleAppointmentAsync(
        int patientId, int appointmentId, DateTime newDate, string? newTime)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);

        if (appointment == null)
            throw new InvalidOperationException("Appointment not found");

        appointment.AppointmentDate = newDate;
        appointment.StartTime = newTime;
        appointment.RescheduledAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (await GetAppointmentAsync(patientId, appointmentId))!;
    }

    public async Task<IEnumerable<PortalDoctorDto>> GetDoctorsForBookingAsync(int branchId, string? specialty = null)
    {
        var query = _context.Users
            .Where(u => u.BranchId == branchId && u.Role == "Doctor" && u.IsActive);

        if (!string.IsNullOrEmpty(specialty))
        {
            query = query.Where(u => u.Specialty == specialty);
        }

        var doctors = await query.ToListAsync();

        return doctors.Select(d => new PortalDoctorDto
        {
            Id = d.Id,
            Name = $"Dr. {d.FirstName} {d.LastName}",
            Specialty = d.Specialty,
            PhotoUrl = d.ProfilePhotoUrl,
            Bio = d.Bio,
            AcceptsNewPatients = d.AcceptsNewPatients,
            OffersTelemedicine = d.OffersTelemedicine,
            ConsultationFee = d.ConsultationFee
        });
    }

    #endregion

    #region Medical Records

    public async Task<PortalMedicalRecordsSummaryDto> GetMedicalRecordsSummaryAsync(int patientId)
    {
        var recentVisits = await GetVisitHistoryAsync(patientId, null, 5);
        var activeDiagnoses = await GetActiveDiagnosesAsync(patientId);
        var allergies = await GetAllergiesAsync(patientId);
        var immunizations = await GetImmunizationsAsync(patientId);
        var vitals = await GetVitalSignsHistoryAsync(patientId, 1);

        return new PortalMedicalRecordsSummaryDto
        {
            RecentVisits = recentVisits.ToList(),
            ActiveDiagnoses = activeDiagnoses.ToList(),
            Allergies = allergies.ToList(),
            Immunizations = immunizations.ToList(),
            LatestVitals = vitals.FirstOrDefault()
        };
    }

    public async Task<IEnumerable<PortalVisitSummaryDto>> GetVisitHistoryAsync(int patientId, int? year = null, int limit = 50)
    {
        var query = _context.Visits
            .Include(v => v.Doctor)
            .Where(v => v.PatientId == patientId);

        if (year.HasValue)
        {
            query = query.Where(v => v.VisitDate.Year == year.Value);
        }

        var visits = await query
            .OrderByDescending(v => v.VisitDate)
            .Take(limit)
            .ToListAsync();

        return visits.Select(v => new PortalVisitSummaryDto
        {
            Id = v.Id,
            VisitDate = v.VisitDate,
            DoctorName = $"Dr. {v.Doctor?.FirstName} {v.Doctor?.LastName}",
            Specialty = v.Doctor?.Specialty,
            ChiefComplaint = v.ChiefComplaint,
            VisitType = v.VisitType,
            Status = v.Status ?? "Completed",
            HasDiagnosis = v.HasDiagnosis,
            HasPrescription = v.HasPrescription,
            HasLabOrders = v.HasLabOrders
        });
    }

    public async Task<PortalVisitDetailDto?> GetVisitDetailAsync(int patientId, int visitId)
    {
        var visit = await _context.Visits
            .Include(v => v.Doctor)
            .Include(v => v.Diagnoses)
            .Include(v => v.VitalSigns)
            .FirstOrDefaultAsync(v => v.Id == visitId && v.PatientId == patientId);

        if (visit == null)
            return null;

        var prescriptions = await _context.Prescriptions
            .Include(p => p.Items)
            .Where(p => p.VisitId == visitId)
            .ToListAsync();

        var labResults = await _context.LabOrders
            .Where(l => l.VisitId == visitId)
            .ToListAsync();

        return new PortalVisitDetailDto
        {
            Id = visit.Id,
            VisitDate = visit.VisitDate,
            DoctorName = $"Dr. {visit.Doctor?.FirstName} {visit.Doctor?.LastName}",
            Specialty = visit.Doctor?.Specialty,
            ChiefComplaint = visit.ChiefComplaint,
            HistoryOfPresentIllness = visit.HistoryOfPresentIllness,
            Assessment = visit.Assessment,
            Plan = visit.Plan,
            Diagnoses = visit.Diagnoses?.Select(d => new PortalDiagnosisSummaryDto
            {
                Id = d.Id,
                DiagnosisName = d.DiagnosisName,
                ICD10Code = d.ICD10Code,
                DiagnosisDate = d.DiagnosisDate,
                Status = d.Status ?? "Active"
            }).ToList() ?? new List<PortalDiagnosisSummaryDto>(),
            Prescriptions = prescriptions.SelectMany(p => p.Items?.Select(i => new PortalMedicationSummaryDto
            {
                Id = i.Id,
                MedicationName = i.MedicationName,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                Route = i.Route,
                StartDate = p.PrescriptionDate,
                PrescribingDoctor = $"Dr. {visit.Doctor?.FirstName} {visit.Doctor?.LastName}"
            }) ?? Enumerable.Empty<PortalMedicationSummaryDto>()).ToList(),
            VitalSigns = visit.VitalSigns != null ? new PortalVitalSignsDto
            {
                RecordedAt = visit.VitalSigns.RecordedAt,
                BloodPressureSystolic = visit.VitalSigns.BloodPressureSystolic,
                BloodPressureDiastolic = visit.VitalSigns.BloodPressureDiastolic,
                HeartRate = visit.VitalSigns.HeartRate,
                Temperature = visit.VitalSigns.Temperature,
                RespiratoryRate = visit.VitalSigns.RespiratoryRate,
                OxygenSaturation = visit.VitalSigns.OxygenSaturation,
                Weight = visit.VitalSigns.Weight,
                Height = visit.VitalSigns.Height,
                BMI = visit.VitalSigns.BMI
            } : null,
            LabResults = labResults.Select(l => new PortalLabResultSummaryDto
            {
                Id = l.Id,
                TestName = l.TestName,
                OrderDate = l.OrderDate,
                ResultDate = l.ResultDate,
                Status = l.Status ?? "Pending",
                OrderingDoctor = $"Dr. {visit.Doctor?.FirstName} {visit.Doctor?.LastName}",
                IsAbnormal = l.IsAbnormal
            }).ToList(),
            FollowUpInstructions = visit.FollowUpInstructions,
            NextFollowUpDate = visit.NextFollowUpDate
        };
    }

    public async Task<IEnumerable<PortalDiagnosisSummaryDto>> GetActiveDiagnosesAsync(int patientId)
    {
        var diagnoses = await _context.Diagnoses
            .Include(d => d.Visit)
                .ThenInclude(v => v!.Doctor)
            .Where(d => d.PatientId == patientId &&
                (d.Status == "Active" || d.Status == "Chronic"))
            .OrderByDescending(d => d.DiagnosisDate)
            .ToListAsync();

        return diagnoses.Select(d => new PortalDiagnosisSummaryDto
        {
            Id = d.Id,
            DiagnosisName = d.DiagnosisName,
            ICD10Code = d.ICD10Code,
            DiagnosisDate = d.DiagnosisDate,
            DoctorName = d.Visit?.Doctor != null
                ? $"Dr. {d.Visit.Doctor.FirstName} {d.Visit.Doctor.LastName}"
                : null,
            Status = d.Status ?? "Active"
        });
    }

    public async Task<IEnumerable<PortalAllergyDto>> GetAllergiesAsync(int patientId)
    {
        var allergies = await _context.PatientAllergies
            .Where(a => a.PatientId == patientId && a.IsActive)
            .OrderBy(a => a.AllergyName)
            .ToListAsync();

        return allergies.Select(a => new PortalAllergyDto
        {
            Id = a.Id,
            AllergyName = a.AllergyName,
            AllergyType = a.AllergyType,
            Severity = a.Severity,
            Reaction = a.Reaction,
            RecordedDate = a.RecordedDate
        });
    }

    public async Task<IEnumerable<PortalImmunizationDto>> GetImmunizationsAsync(int patientId)
    {
        var immunizations = await _context.Immunizations
            .Include(i => i.AdministeredByUser)
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.AdministeredDate)
            .ToListAsync();

        return immunizations.Select(i => new PortalImmunizationDto
        {
            Id = i.Id,
            VaccineName = i.VaccineName,
            AdministeredDate = i.AdministeredDate,
            DoseNumber = i.DoseNumber,
            LotNumber = i.LotNumber,
            AdministeredBy = i.AdministeredByUser != null
                ? $"{i.AdministeredByUser.FirstName} {i.AdministeredByUser.LastName}"
                : null,
            NextDueDate = i.NextDueDate
        });
    }

    public async Task<IEnumerable<PortalVitalSignsDto>> GetVitalSignsHistoryAsync(int patientId, int limit = 20)
    {
        var vitals = await _context.VitalSigns
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.RecordedAt)
            .Take(limit)
            .ToListAsync();

        return vitals.Select(v => new PortalVitalSignsDto
        {
            RecordedAt = v.RecordedAt,
            BloodPressureSystolic = v.BloodPressureSystolic,
            BloodPressureDiastolic = v.BloodPressureDiastolic,
            HeartRate = v.HeartRate,
            Temperature = v.Temperature,
            RespiratoryRate = v.RespiratoryRate,
            OxygenSaturation = v.OxygenSaturation,
            Weight = v.Weight,
            Height = v.Height,
            BMI = v.BMI
        });
    }

    public async Task<byte[]> DownloadMedicalRecordsAsync(int patientId, string format = "pdf")
    {
        // TODO: Implement PDF generation using a library like iTextSharp or QuestPDF
        _logger.LogInformation("Medical records download requested for PatientId: {PatientId}, Format: {Format}",
            patientId, format);

        // Placeholder - return empty byte array
        return Array.Empty<byte>();
    }

    #endregion

    #region Lab Results

    public async Task<IEnumerable<PortalLabResultSummaryDto>> GetLabResultsAsync(int patientId, int? year = null, int limit = 50)
    {
        var query = _context.LabOrders
            .Include(l => l.OrderingDoctor)
            .Where(l => l.PatientId == patientId);

        if (year.HasValue)
        {
            query = query.Where(l => l.OrderDate.Year == year.Value);
        }

        var labResults = await query
            .OrderByDescending(l => l.OrderDate)
            .Take(limit)
            .ToListAsync();

        return labResults.Select(l => new PortalLabResultSummaryDto
        {
            Id = l.Id,
            TestName = l.TestName,
            OrderDate = l.OrderDate,
            ResultDate = l.ResultDate,
            Status = l.Status ?? "Pending",
            OrderingDoctor = l.OrderingDoctor != null
                ? $"Dr. {l.OrderingDoctor.FirstName} {l.OrderingDoctor.LastName}"
                : null,
            IsAbnormal = l.IsAbnormal
        });
    }

    public async Task<PortalLabResultDetailDto?> GetLabResultDetailAsync(int patientId, int labResultId)
    {
        var labOrder = await _context.LabOrders
            .Include(l => l.OrderingDoctor)
            .Include(l => l.Results)
            .FirstOrDefaultAsync(l => l.Id == labResultId && l.PatientId == patientId);

        if (labOrder == null)
            return null;

        return new PortalLabResultDetailDto
        {
            Id = labOrder.Id,
            TestName = labOrder.TestName,
            TestCode = labOrder.TestCode,
            OrderDate = labOrder.OrderDate,
            ResultDate = labOrder.ResultDate,
            OrderingDoctor = labOrder.OrderingDoctor != null
                ? $"Dr. {labOrder.OrderingDoctor.FirstName} {labOrder.OrderingDoctor.LastName}"
                : null,
            Status = labOrder.Status ?? "Pending",
            Results = labOrder.Results?.Select(r => new PortalLabResultItemDto
            {
                ComponentName = r.ComponentName,
                Value = r.Value,
                Unit = r.Unit,
                ReferenceRange = r.ReferenceRange,
                Flag = r.Flag,
                IsAbnormal = r.IsAbnormal
            }).ToList() ?? new List<PortalLabResultItemDto>(),
            Comments = labOrder.Comments,
            CanDownloadReport = labOrder.Status == "Completed"
        };
    }

    public async Task<byte[]> DownloadLabReportAsync(int patientId, int labResultId)
    {
        var labOrder = await _context.LabOrders
            .FirstOrDefaultAsync(l => l.Id == labResultId && l.PatientId == patientId);

        if (labOrder == null)
            return Array.Empty<byte>();

        // TODO: Implement PDF generation
        _logger.LogInformation("Lab report download requested for PatientId: {PatientId}, LabResultId: {LabResultId}",
            patientId, labResultId);

        return Array.Empty<byte>();
    }

    #endregion

    #region Prescriptions

    public async Task<IEnumerable<PortalMedicationSummaryDto>> GetActiveMedicationsAsync(int patientId)
    {
        var medications = await _context.PrescriptionItems
            .Include(pi => pi.Prescription)
                .ThenInclude(p => p!.Doctor)
            .Where(pi => pi.Prescription!.PatientId == patientId &&
                pi.Status == "Active" &&
                (pi.EndDate == null || pi.EndDate > DateTime.Today))
            .OrderBy(pi => pi.MedicationName)
            .ToListAsync();

        return medications.Select(m => new PortalMedicationSummaryDto
        {
            Id = m.Id,
            MedicationName = m.MedicationName,
            Dosage = m.Dosage,
            Frequency = m.Frequency,
            Route = m.Route,
            StartDate = m.StartDate ?? m.Prescription!.PrescriptionDate,
            EndDate = m.EndDate,
            Status = m.Status ?? "Active",
            PrescribingDoctor = m.Prescription?.Doctor != null
                ? $"Dr. {m.Prescription.Doctor.FirstName} {m.Prescription.Doctor.LastName}"
                : null,
            RefillsRemaining = m.RefillsRemaining,
            CanRequestRefill = m.RefillsRemaining > 0
        });
    }

    public async Task<IEnumerable<PortalPrescriptionDetailDto>> GetPrescriptionHistoryAsync(int patientId, int limit = 20)
    {
        var prescriptions = await _context.Prescriptions
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PrescriptionDate)
            .Take(limit)
            .ToListAsync();

        return prescriptions.Select(p => new PortalPrescriptionDetailDto
        {
            Id = p.Id,
            PrescriptionDate = p.PrescriptionDate,
            PrescribingDoctor = $"Dr. {p.Doctor?.FirstName} {p.Doctor?.LastName}",
            DiagnosisRelated = p.DiagnosisRelated,
            Medications = p.Items?.Select(i => new PortalPrescriptionItemDto
            {
                Id = i.Id,
                MedicationName = i.MedicationName,
                GenericName = i.GenericName,
                Strength = i.Strength,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                Duration = i.Duration,
                Quantity = i.Quantity,
                Refills = i.Refills,
                Instructions = i.Instructions
            }).ToList() ?? new List<PortalPrescriptionItemDto>(),
            Instructions = p.Instructions,
            CanRequestRefill = p.Items?.Any(i => i.RefillsRemaining > 0) ?? false
        });
    }

    public async Task<PortalPrescriptionDetailDto?> GetPrescriptionDetailAsync(int patientId, int prescriptionId)
    {
        var prescription = await _context.Prescriptions
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId && p.PatientId == patientId);

        if (prescription == null)
            return null;

        return new PortalPrescriptionDetailDto
        {
            Id = prescription.Id,
            PrescriptionDate = prescription.PrescriptionDate,
            PrescribingDoctor = $"Dr. {prescription.Doctor?.FirstName} {prescription.Doctor?.LastName}",
            DiagnosisRelated = prescription.DiagnosisRelated,
            Medications = prescription.Items?.Select(i => new PortalPrescriptionItemDto
            {
                Id = i.Id,
                MedicationName = i.MedicationName,
                GenericName = i.GenericName,
                Strength = i.Strength,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                Duration = i.Duration,
                Quantity = i.Quantity,
                Refills = i.Refills,
                Instructions = i.Instructions
            }).ToList() ?? new List<PortalPrescriptionItemDto>(),
            Instructions = prescription.Instructions,
            CanRequestRefill = prescription.Items?.Any(i => i.RefillsRemaining > 0) ?? false
        };
    }

    public async Task<bool> RequestRefillAsync(int patientId, PortalRefillRequestDto dto)
    {
        var prescriptionItem = await _context.PrescriptionItems
            .Include(pi => pi.Prescription)
            .FirstOrDefaultAsync(pi => pi.Id == dto.PrescriptionItemId &&
                pi.Prescription!.PatientId == patientId);

        if (prescriptionItem == null || prescriptionItem.RefillsRemaining <= 0)
            return false;

        // Create refill request
        var refillRequest = new RefillRequest
        {
            PrescriptionItemId = dto.PrescriptionItemId,
            PatientId = patientId,
            PharmacyName = dto.PharmacyName,
            PharmacyAddress = dto.PharmacyAddress,
            PharmacyPhone = dto.PharmacyPhone,
            Notes = dto.Notes,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };

        _context.RefillRequests.Add(refillRequest);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refill request created for PatientId: {PatientId}, ItemId: {ItemId}",
            patientId, dto.PrescriptionItemId);

        return true;
    }

    #endregion

    #region Messaging

    public async Task<IEnumerable<PortalMessageThreadDto>> GetMessageThreadsAsync(int patientId)
    {
        var threads = await _context.MessageThreads
            .Include(t => t.Doctor)
            .Include(t => t.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(t => t.PatientId == patientId)
            .OrderByDescending(t => t.LastMessageAt)
            .ToListAsync();

        return threads.Select(t => new PortalMessageThreadDto
        {
            Id = t.Id,
            Subject = t.Subject,
            DoctorId = t.DoctorId,
            DoctorName = $"Dr. {t.Doctor?.FirstName} {t.Doctor?.LastName}",
            DoctorSpecialty = t.Doctor?.Specialty,
            DoctorPhotoUrl = t.Doctor?.ProfilePhotoUrl,
            LastMessageAt = t.LastMessageAt,
            LastMessagePreview = t.Messages.FirstOrDefault()?.Content?.Substring(0, Math.Min(100, t.Messages.FirstOrDefault()?.Content?.Length ?? 0)) ?? "",
            UnreadCount = t.Messages.Count(m => !m.IsRead && m.SenderType != "Patient"),
            IsClosed = t.IsClosed
        });
    }

    public async Task<IEnumerable<PortalMessageDto>> GetMessagesAsync(int patientId, int threadId)
    {
        var thread = await _context.MessageThreads
            .FirstOrDefaultAsync(t => t.Id == threadId && t.PatientId == patientId);

        if (thread == null)
            return Enumerable.Empty<PortalMessageDto>();

        var messages = await _context.Messages
            .Include(m => m.Attachments)
            .Where(m => m.ThreadId == threadId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return messages.Select(m => new PortalMessageDto
        {
            Id = m.Id,
            ThreadId = m.ThreadId,
            SenderType = m.SenderType,
            SenderName = m.SenderName,
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.IsRead,
            Attachments = m.Attachments?.Select(a => new PortalMessageAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize,
                DownloadUrl = a.DownloadUrl
            }).ToList()
        });
    }

    public async Task<PortalMessageDto> SendMessageAsync(int patientId, PortalSendMessageDto dto)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
            throw new InvalidOperationException("Patient not found");

        MessageThread? thread;

        if (dto.ThreadId.HasValue)
        {
            thread = await _context.MessageThreads
                .FirstOrDefaultAsync(t => t.Id == dto.ThreadId && t.PatientId == patientId);

            if (thread == null)
                throw new InvalidOperationException("Thread not found");
        }
        else if (dto.DoctorId.HasValue)
        {
            thread = new MessageThread
            {
                PatientId = patientId,
                DoctorId = dto.DoctorId.Value,
                Subject = dto.Subject ?? "New Message",
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            _context.MessageThreads.Add(thread);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException("Either ThreadId or DoctorId must be provided");
        }

        var message = new Message
        {
            ThreadId = thread.Id,
            SenderType = "Patient",
            SenderName = $"{patient.FirstName} {patient.LastName}",
            SenderId = patientId,
            Content = dto.Content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);

        thread.LastMessageAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new PortalMessageDto
        {
            Id = message.Id,
            ThreadId = message.ThreadId,
            SenderType = message.SenderType,
            SenderName = message.SenderName,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        };
    }

    public async Task MarkMessagesAsReadAsync(int patientId, int threadId)
    {
        var thread = await _context.MessageThreads
            .FirstOrDefaultAsync(t => t.Id == threadId && t.PatientId == patientId);

        if (thread == null)
            return;

        var unreadMessages = await _context.Messages
            .Where(m => m.ThreadId == threadId && !m.IsRead && m.SenderType != "Patient")
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadMessageCountAsync(int patientId)
    {
        return await _context.Messages
            .Include(m => m.Thread)
            .Where(m => m.Thread!.PatientId == patientId &&
                !m.IsRead &&
                m.SenderType != "Patient")
            .CountAsync();
    }

    #endregion

    #region Billing

    public async Task<IEnumerable<PortalInvoiceSummaryDto>> GetInvoicesAsync(int patientId, string? status = null)
    {
        var query = _context.Invoices
            .Where(i => i.PatientId == patientId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        var invoices = await query
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        return invoices.Select(i => new PortalInvoiceSummaryDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            InvoiceDate = i.InvoiceDate,
            DueDate = i.DueDate,
            TotalAmount = i.TotalAmount,
            PaidAmount = i.PaidAmount,
            BalanceDue = i.TotalAmount - i.PaidAmount,
            Status = i.Status ?? "Pending",
            ServiceDescription = i.ServiceDescription
        });
    }

    public async Task<PortalInvoiceDetailDto?> GetInvoiceDetailAsync(int patientId, int invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.PatientId == patientId);

        if (invoice == null)
            return null;

        return new PortalInvoiceDetailDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            BalanceDue = invoice.TotalAmount - invoice.PaidAmount,
            Status = invoice.Status ?? "Pending",
            LineItems = invoice.LineItems?.Select(li => new PortalInvoiceLineItemDto
            {
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                TotalPrice = li.TotalPrice,
                ServiceDate = li.ServiceDate
            }).ToList() ?? new List<PortalInvoiceLineItemDto>(),
            PaymentHistory = invoice.Payments?.Select(p => new PortalPaymentHistoryDto
            {
                Id = p.Id,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                TransactionReference = p.TransactionReference,
                Status = p.Status ?? "Completed"
            }).ToList() ?? new List<PortalPaymentHistoryDto>(),
            CanPayOnline = invoice.TotalAmount - invoice.PaidAmount > 0
        };
    }

    public async Task<decimal> GetOutstandingBalanceAsync(int patientId)
    {
        return await _context.Invoices
            .Where(i => i.PatientId == patientId &&
                (i.Status == "Pending" || i.Status == "Overdue" || i.Status == "Partial"))
            .SumAsync(i => i.TotalAmount - i.PaidAmount);
    }

    public async Task<PortalPaymentResponseDto> MakePaymentAsync(int patientId, PortalMakePaymentDto dto)
    {
        try
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == dto.InvoiceId && i.PatientId == patientId);

            if (invoice == null)
            {
                return new PortalPaymentResponseDto
                {
                    Success = false,
                    Message = "Invoice not found"
                };
            }

            var balanceDue = invoice.TotalAmount - invoice.PaidAmount;
            if (dto.Amount > balanceDue)
            {
                return new PortalPaymentResponseDto
                {
                    Success = false,
                    Message = $"Payment amount exceeds balance due ({balanceDue:C})"
                };
            }

            // TODO: Process payment through payment gateway
            var transactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            var payment = new Payment
            {
                InvoiceId = dto.InvoiceId,
                PatientId = patientId,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                TransactionReference = transactionId,
                Status = "Completed",
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            invoice.PaidAmount += dto.Amount;
            if (invoice.PaidAmount >= invoice.TotalAmount)
            {
                invoice.Status = "Paid";
            }
            else
            {
                invoice.Status = "Partial";
            }

            await _context.SaveChangesAsync();

            return new PortalPaymentResponseDto
            {
                Success = true,
                TransactionId = transactionId,
                Message = "Payment processed successfully",
                NewBalance = invoice.TotalAmount - invoice.PaidAmount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for PatientId: {PatientId}", patientId);
            return new PortalPaymentResponseDto
            {
                Success = false,
                Message = "An error occurred while processing your payment"
            };
        }
    }

    public async Task<byte[]> DownloadInvoiceAsync(int patientId, int invoiceId)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.PatientId == patientId);

        if (invoice == null)
            return Array.Empty<byte>();

        // TODO: Implement PDF generation
        _logger.LogInformation("Invoice download requested for PatientId: {PatientId}, InvoiceId: {InvoiceId}",
            patientId, invoiceId);

        return Array.Empty<byte>();
    }

    public async Task<byte[]> DownloadReceiptAsync(int patientId, int paymentId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId && p.PatientId == patientId);

        if (payment == null)
            return Array.Empty<byte>();

        // TODO: Implement PDF generation
        _logger.LogInformation("Receipt download requested for PatientId: {PatientId}, PaymentId: {PaymentId}",
            patientId, paymentId);

        return Array.Empty<byte>();
    }

    #endregion

    #region Notifications

    public async Task<IEnumerable<PortalNotificationDto>> GetNotificationsAsync(int patientId, bool unreadOnly = false)
    {
        var query = _context.PatientNotifications
            .Where(n => n.PatientId == patientId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return notifications.Select(n => new PortalNotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            NotificationType = n.NotificationType,
            CreatedAt = n.CreatedAt,
            IsRead = n.IsRead,
            ActionUrl = n.ActionUrl,
            ActionText = n.ActionText
        });
    }

    public async Task MarkNotificationAsReadAsync(int patientId, int notificationId)
    {
        var notification = await _context.PatientNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.PatientId == patientId);

        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllNotificationsAsReadAsync(int patientId)
    {
        var unreadNotifications = await _context.PatientNotifications
            .Where(n => n.PatientId == patientId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<PortalNotificationPreferencesDto> GetNotificationPreferencesAsync(int patientId)
    {
        var preferences = await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.PatientId == patientId);

        if (preferences == null)
        {
            return new PortalNotificationPreferencesDto();
        }

        return new PortalNotificationPreferencesDto
        {
            AppointmentReminders = preferences.AppointmentReminders,
            LabResultsReady = preferences.LabResultsReady,
            PrescriptionRefills = preferences.PrescriptionRefills,
            NewMessages = preferences.NewMessages,
            BillingReminders = preferences.BillingReminders,
            HealthTips = preferences.HealthTips,
            EmailNotifications = preferences.EmailNotifications,
            SmsNotifications = preferences.SmsNotifications,
            PushNotifications = preferences.PushNotifications
        };
    }

    public async Task<PortalNotificationPreferencesDto> UpdateNotificationPreferencesAsync(
        int patientId, PortalNotificationPreferencesDto dto)
    {
        var preferences = await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.PatientId == patientId);

        if (preferences == null)
        {
            preferences = new NotificationPreferences
            {
                PatientId = patientId
            };
            _context.NotificationPreferences.Add(preferences);
        }

        preferences.AppointmentReminders = dto.AppointmentReminders;
        preferences.LabResultsReady = dto.LabResultsReady;
        preferences.PrescriptionRefills = dto.PrescriptionRefills;
        preferences.NewMessages = dto.NewMessages;
        preferences.BillingReminders = dto.BillingReminders;
        preferences.HealthTips = dto.HealthTips;
        preferences.EmailNotifications = dto.EmailNotifications;
        preferences.SmsNotifications = dto.SmsNotifications;
        preferences.PushNotifications = dto.PushNotifications;
        preferences.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return dto;
    }

    #endregion

    #region Helper Methods

    private async Task CreateNotificationAsync(int patientId, string title, string message,
        string notificationType, string? actionUrl = null)
    {
        var notification = new PatientNotification
        {
            PatientId = patientId,
            Title = title,
            Message = message,
            NotificationType = notificationType,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PatientNotifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    // Password hashing constants for PBKDF2
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // OWASP recommended minimum

    /// <summary>
    /// Hash password using PBKDF2 with SHA256
    /// Format: iterations.salt.hash (all base64 encoded)
    /// </summary>
    private static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));

        // Generate random salt
        var salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash password with PBKDF2
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        // Combine iterations, salt, and hash
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verify password using constant-time comparison to prevent timing attacks
    /// </summary>
    private static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        try
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 3)
            {
                // Legacy hash format (plain SHA256) - migrate on next password change
                return VerifyLegacyPassword(password, storedHash);
            }

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var storedHashBytes = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(HashSize);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verify legacy SHA256 password (for migration purposes only)
    /// </summary>
    private static bool VerifyLegacyPassword(string password, string storedHash)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var computedHash = Convert.ToBase64String(hashedBytes);

            // Still use constant-time comparison even for legacy
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHash),
                Encoding.UTF8.GetBytes(storedHash));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if password hash needs upgrade (legacy format)
    /// </summary>
    private static bool NeedsPasswordUpgrade(string storedHash)
    {
        return !string.IsNullOrEmpty(storedHash) && !storedHash.Contains('.');
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    #endregion
}

#region Portal Entities

/// <summary>
/// Portal account entity for patient authentication
/// </summary>
public class PortalAccount
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public string? SessionTokenHash { get; set; }
    public DateTime? SessionTokenExpiresAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
}

/// <summary>
/// Message thread entity
/// </summary>
public class MessageThread
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public bool IsClosed { get; set; }

    public Patient? Patient { get; set; }
    public User? Doctor { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

/// <summary>
/// Message entity
/// </summary>
public class Message
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public MessageThread? Thread { get; set; }
    public ICollection<MessageAttachment>? Attachments { get; set; }
}

/// <summary>
/// Message attachment entity
/// </summary>
public class MessageAttachment
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? DownloadUrl { get; set; }
    public string? StoragePath { get; set; }

    public Message? Message { get; set; }
}

/// <summary>
/// Refill request entity
/// </summary>
public class RefillRequest
{
    public int Id { get; set; }
    public int PrescriptionItemId { get; set; }
    public int PatientId { get; set; }
    public string? PharmacyName { get; set; }
    public string? PharmacyAddress { get; set; }
    public string? PharmacyPhone { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }

    public PrescriptionItem? PrescriptionItem { get; set; }
    public Patient? Patient { get; set; }
}

/// <summary>
/// Patient notification entity
/// </summary>
public class PatientNotification
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Patient? Patient { get; set; }
}

/// <summary>
/// Notification preferences entity
/// </summary>
public class NotificationPreferences
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public bool AppointmentReminders { get; set; } = true;
    public bool LabResultsReady { get; set; } = true;
    public bool PrescriptionRefills { get; set; } = true;
    public bool NewMessages { get; set; } = true;
    public bool BillingReminders { get; set; } = true;
    public bool HealthTips { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
}

#endregion
