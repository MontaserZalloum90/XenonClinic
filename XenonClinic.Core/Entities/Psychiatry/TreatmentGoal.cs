using XenonClinic.Core.Enums.Psychiatry;

namespace XenonClinic.Core.Entities.Psychiatry;

public class TreatmentGoal
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime CreatedDate { get; set; }
    public GoalType GoalType { get; set; }
    public string? GoalDescription { get; set; }
    public string? TargetBehavior { get; set; }
    public string? BaselineMeasure { get; set; }
    public string? TargetMeasure { get; set; }
    public DateTime? TargetDate { get; set; }
    public GoalStatus Status { get; set; }
    public int? ProgressPercentage { get; set; }
    public string? Interventions { get; set; }
    public string? BarriersToProgress { get; set; }
    public string? StrategiesUsed { get; set; }
    public string? MilestoneAchieved { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? OutcomeDescription { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
}
