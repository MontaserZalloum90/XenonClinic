using XenonClinic.Core.Enums.Neurology;

namespace XenonClinic.Core.Entities.Neurology;

public class NeuroCondition
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public NeuroConditionType ConditionType { get; set; }
    public string? ConditionName { get; set; }
    public string? ICD10Code { get; set; }
    public DateTime DiagnosisDate { get; set; }
    public DateTime? OnsetDate { get; set; }
    public NeuroConditionStatus Status { get; set; }
    public string? Etiology { get; set; }
    public string? Laterality { get; set; }
    public string? SeizureType { get; set; }
    public string? SeizureFrequency { get; set; }
    public DateTime? LastSeizureDate { get; set; }
    public string? HeadacheType { get; set; }
    public string? HeadacheFrequency { get; set; }
    public string? MovementDisorderType { get; set; }
    public string? CognitiveStatus { get; set; }
    public string? FunctionalStatus { get; set; }
    public string? DisabilityScale { get; set; }
    public decimal? DisabilityScore { get; set; }
    public string? CurrentMedications { get; set; }
    public string? TreatmentHistory { get; set; }
    public string? Complications { get; set; }
    public string? Prognosis { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
