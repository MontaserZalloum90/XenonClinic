namespace XenonClinic.Core.Entities.Psychiatry;

public class MoodRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime RecordTime { get; set; }
    public int MoodRating { get; set; }
    public int? AnxietyRating { get; set; }
    public int? EnergyRating { get; set; }
    public int? IrritabilityRating { get; set; }
    public int? SleepHours { get; set; }
    public string? SleepQuality { get; set; }
    public string? MoodDescription { get; set; }
    public string? Triggers { get; set; }
    public string? CopingStrategiesUsed { get; set; }
    public bool? MedicationTaken { get; set; }
    public string? MedicationMissed { get; set; }
    public string? SideEffectsNoticed { get; set; }
    public string? ActivitiesCompleted { get; set; }
    public string? SocialInteractions { get; set; }
    public bool? ExerciseCompleted { get; set; }
    public string? AppetiteLevel { get; set; }
    public bool? SuicidalThoughts { get; set; }
    public bool? SelfHarmUrges { get; set; }
    public string? PositiveExperiences { get; set; }
    public string? Challenges { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
