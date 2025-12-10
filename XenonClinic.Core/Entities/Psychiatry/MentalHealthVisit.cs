using XenonClinic.Core.Enums.Psychiatry;

namespace XenonClinic.Core.Entities.Psychiatry;

public class MentalHealthVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public MentalHealthVisitType VisitType { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? PresentIllness { get; set; }
    public string? MoodDescription { get; set; }
    public string? AffectDescription { get; set; }
    public string? ThoughtContent { get; set; }
    public string? ThoughtProcess { get; set; }
    public bool? SuicidalIdeation { get; set; }
    public string? SuicidalIdeationDetails { get; set; }
    public bool? HomicidalIdeation { get; set; }
    public bool? Hallucinations { get; set; }
    public string? HallucinationType { get; set; }
    public bool? Delusions { get; set; }
    public string? DelusionType { get; set; }
    public string? Insight { get; set; }
    public string? Judgment { get; set; }
    public string? Cognition { get; set; }
    public int? PHQ9Score { get; set; }
    public int? GAD7Score { get; set; }
    public string? OtherScalesUsed { get; set; }
    public string? SleepPattern { get; set; }
    public string? AppetiteChanges { get; set; }
    public string? EnergyLevel { get; set; }
    public string? SubstanceUse { get; set; }
    public string? MentalStatusExam { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? SafetyPlan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public Appointment? Appointment { get; set; }
    public ICollection<TherapySession> TherapySessions { get; set; } = new List<TherapySession>();
}
