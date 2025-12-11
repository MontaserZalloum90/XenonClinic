using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for backup and disaster recovery operations.
/// </summary>
[Authorize(Policy = "SystemAdmin")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BackupController : BaseApiController
{
    private readonly IBackupService _backupService;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<BackupController> _logger;

    public BackupController(
        IBackupService backupService,
        ITenantContextAccessor tenantContext,
        ICurrentUserContext userContext,
        ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _logger = logger;
    }

    #region Backup Operations

    /// <summary>
    /// Create a full backup.
    /// </summary>
    [HttpPost("full")]
    [ProducesResponseType(typeof(ApiResponse<BackupResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateFullBackup([FromQuery] string? description = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var result = await _backupService.CreateFullBackupAsync(branchId, description);

        _logger.LogInformation(
            "Full backup created: {BackupId}, Branch: {BranchId}, By User: {UserId}",
            result.BackupId, branchId, _userContext.UserId);

        return ApiOk(result);
    }

    /// <summary>
    /// Create an incremental backup.
    /// </summary>
    [HttpPost("incremental")]
    [ProducesResponseType(typeof(ApiResponse<BackupResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateIncrementalBackup()
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var result = await _backupService.CreateIncrementalBackupAsync(branchId);

        _logger.LogInformation(
            "Incremental backup created: {BackupId}, Branch: {BranchId}, By User: {UserId}",
            result.BackupId, branchId, _userContext.UserId);

        return ApiOk(result);
    }

    /// <summary>
    /// List available backups.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<BackupRecordDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBackups(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var branchId = _tenantContext.IsCompanyAdmin ? null : _tenantContext.BranchId;
        var backups = await _backupService.GetBackupsAsync(branchId, fromDate, toDate);
        return ApiOk(backups);
    }

    /// <summary>
    /// Get backup details.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<BackupRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBackup(int id)
    {
        var backup = await _backupService.GetBackupAsync(id);
        if (backup == null)
        {
            return ApiNotFound("Backup not found");
        }
        return ApiOk(backup);
    }

    /// <summary>
    /// Verify backup integrity.
    /// </summary>
    [HttpPost("{id:int}/verify")]
    [ProducesResponseType(typeof(ApiResponse<BackupVerificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyBackup(int id)
    {
        var result = await _backupService.VerifyBackupAsync(id);

        _logger.LogInformation(
            "Backup verified: {BackupId}, IsValid: {IsValid}, By User: {UserId}",
            id, result.IsValid, _userContext.UserId);

        return ApiOk(result);
    }

    #endregion

    #region Restore Operations

    /// <summary>
    /// Restore from backup.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(typeof(ApiResponse<RestoreResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RestoreFromBackup(int id, [FromBody] RestoreOptionsDto options)
    {
        _logger.LogWarning(
            "Restore initiated from Backup: {BackupId}, Options: {Options}, By User: {UserId}",
            id, options, _userContext.UserId);

        var result = await _backupService.RestoreFromBackupAsync(id, options);

        _logger.LogInformation(
            "Restore completed: {BackupId}, Success: {IsSuccess}, By User: {UserId}",
            id, result.IsSuccess, _userContext.UserId);

        return ApiOk(result);
    }

    /// <summary>
    /// Restore to a specific point in time.
    /// </summary>
    [HttpPost("restore/point-in-time")]
    [ProducesResponseType(typeof(ApiResponse<RestoreResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RestoreToPointInTime([FromQuery] DateTime targetTime)
    {
        var branchId = _tenantContext.BranchId ?? 0;

        _logger.LogWarning(
            "Point-in-time restore initiated: Branch: {BranchId}, TargetTime: {TargetTime}, By User: {UserId}",
            branchId, targetTime, _userContext.UserId);

        var result = await _backupService.RestoreToPointInTimeAsync(branchId, targetTime);

        _logger.LogInformation(
            "Point-in-time restore completed: Branch: {BranchId}, Success: {IsSuccess}",
            branchId, result.IsSuccess);

        return ApiOk(result);
    }

    #endregion

    #region Configuration

    /// <summary>
    /// Get backup configuration.
    /// </summary>
    [HttpGet("config")]
    [ProducesResponseType(typeof(ApiResponse<BackupConfigDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguration()
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var config = await _backupService.GetConfigurationAsync(branchId);
        return ApiOk(config);
    }

    /// <summary>
    /// Update backup configuration.
    /// </summary>
    [HttpPut("config")]
    [ProducesResponseType(typeof(ApiResponse<BackupConfigDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateConfiguration([FromBody] BackupConfigDto config)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        config.BranchId = branchId;

        var updated = await _backupService.UpdateConfigurationAsync(branchId, config);

        _logger.LogInformation(
            "Backup configuration updated: Branch: {BranchId}, By User: {UserId}",
            branchId, _userContext.UserId);

        return ApiOk(updated);
    }

    #endregion

    #region Maintenance

    /// <summary>
    /// Clean up old backups based on retention policy.
    /// </summary>
    [HttpPost("cleanup")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CleanupOldBackups([FromQuery] int retentionDays = 90)
    {
        var count = await _backupService.CleanupOldBackupsAsync(retentionDays);

        _logger.LogInformation(
            "Backup cleanup completed: {Count} backups removed, Retention: {RetentionDays} days, By User: {UserId}",
            count, retentionDays, _userContext.UserId);

        return ApiOk(count, $"Cleaned up {count} old backups");
    }

    #endregion
}
