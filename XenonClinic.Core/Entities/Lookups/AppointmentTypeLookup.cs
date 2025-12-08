namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for appointment types (replaces AppointmentType enum).
/// Examples: Hearing Test, Fitting, Consultation, Repair, Follow-up
/// </summary>
public class AppointmentTypeLookup : SystemLookup
{
    /// <summary>
    /// Expected duration in minutes for this appointment type.
    /// </summary>
    public int? DefaultDurationMinutes { get; set; }

    /// <summary>
    /// Whether this appointment type requires patient preparation instructions.
    /// </summary>
    public bool RequiresPreparation { get; set; } = false;

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
