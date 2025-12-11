using XenonClinic.Core.Enums;

namespace XenonClinic.Core.DTOs;

/// <summary>
/// DTO for appointment data transfer. Used for reading appointment information.
/// </summary>
public class AppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientEmiratesId { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;
    public AppointmentType Type { get; set; }
    public string TypeDisplay => GetAppointmentTypeDisplay(Type);
    public AppointmentStatus Status { get; set; }
    public string StatusDisplay => GetAppointmentStatusDisplay(Status);
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Computed properties
    public bool IsUpcoming => StartTime > DateTime.UtcNow && Status != AppointmentStatus.Cancelled;
    public bool IsPast => EndTime < DateTime.UtcNow;
    public bool IsToday => StartTime.Date == DateTime.UtcNow.Date;
    public bool CanCancel => Status is AppointmentStatus.Scheduled or AppointmentStatus.Confirmed && StartTime > DateTime.UtcNow;
    public bool CanCheckIn => Status == AppointmentStatus.Confirmed && StartTime.Date == DateTime.UtcNow.Date;

    private static string GetAppointmentTypeDisplay(AppointmentType type) => type switch
    {
        AppointmentType.Consultation => "General Consultation",
        AppointmentType.FollowUp => "Follow-up",
        AppointmentType.Emergency => "Emergency",
        AppointmentType.NewPatient => "New Patient",
        AppointmentType.HearingTest => "Hearing Test",
        AppointmentType.HearingAidFitting => "Hearing Aid Fitting",
        AppointmentType.DentalCheckup => "Dental Checkup",
        AppointmentType.DentalProcedure => "Dental Procedure",
        AppointmentType.EyeExam => "Eye Exam",
        AppointmentType.PhysioSession => "Physiotherapy Session",
        _ => type.ToString()
    };

    private static string GetAppointmentStatusDisplay(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Scheduled => "Scheduled",
        AppointmentStatus.Confirmed => "Confirmed",
        AppointmentStatus.CheckedIn => "Checked In",
        AppointmentStatus.InProgress => "In Progress",
        AppointmentStatus.Completed => "Completed",
        AppointmentStatus.Cancelled => "Cancelled",
        AppointmentStatus.NoShow => "No Show",
        _ => status.ToString()
    };
}

/// <summary>
/// DTO for creating a new appointment.
/// </summary>
public class CreateAppointmentDto
{
    public int PatientId { get; set; }
    public int? ProviderId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentType Type { get; set; } = AppointmentType.Consultation;
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing appointment.
/// </summary>
public class UpdateAppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? ProviderId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentType Type { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for rescheduling an appointment.
/// </summary>
public class RescheduleAppointmentDto
{
    public int AppointmentId { get; set; }
    public DateTime NewStartTime { get; set; }
    public DateTime NewEndTime { get; set; }
    public int? NewProviderId { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for cancelling an appointment.
/// </summary>
public class CancelAppointmentDto
{
    public int AppointmentId { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for appointment list with lightweight data.
/// </summary>
public class AppointmentListItemDto
{
    public int Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? ProviderName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AppointmentType Type { get; set; }
    public AppointmentStatus Status { get; set; }
}

/// <summary>
/// DTO for available time slot.
/// </summary>
public class TimeSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsAvailable { get; set; }
    public string? ProviderName { get; set; }
}

/// <summary>
/// Request DTO for checking availability.
/// </summary>
public class AvailabilityRequestDto
{
    public DateTime Date { get; set; }
    public int? ProviderId { get; set; }
    public int DurationMinutes { get; set; } = 30;
}

/// <summary>
/// Request DTO for appointment list with filtering.
/// </summary>
public class AppointmentListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? PatientId { get; set; }
    public int? ProviderId { get; set; }
    public AppointmentStatus? Status { get; set; }
    public AppointmentType? Type { get; set; }
    public string? SortBy { get; set; } = "StartTime";
    public bool SortDescending { get; set; }
}

/// <summary>
/// DTO for appointment statistics.
/// </summary>
public class AppointmentStatisticsDto
{
    public int TotalAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CompletedThisMonth { get; set; }
    public int CancelledThisMonth { get; set; }
    public int NoShowThisMonth { get; set; }
    public decimal CompletionRate { get; set; }
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
    public Dictionary<string, int> TypeDistribution { get; set; } = new();
    public Dictionary<string, int> DailyCountThisWeek { get; set; } = new();
}

/// <summary>
/// DTO for daily schedule summary.
/// </summary>
public class DailyScheduleDto
{
    public DateTime Date { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int RemainingAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public List<AppointmentListItemDto> Appointments { get; set; } = new();
}

/// <summary>
/// Standard validation error messages for appointment operations.
/// </summary>
public static class AppointmentValidationMessages
{
    public const string PatientIdRequired = "Patient ID is required";
    public const string PatientNotFound = "Patient not found";

    public const string StartTimeRequired = "Start time is required";
    public const string EndTimeRequired = "End time is required";
    public const string StartTimeInPast = "Appointment cannot be scheduled in the past";
    public const string EndTimeBeforeStartTime = "End time must be after start time";
    public const string InvalidDuration = "Appointment duration must be between 5 minutes and 8 hours";

    public const string TypeRequired = "Appointment type is required";
    public const string InvalidType = "Invalid appointment type";

    public const string SlotNotAvailable = "The selected time slot is not available";
    public const string ProviderNotAvailable = "The provider is not available at the selected time";
    public const string ConflictingAppointment = "There is a conflicting appointment at this time";

    public const string AppointmentNotFound = "Appointment not found";
    public const string CannotModifyCancelled = "Cannot modify a cancelled appointment";
    public const string CannotModifyCompleted = "Cannot modify a completed appointment";
    public const string InvalidStatusTransition = "Invalid status transition";

    public const string CancellationReasonRequired = "Cancellation reason is required";
    public const string BranchAccessDenied = "You do not have access to this branch";

    public const string DateRangeInvalid = "Start date must be before or equal to end date";
    public const string DateRangeTooLarge = "Date range cannot exceed 90 days";
}

/// <summary>
/// Helper class for appointment type default durations.
/// </summary>
public static class AppointmentTypeDefaults
{
    public static int GetDefaultDuration(AppointmentType type) => type switch
    {
        AppointmentType.Consultation => 30,
        AppointmentType.FollowUp => 15,
        AppointmentType.Emergency => 60,
        AppointmentType.NewPatient => 45,
        AppointmentType.HearingTest => 45,
        AppointmentType.HearingAidFitting => 60,
        AppointmentType.DentalCheckup => 30,
        AppointmentType.DentalProcedure => 60,
        AppointmentType.RootCanal => 90,
        AppointmentType.Extraction => 45,
        AppointmentType.EyeExam => 30,
        AppointmentType.ContactLensFitting => 45,
        AppointmentType.PhysioAssessment => 60,
        AppointmentType.PhysioSession => 45,
        _ => 30
    };
}
