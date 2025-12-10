using XenonClinic.Core.Enums.Gastroenterology;

namespace XenonClinic.Core.Entities.Gastroenterology;

public class DigestiveCondition
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DigestiveConditionType ConditionType { get; set; }
    public string? ConditionName { get; set; }
    public string? ICD10Code { get; set; }
    public DateTime DiagnosisDate { get; set; }
    public DateTime? OnsetDate { get; set; }
    public GastroConditionSeverity Severity { get; set; }
    public GastroConditionStatus Status { get; set; }
    public string? Location { get; set; }
    public string? DiagnosticTests { get; set; }
    public string? PathologyFindings { get; set; }
    public string? CurrentSymptoms { get; set; }
    public string? Triggers { get; set; }
    public string? Medications { get; set; }
    public string? DietaryModifications { get; set; }
    public string? SurgicalHistory { get; set; }
    public string? Complications { get; set; }
    public string? MonitoringPlan { get; set; }
    public DateTime? LastFlareDate { get; set; }
    public DateTime? NextScreeningDue { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
