namespace XenonClinic.Core.Enums;

/// <summary>
/// Represents the type of appointment
/// </summary>
public enum AppointmentType
{
    /// <summary>
    /// Hearing test appointment
    /// </summary>
    HearingTest = 0,

    /// <summary>
    /// Hearing aid fitting appointment
    /// </summary>
    Fitting = 1,

    /// <summary>
    /// General consultation
    /// </summary>
    Consultation = 2,

    /// <summary>
    /// Device repair appointment
    /// </summary>
    Repair = 3,

    /// <summary>
    /// Follow-up appointment
    /// </summary>
    FollowUp = 4
}
