using XenonClinic.Core.Enums.SleepMedicine;

namespace XenonClinic.Core.Entities.SleepMedicine;

public class SleepVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public bool? ExcessiveDaytimeSleepiness { get; set; }
    public int? EpworthScore { get; set; }
    public bool? Snoring { get; set; }
    public bool? WitnessedApneas { get; set; }
    public bool? Insomnia { get; set; }
    public string? InsomniaType { get; set; }
    public bool? RestlessLegs { get; set; }
    public bool? Nightmares { get; set; }
    public bool? Sleepwalking { get; set; }
    public bool? SleepTalking { get; set; }
    public decimal? TypicalBedtime { get; set; }
    public decimal? TypicalWakeTime { get; set; }
    public decimal? TotalSleepTime { get; set; }
    public int? SleepLatency { get; set; }
    public int? NumberOfAwakenings { get; set; }
    public string? SleepQuality { get; set; }
    public string? NapPattern { get; set; }
    public decimal? CaffeineIntake { get; set; }
    public bool? AlcoholUse { get; set; }
    public string? WorkSchedule { get; set; }
    public string? SleepEnvironment { get; set; }
    public string? CurrentSleepMedications { get; set; }
    public string? PhysicalExam { get; set; }
    public int? NeckCircumference { get; set; }
    public int? MallampatiScore { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public Appointment? Appointment { get; set; }
    public ICollection<SleepStudy> SleepStudies { get; set; } = new List<SleepStudy>();
}
