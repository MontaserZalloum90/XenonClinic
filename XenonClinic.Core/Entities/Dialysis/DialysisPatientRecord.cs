using XenonClinic.Core.Enums.Dialysis;

namespace XenonClinic.Core.Entities.Dialysis;

public class DialysisPatientRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime ESRDOnsetDate { get; set; }
    public DateTime DialysisStartDate { get; set; }
    public DialysisModality Modality { get; set; }
    public string? ModalityHistory { get; set; }
    public string? PrimaryCauseESRD { get; set; }
    public string? ICD10Code { get; set; }
    public string? BloodType { get; set; }
    public AccessType CurrentAccessType { get; set; }
    public string? AccessLocation { get; set; }
    public DateTime? AccessPlacementDate { get; set; }
    public string? DryWeight { get; set; }
    public decimal? DryWeightKg { get; set; }
    public decimal? TargetUFVolume { get; set; }
    public string? DialysisSchedule { get; set; }
    public int? DialysisDaysPerWeek { get; set; }
    public decimal? SessionDurationHours { get; set; }
    public string? PrescribedDialyzer { get; set; }
    public decimal? BloodFlowRate { get; set; }
    public decimal? DialysateFlowRate { get; set; }
    public string? Anticoagulation { get; set; }
    public string? DialysatePotassium { get; set; }
    public string? DialysateCalcium { get; set; }
    public string? DialysateBicarbonate { get; set; }
    public bool? TransplantCandidate { get; set; }
    public DateTime? TransplantListDate { get; set; }
    public string? TransplantStatus { get; set; }
    public string? EPOAgent { get; set; }
    public string? EPODose { get; set; }
    public string? IronTherapy { get; set; }
    public string? PhosphateBinder { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<DialysisSession> Sessions { get; set; } = new List<DialysisSession>();
}
