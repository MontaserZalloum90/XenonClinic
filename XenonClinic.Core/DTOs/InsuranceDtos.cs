using XenonClinic.Core.Enums;

namespace XenonClinic.Core.DTOs;

#region Insurance Provider DTOs

/// <summary>
/// DTO for insurance provider
/// </summary>
public class InsuranceProviderDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string ProviderCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? ShortName { get; set; }
    public string InsuranceType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int PaymentTermsDays { get; set; }
    public decimal DefaultDiscountPercent { get; set; }
    public int PlansCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating insurance provider
/// </summary>
public class CreateInsuranceProviderDto
{
    public string ProviderCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? ShortName { get; set; }
    public string InsuranceType { get; set; } = "Health";
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }
    public string? LicenseNumber { get; set; }
    public string? ClaimsEndpoint { get; set; }
    public string? EligibilityEndpoint { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public decimal DefaultDiscountPercent { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Insurance Plan DTOs

/// <summary>
/// DTO for insurance plan
/// </summary>
public class InsurancePlanDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public string NetworkType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal CoveragePercent { get; set; }
    public decimal CopayPercent { get; set; }
    public decimal? CopayAmount { get; set; }
    public decimal? DeductibleAmount { get; set; }
    public decimal? AnnualMaximum { get; set; }
    public decimal? LifetimeMaximum { get; set; }
    public decimal? MaxPerVisit { get; set; }
    public bool RequiresPreAuth { get; set; }
    public bool RequiresReferral { get; set; }
    public int WaitingPeriodDays { get; set; }
    public int ClaimGracePeriodDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating insurance plan
/// </summary>
public class CreateInsurancePlanDto
{
    public int ProviderId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string PlanType { get; set; } = "Standard";
    public string NetworkType { get; set; } = "In-Network";
    public decimal CoveragePercent { get; set; } = 80;
    public decimal CopayPercent { get; set; } = 20;
    public decimal? CopayAmount { get; set; }
    public decimal? DeductibleAmount { get; set; }
    public decimal? AnnualMaximum { get; set; }
    public decimal? LifetimeMaximum { get; set; }
    public decimal? MaxPerVisit { get; set; }
    public bool RequiresPreAuth { get; set; }
    public bool RequiresReferral { get; set; }
    public string? CoveredServices { get; set; }
    public string? ExcludedServices { get; set; }
    public int WaitingPeriodDays { get; set; }
    public int ClaimGracePeriodDays { get; set; } = 30;
    public string? Notes { get; set; }
}

#endregion

#region Patient Insurance DTOs

/// <summary>
/// DTO for patient insurance
/// </summary>
public class PatientInsuranceDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public int ProviderId { get; set; }
    public string? ProviderName { get; set; }
    public int PlanId { get; set; }
    public string? PlanName { get; set; }
    public string MemberId { get; set; } = string.Empty;
    public string? GroupNumber { get; set; }
    public string? SubscriberId { get; set; }
    public bool IsPrimary { get; set; }
    public string RelationshipToSubscriber { get; set; } = string.Empty;
    public string? SubscriberName { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastVerifiedDate { get; set; }
    public string? VerificationStatus { get; set; }
    public decimal? RemainingDeductible { get; set; }
    public decimal? RemainingAnnualBenefit { get; set; }
    public decimal YtdClaimsAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating patient insurance
/// </summary>
public class CreatePatientInsuranceDto
{
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public int PlanId { get; set; }
    public string MemberId { get; set; } = string.Empty;
    public string? GroupNumber { get; set; }
    public string? SubscriberId { get; set; }
    public bool IsPrimary { get; set; } = true;
    public string RelationshipToSubscriber { get; set; } = "Self";
    public string? SubscriberName { get; set; }
    public DateTime? SubscriberDateOfBirth { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? CardImageFront { get; set; }
    public string? CardImageBack { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for eligibility verification request
/// </summary>
public class EligibilityVerificationRequestDto
{
    public int PatientInsuranceId { get; set; }
    public DateTime? ServiceDate { get; set; }
    public List<string>? ServiceCodes { get; set; }
}

/// <summary>
/// DTO for eligibility verification response
/// </summary>
public class EligibilityVerificationResponseDto
{
    public bool IsEligible { get; set; }
    public DateTime VerificationDate { get; set; }
    public string? VerificationId { get; set; }
    public string? Status { get; set; }
    public string? StatusMessage { get; set; }
    public DateTime? CoverageStartDate { get; set; }
    public DateTime? CoverageEndDate { get; set; }
    public decimal? RemainingDeductible { get; set; }
    public decimal? RemainingAnnualBenefit { get; set; }
    public decimal? Copay { get; set; }
    public decimal? CoinsurancePercent { get; set; }
    public List<CoveredServiceDto>? CoveredServices { get; set; }
    public string? RawResponse { get; set; }
}

/// <summary>
/// DTO for covered service in eligibility response
/// </summary>
public class CoveredServiceDto
{
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public bool IsCovered { get; set; }
    public decimal? CoveragePercent { get; set; }
    public decimal? Copay { get; set; }
    public bool RequiresPreAuth { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Insurance Claim DTOs

/// <summary>
/// DTO for insurance claim
/// </summary>
public class InsuranceClaimDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public int PatientInsuranceId { get; set; }
    public string? InsuranceProviderName { get; set; }
    public string? InsurancePlanName { get; set; }
    public string? MemberId { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public int? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? PreAuthorizationId { get; set; }
    public string? PreAuthNumber { get; set; }
    public ClaimStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        ClaimStatus.Draft => "Draft",
        ClaimStatus.Submitted => "Submitted",
        ClaimStatus.Acknowledged => "Acknowledged",
        ClaimStatus.InReview => "In Review",
        ClaimStatus.Pending => "Pending",
        ClaimStatus.Approved => "Approved",
        ClaimStatus.PartiallyApproved => "Partially Approved",
        ClaimStatus.Denied => "Denied",
        ClaimStatus.Appealed => "Appealed",
        ClaimStatus.Paid => "Paid",
        ClaimStatus.PartiallyPaid => "Partially Paid",
        ClaimStatus.Voided => "Voided",
        ClaimStatus.Resubmitted => "Resubmitted",
        _ => "Unknown"
    };
    public string ClaimType { get; set; } = string.Empty;
    public DateTime ServiceDateFrom { get; set; }
    public DateTime ServiceDateTo { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public decimal TotalBilledAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PatientResponsibility { get; set; }
    public decimal CopayAmount { get; set; }
    public decimal DeductibleAmount { get; set; }
    public decimal CoinsuranceAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentReference { get; set; }
    public string? RenderingProviderName { get; set; }
    public string? PrimaryDiagnosisCode { get; set; }
    public string? DenialReasonCode { get; set; }
    public string? DenialReason { get; set; }
    public int ItemsCount { get; set; }
    public int ResubmissionCount { get; set; }
    public DateTime? FilingDeadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating insurance claim
/// </summary>
public class CreateInsuranceClaimDto
{
    public int PatientInsuranceId { get; set; }
    public int PatientId { get; set; }
    public int? InvoiceId { get; set; }
    public int? PreAuthorizationId { get; set; }
    public string ClaimType { get; set; } = "Professional";
    public DateTime ServiceDateFrom { get; set; }
    public DateTime ServiceDateTo { get; set; }
    public string? RenderingProviderName { get; set; }
    public string? RenderingProviderNpi { get; set; }
    public string? FacilityName { get; set; }
    public string PlaceOfServiceCode { get; set; } = "11";
    public string? PrimaryDiagnosisCode { get; set; }
    public List<string>? SecondaryDiagnosisCodes { get; set; }
    public List<CreateInsuranceClaimItemDto> Items { get; set; } = new();
    public string? InternalNotes { get; set; }
}

/// <summary>
/// DTO for creating claim line item
/// </summary>
public class CreateInsuranceClaimItemDto
{
    public string ProcedureCode { get; set; } = string.Empty;
    public string? ProcedureDescription { get; set; }
    public string? Modifiers { get; set; }
    public string? DiagnosisPointers { get; set; }
    public string? RevenueCode { get; set; }
    public string? NdcCode { get; set; }
    public DateTime ServiceDate { get; set; }
    public decimal Units { get; set; } = 1;
    public string UnitType { get; set; } = "UN";
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for claim submission
/// </summary>
public class SubmitClaimDto
{
    public int ClaimId { get; set; }
    public bool ValidateOnly { get; set; }
}

/// <summary>
/// Response from claim submission
/// </summary>
public class ClaimSubmissionResponseDto
{
    public bool Success { get; set; }
    public string? ClaimNumber { get; set; }
    public string? PayerClaimControlNumber { get; set; }
    public string? EdiClaimId { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// DTO for claim status check
/// </summary>
public class ClaimStatusResponseDto
{
    public string ClaimNumber { get; set; } = string.Empty;
    public ClaimStatus Status { get; set; }
    public DateTime StatusDate { get; set; }
    public string? StatusDescription { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? DenialReason { get; set; }
    public string? PayerRemarks { get; set; }
}

#endregion

#region Pre-Authorization DTOs

/// <summary>
/// DTO for insurance pre-authorization
/// </summary>
public class InsurancePreAuthorizationDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string PreAuthNumber { get; set; } = string.Empty;
    public int PatientInsuranceId { get; set; }
    public string? InsuranceProviderName { get; set; }
    public string? MemberId { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public PreAuthStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        PreAuthStatus.Draft => "Draft",
        PreAuthStatus.Submitted => "Submitted",
        PreAuthStatus.InReview => "In Review",
        PreAuthStatus.Approved => "Approved",
        PreAuthStatus.PartiallyApproved => "Partially Approved",
        PreAuthStatus.Denied => "Denied",
        PreAuthStatus.Expired => "Expired",
        PreAuthStatus.Cancelled => "Cancelled",
        PreAuthStatus.PendingInfo => "Pending Info",
        _ => "Unknown"
    };
    public string RequestType { get; set; } = string.Empty;
    public string ServiceCategory { get; set; } = string.Empty;
    public string? PrimaryDiagnosisCode { get; set; }
    public string? PrimaryDiagnosisDescription { get; set; }
    public string? RequestingProviderName { get; set; }
    public DateTime? PlannedServiceDate { get; set; }
    public int? RequestedUnits { get; set; }
    public decimal EstimatedCost { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public string? AuthorizationNumber { get; set; }
    public decimal ApprovedAmount { get; set; }
    public int? ApprovedUnits { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? DenialReasonCode { get; set; }
    public string? DenialReason { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating pre-authorization
/// </summary>
public class CreatePreAuthorizationDto
{
    public int PatientInsuranceId { get; set; }
    public int PatientId { get; set; }
    public string RequestType { get; set; } = "Elective";
    public string ServiceCategory { get; set; } = string.Empty;
    public List<string> RequestedProcedures { get; set; } = new();
    public string? PrimaryDiagnosisCode { get; set; }
    public string? PrimaryDiagnosisDescription { get; set; }
    public List<string>? SecondaryDiagnosisCodes { get; set; }
    public string? ClinicalJustification { get; set; }
    public string? RequestingProviderName { get; set; }
    public string? RequestingProviderNpi { get; set; }
    public DateTime? PlannedServiceDate { get; set; }
    public int? RequestedDays { get; set; }
    public int? RequestedUnits { get; set; }
    public decimal EstimatedCost { get; set; }
    public List<string>? AttachedDocuments { get; set; }
    public string? InternalNotes { get; set; }
}

#endregion

#region Insurance Statistics DTOs

/// <summary>
/// Insurance module statistics
/// </summary>
public class InsuranceStatisticsDto
{
    public int TotalProviders { get; set; }
    public int ActiveProviders { get; set; }
    public int TotalPlans { get; set; }
    public int ActivePlans { get; set; }
    public int TotalPatientInsurances { get; set; }
    public int ActivePatientInsurances { get; set; }

    // Claims Statistics
    public int TotalClaims { get; set; }
    public int DraftClaims { get; set; }
    public int SubmittedClaims { get; set; }
    public int ApprovedClaims { get; set; }
    public int DeniedClaims { get; set; }
    public int PaidClaims { get; set; }
    public decimal TotalBilledAmount { get; set; }
    public decimal TotalApprovedAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalDeniedAmount { get; set; }
    public decimal ClaimApprovalRate { get; set; }
    public decimal AveragePaymentTime { get; set; }

    // Pre-Auth Statistics
    public int TotalPreAuths { get; set; }
    public int PendingPreAuths { get; set; }
    public int ApprovedPreAuths { get; set; }
    public int DeniedPreAuths { get; set; }
    public decimal PreAuthApprovalRate { get; set; }

    // Top Providers
    public Dictionary<string, decimal> TopProvidersByVolume { get; set; } = new();
    public Dictionary<string, decimal> TopProvidersByPayments { get; set; } = new();
}

#endregion
