using XenonClinic.Core.Enums.Neurology;

namespace XenonClinic.Core.Entities.Neurology;

public class NeuroVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }
    public bool? Headache { get; set; }
    public string? HeadacheCharacter { get; set; }
    public bool? Seizures { get; set; }
    public string? SeizureDescription { get; set; }
    public bool? Dizziness { get; set; }
    public bool? Vertigo { get; set; }
    public bool? Tremor { get; set; }
    public bool? Weakness { get; set; }
    public string? WeaknessDistribution { get; set; }
    public bool? Numbness { get; set; }
    public string? NumbnessDistribution { get; set; }
    public bool? GaitDisturbance { get; set; }
    public bool? MemoryProblems { get; set; }
    public bool? SpeechDifficulty { get; set; }
    public bool? VisionChanges { get; set; }
    public bool? LossOfConsciousness { get; set; }
    public string? MentalStatusExam { get; set; }
    public string? CranialNervesExam { get; set; }
    public string? MotorExam { get; set; }
    public string? SensoryExam { get; set; }
    public string? ReflexesExam { get; set; }
    public string? CoordinationExam { get; set; }
    public string? GaitExam { get; set; }
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
    public ICollection<NeuroExam> NeuroExams { get; set; } = new List<NeuroExam>();
    public ICollection<EEGRecord> EEGRecords { get; set; } = new List<EEGRecord>();
    public ICollection<NerveStudy> NerveStudies { get; set; } = new List<NerveStudy>();
    public ICollection<NeuroProcedure> Procedures { get; set; } = new List<NeuroProcedure>();
}
