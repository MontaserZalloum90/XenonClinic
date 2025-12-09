using XenonClinic.Core.Enums.PainManagement;

namespace XenonClinic.Core.Entities.PainManagement;

public class PainVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? PrimaryPainLocation { get; set; }
    public int? CurrentPainLevel { get; set; }
    public int? AveragePainLevel { get; set; }
    public int? WorstPainLevel { get; set; }
    public int? BestPainLevel { get; set; }
    public string? PainQuality { get; set; }
    public string? PainRadiation { get; set; }
    public string? AggravatingFactors { get; set; }
    public string? RelievingFactors { get; set; }
    public string? PainTiming { get; set; }
    public string? FunctionalImpact { get; set; }
    public string? SleepImpact { get; set; }
    public string? MoodImpact { get; set; }
    public string? CurrentMedications { get; set; }
    public string? MedicationEffectiveness { get; set; }
    public string? PreviousTreatments { get; set; }
    public string? PhysicalExam { get; set; }
    public string? NeurologicalExam { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public Appointment? Appointment { get; set; }
    public ICollection<PainProcedure> Procedures { get; set; } = new List<PainProcedure>();
}
