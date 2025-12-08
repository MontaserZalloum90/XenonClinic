namespace XenonClinic.Core.Entities.Lookups;

/// <summary>
/// Dynamic lookup for case priorities (replaces CasePriority enum).
/// Examples: Low, Medium, High, Urgent
/// </summary>
public class CasePriorityLookup : SystemLookup
{
    /// <summary>
    /// Priority level for sorting (higher = more urgent).
    /// </summary>
    public int PriorityLevel { get; set; }

    /// <summary>
    /// Expected response time in hours.
    /// </summary>
    public int? ExpectedResponseTimeHours { get; set; }

    // Navigation properties
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
