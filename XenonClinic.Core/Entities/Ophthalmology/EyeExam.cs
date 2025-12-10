namespace XenonClinic.Core.Entities.Ophthalmology;

/// <summary>
/// Represents a comprehensive eye examination
/// </summary>
public class EyeExam
{
    public int Id { get; set; }
    public int OphthalmologyVisitId { get; set; }

    public DateTime ExamDate { get; set; }

    // External Examination
    /// <summary>
    /// External exam findings - Right eye
    /// </summary>
    public string? ExternalExamOd { get; set; }

    /// <summary>
    /// External exam findings - Left eye
    /// </summary>
    public string? ExternalExamOs { get; set; }

    // Slit Lamp Examination
    /// <summary>
    /// Cornea findings - Right eye
    /// </summary>
    public string? CorneaOd { get; set; }
    public string? CorneaOs { get; set; }

    /// <summary>
    /// Anterior chamber findings - Right eye
    /// </summary>
    public string? AnteriorChamberOd { get; set; }
    public string? AnteriorChamberOs { get; set; }

    /// <summary>
    /// Iris findings - Right eye
    /// </summary>
    public string? IrisOd { get; set; }
    public string? IrisOs { get; set; }

    /// <summary>
    /// Lens findings - Right eye
    /// </summary>
    public string? LensOd { get; set; }
    public string? LensOs { get; set; }

    // Posterior Segment (Dilated Exam)
    /// <summary>
    /// Vitreous findings - Right eye
    /// </summary>
    public string? VitreousOd { get; set; }
    public string? VitreousOs { get; set; }

    /// <summary>
    /// Optic disc findings - Right eye (including cup-to-disc ratio)
    /// </summary>
    public string? OpticDiscOd { get; set; }
    public string? OpticDiscOs { get; set; }

    /// <summary>
    /// Cup-to-disc ratio - Right eye
    /// </summary>
    public decimal? CupToDiscRatioOd { get; set; }
    public decimal? CupToDiscRatioOs { get; set; }

    /// <summary>
    /// Macula findings - Right eye
    /// </summary>
    public string? MaculaOd { get; set; }
    public string? MaculaOs { get; set; }

    /// <summary>
    /// Retinal vessels - Right eye
    /// </summary>
    public string? RetinalVesselsOd { get; set; }
    public string? RetinalVesselsOs { get; set; }

    /// <summary>
    /// Peripheral retina - Right eye
    /// </summary>
    public string? PeripheralRetinaOd { get; set; }
    public string? PeripheralRetinaOs { get; set; }

    /// <summary>
    /// Whether pupil dilation was performed
    /// </summary>
    public bool PupilsDilated { get; set; }

    /// <summary>
    /// Dilating agent used
    /// </summary>
    public string? DilatingAgent { get; set; }

    /// <summary>
    /// Pupil reaction - Right eye
    /// </summary>
    public string? PupilReactionOd { get; set; }
    public string? PupilReactionOs { get; set; }

    /// <summary>
    /// Extraocular motility
    /// </summary>
    public string? ExtraocularMotility { get; set; }

    /// <summary>
    /// Confrontation visual fields
    /// </summary>
    public string? ConfrontationFields { get; set; }

    public string? ExaminerId { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public OphthalmologyVisit? Visit { get; set; }
}
