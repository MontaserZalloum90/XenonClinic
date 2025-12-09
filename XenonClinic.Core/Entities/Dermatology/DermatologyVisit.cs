namespace XenonClinic.Core.Entities.Dermatology;

/// <summary>
/// Represents a dermatology clinic visit
/// </summary>
public class DermatologyVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }

    public DateTime VisitDate { get; set; }

    /// <summary>
    /// Visit type (Initial, Follow-up, Procedure, Cosmetic, Urgent)
    /// </summary>
    public string VisitType { get; set; } = "Initial";

    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }

    /// <summary>
    /// Skin type (Fitzpatrick scale I-VI)
    /// </summary>
    public string? SkinType { get; set; }

    public string? PhysicalExamFindings { get; set; }
    public string? Diagnosis { get; set; }

    /// <summary>
    /// ICD-10 diagnosis codes
    /// </summary>
    public string? DiagnosisCodes { get; set; }

    public string? TreatmentPlan { get; set; }
    public string? Prescriptions { get; set; }
    public string? HomeCarInstructions { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextVisitDate { get; set; }

    public string? DermatologistId { get; set; }
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<LesionRecord> Lesions { get; set; } = new List<LesionRecord>();
    public ICollection<SkinProcedure> Procedures { get; set; } = new List<SkinProcedure>();
}
