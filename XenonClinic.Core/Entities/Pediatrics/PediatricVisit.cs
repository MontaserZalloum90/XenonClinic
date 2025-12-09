namespace XenonClinic.Core.Entities.Pediatrics;

/// <summary>
/// Represents a pediatric clinic visit
/// </summary>
public class PediatricVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }

    public DateTime VisitDate { get; set; }

    /// <summary>
    /// Visit type (WellChild, Sick, Follow-up, Emergency, NewPatient, Immunization)
    /// </summary>
    public string VisitType { get; set; } = "WellChild";

    /// <summary>
    /// Age at visit (in months for infants, years for older children)
    /// </summary>
    public string? AgeAtVisit { get; set; }

    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }

    // Vital Signs
    /// <summary>
    /// Weight in kg
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Height/Length in cm
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Head circumference in cm (for infants)
    /// </summary>
    public decimal? HeadCircumference { get; set; }

    /// <summary>
    /// BMI calculated
    /// </summary>
    public decimal? Bmi { get; set; }

    /// <summary>
    /// Weight percentile
    /// </summary>
    public int? WeightPercentile { get; set; }

    /// <summary>
    /// Height percentile
    /// </summary>
    public int? HeightPercentile { get; set; }

    /// <summary>
    /// BMI percentile
    /// </summary>
    public int? BmiPercentile { get; set; }

    /// <summary>
    /// Head circumference percentile
    /// </summary>
    public int? HeadCircumferencePercentile { get; set; }

    /// <summary>
    /// Temperature in Celsius
    /// </summary>
    public decimal? Temperature { get; set; }

    /// <summary>
    /// Heart rate
    /// </summary>
    public int? HeartRate { get; set; }

    /// <summary>
    /// Respiratory rate
    /// </summary>
    public int? RespiratoryRate { get; set; }

    /// <summary>
    /// Blood pressure (if applicable)
    /// </summary>
    public string? BloodPressure { get; set; }

    /// <summary>
    /// Oxygen saturation
    /// </summary>
    public int? OxygenSaturation { get; set; }

    public string? PhysicalExamFindings { get; set; }
    public string? Diagnosis { get; set; }
    public string? DiagnosisCodes { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Prescriptions { get; set; }

    /// <summary>
    /// Feeding/nutrition notes
    /// </summary>
    public string? NutritionNotes { get; set; }

    /// <summary>
    /// Developmental notes
    /// </summary>
    public string? DevelopmentalNotes { get; set; }

    /// <summary>
    /// Safety/anticipatory guidance given
    /// </summary>
    public string? AnticpatoryGuidance { get; set; }

    public string? ParentConcerns { get; set; }
    public DateTime? NextVisitDate { get; set; }

    public string? PediatricianId { get; set; }
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
