using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Insurance management service.
/// </summary>
public interface IInsuranceService
{
    // Insurance Providers
    Task<InsuranceProvider> CreateProviderAsync(CreateInsuranceProviderRequest request);
    Task<InsuranceProvider?> GetProviderAsync(int providerId);
    Task<IEnumerable<InsuranceProvider>> GetProvidersAsync(bool activeOnly = true);
    Task<InsuranceProvider> UpdateProviderAsync(int providerId, UpdateInsuranceProviderRequest request);

    // Insurance Plans
    Task<InsurancePlan> CreatePlanAsync(int providerId, CreateInsurancePlanRequest request);
    Task<IEnumerable<InsurancePlan>> GetPlansByProviderAsync(int providerId);
    Task<InsurancePlan> UpdatePlanAsync(int planId, UpdateInsurancePlanRequest request);

    // Patient Insurance
    Task<PatientInsurance> AddPatientInsuranceAsync(AddPatientInsuranceRequest request);
    Task<PatientInsurance?> GetPatientInsuranceAsync(int patientInsuranceId);
    Task<IEnumerable<PatientInsurance>> GetPatientInsurancesAsync(int patientId);
    Task<PatientInsurance?> GetPrimaryInsuranceAsync(int patientId);
    Task<PatientInsurance> UpdatePatientInsuranceAsync(int patientInsuranceId, UpdatePatientInsuranceRequest request);
    Task TerminatePatientInsuranceAsync(int patientInsuranceId, DateTime terminationDate);

    // Eligibility Verification
    Task<EligibilityVerification> VerifyEligibilityAsync(int patientInsuranceId);
    Task<EligibilityVerification?> GetLastEligibilityVerificationAsync(int patientInsuranceId);
    Task<IEnumerable<EligibilityVerification>> GetEligibilityHistoryAsync(int patientId);

    // Pre-Authorization
    Task<PreAuthorization> RequestPreAuthorizationAsync(CreatePreAuthRequest request);
    Task<PreAuthorization?> GetPreAuthorizationAsync(int preAuthId);
    Task<PreAuthorization?> GetPreAuthorizationByNumberAsync(string authorizationNumber);
    Task<IEnumerable<PreAuthorization>> GetActivePreAuthorizationsAsync(int patientId);
    Task UpdatePreAuthorizationStatusAsync(int preAuthId, AuthorizationStatus status, string? notes);
    Task<bool> ValidatePreAuthorizationAsync(int patientId, string procedureCode);

    // Fee Schedules
    Task<FeeSchedule> CreateFeeScheduleAsync(CreateFeeScheduleRequest request);
    Task<FeeSchedule?> GetFeeScheduleAsync(int feeScheduleId);
    Task<FeeSchedule?> GetFeeScheduleForInsuranceAsync(int insuranceProviderId);
    Task<FeeSchedule?> GetDefaultFeeScheduleAsync();
    Task<decimal> GetFeeForProcedureAsync(string procedureCode, int? insuranceProviderId = null);
    Task AddFeeScheduleItemAsync(int feeScheduleId, AddFeeScheduleItemRequest request);
    Task UpdateFeeScheduleItemAsync(int itemId, decimal fee, decimal? allowedAmount);

    // Benefits
    Task<BenefitsSummary> GetBenefitsSummaryAsync(int patientInsuranceId);
    Task<CoverageCheck> CheckCoverageAsync(int patientId, string procedureCode);
}

// Request DTOs
public record CreateInsuranceProviderRequest(
    string Code,
    string Name,
    string? PayerId,
    InsuranceType Type,
    string? Address,
    string? Phone,
    string? ClaimsAddress,
    int ClaimSubmissionDays = 365,
    bool RequiresPreAuthorization = false
);

public record UpdateInsuranceProviderRequest(
    string? Name,
    string? PayerId,
    string? Address,
    string? Phone,
    string? ClaimsAddress,
    bool? IsActive
);

public record CreateInsurancePlanRequest(
    string PlanCode,
    string PlanName,
    PlanType PlanType,
    decimal? Deductible,
    decimal? OutOfPocketMax,
    decimal? CoPayAmount,
    decimal? CoInsurancePercent
);

public record UpdateInsurancePlanRequest(
    string? PlanName,
    decimal? Deductible,
    decimal? OutOfPocketMax,
    decimal? CoPayAmount,
    decimal? CoInsurancePercent,
    bool? IsActive
);

public record AddPatientInsuranceRequest(
    int PatientId,
    int InsuranceProviderId,
    int? InsurancePlanId,
    InsurancePriority Priority,
    string PolicyNumber,
    string? GroupNumber,
    string SubscriberId,
    string SubscriberFirstName,
    string SubscriberLastName,
    DateTime? SubscriberDateOfBirth,
    PatientRelationship RelationshipToSubscriber,
    DateTime EffectiveDate
);

public record UpdatePatientInsuranceRequest(
    string? PolicyNumber,
    string? GroupNumber,
    InsurancePriority? Priority,
    DateTime? EffectiveDate,
    decimal? CopayAmount
);

public record CreatePreAuthRequest(
    int PatientId,
    int PatientInsuranceId,
    int? ProviderId,
    AuthorizationType Type,
    List<string> ProcedureCodes,
    List<string> DiagnosisCodes,
    int? RequestedVisits,
    string? ClinicalNotes
);

public record CreateFeeScheduleRequest(
    string Name,
    int? InsuranceProviderId,
    DateTime EffectiveDate,
    bool IsDefault = false
);

public record AddFeeScheduleItemRequest(
    string ProcedureCode,
    string? Description,
    decimal Fee,
    decimal? AllowedAmount,
    bool RequiresPreAuth = false
);

// Response DTOs
public record BenefitsSummary(
    string PlanName,
    bool IsEligible,
    DateTime? CoverageEffectiveDate,
    decimal? Deductible,
    decimal? DeductibleMet,
    decimal? DeductibleRemaining,
    decimal? OutOfPocketMax,
    decimal? OutOfPocketMet,
    decimal? OutOfPocketRemaining,
    decimal? CoPay,
    decimal? CoInsurancePercent,
    string? NetworkStatus
);

public record CoverageCheck(
    bool IsCovered,
    bool RequiresPreAuth,
    bool HasActivePreAuth,
    string? PreAuthNumber,
    decimal? EstimatedAllowed,
    decimal? EstimatedPatientResponsibility,
    string? Notes
);
