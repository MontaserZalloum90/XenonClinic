using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;
using XenonClinic.Core.Utilities;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// Patient Portal API - Self-service endpoints for patients
/// </summary>
[Route("api/portal")]
public class PatientPortalController : BaseApiController
{
    private readonly IPatientPortalService _portalService;
    private readonly ILogger<PatientPortalController> _logger;

    // File upload constraints
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

    public PatientPortalController(
        IPatientPortalService portalService,
        ILogger<PatientPortalController> logger)
    {
        _portalService = portalService;
        _logger = logger;
    }

    #region Helper Methods

    /// <summary>
    /// Validates that the authenticated user has access to the specified patient's data.
    /// CRITICAL SECURITY: Prevents unauthorized access to other patients' data.
    /// </summary>
    private bool ValidatePatientAccess(int patientId)
    {
        // Get the authenticated user's patient ID from claims
        var userPatientIdClaim = User.FindFirst("patient_id")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userPatientIdClaim))
        {
            _logger.LogWarning("Patient access validation failed: No patient ID claim found for user");
            return false;
        }

        if (!int.TryParse(userPatientIdClaim, out var userPatientId))
        {
            _logger.LogWarning("Patient access validation failed: Invalid patient ID claim format");
            return false;
        }

        if (userPatientId != patientId)
        {
            _logger.LogWarning(
                "SECURITY: User {UserPatientId} attempted to access data for patient {RequestedPatientId}",
                userPatientId, patientId);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the authenticated patient's ID from claims
    /// </summary>
    private int? GetAuthenticatedPatientId()
    {
        var patientIdClaim = User.FindFirst("patient_id")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(patientIdClaim, out var patientId))
            return patientId;

        return null;
    }

    /// <summary>
    /// Validates file upload for security
    /// </summary>
    private (bool IsValid, string? ErrorMessage) ValidateFileUpload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return (false, "No file uploaded");

        if (file.Length > MaxFileSizeBytes)
            return (false, $"File size exceeds maximum allowed ({MaxFileSizeBytes / 1024 / 1024}MB)");

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
            return (false, $"Invalid file type. Allowed types: {string.Join(", ", AllowedImageExtensions)}");

        if (!AllowedMimeTypes.Contains(file.ContentType?.ToLowerInvariant()))
            return (false, $"Invalid content type. Allowed types: {string.Join(", ", AllowedMimeTypes)}");

        // Check for double extensions (security)
        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
        if (fileName.Contains('.'))
            return (false, "Invalid filename format");

        return (true, null);
    }

    #endregion

    #region Authentication

    /// <summary>
    /// Register a new patient portal account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PortalRegistrationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PortalRegistrationResponseDto>>> Register(
        [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Valid branch ID is required")] int branchId,
        [FromBody] PortalRegistrationDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        try
        {
            var result = await _portalService.RegisterAsync(branchId, request);

            if (!result.Success)
                return ApiBadRequest(result.Message ?? "Registration failed");

            return ApiOk(result, "Registration successful. Please verify your email.");
        }
        catch (Exception ex)
        {
            // BUG FIX: Mask email in logs to prevent PII exposure
            _logger.LogError(ex, "Error during patient portal registration for email: {Email}", LoggingHelpers.MaskEmail(request.Email));
            return ApiServerError("An error occurred during registration");
        }
    }

    /// <summary>
    /// Authenticate patient login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PortalLoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PortalLoginResponseDto>>> Login([FromBody] PortalLoginDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        try
        {
            var result = await _portalService.LoginAsync(request);

            if (!result.Success)
            {
                // BUG FIX: Mask email in logs to prevent PII exposure
                _logger.LogWarning("Failed login attempt for email: {Email}", LoggingHelpers.MaskEmail(request.Email));
                return ApiUnauthorized(result.Message ?? "Invalid credentials");
            }

            // BUG FIX: Mask email in logs to prevent PII exposure
            _logger.LogInformation("Successful login for email: {Email}", LoggingHelpers.MaskEmail(request.Email));
            return ApiOk(result);
        }
        catch (Exception ex)
        {
            // BUG FIX: Mask email in logs to prevent PII exposure
            _logger.LogError(ex, "Error during patient portal login for email: {Email}", LoggingHelpers.MaskEmail(request.Email));
            return ApiServerError("An error occurred during login");
        }
    }

    /// <summary>
    /// Verify email address
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> VerifyEmail(
        [FromQuery][Required(ErrorMessage = "Verification token is required")] string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 500)
            return ApiBadRequest("Invalid verification token");

        var success = await _portalService.VerifyEmailAsync(token);

        if (!success)
            return ApiBadRequest("Invalid or expired verification token");

        return ApiOk("Email verified successfully");
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        // Rate limiting should be implemented at middleware level
        await _portalService.RequestPasswordResetAsync(request.Email);

        // Always return success to prevent email enumeration
        return ApiOk("If an account exists with this email, a reset link has been sent.");
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var success = await _portalService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!success)
            return ApiBadRequest("Invalid or expired reset token");

        return ApiOk("Password reset successfully");
    }

    /// <summary>
    /// Change password (authenticated)
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var success = await _portalService.ChangePasswordAsync(patientId.Value, request.CurrentPassword, request.NewPassword);

        if (!success)
            return ApiBadRequest("Current password is incorrect");

        _logger.LogInformation("Password changed for patient: {PatientId}", patientId.Value);
        return ApiOk("Password changed successfully");
    }

    /// <summary>
    /// Refresh authentication token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PortalLoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PortalLoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var result = await _portalService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
            return ApiUnauthorized("Invalid or expired refresh token");

        return ApiOk(result);
    }

    #endregion

    #region Profile & Dashboard

    /// <summary>
    /// Get patient portal dashboard
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PatientPortalDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PatientPortalDashboardDto>>> GetDashboard()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var dashboard = await _portalService.GetDashboardAsync(patientId.Value);
        return ApiOk(dashboard);
    }

    /// <summary>
    /// Get patient profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PatientPortalProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PatientPortalProfileDto>>> GetProfile()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var profile = await _portalService.GetProfileAsync(patientId.Value);
        return ApiOk(profile);
    }

    /// <summary>
    /// Update patient profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PatientPortalProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PatientPortalProfileDto>>> UpdateProfile(
        [FromBody] UpdatePatientProfileDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var profile = await _portalService.UpdateProfileAsync(patientId.Value, request);
        return ApiOk(profile, "Profile updated successfully");
    }

    /// <summary>
    /// Upload profile photo
    /// </summary>
    [HttpPost("profile/photo")]
    [Authorize]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<string>>> UploadProfilePhoto(IFormFile file)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        // Validate file upload
        var (isValid, errorMessage) = ValidateFileUpload(file);
        if (!isValid)
            return ApiBadRequest(errorMessage!);

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            // Sanitize filename
            var safeFileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName).ToLowerInvariant();

            var photoUrl = await _portalService.UploadProfilePhotoAsync(patientId.Value, stream.ToArray(), safeFileName);

            return ApiOk(photoUrl, "Photo uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile photo for patient: {PatientId}", patientId.Value);
            return ApiServerError("An error occurred while uploading the photo");
        }
    }

    #endregion

    #region Appointments

    /// <summary>
    /// Get upcoming appointments
    /// </summary>
    [HttpGet("appointments/upcoming")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalAppointmentSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalAppointmentSummaryDto>>>> GetUpcomingAppointments()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var appointments = await _portalService.GetUpcomingAppointmentsAsync(patientId.Value);
        return ApiOk(appointments);
    }

    /// <summary>
    /// Get past appointments
    /// </summary>
    [HttpGet("appointments/past")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalAppointmentSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalAppointmentSummaryDto>>>> GetPastAppointments(
        [FromQuery][Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")] int limit = 20)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var appointments = await _portalService.GetPastAppointmentsAsync(patientId.Value, limit);
        return ApiOk(appointments);
    }

    /// <summary>
    /// Get appointment details
    /// </summary>
    [HttpGet("appointments/{appointmentId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalAppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PortalAppointmentDto>>> GetAppointment(int appointmentId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var appointment = await _portalService.GetAppointmentAsync(patientId.Value, appointmentId);

        if (appointment == null)
            return ApiNotFound("Appointment not found");

        return ApiOk(appointment);
    }

    /// <summary>
    /// Get available appointment slots
    /// </summary>
    [HttpGet("appointments/slots")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalAppointmentSlotDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalAppointmentSlotDto>>>> GetAvailableSlots(
        [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Valid branch ID is required")] int branchId,
        [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Valid doctor ID is required")] int doctorId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate >= endDate)
            return ApiBadRequest("Start date must be before end date");

        if (startDate < DateTime.Today)
            return ApiBadRequest("Start date cannot be in the past");

        if ((endDate - startDate).TotalDays > 90)
            return ApiBadRequest("Date range cannot exceed 90 days");

        var slots = await _portalService.GetAvailableSlotsAsync(branchId, doctorId, startDate, endDate);
        return ApiOk(slots);
    }

    /// <summary>
    /// Get doctors available for booking
    /// </summary>
    [HttpGet("appointments/doctors")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalDoctorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalDoctorDto>>>> GetDoctorsForBooking(
        [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Valid branch ID is required")] int branchId,
        [FromQuery][StringLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")] string? specialty = null)
    {
        var doctors = await _portalService.GetDoctorsForBookingAsync(branchId, specialty);
        return ApiOk(doctors);
    }

    /// <summary>
    /// Book an appointment
    /// </summary>
    [HttpPost("appointments")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalAppointmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PortalAppointmentDto>>> BookAppointment(
        [FromBody] PortalBookAppointmentDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        if (request.PreferredDate < DateTime.Today)
            return ApiBadRequest("Cannot book appointments in the past");

        try
        {
            var appointment = await _portalService.BookAppointmentAsync(patientId.Value, request);
            return ApiCreated(appointment, message: "Appointment booked successfully");
        }
        catch (InvalidOperationException ex)
        {
            return ApiBadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Cancel an appointment
    /// </summary>
    [HttpDelete("appointments/{appointmentId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> CancelAppointment(
        int appointmentId,
        [FromQuery][StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")] string? reason = null)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var success = await _portalService.CancelAppointmentAsync(patientId.Value, appointmentId, reason);

        if (!success)
            return ApiBadRequest("Unable to cancel appointment");

        return ApiOk("Appointment cancelled successfully");
    }

    /// <summary>
    /// Reschedule an appointment
    /// </summary>
    [HttpPut("appointments/{appointmentId:int}/reschedule")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalAppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PortalAppointmentDto>>> RescheduleAppointment(
        int appointmentId,
        [FromQuery] DateTime newDate,
        [FromQuery][StringLength(10, ErrorMessage = "Time format invalid")] string? newTime = null)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        if (newDate < DateTime.Today)
            return ApiBadRequest("Cannot reschedule to a past date");

        try
        {
            var appointment = await _portalService.RescheduleAppointmentAsync(patientId.Value, appointmentId, newDate, newTime);
            return ApiOk(appointment, "Appointment rescheduled successfully");
        }
        catch (InvalidOperationException ex)
        {
            return ApiBadRequest(ex.Message);
        }
    }

    #endregion

    #region Medical Records

    /// <summary>
    /// Get medical records summary
    /// </summary>
    [HttpGet("medical-records")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalMedicalRecordsSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PortalMedicalRecordsSummaryDto>>> GetMedicalRecordsSummary()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var records = await _portalService.GetMedicalRecordsSummaryAsync(patientId.Value);
        return ApiOk(records);
    }

    /// <summary>
    /// Get visit history
    /// </summary>
    [HttpGet("medical-records/visits")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalVisitSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalVisitSummaryDto>>>> GetVisitHistory(
        [FromQuery][Range(2000, 2100, ErrorMessage = "Invalid year")] int? year = null,
        [FromQuery][Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")] int limit = 50)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var visits = await _portalService.GetVisitHistoryAsync(patientId.Value, year, limit);
        return ApiOk(visits);
    }

    /// <summary>
    /// Get visit details
    /// </summary>
    [HttpGet("medical-records/visits/{visitId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalVisitDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PortalVisitDetailDto>>> GetVisitDetail(int visitId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var visit = await _portalService.GetVisitDetailAsync(patientId.Value, visitId);

        if (visit == null)
            return ApiNotFound("Visit not found");

        return ApiOk(visit);
    }

    /// <summary>
    /// Get allergies
    /// </summary>
    [HttpGet("medical-records/allergies")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalAllergyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalAllergyDto>>>> GetAllergies()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var allergies = await _portalService.GetAllergiesAsync(patientId.Value);
        return ApiOk(allergies);
    }

    /// <summary>
    /// Get immunizations
    /// </summary>
    [HttpGet("medical-records/immunizations")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalImmunizationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalImmunizationDto>>>> GetImmunizations()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var immunizations = await _portalService.GetImmunizationsAsync(patientId.Value);
        return ApiOk(immunizations);
    }

    /// <summary>
    /// Get vital signs history
    /// </summary>
    [HttpGet("medical-records/vitals")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalVitalSignsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalVitalSignsDto>>>> GetVitalSignsHistory(
        [FromQuery][Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")] int limit = 20)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var vitals = await _portalService.GetVitalSignsHistoryAsync(patientId.Value, limit);
        return ApiOk(vitals);
    }

    /// <summary>
    /// Download medical records
    /// </summary>
    [HttpGet("medical-records/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadMedicalRecords(
        [FromQuery][RegularExpression("^(pdf|json)$", ErrorMessage = "Format must be 'pdf' or 'json'")] string format = "pdf")
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Failure("Unable to identify authenticated patient"));

        var data = await _portalService.DownloadMedicalRecordsAsync(patientId.Value, format);
        var contentType = format.ToLower() == "pdf" ? "application/pdf" : "application/json";
        return File(data, contentType, $"medical-records.{format}");
    }

    #endregion

    #region Lab Results

    /// <summary>
    /// Get lab results
    /// </summary>
    [HttpGet("lab-results")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalLabResultSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalLabResultSummaryDto>>>> GetLabResults(
        [FromQuery][Range(2000, 2100, ErrorMessage = "Invalid year")] int? year = null,
        [FromQuery][Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")] int limit = 50)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var results = await _portalService.GetLabResultsAsync(patientId.Value, year, limit);
        return ApiOk(results);
    }

    /// <summary>
    /// Get lab result details
    /// </summary>
    [HttpGet("lab-results/{labResultId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalLabResultDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PortalLabResultDetailDto>>> GetLabResultDetail(int labResultId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var result = await _portalService.GetLabResultDetailAsync(patientId.Value, labResultId);

        if (result == null)
            return ApiNotFound("Lab result not found");

        return ApiOk(result);
    }

    /// <summary>
    /// Download lab report
    /// </summary>
    [HttpGet("lab-results/{labResultId:int}/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadLabReport(int labResultId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Failure("Unable to identify authenticated patient"));

        var data = await _portalService.DownloadLabReportAsync(patientId.Value, labResultId);
        return File(data, "application/pdf", $"lab-report-{labResultId}.pdf");
    }

    #endregion

    #region Prescriptions

    /// <summary>
    /// Get active medications
    /// </summary>
    [HttpGet("medications")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalMedicationSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalMedicationSummaryDto>>>> GetActiveMedications()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var medications = await _portalService.GetActiveMedicationsAsync(patientId.Value);
        return ApiOk(medications);
    }

    /// <summary>
    /// Get prescription history
    /// </summary>
    [HttpGet("prescriptions")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalPrescriptionDetailDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalPrescriptionDetailDto>>>> GetPrescriptionHistory(
        [FromQuery][Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")] int limit = 20)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var prescriptions = await _portalService.GetPrescriptionHistoryAsync(patientId.Value, limit);
        return ApiOk(prescriptions);
    }

    /// <summary>
    /// Get prescription details
    /// </summary>
    [HttpGet("prescriptions/{prescriptionId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalPrescriptionDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PortalPrescriptionDetailDto>>> GetPrescriptionDetail(int prescriptionId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var prescription = await _portalService.GetPrescriptionDetailAsync(patientId.Value, prescriptionId);

        if (prescription == null)
            return ApiNotFound("Prescription not found");

        return ApiOk(prescription);
    }

    /// <summary>
    /// Request prescription refill
    /// </summary>
    [HttpPost("prescriptions/refill")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> RequestRefill([FromBody] PortalRefillRequestDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var success = await _portalService.RequestRefillAsync(patientId.Value, request);

        if (!success)
            return ApiBadRequest("Unable to request refill");

        return ApiOk("Refill request submitted successfully");
    }

    #endregion

    #region Messaging

    /// <summary>
    /// Get message threads
    /// </summary>
    [HttpGet("messages")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalMessageThreadDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalMessageThreadDto>>>> GetMessageThreads()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var threads = await _portalService.GetMessageThreadsAsync(patientId.Value);
        return ApiOk(threads);
    }

    /// <summary>
    /// Get messages in a thread
    /// </summary>
    [HttpGet("messages/thread/{threadId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalMessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalMessageDto>>>> GetMessages(int threadId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var messages = await _portalService.GetMessagesAsync(patientId.Value, threadId);
        return ApiOk(messages);
    }

    /// <summary>
    /// Send a message
    /// </summary>
    [HttpPost("messages")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalMessageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PortalMessageDto>>> SendMessage([FromBody] PortalSendMessageDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        try
        {
            var message = await _portalService.SendMessageAsync(patientId.Value, request);
            return ApiCreated(message, message: "Message sent successfully");
        }
        catch (InvalidOperationException ex)
        {
            return ApiBadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Mark messages in thread as read
    /// </summary>
    [HttpPut("messages/thread/{threadId:int}/read")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> MarkMessagesAsRead(int threadId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        await _portalService.MarkMessagesAsReadAsync(patientId.Value, threadId);
        return ApiOk("Messages marked as read");
    }

    /// <summary>
    /// Get unread message count
    /// </summary>
    [HttpGet("messages/unread-count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadMessageCount()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var count = await _portalService.GetUnreadMessageCountAsync(patientId.Value);
        return ApiOk(count);
    }

    #endregion

    #region Billing

    /// <summary>
    /// Get invoices
    /// </summary>
    [HttpGet("billing/invoices")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalInvoiceSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalInvoiceSummaryDto>>>> GetInvoices(
        [FromQuery][RegularExpression("^(Pending|Paid|Overdue|Partial)?$", ErrorMessage = "Invalid status")] string? status = null)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var invoices = await _portalService.GetInvoicesAsync(patientId.Value, status);
        return ApiOk(invoices);
    }

    /// <summary>
    /// Get invoice details
    /// </summary>
    [HttpGet("billing/invoices/{invoiceId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalInvoiceDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PortalInvoiceDetailDto>>> GetInvoiceDetail(int invoiceId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var invoice = await _portalService.GetInvoiceDetailAsync(patientId.Value, invoiceId);

        if (invoice == null)
            return ApiNotFound("Invoice not found");

        return ApiOk(invoice);
    }

    /// <summary>
    /// Get outstanding balance
    /// </summary>
    [HttpGet("billing/balance")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<decimal>>> GetOutstandingBalance()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var balance = await _portalService.GetOutstandingBalanceAsync(patientId.Value);
        return ApiOk(balance);
    }

    /// <summary>
    /// Make a payment
    /// </summary>
    [HttpPost("billing/pay")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalPaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PortalPaymentResponseDto>>> MakePayment([FromBody] PortalMakePaymentDto request)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        if (request.Amount <= 0)
            return ApiBadRequest("Payment amount must be greater than zero");

        var result = await _portalService.MakePaymentAsync(patientId.Value, request);

        if (!result.Success)
            return ApiBadRequest(result.Message ?? "Payment failed");

        return ApiOk(result, "Payment processed successfully");
    }

    /// <summary>
    /// Download invoice PDF
    /// </summary>
    [HttpGet("billing/invoices/{invoiceId:int}/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadInvoice(int invoiceId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Failure("Unable to identify authenticated patient"));

        var data = await _portalService.DownloadInvoiceAsync(patientId.Value, invoiceId);
        return File(data, "application/pdf", $"invoice-{invoiceId}.pdf");
    }

    /// <summary>
    /// Download payment receipt
    /// </summary>
    [HttpGet("billing/receipts/{paymentId:int}/download")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadReceipt(int paymentId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Failure("Unable to identify authenticated patient"));

        var data = await _portalService.DownloadReceiptAsync(patientId.Value, paymentId);
        return File(data, "application/pdf", $"receipt-{paymentId}.pdf");
    }

    #endregion

    #region Notifications

    /// <summary>
    /// Get notifications
    /// </summary>
    [HttpGet("notifications")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PortalNotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PortalNotificationDto>>>> GetNotifications(
        [FromQuery] bool unreadOnly = false)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var notifications = await _portalService.GetNotificationsAsync(patientId.Value, unreadOnly);
        return ApiOk(notifications);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPut("notifications/{notificationId:int}/read")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> MarkNotificationAsRead(int notificationId)
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        await _portalService.MarkNotificationAsReadAsync(patientId.Value, notificationId);
        return ApiOk("Notification marked as read");
    }

    /// <summary>
    /// Mark all notifications as read
    /// </summary>
    [HttpPut("notifications/read-all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> MarkAllNotificationsAsRead()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        await _portalService.MarkAllNotificationsAsReadAsync(patientId.Value);
        return ApiOk("All notifications marked as read");
    }

    /// <summary>
    /// Get notification preferences
    /// </summary>
    [HttpGet("notifications/preferences")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalNotificationPreferencesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PortalNotificationPreferencesDto>>> GetNotificationPreferences()
    {
        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var preferences = await _portalService.GetNotificationPreferencesAsync(patientId.Value);
        return ApiOk(preferences);
    }

    /// <summary>
    /// Update notification preferences
    /// </summary>
    [HttpPut("notifications/preferences")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PortalNotificationPreferencesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PortalNotificationPreferencesDto>>> UpdateNotificationPreferences(
        [FromBody] PortalNotificationPreferencesDto preferences)
    {
        if (!ModelState.IsValid)
            return ApiBadRequestFromModelState();

        var patientId = GetAuthenticatedPatientId();
        if (!patientId.HasValue)
            return ApiForbidden("Unable to identify authenticated patient");

        var updated = await _portalService.UpdateNotificationPreferencesAsync(patientId.Value, preferences);
        return ApiOk(updated, "Preferences updated successfully");
    }

    #endregion
}

#region Request Models with Validation

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Token is required")]
    [StringLength(500, ErrorMessage = "Token cannot exceed 500 characters")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase, lowercase, number and special character")]
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase, lowercase, number and special character")]
    public string NewPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    [StringLength(500, ErrorMessage = "Refresh token cannot exceed 500 characters")]
    public string RefreshToken { get; set; } = string.Empty;
}

#endregion
