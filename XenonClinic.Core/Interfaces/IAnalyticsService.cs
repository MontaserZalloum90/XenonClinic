using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for advanced analytics and BI dashboards
/// </summary>
public interface IAnalyticsService
{
    #region Dashboard Management

    /// <summary>
    /// Get all dashboards for the current user
    /// </summary>
    Task<List<DashboardDto>> GetDashboardsAsync(int userId);

    /// <summary>
    /// Get dashboard by ID
    /// </summary>
    Task<DashboardDto?> GetDashboardByIdAsync(int dashboardId);

    /// <summary>
    /// Get default dashboard
    /// </summary>
    Task<DashboardDto?> GetDefaultDashboardAsync(int userId, int branchId);

    /// <summary>
    /// Create a new dashboard
    /// </summary>
    Task<DashboardDto> CreateDashboardAsync(SaveDashboardDto request, int userId);

    /// <summary>
    /// Update dashboard
    /// </summary>
    Task<DashboardDto?> UpdateDashboardAsync(int dashboardId, SaveDashboardDto request);

    /// <summary>
    /// Delete dashboard
    /// </summary>
    Task DeleteDashboardAsync(int dashboardId);

    /// <summary>
    /// Get dashboard data with widget values
    /// </summary>
    Task<DashboardDataDto> GetDashboardDataAsync(int dashboardId, int branchId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Duplicate dashboard
    /// </summary>
    Task<DashboardDto> DuplicateDashboardAsync(int dashboardId, string newName, int userId);

    /// <summary>
    /// Set default dashboard
    /// </summary>
    Task<bool> SetDefaultDashboardAsync(int dashboardId, int userId);

    #endregion

    #region Widget Management

    /// <summary>
    /// Add widget to dashboard
    /// </summary>
    Task<WidgetDto> AddWidgetAsync(int dashboardId, SaveWidgetDto widget);

    /// <summary>
    /// Update widget
    /// </summary>
    Task<WidgetDto?> UpdateWidgetAsync(int widgetId, SaveWidgetDto widget);

    /// <summary>
    /// Delete widget
    /// </summary>
    Task DeleteWidgetAsync(int widgetId);

    /// <summary>
    /// Get widget data
    /// </summary>
    Task<WidgetDataDto> GetWidgetDataAsync(int widgetId, int branchId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Refresh widget data
    /// </summary>
    Task<Dictionary<string, object>> RefreshWidgetAsync(int dashboardId, string widgetId);

    #endregion

    #region Healthcare Analytics

    /// <summary>
    /// Get patient analytics
    /// </summary>
    Task<PatientAnalyticsDto> GetPatientAnalyticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get appointment analytics
    /// </summary>
    Task<AppointmentAnalyticsDto> GetAppointmentAnalyticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get revenue analytics
    /// </summary>
    Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get claims analytics
    /// </summary>
    Task<ClaimsAnalyticsDto> GetClaimsAnalyticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get clinical quality metrics
    /// </summary>
    Task<ClinicalQualityMetricsDto> GetClinicalQualityMetricsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get operational efficiency metrics
    /// </summary>
    Task<OperationalEfficiencyDto> GetOperationalEfficiencyAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get clinical outcomes analytics
    /// </summary>
    Task<ClinicalOutcomesDto> GetClinicalOutcomesAsync(int branchId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get resource utilization analytics
    /// </summary>
    Task<ResourceUtilizationDto> GetResourceUtilizationAsync(int branchId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get KPI dashboard
    /// </summary>
    Task<KPIDashboardDto> GetKPIsAsync(int branchId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get metric history for trending
    /// </summary>
    Task<List<MetricDataPointDto>> GetMetricHistoryAsync(int branchId, string metricName, DateTime startDate, DateTime endDate, string granularity);

    #endregion

    #region Metrics & Time Series

    /// <summary>
    /// Get available metrics
    /// </summary>
    Task<List<AnalyticsMetricDto>> GetAvailableMetricsAsync();

    /// <summary>
    /// Get metric value
    /// </summary>
    Task<MetricValueDto> GetMetricValueAsync(string metricId, int branchId, DateTime? asOf = null);

    /// <summary>
    /// Get multiple metric values
    /// </summary>
    Task<List<MetricValueDto>> GetMetricValuesAsync(List<string> metricIds, int branchId, DateTime? asOf = null);

    /// <summary>
    /// Get time series data for a metric
    /// </summary>
    Task<TimeSeriesResultDto> GetTimeSeriesAsync(string metricId, int branchId, DateTime startDate, DateTime endDate, string granularity = "Day");

    /// <summary>
    /// Execute custom analytics query
    /// </summary>
    Task<List<Dictionary<string, object>>> ExecuteQueryAsync(AnalyticsQueryDto query, int branchId);

    #endregion

    #region Predictive Analytics

    /// <summary>
    /// Get demand forecast
    /// </summary>
    Task<DemandForecastDto> GetDemandForecastAsync(int branchId, DateTime forecastDate, int daysAhead = 7);

    /// <summary>
    /// Get patient risk scores
    /// </summary>
    Task<List<PatientRiskScoreDto>> GetPatientRiskScoresAsync(int branchId, string? riskCategory = null, int minScore = 0);

    /// <summary>
    /// Get patient risk score by patient ID
    /// </summary>
    Task<PatientRiskScoreDto?> GetPatientRiskScoreAsync(int patientId);

    /// <summary>
    /// Get no-show predictions for upcoming appointments
    /// </summary>
    Task<List<NoShowPredictionDto>> GetNoShowPredictionsAsync(int branchId, DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Get revenue prediction
    /// </summary>
    Task<PredictionResultDto> GetRevenuePredictionAsync(int branchId, DateTime targetDate);

    #endregion

    #region Benchmarks & Comparisons

    /// <summary>
    /// Get benchmark comparisons
    /// </summary>
    Task<List<BenchmarkComparisonDto>> GetBenchmarkComparisonsAsync(int branchId, List<string>? metricIds = null);

    /// <summary>
    /// Get period comparison
    /// </summary>
    Task<List<PeriodComparisonDto>> GetPeriodComparisonAsync(int branchId, string currentPeriod, string previousPeriod, List<string>? metricIds = null);

    /// <summary>
    /// Get provider comparison
    /// </summary>
    Task<List<ProviderComparisonDto>> GetProviderComparisonAsync(int branchId, List<string>? metricIds = null);

    /// <summary>
    /// Get location comparison
    /// </summary>
    Task<List<LocationComparisonDto>> GetLocationComparisonAsync(List<int>? branchIds = null, List<string>? metricIds = null);

    #endregion

    #region Alerts & Anomalies

    /// <summary>
    /// Get active alerts
    /// </summary>
    Task<List<AnalyticsAlertDto>> GetActiveAlertsAsync(int branchId);

    /// <summary>
    /// Acknowledge alert
    /// </summary>
    Task<bool> AcknowledgeAlertAsync(int alertId, int userId);

    /// <summary>
    /// Get alert rules
    /// </summary>
    Task<List<AlertRuleDto>> GetAlertRulesAsync(int branchId);

    /// <summary>
    /// Create alert rule
    /// </summary>
    Task<AlertRuleDto> CreateAlertRuleAsync(CreateAlertRuleDto request, int branchId, int userId);

    /// <summary>
    /// Update alert rule
    /// </summary>
    Task<AlertRuleDto> UpdateAlertRuleAsync(int ruleId, CreateAlertRuleDto request, int userId);

    /// <summary>
    /// Delete alert rule
    /// </summary>
    Task<bool> DeleteAlertRuleAsync(int ruleId, int userId);

    /// <summary>
    /// Get detected anomalies
    /// </summary>
    Task<List<AnomalyDto>> GetAnomaliesAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Mark anomaly as investigated
    /// </summary>
    Task<bool> MarkAnomalyInvestigatedAsync(int anomalyId, int userId);

    /// <summary>
    /// Configure analytics alert
    /// </summary>
    Task<AnalyticsAlertConfigDto> ConfigureAlertAsync(AnalyticsAlertConfigDto config);

    #endregion

    #region Export & Sharing

    /// <summary>
    /// Export dashboard
    /// </summary>
    Task<byte[]> ExportDashboardAsync(int dashboardId, int branchId, string format, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Export analytics data
    /// </summary>
    Task<byte[]> ExportAnalyticsDataAsync(int branchId, string reportType, string format, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Share dashboard
    /// </summary>
    Task<bool> ShareDashboardAsync(ShareDashboardDto request, int userId);

    /// <summary>
    /// Create share link
    /// </summary>
    Task<DashboardShareLinkDto> CreateShareLinkAsync(int dashboardId, int userId, DateTime? expiresAt = null, bool requiresAuth = true);

    /// <summary>
    /// Get dashboard by share token
    /// </summary>
    Task<DashboardDto?> GetDashboardByShareTokenAsync(string shareToken);

    #endregion

    #region Subscriptions

    /// <summary>
    /// Get user subscriptions
    /// </summary>
    Task<List<DashboardSubscriptionDto>> GetSubscriptionsAsync(int userId);

    /// <summary>
    /// Create subscription
    /// </summary>
    Task<DashboardSubscriptionDto> CreateSubscriptionAsync(CreateDashboardSubscriptionDto request, int userId);

    /// <summary>
    /// Update subscription
    /// </summary>
    Task<DashboardSubscriptionDto> UpdateSubscriptionAsync(int subscriptionId, CreateDashboardSubscriptionDto request, int userId);

    /// <summary>
    /// Delete subscription
    /// </summary>
    Task<bool> DeleteSubscriptionAsync(int subscriptionId, int userId);

    /// <summary>
    /// Process scheduled subscriptions (called by background job)
    /// </summary>
    Task ProcessScheduledSubscriptionsAsync();

    #endregion

    #region Date Ranges

    /// <summary>
    /// Get available date range presets
    /// </summary>
    Task<List<DateRangePresetDto>> GetDateRangePresetsAsync();

    #endregion
}
