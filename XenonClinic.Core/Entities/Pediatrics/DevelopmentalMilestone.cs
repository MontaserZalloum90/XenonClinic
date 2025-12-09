namespace XenonClinic.Core.Entities.Pediatrics;

/// <summary>
/// Represents a developmental milestone record for tracking child development
/// </summary>
public class DevelopmentalMilestone
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PediatricVisitId { get; set; }

    public DateTime AssessmentDate { get; set; }

    /// <summary>
    /// Age at assessment in months
    /// </summary>
    public int AgeInMonths { get; set; }

    /// <summary>
    /// Domain (Gross Motor, Fine Motor, Language, Cognitive, Social-Emotional, Self-Help)
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Milestone name (e.g., "Rolls over", "First words", "Walks independently")
    /// </summary>
    public string MilestoneName { get; set; } = string.Empty;

    /// <summary>
    /// Expected age range for this milestone (e.g., "4-6 months")
    /// </summary>
    public string? ExpectedAgeRange { get; set; }

    /// <summary>
    /// Status (Achieved, NotYet, Delayed, Regressed, Emerging)
    /// </summary>
    public string Status { get; set; } = "Achieved";

    /// <summary>
    /// Date milestone was achieved (if different from assessment date)
    /// </summary>
    public DateTime? DateAchieved { get; set; }

    /// <summary>
    /// How milestone was assessed (Parent Report, Direct Observation, Formal Screening)
    /// </summary>
    public string? AssessmentMethod { get; set; }

    /// <summary>
    /// Screening tool used if any (ASQ, PEDS, Denver II, etc.)
    /// </summary>
    public string? ScreeningTool { get; set; }

    /// <summary>
    /// Concern level (None, Monitor, Mild Concern, Significant Concern)
    /// </summary>
    public string? ConcernLevel { get; set; }

    /// <summary>
    /// Referral made for further evaluation
    /// </summary>
    public bool ReferralMade { get; set; }

    /// <summary>
    /// Referral details
    /// </summary>
    public string? ReferralDetails { get; set; }

    /// <summary>
    /// Parent concerns about development
    /// </summary>
    public string? ParentConcerns { get; set; }

    public string? AssessedById { get; set; }
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PediatricVisit? Visit { get; set; }
}
