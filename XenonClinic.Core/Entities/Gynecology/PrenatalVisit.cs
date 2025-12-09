namespace XenonClinic.Core.Entities.Gynecology;

public class PrenatalVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int PregnancyRecordId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public int GestationalWeeks { get; set; }
    public int? GestationalDays { get; set; }
    public decimal? Weight { get; set; }
    public int? SystolicBP { get; set; }
    public int? DiastolicBP { get; set; }
    public decimal? FundalHeight { get; set; }
    public string? FetalPresentation { get; set; }
    public int? FetalHeartRate { get; set; }
    public string? FetalMovement { get; set; }
    public bool? Edema { get; set; }
    public string? EdemaLocation { get; set; }
    public bool? Proteinuria { get; set; }
    public bool? Glucosuria { get; set; }
    public string? UrineResults { get; set; }
    public string? Symptoms { get; set; }
    public bool? VaginalBleeding { get; set; }
    public bool? LeakingFluid { get; set; }
    public bool? Contractions { get; set; }
    public decimal? CervicalDilation { get; set; }
    public decimal? CervicalEffacement { get; set; }
    public string? CervicalExam { get; set; }
    public string? LabsOrdered { get; set; }
    public string? LabResults { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public DateTime? NextVisitDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public PregnancyRecord? PregnancyRecord { get; set; }
    public Appointment? Appointment { get; set; }
}
