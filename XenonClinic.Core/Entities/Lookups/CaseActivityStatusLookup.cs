namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for case activity statuses (replaces CaseActivityStatus enum).
/// Examples: Pending, In Progress, Completed, Cancelled, Overdue
/// </summary>
public class CaseActivityStatusLookup : SystemLookup
{
    /// <summary>
    /// Whether this status represents a completed activity.
    /// </summary>
    public bool IsCompletedStatus { get; set; } = false;

    /// <summary>
    /// Whether this status represents a cancelled activity.
    /// </summary>
    public bool IsCancelledStatus { get; set; } = false;

    /// <summary>
    /// Whether activities in this status contribute to progress calculations.
    /// </summary>
    public bool CountsTowardProgress { get; set; } = true;

    // Navigation properties
    public ICollection<CaseActivity> CaseActivities { get; set; } = new List<CaseActivity>();
}
