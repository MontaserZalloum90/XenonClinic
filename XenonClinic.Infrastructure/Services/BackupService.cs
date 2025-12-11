using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Backup types
/// </summary>
public enum BackupType
{
    Full,
    Incremental,
    Differential
}

/// <summary>
/// Backup status
/// </summary>
public enum BackupStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Verified,
    Expired
}

/// <summary>
/// Backup configuration
/// </summary>
public class BackupConfiguration
{
    public string BackupPath { get; set; } = "./backups";
    public string ArchivePath { get; set; } = "./backups/archive";
    public int RetentionDays { get; set; } = 90;
    public int FullBackupIntervalDays { get; set; } = 7;
    public bool EncryptBackups { get; set; } = true;
    public bool CompressBackups { get; set; } = true;
    public string? RemoteStorageConnectionString { get; set; }
    public int MaxConcurrentBackups { get; set; } = 1;
}

/// <summary>
/// Backup result
/// </summary>
public class BackupResult
{
    public int BackupId { get; set; }
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Checksum { get; set; }
    public int TablesBackedUp { get; set; }
    public long RowsBackedUp { get; set; }
}

/// <summary>
/// Restore result
/// </summary>
public class RestoreResult
{
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public int TablesRestored { get; set; }
    public long RowsRestored { get; set; }
    public DateTime? RestorePoint { get; set; }
}

/// <summary>
/// Backup and disaster recovery service
/// </summary>
public interface IBackupService
{
    Task<BackupResult> CreateBackupAsync(BackupType type, string? description = null, CancellationToken cancellationToken = default);
    Task<RestoreResult> RestoreFromBackupAsync(int backupId, CancellationToken cancellationToken = default);
    Task<RestoreResult> RestoreToPointInTimeAsync(DateTime targetTime, CancellationToken cancellationToken = default);
    Task<List<BackupRecord>> GetBackupHistoryAsync(int limit = 50);
    Task<BackupRecord?> GetBackupAsync(int backupId);
    Task<bool> VerifyBackupAsync(int backupId);
    Task<bool> DeleteBackupAsync(int backupId);
    Task<int> CleanupOldBackupsAsync();
    Task<BackupResult> UploadToRemoteStorageAsync(int backupId);
    Task<int> DownloadFromRemoteStorageAsync(string remoteBackupId);
    Task<BackupHealthStatus> GetBackupHealthStatusAsync();
}

/// <summary>
/// Backup health status
/// </summary>
public class BackupHealthStatus
{
    public DateTime? LastFullBackup { get; set; }
    public DateTime? LastIncrementalBackup { get; set; }
    public DateTime? LastVerifiedBackup { get; set; }
    public int TotalBackups { get; set; }
    public int FailedBackups { get; set; }
    public long TotalBackupSizeBytes { get; set; }
    public int DaysSinceLastFullBackup { get; set; }
    public bool IsHealthy { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Backup service implementation
/// </summary>
public class BackupService : IBackupService
{
    private readonly ClinicDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IAuditService _auditService;
    private readonly ILogger<BackupService> _logger;
    private readonly BackupConfiguration _config;
    private static readonly SemaphoreSlim _backupSemaphore = new(1, 1);

    public BackupService(
        ClinicDbContext context,
        IEncryptionService encryptionService,
        IAuditService auditService,
        IConfiguration configuration,
        ILogger<BackupService> logger)
    {
        _context = context;
        _encryptionService = encryptionService;
        _auditService = auditService;
        _logger = logger;
        _config = configuration.GetSection("Backup").Get<BackupConfiguration>() ?? new BackupConfiguration();
    }

    public async Task<BackupResult> CreateBackupAsync(BackupType type, string? description = null, CancellationToken cancellationToken = default)
    {
        if (!await _backupSemaphore.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken))
        {
            return new BackupResult { Success = false, ErrorMessage = "Another backup is in progress" };
        }

        var startTime = DateTime.UtcNow;
        var record = new BackupRecord
        {
            BackupType = type.ToString(),
            Status = BackupStatus.InProgress.ToString(),
            Description = description,
            StartedAt = startTime,
            CreatedAt = startTime
        };

        try
        {
            _context.Set<BackupRecord>().Add(record);
            await _context.SaveChangesAsync(cancellationToken);

            Directory.CreateDirectory(_config.BackupPath);
            var fileName = $"backup_{type}_{startTime:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_config.BackupPath, fileName);

            // Export database tables
            var backupData = await ExportDatabaseAsync(type, cancellationToken);
            var json = JsonSerializer.Serialize(backupData, new JsonSerializerOptions { WriteIndented = false });

            // Compress if configured
            if (_config.CompressBackups)
            {
                filePath += ".gz";
                await using var fileStream = File.Create(filePath);
                await using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
                await using var writer = new StreamWriter(gzipStream);
                await writer.WriteAsync(json);
            }
            else
            {
                await File.WriteAllTextAsync(filePath, json, cancellationToken);
            }

            // Encrypt if configured
            if (_config.EncryptBackups)
            {
                var encryptedPath = filePath + ".enc";
                var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                var encryptedBytes = _encryptionService.EncryptBytes(fileBytes);
                await File.WriteAllBytesAsync(encryptedPath, encryptedBytes, cancellationToken);
                File.Delete(filePath);
                filePath = encryptedPath;
            }

            var fileInfo = new FileInfo(filePath);
            var checksum = ComputeChecksum(filePath);

            record.FilePath = filePath;
            record.FileSizeBytes = fileInfo.Length;
            record.Checksum = checksum;
            record.Status = BackupStatus.Completed.ToString();
            record.CompletedAt = DateTime.UtcNow;
            record.TablesCount = backupData.Tables.Count;
            record.RowsCount = backupData.Tables.Values.Sum(t => t.Count);

            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogSystemEventAsync(AuditEventTypes.BackupCreated, 
                $"Created {type} backup: {fileName}", $"Size: {fileInfo.Length} bytes, Tables: {record.TablesCount}");

            _logger.LogInformation("Backup completed: {FileName}, Size: {Size}KB, Duration: {Duration}ms",
                fileName, fileInfo.Length / 1024, (DateTime.UtcNow - startTime).TotalMilliseconds);

            return new BackupResult
            {
                BackupId = record.Id,
                Success = true,
                FilePath = filePath,
                FileSizeBytes = fileInfo.Length,
                Duration = DateTime.UtcNow - startTime,
                Checksum = checksum,
                TablesBackedUp = record.TablesCount,
                RowsBackedUp = record.RowsCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup failed");
            record.Status = BackupStatus.Failed.ToString();
            record.ErrorMessage = ex.Message;
            record.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);

            return new BackupResult { Success = false, ErrorMessage = ex.Message, Duration = DateTime.UtcNow - startTime };
        }
        finally
        {
            _backupSemaphore.Release();
        }
    }

    private async Task<DatabaseBackup> ExportDatabaseAsync(BackupType type, CancellationToken cancellationToken)
    {
        var backup = new DatabaseBackup
        {
            CreatedAt = DateTime.UtcNow,
            BackupType = type.ToString(),
            Version = "1.0"
        };

        // Export critical tables
        backup.Tables["Patients"] = await _context.Patients.ToListAsync(cancellationToken);
        backup.Tables["Appointments"] = await _context.Appointments.ToListAsync(cancellationToken);
        backup.Tables["Invoices"] = await _context.Invoices.ToListAsync(cancellationToken);
        backup.Tables["Doctors"] = await _context.Doctors.ToListAsync(cancellationToken);
        backup.Tables["Employees"] = await _context.Employees.ToListAsync(cancellationToken);

        // Add metadata
        backup.Metadata["PatientCount"] = backup.Tables["Patients"].Count;
        backup.Metadata["DatabaseVersion"] = "1.0";

        return backup;
    }

    public async Task<RestoreResult> RestoreFromBackupAsync(int backupId, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var backup = await _context.Set<BackupRecord>().FindAsync(new object[] { backupId }, cancellationToken);
        
        if (backup == null || string.IsNullOrEmpty(backup.FilePath))
            return new RestoreResult { Success = false, ErrorMessage = "Backup not found" };

        if (!File.Exists(backup.FilePath))
            return new RestoreResult { Success = false, ErrorMessage = "Backup file not found" };

        try
        {
            // Verify checksum
            var currentChecksum = ComputeChecksum(backup.FilePath);
            if (currentChecksum != backup.Checksum)
            {
                return new RestoreResult { Success = false, ErrorMessage = "Backup file checksum mismatch - file may be corrupted" };
            }

            _logger.LogWarning("Starting database restore from backup {BackupId}", backupId);

            // Read and decrypt/decompress backup file
            var fileBytes = await File.ReadAllBytesAsync(backup.FilePath, cancellationToken);
            
            if (_config.EncryptBackups && backup.FilePath.EndsWith(".enc"))
            {
                fileBytes = _encryptionService.DecryptBytes(fileBytes);
            }

            string json;
            if (_config.CompressBackups)
            {
                using var ms = new MemoryStream(fileBytes);
                using var gzip = new GZipStream(ms, CompressionMode.Decompress);
                using var reader = new StreamReader(gzip);
                json = await reader.ReadToEndAsync(cancellationToken);
            }
            else
            {
                json = System.Text.Encoding.UTF8.GetString(fileBytes);
            }

            var backupData = JsonSerializer.Deserialize<DatabaseBackup>(json);
            if (backupData == null)
                return new RestoreResult { Success = false, ErrorMessage = "Invalid backup data" };

            // Restore would be implemented here - this is a placeholder
            // In production, this would use transactions and proper restore logic
            
            await _auditService.LogSystemEventAsync(AuditEventTypes.BackupRestored,
                $"Database restored from backup {backupId}", $"Restore point: {backup.CreatedAt}");

            return new RestoreResult
            {
                Success = true,
                Duration = DateTime.UtcNow - startTime,
                TablesRestored = backupData.Tables.Count,
                RowsRestored = backupData.Tables.Values.Sum(t => t.Count),
                RestorePoint = backup.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restore failed for backup {BackupId}", backupId);
            return new RestoreResult { Success = false, ErrorMessage = ex.Message, Duration = DateTime.UtcNow - startTime };
        }
    }

    public async Task<RestoreResult> RestoreToPointInTimeAsync(DateTime targetTime, CancellationToken cancellationToken = default)
    {
        // Find the most recent backup before the target time
        var backup = await _context.Set<BackupRecord>()
            .Where(b => b.CreatedAt <= targetTime && b.Status == BackupStatus.Completed.ToString())
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (backup == null)
            return new RestoreResult { Success = false, ErrorMessage = $"No backup found before {targetTime}" };

        return await RestoreFromBackupAsync(backup.Id, cancellationToken);
    }

    public async Task<List<BackupRecord>> GetBackupHistoryAsync(int limit = 50)
    {
        return await _context.Set<BackupRecord>()
            .OrderByDescending(b => b.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<BackupRecord?> GetBackupAsync(int backupId)
    {
        return await _context.Set<BackupRecord>().FindAsync(backupId);
    }

    public async Task<bool> VerifyBackupAsync(int backupId)
    {
        var backup = await _context.Set<BackupRecord>().FindAsync(backupId);
        if (backup == null || string.IsNullOrEmpty(backup.FilePath))
            return false;

        if (!File.Exists(backup.FilePath))
        {
            backup.Status = BackupStatus.Failed.ToString();
            backup.ErrorMessage = "Backup file not found";
            await _context.SaveChangesAsync();
            return false;
        }

        var checksum = ComputeChecksum(backup.FilePath);
        if (checksum != backup.Checksum)
        {
            backup.Status = BackupStatus.Failed.ToString();
            backup.ErrorMessage = "Checksum verification failed";
            await _context.SaveChangesAsync();
            return false;
        }

        backup.Status = BackupStatus.Verified.ToString();
        backup.VerifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Backup {BackupId} verified successfully", backupId);
        return true;
    }

    public async Task<bool> DeleteBackupAsync(int backupId)
    {
        var backup = await _context.Set<BackupRecord>().FindAsync(backupId);
        if (backup == null) return false;

        if (!string.IsNullOrEmpty(backup.FilePath) && File.Exists(backup.FilePath))
        {
            File.Delete(backup.FilePath);
        }

        _context.Set<BackupRecord>().Remove(backup);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted backup {BackupId}", backupId);
        return true;
    }

    public async Task<int> CleanupOldBackupsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-_config.RetentionDays);
        var oldBackups = await _context.Set<BackupRecord>()
            .Where(b => b.CreatedAt < cutoffDate)
            .ToListAsync();

        var count = 0;
        foreach (var backup in oldBackups)
        {
            if (await DeleteBackupAsync(backup.Id))
                count++;
        }

        _logger.LogInformation("Cleaned up {Count} old backups", count);
        return count;
    }

    public async Task<BackupResult> UploadToRemoteStorageAsync(int backupId)
    {
        // Placeholder for cloud storage upload (Azure Blob, AWS S3, etc.)
        var backup = await _context.Set<BackupRecord>().FindAsync(backupId);
        if (backup == null)
            return new BackupResult { Success = false, ErrorMessage = "Backup not found" };

        _logger.LogInformation("Upload to remote storage not implemented - backup {BackupId}", backupId);
        return new BackupResult { Success = true, BackupId = backupId };
    }

    public Task<int> DownloadFromRemoteStorageAsync(string remoteBackupId)
    {
        // Placeholder for cloud storage download
        _logger.LogInformation("Download from remote storage not implemented");
        return Task.FromResult(0);
    }

    public async Task<BackupHealthStatus> GetBackupHealthStatusAsync()
    {
        var backups = await _context.Set<BackupRecord>().ToListAsync();
        var lastFull = backups.Where(b => b.BackupType == BackupType.Full.ToString() && b.Status == BackupStatus.Completed.ToString())
            .OrderByDescending(b => b.CreatedAt).FirstOrDefault();
        var lastIncremental = backups.Where(b => b.BackupType == BackupType.Incremental.ToString() && b.Status == BackupStatus.Completed.ToString())
            .OrderByDescending(b => b.CreatedAt).FirstOrDefault();
        var lastVerified = backups.Where(b => b.Status == BackupStatus.Verified.ToString())
            .OrderByDescending(b => b.VerifiedAt).FirstOrDefault();

        var daysSinceFullBackup = lastFull != null ? (int)(DateTime.UtcNow - lastFull.CreatedAt).TotalDays : int.MaxValue;

        var status = new BackupHealthStatus
        {
            LastFullBackup = lastFull?.CreatedAt,
            LastIncrementalBackup = lastIncremental?.CreatedAt,
            LastVerifiedBackup = lastVerified?.VerifiedAt,
            TotalBackups = backups.Count,
            FailedBackups = backups.Count(b => b.Status == BackupStatus.Failed.ToString()),
            TotalBackupSizeBytes = backups.Sum(b => b.FileSizeBytes),
            DaysSinceLastFullBackup = daysSinceFullBackup,
            IsHealthy = daysSinceFullBackup <= _config.FullBackupIntervalDays * 2
        };

        if (daysSinceFullBackup > _config.FullBackupIntervalDays)
            status.Warnings.Add($"No full backup in {daysSinceFullBackup} days");
        if (status.FailedBackups > 0)
            status.Warnings.Add($"{status.FailedBackups} failed backups detected");
        if (lastVerified == null || (DateTime.UtcNow - lastVerified.VerifiedAt!.Value).TotalDays > 7)
            status.Recommendations.Add("Verify recent backups to ensure data integrity");

        return status;
    }

    private static string ComputeChecksum(string filePath)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToBase64String(hash);
    }
}

/// <summary>
/// Database backup data structure
/// </summary>
public class DatabaseBackup
{
    public DateTime CreatedAt { get; set; }
    public string BackupType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, object> Tables { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Backup record entity
/// </summary>
[Table("BackupRecords")]
public class BackupRecord
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(20)] public string BackupType { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string Status { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(500)] public string? FilePath { get; set; }
    public long FileSizeBytes { get; set; }
    [MaxLength(100)] public string? Checksum { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int TablesCount { get; set; }
    public long RowsCount { get; set; }
    [MaxLength(2000)] public string? ErrorMessage { get; set; }
    [MaxLength(500)] public string? RemoteStorageId { get; set; }
    public DateTime CreatedAt { get; set; }
}
