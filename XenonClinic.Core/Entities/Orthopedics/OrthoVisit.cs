using XenonClinic.Core.Enums.Orthopedics;

namespace XenonClinic.Core.Entities.Orthopedics;

public class OrthoVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? HistoryOfPresentIllness { get; set; }
    public string? MechanismOfInjury { get; set; }
    public DateTime? InjuryDate { get; set; }
    public string? PainLocation { get; set; }
    public int? PainScale { get; set; }
    public string? PainCharacter { get; set; }
    public bool? Swelling { get; set; }
    public bool? Bruising { get; set; }
    public bool? Deformity { get; set; }
    public bool? WeightBearing { get; set; }
    public string? RangeOfMotion { get; set; }
    public string? Strength { get; set; }
    public string? Sensation { get; set; }
    public string? Pulses { get; set; }
    public string? SpecialTests { get; set; }
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
    public ICollection<OrthoInjury> Injuries { get; set; } = new List<OrthoInjury>();
    public ICollection<OrthoProcedure> Procedures { get; set; } = new List<OrthoProcedure>();
    public ICollection<OrthoImaging> ImagingStudies { get; set; } = new List<OrthoImaging>();
}
