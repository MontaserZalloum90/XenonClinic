using XenonClinic.Core.Enums.SleepMedicine;

namespace XenonClinic.Core.Entities.SleepMedicine;

public class SleepStudy
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? SleepVisitId { get; set; }
    public DateTime StudyDate { get; set; }
    public SleepStudyType StudyType { get; set; }
    public string? StudyLocation { get; set; }
    public decimal? TotalRecordingTime { get; set; }
    public decimal? TotalSleepTime { get; set; }
    public decimal? SleepEfficiency { get; set; }
    public decimal? SleepLatency { get; set; }
    public decimal? REMLatency { get; set; }
    public decimal? WakeAfterSleepOnset { get; set; }
    public decimal? PercentStageN1 { get; set; }
    public decimal? PercentStageN2 { get; set; }
    public decimal? PercentStageN3 { get; set; }
    public decimal? PercentStageREM { get; set; }
    public decimal? AHI { get; set; }
    public decimal? AHISupine { get; set; }
    public decimal? AHINonSupine { get; set; }
    public decimal? RDI { get; set; }
    public int? TotalApneas { get; set; }
    public int? ObstructiveApneas { get; set; }
    public int? CentralApneas { get; set; }
    public int? MixedApneas { get; set; }
    public int? TotalHypopneas { get; set; }
    public decimal? LowestSpO2 { get; set; }
    public decimal? MeanSpO2 { get; set; }
    public decimal? TimeBelow90Percent { get; set; }
    public decimal? PeriodicLimbMovementIndex { get; set; }
    public int? ArousaIndex { get; set; }
    public string? HeartRhythmFindings { get; set; }
    public SleepApneaSeverity? ApneaSeverity { get; set; }
    public string? TitrationResults { get; set; }
    public decimal? OptimalCPAPPressure { get; set; }
    public decimal? OptimalBiPAPIPAP { get; set; }
    public decimal? OptimalBiPAPEPAP { get; set; }
    public string? Interpretation { get; set; }
    public string? Recommendations { get; set; }
    public string? FilePath { get; set; }
    public string? InterpretingPhysician { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public SleepVisit? SleepVisit { get; set; }
}
