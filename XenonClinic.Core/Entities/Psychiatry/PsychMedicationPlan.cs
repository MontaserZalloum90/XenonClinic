using XenonClinic.Core.Enums.Psychiatry;

namespace XenonClinic.Core.Entities.Psychiatry;

public class PsychMedicationPlan
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PsychMedicationStatus Status { get; set; }
    public string? MedicationName { get; set; }
    public string? MedicationClass { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Route { get; set; }
    public string? Indication { get; set; }
    public string? TargetSymptoms { get; set; }
    public DateTime? TitrationStartDate { get; set; }
    public string? TitrationSchedule { get; set; }
    public string? CurrentDosage { get; set; }
    public string? MaximumDosage { get; set; }
    public string? SideEffectsExperienced { get; set; }
    public string? Effectiveness { get; set; }
    public string? Adherence { get; set; }
    public bool? RefillsAuthorized { get; set; }
    public int? RefillsRemaining { get; set; }
    public string? PrescribingPhysician { get; set; }
    public string? Pharmacy { get; set; }
    public string? DiscontinuationReason { get; set; }
    public string? TaperSchedule { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
