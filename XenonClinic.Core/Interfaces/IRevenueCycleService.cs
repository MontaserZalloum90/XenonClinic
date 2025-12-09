using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Revenue cycle management service.
/// </summary>
public interface IRevenueCycleService
{
    // Claims
    Task<InsuranceClaim> CreateClaimAsync(CreateClaimRequest request);
    Task<InsuranceClaim?> GetClaimAsync(int claimId);
    Task<InsuranceClaim?> GetClaimByNumberAsync(string claimNumber);
    Task<IEnumerable<InsuranceClaim>> GetClaimsAsync(ClaimSearchCriteria criteria);
    Task<InsuranceClaim> UpdateClaimAsync(int claimId, UpdateClaimRequest request);
    Task SubmitClaimAsync(int claimId);
    Task VoidClaimAsync(int claimId, string reason);

    // Claim Lines
    Task<ClaimLine> AddClaimLineAsync(int claimId, AddClaimLineRequest request);
    Task UpdateClaimLineAsync(int claimLineId, UpdateClaimLineRequest request);
    Task DeleteClaimLineAsync(int claimLineId);

    // Payments
    Task<ClaimPayment> PostPaymentAsync(int claimId, PostPaymentRequest request);
    Task<decimal> GetClaimBalanceAsync(int claimId);
    Task WriteOffBalanceAsync(int claimId, decimal amount, string reason);

    // Denials & Appeals
    Task RecordDenialAsync(int claimId, RecordDenialRequest request);
    Task<InsuranceClaim> CreateAppealAsync(int claimId, CreateAppealRequest request);

    // Patient Statements
    Task<PatientStatement> GenerateStatementAsync(int patientId);
    Task SendStatementAsync(int statementId, string deliveryMethod);
    Task<IEnumerable<PatientStatement>> GetPatientStatementsAsync(int patientId);

    // Payment Plans
    Task<PaymentPlan> CreatePaymentPlanAsync(CreatePaymentPlanRequest request);
    Task<PaymentPlanPayment> RecordPaymentPlanPaymentAsync(int paymentPlanId, decimal amount);
    Task<IEnumerable<PaymentPlan>> GetOverduePaymentPlansAsync();

    // Reporting
    Task<RevenueSummary> GetRevenueSummaryAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<AgingBucket>> GetAccountsReceivableAgingAsync();
    Task<IEnumerable<DenialAnalysis>> GetDenialAnalysisAsync(DateTime startDate, DateTime endDate);
}

// Request/Response DTOs
public record CreateClaimRequest(
    int PatientId,
    int? InsurancePolicyId,
    int? EncounterId,
    ClaimType ClaimType,
    DateTime ServiceDate,
    string? PrimaryDiagnosisCode,
    string? AuthorizationNumber,
    List<AddClaimLineRequest> Lines
);

public record UpdateClaimRequest(
    string? PrimaryDiagnosisCode,
    string? SecondaryDiagnosisCodes,
    string? AuthorizationNumber,
    string? ReferralNumber
);

public record AddClaimLineRequest(
    string ProcedureCode,
    string? Modifier1,
    string DiagnosisPointer,
    decimal Units,
    decimal ChargeAmount,
    DateTime ServiceDateFrom,
    DateTime? ServiceDateTo,
    int? ProviderId,
    string? PlaceOfService
);

public record UpdateClaimLineRequest(
    string? ProcedureCode,
    decimal? Units,
    decimal? ChargeAmount
);

public record PostPaymentRequest(
    PaymentSource Source,
    decimal Amount,
    DateTime PaymentDate,
    string? CheckNumber,
    string? EftNumber
);

public record RecordDenialRequest(
    string DenialCode,
    string DenialReason,
    DateTime DenialDate
);

public record CreateAppealRequest(
    string AppealReason,
    string SupportingDocumentation
);

public record CreatePaymentPlanRequest(
    int PatientId,
    decimal TotalAmount,
    decimal DownPayment,
    int NumberOfPayments,
    int PaymentDayOfMonth,
    DateTime StartDate
);

public record ClaimSearchCriteria(
    int? PatientId = null,
    ClaimStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? ClaimNumber = null,
    int Page = 1,
    int PageSize = 50
);

public record RevenueSummary(
    decimal TotalCharges,
    decimal TotalPayments,
    decimal TotalAdjustments,
    decimal TotalWriteOffs,
    decimal NetRevenue,
    decimal OutstandingAR,
    int ClaimsSubmitted,
    int ClaimsPaid,
    int ClaimsDenied,
    decimal DenialRate,
    decimal CollectionRate
);

public record AgingBucket(
    string BucketName,
    int DaysFrom,
    int DaysTo,
    decimal Amount,
    int ClaimCount,
    decimal Percentage
);

public record DenialAnalysis(
    string DenialCode,
    string DenialReason,
    int Count,
    decimal TotalAmount,
    decimal Percentage
);
