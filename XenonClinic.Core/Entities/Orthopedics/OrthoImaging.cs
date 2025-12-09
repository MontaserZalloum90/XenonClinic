using XenonClinic.Core.Enums.Orthopedics;

namespace XenonClinic.Core.Entities.Orthopedics;

public class OrthoImaging
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int? OrthoVisitId { get; set; }
    public DateTime StudyDate { get; set; }
    public OrthoImagingType ImagingType { get; set; }
    public string? StudyDescription { get; set; }
    public BodyRegion BodyRegion { get; set; }
    public string? SpecificLocation { get; set; }
    public Laterality Laterality { get; set; }
    public string? Views { get; set; }
    public string? Findings { get; set; }
    public bool? FracturePresent { get; set; }
    public string? FractureType { get; set; }
    public string? FractureLocation { get; set; }
    public bool? Displacement { get; set; }
    public string? DisplacementDetails { get; set; }
    public bool? Arthritis { get; set; }
    public string? ArthritisGrade { get; set; }
    public bool? SoftTissueAbnormality { get; set; }
    public string? SoftTissueFindings { get; set; }
    public string? Impression { get; set; }
    public string? Recommendations { get; set; }
    public string? FilePath { get; set; }
    public string? Radiologist { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }
    public OrthoVisit? OrthoVisit { get; set; }
}
