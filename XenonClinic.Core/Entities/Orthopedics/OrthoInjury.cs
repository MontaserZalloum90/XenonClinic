using XenonClinic.Core.Enums.Orthopedics;

namespace XenonClinic.Core.Entities.Orthopedics;

public class OrthoInjury
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? OrthoVisitId { get; set; }
    public InjuryType InjuryType { get; set; }
    public string? InjuryDescription { get; set; }
    public string? ICD10Code { get; set; }
    public BodyRegion BodyRegion { get; set; }
    public string? SpecificLocation { get; set; }
    public Laterality Laterality { get; set; }
    public DateTime InjuryDate { get; set; }
    public string? MechanismOfInjury { get; set; }
    public InjurySeverity Severity { get; set; }
    public InjuryStatus Status { get; set; }
    public bool? IsWorkRelated { get; set; }
    public bool? IsSportsRelated { get; set; }
    public string? SportActivity { get; set; }
    public string? InitialTreatment { get; set; }
    public string? CurrentTreatment { get; set; }
    public int? ExpectedRecoveryWeeks { get; set; }
    public DateTime? RecoveryDate { get; set; }
    public string? Complications { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
    public OrthoVisit? OrthoVisit { get; set; }
}
