namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Infrastructure.Data;

/// <summary>
/// Service implementation for workflow timers.
/// </summary>
public partial class TimerService : ITimerService
{
    private readonly WorkflowEngineDbContext _context;
    private readonly IProcessExecutionService _executionService;
    private readonly IAuditService _auditService;
    private readonly ILogger<TimerService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public TimerService(
        WorkflowEngineDbContext context,
        IProcessExecutionService executionService,
        IAuditService auditService,
        ILogger<TimerService> logger)
    {
        _context = context;
        _executionService = executionService;
        _auditService = auditService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<ProcessTimer> ScheduleTimerAsync(
        ScheduleTimerRequest request,
        CancellationToken cancellationToken = default)
    {
        var fireAt = CalculateFireTime(request);

        var timer = new ProcessTimer
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = request.TenantId,
            ProcessInstanceId = request.ProcessInstanceId,
            ActivityInstanceId = request.ActivityInstanceId,
            Type = request.Type,
            Status = TimerStatus.Scheduled,
            FireAt = fireAt,
            DurationExpression = request.Duration,
            CycleExpression = request.CycleExpression,
            TimerDefinitionKey = request.TimerDefinitionKey,
            DataJson = request.Data != null
                ? JsonSerializer.Serialize(request.Data, _jsonOptions)
                : null,
            CreatedAt = DateTime.UtcNow
        };

        // Parse cycle expression for max cycles
        if (request.Type == TimerType.Cycle && !string.IsNullOrEmpty(request.CycleExpression))
        {
            timer.MaxCycles = ParseMaxCycles(request.CycleExpression);
        }

        _context.ProcessTimers.Add(timer);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(new AuditEventDto
        {
            TenantId = request.TenantId,
            EventType = AuditEventTypes.TimerScheduled,
            EntityType = "ProcessTimer",
            EntityId = timer.Id,
            ProcessInstanceId = request.ProcessInstanceId,
            ActivityInstanceId = request.ActivityInstanceId,
            NewValues = new Dictionary<string, object>
            {
                ["type"] = request.Type.ToString(),
                ["fireAt"] = fireAt.ToString("O"),
                ["cycleExpression"] = request.CycleExpression ?? ""
            }
        }, cancellationToken);

        _logger.LogInformation(
            "Scheduled {Type} timer {TimerId} to fire at {FireAt} for process {ProcessInstanceId}",
            request.Type, timer.Id, fireAt, request.ProcessInstanceId);

        return timer;
    }

    public async Task CancelTimerAsync(
        string timerId,
        CancellationToken cancellationToken = default)
    {
        var timer = await _context.ProcessTimers
            .FirstOrDefaultAsync(t => t.Id == timerId, cancellationToken);

        if (timer == null)
        {
            _logger.LogWarning("Timer {TimerId} not found for cancellation", timerId);
            return;
        }

        if (timer.Status == TimerStatus.Fired || timer.Status == TimerStatus.Cancelled)
        {
            return;
        }

        timer.Status = TimerStatus.Cancelled;
        timer.CancelledAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(new AuditEventDto
        {
            TenantId = timer.TenantId,
            EventType = AuditEventTypes.TimerCancelled,
            EntityType = "ProcessTimer",
            EntityId = timer.Id,
            ProcessInstanceId = timer.ProcessInstanceId
        }, cancellationToken);

        _logger.LogInformation("Cancelled timer {TimerId}", timerId);
    }

    public async Task CancelProcessTimersAsync(
        Guid processInstanceId,
        CancellationToken cancellationToken = default)
    {
        var timers = await _context.ProcessTimers
            .Where(t => t.ProcessInstanceId == processInstanceId &&
                       t.Status == TimerStatus.Scheduled)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var timer in timers)
        {
            timer.Status = TimerStatus.Cancelled;
            timer.CancelledAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cancelled {Count} timers for process {ProcessInstanceId}",
            timers.Count, processInstanceId);
    }

    public async Task<IList<ProcessTimer>> GetDueTimersAsync(
        DateTime until,
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProcessTimers
            .Where(t => t.Status == TimerStatus.Scheduled && t.FireAt <= until)
            .OrderBy(t => t.FireAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task ProcessTimerAsync(
        string timerId,
        CancellationToken cancellationToken = default)
    {
        var timer = await _context.ProcessTimers
            .FirstOrDefaultAsync(t => t.Id == timerId, cancellationToken);

        if (timer == null)
        {
            _logger.LogWarning("Timer {TimerId} not found for processing", timerId);
            return;
        }

        if (timer.Status != TimerStatus.Scheduled)
        {
            _logger.LogWarning("Timer {TimerId} is not in scheduled status", timerId);
            return;
        }

        var now = DateTime.UtcNow;

        try
        {
            // Get the process instance
            var instance = await _context.ProcessInstances
                .FirstOrDefaultAsync(i => i.Id == timer.ProcessInstanceId, cancellationToken);

            if (instance == null || instance.Status != ProcessInstanceStatus.Running)
            {
                _logger.LogWarning(
                    "Process instance {ProcessInstanceId} not found or not running for timer {TimerId}",
                    timer.ProcessInstanceId, timerId);

                timer.Status = TimerStatus.Cancelled;
                timer.CancelledAt = now;
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            // Mark timer as fired
            timer.Status = TimerStatus.Fired;
            timer.FiredAt = now;
            timer.CycleCount++;

            // Signal the process instance
            var data = !string.IsNullOrEmpty(timer.DataJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(timer.DataJson, _jsonOptions)
                : new Dictionary<string, object>();

            data ??= new Dictionary<string, object>();
            data["timerId"] = timer.Id;
            data["timerType"] = timer.Type.ToString();
            data["timerDefinitionKey"] = timer.TimerDefinitionKey ?? "";

            await _executionService.SignalAsync(
                timer.ProcessInstanceId,
                $"timer-{timer.TimerDefinitionKey ?? timer.Id}",
                data,
                instance.TenantId,
                "system",
                cancellationToken);

            await _auditService.LogAsync(new AuditEventDto
            {
                TenantId = timer.TenantId,
                EventType = AuditEventTypes.TimerFired,
                EntityType = "ProcessTimer",
                EntityId = timer.Id,
                ProcessInstanceId = timer.ProcessInstanceId,
                ActivityInstanceId = timer.ActivityInstanceId,
                NewValues = new Dictionary<string, object>
                {
                    ["firedAt"] = now.ToString("O"),
                    ["cycleCount"] = timer.CycleCount
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Fired timer {TimerId} for process {ProcessInstanceId}",
                timerId, timer.ProcessInstanceId);

            // Handle cycle timers - schedule next occurrence
            if (timer.Type == TimerType.Cycle)
            {
                var nextFireTime = await CalculateNextFireTimeAsync(timer, cancellationToken);
                if (nextFireTime.HasValue)
                {
                    var nextTimer = new ProcessTimer
                    {
                        Id = Guid.NewGuid().ToString(),
                        TenantId = timer.TenantId,
                        ProcessInstanceId = timer.ProcessInstanceId,
                        ActivityInstanceId = timer.ActivityInstanceId,
                        Type = TimerType.Cycle,
                        Status = TimerStatus.Scheduled,
                        FireAt = nextFireTime.Value,
                        CycleExpression = timer.CycleExpression,
                        CycleCount = timer.CycleCount,
                        MaxCycles = timer.MaxCycles,
                        TimerDefinitionKey = timer.TimerDefinitionKey,
                        DataJson = timer.DataJson,
                        CreatedAt = now
                    };

                    _context.ProcessTimers.Add(nextTimer);

                    _logger.LogInformation(
                        "Scheduled next cycle timer {NextTimerId} to fire at {FireAt}",
                        nextTimer.Id, nextFireTime.Value);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing timer {TimerId}", timerId);

            timer.Status = TimerStatus.Failed;
            timer.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync(cancellationToken);

            throw;
        }
    }

    public Task<DateTime?> CalculateNextFireTimeAsync(
        ProcessTimer timer,
        CancellationToken cancellationToken = default)
    {
        if (timer.Type != TimerType.Cycle || string.IsNullOrEmpty(timer.CycleExpression))
        {
            return Task.FromResult<DateTime?>(null);
        }

        // Check if max cycles reached
        if (timer.MaxCycles.HasValue && timer.CycleCount >= timer.MaxCycles.Value)
        {
            return Task.FromResult<DateTime?>(null);
        }

        // Parse cycle expression and calculate next fire time
        var interval = ParseCycleInterval(timer.CycleExpression);
        if (interval.HasValue)
        {
            var nextFire = (timer.FiredAt ?? DateTime.UtcNow).Add(interval.Value);
            return Task.FromResult<DateTime?>(nextFire);
        }

        return Task.FromResult<DateTime?>(null);
    }

    public async Task<IList<TimerDto>> GetProcessTimersAsync(
        Guid processInstanceId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var timers = await _context.ProcessTimers
            .Where(t => t.ProcessInstanceId == processInstanceId && t.TenantId == tenantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return timers.Select(t => new TimerDto
        {
            Id = t.Id,
            ProcessInstanceId = t.ProcessInstanceId,
            ActivityInstanceId = t.ActivityInstanceId,
            Type = t.Type,
            Status = t.Status,
            FireAt = t.FireAt,
            FiredAt = t.FiredAt,
            CycleExpression = t.CycleExpression,
            CycleCount = t.CycleCount,
            MaxCycles = t.MaxCycles,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    #region Private Methods

    private DateTime CalculateFireTime(ScheduleTimerRequest request)
    {
        return request.Type switch
        {
            TimerType.Date => request.FireAt ?? DateTime.UtcNow,
            TimerType.Duration => DateTime.UtcNow.Add(ParseDuration(request.Duration)),
            TimerType.Cycle => DateTime.UtcNow.Add(ParseCycleInterval(request.CycleExpression) ?? TimeSpan.FromHours(1)),
            _ => DateTime.UtcNow
        };
    }

    /// <summary>
    /// Parses ISO 8601 duration format (e.g., PT1H30M, P1D, PT30S).
    /// </summary>
    private static TimeSpan ParseDuration(string? duration)
    {
        if (string.IsNullOrEmpty(duration))
            return TimeSpan.Zero;

        try
        {
            // Handle ISO 8601 duration format
            var match = DurationRegex().Match(duration);
            if (match.Success)
            {
                var days = ParseGroup(match.Groups["days"]);
                var hours = ParseGroup(match.Groups["hours"]);
                var minutes = ParseGroup(match.Groups["minutes"]);
                var seconds = ParseGroup(match.Groups["seconds"]);

                return new TimeSpan(days, hours, minutes, seconds);
            }

            // Fallback to TimeSpan.Parse
            return TimeSpan.Parse(duration);
        }
        catch
        {
            return TimeSpan.FromHours(1); // Default fallback
        }
    }

    private static int ParseGroup(Group group)
    {
        if (group.Success && int.TryParse(group.Value, out var value))
            return value;
        return 0;
    }

    /// <summary>
    /// Parses ISO 8601 repeating interval (e.g., R3/PT1H, R/P1D).
    /// Returns the interval portion.
    /// </summary>
    private static TimeSpan? ParseCycleInterval(string? cycleExpression)
    {
        if (string.IsNullOrEmpty(cycleExpression))
            return null;

        // Format: R[count]/duration or just duration
        var parts = cycleExpression.Split('/');
        var durationPart = parts.Length > 1 ? parts[1] : parts[0];

        var duration = ParseDuration(durationPart);
        return duration != TimeSpan.Zero ? duration : null;
    }

    /// <summary>
    /// Parses the max cycles from a cycle expression (e.g., R3/PT1H returns 3).
    /// </summary>
    private static int? ParseMaxCycles(string? cycleExpression)
    {
        if (string.IsNullOrEmpty(cycleExpression))
            return null;

        var match = CycleCountRegex().Match(cycleExpression);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var count))
        {
            return count;
        }

        // R without a number means infinite
        if (cycleExpression.StartsWith("R/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return null;
    }

    [GeneratedRegex(@"P(?:(?<days>\d+)D)?(?:T(?:(?<hours>\d+)H)?(?:(?<minutes>\d+)M)?(?:(?<seconds>\d+)S)?)?", RegexOptions.IgnoreCase)]
    private static partial Regex DurationRegex();

    [GeneratedRegex(@"^R(\d+)/", RegexOptions.IgnoreCase)]
    private static partial Regex CycleCountRegex();

    #endregion
}
