namespace XenonClinic.Core.Entities.Dermatology;

/// <summary>
/// Represents a documented skin lesion for monitoring
/// </summary>
public class LesionRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? DermatologyVisitId { get; set; }

    /// <summary>
    /// Unique identifier for tracking this lesion across visits
    /// </summary>
    public string LesionCode { get; set; } = string.Empty;

    /// <summary>
    /// Body location (e.g., Right forearm, Left cheek, Upper back)
    /// </summary>
    public string BodyLocation { get; set; } = string.Empty;

    /// <summary>
    /// Lesion type (Macule, Papule, Nodule, Plaque, Vesicle, Pustule, Cyst, etc.)
    /// </summary>
    public string? LesionType { get; set; }

    /// <summary>
    /// Size in mm (e.g., "5x3" or "5")
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Color description
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Shape (Round, Oval, Irregular, Linear)
    /// </summary>
    public string? Shape { get; set; }

    /// <summary>
    /// Border characteristics (Well-defined, Irregular, Raised)
    /// </summary>
    public string? Border { get; set; }

    /// <summary>
    /// Surface characteristics (Smooth, Rough, Scaly, Ulcerated)
    /// </summary>
    public string? Surface { get; set; }

    /// <summary>
    /// ABCDE assessment for melanoma screening
    /// </summary>
    public string? AbcdeAssessment { get; set; }

    /// <summary>
    /// Clinical impression/suspected diagnosis
    /// </summary>
    public string? ClinicalImpression { get; set; }

    /// <summary>
    /// Risk level (Low, Moderate, High, Urgent)
    /// </summary>
    public string? RiskLevel { get; set; }

    /// <summary>
    /// Status (New, Stable, Changed, Resolved, Removed)
    /// </summary>
    public string Status { get; set; } = "New";

    public DateTime FirstDocumentedDate { get; set; }
    public DateTime LastAssessedDate { get; set; }
    public DateTime? NextReviewDate { get; set; }

    /// <summary>
    /// Clinical photo path
    /// </summary>
    public string? PhotoPath { get; set; }

    /// <summary>
    /// Dermoscopy image path
    /// </summary>
    public string? DermoscopyImagePath { get; set; }

    /// <summary>
    /// Dermoscopy findings
    /// </summary>
    public string? DermoscopyFindings { get; set; }

    /// <summary>
    /// Biopsy performed
    /// </summary>
    public bool BiopsyPerformed { get; set; }

    /// <summary>
    /// Biopsy result/pathology
    /// </summary>
    public string? BiopsyResult { get; set; }

    public string? DocumentedById { get; set; }
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public DermatologyVisit? Visit { get; set; }
}
