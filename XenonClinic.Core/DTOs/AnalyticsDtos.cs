namespace XenonClinic.Core.DTOs;

#region Dashboard DTOs

/// <summary>
/// BI Dashboard definition
/// </summary>
public class DashboardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsPublic { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public List<DashboardWidgetDto> Widgets { get; set; } = new();
    public DashboardLayoutDto? Layout { get; set; }
    public DashboardThemeDto? Theme { get; set; }
    public int RefreshIntervalSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Dashboard widget
/// </summary>
public class DashboardWidgetDto
{
    public int Id { get; set; }
    public string WidgetId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string WidgetType { get; set; } = string.Empty; // KPI, Chart, Table, Metric, Gauge, Map, Timeline
    public string DataSource { get; set; } = string.Empty;
    public WidgetConfigurationDto Configuration { get; set; } = new();
    public WidgetPositionDto Position { get; set; } = new();
    public Dictionary<string, object>? Filters { get; set; }
    public int RefreshIntervalSeconds { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// Widget configuration
/// </summary>
public class WidgetConfigurationDto
{
    public string? ChartType { get; set; } // Bar, Line, Pie, Area, Doughnut, Scatter, Radar, Funnel
    public string? XAxisField { get; set; }
    public List<string>? YAxisFields { get; set; }
    public string? SeriesField { get; set; }
    public string? ValueField { get; set; }
    public string? LabelField { get; set; }
    public string? AggregateFunction { get; set; } // Sum, Avg, Count, Min, Max
    public string? TimeGranularity { get; set; } // Hour, Day, Week, Month, Quarter, Year
    public bool ShowLegend { get; set; } = true;
    public bool ShowLabels { get; set; } = true;
    public bool Stacked { get; set; }
    public string? ColorScheme { get; set; }
    public List<string>? CustomColors { get; set; }
    public KpiConfigurationDto? KpiConfig { get; set; }
    public GaugeConfigurationDto? GaugeConfig { get; set; }
    public TableConfigurationDto? TableConfig { get; set; }
}

/// <summary>
/// KPI specific configuration
/// </summary>
public class KpiConfigurationDto
{
    public string MetricName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? Format { get; set; }
    public string? ComparisonPeriod { get; set; } // PreviousDay, PreviousWeek, PreviousMonth, PreviousYear
    public bool ShowTrend { get; set; } = true;
    public bool ShowSparkline { get; set; }
    public decimal? TargetValue { get; set; }
    public List<KpiThresholdDto>? Thresholds { get; set; }
}

/// <summary>
/// KPI threshold for color coding
/// </summary>
public class KpiThresholdDto
{
    public decimal Value { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? Label { get; set; }
}

/// <summary>
/// Gauge specific configuration
/// </summary>
public class GaugeConfigurationDto
{
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public List<GaugeRangeDto> Ranges { get; set; } = new();
    public bool ShowValue { get; set; } = true;
    public string? Unit { get; set; }
}

/// <summary>
/// Gauge range definition
/// </summary>
public class GaugeRangeDto
{
    public decimal From { get; set; }
    public decimal To { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? Label { get; set; }
}

/// <summary>
/// Table widget configuration
/// </summary>
public class TableConfigurationDto
{
    public List<TableColumnConfigDto> Columns { get; set; } = new();
    public bool Paginated { get; set; } = true;
    public int PageSize { get; set; } = 10;
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = true;
    public string? DefaultSortField { get; set; }
    public string? DefaultSortDirection { get; set; }
}

/// <summary>
/// Table column configuration
/// </summary>
public class TableColumnConfigDto
{
    public string Field { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public string? Format { get; set; }
    public int Width { get; set; }
    public string? Alignment { get; set; }
    public bool Visible { get; set; } = true;
}

/// <summary>
/// Widget position in grid
/// </summary>
public class WidgetPositionDto
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 2;
    public int MinWidth { get; set; } = 2;
    public int MinHeight { get; set; } = 1;
}

/// <summary>
/// Dashboard layout settings
/// </summary>
public class DashboardLayoutDto
{
    public int GridColumns { get; set; } = 12;
    public int RowHeight { get; set; } = 100;
    public int Margin { get; set; } = 10;
    public bool IsDraggable { get; set; } = true;
    public bool IsResizable { get; set; } = true;
}

/// <summary>
/// Dashboard theme
/// </summary>
public class DashboardThemeDto
{
    public string PrimaryColor { get; set; } = "#1976D2";
    public string SecondaryColor { get; set; } = "#424242";
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public string TextColor { get; set; } = "#212121";
    public string FontFamily { get; set; } = "Roboto, sans-serif";
    public bool DarkMode { get; set; }
}

/// <summary>
/// Create dashboard request
/// </summary>
public class CreateDashboardDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsPublic { get; set; }
    public List<DashboardWidgetDto>? Widgets { get; set; }
    public DashboardLayoutDto? Layout { get; set; }
    public DashboardThemeDto? Theme { get; set; }
    public int RefreshIntervalSeconds { get; set; } = 300;
}

/// <summary>
/// Update dashboard request
/// </summary>
public class UpdateDashboardDto : CreateDashboardDto
{
}

#endregion

#region Analytics Metric DTOs

/// <summary>
/// Analytics metric definition
/// </summary>
public class AnalyticsMetricDto
{
    public string MetricId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = string.Empty; // Number, Currency, Percentage, Duration
    public string? Unit { get; set; }
    public string CalculationType { get; set; } = string.Empty; // Count, Sum, Average, Ratio, Custom
    public string? Formula { get; set; }
    public List<string>? AvailableDimensions { get; set; }
    public List<string>? AvailableFilters { get; set; }
}

/// <summary>
/// Metric value with trend
/// </summary>
public class MetricValueDto
{
    public string MetricId { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? FormattedValue { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal? ChangePercent { get; set; }
    public string? TrendDirection { get; set; } // Up, Down, Stable
    public bool IsPositiveTrend { get; set; }
    public DateTime AsOf { get; set; }
    public string? Period { get; set; }
}

/// <summary>
/// Time series data point
/// </summary>
public class TimeSeriesDataPointDto
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
    public string? Label { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Time series result
/// </summary>
public class TimeSeriesResultDto
{
    public string MetricId { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public string Granularity { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<TimeSeriesDataPointDto> DataPoints { get; set; } = new();
    public TimeSeriesStatsDto? Stats { get; set; }
}

/// <summary>
/// Time series statistics
/// </summary>
public class TimeSeriesStatsDto
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Average { get; set; }
    public decimal Sum { get; set; }
    public int Count { get; set; }
    public decimal? StandardDeviation { get; set; }
    public decimal? Trend { get; set; }
}

#endregion

#region Healthcare Analytics DTOs

/// <summary>
/// Patient analytics summary
/// </summary>
public class PatientAnalyticsDto
{
    public int TotalPatients { get; set; }
    public int NewPatientsThisMonth { get; set; }
    public int ActivePatients { get; set; }
    public decimal PatientRetentionRate { get; set; }
    public decimal AveragePatientAge { get; set; }
    public List<DemographicBreakdownDto> AgeDistribution { get; set; } = new();
    public List<DemographicBreakdownDto> GenderDistribution { get; set; } = new();
    public List<DemographicBreakdownDto> InsuranceDistribution { get; set; } = new();
    public List<TopConditionDto> TopConditions { get; set; } = new();
}

/// <summary>
/// Demographic breakdown
/// </summary>
public class DemographicBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Top condition/diagnosis
/// </summary>
public class TopConditionDto
{
    public string IcdCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PatientCount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Appointment analytics
/// </summary>
public class AppointmentAnalyticsDto
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int NoShowAppointments { get; set; }
    public decimal NoShowRate { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal AverageWaitTimeMinutes { get; set; }
    public decimal AverageAppointmentDurationMinutes { get; set; }
    public decimal ScheduleUtilizationRate { get; set; }
    public List<AppointmentsByTypeDto> ByType { get; set; } = new();
    public List<AppointmentsByProviderDto> ByProvider { get; set; } = new();
    public List<TimeSeriesDataPointDto> DailyTrend { get; set; } = new();
    public List<HourlyDistributionDto> HourlyDistribution { get; set; } = new();
}

/// <summary>
/// Appointments by type
/// </summary>
public class AppointmentsByTypeDto
{
    public string AppointmentType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public decimal AverageDuration { get; set; }
}

/// <summary>
/// Appointments by provider
/// </summary>
public class AppointmentsByProviderDto
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal UtilizationRate { get; set; }
    public decimal AverageRating { get; set; }
}

/// <summary>
/// Hourly distribution
/// </summary>
public class HourlyDistributionDto
{
    public int Hour { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Revenue analytics
/// </summary>
public class RevenueAnalyticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueGrowthPercent { get; set; }
    public decimal AverageRevenuePerPatient { get; set; }
    public decimal AverageRevenuePerVisit { get; set; }
    public decimal CollectionRate { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal DaysInAR { get; set; }
    public List<RevenueByServiceDto> ByService { get; set; } = new();
    public List<RevenueByPayerDto> ByPayer { get; set; } = new();
    public List<RevenueByProviderDto> ByProvider { get; set; } = new();
    public List<TimeSeriesDataPointDto> MonthlyTrend { get; set; } = new();
}

/// <summary>
/// Revenue by service
/// </summary>
public class RevenueByServiceDto
{
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Revenue by payer
/// </summary>
public class RevenueByPayerDto
{
    public string PayerName { get; set; } = string.Empty;
    public string PayerType { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
    public decimal CollectionRate { get; set; }
    public decimal AverageDaysToPayment { get; set; }
}

/// <summary>
/// Revenue by provider
/// </summary>
public class RevenueByProviderDto
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int PatientCount { get; set; }
    public decimal RevenuePerPatient { get; set; }
}

/// <summary>
/// Claims analytics
/// </summary>
public class ClaimsAnalyticsDto
{
    public int TotalClaims { get; set; }
    public int PendingClaims { get; set; }
    public int ApprovedClaims { get; set; }
    public int DeniedClaims { get; set; }
    public decimal DenialRate { get; set; }
    public decimal FirstPassResolutionRate { get; set; }
    public decimal AverageDaysToResolution { get; set; }
    public decimal TotalClaimedAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal AverageReimbursementRate { get; set; }
    public List<DenialReasonDto> TopDenialReasons { get; set; } = new();
    public List<ClaimsByStatusDto> ByStatus { get; set; } = new();
    public List<TimeSeriesDataPointDto> SubmissionTrend { get; set; } = new();
}

/// <summary>
/// Denial reason breakdown
/// </summary>
public class DenialReasonDto
{
    public string ReasonCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Claims by status
/// </summary>
public class ClaimsByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Clinical quality metrics
/// </summary>
public class ClinicalQualityMetricsDto
{
    public decimal PatientSatisfactionScore { get; set; }
    public decimal ReadmissionRate { get; set; }
    public decimal MedicationAdherenceRate { get; set; }
    public decimal PreventiveCareComplianceRate { get; set; }
    public decimal LabResultTurnaroundHours { get; set; }
    public decimal CriticalResultNotificationRate { get; set; }
    public List<QualityMeasureDto> QualityMeasures { get; set; } = new();
    public List<OutcomeMetricDto> OutcomeMetrics { get; set; } = new();
}

/// <summary>
/// Quality measure
/// </summary>
public class QualityMeasureDto
{
    public string MeasureId { get; set; } = string.Empty;
    public string MeasureName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal TargetValue { get; set; }
    public decimal NationalBenchmark { get; set; }
    public string PerformanceLevel { get; set; } = string.Empty; // Below, At, Above
}

/// <summary>
/// Outcome metric
/// </summary>
public class OutcomeMetricDto
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public decimal? Target { get; set; }
    public string? TrendDirection { get; set; }
}

/// <summary>
/// Operational efficiency metrics
/// </summary>
public class OperationalEfficiencyDto
{
    public decimal StaffUtilizationRate { get; set; }
    public decimal RoomUtilizationRate { get; set; }
    public decimal EquipmentUtilizationRate { get; set; }
    public decimal AverageCheckInTimeMinutes { get; set; }
    public decimal AverageCheckOutTimeMinutes { get; set; }
    public decimal PatientThroughputPerDay { get; set; }
    public int PeakHour { get; set; }
    public decimal OverTimeHoursPercent { get; set; }
    public List<ResourceUtilizationDto> ResourceUtilization { get; set; } = new();
    public List<BottleneckDto> Bottlenecks { get; set; } = new();
}

/// <summary>
/// Resource utilization
/// </summary>
public class ResourceUtilizationDto
{
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public decimal UtilizationRate { get; set; }
    public decimal AvailableHours { get; set; }
    public decimal UsedHours { get; set; }
}

/// <summary>
/// Bottleneck identification
/// </summary>
public class BottleneckDto
{
    public string Area { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Low, Medium, High
    public decimal ImpactScore { get; set; }
    public string? RecommendedAction { get; set; }
}

#endregion

#region Predictive Analytics DTOs

/// <summary>
/// Prediction result
/// </summary>
public class PredictionResultDto
{
    public string PredictionType { get; set; } = string.Empty;
    public decimal PredictedValue { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
    public DateTime PredictionDate { get; set; }
    public string? ModelVersion { get; set; }
    public List<PredictionFactorDto>? ContributingFactors { get; set; }
}

/// <summary>
/// Prediction contributing factor
/// </summary>
public class PredictionFactorDto
{
    public string FactorName { get; set; } = string.Empty;
    public decimal Impact { get; set; }
    public string Direction { get; set; } = string.Empty; // Positive, Negative
}

/// <summary>
/// Demand forecast
/// </summary>
public class DemandForecastDto
{
    public DateTime ForecastDate { get; set; }
    public string ForecastPeriod { get; set; } = string.Empty;
    public int PredictedAppointments { get; set; }
    public int PredictedEmergencyVisits { get; set; }
    public decimal PredictedRevenue { get; set; }
    public List<DepartmentForecastDto> ByDepartment { get; set; } = new();
    public List<TimeSeriesDataPointDto> HourlyDistribution { get; set; } = new();
}

/// <summary>
/// Department forecast
/// </summary>
public class DepartmentForecastDto
{
    public string Department { get; set; } = string.Empty;
    public int PredictedPatients { get; set; }
    public decimal PredictedUtilization { get; set; }
    public int RecommendedStaffCount { get; set; }
}

/// <summary>
/// Patient risk score
/// </summary>
public class PatientRiskScoreDto
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public decimal OverallRiskScore { get; set; }
    public string RiskCategory { get; set; } = string.Empty; // Low, Medium, High, Critical
    public List<RiskFactorDto> RiskFactors { get; set; } = new();
    public List<string>? RecommendedInterventions { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Risk factor
/// </summary>
public class RiskFactorDto
{
    public string FactorName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal Weight { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// No-show prediction
/// </summary>
public class NoShowPredictionDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public decimal NoShowProbability { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string>? RiskFactors { get; set; }
    public string? RecommendedAction { get; set; }
}

#endregion

#region Benchmark & Comparison DTOs

/// <summary>
/// Benchmark comparison
/// </summary>
public class BenchmarkComparisonDto
{
    public string MetricName { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal IndustryAverage { get; set; }
    public decimal TopPerformer { get; set; }
    public decimal Percentile { get; set; }
    public string PerformanceStatus { get; set; } = string.Empty; // Below, At, Above
    public decimal GapToTarget { get; set; }
}

/// <summary>
/// Period comparison
/// </summary>
public class PeriodComparisonDto
{
    public string MetricName { get; set; } = string.Empty;
    public string CurrentPeriod { get; set; } = string.Empty;
    public string PreviousPeriod { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
}

/// <summary>
/// Provider comparison
/// </summary>
public class ProviderComparisonDto
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public Dictionary<string, decimal> Metrics { get; set; } = new();
    public int Rank { get; set; }
    public decimal OverallScore { get; set; }
}

/// <summary>
/// Location comparison
/// </summary>
public class LocationComparisonDto
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public Dictionary<string, decimal> Metrics { get; set; } = new();
    public int Rank { get; set; }
    public decimal OverallScore { get; set; }
}

#endregion

#region Alert & Anomaly DTOs

/// <summary>
/// Analytics alert
/// </summary>
public class AnalyticsAlertDto
{
    public int Id { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Info, Warning, Critical
    public string MetricName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal ThresholdValue { get; set; }
    public DateTime DetectedAt { get; set; }
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}

/// <summary>
/// Alert rule definition
/// </summary>
public class AlertRuleDto
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string MetricId { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty; // GreaterThan, LessThan, Equals, PercentChange
    public decimal ThresholdValue { get; set; }
    public string Severity { get; set; } = string.Empty;
    public List<string> NotificationChannels { get; set; } = new();
    public List<string> NotifyUsers { get; set; } = new();
    public int CooldownMinutes { get; set; } = 60;
    public bool IsActive { get; set; }
}

/// <summary>
/// Create alert rule request
/// </summary>
public class CreateAlertRuleDto
{
    public string RuleName { get; set; } = string.Empty;
    public string MetricId { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public decimal ThresholdValue { get; set; }
    public string Severity { get; set; } = string.Empty;
    public List<string> NotificationChannels { get; set; } = new();
    public List<string> NotifyUsers { get; set; } = new();
    public int CooldownMinutes { get; set; } = 60;
}

/// <summary>
/// Anomaly detection result
/// </summary>
public class AnomalyDto
{
    public int Id { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public decimal ExpectedValue { get; set; }
    public decimal ActualValue { get; set; }
    public decimal DeviationPercent { get; set; }
    public string AnomalyType { get; set; } = string.Empty; // Spike, Drop, Trend Change
    public decimal ConfidenceScore { get; set; }
    public string? PossibleCause { get; set; }
    public bool IsInvestigated { get; set; }
}

#endregion

#region Query & Filter DTOs

/// <summary>
/// Analytics query request
/// </summary>
public class AnalyticsQueryDto
{
    public List<string> Metrics { get; set; } = new();
    public List<string>? Dimensions { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Granularity { get; set; }
    public List<AnalyticsFilterDto>? Filters { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int? Limit { get; set; }
}

/// <summary>
/// Analytics filter
/// </summary>
public class AnalyticsFilterDto
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public object? Value { get; set; }
}

/// <summary>
/// Date range preset
/// </summary>
public class DateRangePresetDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

#endregion

#region Export & Sharing DTOs

/// <summary>
/// Dashboard export request
/// </summary>
public class ExportDashboardDto
{
    public int DashboardId { get; set; }
    public string Format { get; set; } = string.Empty; // PDF, PNG, Excel
    public bool IncludeData { get; set; }
    public string? DateRange { get; set; }
}

/// <summary>
/// Share dashboard request
/// </summary>
public class ShareDashboardDto
{
    public int DashboardId { get; set; }
    public List<int>? UserIds { get; set; }
    public List<int>? RoleIds { get; set; }
    public bool CanEdit { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Dashboard share link
/// </summary>
public class DashboardShareLinkDto
{
    public string ShareToken { get; set; } = string.Empty;
    public string ShareUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresAuth { get; set; }
}

#endregion

#region Subscription & Notification DTOs

/// <summary>
/// Dashboard subscription
/// </summary>
public class DashboardSubscriptionDto
{
    public int Id { get; set; }
    public int DashboardId { get; set; }
    public string DashboardName { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty; // Daily, Weekly, Monthly
    public string? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public string? TimeOfDay { get; set; }
    public string DeliveryMethod { get; set; } = string.Empty; // Email, Slack, Teams
    public string Format { get; set; } = string.Empty; // PDF, PNG
    public bool IsActive { get; set; }
}

/// <summary>
/// Create subscription request
/// </summary>
public class CreateDashboardSubscriptionDto
{
    public int DashboardId { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public string? TimeOfDay { get; set; }
    public string DeliveryMethod { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
}

#endregion
