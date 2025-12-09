namespace XenonClinic.Core.Entities;

/// <summary>
/// Insurance provider/payer.
/// </summary>
public class InsuranceProvider : AuditableEntityWithId
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PayerId { get; set; } // EDI Payer ID
    public InsuranceType Type { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    public string? ClaimsAddress { get; set; }
    public string? ElectronicSubmissionMethod { get; set; } // EDI, Portal, etc.
    public string? PortalUrl { get; set; }

    public int ClaimSubmissionDays { get; set; } = 365; // Filing limit
    public int AppealDays { get; set; } = 60;
    public bool RequiresPreAuthorization { get; set; }
    public bool RequiresReferral { get; set; }

    public bool IsActive { get; set; } = true;
    public int CompanyId { get; set; }

    public virtual ICollection<InsurancePlan> Plans { get; set; } = new List<InsurancePlan>();
    public virtual ICollection<FeeSchedule> FeeSchedules { get; set; } = new List<FeeSchedule>();
}

/// <summary>
/// Insurance plan offered by a provider.
/// </summary>
public class InsurancePlan : AuditableEntityWithId
{
    public int InsuranceProviderId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }

    public decimal? Deductible { get; set; }
    public decimal? OutOfPocketMax { get; set; }
    public decimal? CoPayAmount { get; set; }
    public decimal? CoInsurancePercent { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public virtual InsuranceProvider? InsuranceProvider { get; set; }
}

/// <summary>
/// Patient's insurance policy.
/// </summary>
public class PatientInsurance : AuditableEntityWithId
{
    public int PatientId { get; set; }
    public int InsuranceProviderId { get; set; }
    public int? InsurancePlanId { get; set; }
    public InsurancePriority Priority { get; set; } = InsurancePriority.Primary;

    public string PolicyNumber { get; set; } = string.Empty;
    public string? GroupNumber { get; set; }
    public string SubscriberId { get; set; } = string.Empty;

    public string SubscriberFirstName { get; set; } = string.Empty;
    public string SubscriberLastName { get; set; } = string.Empty;
    public DateTime? SubscriberDateOfBirth { get; set; }
    public string? SubscriberGender { get; set; }
    public PatientRelationship RelationshipToSubscriber { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }

    public decimal? CopayAmount { get; set; }
    public decimal? DeductibleMet { get; set; }
    public decimal? OutOfPocketMet { get; set; }

    public DateTime? EligibilityVerifiedDate { get; set; }
    public string? EligibilityStatus { get; set; }
    public string? EligibilityNotes { get; set; }

    public bool IsActive { get; set; } = true;
    public int CompanyId { get; set; }

    public virtual InsuranceProvider? InsuranceProvider { get; set; }
    public virtual InsurancePlan? InsurancePlan { get; set; }
}

/// <summary>
/// Pre-authorization request.
/// </summary>
public class PreAuthorization : AuditableEntityWithId
{
    public string AuthorizationNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int PatientInsuranceId { get; set; }
    public int? ProviderId { get; set; }

    public AuthorizationType Type { get; set; }
    public AuthorizationStatus Status { get; set; } = AuthorizationStatus.Pending;

    public DateTime RequestDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }

    public string? RequestedProcedureCodes { get; set; } // JSON array
    public string? ApprovedProcedureCodes { get; set; }
    public int? RequestedVisits { get; set; }
    public int? ApprovedVisits { get; set; }
    public int UsedVisits { get; set; }

    public string? DiagnosisCodes { get; set; }
    public string? ClinicalNotes { get; set; }
    public string? DenialReason { get; set; }
    public string? PayerNotes { get; set; }

    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ReferenceNumber { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }
}

/// <summary>
/// Fee schedule for procedures.
/// </summary>
public class FeeSchedule : AuditableEntityWithId
{
    public string Name { get; set; } = string.Empty;
    public int? InsuranceProviderId { get; set; } // Null = default fee schedule
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public int CompanyId { get; set; }

    public virtual InsuranceProvider? InsuranceProvider { get; set; }
    public virtual ICollection<FeeScheduleItem> Items { get; set; } = new List<FeeScheduleItem>();
}

/// <summary>
/// Individual fee in a fee schedule.
/// </summary>
public class FeeScheduleItem : AuditableEntityWithId
{
    public int FeeScheduleId { get; set; }
    public string ProcedureCode { get; set; } = string.Empty;
    public string? Modifier { get; set; }
    public string? Description { get; set; }

    public decimal Fee { get; set; }
    public decimal? AllowedAmount { get; set; }
    public decimal? MedicareRate { get; set; }

    public string? PlaceOfService { get; set; }
    public bool RequiresPreAuth { get; set; }

    public virtual FeeSchedule? FeeSchedule { get; set; }
}

/// <summary>
/// Eligibility verification request/response.
/// </summary>
public class EligibilityVerification : AuditableEntityWithId
{
    public int PatientId { get; set; }
    public int PatientInsuranceId { get; set; }

    public DateTime RequestDate { get; set; }
    public DateTime? ResponseDate { get; set; }
    public EligibilityStatus Status { get; set; }

    public string? RequestTraceNumber { get; set; }
    public string? ResponseTraceNumber { get; set; }

    public bool? IsEligible { get; set; }
    public DateTime? CoverageEffectiveDate { get; set; }
    public DateTime? CoverageTerminationDate { get; set; }

    public string? PlanName { get; set; }
    public string? NetworkStatus { get; set; } // In-Network, Out-of-Network
    public decimal? Deductible { get; set; }
    public decimal? DeductibleMet { get; set; }
    public decimal? OutOfPocketMax { get; set; }
    public decimal? OutOfPocketMet { get; set; }
    public decimal? CoPayAmount { get; set; }
    public decimal? CoInsurancePercent { get; set; }

    public string? BenefitsJson { get; set; } // Full benefits response
    public string? ErrorMessage { get; set; }

    public int CompanyId { get; set; }
}

// Enums
public enum InsuranceType
{
    Commercial = 1,
    Medicare = 2,
    Medicaid = 3,
    Tricare = 4,
    WorkersComp = 5,
    AutoInsurance = 6,
    SelfPay = 7,
    Other = 8
}

public enum PlanType
{
    PPO = 1,
    HMO = 2,
    EPO = 3,
    POS = 4,
    HDHP = 5,
    Medicare = 6,
    Medicaid = 7,
    Other = 8
}

public enum InsurancePriority
{
    Primary = 1,
    Secondary = 2,
    Tertiary = 3
}

public enum PatientRelationship
{
    Self = 1,
    Spouse = 2,
    Child = 3,
    Other = 4
}

public enum AuthorizationType
{
    Inpatient = 1,
    Outpatient = 2,
    Procedure = 3,
    Referral = 4,
    DME = 5,
    Therapy = 6
}

public enum AuthorizationStatus
{
    Pending = 1,
    Approved = 2,
    PartiallyApproved = 3,
    Denied = 4,
    Expired = 5,
    Cancelled = 6
}

public enum EligibilityStatus
{
    Pending = 1,
    Verified = 2,
    NotEligible = 3,
    Error = 4
}
