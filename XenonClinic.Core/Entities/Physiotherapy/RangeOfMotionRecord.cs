namespace XenonClinic.Core.Entities.Physiotherapy;

/// <summary>
/// Represents range of motion measurements for a joint
/// </summary>
public class RangeOfMotionRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int BranchId { get; set; }
    public int? PhysioAssessmentId { get; set; }
    public DateTime MeasurementDate { get; set; }

    /// <summary>
    /// Joint measured (e.g., Shoulder, Elbow, Wrist, Hip, Knee, Ankle, Cervical Spine, Lumbar Spine)
    /// </summary>
    public string Joint { get; set; } = string.Empty;

    /// <summary>
    /// Side (Left, Right, Bilateral)
    /// </summary>
    public string Side { get; set; } = "Right";

    /// <summary>
    /// Movement type (e.g., Flexion, Extension, Abduction, Adduction, Internal Rotation, External Rotation)
    /// </summary>
    public string Movement { get; set; } = string.Empty;

    /// <summary>
    /// Active range of motion in degrees
    /// </summary>
    public decimal? ActiveRom { get; set; }

    /// <summary>
    /// Passive range of motion in degrees
    /// </summary>
    public decimal? PassiveRom { get; set; }

    /// <summary>
    /// Normal/expected ROM for this movement
    /// </summary>
    public decimal? NormalRom { get; set; }

    /// <summary>
    /// Percentage of normal ROM
    /// </summary>
    public decimal? PercentageOfNormal { get; set; }

    /// <summary>
    /// End feel (Normal, Capsular, Bone-to-Bone, Soft Tissue, Empty, Spasm)
    /// </summary>
    public string? EndFeel { get; set; }

    /// <summary>
    /// Pain during movement (None, Mild, Moderate, Severe)
    /// </summary>
    public string? PainDuringMovement { get; set; }

    /// <summary>
    /// Measurement tool used (Goniometer, Inclinometer, Visual Estimate)
    /// </summary>
    public string? MeasurementMethod { get; set; }

    public string? MeasuredById { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Patient? Patient { get; set; }
    public Branch? Branch { get; set; }
    public PhysioAssessment? Assessment { get; set; }
}
