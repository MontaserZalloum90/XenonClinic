using XenonClinic.Core.Enums.Chiropractic;

namespace XenonClinic.Core.Entities.Chiropractic;

public class ChiroTreatmentPlan
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime PlanDate { get; set; }
    public string? Diagnosis { get; set; }
    public string? ICD10Codes { get; set; }
    public ChiroTreatmentPhase TreatmentPhase { get; set; }
    public string? TreatmentGoals { get; set; }
    public string? ShortTermGoals { get; set; }
    public string? LongTermGoals { get; set; }
    public int? TotalVisitsPlanned { get; set; }
    public int? VisitsCompleted { get; set; }
    public string? VisitFrequency { get; set; }
    public int? DurationWeeks { get; set; }
    public string? TechniquesToBeUsed { get; set; }
    public string? AdjunctTherapies { get; set; }
    public string? HomeExerciseProgram { get; set; }
    public string? LifestyleModifications { get; set; }
    public string? ProgressIndicators { get; set; }
    public string? ReEvaluationSchedule { get; set; }
    public DateTime? NextReEvalDate { get; set; }
    public ChiroTreatmentStatus Status { get; set; }
    public string? DischargeCondition { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string? MaintenancePlan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
