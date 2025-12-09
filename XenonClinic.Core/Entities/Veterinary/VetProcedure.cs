namespace XenonClinic.Core.Entities.Veterinary;

/// <summary>
/// Represents a veterinary procedure performed during a visit
/// </summary>
public class VetProcedure
{
    public int Id { get; set; }
    public int VetVisitId { get; set; }

    /// <summary>
    /// Procedure category (Diagnostic, Surgical, Dental, Laboratory, Imaging, Therapeutic)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    public string ProcedureName { get; set; } = string.Empty;
    public string? ProcedureCode { get; set; }
    public string? Description { get; set; }

    /// <summary>
    /// Status (Scheduled, InProgress, Completed, Cancelled)
    /// </summary>
    public string Status { get; set; } = "Completed";

    public DateTime ProcedureDate { get; set; }
    public string? AnesthesiaType { get; set; }
    public string? Findings { get; set; }
    public string? Complications { get; set; }
    public decimal? Fee { get; set; }
    public string? Notes { get; set; }

    public VetVisit? VetVisit { get; set; }
}
