using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Patient consent management service
/// </summary>
public interface IConsentService
{
    #region Consent CRUD

    Task<PatientConsentDto?> GetConsentByIdAsync(int consentId);
    Task<List<PatientConsentDto>> GetPatientConsentsAsync(int patientId, string? consentType = null, bool activeOnly = false);
    Task<PatientConsentDto> GrantConsentAsync(SaveConsentDto request, int grantedByUserId);
    Task<PatientConsentDto?> UpdateConsentAsync(int consentId, SaveConsentDto request, int updatedByUserId);
    Task<PatientConsentDto> RevokeConsentAsync(RevokeConsentDto request, int revokedByUserId);
    Task<List<ConsentHistoryDto>> GetConsentHistoryAsync(int consentId);

    #endregion

    #region Consent Verification

    Task<ConsentVerificationDto> VerifyConsentAsync(int patientId, string consentType);
    Task<ConsentVerificationDto> VerifyConsentForPurposeAsync(int patientId, string consentType, string purpose);
    Task<Dictionary<string, ConsentVerificationDto>> VerifyMultipleConsentsAsync(int patientId, List<string> consentTypes);
    Task<bool> HasActiveConsentAsync(int patientId, string consentType);
    Task<bool> CanShareDataAsync(int patientId, string recipientType, string purpose);
    Task<bool> CanParticipateInResearchAsync(int patientId);

    #endregion

    #region Consent Summary & Reports

    Task<PatientConsentSummaryDto> GetPatientConsentSummaryAsync(int patientId);
    Task<List<PatientConsentDto>> GetExpiringConsentsAsync(int branchId, int daysAhead = 30);
    Task<List<PatientConsentDto>> GetPendingConsentsAsync(int? branchId = null);
    Task<object> GetComplianceReportAsync(int branchId, DateTime startDate, DateTime endDate);

    #endregion

    #region Templates

    Task<List<ConsentFormTemplateDto>> GetConsentFormTemplatesAsync(string? consentType = null, string? language = null);
    Task<ConsentFormTemplateDto?> GetConsentFormTemplateAsync(int templateId);
    Task<ConsentFormTemplateDto> CreateConsentFormTemplateAsync(ConsentFormTemplateDto template);
    Task<ConsentFormTemplateDto?> UpdateConsentFormTemplateAsync(ConsentFormTemplateDto template);
    Task<string> GenerateConsentDocumentAsync(int templateId, int patientId);

    #endregion

    #region HIE/FHIR Integration

    Task<ConsentDirectiveDto> GetConsentDirectiveAsync(int patientId);
    Task<ConsentDirectiveDto?> ExportToFhirAsync(int consentId);
    Task<PatientConsentDto> ImportFromFhirConsentAsync(object fhirConsent, int patientId);

    #endregion

    #region Notifications

    Task SendConsentReminderAsync(int consentId);
    Task ProcessExpiringConsentsAsync();

    #endregion
}
