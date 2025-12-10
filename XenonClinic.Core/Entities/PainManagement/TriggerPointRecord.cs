using XenonClinic.Core.Enums.PainManagement;

namespace XenonClinic.Core.Entities.PainManagement;

public class TriggerPointRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PainVisitId { get; set; }
    public DateTime RecordDate { get; set; }
    public string? MuscleAffected { get; set; }
    public string? TriggerPointLocation { get; set; }
    public TriggerPointSeverity Severity { get; set; }
    public string? ReferralPattern { get; set; }
    public bool? TautBandPresent { get; set; }
    public bool? LocalTwitchResponse { get; set; }
    public bool? JumpSign { get; set; }
    public string? AssociatedSymptoms { get; set; }
    public string? TreatmentPerformed { get; set; }
    public string? MedicationsInjected { get; set; }
    public decimal? VolumeInjected { get; set; }
    public string? DryNeedlingDetails { get; set; }
    public int? PreTreatmentPain { get; set; }
    public int? PostTreatmentPain { get; set; }
    public string? Response { get; set; }
    public string? HomeExercises { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PainVisit? PainVisit { get; set; }
}
