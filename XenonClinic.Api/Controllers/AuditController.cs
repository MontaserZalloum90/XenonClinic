using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for HIPAA-compliant audit logging operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuditController : BaseApiController
{
    private readonly IAuditService _auditService;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditService auditService,
        ITenantContextAccessor tenantContext,
        ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    #region Query Operations

    /// <summary>
    /// Query audit logs with filters.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AuditLogView")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> QueryLogs([FromQuery] AuditLogQueryDto query)
    {
        if (!_tenantContext.IsCompanyAdmin)
        {
            query.BranchId = _tenantContext.BranchId;
        }

        var result = await _auditService.QueryLogsAsync(query);
        return ApiOk(result);
    }

    /// <summary>
    /// Get audit log by ID.
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize(Policy = "AuditLogView")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var log = await _auditService.GetByIdAsync(id);
        if (log == null)
        {
            return ApiNotFound("Audit log entry not found");
        }

        // Verify branch access
        if (!_tenantContext.IsCompanyAdmin && log.BranchId != _tenantContext.BranchId)
        {
            return ApiForbidden("Access denied to this audit log");
        }

        return ApiOk(log);
    }

    /// <summary>
    /// Get PHI access report.
    /// </summary>
    [HttpGet("phi-access-report")]
    [Authorize(Policy = "AuditLogView")]
    [ProducesResponseType(typeof(ApiResponse<PHIAccessReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPHIAccessReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var branchId = _tenantContext.IsCompanyAdmin ? null : _tenantContext.BranchId;
        var report = await _auditService.GetPHIAccessReportAsync(startDate, endDate, branchId);
        return ApiOk(report);
    }

    /// <summary>
    /// Get patient access history (HIPAA requirement).
    /// </summary>
    [HttpGet("patient/{patientId:int}/access-history")]
    [Authorize(Policy = "AuditLogView")]
    [ProducesResponseType(typeof(ApiResponse<PatientAccessHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientAccessHistory(
        int patientId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var history = await _auditService.GetPatientAccessHistoryAsync(patientId, startDate, endDate);
        return ApiOk(history);
    }

    /// <summary>
    /// Get user activity summary.
    /// </summary>
    [HttpGet("user-activity")]
    [Authorize(Policy = "AuditLogView")]
    [ProducesResponseType(typeof(ApiResponse<List<PHIAccessByUserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserActivitySummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var branchId = _tenantContext.IsCompanyAdmin ? null : _tenantContext.BranchId;
        var summary = await _auditService.GetUserActivitySummaryAsync(startDate, endDate, branchId);
        return ApiOk(summary);
    }

    /// <summary>
    /// Detect suspicious activities.
    /// </summary>
    [HttpGet("suspicious-activities")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<List<SuspiciousActivityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DetectSuspiciousActivities(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var activities = await _auditService.DetectSuspiciousActivitiesAsync(startDate, endDate);
        return ApiOk(activities);
    }

    #endregion

    #region Retention & Archival

    /// <summary>
    /// Get retention policies.
    /// </summary>
    [HttpGet("retention-policies")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<List<AuditRetentionPolicyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRetentionPolicies()
    {
        var policies = await _auditService.GetRetentionPoliciesAsync();
        return ApiOk(policies);
    }

    /// <summary>
    /// Update retention policy.
    /// </summary>
    [HttpPut("retention-policies")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<AuditRetentionPolicyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRetentionPolicy([FromBody] AuditRetentionPolicyDto policy)
    {
        var updated = await _auditService.UpdateRetentionPolicyAsync(policy);
        return ApiOk(updated);
    }

    /// <summary>
    /// Archive old logs.
    /// </summary>
    [HttpPost("archive")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ArchiveOldLogs(
        [FromQuery] DateTime beforeDate,
        [FromQuery]
        [Required(ErrorMessage = "Archive path is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Archive path must be between 1 and 500 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_\-/\\:.]+$", ErrorMessage = "Archive path contains invalid characters")]
        string archivePath)
    {
        // Additional validation to prevent path traversal
        if (archivePath.Contains("..") || archivePath.Contains("~"))
        {
            return ApiBadRequest("Archive path cannot contain path traversal sequences");
        }

        var count = await _auditService.ArchiveOldLogsAsync(beforeDate, archivePath);
        return ApiOk(count, $"Archived {count} audit log entries");
    }

    #endregion

    #region Alerts

    /// <summary>
    /// Get alert configurations.
    /// </summary>
    [HttpGet("alerts")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<List<AuditAlertConfigDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlertConfigs()
    {
        var configs = await _auditService.GetAlertConfigsAsync();
        return ApiOk(configs);
    }

    /// <summary>
    /// Create or update alert configuration.
    /// </summary>
    [HttpPost("alerts")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<AuditAlertConfigDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveAlertConfig([FromBody] AuditAlertConfigDto config)
    {
        var saved = await _auditService.SaveAlertConfigAsync(config);
        return ApiOk(saved);
    }

    #endregion

    #region Export

    /// <summary>
    /// Export audit logs.
    /// </summary>
    [HttpPost("export")]
    [Authorize(Policy = "AuditLogView")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportLogs(
        [FromBody] AuditLogQueryDto query,
        [FromQuery]
        [RegularExpression("^(CSV|JSON|csv|json)$", ErrorMessage = "Format must be 'CSV' or 'JSON'")]
        string format = "CSV")
    {
        if (!_tenantContext.IsCompanyAdmin)
        {
            query.BranchId = _tenantContext.BranchId;
        }

        var data = await _auditService.ExportLogsAsync(query, format);
        var contentType = format.ToUpperInvariant() == "CSV" ? "text/csv" : "application/json";
        var fileName = $"audit_logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{format.ToLowerInvariant()}";

        return File(data, contentType, fileName);
    }

    /// <summary>
    /// Generate compliance report.
    /// </summary>
    [HttpGet("compliance-report")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateComplianceReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var data = await _auditService.GenerateComplianceReportAsync(startDate, endDate, branchId);
        var fileName = $"compliance_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf";

        return File(data, "application/pdf", fileName);
    }

    #endregion
}
