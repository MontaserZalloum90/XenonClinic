using XenonClinic.Core.Enums.Cardiology;

namespace XenonClinic.Core.Entities.Cardiology;

public class StressTest
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? CardioVisitId { get; set; }
    public DateTime TestDate { get; set; }
    public StressTestType TestType { get; set; }
    public string? Protocol { get; set; }
    public int? BaselineHeartRate { get; set; }
    public int? BaselineSystolicBP { get; set; }
    public int? BaselineDiastolicBP { get; set; }
    public int? PeakHeartRate { get; set; }
    public int? PeakSystolicBP { get; set; }
    public int? PeakDiastolicBP { get; set; }
    public int? TargetHeartRate { get; set; }
    public decimal? PercentTargetAchieved { get; set; }
    public decimal? ExerciseDuration { get; set; }
    public decimal? METs { get; set; }
    public string? ReasonForStopping { get; set; }
    public bool? ChestPainDuringTest { get; set; }
    public bool? DyspneaDuringTest { get; set; }
    public bool? STChanges { get; set; }
    public string? STChangeDetails { get; set; }
    public bool? Arrhythmias { get; set; }
    public string? ArrhythmiaDetails { get; set; }
    public bool? HypotensiveResponse { get; set; }
    public StressTestResult Result { get; set; }
    public string? Interpretation { get; set; }
    public string? Recommendations { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public CardioVisit? CardioVisit { get; set; }
}
