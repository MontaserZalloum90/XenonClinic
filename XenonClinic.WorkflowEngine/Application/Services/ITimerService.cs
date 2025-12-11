namespace XenonClinic.WorkflowEngine.Application.Services;

using XenonClinic.WorkflowEngine.Domain.Entities;

/// <summary>
/// Service for managing workflow timers and scheduled events.
/// </summary>
public interface ITimerService
{
    /// <summary>
    /// Schedules a timer for a process instance.
    /// </summary>
    Task<ProcessTimer> ScheduleTimerAsync(
        ScheduleTimerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a specific timer.
    /// </summary>
    Task CancelTimerAsync(
        string timerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels all timers for a process instance.
    /// </summary>
    Task CancelProcessTimersAsync(
        Guid processInstanceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets due timers ready to fire.
    /// </summary>
    Task<IList<ProcessTimer>> GetDueTimersAsync(
        DateTime until,
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a timer as fired and processes it.
    /// </summary>
    Task ProcessTimerAsync(
        string timerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recalculates the next fire time for cycle timers.
    /// </summary>
    Task<DateTime?> CalculateNextFireTimeAsync(
        ProcessTimer timer,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active timers for a process instance.
    /// </summary>
    Task<IList<TimerDto>> GetProcessTimersAsync(
        Guid processInstanceId,
        int tenantId,
        CancellationToken cancellationToken = default);
}

#region DTOs

public class ScheduleTimerRequest
{
    public int TenantId { get; set; }
    public Guid ProcessInstanceId { get; set; }
    public string? ActivityInstanceId { get; set; }
    public TimerType Type { get; set; }

    /// <summary>
    /// For Date timers: the specific date/time to fire.
    /// </summary>
    public DateTime? FireAt { get; set; }

    /// <summary>
    /// For Duration timers: ISO 8601 duration (e.g., "PT1H" for 1 hour).
    /// </summary>
    public string? Duration { get; set; }

    /// <summary>
    /// For Cycle timers: ISO 8601 repeating interval (e.g., "R3/PT1H" for 3 times every hour).
    /// </summary>
    public string? CycleExpression { get; set; }

    /// <summary>
    /// Timer definition key from the process model.
    /// </summary>
    public string? TimerDefinitionKey { get; set; }

    /// <summary>
    /// Data to include when the timer fires.
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}

public class TimerDto
{
    public string Id { get; set; } = string.Empty;
    public Guid ProcessInstanceId { get; set; }
    public string? ActivityInstanceId { get; set; }
    public TimerType Type { get; set; }
    public TimerStatus Status { get; set; }
    public DateTime FireAt { get; set; }
    public DateTime? FiredAt { get; set; }
    public string? CycleExpression { get; set; }
    public int CycleCount { get; set; }
    public int? MaxCycles { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion
