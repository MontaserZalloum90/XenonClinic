namespace XenonClinic.Core.Entities.Physiotherapy;

/// <summary>
/// Represents an initial or follow-up physiotherapy assessment
/// </summary>
public class PhysioAssessment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime AssessmentDate { get; set; }

    /// <summary>
    /// Assessment type (Initial, Follow-up, Discharge, Re-assessment)
    /// </summary>
    public string AssessmentType { get; set; } = "Initial";

    /// <summary>
    /// Referral source (Self, Physician, Orthopedic, Neurologist, etc.)
    /// </summary>
    public string? ReferralSource { get; set; }

    /// <summary>
    /// Referring physician name/ID
    /// </summary>
    public string? ReferringPhysician { get; set; }

    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }
    public string? PainDescription { get; set; }

    /// <summary>
    /// Pain level (0-10 scale)
    /// </summary>
    public int? PainLevel { get; set; }

    /// <summary>
    /// Pain location(s)
    /// </summary>
    public string? PainLocation { get; set; }

    /// <summary>
    /// Aggravating factors
    /// </summary>
    public string? AggravatingFactors { get; set; }

    /// <summary>
    /// Relieving factors
    /// </summary>
    public string? RelievingFactors { get; set; }

    /// <summary>
    /// Medical history relevant to physiotherapy
    /// </summary>
    public string? MedicalHistory { get; set; }

    /// <summary>
    /// Surgical history
    /// </summary>
    public string? SurgicalHistory { get; set; }

    /// <summary>
    /// Current medications
    /// </summary>
    public string? CurrentMedications { get; set; }

    /// <summary>
    /// Imaging results (X-ray, MRI, CT findings)
    /// </summary>
    public string? ImagingResults { get; set; }

    /// <summary>
    /// Posture assessment findings
    /// </summary>
    public string? PostureAssessment { get; set; }

    /// <summary>
    /// Gait analysis findings
    /// </summary>
    public string? GaitAnalysis { get; set; }

    /// <summary>
    /// Manual muscle testing results
    /// </summary>
    public string? MuscleTesting { get; set; }

    /// <summary>
    /// Special tests performed and results
    /// </summary>
    public string? SpecialTests { get; set; }

    /// <summary>
    /// Neurological examination findings
    /// </summary>
    public string? NeurologicalExam { get; set; }

    /// <summary>
    /// Functional limitations
    /// </summary>
    public string? FunctionalLimitations { get; set; }

    public string? Diagnosis { get; set; }

    /// <summary>
    /// ICD-10 codes
    /// </summary>
    public string? DiagnosisCodes { get; set; }

    /// <summary>
    /// Short-term goals
    /// </summary>
    public string? ShortTermGoals { get; set; }

    /// <summary>
    /// Long-term goals
    /// </summary>
    public string? LongTermGoals { get; set; }

    /// <summary>
    /// Treatment plan summary
    /// </summary>
    public string? TreatmentPlan { get; set; }

    /// <summary>
    /// Recommended frequency of sessions
    /// </summary>
    public string? RecommendedFrequency { get; set; }

    /// <summary>
    /// Estimated duration of treatment (weeks)
    /// </summary>
    public int? EstimatedDurationWeeks { get; set; }

    /// <summary>
    /// Prognosis (Excellent, Good, Fair, Poor)
    /// </summary>
    public string? Prognosis { get; set; }

    public string? PhysiotherapistId { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<RangeOfMotionRecord> RangeOfMotionRecords { get; set; } = new List<RangeOfMotionRecord>();
}
