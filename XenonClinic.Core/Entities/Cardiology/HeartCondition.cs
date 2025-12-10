using XenonClinic.Core.Enums.Cardiology;

namespace XenonClinic.Core.Entities.Cardiology;

public class HeartCondition
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public HeartConditionType ConditionType { get; set; }
    public string? ConditionName { get; set; }
    public string? ICD10Code { get; set; }
    public DateTime DiagnosisDate { get; set; }
    public DateTime? OnsetDate { get; set; }
    public ConditionSeverity Severity { get; set; }
    public ConditionStatus Status { get; set; }
    public string? NYHAClass { get; set; }
    public string? EtiologyDetails { get; set; }
    public string? AffectedVessels { get; set; }
    public string? AffectedValves { get; set; }
    public decimal? BaselineEjectionFraction { get; set; }
    public string? CurrentMedications { get; set; }
    public string? TreatmentHistory { get; set; }
    public string? RiskFactors { get; set; }
    public string? Complications { get; set; }
    public DateTime? LastEvaluationDate { get; set; }
    public string? Prognosis { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
