namespace XenonClinic.Core.Entities.Ophthalmology;

/// <summary>
/// Represents an ophthalmological procedure
/// </summary>
public class EyeProcedure
{
    public int Id { get; set; }
    public int OphthalmologyVisitId { get; set; }
    public int BranchId { get; set; }

    /// <summary>
    /// CPT procedure code
    /// </summary>
    public string? ProcedureCode { get; set; }

    public string ProcedureName { get; set; } = string.Empty;

    /// <summary>
    /// Category (Diagnostic, Surgical, Laser, Injection, Imaging)
    /// </summary>
    public string Category { get; set; } = "Diagnostic";

    /// <summary>
    /// Eye treated (OD, OS, OU)
    /// </summary>
    public string Eye { get; set; } = "OU";

    /// <summary>
    /// Status (Scheduled, InProgress, Completed, Cancelled)
    /// </summary>
    public string Status { get; set; } = "Completed";

    public DateTime ProcedureDate { get; set; }

    public string? AnesthesiaType { get; set; }
    public string? Technique { get; set; }
    public string? Findings { get; set; }
    public string? Complications { get; set; }
    public string? PostOpInstructions { get; set; }

    public string? PerformedById { get; set; }
    public decimal? Fee { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public OphthalmologyVisit? Visit { get; set; }
    public Branch? Branch { get; set; }
}
