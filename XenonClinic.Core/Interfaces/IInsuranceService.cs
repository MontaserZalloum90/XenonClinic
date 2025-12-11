using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service interface for Insurance operations
/// </summary>
public interface IInsuranceService
{
    #region Insurance Providers

    /// <summary>
    /// Get all insurance providers for a branch
    /// </summary>
    Task<IEnumerable<InsuranceProviderDto>> GetProvidersAsync(int branchId, bool activeOnly = false);

    /// <summary>
    /// Get an insurance provider by ID
    /// </summary>
    Task<InsuranceProviderDto?> GetProviderByIdAsync(int id);

    /// <summary>
    /// Get an insurance provider by code
    /// </summary>
    Task<InsuranceProviderDto?> GetProviderByCodeAsync(int branchId, string providerCode);

    /// <summary>
    /// Create an insurance provider
    /// </summary>
    Task<InsuranceProviderDto> CreateProviderAsync(int branchId, CreateInsuranceProviderDto dto);

    /// <summary>
    /// Update an insurance provider
    /// </summary>
    Task<InsuranceProviderDto> UpdateProviderAsync(int id, CreateInsuranceProviderDto dto);

    /// <summary>
    /// Delete an insurance provider
    /// </summary>
    Task DeleteProviderAsync(int id);

    #endregion

    #region Insurance Plans

    /// <summary>
    /// Get all plans for a provider
    /// </summary>
    Task<IEnumerable<InsurancePlanDto>> GetPlansByProviderAsync(int providerId, bool activeOnly = false);

    /// <summary>
    /// Get an insurance plan by ID
    /// </summary>
    Task<InsurancePlanDto?> GetPlanByIdAsync(int id);

    /// <summary>
    /// Create an insurance plan
    /// </summary>
    Task<InsurancePlanDto> CreatePlanAsync(int branchId, CreateInsurancePlanDto dto);

    /// <summary>
    /// Update an insurance plan
    /// </summary>
    Task<InsurancePlanDto> UpdatePlanAsync(int id, CreateInsurancePlanDto dto);

    /// <summary>
    /// Delete an insurance plan
    /// </summary>
    Task DeletePlanAsync(int id);

    #endregion

    #region Patient Insurance

    /// <summary>
    /// Get all insurance records for a patient
    /// </summary>
    Task<IEnumerable<PatientInsuranceDto>> GetPatientInsurancesAsync(int patientId);

    /// <summary>
    /// Get a patient insurance by ID
    /// </summary>
    Task<PatientInsuranceDto?> GetPatientInsuranceByIdAsync(int id);

    /// <summary>
    /// Get primary insurance for a patient
    /// </summary>
    Task<PatientInsuranceDto?> GetPrimaryInsuranceAsync(int patientId);

    /// <summary>
    /// Create a patient insurance record
    /// </summary>
    Task<PatientInsuranceDto> CreatePatientInsuranceAsync(int branchId, CreatePatientInsuranceDto dto);

    /// <summary>
    /// Update a patient insurance record
    /// </summary>
    Task<PatientInsuranceDto> UpdatePatientInsuranceAsync(int id, CreatePatientInsuranceDto dto);

    /// <summary>
    /// Terminate a patient insurance
    /// </summary>
    Task TerminatePatientInsuranceAsync(int id, DateTime terminationDate);

    /// <summary>
    /// Verify eligibility with insurance provider
    /// </summary>
    Task<EligibilityVerificationResponseDto> VerifyEligibilityAsync(EligibilityVerificationRequestDto request);

    #endregion

    #region Insurance Claims

    /// <summary>
    /// Get claims for a branch
    /// </summary>
    Task<IEnumerable<InsuranceClaimDto>> GetClaimsAsync(int branchId, ClaimStatus? status = null, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get claims for a patient
    /// </summary>
    Task<IEnumerable<InsuranceClaimDto>> GetClaimsByPatientAsync(int patientId);

    /// <summary>
    /// Get a claim by ID
    /// </summary>
    Task<InsuranceClaimDto?> GetClaimByIdAsync(int id);

    /// <summary>
    /// Get a claim by claim number
    /// </summary>
    Task<InsuranceClaimDto?> GetClaimByNumberAsync(string claimNumber);

    /// <summary>
    /// Create a new claim
    /// </summary>
    Task<InsuranceClaimDto> CreateClaimAsync(int branchId, CreateInsuranceClaimDto dto);

    /// <summary>
    /// Update a claim
    /// </summary>
    Task<InsuranceClaimDto> UpdateClaimAsync(int id, CreateInsuranceClaimDto dto);

    /// <summary>
    /// Submit a claim to insurance
    /// </summary>
    Task<ClaimSubmissionResponseDto> SubmitClaimAsync(SubmitClaimDto dto);

    /// <summary>
    /// Check claim status with insurance
    /// </summary>
    Task<ClaimStatusResponseDto> CheckClaimStatusAsync(int claimId);

    /// <summary>
    /// Void a claim
    /// </summary>
    Task VoidClaimAsync(int claimId, string reason);

    /// <summary>
    /// Resubmit a denied claim
    /// </summary>
    Task<ClaimSubmissionResponseDto> ResubmitClaimAsync(int claimId);

    /// <summary>
    /// Generate claim number
    /// </summary>
    Task<string> GenerateClaimNumberAsync(int branchId);

    #endregion

    #region Pre-Authorization

    /// <summary>
    /// Get pre-authorizations for a branch
    /// </summary>
    Task<IEnumerable<InsurancePreAuthorizationDto>> GetPreAuthorizationsAsync(int branchId, PreAuthStatus? status = null);

    /// <summary>
    /// Get pre-authorizations for a patient
    /// </summary>
    Task<IEnumerable<InsurancePreAuthorizationDto>> GetPreAuthorizationsByPatientAsync(int patientId);

    /// <summary>
    /// Get a pre-authorization by ID
    /// </summary>
    Task<InsurancePreAuthorizationDto?> GetPreAuthorizationByIdAsync(int id);

    /// <summary>
    /// Create a pre-authorization request
    /// </summary>
    Task<InsurancePreAuthorizationDto> CreatePreAuthorizationAsync(int branchId, CreatePreAuthorizationDto dto);

    /// <summary>
    /// Update a pre-authorization request
    /// </summary>
    Task<InsurancePreAuthorizationDto> UpdatePreAuthorizationAsync(int id, CreatePreAuthorizationDto dto);

    /// <summary>
    /// Submit a pre-authorization to insurance
    /// </summary>
    Task<InsurancePreAuthorizationDto> SubmitPreAuthorizationAsync(int id);

    /// <summary>
    /// Cancel a pre-authorization
    /// </summary>
    Task CancelPreAuthorizationAsync(int id, string reason);

    /// <summary>
    /// Generate pre-auth number
    /// </summary>
    Task<string> GeneratePreAuthNumberAsync(int branchId);

    #endregion

    #region Statistics

    /// <summary>
    /// Get insurance statistics for a branch
    /// </summary>
    Task<InsuranceStatisticsDto> GetStatisticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    #endregion
}
