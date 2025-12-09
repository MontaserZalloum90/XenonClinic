using XenonClinic.Core.Enums.Oncology;

namespace XenonClinic.Core.Entities.Oncology;

public class OncologyVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public OncologyVisitType VisitType { get; set; }
    public string? ChiefComplaint { get; set; }
    public int? ECOGStatus { get; set; }
    public int? KarnofskyScore { get; set; }
    public decimal? Weight { get; set; }
    public decimal? WeightChange { get; set; }
    public string? CurrentSymptoms { get; set; }
    public int? PainLevel { get; set; }
    public bool? Fatigue { get; set; }
    public bool? Nausea { get; set; }
    public bool? Appetite { get; set; }
    public string? TreatmentTolerance { get; set; }
    public string? SideEffects { get; set; }
    public string? PhysicalExam { get; set; }
    public string? TumorAssessment { get; set; }
    public string? LabReview { get; set; }
    public string? ImagingReview { get; set; }
    public string? TreatmentResponse { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? ClinicalTrialStatus { get; set; }
    public string? PalliativeCareConsult { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public Appointment? Appointment { get; set; }
    public ICollection<ChemotherapySession> ChemotherapySessions { get; set; } = new List<ChemotherapySession>();
    public ICollection<RadiationRecord> RadiationRecords { get; set; } = new List<RadiationRecord>();
}
