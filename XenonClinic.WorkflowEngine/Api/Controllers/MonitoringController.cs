namespace XenonClinic.WorkflowEngine.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// API controller for workflow monitoring and analytics.
/// </summary>
[ApiController]
[Route("api/workflow/monitoring")]
[Authorize]
public class MonitoringController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        IMonitoringService monitoringService,
        ILogger<MonitoringController> logger)
    {
        _monitoringService = monitoringService;
        _logger = logger;
    }

    /// <summary>
    /// Gets dashboard summary statistics.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardSummaryDto), 200)]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _monitoringService.GetDashboardSummaryAsync(tenantId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets process execution statistics.
    /// </summary>
    [HttpGet("processes/{processDefinitionId}/statistics")]
    [ProducesResponseType(typeof(ProcessStatisticsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProcessStatistics(
        string processDefinitionId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenantId = GetTenantId();
            var result = await _monitoringService.GetProcessStatisticsAsync(
                processDefinitionId, tenantId, from, to, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets task statistics.
    /// </summary>
    [HttpGet("tasks/statistics")]
    [ProducesResponseType(typeof(TaskStatisticsDto), 200)]
    public async Task<IActionResult> GetTaskStatistics(
        [FromQuery] string? processDefinitionId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _monitoringService.GetTaskStatisticsAsync(
            tenantId, processDefinitionId, from, to, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets activity heatmap data for a process.
    /// </summary>
    [HttpGet("processes/{processDefinitionId}/heatmap")]
    [ProducesResponseType(typeof(ActivityHeatmapDto), 200)]
    public async Task<IActionResult> GetActivityHeatmap(
        string processDefinitionId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _monitoringService.GetActivityHeatmapAsync(
            processDefinitionId, tenantId, from, to, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets SLA compliance metrics.
    /// </summary>
    [HttpGet("sla")]
    [ProducesResponseType(typeof(SlaMetricsDto), 200)]
    public async Task<IActionResult> GetSlaMetrics(
        [FromQuery] string? processDefinitionId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _monitoringService.GetSlaMetricsAsync(
            tenantId, processDefinitionId, from, to, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets user performance metrics.
    /// </summary>
    [HttpGet("users/{userId}/performance")]
    [ProducesResponseType(typeof(UserPerformanceDto), 200)]
    public async Task<IActionResult> GetUserPerformance(
        string userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var tenantId = GetTenantId();
        var result = await _monitoringService.GetUserPerformanceAsync(
            userId, tenantId, from, to, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets system health status.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SystemHealthDto), 200)]
    public async Task<IActionResult> GetSystemHealth(CancellationToken cancellationToken = default)
    {
        var result = await _monitoringService.GetSystemHealthAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets recent errors and incidents.
    /// </summary>
    [HttpGet("incidents")]
    [ProducesResponseType(typeof(IList<IncidentDto>), 200)]
    public async Task<IActionResult> GetRecentIncidents(
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        // Validate and clamp limit to reasonable range
        limit = Math.Clamp(limit, 1, 100);
        var tenantId = GetTenantId();
        var result = await _monitoringService.GetRecentIncidentsAsync(tenantId, limit, cancellationToken);
        return Ok(result);
    }

    private int GetTenantId()
    {
        var claim = User.FindFirst("tenant_id");
        if (claim != null && int.TryParse(claim.Value, out var tenantId))
            return tenantId;
        return 1;
    }
}
