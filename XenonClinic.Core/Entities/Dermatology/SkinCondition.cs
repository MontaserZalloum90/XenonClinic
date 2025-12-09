namespace XenonClinic.Core.Entities.Dermatology;

/// <summary>
/// Represents a tracked skin condition for a patient
/// </summary>
public class SkinCondition
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }

    /// <summary>
    /// Condition name (e.g., Acne, Eczema, Psoriasis, Rosacea, Melanoma)
    /// </summary>
    public string ConditionName { get; set; } = string.Empty;

    /// <summary>
    /// ICD-10 code
    /// </summary>
    public string? IcdCode { get; set; }

    /// <summary>
    /// Category (Inflammatory, Infectious, Neoplastic, Pigmentary, Autoimmune, Allergic, Cosmetic)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Body areas affected (comma-separated)
    /// </summary>
    public string? AffectedAreas { get; set; }

    /// <summary>
    /// Severity (Mild, Moderate, Severe)
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// Status (Active, Resolved, Chronic, InRemission, Recurring)
    /// </summary>
    public string Status { get; set; } = "Active";

    public DateTime OnsetDate { get; set; }
    public DateTime? DiagnosisDate { get; set; }
    public DateTime? ResolutionDate { get; set; }

    /// <summary>
    /// Known triggers (sun exposure, stress, foods, etc.)
    /// </summary>
    public string? Triggers { get; set; }

    /// <summary>
    /// Current treatment regimen
    /// </summary>
    public string? CurrentTreatment { get; set; }

    /// <summary>
    /// Treatment response (Excellent, Good, Partial, Poor, None)
    /// </summary>
    public string? TreatmentResponse { get; set; }

    public string? DiagnosedById { get; set; }
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
