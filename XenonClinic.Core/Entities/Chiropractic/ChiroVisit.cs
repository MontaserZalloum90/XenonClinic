using XenonClinic.Core.Enums.Chiropractic;

namespace XenonClinic.Core.Entities.Chiropractic;

public class ChiroVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public ChiroVisitType VisitType { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? PainLocation { get; set; }
    public int? PainLevel { get; set; }
    public string? PainQuality { get; set; }
    public string? PainRadiation { get; set; }
    public string? AggravatingFactors { get; set; }
    public string? RelievingFactors { get; set; }
    public string? FunctionalLimitations { get; set; }
    public string? PosturalObservations { get; set; }
    public string? GaitObservations { get; set; }
    public string? RangeOfMotion { get; set; }
    public string? Palpation { get; set; }
    public string? OrthopedicTests { get; set; }
    public string? NeurologicExam { get; set; }
    public string? SubluxationsFound { get; set; }
    public string? TreatmentProvided { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public int? VisitsRemaining { get; set; }
    public string? HomeExercises { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public Appointment? Appointment { get; set; }
    public ICollection<ChiroAdjustment> Adjustments { get; set; } = new List<ChiroAdjustment>();
}
