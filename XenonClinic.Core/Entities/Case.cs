namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a patient case for managing comprehensive care episodes
/// </summary>
public class Case
{
    public int Id { get; set; }

    /// <summary>
    /// Unique case reference number (e.g., CASE-2024-001)
    /// </summary>
    public string CaseNumber { get; set; } = string.Empty;

    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int CaseTypeId { get; set; }
    public int CaseStatusId { get; set; }
    public int CasePriorityId { get; set; }

    /// <summary>
    /// Primary provider/audiologist assigned to this case
    /// </summary>
    public string? AssignedToUserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Chief complaint or reason for the case
    /// </summary>
    public string? ChiefComplaint { get; set; }

    /// <summary>
    /// Expected or target date for case completion
    /// </summary>
    public DateTime? TargetDate { get; set; }

    public DateTime OpenedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedDate { get; set; }

    public string? OpenedBy { get; set; }
    public string? ClosedBy { get; set; }

    /// <summary>
    /// Outcome or resolution of the case
    /// </summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// Tags for categorization and filtering (comma-separated)
    /// </summary>
    public string? Tags { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public CaseType CaseType { get; set; } = null!;
    public CaseStatus CaseStatus { get; set; } = null!;
    public Lookups.CasePriorityLookup CasePriority { get; set; } = null!;
    // Note: ApplicationUser navigation removed to avoid circular dependency with Infrastructure

    public ICollection<CaseNote> Notes { get; set; } = new List<CaseNote>();
    public ICollection<CaseActivity> Activities { get; set; } = new List<CaseActivity>();
}
