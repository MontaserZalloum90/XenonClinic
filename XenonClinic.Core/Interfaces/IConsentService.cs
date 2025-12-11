using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Patient consent management service
/// </summary>
public interface IConsentService
{
    #region Consent CRUD

    Task<PatientConsentDto> GetConsentAsync(int consentId);
    Task<List<PatientConsentDto>> GetPatientConsentsAsync(int patientId);
    Task<PatientConsentDto> CreateConsentAsync(SaveConsentDto request, int createdByUserId);
    Task<PatientConsentDto> UpdateConsentAsync(int consentId, SaveConsentDto request, int updatedByUserId);
    Task<bool> RevokeConsentAsync(RevokeConsentDto request, int revokedByUserId);

    #endregion

    #region Consent Verification

    Task<ConsentVerificationDto> VerifyConsentAsync(int patientId, string consentType, string? purpose = null);
    Task<bool> HasActiveConsentAsync(int patientId, string consentType);
    Task<bool> CanShareDataAsync(int patientId, string recipientType, string purpose);
    Task<bool> CanParticipateInResearchAsync(int patientId);

    #endregion

    #region Consent Summary

    Task<PatientConsentSummaryDto> GetConsentSummaryAsync(int patientId);
    Task<List<PatientConsentDto>> GetExpiringConsentsAsync(int daysAhead = 30);
    Task<List<PatientConsentDto>> GetPendingConsentsAsync(int? branchId = null);

    #endregion

    #region Templates

    Task<List<ConsentFormTemplateDto>> GetConsentTemplatesAsync(string? consentType = null);
    Task<ConsentFormTemplateDto> GetTemplateAsync(int templateId);
    Task<ConsentFormTemplateDto> SaveTemplateAsync(ConsentFormTemplateDto template, int userId);
    Task<string> GenerateConsentDocumentAsync(int templateId, int patientId);

    #endregion

    #region HIE/FHIR Integration

    Task<ConsentDirectiveDto> GetConsentDirectiveAsync(int patientId);
    Task<object> ExportToFhirConsentAsync(int consentId);
    Task<PatientConsentDto> ImportFromFhirConsentAsync(object fhirConsent, int patientId);

    #endregion

    #region Notifications

    Task SendConsentReminderAsync(int consentId);
    Task ProcessExpiringConsentsAsync();

    #endregion
}
