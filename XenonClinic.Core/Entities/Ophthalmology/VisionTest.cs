namespace XenonClinic.Core.Entities.Ophthalmology;

/// <summary>
/// Represents a comprehensive vision test/refraction
/// </summary>
public class VisionTest
{
    public int Id { get; set; }
    public int OphthalmologyVisitId { get; set; }

    public DateTime TestDate { get; set; }

    // Uncorrected Visual Acuity
    /// <summary>
    /// Uncorrected VA - Right eye (e.g., 20/40)
    /// </summary>
    public string? UcvaOd { get; set; }

    /// <summary>
    /// Uncorrected VA - Left eye
    /// </summary>
    public string? UcvaOs { get; set; }

    // Best Corrected Visual Acuity
    /// <summary>
    /// Best corrected VA - Right eye (e.g., 20/20)
    /// </summary>
    public string? BcvaOd { get; set; }

    /// <summary>
    /// Best corrected VA - Left eye
    /// </summary>
    public string? BcvaOs { get; set; }

    // Refraction - Right Eye (OD)
    public decimal? SphereOd { get; set; }
    public decimal? CylinderOd { get; set; }
    public int? AxisOd { get; set; }
    public decimal? AddOd { get; set; }

    // Refraction - Left Eye (OS)
    public decimal? SphereOs { get; set; }
    public decimal? CylinderOs { get; set; }
    public int? AxisOs { get; set; }
    public decimal? AddOs { get; set; }

    /// <summary>
    /// Pupillary distance in mm
    /// </summary>
    public decimal? PupillaryDistance { get; set; }

    /// <summary>
    /// Near vision - Right eye
    /// </summary>
    public string? NearVisionOd { get; set; }

    /// <summary>
    /// Near vision - Left eye
    /// </summary>
    public string? NearVisionOs { get; set; }

    /// <summary>
    /// Color vision test result
    /// </summary>
    public string? ColorVision { get; set; }

    /// <summary>
    /// Contrast sensitivity
    /// </summary>
    public string? ContrastSensitivity { get; set; }

    /// <summary>
    /// Stereopsis (depth perception)
    /// </summary>
    public string? Stereopsis { get; set; }

    public string? PerformedById { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public OphthalmologyVisit? Visit { get; set; }
}
