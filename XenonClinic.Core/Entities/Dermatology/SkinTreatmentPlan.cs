namespace XenonClinic.Core.Entities.Dermatology;

/// <summary>
/// Represents a comprehensive skin treatment plan (often for cosmetic or chronic conditions)
/// </summary>
public class SkinTreatmentPlan
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? SkinConditionId { get; set; }

    public string PlanName { get; set; } = string.Empty;

    /// <summary>
    /// Treatment goal (e.g., Acne clearance, Anti-aging, Pigmentation correction)
    /// </summary>
    public string? TreatmentGoal { get; set; }

    /// <summary>
    /// Plan type (Medical, Cosmetic, Combined)
    /// </summary>
    public string PlanType { get; set; } = "Medical";

    /// <summary>
    /// Status (Draft, Active, Completed, Cancelled, OnHold)
    /// </summary>
    public string Status { get; set; } = "Draft";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    /// <summary>
    /// Number of sessions planned (for procedure-based treatments)
    /// </summary>
    public int? TotalSessionsPlanned { get; set; }
    public int CompletedSessions { get; set; }

    /// <summary>
    /// Topical medications prescribed
    /// </summary>
    public string? TopicalMedications { get; set; }

    /// <summary>
    /// Oral medications prescribed
    /// </summary>
    public string? OralMedications { get; set; }

    /// <summary>
    /// Recommended skincare routine
    /// </summary>
    public string? SkincareRoutine { get; set; }

    /// <summary>
    /// Lifestyle recommendations (diet, sun protection, etc.)
    /// </summary>
    public string? LifestyleRecommendations { get; set; }

    /// <summary>
    /// Procedures included in the plan
    /// </summary>
    public string? PlannedProcedures { get; set; }

    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }

    /// <summary>
    /// Treatment response assessment
    /// </summary>
    public string? ResponseAssessment { get; set; }

    /// <summary>
    /// Before treatment photo path
    /// </summary>
    public string? BeforePhotoPath { get; set; }

    /// <summary>
    /// Progress photo paths (JSON array)
    /// </summary>
    public string? ProgressPhotosJson { get; set; }

    /// <summary>
    /// After treatment photo path
    /// </summary>
    public string? AfterPhotoPath { get; set; }

    public string? DermatologistId { get; set; }
    public string? Notes { get; set; }

    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public SkinCondition? SkinCondition { get; set; }
}
