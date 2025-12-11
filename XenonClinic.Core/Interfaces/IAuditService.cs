using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// HIPAA-compliant audit logging service interface
/// </summary>
public interface IAuditService
{
    #region Logging Methods

    /// <summary>
    /// Log an audit event
    /// </summary>
    Task LogAsync(AuditLogEntry entry);

    /// <summary>
    /// Log PHI access event
    /// </summary>
    Task LogPHIAccessAsync(int userId, int patientId, string resourceType, string action, string? resourceId = null, string? reason = null);

    /// <summary>
    /// Log emergency/break-the-glass access
    /// </summary>
    Task LogEmergencyAccessAsync(int userId, int patientId, string resourceType, string justification);

    /// <summary>
    /// Log authentication event
    /// </summary>
    Task LogAuthenticationAsync(string eventType, int? userId, string? userName, bool isSuccess, string? ipAddress = null, string? errorMessage = null);

    /// <summary>
    /// Log authorization event
    /// </summary>
    Task LogAuthorizationAsync(int userId, string resource, string action, bool isGranted, string? reason = null);

    /// <summary>
    /// Log data change event with old/new values
    /// </summary>
    Task LogDataChangeAsync(int userId, string resourceType, string resourceId, string action, object? oldValue, object? newValue, string? reason = null);

    /// <summary>
    /// Log system event
    /// </summary>
    Task LogSystemEventAsync(string eventType, string description, string? details = null);

    /// <summary>
    /// Log integration/external API call
    /// </summary>
    Task LogIntegrationCallAsync(string serviceName, string operation, bool isSuccess, long durationMs, string? errorMessage = null);

    #endregion

    #region Query Methods

    /// <summary>
    /// Query audit logs with filters
    /// </summary>
    Task<AuditLogResultDto> QueryLogsAsync(AuditLogQueryDto query);

    /// <summary>
    /// Get audit log by ID
    /// </summary>
    Task<AuditLogDto?> GetByIdAsync(long id);

    /// <summary>
    /// Get PHI access report
    /// </summary>
    Task<PHIAccessReportDto> GetPHIAccessReportAsync(DateTime startDate, DateTime endDate, int? branchId = null);

    /// <summary>
    /// Get patient access history (HIPAA requirement - patient's right to access logs)
    /// </summary>
    Task<PatientAccessHistoryDto> GetPatientAccessHistoryAsync(int patientId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get user activity summary
    /// </summary>
    Task<List<PHIAccessByUserDto>> GetUserActivitySummaryAsync(DateTime startDate, DateTime endDate, int? branchId = null);

    /// <summary>
    /// Detect suspicious activities
    /// </summary>
    Task<List<SuspiciousActivityDto>> DetectSuspiciousActivitiesAsync(DateTime startDate, DateTime endDate);

    #endregion

    #region Retention & Archival

    /// <summary>
    /// Archive old audit logs
    /// </summary>
    Task<int> ArchiveOldLogsAsync(DateTime beforeDate, string archivePath);

    /// <summary>
    /// Purge archived logs per retention policy
    /// </summary>
    Task<int> PurgeArchivedLogsAsync(int retentionDays);

    /// <summary>
    /// Get retention policies
    /// </summary>
    Task<List<AuditRetentionPolicyDto>> GetRetentionPoliciesAsync();

    /// <summary>
    /// Update retention policy
    /// </summary>
    Task<AuditRetentionPolicyDto> UpdateRetentionPolicyAsync(AuditRetentionPolicyDto policy);

    #endregion

    #region Alerts

    /// <summary>
    /// Get alert configurations
    /// </summary>
    Task<List<AuditAlertConfigDto>> GetAlertConfigsAsync();

    /// <summary>
    /// Create/update alert configuration
    /// </summary>
    Task<AuditAlertConfigDto> SaveAlertConfigAsync(AuditAlertConfigDto config);

    /// <summary>
    /// Process alerts (called by background job)
    /// </summary>
    Task ProcessAlertsAsync();

    #endregion

    #region Export

    /// <summary>
    /// Export audit logs for compliance review
    /// </summary>
    Task<byte[]> ExportLogsAsync(AuditLogQueryDto query, string format = "CSV");

    /// <summary>
    /// Generate compliance report
    /// </summary>
    Task<byte[]> GenerateComplianceReportAsync(DateTime startDate, DateTime endDate, int branchId);

    #endregion
}

/// <summary>
/// Audit log entry for creating new logs
/// </summary>
public class AuditLogEntry
{
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
    public object? OldValues { get; set; }
    public object? NewValues { get; set; }
    public List<string>? AffectedFields { get; set; }
    public string? Reason { get; set; }
    public bool IsEmergencyAccess { get; set; }
    public string? EmergencyJustification { get; set; }
    public int BranchId { get; set; }
    public string? CorrelationId { get; set; }
    public long? DurationMs { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}
