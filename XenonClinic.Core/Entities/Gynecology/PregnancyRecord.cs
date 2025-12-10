using XenonClinic.Core.Enums.Gynecology;

namespace XenonClinic.Core.Entities.Gynecology;

public class PregnancyRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public DateTime? LastMenstrualPeriod { get; set; }
    public DateTime EstimatedDueDate { get; set; }
    public DateTime? ConceptionDate { get; set; }
    public bool? IVFConception { get; set; }
    public int? GestationalAgeAtFirstVisit { get; set; }
    public PregnancyStatus Status { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public string? RiskFactors { get; set; }
    public string? MedicalConditions { get; set; }
    public string? ObstetricHistory { get; set; }
    public int? GravidaNumber { get; set; }
    public int? ParaNumber { get; set; }
    public string? BloodType { get; set; }
    public bool? RhPositive { get; set; }
    public bool? RhogamGiven { get; set; }
    public DateTime? RhogamDate { get; set; }
    public bool? GBSStatus { get; set; }
    public DateTime? GBSTestDate { get; set; }
    public string? GeneticScreeningResults { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DeliveryType? DeliveryType { get; set; }
    public string? DeliveryLocation { get; set; }
    public int? GestationalAgeAtDelivery { get; set; }
    public string? DeliveryComplications { get; set; }
    public PregnancyOutcome? Outcome { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public ICollection<PrenatalVisit> PrenatalVisits { get; set; } = new List<PrenatalVisit>();
    public ICollection<ObUltrasound> Ultrasounds { get; set; } = new List<ObUltrasound>();
}
