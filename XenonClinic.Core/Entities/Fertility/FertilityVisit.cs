using XenonClinic.Core.Enums.Fertility;

namespace XenonClinic.Core.Entities.Fertility;

public class FertilityVisit
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? PartnerId { get; set; }
    public int? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; }
    public FertilityVisitType VisitType { get; set; }
    public string? ChiefComplaint { get; set; }
    public int? DurationTryingMonths { get; set; }
    public string? MenstrualHistory { get; set; }
    public int? CycleLength { get; set; }
    public bool? CycleRegular { get; set; }
    public int? Gravida { get; set; }
    public int? Para { get; set; }
    public string? ObstetricHistory { get; set; }
    public string? PreviousTreatments { get; set; }
    public string? MaleFactorHistory { get; set; }
    public string? SexualHistory { get; set; }
    public string? MedicalHistory { get; set; }
    public string? SurgicalHistory { get; set; }
    public string? FamilyHistory { get; set; }
    public string? LifestyleFactors { get; set; }
    public string? PhysicalExam { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public Patient? Partner { get; set; }
    public Appointment? Appointment { get; set; }
}
