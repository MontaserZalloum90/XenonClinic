using XenonClinic.Core.Enums.SleepMedicine;

namespace XenonClinic.Core.Entities.SleepMedicine;

public class SleepDisorder
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public SleepDisorderType DisorderType { get; set; }
    public string? DisorderName { get; set; }
    public string? ICD10Code { get; set; }
    public DateTime DiagnosisDate { get; set; }
    public DateTime? OnsetDate { get; set; }
    public SleepDisorderSeverity Severity { get; set; }
    public SleepDisorderStatus Status { get; set; }
    public decimal? BaselineAHI { get; set; }
    public decimal? CurrentAHI { get; set; }
    public string? DiagnosticStudy { get; set; }
    public string? CurrentTreatment { get; set; }
    public bool? CPAPTherapy { get; set; }
    public bool? OralApplianceTherapy { get; set; }
    public bool? PositionalTherapy { get; set; }
    public bool? SurgicalTreatment { get; set; }
    public string? SurgicalDetails { get; set; }
    public string? Medications { get; set; }
    public string? BehavioralTreatment { get; set; }
    public string? TreatmentResponse { get; set; }
    public string? Complications { get; set; }
    public DateTime? LastEvaluationDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
