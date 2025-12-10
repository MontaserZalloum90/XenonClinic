namespace XenonClinic.Core.Entities.Ophthalmology;

/// <summary>
/// Represents an optical prescription (glasses or contact lenses)
/// </summary>
public class EyePrescription
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? OphthalmologyVisitId { get; set; }

    /// <summary>
    /// Prescription type (Glasses, ContactLenses, Both)
    /// </summary>
    public string PrescriptionType { get; set; } = "Glasses";

    public DateTime PrescriptionDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    // Right Eye (OD)
    public decimal? SphereOd { get; set; }
    public decimal? CylinderOd { get; set; }
    public int? AxisOd { get; set; }
    public decimal? AddOd { get; set; }
    public string? PrismOd { get; set; }
    public string? PrismBaseOd { get; set; }

    // Left Eye (OS)
    public decimal? SphereOs { get; set; }
    public decimal? CylinderOs { get; set; }
    public int? AxisOs { get; set; }
    public decimal? AddOs { get; set; }
    public string? PrismOs { get; set; }
    public string? PrismBaseOs { get; set; }

    public decimal? PupillaryDistance { get; set; }
    public decimal? PupillaryDistanceNear { get; set; }

    /// <summary>
    /// Segment height for progressive/bifocal lenses
    /// </summary>
    public decimal? SegmentHeight { get; set; }

    // Contact Lens Specific
    /// <summary>
    /// Base curve - Right eye
    /// </summary>
    public decimal? BaseCurveOd { get; set; }
    public decimal? BaseCurveOs { get; set; }

    /// <summary>
    /// Diameter - Right eye
    /// </summary>
    public decimal? DiameterOd { get; set; }
    public decimal? DiameterOs { get; set; }

    /// <summary>
    /// Contact lens brand/type
    /// </summary>
    public string? ContactLensBrand { get; set; }

    /// <summary>
    /// Replacement schedule (Daily, Weekly, BiWeekly, Monthly)
    /// </summary>
    public string? ReplacementSchedule { get; set; }

    /// <summary>
    /// Lens recommendations (e.g., Progressive, Single Vision, Bifocal)
    /// </summary>
    public string? LensType { get; set; }

    /// <summary>
    /// Lens material recommendations
    /// </summary>
    public string? LensMaterial { get; set; }

    /// <summary>
    /// Lens coatings recommended (e.g., Anti-reflective, Blue light filter, Photochromic)
    /// </summary>
    public string? LensCoatings { get; set; }

    public string? PrescriberId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public OphthalmologyVisit? Visit { get; set; }
}
