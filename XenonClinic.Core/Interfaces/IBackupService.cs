using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Backup and disaster recovery service interface
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Create a full backup of the database
    /// </summary>
    Task<BackupResultDto> CreateFullBackupAsync(int branchId, string? description = null);

    /// <summary>
    /// Create an incremental backup
    /// </summary>
    Task<BackupResultDto> CreateIncrementalBackupAsync(int branchId);

    /// <summary>
    /// Get list of available backups
    /// </summary>
    Task<List<BackupRecordDto>> GetBackupsAsync(int? branchId = null, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Get backup by ID
    /// </summary>
    Task<BackupRecordDto?> GetBackupAsync(int backupId);

    /// <summary>
    /// Verify backup integrity
    /// </summary>
    Task<BackupVerificationDto> VerifyBackupAsync(int backupId);

    /// <summary>
    /// Restore from backup
    /// </summary>
    Task<RestoreResultDto> RestoreFromBackupAsync(int backupId, RestoreOptionsDto options);

    /// <summary>
    /// Point-in-time restore
    /// </summary>
    Task<RestoreResultDto> RestoreToPointInTimeAsync(int branchId, DateTime targetTime);

    /// <summary>
    /// Delete old backups based on retention policy
    /// </summary>
    Task<int> CleanupOldBackupsAsync(int retentionDays);

    /// <summary>
    /// Get backup configuration
    /// </summary>
    Task<BackupConfigDto> GetConfigurationAsync(int branchId);

    /// <summary>
    /// Update backup configuration
    /// </summary>
    Task<BackupConfigDto> UpdateConfigurationAsync(int branchId, BackupConfigDto config);
}

/// <summary>
/// Backup result DTO
/// </summary>
public class BackupResultDto
{
    public int BackupId { get; set; }
    public string BackupType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Backup record DTO
/// </summary>
public class BackupRecordDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string BackupType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Backup verification result
/// </summary>
public class BackupVerificationDto
{
    public int BackupId { get; set; }
    public bool IsValid { get; set; }
    public bool ChecksumMatch { get; set; }
    public bool FileExists { get; set; }
    public bool IsReadable { get; set; }
    public DateTime VerifiedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Restore options
/// </summary>
public class RestoreOptionsDto
{
    public bool OverwriteExisting { get; set; }
    public bool RestoreToNewDatabase { get; set; }
    public string? TargetDatabaseName { get; set; }
    public bool IncludeAuditLogs { get; set; } = true;
    public List<string>? TablesToRestore { get; set; }
}

/// <summary>
/// Restore result
/// </summary>
public class RestoreResultDto
{
    public bool IsSuccess { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int TablesRestored { get; set; }
    public long RowsRestored { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Backup configuration
/// </summary>
public class BackupConfigDto
{
    public int BranchId { get; set; }
    public bool AutoBackupEnabled { get; set; }
    public string BackupSchedule { get; set; } = "0 2 * * *"; // Cron: 2 AM daily
    public int FullBackupIntervalDays { get; set; } = 7;
    public int IncrementalBackupIntervalHours { get; set; } = 6;
    public int RetentionDays { get; set; } = 90;
    public string BackupPath { get; set; } = string.Empty;
    public bool EncryptBackups { get; set; } = true;
    public bool CompressBackups { get; set; } = true;
}
