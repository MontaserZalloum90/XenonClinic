namespace XenonClinic.Core.DTOs;

#region Consent DTOs

/// <summary>
/// Patient consent record
/// </summary>
public class PatientConsentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ConsentType { get; set; } = string.Empty;
    public string ConsentCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? GrantedDate { get; set; }
    public DateTime? RevokedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? GrantedBy { get; set; }
    public string? RevokedBy { get; set; }
    public string? RevokedReason { get; set; }
    public string? DocumentPath { get; set; }
    public string? SignatureData { get; set; }
    public string? WitnessName { get; set; }
    public ConsentScopeDto? Scope { get; set; }
    public List<ConsentHistoryDto>? History { get; set; }
}

/// <summary>
/// Consent scope - what the consent covers
/// </summary>
public class ConsentScopeDto
{
    public List<string>? DataCategories { get; set; }
    public List<string>? AllowedPurposes { get; set; }
    public List<string>? AllowedRecipients { get; set; }
    public bool AllowResearch { get; set; }
    public bool AllowMarketing { get; set; }
    public bool AllowDataSharing { get; set; }
    public bool AllowHIEExchange { get; set; }
    public List<int>? SpecificProviderIds { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

/// <summary>
/// Consent history entry
/// </summary>
public class ConsentHistoryDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ActionDate { get; set; }
    public int? ActionByUserId { get; set; }
    public string? ActionByUserName { get; set; }
    public string? Reason { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// Create/Update consent request
/// </summary>
public class SaveConsentDto
{
    public int PatientId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public string ConsentCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? DocumentPath { get; set; }
    public string? SignatureData { get; set; }
    public string? WitnessName { get; set; }
    public ConsentScopeDto? Scope { get; set; }
}

/// <summary>
/// Revoke consent request
/// </summary>
public class RevokeConsentDto
{
    public int ConsentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? EffectiveDate { get; set; }
}

/// <summary>
/// Consent verification result
/// </summary>
public class ConsentVerificationDto
{
    public bool HasConsent { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? GrantedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsExpired { get; set; }
    public List<string>? AllowedPurposes { get; set; }
    public string? DenialReason { get; set; }
}

/// <summary>
/// Consent form template
/// </summary>
public class ConsentFormTemplateDto
{
    public int Id { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string ConsentType { get; set; } = string.Empty;
    public string ConsentCategory { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TemplateContent { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public bool RequiresWitness { get; set; }
    public int? ValidityDays { get; set; }
    public List<string>? RequiredFields { get; set; }
}

/// <summary>
/// Patient consent summary
/// </summary>
public class PatientConsentSummaryDto
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int TotalConsents { get; set; }
    public int ActiveConsents { get; set; }
    public int RevokedConsents { get; set; }
    public int ExpiredConsents { get; set; }
    public int PendingConsents { get; set; }
    public List<ConsentStatusItemDto> ConsentsByType { get; set; } = new();
    public DateTime? LastConsentDate { get; set; }
    public bool HasTreatmentConsent { get; set; }
    public bool HasHIPAAConsent { get; set; }
    public bool HasResearchConsent { get; set; }
}

/// <summary>
/// Consent status by type
/// </summary>
public class ConsentStatusItemDto
{
    public string ConsentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? GrantedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool NeedsRenewal { get; set; }
}

/// <summary>
/// Consent directive for HIE/FHIR
/// </summary>
public class ConsentDirectiveDto
{
    public int PatientId { get; set; }
    public string DirectiveType { get; set; } = string.Empty; // permit, deny, permit-with-exception
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? Actors { get; set; }
    public List<string>? Actions { get; set; }
    public List<string>? Purposes { get; set; }
    public List<string>? DataCategories { get; set; }
    public string? PolicyUri { get; set; }
}

#endregion

#region Consent Types

/// <summary>
/// Standard consent types
/// </summary>
public static class ConsentTypes
{
    // Clinical Consents
    public const string TreatmentConsent = "TREATMENT";
    public const string InformedConsent = "INFORMED_CONSENT";
    public const string SurgicalConsent = "SURGICAL";
    public const string AnesthesiaConsent = "ANESTHESIA";
    public const string BloodTransfusion = "BLOOD_TRANSFUSION";
    public const string ProcedureConsent = "PROCEDURE";

    // Privacy & Data
    public const string HIPAAConsent = "HIPAA";
    public const string PrivacyPolicy = "PRIVACY_POLICY";
    public const string DataSharing = "DATA_SHARING";
    public const string HIEParticipation = "HIE_PARTICIPATION";
    public const string ThirdPartyDisclosure = "THIRD_PARTY_DISCLOSURE";

    // Research & Marketing
    public const string ResearchParticipation = "RESEARCH";
    public const string ClinicalTrial = "CLINICAL_TRIAL";
    public const string MarketingCommunication = "MARKETING";
    public const string Telehealth = "TELEHEALTH";

    // Financial
    public const string FinancialResponsibility = "FINANCIAL";
    public const string AssignmentOfBenefits = "ASSIGNMENT_OF_BENEFITS";
    public const string PaymentPlan = "PAYMENT_PLAN";

    // Other
    public const string AdvanceDirective = "ADVANCE_DIRECTIVE";
    public const string DNR = "DNR";
    public const string PhotographConsent = "PHOTOGRAPH";
    public const string MinorConsent = "MINOR";
}

/// <summary>
/// Consent categories
/// </summary>
public static class ConsentCategories
{
    public const string Clinical = "CLINICAL";
    public const string Privacy = "PRIVACY";
    public const string Research = "RESEARCH";
    public const string Financial = "FINANCIAL";
    public const string Administrative = "ADMINISTRATIVE";
}

/// <summary>
/// Consent statuses
/// </summary>
public static class ConsentStatuses
{
    public const string Pending = "PENDING";
    public const string Active = "ACTIVE";
    public const string Revoked = "REVOKED";
    public const string Expired = "EXPIRED";
    public const string Rejected = "REJECTED";
    public const string Superseded = "SUPERSEDED";
}

#endregion
