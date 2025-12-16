using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for analytics and business intelligence.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalyticsController : BaseApiController
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ITenantContextAccessor tenantContext,
        ICurrentUserContext userContext,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _logger = logger;
    }

    #region Dashboards

    /// <summary>
    /// Get available dashboards.
    /// </summary>
    [HttpGet("dashboards")]
    [ProducesResponseType(typeof(ApiResponse<List<DashboardDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboards()
    {
        var userId = int.TryParse(_userContext.UserId, out var parsedUserId) ? parsedUserId : 0;
        var dashboards = await _analyticsService.GetDashboardsAsync(userId);
        return ApiOk(dashboards);
    }

    /// <summary>
    /// Get dashboard by ID.
    /// </summary>
    [HttpGet("dashboards/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDashboard(int id)
    {
        var dashboard = await _analyticsService.GetDashboardByIdAsync(id);
        if (dashboard == null)
        {
            return ApiNotFound("Dashboard not found");
        }
        return ApiOk(dashboard);
    }

    /// <summary>
    /// Create a new dashboard.
    /// </summary>
    [HttpPost("dashboards")]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDashboard([FromBody] SaveDashboardDto dto)
    {
        var userId = int.TryParse(_userContext.UserId, out var parsedUserId) ? parsedUserId : 0;
        var dashboard = await _analyticsService.CreateDashboardAsync(dto, userId);
        return ApiCreated(dashboard, $"/api/analytics/dashboards/{dashboard.Id}");
    }

    /// <summary>
    /// Update a dashboard.
    /// </summary>
    [HttpPut("dashboards/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDashboard(int id, [FromBody] SaveDashboardDto dto)
    {
        var dashboard = await _analyticsService.UpdateDashboardAsync(id, dto);
        if (dashboard == null)
        {
            return ApiNotFound("Dashboard not found");
        }
        return ApiOk(dashboard);
    }

    /// <summary>
    /// Delete a dashboard.
    /// </summary>
    [HttpDelete("dashboards/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteDashboard(int id)
    {
        await _analyticsService.DeleteDashboardAsync(id);
        return ApiOk("Dashboard deleted successfully");
    }

    /// <summary>
    /// Get dashboard data (widgets with data).
    /// </summary>
    [HttpGet("dashboards/{id:int}/data")]
    [ProducesResponseType(typeof(ApiResponse<DashboardDataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardData(
        int id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var data = await _analyticsService.GetDashboardDataAsync(id, branchId, startDate, endDate);
        return ApiOk(data);
    }

    #endregion

    #region Widgets

    /// <summary>
    /// Add widget to dashboard.
    /// </summary>
    [HttpPost("dashboards/{dashboardId:int}/widgets")]
    [ProducesResponseType(typeof(ApiResponse<WidgetDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddWidget(int dashboardId, [FromBody] SaveWidgetDto dto)
    {
        var widget = await _analyticsService.AddWidgetAsync(dashboardId, dto);
        return ApiCreated(widget, $"/api/analytics/widgets/{widget.Id}");
    }

    /// <summary>
    /// Update widget.
    /// </summary>
    [HttpPut("widgets/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<WidgetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateWidget(int id, [FromBody] SaveWidgetDto dto)
    {
        var widget = await _analyticsService.UpdateWidgetAsync(id, dto);
        if (widget == null)
        {
            return ApiNotFound("Widget not found");
        }
        return ApiOk(widget);
    }

    /// <summary>
    /// Delete widget.
    /// </summary>
    [HttpDelete("widgets/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteWidget(int id)
    {
        await _analyticsService.DeleteWidgetAsync(id);
        return ApiOk("Widget deleted successfully");
    }

    /// <summary>
    /// Get widget data.
    /// </summary>
    [HttpGet("widgets/{id:int}/data")]
    [ProducesResponseType(typeof(ApiResponse<WidgetDataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWidgetData(
        int id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var data = await _analyticsService.GetWidgetDataAsync(id, branchId, startDate, endDate);
        return ApiOk(data);
    }

    #endregion

    #region Healthcare Analytics

    /// <summary>
    /// Get patient analytics.
    /// </summary>
    [HttpGet("patients")]
    [Authorize(Policy = "ReportView")]
    [ProducesResponseType(typeof(ApiResponse<PatientAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var analytics = await _analyticsService.GetPatientAnalyticsAsync(
            branchId,
            startDate ?? DateTime.UtcNow.AddMonths(-1),
            endDate ?? DateTime.UtcNow);
        return ApiOk(analytics);
    }

    /// <summary>
    /// Get appointment analytics.
    /// </summary>
    [HttpGet("appointments")]
    [Authorize(Policy = "ReportView")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAppointmentAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var analytics = await _analyticsService.GetAppointmentAnalyticsAsync(
            branchId,
            startDate ?? DateTime.UtcNow.AddMonths(-1),
            endDate ?? DateTime.UtcNow);
        return ApiOk(analytics);
    }

    /// <summary>
    /// Get revenue analytics.
    /// </summary>
    [HttpGet("revenue")]
    [Authorize(Policy = "FinancialReportView")]
    [ProducesResponseType(typeof(ApiResponse<RevenueAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var analytics = await _analyticsService.GetRevenueAnalyticsAsync(
            branchId,
            startDate ?? DateTime.UtcNow.AddMonths(-1),
            endDate ?? DateTime.UtcNow);
        return ApiOk(analytics);
    }

    /// <summary>
    /// Get clinical outcomes analytics.
    /// </summary>
    [HttpGet("clinical-outcomes")]
    [Authorize(Policy = "ClinicalReportView")]
    [ProducesResponseType(typeof(ApiResponse<ClinicalOutcomesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClinicalOutcomes(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var analytics = await _analyticsService.GetClinicalOutcomesAsync(
            branchId,
            startDate ?? DateTime.UtcNow.AddMonths(-6),
            endDate ?? DateTime.UtcNow);
        return ApiOk(analytics);
    }

    /// <summary>
    /// Get resource utilization analytics.
    /// </summary>
    [HttpGet("resource-utilization")]
    [Authorize(Policy = "ReportView")]
    [ProducesResponseType(typeof(ApiResponse<ResourceUtilizationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResourceUtilization(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var analytics = await _analyticsService.GetResourceUtilizationAsync(
            branchId,
            startDate ?? DateTime.UtcNow.AddMonths(-1),
            endDate ?? DateTime.UtcNow);
        return ApiOk(analytics);
    }

    #endregion

    #region Predictive Analytics

    /// <summary>
    /// Get patient risk scores.
    /// </summary>
    [HttpGet("predictive/patient-risk")]
    [Authorize(Policy = "ClinicalReportView")]
    [ProducesResponseType(typeof(ApiResponse<List<PatientRiskScoreDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientRiskScores(
        [FromQuery] string? riskCategory = null,
        [FromQuery] int minScore = 0)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var scores = await _analyticsService.GetPatientRiskScoresAsync(branchId, riskCategory, minScore);
        return ApiOk(scores);
    }

    /// <summary>
    /// Get appointment no-show predictions.
    /// </summary>
    [HttpGet("predictive/no-show")]
    [Authorize(Policy = "ReportView")]
    [ProducesResponseType(typeof(ApiResponse<List<NoShowPredictionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNoShowPredictions(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var predictions = await _analyticsService.GetNoShowPredictionsAsync(
            branchId,
            fromDate ?? DateTime.UtcNow,
            toDate ?? DateTime.UtcNow.AddDays(7));
        return ApiOk(predictions);
    }

    /// <summary>
    /// Get demand forecast.
    /// </summary>
    [HttpGet("predictive/demand-forecast")]
    [Authorize(Policy = "ReportView")]
    [ProducesResponseType(typeof(ApiResponse<DemandForecastDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDemandForecast(
        [FromQuery] int daysAhead = 30)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var forecast = await _analyticsService.GetDemandForecastAsync(branchId, DateTime.UtcNow, daysAhead);
        return ApiOk(forecast);
    }

    #endregion

    #region KPIs & Metrics

    /// <summary>
    /// Get KPIs overview.
    /// </summary>
    [HttpGet("kpis")]
    [Authorize(Policy = "ReportView")]
    [ProducesResponseType(typeof(ApiResponse<KPIDashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKPIs(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var kpis = await _analyticsService.GetKPIsAsync(
            branchId,
            startDate ?? DateTime.UtcNow.AddMonths(-1),
            endDate ?? DateTime.UtcNow);
        return ApiOk(kpis);
    }

    /// <summary>
    /// Get metric history for trending.
    /// </summary>
    [HttpGet("metrics/{metricName}/history")]
    [Authorize(Policy = "ReportView")]
    [ProducesResponseType(typeof(ApiResponse<List<MetricDataPointDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricHistory(
        string metricName,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string granularity = "daily")
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var history = await _analyticsService.GetMetricHistoryAsync(
            branchId,
            metricName,
            startDate ?? DateTime.UtcNow.AddMonths(-3),
            endDate ?? DateTime.UtcNow,
            granularity);
        return ApiOk(history);
    }

    #endregion

    #region Alerts

    /// <summary>
    /// Get active analytics alerts.
    /// </summary>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(ApiResponse<List<AnalyticsAlertDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var alerts = await _analyticsService.GetActiveAlertsAsync(branchId);
        return ApiOk(alerts);
    }

    /// <summary>
    /// Configure analytics alert.
    /// </summary>
    [HttpPost("alerts")]
    [Authorize(Policy = "SettingsManage")]
    [ProducesResponseType(typeof(ApiResponse<AnalyticsAlertConfigDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> ConfigureAlert([FromBody] AnalyticsAlertConfigDto dto)
    {
        var config = await _analyticsService.ConfigureAlertAsync(dto);
        return ApiCreated(config, $"/api/analytics/alerts/{config.Id}");
    }

    /// <summary>
    /// Acknowledge an alert.
    /// </summary>
    [HttpPost("alerts/{id:int}/acknowledge")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcknowledgeAlert(int id)
    {
        var userId = int.TryParse(_userContext.UserId, out var parsedUserId) ? parsedUserId : 0;
        await _analyticsService.AcknowledgeAlertAsync(id, userId);
        return ApiOk("Alert acknowledged");
    }

    #endregion

    #region Export

    /// <summary>
    /// Export dashboard as PDF.
    /// </summary>
    [HttpGet("dashboards/{id:int}/export/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportDashboardPdf(
        int id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var data = await _analyticsService.ExportDashboardAsync(id, branchId, "PDF", startDate, endDate);
        return File(data, "application/pdf", $"dashboard_{id}_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    /// <summary>
    /// Export analytics data as Excel.
    /// </summary>
    [HttpGet("export/excel")]
    [Authorize(Policy = "ReportExport")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToExcel(
        [FromQuery] string reportType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var data = await _analyticsService.ExportAnalyticsDataAsync(
            branchId, reportType, "XLSX", startDate, endDate);
        return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{reportType}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    #endregion
}
