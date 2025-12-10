namespace XenonClinic.Core.Entities.SleepMedicine;

public class SleepDiary
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime? Bedtime { get; set; }
    public DateTime? LightsOutTime { get; set; }
    public int? SleepLatencyMinutes { get; set; }
    public int? NumberOfAwakenings { get; set; }
    public int? TotalWakeMinutes { get; set; }
    public DateTime? FinalWakeTime { get; set; }
    public DateTime? OutOfBedTime { get; set; }
    public decimal? TotalSleepHours { get; set; }
    public int? SleepQualityRating { get; set; }
    public int? RestorationRating { get; set; }
    public string? MedicationsTaken { get; set; }
    public string? AlcoholCaffeine { get; set; }
    public bool? Exercise { get; set; }
    public string? ExerciseTime { get; set; }
    public int? NapsNumber { get; set; }
    public int? NapsTotalMinutes { get; set; }
    public int? DaytimeSleepinessRating { get; set; }
    public string? MoodRating { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
}
