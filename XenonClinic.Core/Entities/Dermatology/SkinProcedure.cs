namespace XenonClinic.Core.Entities.Dermatology;

/// <summary>
/// Represents a dermatological procedure
/// </summary>
public class SkinProcedure
{
    public int Id { get; set; }
    public int DermatologyVisitId { get; set; }
    public int BranchId { get; set; }

    /// <summary>
    /// CPT procedure code
    /// </summary>
    public string? ProcedureCode { get; set; }

    public string ProcedureName { get; set; } = string.Empty;

    /// <summary>
    /// Category (Diagnostic, Therapeutic, Surgical, Cosmetic, Laser)
    /// </summary>
    public string Category { get; set; } = "Therapeutic";

    /// <summary>
    /// Procedure type (Biopsy, Excision, Cryotherapy, Electrosurgery, LaserTreatment, ChemicalPeel, Injection, etc.)
    /// </summary>
    public string? ProcedureType { get; set; }

    /// <summary>
    /// Body site(s) treated
    /// </summary>
    public string? BodySite { get; set; }

    /// <summary>
    /// Status (Scheduled, InProgress, Completed, Cancelled)
    /// </summary>
    public string Status { get; set; } = "Completed";

    public DateTime ProcedureDate { get; set; }

    /// <summary>
    /// Anesthesia type (None, Topical, Local, Regional)
    /// </summary>
    public string? AnesthesiaType { get; set; }

    /// <summary>
    /// Equipment/laser settings used
    /// </summary>
    public string? EquipmentSettings { get; set; }

    /// <summary>
    /// Products/materials used
    /// </summary>
    public string? MaterialsUsed { get; set; }

    public string? Findings { get; set; }
    public string? Complications { get; set; }

    /// <summary>
    /// Post-procedure care instructions
    /// </summary>
    public string? PostProcedureCare { get; set; }

    /// <summary>
    /// Before procedure photo path
    /// </summary>
    public string? BeforePhotoPath { get; set; }

    /// <summary>
    /// After procedure photo path
    /// </summary>
    public string? AfterPhotoPath { get; set; }

    /// <summary>
    /// Specimen sent to pathology
    /// </summary>
    public bool SpecimenSent { get; set; }

    /// <summary>
    /// Pathology report reference
    /// </summary>
    public string? PathologyReference { get; set; }

    public string? PerformedById { get; set; }
    public decimal? Fee { get; set; }
    public string? Notes { get; set; }

    public DermatologyVisit? Visit { get; set; }
    public Branch? Branch { get; set; }
}
