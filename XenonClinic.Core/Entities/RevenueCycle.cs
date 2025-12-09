namespace XenonClinic.Core.Entities;

/// <summary>
/// Insurance claim for billing and reimbursement.
/// </summary>
public class InsuranceClaim : AuditableEntityWithId
{
    public string ClaimNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int? InsurancePolicyId { get; set; }
    public int? EncounterId { get; set; }
    public ClaimType ClaimType { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;

    public DateTime ServiceDate { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? ProcessedDate { get; set; }

    public decimal TotalCharges { get; set; }
    public decimal AllowedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PatientResponsibility { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public decimal WriteOffAmount { get; set; }

    public string? PrimaryDiagnosisCode { get; set; }
    public string? SecondaryDiagnosisCodes { get; set; } // JSON array
    public string? AuthorizationNumber { get; set; }
    public string? ReferralNumber { get; set; }

    public string? DenialReason { get; set; }
    public string? DenialCode { get; set; }
    public string? AppealNotes { get; set; }
    public int AppealCount { get; set; }

    public string? RemittanceAdviceNumber { get; set; }
    public string? PayerClaimNumber { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    // Navigation
    public virtual ICollection<ClaimLine> ClaimLines { get; set; } = new List<ClaimLine>();
    public virtual ICollection<ClaimNote> ClaimNotes { get; set; } = new List<ClaimNote>();
    public virtual ICollection<ClaimPayment> ClaimPayments { get; set; } = new List<ClaimPayment>();
}

/// <summary>
/// Individual line item on a claim.
/// </summary>
public class ClaimLine : AuditableEntityWithId
{
    public int ClaimId { get; set; }
    public int LineNumber { get; set; }

    public string ProcedureCode { get; set; } = string.Empty; // CPT code
    public string? Modifier1 { get; set; }
    public string? Modifier2 { get; set; }
    public string? Modifier3 { get; set; }
    public string? Modifier4 { get; set; }

    public string DiagnosisPointer { get; set; } = "1"; // e.g., "1,2"
    public string? RevenueCode { get; set; }
    public string? NationalDrugCode { get; set; }

    public decimal Units { get; set; } = 1;
    public decimal ChargeAmount { get; set; }
    public decimal AllowedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }

    public string? DenialReason { get; set; }
    public DateTime ServiceDateFrom { get; set; }
    public DateTime? ServiceDateTo { get; set; }

    public int? ProviderId { get; set; }
    public int? RenderingProviderId { get; set; }
    public string? PlaceOfService { get; set; }

    // Navigation
    public virtual InsuranceClaim? Claim { get; set; }
}

/// <summary>
/// Notes attached to a claim.
/// </summary>
public class ClaimNote : AuditableEntityWithId
{
    public int ClaimId { get; set; }
    public string NoteType { get; set; } = "General"; // General, Appeal, Denial, Follow-up
    public string Content { get; set; } = string.Empty;
    public string? AddedBy { get; set; }

    public virtual InsuranceClaim? Claim { get; set; }
}

/// <summary>
/// Payment received for a claim.
/// </summary>
public class ClaimPayment : AuditableEntityWithId
{
    public int ClaimId { get; set; }
    public PaymentSource Source { get; set; }
    public string? CheckNumber { get; set; }
    public string? EftNumber { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime PostedDate { get; set; }
    public decimal Amount { get; set; }
    public string? RemittanceAdviceId { get; set; }

    public virtual InsuranceClaim? Claim { get; set; }
}

/// <summary>
/// Patient billing statement.
/// </summary>
public class PatientStatement : AuditableEntityWithId
{
    public string StatementNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public DateTime StatementDate { get; set; }
    public DateTime DueDate { get; set; }

    public decimal PreviousBalance { get; set; }
    public decimal NewCharges { get; set; }
    public decimal Payments { get; set; }
    public decimal Adjustments { get; set; }
    public decimal CurrentBalance { get; set; }

    public StatementStatus Status { get; set; } = StatementStatus.Generated;
    public DateTime? SentDate { get; set; }
    public string? DeliveryMethod { get; set; } // Email, Print, Portal

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual ICollection<StatementLine> StatementLines { get; set; } = new List<StatementLine>();
}

/// <summary>
/// Line item on patient statement.
/// </summary>
public class StatementLine : AuditableEntityWithId
{
    public int StatementId { get; set; }
    public DateTime ServiceDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ProcedureCode { get; set; }
    public decimal Charges { get; set; }
    public decimal InsurancePaid { get; set; }
    public decimal Adjustments { get; set; }
    public decimal PatientPaid { get; set; }
    public decimal Balance { get; set; }
    public int? ClaimId { get; set; }

    public virtual PatientStatement? Statement { get; set; }
}

/// <summary>
/// Payment plan for patient balance.
/// </summary>
public class PaymentPlan : AuditableEntityWithId
{
    public string PlanNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DownPayment { get; set; }
    public decimal RemainingBalance { get; set; }
    public int NumberOfPayments { get; set; }
    public decimal MonthlyPayment { get; set; }
    public int PaymentDayOfMonth { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PaymentPlanStatus Status { get; set; } = PaymentPlanStatus.Active;
    public string? Notes { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual ICollection<PaymentPlanPayment> Payments { get; set; } = new List<PaymentPlanPayment>();
}

/// <summary>
/// Payment made on a payment plan.
/// </summary>
public class PaymentPlanPayment : AuditableEntityWithId
{
    public int PaymentPlanId { get; set; }
    public int PaymentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public PaymentPlanPaymentStatus Status { get; set; }

    public virtual PaymentPlan? PaymentPlan { get; set; }
}

// Enums
public enum ClaimType
{
    Professional = 1,   // CMS-1500
    Institutional = 2,  // UB-04
    Dental = 3,
    Vision = 4
}

public enum ClaimStatus
{
    Draft = 1,
    Ready = 2,
    Submitted = 3,
    Acknowledged = 4,
    InProcess = 5,
    Paid = 6,
    PartiallyPaid = 7,
    Denied = 8,
    Appealed = 9,
    Closed = 10,
    Voided = 11
}

public enum PaymentSource
{
    Insurance = 1,
    Patient = 2,
    Guarantee = 3,
    Collection = 4,
    Other = 5
}

public enum StatementStatus
{
    Generated = 1,
    Sent = 2,
    Delivered = 3,
    Paid = 4,
    PastDue = 5,
    Collections = 6
}

public enum PaymentPlanStatus
{
    Pending = 1,
    Active = 2,
    Completed = 3,
    Defaulted = 4,
    Cancelled = 5
}

public enum PaymentPlanPaymentStatus
{
    Scheduled = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Missed = 4,
    Waived = 5
}
