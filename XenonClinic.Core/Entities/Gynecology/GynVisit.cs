using XenonClinic.Core.Enums.Gynecology;

namespace XenonClinic.Core.Entities.Gynecology;

public class GynVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public GynVisitType VisitType { get; set; }
    public string? ChiefComplaint { get; set; }
    public DateTime? LastMenstrualPeriod { get; set; }
    public int? CycleLength { get; set; }
    public int? CycleDuration { get; set; }
    public bool? CycleRegular { get; set; }
    public string? MenstrualHistory { get; set; }
    public int? Gravida { get; set; }
    public int? Para { get; set; }
    public int? Abortions { get; set; }
    public int? LivingChildren { get; set; }
    public string? ObstetricHistory { get; set; }
    public string? ContraceptiveMethod { get; set; }
    public string? SexualHistory { get; set; }
    public string? AbdominalExam { get; set; }
    public string? PelvicExam { get; set; }
    public string? CervixExam { get; set; }
    public string? UterusExam { get; set; }
    public string? AdnexaExam { get; set; }
    public string? BreastExam { get; set; }
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
    public ICollection<GynProcedure> Procedures { get; set; } = new List<GynProcedure>();
    public ICollection<PapSmearRecord> PapSmears { get; set; } = new List<PapSmearRecord>();
    public ICollection<ObUltrasound> Ultrasounds { get; set; } = new List<ObUltrasound>();

    /// <summary>
    /// Alias for PapSmears for compatibility
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public ICollection<PapSmearRecord> PapSmearRecords { get => PapSmears; set => PapSmears = value; }
}
