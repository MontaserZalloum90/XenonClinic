namespace XenonClinic.WorkflowEngine.Persistence.Abstractions;

using XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Store for workflow definitions
/// </summary>
public interface IWorkflowDefinitionStore
{
    /// <summary>
    /// Gets a workflow definition by ID and optional version
    /// </summary>
    Task<IWorkflowDefinition?> GetAsync(string id, int? version = null);

    /// <summary>
    /// Gets all versions of a workflow definition
    /// </summary>
    Task<IList<IWorkflowDefinition>> GetVersionsAsync(string id);

    /// <summary>
    /// Saves a workflow definition (creates new version if exists)
    /// </summary>
    Task<IWorkflowDefinition> SaveAsync(IWorkflowDefinition definition);

    /// <summary>
    /// Deletes a workflow definition (marks as inactive)
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// Lists workflow definitions
    /// </summary>
    Task<WorkflowDefinitionListResult> ListAsync(WorkflowDefinitionQuery query);

    /// <summary>
    /// Gets workflow definitions by trigger type and configuration
    /// </summary>
    Task<IList<IWorkflowDefinition>> GetByTriggerAsync(TriggerType triggerType, string? triggerValue = null);

    /// <summary>
    /// Publishes a draft version (makes it active)
    /// </summary>
    Task PublishAsync(string id, int version);

    /// <summary>
    /// Unpublishes a version (marks as draft)
    /// </summary>
    Task UnpublishAsync(string id, int version);
}

/// <summary>
/// Query for listing workflow definitions
/// </summary>
public class WorkflowDefinitionQuery
{
    public string? SearchTerm { get; init; }
    public string? Category { get; init; }
    public IList<string>? Tags { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsDraft { get; init; }
    public int? TenantId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? OrderBy { get; init; }
    public bool OrderDescending { get; init; }
}

/// <summary>
/// Result of listing workflow definitions
/// </summary>
public class WorkflowDefinitionListResult
{
    public IList<IWorkflowDefinition> Items { get; init; } = new List<IWorkflowDefinition>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

/// <summary>
/// Store for workflow instances
/// </summary>
public interface IWorkflowInstanceStore
{
    /// <summary>
    /// Gets a workflow instance by ID
    /// </summary>
    Task<WorkflowInstanceState?> GetAsync(Guid id);

    /// <summary>
    /// Saves a workflow instance
    /// </summary>
    Task SaveAsync(WorkflowInstanceState instance);

    /// <summary>
    /// Deletes a workflow instance
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Queries workflow instances
    /// </summary>
    Task<WorkflowInstanceQueryResult> QueryAsync(WorkflowInstanceQuery query);

    /// <summary>
    /// Gets execution history for an instance
    /// </summary>
    Task<IList<WorkflowExecutionRecord>> GetHistoryAsync(Guid instanceId);

    /// <summary>
    /// Adds an execution history record
    /// </summary>
    Task AddHistoryAsync(WorkflowExecutionRecord record);

    /// <summary>
    /// Gets instances waiting for a specific signal
    /// </summary>
    Task<IList<WorkflowInstanceState>> GetByBookmarkAsync(string bookmarkName);

    /// <summary>
    /// Gets instances scheduled to start
    /// </summary>
    Task<IList<WorkflowInstanceState>> GetScheduledAsync(DateTime until);

    /// <summary>
    /// Acquires a lock on a workflow instance for execution
    /// </summary>
    Task<bool> TryAcquireLockAsync(Guid instanceId, string lockHolder, TimeSpan duration);

    /// <summary>
    /// Releases a lock on a workflow instance
    /// </summary>
    Task ReleaseLockAsync(Guid instanceId, string lockHolder);
}

/// <summary>
/// Store for workflow timers
/// </summary>
public interface IWorkflowTimerStore
{
    /// <summary>
    /// Schedules a timer
    /// </summary>
    Task ScheduleAsync(WorkflowTimer timer);

    /// <summary>
    /// Gets due timers
    /// </summary>
    Task<IList<WorkflowTimer>> GetDueTimersAsync(DateTime until);

    /// <summary>
    /// Marks a timer as triggered
    /// </summary>
    Task MarkTriggeredAsync(Guid timerId);

    /// <summary>
    /// Cancels a timer
    /// </summary>
    Task CancelAsync(Guid instanceId, string? bookmarkName = null);
}

/// <summary>
/// Workflow timer
/// </summary>
public class WorkflowTimer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid InstanceId { get; init; }
    public string BookmarkName { get; init; } = string.Empty;
    public DateTime FireAt { get; init; }
    public string? CronExpression { get; init; }
    public bool IsTriggered { get; set; }
    public DateTime? TriggeredAt { get; set; }
}
