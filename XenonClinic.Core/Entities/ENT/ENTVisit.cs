using XenonClinic.Core.Enums.ENT;

namespace XenonClinic.Core.Entities.ENT;

public class ENTVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }
    public string? EarExamRight { get; set; }
    public string? EarExamLeft { get; set; }
    public string? TympanicMembraneRight { get; set; }
    public string? TympanicMembraneLeft { get; set; }
    public string? NasalExam { get; set; }
    public string? SeptumExam { get; set; }
    public string? TurbinatesExam { get; set; }
    public string? OropharynxExam { get; set; }
    public string? TonsilsExam { get; set; }
    public string? LarynxExam { get; set; }
    public string? VocalCordsExam { get; set; }
    public string? NeckExam { get; set; }
    public string? LymphNodesExam { get; set; }
    public string? ThyroidExam { get; set; }
    public string? SalivaryGlandsExam { get; set; }
    public bool? Vertigo { get; set; }
    public bool? Tinnitus { get; set; }
    public bool? HearingLoss { get; set; }
    public bool? NasalObstruction { get; set; }
    public bool? PostnasalDrip { get; set; }
    public bool? Snoring { get; set; }
    public bool? SleepApnea { get; set; }
    public bool? Hoarseness { get; set; }
    public bool? Dysphagia { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public Appointment? Appointment { get; set; }
    public ICollection<HearingScreening> HearingScreenings { get; set; } = new List<HearingScreening>();
    public ICollection<SinusAssessment> SinusAssessments { get; set; } = new List<SinusAssessment>();
    public ICollection<ENTProcedure> Procedures { get; set; } = new List<ENTProcedure>();
}
