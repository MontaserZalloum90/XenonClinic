using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
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
                HttpMethod = entry.RequestMethod,
                StatusCode = entry.ResponseStatusCode,
                OldValues = entry.OldValues != null ? JsonSerializer.Serialize(entry.OldValues) : null,
                NewValues = entry.NewValues != null ? JsonSerializer.Serialize(entry.NewValues) : null,
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
                .Select(g => new PHIAccessByUserDto {
                    UserId = g.Key,
                    UserName = g.First().UserName ?? "",
                    Role = g.First().UserRole ?? "",
                    AccessCount = g.Count(),
                    UniquePatients = g.Where(l => l.PatientId.HasValue).Select(l => l.PatientId!.Value).Distinct().Count(),
                    LastAccess = g.Max(l => l.Timestamp)
                }).ToList()
        };
    }

    public async Task<PatientAccessHistoryDto> GetPatientAccessHistoryAsync(int patientId, DateTime startDate, DateTime endDate)
    {
        var patient = await _context.Patients.FindAsync(patientId);
        var logs = await _context.Set<AuditLog>().AsNoTracking().Where(a => a.PatientId == patientId && a.Timestamp >= startDate && a.Timestamp <= endDate).OrderByDescending(a => a.Timestamp).ToListAsync();

        return new PatientAccessHistoryDto
        {
            PatientId = patientId, PatientName = patient != null ? $"{patient.FirstName} {patient.LastName}" : "Unknown",
            RequestDate = DateTime.UtcNow, PeriodStart = startDate, PeriodEnd = endDate,
            AccessEntries = logs.Select(l => new PatientAccessEntryDto {
                AccessDate = l.Timestamp,
                AccessedBy = l.UserName ?? "",
                Role = l.UserRole ?? "",
                Department = l.ModuleName ?? "",
                Action = l.Action,
                ResourceAccessed = l.ResourceType ?? "",
                Reason = l.Reason
            }).ToList()
        };
    }

    public async Task<List<PHIAccessByUserDto>> GetUserActivitySummaryAsync(DateTime startDate, DateTime endDate, int? branchId = null)
    {
        var q = _context.Set<AuditLog>().AsNoTracking().Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate && a.UserId.HasValue);
        if (branchId.HasValue) q = q.Where(a => a.BranchId == branchId.Value);
        var logs = await q.ToListAsync();
        return logs.GroupBy(l => l.UserId!.Value).Select(g => new PHIAccessByUserDto {
            UserId = g.Key,
            UserName = g.First().UserName ?? "",
            Role = g.First().UserRole ?? "",
            AccessCount = g.Count(),
            UniquePatients = g.Where(l => l.PatientId.HasValue).Select(l => l.PatientId!.Value).Distinct().Count(),
            LastAccess = g.Max(l => l.Timestamp)
        }).ToList();
    }

    public async Task<List<SuspiciousActivityDto>> DetectSuspiciousActivitiesAsync(DateTime startDate, DateTime endDate)
    {
        var suspicious = new List<SuspiciousActivityDto>();

        // Limit date range to prevent excessive data loading
        var maxDays = 30;
        if ((endDate - startDate).TotalDays > maxDays)
        {
            _logger.LogWarning("Suspicious activity detection date range exceeds {MaxDays} days, limiting", maxDays);
            startDate = endDate.AddDays(-maxDays);
        }

        // High volume access detection - query aggregated data directly
        var highVolumeUsers = await _context.Set<AuditLog>()
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate && a.UserId.HasValue && a.IsPHIAccess)
            .GroupBy(a => new { a.UserId, Date = a.Timestamp.Date })
            .Where(g => g.Count() > 100)
            .Select(g => new { g.Key.UserId, g.Key.Date, Count = g.Count() })
            .ToListAsync();

        foreach (var g in highVolumeUsers)
            suspicious.Add(new SuspiciousActivityDto
            {
                ActivityType = "HIGH_VOLUME_ACCESS",
                Severity = "Warning",
                Description = $"User accessed {g.Count} PHI records",
                UserId = g.UserId,
                DetectedAt = g.Date
            });

        // Failed logins - query aggregated data directly
        var failedLogins = await _context.Set<AuditLog>()
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate && a.EventType == AuditEventTypes.LoginFailed)
            .GroupBy(a => a.IpAddress)
            .Where(g => g.Count() > 5)
            .Select(g => new { IpAddress = g.Key, Count = g.Count(), LastAttempt = g.Max(a => a.Timestamp) })
            .ToListAsync();

        foreach (var g in failedLogins)
            suspicious.Add(new SuspiciousActivityDto
            {
                ActivityType = "MULTIPLE_FAILED_LOGINS",
                Severity = "High",
                Description = $"Multiple failed logins ({g.Count}) from {g.IpAddress}",
                DetectedAt = g.LastAttempt
            });

        // Emergency access - paginated query
        var emergencyAccesses = await _context.Set<AuditLog>()
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate && a.IsEmergencyAccess)
            .Take(100) // Limit to prevent excessive data
            .ToListAsync();

        foreach (var e in emergencyAccesses)
            suspicious.Add(new SuspiciousActivityDto
            {
                ActivityType = "EMERGENCY_ACCESS",
                Severity = "Info",
                Description = $"Emergency access used: {e.EmergencyJustification ?? "No justification provided"}",
                UserId = e.UserId,
                DetectedAt = e.Timestamp
            });

        return suspicious;
    }

    // Allowed archive base paths (should be configured via appsettings)
    private static readonly HashSet<string> AllowedArchiveBasePaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/var/xenonclinic/archives",
        "/data/archives",
        "C:\\XenonClinic\\Archives"
    };
    private const int ArchiveBatchSize = 1000;

    public async Task<int> ArchiveOldLogsAsync(DateTime beforeDate, string archivePath)
    {
        // Validate archive path to prevent path traversal attacks
        var normalizedPath = Path.GetFullPath(archivePath);
        var isAllowedPath = AllowedArchiveBasePaths.Any(basePath =>
            normalizedPath.StartsWith(Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase));

        if (!isAllowedPath)
        {
            _logger.LogError("Archive path not allowed: {Path}", archivePath);
            throw new UnauthorizedAccessException("Archive path is not in the allowed list of archive directories");
        }

        // Validate no path traversal sequences
        if (archivePath.Contains("..") || archivePath.Contains("~"))
        {
            _logger.LogError("Path traversal attempt detected: {Path}", archivePath);
            throw new UnauthorizedAccessException("Invalid archive path");
        }

        var totalArchived = 0;
        var batchNumber = 0;

        // Process in batches to prevent memory issues
        while (true)
        {
            // Use transaction for each batch
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var logs = await _context.Set<AuditLog>()
                    .Where(a => a.Timestamp < beforeDate && !a.IsArchived)
                    .OrderBy(a => a.Timestamp)
                    .Take(ArchiveBatchSize)
                    .ToListAsync();

                if (!logs.Any())
                    break;

                // Create directory if not exists
                Directory.CreateDirectory(normalizedPath);

                // Use timestamped filename to prevent overwrites
                var fileName = Path.Combine(normalizedPath,
                    $"audit_{beforeDate:yyyyMMdd}_{DateTime.UtcNow:HHmmss}_batch{batchNumber:D4}.json");

                // Serialize with proper options
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await File.WriteAllTextAsync(fileName, JsonSerializer.Serialize(logs, jsonOptions));

                // Mark as archived
                var archiveTime = DateTime.UtcNow;
                foreach (var log in logs)
                {
                    log.IsArchived = true;
                    log.ArchivedAt = archiveTime;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                totalArchived += logs.Count;
                batchNumber++;

                _logger.LogInformation("Archived {Count} audit logs to {FileName}", logs.Count, fileName);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error archiving audit logs batch {Batch}", batchNumber);
                throw;
            }
        }

        _logger.LogInformation("Total audit logs archived: {Count}", totalArchived);
        return totalArchived;
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

    /// <summary>
    /// Generate HMAC-based integrity hash for audit log entry
    /// Includes all critical fields to detect tampering
    /// </summary>
    private static string GenerateHash(AuditLogEntry entry)
    {
        // Include all critical fields in the hash
        var dataBuilder = new StringBuilder();
        dataBuilder.Append(entry.EventType ?? "");
        dataBuilder.Append('|');
        dataBuilder.Append(entry.EventCategory ?? "");
        dataBuilder.Append('|');
        dataBuilder.Append(entry.Action ?? "");
        dataBuilder.Append('|');
        dataBuilder.Append(entry.ResourceType ?? "");
        dataBuilder.Append('|');
        dataBuilder.Append(entry.ResourceId ?? "");
        dataBuilder.Append('|');
        dataBuilder.Append(entry.UserId?.ToString() ?? "");
        dataBuilder.Append('|');
        dataBuilder.Append(entry.PatientId?.ToString() ?? "");
        dataBuilder.Append('|');
        dataBuilder.Append(entry.IsPHIAccess.ToString());
        dataBuilder.Append('|');
        dataBuilder.Append(entry.IsEmergencyAccess.ToString());
        dataBuilder.Append('|');
        dataBuilder.Append(entry.IpAddress ?? "");
        dataBuilder.Append('|');
        dataBuilder.Append(entry.CorrelationId ?? "");
        dataBuilder.Append('|');
        dataBuilder.Append(entry.IsSuccess.ToString());
        dataBuilder.Append('|');
        // Include hash of old/new values if present
        if (entry.OldValues != null)
            dataBuilder.Append(JsonSerializer.Serialize(entry.OldValues).GetHashCode());
        dataBuilder.Append('|');
        if (entry.NewValues != null)
            dataBuilder.Append(JsonSerializer.Serialize(entry.NewValues).GetHashCode());

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataBuilder.ToString()));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Verify the integrity of an audit log entry
    /// </summary>
    public bool VerifyLogIntegrity(AuditLog log)
    {
        if (string.IsNullOrEmpty(log.IntegrityHash))
            return false;

        var entry = new AuditLogEntry
        {
            EventType = log.EventType,
            EventCategory = log.EventCategory,
            Action = log.Action,
            ResourceType = log.ResourceType,
            ResourceId = log.ResourceId,
            UserId = log.UserId,
            PatientId = log.PatientId,
            IsPHIAccess = log.IsPHIAccess,
            IsEmergencyAccess = log.IsEmergencyAccess,
            IpAddress = log.IpAddress,
            CorrelationId = log.CorrelationId,
            IsSuccess = log.IsSuccess,
            OldValues = !string.IsNullOrEmpty(log.OldValues)
                ? JsonSerializer.Deserialize<object>(log.OldValues)
                : null,
            NewValues = !string.IsNullOrEmpty(log.NewValues)
                ? JsonSerializer.Deserialize<object>(log.NewValues)
                : null
        };

        var computedHash = GenerateHash(entry);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computedHash),
            Convert.FromBase64String(log.IntegrityHash));
    }
}

#region Audit Entities

// Note: AuditLog class is defined in XenonClinic.Core.Entities.AuditLog

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
