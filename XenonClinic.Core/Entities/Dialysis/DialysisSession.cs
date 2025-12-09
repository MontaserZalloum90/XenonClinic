using XenonClinic.Core.Enums.Dialysis;

namespace XenonClinic.Core.Entities.Dialysis;

public class DialysisSession
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? DialysisPatientRecordId { get; set; }
    public DateTime SessionDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? ActualDurationMinutes { get; set; }
    public DialysisModality Modality { get; set; }
    public string? MachineNumber { get; set; }
    public AccessType AccessUsed { get; set; }
    public string? AccessStatus { get; set; }
    public decimal? PreWeight { get; set; }
    public decimal? PostWeight { get; set; }
    public decimal? TargetDryWeight { get; set; }
    public decimal? UFGoal { get; set; }
    public decimal? ActualUFRemoved { get; set; }
    public int? PreSystolicBP { get; set; }
    public int? PreDiastolicBP { get; set; }
    public int? PreHeartRate { get; set; }
    public int? PreTemperature { get; set; }
    public int? PostSystolicBP { get; set; }
    public int? PostDiastolicBP { get; set; }
    public int? PostHeartRate { get; set; }
    public decimal? BloodFlowRate { get; set; }
    public decimal? DialysateFlowRate { get; set; }
    public string? DialyzerUsed { get; set; }
    public string? AnticoagulantUsed { get; set; }
    public decimal? HeparinDose { get; set; }
    public decimal? Kt_V { get; set; }
    public decimal? URR { get; set; }
    public string? IntraDialyticSymptoms { get; set; }
    public bool? Hypotension { get; set; }
    public bool? Cramping { get; set; }
    public bool? Nausea { get; set; }
    public string? InterventionsDuringSession { get; set; }
    public string? NurseNotes { get; set; }
    public SessionStatus Status { get; set; }
    public string? EarlyTerminationReason { get; set; }
    public string? StaffInitials { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public DialysisPatientRecord? DialysisPatientRecord { get; set; }
}
