namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents the status/stage of a case in the workflow
/// </summary>
public class CaseStatus
{
    public int Id { get; set; }
    public int TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Category for grouping statuses: Open, InProgress, Closed, Cancelled
    /// </summary>
    public CaseStatusCategory Category { get; set; } = CaseStatusCategory.Open;

    /// <summary>
    /// Color code for visual identification (e.g., "#1F6FEB")
    /// </summary>
    public string? ColorCode { get; set; }

    /// <summary>
    /// Icon class for UI display (e.g., "bi bi-circle-fill")
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Whether this is a final status (case cannot be reopened)
    /// </summary>
    public bool IsFinalStatus { get; set; } = false;

    /// <summary>
    /// Whether this status indicates case closure
    /// </summary>
    public bool IsClosedStatus { get; set; } = false;

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}

public enum CaseStatusCategory
{
    Open = 1,
    InProgress = 2,
    OnHold = 3,
    Closed = 4,
    Cancelled = 5
}
