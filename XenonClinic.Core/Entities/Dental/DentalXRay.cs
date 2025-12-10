namespace XenonClinic.Core.Entities.Dental;

/// <summary>
/// Represents a dental X-ray/radiograph record
/// </summary>
public class DentalXRay
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? DentalVisitId { get; set; }

    /// <summary>
    /// Type of X-ray (Periapical, Bitewing, Panoramic, Cephalometric, CBCT, Occlusal)
    /// </summary>
    public string XRayType { get; set; } = string.Empty;

    /// <summary>
    /// Tooth number(s) or region captured
    /// </summary>
    public string? Region { get; set; }

    public DateTime DateTaken { get; set; }

    /// <summary>
    /// Path to the image file or DICOM reference
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Radiologist or dentist interpretation/findings
    /// </summary>
    public string? Findings { get; set; }

    /// <summary>
    /// Quality assessment (Good, Acceptable, Poor, Retake Required)
    /// </summary>
    public string? QualityAssessment { get; set; }

    public string? TechniquedById { get; set; }
    public string? InterpretedById { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public DentalVisit? DentalVisit { get; set; }
}
