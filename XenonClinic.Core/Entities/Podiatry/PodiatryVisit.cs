using XenonClinic.Core.Enums.Podiatry;

namespace XenonClinic.Core.Entities.Podiatry;

public class PodiatryVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public PodiatryVisitType VisitType { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? PainLocation { get; set; }
    public int? PainLevel { get; set; }
    public string? FootwearHistory { get; set; }
    public string? ActivityLevel { get; set; }
    public bool? DiabetesPresent { get; set; }
    public bool? PeripheralNeuropathy { get; set; }
    public bool? PVD { get; set; }
    public string? VascularExamRight { get; set; }
    public string? VascularExamLeft { get; set; }
    public string? DorsalisPedisRight { get; set; }
    public string? DorsalisPedisLeft { get; set; }
    public string? PosteriorTibialRight { get; set; }
    public string? PosteriorTibialLeft { get; set; }
    public string? SensoryExamRight { get; set; }
    public string? SensoryExamLeft { get; set; }
    public string? MonofilamentTestRight { get; set; }
    public string? MonofilamentTestLeft { get; set; }
    public string? DermatologicExam { get; set; }
    public string? NailExam { get; set; }
    public string? MusculoskeletalExam { get; set; }
    public string? BiomechanicalExam { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public Appointment? Appointment { get; set; }
    public ICollection<PodiatryProcedure> Procedures { get; set; } = new List<PodiatryProcedure>();
}
