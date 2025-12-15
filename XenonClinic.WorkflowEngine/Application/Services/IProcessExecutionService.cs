namespace XenonClinic.WorkflowEngine.Application.Services;

using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Domain.Entities;

/// <summary>
/// Service for executing process instances.
/// </summary>
public interface IProcessExecutionService
{
    /// <summary>
    /// Starts a new process instance.
    /// </summary>
    Task<ProcessInstanceDto> StartProcessAsync(
        StartProcessRequest request,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a process instance by ID.
    /// </summary>
    Task<ProcessInstanceDetailDto?> GetInstanceAsync(
        Guid instanceId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists process instances with filtering.
    /// </summary>
    Task<PagedResultDto<ProcessInstanceSummaryDto>> ListInstancesAsync(
        ProcessInstanceQueryDto query,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Signals an event to a process instance.
    /// </summary>
    Task<ProcessInstanceDto> SignalAsync(
        Guid instanceId,
        string signalName,
        Dictionary<string, object>? variables,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a process instance.
    /// </summary>
    Task CancelAsync(
        Guid instanceId,
        string? reason,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends a process instance.
    /// </summary>
    Task SuspendAsync(
        Guid instanceId,
        string? reason,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a suspended process instance.
    /// </summary>
    Task<ProcessInstanceDto> ResumeAsync(
        Guid instanceId,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a failed activity.
    /// </summary>
    Task<ProcessInstanceDto> RetryActivityAsync(
        Guid instanceId,
        string activityInstanceId,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets process instance variables.
    /// </summary>
    Task<Dictionary<string, object>> GetVariablesAsync(
        Guid instanceId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets process instance variables.
    /// </summary>
    Task SetVariablesAsync(
        Guid instanceId,
        Dictionary<string, object> variables,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the execution history of a process instance.
    /// </summary>
    Task<IList<ActivityExecutionDto>> GetHistoryAsync(
        Guid instanceId,
        int tenantId,
        CancellationToken cancellationToken = default);
}

#region Request DTOs

public class StartProcessRequest
{
    /// <summary>
    /// Process definition key.
    /// </summary>
    public string ProcessKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional specific version to start. If not specified, uses published version.
    /// </summary>
    public int? Version { get; set; }

    /// <summary>
    /// Business key for correlation.
    /// </summary>
    public string? BusinessKey { get; set; }

    /// <summary>
    /// Initial variables.
    /// </summary>
    public Dictionary<string, object>? Variables { get; set; }

    /// <summary>
    /// Parent instance ID for sub-processes.
    /// </summary>
    public Guid? ParentInstanceId { get; set; }

    /// <summary>
    /// Parent activity instance ID.
    /// </summary>
    public string? ParentActivityInstanceId { get; set; }
}

public class ProcessInstanceQueryDto
{
    public string? ProcessDefinitionId { get; set; }
    public string? ProcessKey { get; set; }
    public string? BusinessKey { get; set; }
    public List<ProcessInstanceStatus>? Statuses { get; set; }
    public string? StartedBy { get; set; }
    public DateTime? StartedAfter { get; set; }
    public DateTime? StartedBefore { get; set; }
    public bool IncludeSubProcesses { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

#endregion

#region Response DTOs

public class ProcessInstanceDto
{
    public Guid Id { get; set; }
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessKey { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessVersion { get; set; }
    public ProcessInstanceStatus Status { get; set; }
    public string? BusinessKey { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? StartedBy { get; set; }
    public List<string> ActiveActivityIds { get; set; } = new();
}

public class ProcessInstanceSummaryDto
{
    public Guid Id { get; set; }
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessKey { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessVersion { get; set; }
    public ProcessInstanceStatus Status { get; set; }
    public string? BusinessKey { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? StartedBy { get; set; }
    public int ActiveTaskCount { get; set; }
    public string? CurrentActivityId { get; set; }
}

public class ProcessInstanceDetailDto : ProcessInstanceDto
{
    public int TenantId { get; set; }
    public Guid? ParentInstanceId { get; set; }
    public string? ParentActivityInstanceId { get; set; }
    public Dictionary<string, object> Variables { get; set; } = new();
    public List<ActivityInstanceDto> ActivityInstances { get; set; } = new();
    public List<HumanTaskSummaryDto> ActiveTasks { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class ActivityInstanceDto
{
    public string Id { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public ActivityInstanceStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}

public class ActivityExecutionDto
{
    public string ActivityInstanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class HumanTaskSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public HumanTaskStatus Status { get; set; }
    public string? AssigneeUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public TaskPriority Priority { get; set; }
}

#endregion
