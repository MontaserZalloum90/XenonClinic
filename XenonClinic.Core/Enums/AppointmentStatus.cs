namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the status of an appointment
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Appointment is scheduled and confirmed
    /// </summary>
    Booked = 0,

    /// <summary>
    /// Appointment has been completed
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Appointment was cancelled
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Patient did not show up for appointment
    /// </summary>
    NoShow = 3
}
