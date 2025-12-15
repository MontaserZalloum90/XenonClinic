namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents an activity or task within a case
/// </summary>
public class CaseActivity
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public int CaseActivityTypeId { get; set; }
    public int CaseActivityStatusId { get; set; }
    public int CasePriorityId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// User assigned to complete this activity
    /// </summary>
    public string? AssignedToUserId { get; set; }

    /// <summary>
    /// Due date for this activity
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Date when the activity was completed
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Result or outcome of the activity
    /// </summary>
    public string? Result { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Case Case { get; set; } = null!;
    public Lookups.CaseActivityTypeLookup CaseActivityType { get; set; } = null!;
    public Lookups.CaseActivityStatusLookup CaseActivityStatus { get; set; } = null!;
    public Lookups.CasePriorityLookup CasePriority { get; set; } = null!;
    /// <summary>
    /// Assigned user navigation property - configured in DbContext
    /// Uses object type to avoid circular dependency with Infrastructure's ApplicationUser
    /// </summary>
    public object? AssignedToUser { get; set; }
}
