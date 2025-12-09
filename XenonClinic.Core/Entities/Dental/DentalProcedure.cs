namespace XenonClinic.Core.Entities.Dental;

/// <summary>
/// Represents a dental procedure performed during a visit
/// </summary>
public class DentalProcedure
{
    public int Id { get; set; }
    public int DentalVisitId { get; set; }
    public int BranchId { get; set; }

    /// <summary>
    /// CDT (Current Dental Terminology) procedure code
    /// </summary>
    public string? ProcedureCode { get; set; }

    /// <summary>
    /// Name of the procedure (e.g., Root Canal, Filling, Extraction, Crown)
    /// </summary>
    public string ProcedureName { get; set; } = string.Empty;

    /// <summary>
    /// Procedure category (e.g., Preventive, Restorative, Endodontic, Periodontic, Prosthodontic, Oral Surgery, Orthodontic)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Tooth number(s) involved (comma-separated for multiple teeth)
    /// </summary>
    public string? ToothNumbers { get; set; }

    /// <summary>
    /// Surfaces treated (e.g., MOD, DO, B)
    /// </summary>
    public string? Surfaces { get; set; }

    /// <summary>
    /// Status of the procedure (Planned, InProgress, Completed, Cancelled)
    /// </summary>
    public string Status { get; set; } = "Completed";

    public DateTime ProcedureDate { get; set; }
    public string? ProviderId { get; set; }
    public decimal? Fee { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Materials used (e.g., Composite resin, Amalgam, Ceramic)
    /// </summary>
    public string? MaterialsUsed { get; set; }

    /// <summary>
    /// Anesthesia type used (e.g., Local, Topical, Sedation, General)
    /// </summary>
    public string? AnesthesiaType { get; set; }

    public DentalVisit? DentalVisit { get; set; }
    public Branch? Branch { get; set; }
}
