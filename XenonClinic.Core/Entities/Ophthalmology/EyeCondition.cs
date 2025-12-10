namespace XenonClinic.Core.Entities.Ophthalmology;

/// <summary>
/// Represents a tracked eye condition for a patient
/// </summary>
public class EyeCondition
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }

    /// <summary>
    /// Condition name (e.g., Glaucoma, Cataract, AMD, Diabetic Retinopathy)
    /// </summary>
    public string ConditionName { get; set; } = string.Empty;

    /// <summary>
    /// ICD-10 code
    /// </summary>
    public string? IcdCode { get; set; }

    /// <summary>
    /// Eye affected (OD, OS, OU)
    /// </summary>
    public string Eye { get; set; } = "OU";

    /// <summary>
    /// Severity (Mild, Moderate, Severe, Advanced)
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// Status (Active, Resolved, Stable, Progressing, InRemission)
    /// </summary>
    public string Status { get; set; } = "Active";

    public DateTime DiagnosisDate { get; set; }
    public DateTime? ResolutionDate { get; set; }

    /// <summary>
    /// Current treatment for this condition
    /// </summary>
    public string? CurrentTreatment { get; set; }

    /// <summary>
    /// Monitoring frequency (e.g., Monthly, Quarterly, Annually)
    /// </summary>
    public string? MonitoringFrequency { get; set; }

    public DateTime? LastAssessmentDate { get; set; }
    public DateTime? NextAssessmentDate { get; set; }

    public string? DiagnosedById { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
