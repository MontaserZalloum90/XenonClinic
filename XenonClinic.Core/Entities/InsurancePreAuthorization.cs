using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Pre-authorization request for insurance coverage
/// </summary>
public class InsurancePreAuthorization : IBranchEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID for multi-tenant data isolation
    /// </summary>
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    /// <summary>
    /// Unique pre-auth request number
    /// </summary>
    public string PreAuthNumber { get; set; } = string.Empty;

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
    /// Current status
    /// </summary>
    public PreAuthStatus Status { get; set; } = PreAuthStatus.Draft;

    /// <summary>
    /// Pre-auth type (Elective, Urgent, Emergency)
    /// </summary>
    public string RequestType { get; set; } = "Elective";

    /// <summary>
    /// Service category
    /// </summary>
    public string ServiceCategory { get; set; } = string.Empty;

    /// <summary>
    /// Requested procedure codes (JSON array)
    /// </summary>
    public string RequestedProcedures { get; set; } = "[]";

    /// <summary>
    /// Primary diagnosis code (ICD-10)
    /// </summary>
    public string? PrimaryDiagnosisCode { get; set; }

    /// <summary>
    /// Primary diagnosis description
    /// </summary>
    public string? PrimaryDiagnosisDescription { get; set; }

    /// <summary>
    /// Secondary diagnosis codes (JSON array)
    /// </summary>
    public string? SecondaryDiagnosisCodes { get; set; }

    /// <summary>
    /// Clinical justification/notes
    /// </summary>
    public string? ClinicalJustification { get; set; }

    /// <summary>
    /// Requesting provider name
    /// </summary>
    public string? RequestingProviderName { get; set; }

    /// <summary>
    /// Requesting provider NPI/License
    /// </summary>
    public string? RequestingProviderNpi { get; set; }

    /// <summary>
    /// Planned service date
    /// </summary>
    public DateTime? PlannedServiceDate { get; set; }

    /// <summary>
    /// Requested service duration (days)
    /// </summary>
    public int? RequestedDays { get; set; }

    /// <summary>
    /// Requested service quantity/units
    /// </summary>
    public int? RequestedUnits { get; set; }

    /// <summary>
    /// Estimated cost
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// Date submitted to insurance
    /// </summary>
    public DateTime? SubmissionDate { get; set; }

    /// <summary>
    /// Insurance authorization number (from payer)
    /// </summary>
    public string? AuthorizationNumber { get; set; }

    /// <summary>
    /// Approved amount
    /// </summary>
    public decimal ApprovedAmount { get; set; }

    /// <summary>
    /// Approved units
    /// </summary>
    public int? ApprovedUnits { get; set; }

    /// <summary>
    /// Approved days
    /// </summary>
    public int? ApprovedDays { get; set; }

    /// <summary>
    /// Approval date
    /// </summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>
    /// Authorization effective date
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Authorization expiration date
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

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
    /// Attached documents (JSON array of file paths)
    /// </summary>
    public string? AttachedDocuments { get; set; }

    /// <summary>
    /// Follow-up date
    /// </summary>
    public DateTime? FollowUpDate { get; set; }

    /// <summary>
    /// Assigned to (staff member)
    /// </summary>
    public string? AssignedTo { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
