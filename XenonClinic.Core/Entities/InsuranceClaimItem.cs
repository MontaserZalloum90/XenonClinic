using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Line item in an insurance claim
/// </summary>
public class InsuranceClaimItem : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// Reference to the parent claim
    /// </summary>
    public int ClaimId { get; set; }
    public InsuranceClaim Claim { get; set; } = null!;

    /// <summary>
    /// Line number
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// CPT/HCPCS procedure code
    /// </summary>
    public string ProcedureCode { get; set; } = string.Empty;

    /// <summary>
    /// Procedure code description
    /// </summary>
    public string? ProcedureDescription { get; set; }

    /// <summary>
    /// Modifier codes (comma-separated)
    /// </summary>
    public string? Modifiers { get; set; }

    /// <summary>
    /// Diagnosis code pointers (e.g., "1,2")
    /// </summary>
    public string? DiagnosisPointers { get; set; }

    /// <summary>
    /// Revenue code (for institutional claims)
    /// </summary>
    public string? RevenueCode { get; set; }

    /// <summary>
    /// National Drug Code (for pharmacy claims)
    /// </summary>
    public string? NdcCode { get; set; }

    /// <summary>
    /// Service date
    /// </summary>
    public DateTime ServiceDate { get; set; }

    /// <summary>
    /// Units/Quantity
    /// </summary>
    public decimal Units { get; set; } = 1;

    /// <summary>
    /// Unit type (UN = Units, MJ = Minutes, etc.)
    /// </summary>
    public string UnitType { get; set; } = "UN";

    /// <summary>
    /// Billed amount per unit
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Total billed amount
    /// </summary>
    public decimal BilledAmount { get; set; }

    /// <summary>
    /// Allowed/Approved amount
    /// </summary>
    public decimal AllowedAmount { get; set; }

    /// <summary>
    /// Paid amount
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Patient responsibility
    /// </summary>
    public decimal PatientResponsibility { get; set; }

    /// <summary>
    /// Adjustment amount
    /// </summary>
    public decimal AdjustmentAmount { get; set; }

    /// <summary>
    /// Adjustment reason code
    /// </summary>
    public string? AdjustmentReasonCode { get; set; }

    /// <summary>
    /// Denial reason code
    /// </summary>
    public string? DenialReasonCode { get; set; }

    /// <summary>
    /// Remark codes (JSON array)
    /// </summary>
    public string? RemarkCodes { get; set; }

    /// <summary>
    /// Notes
    /// </summary>
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
