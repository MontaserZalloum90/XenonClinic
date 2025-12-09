using XenonClinic.Core.Enums.Podiatry;

namespace XenonClinic.Core.Entities.Podiatry;

public class FootCondition
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public FootConditionType ConditionType { get; set; }
    public string? ConditionName { get; set; }
    public string? ICD10Code { get; set; }
    public DateTime DiagnosisDate { get; set; }
    public DateTime? OnsetDate { get; set; }
    public FootSide Side { get; set; }
    public string? Location { get; set; }
    public FootConditionSeverity Severity { get; set; }
    public FootConditionStatus Status { get; set; }
    public string? Etiology { get; set; }
    public string? ContributingFactors { get; set; }
    public string? CurrentSymptoms { get; set; }
    public string? CurrentTreatment { get; set; }
    public bool? Orthotics { get; set; }
    public bool? SurgicalHistory { get; set; }
    public string? SurgicalDetails { get; set; }
    public string? PrognosticFactors { get; set; }
    public string? MonitoringPlan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
}
