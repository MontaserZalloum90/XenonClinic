namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for workflow monitoring and analytics.
/// </summary>
public interface IMonitoringService
{
    /// <summary>
    /// Gets dashboard summary statistics.
    /// </summary>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets process execution statistics.
    /// </summary>
    Task<ProcessStatisticsDto> GetProcessStatisticsAsync(
        string processDefinitionId,
        int tenantId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets task statistics.
    /// </summary>
    Task<TaskStatisticsDto> GetTaskStatisticsAsync(
        int tenantId,
        string? processDefinitionId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets activity heatmap data for a process.
    /// </summary>
    Task<ActivityHeatmapDto> GetActivityHeatmapAsync(
        string processDefinitionId,
        int tenantId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets SLA compliance metrics.
    /// </summary>
    Task<SlaMetricsDto> GetSlaMetricsAsync(
        int tenantId,
        string? processDefinitionId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user performance metrics.
    /// </summary>
    Task<UserPerformanceDto> GetUserPerformanceAsync(
        string userId,
        int tenantId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets system health status.
    /// </summary>
    Task<SystemHealthDto> GetSystemHealthAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent errors and incidents.
    /// </summary>
    Task<IList<IncidentDto>> GetRecentIncidentsAsync(
        int tenantId,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a metric data point.
    /// </summary>
    Task RecordMetricAsync(
        RecordMetricRequest request,
        CancellationToken cancellationToken = default);
}

#region DTOs

public class DashboardSummaryDto
{
    public int TotalDefinitions { get; set; }
    public int ActiveDefinitions { get; set; }
    public ProcessCountsDto ProcessCounts { get; set; } = new();
    public TaskCountsDto TaskCounts { get; set; } = new();
    public List<TrendDataPointDto> ProcessTrend { get; set; } = new();
    public List<TrendDataPointDto> TaskTrend { get; set; } = new();
    public List<TopProcessDto> TopProcesses { get; set; } = new();
}

public class ProcessCountsDto
{
    public int Running { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int Suspended { get; set; }
    public int TotalToday { get; set; }
    public int TotalThisWeek { get; set; }
    public int TotalThisMonth { get; set; }
}

public class TaskCountsDto
{
    public int Open { get; set; }
    public int Assigned { get; set; }
    public int Overdue { get; set; }
    public int CompletedToday { get; set; }
    public int DueToday { get; set; }
    public double AverageCompletionTimeHours { get; set; }
}

public class TrendDataPointDto
{
    public DateTime Date { get; set; }
    public int Started { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
}

public class TopProcessDto
{
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessKey { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int InstanceCount { get; set; }
    public int OpenTaskCount { get; set; }
    public double AverageDurationHours { get; set; }
}

public class ProcessStatisticsDto
{
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessKey { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int TotalInstances { get; set; }
    public int CompletedInstances { get; set; }
    public int FailedInstances { get; set; }
    public int RunningInstances { get; set; }
    public double CompletionRate { get; set; }
    public double AverageDurationHours { get; set; }
    public double MedianDurationHours { get; set; }
    public double MinDurationHours { get; set; }
    public double MaxDurationHours { get; set; }
    public List<TrendDataPointDto> DailyTrend { get; set; } = new();
    public List<ActivityStatisticsDto> ActivityStatistics { get; set; } = new();
}

public class ActivityStatisticsDto
{
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
    public int FailedCount { get; set; }
    public double AverageDurationSeconds { get; set; }
    public double FailureRate { get; set; }
}

public class TaskStatisticsDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OpenTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double AverageCompletionTimeHours { get; set; }
    public double AverageClaimTimeMinutes { get; set; }
    public List<TasksByPriorityDto> ByPriority { get; set; } = new();
    public List<TasksByAssigneeDto> ByAssignee { get; set; } = new();
    public List<TrendDataPointDto> DailyTrend { get; set; } = new();
}

public class TasksByPriorityDto
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
    public int OverdueCount { get; set; }
}

public class TasksByAssigneeDto
{
    public string AssigneeUserId { get; set; } = string.Empty;
    public int AssignedCount { get; set; }
    public int CompletedCount { get; set; }
    public double AverageCompletionTimeHours { get; set; }
}

public class ActivityHeatmapDto
{
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public List<ActivityHeatmapItem> Items { get; set; } = new();
}

public class ActivityHeatmapItem
{
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
    public int FailedCount { get; set; }
    public double AverageDurationSeconds { get; set; }
    public double Intensity { get; set; } // 0-1 normalized value
}

public class SlaMetricsDto
{
    public double OverallComplianceRate { get; set; }
    public int TotalMeasurements { get; set; }
    public int ComplianceMet { get; set; }
    public int ComplianceBreached { get; set; }
    public List<SlaByProcessDto> ByProcess { get; set; } = new();
    public List<TrendDataPointDto> DailyTrend { get; set; } = new();
}

public class SlaByProcessDto
{
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public double ComplianceRate { get; set; }
    public int TotalMeasurements { get; set; }
}

public class UserPerformanceDto
{
    public string UserId { get; set; } = string.Empty;
    public int TotalTasksCompleted { get; set; }
    public int TasksCompletedOnTime { get; set; }
    public int TasksCompletedLate { get; set; }
    public double AverageCompletionTimeHours { get; set; }
    public double OnTimeCompletionRate { get; set; }
    public List<TrendDataPointDto> DailyCompletions { get; set; } = new();
    public List<TaskTypePerformanceDto> ByTaskType { get; set; } = new();
}

public class TaskTypePerformanceDto
{
    public string TaskDefinitionKey { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public int CompletedCount { get; set; }
    public double AverageCompletionTimeHours { get; set; }
}

public class SystemHealthDto
{
    public string Status { get; set; } = "healthy"; // healthy, degraded, unhealthy
    public DateTime CheckedAt { get; set; }
    public DatabaseHealthDto Database { get; set; } = new();
    public JobProcessorHealthDto JobProcessor { get; set; } = new();
    public TimerServiceHealthDto TimerService { get; set; } = new();
}

public class DatabaseHealthDto
{
    public bool IsConnected { get; set; }
    public int ResponseTimeMs { get; set; }
}

public class JobProcessorHealthDto
{
    public int PendingJobs { get; set; }
    public int RunningJobs { get; set; }
    public int FailedJobsLast24h { get; set; }
    public DateTime? LastProcessedAt { get; set; }
}

public class TimerServiceHealthDto
{
    public int ScheduledTimers { get; set; }
    public int OverdueTimers { get; set; }
    public DateTime? LastFiredAt { get; set; }
}

public class IncidentDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? ProcessInstanceId { get; set; }
    public string? ActivityInstanceId { get; set; }
    public DateTime OccurredAt { get; set; }
    public bool IsResolved { get; set; }
}

public class RecordMetricRequest
{
    public int TenantId { get; set; }
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public Dictionary<string, object>? Dimensions { get; set; }
    public decimal Value { get; set; }
    public DateTime? Timestamp { get; set; }
}

#endregion
