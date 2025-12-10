namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the status of an appointment
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Appointment is scheduled but not yet confirmed
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Appointment has been confirmed by patient or staff
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Patient has checked in and is waiting
    /// </summary>
    CheckedIn = 2,

    /// <summary>
    /// Appointment is in progress (patient with provider)
    /// </summary>
    InProgress = 3,

    /// <summary>
    /// Appointment has been completed
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Appointment was cancelled
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Patient did not show up for appointment
    /// </summary>
    NoShow = 6,

    /// <summary>
    /// Legacy: Appointment is scheduled and confirmed (maps to Confirmed)
    /// </summary>
    [Obsolete("Use Scheduled or Confirmed instead")]
    Booked = 1
}
