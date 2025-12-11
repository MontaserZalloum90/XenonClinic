using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Appointment management
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly ClinicDbContext _context;

    public AppointmentService(ClinicDbContext context)
    {
        _context = context;
    }

    #region Appointment Management

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Branch)
            .Include(a => a.Provider)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByBranchIdAsync(int branchId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Where(a => a.BranchId == branchId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Branch)
            .Include(a => a.Provider)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByProviderIdAsync(int providerId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Branch)
            .Where(a => a.ProviderId == providerId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(int branchId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Where(a => a.BranchId == branchId &&
                   a.StartTime >= startOfDay &&
                   a.StartTime < endOfDay)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        // Validate date range
        if (endDate < startDate)
        {
            throw new ArgumentException("End date must be greater than or equal to start date", nameof(endDate));
        }

        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Where(a => a.BranchId == branchId &&
                   a.StartTime >= startDate &&
                   a.StartTime <= endDate)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(int branchId, AppointmentStatus status)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Where(a => a.BranchId == branchId && a.Status == status)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetTodayAppointmentsAsync(int branchId)
    {
        return await GetAppointmentsByDateAsync(branchId, DateTime.UtcNow.Date);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int branchId, int days = 7)
    {
        // Validate days parameter
        if (days <= 0)
        {
            throw new ArgumentException("Days must be greater than zero", nameof(days));
        }

        var today = DateTime.UtcNow.Date;
        var futureDate = today.AddDays(days);

        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Provider)
            .Where(a => a.BranchId == branchId &&
                   a.StartTime >= today &&
                   a.StartTime < futureDate &&
                   a.Status != AppointmentStatus.Cancelled &&
                   a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
    {
        // Validate patient exists
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == appointment.PatientId && !p.IsDeleted);
        if (!patientExists)
        {
            throw new KeyNotFoundException($"Patient with ID {appointment.PatientId} not found");
        }

        // Validate provider exists if specified
        if (appointment.ProviderId.HasValue)
        {
            var providerExists = await _context.Providers.AnyAsync(p => p.Id == appointment.ProviderId.Value);
            if (!providerExists)
            {
                throw new KeyNotFoundException($"Provider with ID {appointment.ProviderId.Value} not found");
            }
        }

        // Validate branch exists
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == appointment.BranchId);
        if (!branchExists)
        {
            throw new KeyNotFoundException($"Branch with ID {appointment.BranchId} not found");
        }

        // Validate appointment times
        if (appointment.EndTime <= appointment.StartTime)
        {
            throw new InvalidOperationException("Appointment end time must be after start time");
        }

        // Validate appointment is not in the past (allow same-day appointments)
        if (appointment.StartTime.Date < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Cannot create appointments in the past");
        }

        // Check for time slot availability
        if (!await IsTimeSlotAvailableAsync(appointment.BranchId, appointment.ProviderId, appointment.StartTime, appointment.EndTime))
        {
            throw new InvalidOperationException("The requested time slot is not available");
        }

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task UpdateAppointmentAsync(Appointment appointment)
    {
        // Validate appointment exists
        var existingAppointment = await _context.Appointments.FindAsync(appointment.Id);
        if (existingAppointment == null)
        {
            throw new KeyNotFoundException($"Appointment with ID {appointment.Id} not found");
        }

        // Validate appointment times if they changed
        if (appointment.EndTime <= appointment.StartTime)
        {
            throw new InvalidOperationException("Appointment end time must be after start time");
        }

        _context.Entry(existingAppointment).CurrentValues.SetValues(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Appointment Status Management

    // Valid status transitions state machine
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> ValidTransitions = new()
    {
        { AppointmentStatus.Scheduled, new[] { AppointmentStatus.Confirmed, AppointmentStatus.Cancelled } },
        { AppointmentStatus.Confirmed, new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow } },
        { AppointmentStatus.CheckedIn, new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled } },
        { AppointmentStatus.InProgress, new[] { AppointmentStatus.Completed, AppointmentStatus.Cancelled } },
        { AppointmentStatus.Completed, Array.Empty<AppointmentStatus>() }, // Terminal state
        { AppointmentStatus.Cancelled, Array.Empty<AppointmentStatus>() }, // Terminal state
        { AppointmentStatus.NoShow, Array.Empty<AppointmentStatus>() } // Terminal state
    };

    private static void ValidateStatusTransition(AppointmentStatus currentStatus, AppointmentStatus newStatus)
    {
        if (!ValidTransitions.TryGetValue(currentStatus, out var allowedStatuses) ||
            !allowedStatuses.Contains(newStatus))
        {
            throw new InvalidOperationException(
                $"Cannot transition from {currentStatus} to {newStatus}. " +
                $"Valid transitions from {currentStatus}: {string.Join(", ", ValidTransitions.GetValueOrDefault(currentStatus, Array.Empty<AppointmentStatus>()))}");
        }
    }

    public async Task ConfirmAppointmentAsync(int appointmentId)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");

        ValidateStatusTransition(appointment.Status, AppointmentStatus.Confirmed);
        appointment.Status = AppointmentStatus.Confirmed;
        await _context.SaveChangesAsync();
    }

    public async Task CancelAppointmentAsync(int appointmentId, string? reason)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");

        ValidateStatusTransition(appointment.Status, AppointmentStatus.Cancelled);
        appointment.Status = AppointmentStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            appointment.Notes = string.IsNullOrWhiteSpace(appointment.Notes)
                ? $"Cancelled: {reason}"
                : $"{appointment.Notes}\nCancelled: {reason}";
        }
        await _context.SaveChangesAsync();
    }

    public async Task CheckInAppointmentAsync(int appointmentId)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");

        ValidateStatusTransition(appointment.Status, AppointmentStatus.CheckedIn);
        appointment.Status = AppointmentStatus.CheckedIn;
        await _context.SaveChangesAsync();
    }

    public async Task CompleteAppointmentAsync(int appointmentId)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");

        // Allow completion from CheckedIn or InProgress
        if (appointment.Status != AppointmentStatus.CheckedIn && appointment.Status != AppointmentStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Cannot complete appointment from {appointment.Status}. Appointment must be CheckedIn or InProgress.");
        }

        appointment.Status = AppointmentStatus.Completed;
        await _context.SaveChangesAsync();
    }

    public async Task NoShowAppointmentAsync(int appointmentId)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");

        ValidateStatusTransition(appointment.Status, AppointmentStatus.NoShow);
        appointment.Status = AppointmentStatus.NoShow;
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Appointment Scheduling

    public async Task<bool> IsTimeSlotAvailableAsync(int branchId, int? providerId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null)
    {
        var query = _context.Appointments
            .Where(a => a.BranchId == branchId &&
                   a.Status != AppointmentStatus.Cancelled &&
                   a.Status != AppointmentStatus.NoShow &&
                   ((a.StartTime < endTime && a.EndTime > startTime)));

        if (providerId.HasValue)
        {
            query = query.Where(a => a.ProviderId == providerId.Value);
        }

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        var conflictingAppointments = await query.CountAsync();
        return conflictingAppointments == 0;
    }

    public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int branchId, int? providerId, DateTime date, int durationMinutes = 30)
    {
        var availableSlots = new List<DateTime>();
        var startOfDay = date.Date.AddHours(8); // Start at 8 AM
        var endOfDay = date.Date.AddHours(18); // End at 6 PM

        var currentSlot = startOfDay;
        while (currentSlot.AddMinutes(durationMinutes) <= endOfDay)
        {
            var slotEnd = currentSlot.AddMinutes(durationMinutes);
            if (await IsTimeSlotAvailableAsync(branchId, providerId, currentSlot, slotEnd))
            {
                availableSlots.Add(currentSlot);
            }
            currentSlot = currentSlot.AddMinutes(durationMinutes);
        }

        return availableSlots;
    }

    public async Task<Appointment> RescheduleAppointmentAsync(int appointmentId, DateTime newStartTime, DateTime newEndTime)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");

        // Check if new time slot is available
        if (!await IsTimeSlotAvailableAsync(appointment.BranchId, appointment.ProviderId, newStartTime, newEndTime, appointmentId))
        {
            throw new InvalidOperationException("The requested time slot is not available");
        }

        appointment.StartTime = newStartTime;
        appointment.EndTime = newEndTime;
        await _context.SaveChangesAsync();

        return appointment;
    }

    #endregion

    #region Statistics & Reporting

    public async Task<int> GetTotalAppointmentsCountAsync(int branchId)
    {
        return await _context.Appointments
            .CountAsync(a => a.BranchId == branchId);
    }

    public async Task<int> GetTodayAppointmentsCountAsync(int branchId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _context.Appointments
            .CountAsync(a => a.BranchId == branchId &&
                        a.StartTime >= today &&
                        a.StartTime < tomorrow);
    }

    public async Task<int> GetUpcomingAppointmentsCountAsync(int branchId)
    {
        var now = DateTime.UtcNow;

        return await _context.Appointments
            .CountAsync(a => a.BranchId == branchId &&
                        a.StartTime >= now &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.Completed);
    }

    public async Task<int> GetCompletedAppointmentsCountAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Appointments
            .CountAsync(a => a.BranchId == branchId &&
                        a.Status == AppointmentStatus.Completed &&
                        a.StartTime >= startDate &&
                        a.StartTime <= endDate);
    }

    public async Task<int> GetCancelledAppointmentsCountAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Appointments
            .CountAsync(a => a.BranchId == branchId &&
                        a.Status == AppointmentStatus.Cancelled &&
                        a.StartTime >= startDate &&
                        a.StartTime <= endDate);
    }

    public async Task<int> GetNoShowAppointmentsCountAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        return await _context.Appointments
            .CountAsync(a => a.BranchId == branchId &&
                        a.Status == AppointmentStatus.NoShow &&
                        a.StartTime >= startDate &&
                        a.StartTime <= endDate);
    }

    public async Task<Dictionary<AppointmentStatus, int>> GetAppointmentStatusDistributionAsync(int branchId)
    {
        var distribution = await _context.Appointments
            .Where(a => a.BranchId == branchId)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<Dictionary<AppointmentType, int>> GetAppointmentTypeDistributionAsync(int branchId)
    {
        var distribution = await _context.Appointments
            .Where(a => a.BranchId == branchId)
            .GroupBy(a => a.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.Type, x => x.Count);
    }

    public async Task<decimal> GetAppointmentCompletionRateAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        var totalAppointments = await _context.Appointments
            .CountAsync(a => a.BranchId == branchId &&
                        a.StartTime >= startDate &&
                        a.StartTime <= endDate);

        if (totalAppointments == 0)
            return 0;

        var completedAppointments = await _context.Appointments
            .CountAsync(a => a.BranchId == branchId &&
                        a.Status == AppointmentStatus.Completed &&
                        a.StartTime >= startDate &&
                        a.StartTime <= endDate);

        return Math.Round((decimal)completedAppointments / totalAppointments * 100, 2);
    }

    #endregion
}
