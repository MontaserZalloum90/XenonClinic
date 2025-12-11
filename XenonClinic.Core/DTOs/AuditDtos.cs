namespace XenonClinic.Core.DTOs;

#region Audit Log DTOs

/// <summary>
/// HIPAA-compliant audit log entry
/// </summary>
public class AuditLogDto
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventCategory { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserRole { get; set; }
    public int? PatientId { get; set; }
    public bool IsPHIAccess { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedFields { get; set; }
    public string? Reason { get; set; }
    public bool IsEmergencyAccess { get; set; }
    public string? EmergencyJustification { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? CorrelationId { get; set; }
    public long? DurationMs { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AdditionalData { get; set; }
}

/// <summary>
/// Audit log query request
/// </summary>
public class AuditLogQueryDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? EventType { get; set; }
    public string? EventCategory { get; set; }
    public string? Action { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public int? UserId { get; set; }
    public int? PatientId { get; set; }
    public bool? IsPHIAccess { get; set; }
    public bool? IsEmergencyAccess { get; set; }
    public string? IpAddress { get; set; }
    public int? BranchId { get; set; }
    public bool? IsSuccess { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Audit log query result
/// </summary>
public class AuditLogResultDto
{
    public List<AuditLogDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// PHI access report
/// </summary>
public class PHIAccessReportDto
{
    public DateTime ReportDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalPHIAccesses { get; set; }
    public int UniquePatients { get; set; }
    public int UniqueUsers { get; set; }
    public int EmergencyAccesses { get; set; }
    public int UnauthorizedAttempts { get; set; }
    public List<PHIAccessByUserDto> AccessByUser { get; set; } = new();
    public List<PHIAccessByResourceDto> AccessByResource { get; set; } = new();
    public List<PHIAccessTrendDto> DailyTrend { get; set; } = new();
    public List<SuspiciousActivityDto> SuspiciousActivities { get; set; } = new();
}

/// <summary>
/// PHI access by user
/// </summary>
public class PHIAccessByUserDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int AccessCount { get; set; }
    public int UniquePatients { get; set; }
    public DateTime LastAccess { get; set; }
}

/// <summary>
/// PHI access by resource type
/// </summary>
public class PHIAccessByResourceDto
{
    public string ResourceType { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int CreateCount { get; set; }
    public int UpdateCount { get; set; }
    public int DeleteCount { get; set; }
    public int ExportCount { get; set; }
}

/// <summary>
/// PHI access trend
/// </summary>
public class PHIAccessTrendDto
{
    public DateTime Date { get; set; }
    public int AccessCount { get; set; }
    public int UniqueUsers { get; set; }
    public int UniquePatients { get; set; }
}

/// <summary>
/// Suspicious activity detection
/// </summary>
public class SuspiciousActivityDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime DetectedAt { get; set; }
    public string? Details { get; set; }
    public bool IsInvestigated { get; set; }
}

/// <summary>
/// Patient access history (for patient's right to access logs)
/// </summary>
public class PatientAccessHistoryDto
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<PatientAccessEntryDto> AccessEntries { get; set; } = new();
}

/// <summary>
/// Individual access entry for patient history
/// </summary>
public class PatientAccessEntryDto
{
    public DateTime AccessDate { get; set; }
    public string AccessedBy { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResourceAccessed { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

#endregion

#region Security Event DTOs

/// <summary>
/// Security event types
/// </summary>
public static class AuditEventTypes
{
    // Authentication events
    public const string Login = "LOGIN";
    public const string Logout = "LOGOUT";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string PasswordChanged = "PASSWORD_CHANGED";
    public const string PasswordReset = "PASSWORD_RESET";
    public const string MfaEnabled = "MFA_ENABLED";
    public const string MfaDisabled = "MFA_DISABLED";
    public const string SessionTimeout = "SESSION_TIMEOUT";
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string AccountUnlocked = "ACCOUNT_UNLOCKED";

    // Authorization events
    public const string AccessGranted = "ACCESS_GRANTED";
    public const string AccessDenied = "ACCESS_DENIED";
    public const string PermissionChanged = "PERMISSION_CHANGED";
    public const string RoleAssigned = "ROLE_ASSIGNED";
    public const string RoleRevoked = "ROLE_REVOKED";

    // Data events
    public const string DataCreate = "DATA_CREATE";
    public const string DataRead = "DATA_READ";
    public const string DataUpdate = "DATA_UPDATE";
    public const string DataDelete = "DATA_DELETE";
    public const string DataExport = "DATA_EXPORT";
    public const string DataImport = "DATA_IMPORT";
    public const string DataPrint = "DATA_PRINT";

    // PHI specific events
    public const string PHIAccess = "PHI_ACCESS";
    public const string PHIDisclosure = "PHI_DISCLOSURE";
    public const string PHIAmendment = "PHI_AMENDMENT";
    public const string EmergencyAccess = "EMERGENCY_ACCESS";
    public const string BreakTheGlass = "BREAK_THE_GLASS";

    // System events
    public const string SystemStartup = "SYSTEM_STARTUP";
    public const string SystemShutdown = "SYSTEM_SHUTDOWN";
    public const string ConfigChange = "CONFIG_CHANGE";
    public const string BackupCreated = "BACKUP_CREATED";
    public const string BackupRestored = "BACKUP_RESTORED";
    public const string IntegrationCall = "INTEGRATION_CALL";
}

/// <summary>
/// Audit event categories
/// </summary>
public static class AuditEventCategories
{
    public const string Authentication = "AUTHENTICATION";
    public const string Authorization = "AUTHORIZATION";
    public const string PatientData = "PATIENT_DATA";
    public const string ClinicalData = "CLINICAL_DATA";
    public const string FinancialData = "FINANCIAL_DATA";
    public const string Administrative = "ADMINISTRATIVE";
    public const string System = "SYSTEM";
    public const string Integration = "INTEGRATION";
    public const string Security = "SECURITY";
}

/// <summary>
/// PHI resource types requiring audit
/// </summary>
public static class PHIResourceTypes
{
    public const string Patient = "PATIENT";
    public const string MedicalRecord = "MEDICAL_RECORD";
    public const string Diagnosis = "DIAGNOSIS";
    public const string Prescription = "PRESCRIPTION";
    public const string LabResult = "LAB_RESULT";
    public const string Imaging = "IMAGING";
    public const string ClinicalNote = "CLINICAL_NOTE";
    public const string InsuranceInfo = "INSURANCE_INFO";
    public const string BillingRecord = "BILLING_RECORD";
    public const string Appointment = "APPOINTMENT";
    public const string Message = "MESSAGE";
    public const string Consent = "CONSENT";
}

#endregion

#region Audit Configuration DTOs

/// <summary>
/// Audit retention policy
/// </summary>
public class AuditRetentionPolicyDto
{
    public int Id { get; set; }
    public string EventCategory { get; set; } = string.Empty;
    public int RetentionDays { get; set; }
    public bool ArchiveBeforeDelete { get; set; }
    public string? ArchiveLocation { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Audit alert configuration
/// </summary>
public class AuditAlertConfigDto
{
    public int Id { get; set; }
    public string AlertName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public int Threshold { get; set; }
    public int TimeWindowMinutes { get; set; }
    public string Severity { get; set; } = string.Empty;
    public List<string> NotifyEmails { get; set; } = new();
    public bool IsActive { get; set; }
}

#endregion
