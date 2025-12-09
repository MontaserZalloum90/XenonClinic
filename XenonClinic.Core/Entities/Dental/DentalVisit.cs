namespace XenonClinic.Core.Entities.Dental;

/// <summary>
/// Represents a dental clinic visit
/// </summary>
public class DentalVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? ClinicalFindings { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentNotes { get; set; }
    public string? NextVisitRecommendation { get; set; }
    public int? DentalTreatmentPlanId { get; set; }
    public string? ProviderId { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public DentalTreatmentPlan? TreatmentPlan { get; set; }
    public ICollection<DentalProcedure> Procedures { get; set; } = new List<DentalProcedure>();
}
