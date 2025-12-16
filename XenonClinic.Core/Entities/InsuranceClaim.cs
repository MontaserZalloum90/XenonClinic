using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents an insurance claim submission
/// </summary>
public class InsuranceClaim : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// Unique claim number
    /// </summary>
    public string ClaimNumber { get; set; } = string.Empty;

    /// <summary>
    /// Reference to patient's insurance
    /// </summary>
    public int PatientInsuranceId { get; set; }
    public PatientInsurance PatientInsurance { get; set; } = null!;

    /// <summary>
    /// Reference to the patient
    /// </summary>
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Reference to the invoice (if any)
    /// </summary>
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    /// <summary>
    /// Reference to pre-authorization (if any)
    /// </summary>
    public int? PreAuthorizationId { get; set; }
    public InsurancePreAuthorization? PreAuthorization { get; set; }

    /// <summary>
    /// Current claim status
    /// </summary>
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;

    /// <summary>
    /// Claim type (Professional, Institutional, Dental, Pharmacy)
    /// </summary>
    public string ClaimType { get; set; } = "Professional";

    /// <summary>
    /// Date of service (for single-day services)
    /// </summary>
    public DateTime? ServiceDate { get; set; }

    /// <summary>
    /// Date of service (start)
    /// </summary>
    public DateTime ServiceDateFrom { get; set; }

    /// <summary>
    /// Date of service (end)
    /// </summary>
    public DateTime ServiceDateTo { get; set; }

    /// <summary>
    /// Submission date
    /// </summary>
    public DateTime? SubmissionDate { get; set; }

    /// <summary>
    /// Total billed amount
    /// </summary>
    public decimal TotalBilledAmount { get; set; }

    /// <summary>
    /// Amount approved by insurance
    /// </summary>
    public decimal ApprovedAmount { get; set; }

    /// <summary>
    /// Patient responsibility (copay + deductible + coinsurance)
    /// </summary>
    public decimal PatientResponsibility { get; set; }

    /// <summary>
    /// Copay amount
    /// </summary>
    public decimal CopayAmount { get; set; }

    /// <summary>
    /// Deductible amount applied
    /// </summary>
    public decimal DeductibleAmount { get; set; }

    /// <summary>
    /// Coinsurance amount
    /// </summary>
    public decimal CoinsuranceAmount { get; set; }

    /// <summary>
    /// Adjustment amount (write-offs)
    /// </summary>
    public decimal AdjustmentAmount { get; set; }

    /// <summary>
    /// Amount paid by insurance
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    /// Insurance payment date
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Insurance payment reference/check number
    /// </summary>
    public string? PaymentReference { get; set; }

    /// <summary>
    /// Rendering provider (doctor) name
    /// </summary>
    public string? RenderingProviderName { get; set; }

    /// <summary>
    /// Rendering provider NPI/License
    /// </summary>
    public string? RenderingProviderNpi { get; set; }

    /// <summary>
    /// Facility name
    /// </summary>
    public string? FacilityName { get; set; }

    /// <summary>
    /// Place of service code
    /// </summary>
    public string PlaceOfServiceCode { get; set; } = "11"; // Office

    /// <summary>
    /// Primary diagnosis code (ICD-10)
    /// </summary>
    public string? PrimaryDiagnosisCode { get; set; }

    /// <summary>
    /// Secondary diagnosis codes (JSON array)
    /// </summary>
    public string? SecondaryDiagnosisCodes { get; set; }

    /// <summary>
    /// Denial reason code
    /// </summary>
    public string? DenialReasonCode { get; set; }

    /// <summary>
    /// Denial reason description
    /// </summary>
    public string? DenialReason { get; set; }

    /// <summary>
    /// Insurance response/remarks
    /// </summary>
    public string? InsuranceRemarks { get; set; }

    /// <summary>
    /// Internal notes
    /// </summary>
    public string? InternalNotes { get; set; }

    /// <summary>
    /// EDI claim ID (837)
    /// </summary>
    public string? EdiClaimId { get; set; }

    /// <summary>
    /// Payer claim control number
    /// </summary>
    public string? PayerClaimControlNumber { get; set; }

    /// <summary>
    /// Original claim reference (for resubmissions)
    /// </summary>
    public int? OriginalClaimId { get; set; }

    /// <summary>
    /// Resubmission count
    /// </summary>
    public int ResubmissionCount { get; set; }

    /// <summary>
    /// Filing deadline
    /// </summary>
    public DateTime? FilingDeadline { get; set; }

    // Navigation
    public ICollection<InsuranceClaimItem> Items { get; set; } = new List<InsuranceClaimItem>();

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
