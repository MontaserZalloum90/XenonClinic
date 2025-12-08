namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for appointment statuses (replaces AppointmentStatus enum).
/// Examples: Booked, Completed, Cancelled, No-Show
/// </summary>
public class AppointmentStatusLookup : SystemLookup
{
    /// <summary>
    /// Whether this status represents a completed appointment.
    /// </summary>
    public bool IsCompletedStatus { get; set; } = false;

    /// <summary>
    /// Whether this status represents a cancelled appointment.
    /// </summary>
    public bool IsCancelledStatus { get; set; } = false;

    /// <summary>
    /// Whether appointments in this status should appear in active lists.
    /// </summary>
    public bool ShowInActiveView { get; set; } = true;

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
