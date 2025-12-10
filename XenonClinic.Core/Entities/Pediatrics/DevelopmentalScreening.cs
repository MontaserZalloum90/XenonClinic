namespace XenonClinic.Core.Entities.Pediatrics;

/// <summary>
/// Represents a standardized developmental screening assessment
/// </summary>
public class DevelopmentalScreening
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PediatricVisitId { get; set; }
    public DateTime ScreeningDate { get; set; }

    /// <summary>
    /// Age at screening in months
    /// </summary>
    public int AgeInMonths { get; set; }

    /// <summary>
    /// Screening tool used (ASQ-3, ASQ-SE, Denver II, PEDS, M-CHAT, etc.)
    /// </summary>
    public string ScreeningTool { get; set; } = string.Empty;

    /// <summary>
    /// Version of the screening tool
    /// </summary>
    public string? ToolVersion { get; set; }

    /// <summary>
    /// Communication domain score
    /// </summary>
    public decimal? CommunicationScore { get; set; }
    public string? CommunicationResult { get; set; }

    /// <summary>
    /// Gross motor domain score
    /// </summary>
    public decimal? GrossMotorScore { get; set; }
    public string? GrossMotorResult { get; set; }

    /// <summary>
    /// Fine motor domain score
    /// </summary>
    public decimal? FineMotorScore { get; set; }
    public string? FineMotorResult { get; set; }

    /// <summary>
    /// Problem solving domain score
    /// </summary>
    public decimal? ProblemSolvingScore { get; set; }
    public string? ProblemSolvingResult { get; set; }

    /// <summary>
    /// Personal-social domain score
    /// </summary>
    public decimal? PersonalSocialScore { get; set; }
    public string? PersonalSocialResult { get; set; }

    /// <summary>
    /// Social-emotional score (for ASQ-SE)
    /// </summary>
    public decimal? SocialEmotionalScore { get; set; }
    public string? SocialEmotionalResult { get; set; }

    /// <summary>
    /// Overall result (Normal, Monitor, Below Cutoff, Refer)
    /// </summary>
    public string OverallResult { get; set; } = "Normal";

    /// <summary>
    /// Raw response data (JSON)
    /// </summary>
    public string? ResponseDataJson { get; set; }

    /// <summary>
    /// Areas of concern identified
    /// </summary>
    public string? AreasOfConcern { get; set; }

    /// <summary>
    /// Recommendations
    /// </summary>
    public string? Recommendations { get; set; }

    /// <summary>
    /// Referrals made
    /// </summary>
    public string? ReferralsMade { get; set; }

    /// <summary>
    /// Follow-up plan
    /// </summary>
    public string? FollowUpPlan { get; set; }

    /// <summary>
    /// Person who completed the screening (Parent, Caregiver, Clinician)
    /// </summary>
    public string? CompletedBy { get; set; }

    public string? AdministeredById { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PediatricVisit? Visit { get; set; }
}
