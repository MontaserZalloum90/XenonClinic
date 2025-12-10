using XenonClinic.Core.Enums.Orthopedics;

namespace XenonClinic.Core.Entities.Orthopedics;

public class JointAssessment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? OrthoVisitId { get; set; }
    public DateTime AssessmentDate { get; set; }
    public JointType JointType { get; set; }
    public Laterality Laterality { get; set; }
    public decimal? FlexionDegrees { get; set; }
    public decimal? ExtensionDegrees { get; set; }
    public decimal? AbductionDegrees { get; set; }
    public decimal? AdductionDegrees { get; set; }
    public decimal? InternalRotationDegrees { get; set; }
    public decimal? ExternalRotationDegrees { get; set; }
    public decimal? PronationDegrees { get; set; }
    public decimal? SupinationDegrees { get; set; }
    public int? StrengthFlexion { get; set; }
    public int? StrengthExtension { get; set; }
    public bool? Instability { get; set; }
    public string? InstabilityDirection { get; set; }
    public bool? Crepitus { get; set; }
    public bool? Effusion { get; set; }
    public bool? Warmth { get; set; }
    public bool? Tenderness { get; set; }
    public string? TendernessLocation { get; set; }
    public string? SpecialTests { get; set; }
    public string? SpecialTestResults { get; set; }
    public string? Assessment { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public OrthoVisit? OrthoVisit { get; set; }
}
