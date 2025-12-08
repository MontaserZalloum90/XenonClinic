namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for case activity types (replaces CaseActivityType enum).
/// Examples: Task, Appointment, Test, Follow-up, Phone Call, Email, Consultation, Review
/// </summary>
public class CaseActivityTypeLookup : SystemLookup
{
    /// <summary>
    /// Whether this activity type requires a due date.
    /// </summary>
    public bool RequiresDueDate { get; set; } = true;

    /// <summary>
    /// Whether this activity type can be assigned to users.
    /// </summary>
    public bool CanAssign { get; set; } = true;

    // Navigation properties
    public ICollection<CaseActivity> CaseActivities { get; set; } = new List<CaseActivity>();
}
