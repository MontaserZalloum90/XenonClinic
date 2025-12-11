using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// HIPAA-compliant audit logging service implementation
/// </summary>
public class AuditService : IAuditService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ClinicDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(AuditLogEntry entry)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                EventType = entry.EventType,
                EventCategory = entry.EventCategory,
                Action = entry.Action,
                ResourceType = entry.ResourceType,
                ResourceId = entry.ResourceId,
                UserId = entry.UserId,
                UserName = entry.UserName,
                UserRole = entry.UserRole,
                PatientId = entry.PatientId,
                IsPHIAccess = entry.IsPHIAccess,
                IpAddress = entry.IpAddress,
                SessionId = entry.SessionId,
                RequestPath = entry.RequestPath,
                RequestMethod = entry.RequestMethod,
                ResponseStatusCode = entry.ResponseStatusCode,
                OldValuesJson = entry.OldValues != null ? JsonSerializer.Serialize(entry.OldValues) : null,
                NewValuesJson = entry.NewValues != null ? JsonSerializer.Serialize(entry.NewValues) : null,
                Reason = entry.Reason,
                IsEmergencyAccess = entry.IsEmergencyAccess,
                EmergencyJustification = entry.EmergencyJustification,
                BranchId = entry.BranchId,
                CorrelationId = entry.CorrelationId ?? Guid.NewGuid().ToString(),
                DurationMs = entry.DurationMs,
                IsSuccess = entry.IsSuccess,
                ErrorMessage = entry.ErrorMessage,
                IntegrityHash = GenerateHash(entry)
            };

            _context.Set<AuditLog>().Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log: {EventType}", entry.EventType);
        }
    }

    public async Task LogPHIAccessAsync(int userId, int patientId, string resourceType, string action, string? resourceId = null, string? reason = null)
    {
        await LogAsync(new AuditLogEntry
        {
            EventType = AuditEventTypes.PHIAccess,
            EventCategory = AuditEventCategories.PatientData,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            UserId = userId,
            PatientId = patientId,
            IsPHIAccess = true,
            Reason = reason,
            IsSuccess = true
        });
    }

    public async Task LogEmergencyAccessAsync(int userId, int patientId, string resourceType, string justification)
    {
        await LogAsync(new AuditLogEntry
        {
            EventType = AuditEventTypes.EmergencyAccess,
            EventCategory = AuditEventCategories.PatientData,
            Action = "EMERGENCY_READ",
            ResourceType = resourceType,
            UserId = userId,
            PatientId = patientId,
            IsPHIAccess = true,
            IsEmergencyAccess = true,
            EmergencyJustification = justification,
            IsSuccess = true
        });
        _logger.LogWarning("Emergency access by user {UserId} to patient {PatientId}", userId, patientId);
    }

    public async Task LogAuthenticationAsync(string eventType, int? userId, string? userName, bool isSuccess, string? ipAddress = null, string? errorMessage = null)
    {
        await LogAsync(new AuditLogEntry
        {
            EventType = eventType,
            EventCategory = AuditEventCategories.Authentication,
            Action = eventType,
            ResourceType = "USER_SESSION",
            UserId = userId,
            UserName = userName,
            IpAddress = ipAddress,
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage
        });
    }

    public async Task LogAuthorizationAsync(int userId, string resource, string action, bool isGranted, string? reason = null)
    {
        await LogAsync(new AuditLogEntry
        {
            EventType = isGranted ? AuditEventTypes.AccessGranted : AuditEventTypes.AccessDenied,
            EventCategory = AuditEventCategories.Authorization,
            Action = action,
            ResourceType = resource,
            UserId = userId,
            IsSuccess = isGranted,
            Reason = reason
        });
    }

    public async Task LogDataChangeAsync(int userId, string resourceType, string resourceId, string action, object? oldValue, object? newValue, string? reason = null)
    {
        await LogAsync(new AuditLogEntry
        {
            EventType = action switch { "CREATE" => AuditEventTypes.DataCreate, "UPDATE" => AuditEventTypes.DataUpdate, "DELETE" => AuditEventTypes.DataDelete, _ => AuditEventTypes.DataRead },
            EventCategory = AuditEventCategories.PatientData,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            UserId = userId,
            IsPHIAccess = true,
            OldValues = oldValue,
            NewValues = newValue,
            Reason = reason,
            IsSuccess = true
        });
    }

    public async Task LogSystemEventAsync(string eventType, string description, string? details = null)
    {
        await LogAsync(new AuditLogEntry
        {
            EventType = eventType,
            EventCategory = AuditEventCategories.System,
            Action = eventType,
            ResourceType = "SYSTEM",
            AdditionalData = new Dictionary<string, object> { ["description"] = description },
            IsSuccess = true
        });
    }

    public async Task LogIntegrationCallAsync(string serviceName, string operation, bool isSuccess, long durationMs, string? errorMessage = null)
    {
        await LogAsync(new AuditLogEntry
        {
            EventType = AuditEventTypes.IntegrationCall,
            EventCategory = AuditEventCategories.Integration,
            Action = operation,
            ResourceType = serviceName,
            DurationMs = durationMs,
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage
        });
    }

    public async Task<AuditLogResultDto> QueryLogsAsync(AuditLogQueryDto query)
    {
        var q = _context.Set<AuditLog>().AsQueryable();
        if (query.StartDate.HasValue) q = q.Where(a => a.Timestamp >= query.StartDate.Value);
        if (query.EndDate.HasValue) q = q.Where(a => a.Timestamp <= query.EndDate.Value);
        if (!string.IsNullOrEmpty(query.EventType)) q = q.Where(a => a.EventType == query.EventType);
        if (query.UserId.HasValue) q = q.Where(a => a.UserId == query.UserId.Value);
        if (query.PatientId.HasValue) q = q.Where(a => a.PatientId == query.PatientId.Value);
        if (query.IsPHIAccess.HasValue) q = q.Where(a => a.IsPHIAccess == query.IsPHIAccess.Value);
        if (query.BranchId.HasValue) q = q.Where(a => a.BranchId == query.BranchId.Value);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(a => a.Timestamp).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        return new AuditLogResultDto
        {
            Items = items.Select(l => new AuditLogDto
            {
                Id = l.Id, Timestamp = l.Timestamp, EventType = l.EventType, EventCategory = l.EventCategory,
                Action = l.Action, ResourceType = l.ResourceType, ResourceId = l.ResourceId, UserId = l.UserId,
                UserName = l.UserName, PatientId = l.PatientId, IsPHIAccess = l.IsPHIAccess, IpAddress = l.IpAddress,
                IsEmergencyAccess = l.IsEmergencyAccess, BranchId = l.BranchId, IsSuccess = l.IsSuccess
            }).ToList(),
            TotalCount = total, Page = query.Page, PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)total / query.PageSize)
        };
    }

    public async Task<AuditLogDto?> GetByIdAsync(long id)
    {
        var log = await _context.Set<AuditLog>().FindAsync(id);
        if (log == null) return null;
        return new AuditLogDto { Id = log.Id, Timestamp = log.Timestamp, EventType = log.EventType, Action = log.Action, ResourceType = log.ResourceType, UserId = log.UserId, PatientId = log.PatientId, IsPHIAccess = log.IsPHIAccess, IsSuccess = log.IsSuccess };
    }

    public async Task<PHIAccessReportDto> GetPHIAccessReportAsync(DateTime startDate, DateTime endDate, int? branchId = null)
    {
        var q = _context.Set<AuditLog>().Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate && a.IsPHIAccess);
        if (branchId.HasValue) q = q.Where(a => a.BranchId == branchId.Value);
        var logs = await q.ToListAsync();

        return new PHIAccessReportDto
        {
            ReportDate = DateTime.UtcNow, StartDate = startDate, EndDate = endDate,
            TotalPHIAccesses = logs.Count,
            UniquePatients = logs.Where(l => l.PatientId.HasValue).Select(l => l.PatientId!.Value).Distinct().Count(),
            UniqueUsers = logs.Where(l => l.UserId.HasValue).Select(l => l.UserId!.Value).Distinct().Count(),
            EmergencyAccesses = logs.Count(l => l.IsEmergencyAccess),
            UnauthorizedAttempts = logs.Count(l => !l.IsSuccess),
            AccessByUser = logs.Where(l => l.UserId.HasValue).GroupBy(l => l.UserId!.Value)
                .Select(g => new PHIAccessByUserDto { UserId = g.Key, UserName = g.First().UserName ?? "", AccessCount = g.Count(), LastAccess = g.Max(l => l.Timestamp) }).ToList()
        };
    }

    public async Task<PatientAccessHistoryDto> GetPatientAccessHistoryAsync(int patientId, DateTime startDate, DateTime endDate)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        var logs = await _context.Set<AuditLog>().Where(a => a.PatientId == patientId && a.Timestamp >= startDate && a.Timestamp <= endDate).OrderByDescending(a => a.Timestamp).ToListAsync();

        return new PatientAccessHistoryDto
        {
            PatientId = patientId, PatientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Unknown",
            RequestDate = DateTime.UtcNow, PeriodStart = startDate, PeriodEnd = endDate,
            AccessEntries = logs.Select(l => new PatientAccessEntryDto { AccessDate = l.Timestamp, AccessedBy = l.UserName ?? "", Role = l.UserRole ?? "", Action = l.Action, ResourceAccessed = l.ResourceType }).ToList()
        };
    }

    public async Task<List<PHIAccessByUserDto>> GetUserActivitySummaryAsync(DateTime startDate, DateTime endDate, int? branchId = null)
    {
        var q = _context.Set<AuditLog>().Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate && a.UserId.HasValue);
        if (branchId.HasValue) q = q.Where(a => a.BranchId == branchId.Value);
        var logs = await q.ToListAsync();
        return logs.GroupBy(l => l.UserId!.Value).Select(g => new PHIAccessByUserDto { UserId = g.Key, UserName = g.First().UserName ?? "", AccessCount = g.Count(), LastAccess = g.Max(l => l.Timestamp) }).ToList();
    }

    public async Task<List<SuspiciousActivityDto>> DetectSuspiciousActivitiesAsync(DateTime startDate, DateTime endDate)
    {
        var suspicious = new List<SuspiciousActivityDto>();
        var logs = await _context.Set<AuditLog>().Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate).ToListAsync();

        // High volume access detection
        var highVolume = logs.Where(l => l.UserId.HasValue && l.IsPHIAccess).GroupBy(l => new { l.UserId, Date = l.Timestamp.Date }).Where(g => g.Count() > 100);
        foreach (var g in highVolume)
            suspicious.Add(new SuspiciousActivityDto { ActivityType = "HIGH_VOLUME_ACCESS", Severity = "Warning", Description = $"User accessed {g.Count()} PHI records", UserId = g.Key.UserId, DetectedAt = g.Key.Date });

        // Failed logins
        var failedLogins = logs.Where(l => l.EventType == AuditEventTypes.LoginFailed).GroupBy(l => l.IpAddress).Where(g => g.Count() > 5);
        foreach (var g in failedLogins)
            suspicious.Add(new SuspiciousActivityDto { ActivityType = "MULTIPLE_FAILED_LOGINS", Severity = "High", Description = $"Multiple failed logins from {g.Key}", DetectedAt = g.Max(l => l.Timestamp) });

        // Emergency access
        foreach (var e in logs.Where(l => l.IsEmergencyAccess))
            suspicious.Add(new SuspiciousActivityDto { ActivityType = "EMERGENCY_ACCESS", Severity = "Info", Description = "Emergency access used", UserId = e.UserId, DetectedAt = e.Timestamp });

        return suspicious;
    }

    public async Task<int> ArchiveOldLogsAsync(DateTime beforeDate, string archivePath)
    {
        var logs = await _context.Set<AuditLog>().Where(a => a.Timestamp < beforeDate && !a.IsArchived).ToListAsync();
        if (!logs.Any()) return 0;

        Directory.CreateDirectory(archivePath);
        var fileName = Path.Combine(archivePath, $"audit_{beforeDate:yyyyMMdd}.json");
        await File.WriteAllTextAsync(fileName, JsonSerializer.Serialize(logs));

        foreach (var log in logs) { log.IsArchived = true; log.ArchivedAt = DateTime.UtcNow; }
        await _context.SaveChangesAsync();
        return logs.Count;
    }

    public async Task<int> PurgeArchivedLogsAsync(int retentionDays)
    {
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
        var logs = await _context.Set<AuditLog>().Where(a => a.IsArchived && a.ArchivedAt < cutoff).ToListAsync();
        _context.Set<AuditLog>().RemoveRange(logs);
        await _context.SaveChangesAsync();
        return logs.Count;
    }

    public async Task<List<AuditRetentionPolicyDto>> GetRetentionPoliciesAsync()
    {
        var policies = await _context.Set<AuditRetentionPolicy>().ToListAsync();
        return policies.Select(p => new AuditRetentionPolicyDto { Id = p.Id, EventCategory = p.EventCategory, RetentionDays = p.RetentionDays, ArchiveBeforeDelete = p.ArchiveBeforeDelete, IsActive = p.IsActive }).ToList();
    }

    public async Task<AuditRetentionPolicyDto> UpdateRetentionPolicyAsync(AuditRetentionPolicyDto policy)
    {
        var entity = await _context.Set<AuditRetentionPolicy>().FindAsync(policy.Id) ?? new AuditRetentionPolicy();
        entity.EventCategory = policy.EventCategory; entity.RetentionDays = policy.RetentionDays; entity.ArchiveBeforeDelete = policy.ArchiveBeforeDelete; entity.IsActive = policy.IsActive;
        if (entity.Id == 0) _context.Set<AuditRetentionPolicy>().Add(entity);
        await _context.SaveChangesAsync();
        policy.Id = entity.Id;
        return policy;
    }

    public async Task<List<AuditAlertConfigDto>> GetAlertConfigsAsync()
    {
        var configs = await _context.Set<AuditAlertConfig>().ToListAsync();
        return configs.Select(c => new AuditAlertConfigDto { Id = c.Id, AlertName = c.AlertName, EventType = c.EventType, Condition = c.Condition, Threshold = c.Threshold, Severity = c.Severity, IsActive = c.IsActive }).ToList();
    }

    public async Task<AuditAlertConfigDto> SaveAlertConfigAsync(AuditAlertConfigDto config)
    {
        var entity = config.Id > 0 ? await _context.Set<AuditAlertConfig>().FindAsync(config.Id) : null;
        if (entity == null) { entity = new AuditAlertConfig(); _context.Set<AuditAlertConfig>().Add(entity); }
        entity.AlertName = config.AlertName; entity.EventType = config.EventType; entity.Condition = config.Condition; entity.Threshold = config.Threshold; entity.Severity = config.Severity; entity.IsActive = config.IsActive;
        await _context.SaveChangesAsync();
        config.Id = entity.Id;
        return config;
    }

    public Task ProcessAlertsAsync() => Task.CompletedTask;

    public async Task<byte[]> ExportLogsAsync(AuditLogQueryDto query, string format = "CSV")
    {
        var result = await QueryLogsAsync(new AuditLogQueryDto { StartDate = query.StartDate, EndDate = query.EndDate, Page = 1, PageSize = 100000 });
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,EventType,Action,ResourceType,UserId,PatientId,IsPHIAccess,IsSuccess");
        foreach (var log in result.Items)
            sb.AppendLine($"{log.Timestamp:O},{log.EventType},{log.Action},{log.ResourceType},{log.UserId},{log.PatientId},{log.IsPHIAccess},{log.IsSuccess}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> GenerateComplianceReportAsync(DateTime startDate, DateTime endDate, int branchId)
    {
        var report = await GetPHIAccessReportAsync(startDate, endDate, branchId);
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static string GenerateHash(AuditLogEntry entry)
    {
        var data = $"{entry.EventType}|{entry.UserId}|{entry.ResourceType}|{DateTime.UtcNow:O}";
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }
}

#region Audit Entities

[Table("AuditLogs")]
public class AuditLog
{
    [Key] public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    [Required, MaxLength(50)] public string EventType { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string EventCategory { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string Action { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string ResourceType { get; set; } = string.Empty;
    [MaxLength(100)] public string? ResourceId { get; set; }
    public int? UserId { get; set; }
    [MaxLength(200)] public string? UserName { get; set; }
    [MaxLength(100)] public string? UserRole { get; set; }
    public int? PatientId { get; set; }
    public bool IsPHIAccess { get; set; }
    [MaxLength(50)] public string? IpAddress { get; set; }
    [MaxLength(100)] public string? SessionId { get; set; }
    [MaxLength(500)] public string? RequestPath { get; set; }
    [MaxLength(10)] public string? RequestMethod { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }
    public bool IsEmergencyAccess { get; set; }
    [MaxLength(2000)] public string? EmergencyJustification { get; set; }
    public int BranchId { get; set; }
    [MaxLength(100)] public string? CorrelationId { get; set; }
    public long? DurationMs { get; set; }
    public bool IsSuccess { get; set; }
    [MaxLength(2000)] public string? ErrorMessage { get; set; }
    [MaxLength(64)] public string? IntegrityHash { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
}

[Table("AuditRetentionPolicies")]
public class AuditRetentionPolicy
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string EventCategory { get; set; } = string.Empty;
    public int RetentionDays { get; set; } = 2555;
    public bool ArchiveBeforeDelete { get; set; } = true;
    [MaxLength(500)] public string? ArchiveLocation { get; set; }
    public bool IsActive { get; set; } = true;
}

[Table("AuditAlertConfigs")]
public class AuditAlertConfig
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)] public string AlertName { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string EventType { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string Condition { get; set; } = string.Empty;
    public int Threshold { get; set; }
    public int TimeWindowMinutes { get; set; } = 60;
    [Required, MaxLength(20)] public string Severity { get; set; } = "Warning";
    public string? NotifyEmailsJson { get; set; }
    public bool IsActive { get; set; } = true;
}

#endregion
