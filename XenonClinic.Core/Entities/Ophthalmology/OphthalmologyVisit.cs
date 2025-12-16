namespace XenonClinic.Core.Entities.Ophthalmology;

/// <summary>
/// Represents an ophthalmology clinic visit
/// </summary>
public class OphthalmologyVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime VisitDate { get; set; }
    public int? ProviderId { get; set; }

    /// <summary>
    /// Visit type (Routine, Comprehensive, Follow-up, Emergency, Pre-operative, Post-operative)
    /// </summary>
    public string VisitType { get; set; } = "Routine";
    public string? ChiefComplaint { get; set; }

    /// <summary>
    /// History of present illness
    /// </summary>
    public string? HistoryOfPresentIllness { get; set; }
    public string? Diagnosis { get; set; }

    /// <summary>
    /// ICD-10 diagnosis codes (comma-separated)
    /// </summary>
    public string? DiagnosisCodes { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Prescriptions { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextVisitDate { get; set; }

    /// <summary>
    /// Intraocular pressure - Right eye (mmHg)
    /// </summary>
    public decimal? IopOd { get; set; }

    /// <summary>
    /// Intraocular pressure - Left eye (mmHg)
    /// </summary>
    public decimal? IopOs { get; set; }
    public string? OphthalmologistId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public VisionTest? VisionTest { get; set; }
    public EyeExam? EyeExam { get; set; }
    public ICollection<EyeProcedure> Procedures { get; set; } = new List<EyeProcedure>();
}
