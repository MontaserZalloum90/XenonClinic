namespace XenonClinic.Core.Entities.Physiotherapy;

/// <summary>
/// Represents functional outcome measurements and progress tracking
/// </summary>
public class FunctionalOutcome
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PhysioAssessmentId { get; set; }
    public DateTime MeasurementDate { get; set; }

    /// <summary>
    /// Outcome measure tool used (e.g., Oswestry, NDI, DASH, KOOS, SF-36, VAS)
    /// </summary>
    public string OutcomeMeasure { get; set; } = string.Empty;

    /// <summary>
    /// Category of the measure (Pain, Function, Disability, Quality of Life)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Score achieved
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// Maximum possible score
    /// </summary>
    public decimal? MaxScore { get; set; }

    /// <summary>
    /// Percentage score
    /// </summary>
    public decimal? PercentageScore { get; set; }

    /// <summary>
    /// Interpretation (Minimal, Mild, Moderate, Severe, Complete)
    /// </summary>
    public string? Interpretation { get; set; }

    /// <summary>
    /// Change from baseline score
    /// </summary>
    public decimal? ChangeFromBaseline { get; set; }

    /// <summary>
    /// Whether minimal clinically important difference (MCID) achieved
    /// </summary>
    public bool? McidAchieved { get; set; }

    /// <summary>
    /// Raw response data (JSON)
    /// </summary>
    public string? ResponseDataJson { get; set; }

    public string? MeasuredById { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PhysioAssessment? Assessment { get; set; }
}
