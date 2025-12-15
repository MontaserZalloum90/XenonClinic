namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Infrastructure.Data;

/// <summary>
/// Service implementation for workflow monitoring and analytics.
/// </summary>
public class MonitoringService : IMonitoringService
{
    private readonly WorkflowEngineDbContext _context;
    private readonly ILogger<MonitoringService> _logger;

    public MonitoringService(
        WorkflowEngineDbContext context,
        ILogger<MonitoringService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);

        // Process counts
        var processCounts = await _context.ProcessInstances
            .Where(i => i.TenantId == tenantId)
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var processCountsDto = new ProcessCountsDto
        {
            Running = processCounts.FirstOrDefault(c => c.Status == ProcessInstanceStatus.Running)?.Count ?? 0,
            Completed = processCounts.FirstOrDefault(c => c.Status == ProcessInstanceStatus.Completed)?.Count ?? 0,
            Failed = processCounts.FirstOrDefault(c => c.Status == ProcessInstanceStatus.Failed)?.Count ?? 0,
            Suspended = processCounts.FirstOrDefault(c => c.Status == ProcessInstanceStatus.Suspended)?.Count ?? 0,
        };

        processCountsDto.TotalToday = await _context.ProcessInstances
            .CountAsync(i => i.TenantId == tenantId && i.StartedAt >= today, cancellationToken);

        processCountsDto.TotalThisWeek = await _context.ProcessInstances
            .CountAsync(i => i.TenantId == tenantId && i.StartedAt >= weekAgo, cancellationToken);

        processCountsDto.TotalThisMonth = await _context.ProcessInstances
            .CountAsync(i => i.TenantId == tenantId && i.StartedAt >= monthAgo, cancellationToken);

        // Task counts
        var taskCounts = new TaskCountsDto
        {
            Open = await _context.HumanTasks
                .CountAsync(t => t.TenantId == tenantId &&
                                t.Status != HumanTaskStatus.Completed &&
                                t.Status != HumanTaskStatus.Cancelled, cancellationToken),

            Assigned = await _context.HumanTasks
                .CountAsync(t => t.TenantId == tenantId &&
                                t.Status == HumanTaskStatus.Assigned, cancellationToken),

            Overdue = await _context.HumanTasks
                .CountAsync(t => t.TenantId == tenantId &&
                                t.DueDate < now &&
                                t.Status != HumanTaskStatus.Completed &&
                                t.Status != HumanTaskStatus.Cancelled, cancellationToken),

            CompletedToday = await _context.HumanTasks
                .CountAsync(t => t.TenantId == tenantId &&
                                t.CompletedAt >= today &&
                                t.Status == HumanTaskStatus.Completed, cancellationToken),

            DueToday = await _context.HumanTasks
                .CountAsync(t => t.TenantId == tenantId &&
                                t.DueDate >= today &&
                                t.DueDate < today.AddDays(1) &&
                                t.Status != HumanTaskStatus.Completed &&
                                t.Status != HumanTaskStatus.Cancelled, cancellationToken)
        };

        // Average completion time
        var completedTasks = await _context.HumanTasks
            .Where(t => t.TenantId == tenantId &&
                       t.Status == HumanTaskStatus.Completed &&
                       t.CompletedAt != null)
            .Select(t => new { t.CreatedAt, t.CompletedAt })
            .Take(1000)
            .ToListAsync(cancellationToken);

        if (completedTasks.Count > 0)
        {
            taskCounts.AverageCompletionTimeHours = completedTasks
                .Average(t => (t.CompletedAt!.Value - t.CreatedAt).TotalHours);
        }

        // Process trend (last 7 days)
        var processTrend = await GetProcessTrendAsync(tenantId, weekAgo, now, cancellationToken);

        // Top processes
        var topProcesses = await _context.ProcessInstances
            .Where(i => i.TenantId == tenantId && i.StartedAt >= monthAgo)
            .GroupBy(i => i.ProcessDefinitionId)
            .Select(g => new
            {
                ProcessDefinitionId = g.Key,
                InstanceCount = g.Count()
            })
            .OrderByDescending(x => x.InstanceCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        var definitionIds = topProcesses.Select(t => t.ProcessDefinitionId).ToList();
        var definitions = await _context.ProcessDefinitions
            .Where(d => definitionIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        var topProcessDtos = topProcesses.Select(t =>
        {
            definitions.TryGetValue(t.ProcessDefinitionId, out var def);
            return new TopProcessDto
            {
                ProcessDefinitionId = t.ProcessDefinitionId,
                ProcessKey = def?.Key ?? "",
                ProcessName = def?.Name ?? "",
                InstanceCount = t.InstanceCount
            };
        }).ToList();

        // Definition counts
        var totalDefinitions = await _context.ProcessDefinitions
            .CountAsync(d => d.TenantId == tenantId, cancellationToken);

        var activeDefinitions = await _context.ProcessDefinitions
            .CountAsync(d => d.TenantId == tenantId && d.Status == ProcessDefinitionStatus.Active, cancellationToken);

        return new DashboardSummaryDto
        {
            TotalDefinitions = totalDefinitions,
            ActiveDefinitions = activeDefinitions,
            ProcessCounts = processCountsDto,
            TaskCounts = taskCounts,
            ProcessTrend = processTrend,
            TopProcesses = topProcessDtos
        };
    }

    public async Task<ProcessStatisticsDto> GetProcessStatisticsAsync(
        string processDefinitionId,
        int tenantId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var startDate = from ?? DateTime.UtcNow.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == processDefinitionId && d.TenantId == tenantId, cancellationToken);

        if (definition == null)
        {
            throw new KeyNotFoundException($"Process definition '{processDefinitionId}' not found.");
        }

        var instances = await _context.ProcessInstances
            .Where(i => i.ProcessDefinitionId == processDefinitionId &&
                       i.TenantId == tenantId &&
                       i.StartedAt >= startDate &&
                       i.StartedAt <= endDate)
            .ToListAsync(cancellationToken);

        var completedInstances = instances
            .Where(i => i.Status == ProcessInstanceStatus.Completed && i.CompletedAt != null)
            .ToList();

        var durations = completedInstances
            .Select(i => (i.CompletedAt!.Value - i.StartedAt).TotalHours)
            .OrderBy(d => d)
            .ToList();

        var result = new ProcessStatisticsDto
        {
            ProcessDefinitionId = processDefinitionId,
            ProcessKey = definition.Key,
            ProcessName = definition.Name,
            TotalInstances = instances.Count,
            CompletedInstances = completedInstances.Count,
            FailedInstances = instances.Count(i => i.Status == ProcessInstanceStatus.Failed),
            RunningInstances = instances.Count(i => i.Status == ProcessInstanceStatus.Running)
        };

        if (result.TotalInstances > 0)
        {
            result.CompletionRate = (double)result.CompletedInstances / result.TotalInstances * 100;
        }

        if (durations.Count > 0)
        {
            result.AverageDurationHours = durations.Average();
            result.MedianDurationHours = durations[durations.Count / 2];
            result.MinDurationHours = durations.Min();
            result.MaxDurationHours = durations.Max();
        }

        // Daily trend
        result.DailyTrend = await GetProcessTrendAsync(tenantId, startDate, endDate, cancellationToken, processDefinitionId);

        // Activity statistics
        var activityInstances = await _context.ActivityInstances
            .Where(ai => instances.Select(i => i.Id).Contains(ai.ProcessInstanceId))
            .GroupBy(ai => new { ai.ActivityId, ai.ActivityName, ai.ActivityType })
            .Select(g => new
            {
                g.Key.ActivityId,
                g.Key.ActivityName,
                g.Key.ActivityType,
                ExecutionCount = g.Count(),
                FailedCount = g.Count(ai => ai.Status == ActivityInstanceStatus.Failed),
                TotalDuration = g.Sum(ai => ai.CompletedAt != null
                    ? (double?)(ai.CompletedAt.Value - ai.StartedAt).TotalSeconds
                    : null)
            })
            .ToListAsync(cancellationToken);

        result.ActivityStatistics = activityInstances.Select(a => new ActivityStatisticsDto
        {
            ActivityId = a.ActivityId,
            ActivityName = a.ActivityName,
            ActivityType = a.ActivityType,
            ExecutionCount = a.ExecutionCount,
            FailedCount = a.FailedCount,
            AverageDurationSeconds = a.TotalDuration.HasValue && a.ExecutionCount > 0
                ? a.TotalDuration.Value / a.ExecutionCount
                : 0,
            FailureRate = a.ExecutionCount > 0 ? (double)a.FailedCount / a.ExecutionCount * 100 : 0
        }).ToList();

        return result;
    }

    public async Task<TaskStatisticsDto> GetTaskStatisticsAsync(
        int tenantId,
        string? processDefinitionId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var startDate = from ?? DateTime.UtcNow.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;
        var now = DateTime.UtcNow;

        var tasksQuery = _context.HumanTasks.Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrEmpty(processDefinitionId))
        {
            var processInstanceIds = await _context.ProcessInstances
                .Where(i => i.ProcessDefinitionId == processDefinitionId)
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            tasksQuery = tasksQuery.Where(t => processInstanceIds.Contains(t.ProcessInstanceId));
        }

        var tasks = await tasksQuery
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .ToListAsync(cancellationToken);

        var completedTasks = tasks.Where(t => t.Status == HumanTaskStatus.Completed).ToList();
        var openTasks = tasks.Where(t =>
            t.Status != HumanTaskStatus.Completed && t.Status != HumanTaskStatus.Cancelled).ToList();

        var result = new TaskStatisticsDto
        {
            TotalTasks = tasks.Count,
            CompletedTasks = completedTasks.Count,
            OpenTasks = openTasks.Count,
            OverdueTasks = openTasks.Count(t => t.DueDate < now)
        };

        if (completedTasks.Count > 0)
        {
            result.AverageCompletionTimeHours = completedTasks
                .Where(t => t.CompletedAt != null)
                .Average(t => (t.CompletedAt!.Value - t.CreatedAt).TotalHours);

            var tasksWithClaimTime = completedTasks.Where(t => t.ClaimedAt != null).ToList();
            if (tasksWithClaimTime.Count > 0)
            {
                result.AverageClaimTimeMinutes = tasksWithClaimTime
                    .Average(t => (t.ClaimedAt!.Value - t.CreatedAt).TotalMinutes);
            }
        }

        // By priority
        result.ByPriority = Enum.GetValues<TaskPriority>()
            .Select(p => new TasksByPriorityDto
            {
                Priority = p.ToString(),
                Count = openTasks.Count(t => t.Priority == p),
                OverdueCount = openTasks.Count(t => t.Priority == p && t.DueDate < now)
            })
            .ToList();

        // By assignee
        result.ByAssignee = completedTasks
            .Where(t => t.AssigneeUserId != null)
            .GroupBy(t => t.AssigneeUserId!)
            .Select(g => new TasksByAssigneeDto
            {
                AssigneeUserId = g.Key,
                CompletedCount = g.Count(),
                AssignedCount = openTasks.Count(t => t.AssigneeUserId == g.Key),
                AverageCompletionTimeHours = g.Where(t => t.CompletedAt != null)
                    .Average(t => (t.CompletedAt!.Value - t.CreatedAt).TotalHours)
            })
            .OrderByDescending(a => a.CompletedCount)
            .Take(10)
            .ToList();

        return result;
    }

    public async Task<ActivityHeatmapDto> GetActivityHeatmapAsync(
        string processDefinitionId,
        int tenantId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var startDate = from ?? DateTime.UtcNow.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        var instanceIds = await _context.ProcessInstances
            .Where(i => i.ProcessDefinitionId == processDefinitionId &&
                       i.TenantId == tenantId &&
                       i.StartedAt >= startDate &&
                       i.StartedAt <= endDate)
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        var activities = await _context.ActivityInstances
            .Where(ai => instanceIds.Contains(ai.ProcessInstanceId))
            .GroupBy(ai => new { ai.ActivityId, ai.ActivityName })
            .Select(g => new
            {
                g.Key.ActivityId,
                g.Key.ActivityName,
                ExecutionCount = g.Count(),
                FailedCount = g.Count(ai => ai.Status == ActivityInstanceStatus.Failed),
                TotalDuration = g.Sum(ai => ai.CompletedAt != null
                    ? (double?)(ai.CompletedAt.Value - ai.StartedAt).TotalSeconds
                    : null)
            })
            .ToListAsync(cancellationToken);

        var maxExecutions = activities.Max(a => a.ExecutionCount);

        return new ActivityHeatmapDto
        {
            ProcessDefinitionId = processDefinitionId,
            Items = activities.Select(a => new ActivityHeatmapItem
            {
                ActivityId = a.ActivityId,
                ActivityName = a.ActivityName,
                ExecutionCount = a.ExecutionCount,
                FailedCount = a.FailedCount,
                AverageDurationSeconds = a.TotalDuration.HasValue && a.ExecutionCount > 0
                    ? a.TotalDuration.Value / a.ExecutionCount
                    : 0,
                Intensity = maxExecutions > 0 ? (double)a.ExecutionCount / maxExecutions : 0
            }).ToList()
        };
    }

    public async Task<SlaMetricsDto> GetSlaMetricsAsync(
        int tenantId,
        string? processDefinitionId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var startDate = from ?? DateTime.UtcNow.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        // Calculate based on task due dates
        var tasksQuery = _context.HumanTasks
            .Where(t => t.TenantId == tenantId &&
                       t.Status == HumanTaskStatus.Completed &&
                       t.DueDate != null &&
                       t.CompletedAt != null &&
                       t.CreatedAt >= startDate &&
                       t.CreatedAt <= endDate);

        if (!string.IsNullOrEmpty(processDefinitionId))
        {
            var instanceIds = await _context.ProcessInstances
                .Where(i => i.ProcessDefinitionId == processDefinitionId)
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            tasksQuery = tasksQuery.Where(t => instanceIds.Contains(t.ProcessInstanceId));
        }

        var tasks = await tasksQuery.ToListAsync(cancellationToken);

        var complianceMet = tasks.Count(t => t.CompletedAt <= t.DueDate);
        var complianceBreached = tasks.Count(t => t.CompletedAt > t.DueDate);

        return new SlaMetricsDto
        {
            TotalMeasurements = tasks.Count,
            ComplianceMet = complianceMet,
            ComplianceBreached = complianceBreached,
            OverallComplianceRate = tasks.Count > 0 ? (double)complianceMet / tasks.Count * 100 : 100
        };
    }

    public async Task<UserPerformanceDto> GetUserPerformanceAsync(
        string userId,
        int tenantId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var startDate = from ?? DateTime.UtcNow.AddDays(-30);
        var endDate = to ?? DateTime.UtcNow;

        var tasks = await _context.HumanTasks
            .Where(t => t.TenantId == tenantId &&
                       t.CompletedBy == userId &&
                       t.Status == HumanTaskStatus.Completed &&
                       t.CompletedAt >= startDate &&
                       t.CompletedAt <= endDate)
            .ToListAsync(cancellationToken);

        var onTime = tasks.Count(t => t.DueDate == null || t.CompletedAt <= t.DueDate);
        var late = tasks.Count(t => t.DueDate != null && t.CompletedAt > t.DueDate);

        return new UserPerformanceDto
        {
            UserId = userId,
            TotalTasksCompleted = tasks.Count,
            TasksCompletedOnTime = onTime,
            TasksCompletedLate = late,
            AverageCompletionTimeHours = tasks.Count > 0
                ? tasks.Average(t => (t.CompletedAt!.Value - t.CreatedAt).TotalHours)
                : 0,
            OnTimeCompletionRate = tasks.Count > 0 ? (double)onTime / tasks.Count * 100 : 100
        };
    }

    public async Task<SystemHealthDto> GetSystemHealthAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var health = new SystemHealthDto
        {
            CheckedAt = now,
            Status = "healthy"
        };

        // Database health
        var sw = Stopwatch.StartNew();
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            health.Database.IsConnected = true;
            health.Database.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
        }
        catch
        {
            health.Database.IsConnected = false;
            health.Status = "unhealthy";
        }

        // Job processor health
        health.JobProcessor.PendingJobs = await _context.AsyncJobs
            .CountAsync(j => j.Status == JobStatus.Pending || j.Status == JobStatus.Retrying, cancellationToken);

        health.JobProcessor.RunningJobs = await _context.AsyncJobs
            .CountAsync(j => j.Status == JobStatus.Running, cancellationToken);

        health.JobProcessor.FailedJobsLast24h = await _context.AsyncJobs
            .CountAsync(j => j.Status == JobStatus.Failed &&
                           j.CompletedAt >= now.AddHours(-24), cancellationToken);

        health.JobProcessor.LastProcessedAt = await _context.AsyncJobs
            .Where(j => j.Status == JobStatus.Completed)
            .OrderByDescending(j => j.CompletedAt)
            .Select(j => j.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Timer service health
        health.TimerService.ScheduledTimers = await _context.ProcessTimers
            .CountAsync(t => t.Status == TimerStatus.Scheduled, cancellationToken);

        health.TimerService.OverdueTimers = await _context.ProcessTimers
            .CountAsync(t => t.Status == TimerStatus.Scheduled && t.FireAt < now, cancellationToken);

        health.TimerService.LastFiredAt = await _context.ProcessTimers
            .Where(t => t.Status == TimerStatus.Fired)
            .OrderByDescending(t => t.FiredAt)
            .Select(t => t.FiredAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Determine overall status
        if (health.JobProcessor.FailedJobsLast24h > 100 ||
            health.TimerService.OverdueTimers > 50)
        {
            health.Status = "degraded";
        }

        return health;
    }

    public async Task<IList<IncidentDto>> GetRecentIncidentsAsync(
        int tenantId,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        // Get recent failures from process instances and jobs
        var recentFailures = await _context.ProcessInstances
            .Where(i => i.TenantId == tenantId &&
                       i.Status == ProcessInstanceStatus.Faulted &&
                       i.CompletedAt != null)
            .OrderByDescending(i => i.CompletedAt)
            .Take(limit)
            .Select(i => new IncidentDto
            {
                Id = i.Id.ToString(),
                Type = "process_failure",
                Severity = "high",
                Message = i.ErrorJson ?? "Process failed",
                ProcessInstanceId = i.Id,
                OccurredAt = i.CompletedAt ?? DateTime.UtcNow,
                IsResolved = false
            })
            .ToListAsync(cancellationToken);

        var recentJobFailures = await _context.AsyncJobs
            .Where(j => j.TenantId == tenantId &&
                       j.Status == JobStatus.Failed &&
                       j.CompletedAt != null)
            .OrderByDescending(j => j.CompletedAt)
            .Take(limit)
            .Select(j => new IncidentDto
            {
                Id = j.Id.ToString(),
                Type = "job_failure",
                Severity = "medium",
                Message = j.ErrorMessage ?? "Job failed",
                ProcessInstanceId = j.ProcessInstanceId,
                ActivityInstanceId = j.ActivityInstanceId?.ToString(),
                OccurredAt = j.CompletedAt ?? DateTime.UtcNow,
                IsResolved = false
            })
            .ToListAsync(cancellationToken);

        return recentFailures.Concat(recentJobFailures)
            .OrderByDescending(i => i.OccurredAt)
            .Take(limit)
            .ToList();
    }

    public async Task RecordMetricAsync(
        RecordMetricRequest request,
        CancellationToken cancellationToken = default)
    {
        var timestamp = request.Timestamp ?? DateTime.UtcNow;
        var metric = new ProcessMetric
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ProcessDefinitionId = request.ProcessDefinitionId,
            PeriodStart = timestamp.Date,
            Granularity = "Day",
            InstancesStarted = request.MetricType == "instances_started" ? (int)request.Value : 0,
            InstancesCompleted = request.MetricType == "instances_completed" ? (int)request.Value : 0,
            InstancesFaulted = request.MetricType == "instances_faulted" ? (int)request.Value : 0,
            ComputedAt = DateTime.UtcNow
        };

        _context.ProcessMetrics.Add(metric);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #region Private Methods

    private async Task<List<TrendDataPointDto>> GetProcessTrendAsync(
        int tenantId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken,
        string? processDefinitionId = null)
    {
        var query = _context.ProcessInstances
            .Where(i => i.TenantId == tenantId &&
                       i.StartedAt >= startDate &&
                       i.StartedAt <= endDate);

        if (!string.IsNullOrEmpty(processDefinitionId))
        {
            query = query.Where(i => i.ProcessDefinitionId == processDefinitionId);
        }

        var dailyData = await query
            .Where(i => i.StartedAt.HasValue)
            .GroupBy(i => i.StartedAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Started = g.Count(),
                Completed = g.Count(i => i.Status == ProcessInstanceStatus.Completed),
                Failed = g.Count(i => i.Status == ProcessInstanceStatus.Faulted)
            })
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        return dailyData.Select(d => new TrendDataPointDto
        {
            Date = d.Date,
            Started = d.Started,
            Completed = d.Completed,
            Failed = d.Failed
        }).ToList();
    }

    #endregion
}
