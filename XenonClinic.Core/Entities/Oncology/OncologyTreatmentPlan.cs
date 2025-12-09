using XenonClinic.Core.Enums.Oncology;

namespace XenonClinic.Core.Entities.Oncology;

public class OncologyTreatmentPlan
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int CancerDiagnosisId { get; set; }
    public DateTime PlanDate { get; set; }
    public TreatmentIntent Intent { get; set; }
    public string? PlanName { get; set; }
    public string? ProtocolName { get; set; }
    public bool? ClinicalTrial { get; set; }
    public string? ClinicalTrialId { get; set; }
    public bool? SurgeryPlanned { get; set; }
    public string? SurgeryDetails { get; set; }
    public DateTime? SurgeryDate { get; set; }
    public bool? ChemotherapyPlanned { get; set; }
    public string? ChemoRegimen { get; set; }
    public int? ChemoCycles { get; set; }
    public string? ChemoSchedule { get; set; }
    public bool? RadiationPlanned { get; set; }
    public string? RadiationDetails { get; set; }
    public decimal? RadiationDose { get; set; }
    public int? RadiationFractions { get; set; }
    public bool? ImmunotherapyPlanned { get; set; }
    public string? ImmunotherapyDetails { get; set; }
    public bool? HormoneTherapyPlanned { get; set; }
    public string? HormoneTherapyDetails { get; set; }
    public bool? TargetedTherapyPlanned { get; set; }
    public string? TargetedTherapyDetails { get; set; }
    public string? SupportiveCare { get; set; }
    public string? ResponseCriteria { get; set; }
    public string? RestaStagingSchedule { get; set; }
    public string? TumorBoardDate { get; set; }
    public string? TumorBoardRecommendations { get; set; }
    public TreatmentPlanStatus Status { get; set; }
    public string? ModificationReason { get; set; }
    public string? DiscontinuationReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public CancerDiagnosis? CancerDiagnosis { get; set; }
    public ICollection<ChemotherapySession> ChemotherapySessions { get; set; } = new List<ChemotherapySession>();
    public ICollection<RadiationRecord> RadiationRecords { get; set; } = new List<RadiationRecord>();
}
