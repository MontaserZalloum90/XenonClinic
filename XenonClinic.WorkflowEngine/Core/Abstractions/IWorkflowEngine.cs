namespace XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Main workflow engine interface for creating, executing, and managing workflow instances.
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// Creates a new workflow instance from a definition
    /// </summary>
    /// <param name="workflowId">The workflow definition ID</param>
    /// <param name="input">Input parameters for the workflow</param>
    /// <param name="options">Instance creation options</param>
    /// <returns>The created workflow instance</returns>
    Task<IWorkflowInstance> CreateInstanceAsync(
        string workflowId,
        IDictionary<string, object?>? input = null,
        WorkflowInstanceOptions? options = null);

    /// <summary>
    /// Starts a workflow instance
    /// </summary>
    /// <param name="instanceId">The instance ID to start</param>
    /// <returns>The execution result</returns>
    Task<WorkflowExecutionResult> StartAsync(Guid instanceId);

    /// <summary>
    /// Starts a new workflow instance (create + start in one call)
    /// </summary>
    Task<WorkflowExecutionResult> StartNewAsync(
        string workflowId,
        IDictionary<string, object?>? input = null,
        WorkflowInstanceOptions? options = null);

    /// <summary>
    /// Resumes a suspended workflow instance
    /// </summary>
    /// <param name="instanceId">The instance ID to resume</param>
    /// <param name="bookmarkName">The bookmark to resume at</param>
    /// <param name="input">Input data for resumption</param>
    /// <returns>The execution result</returns>
    Task<WorkflowExecutionResult> ResumeAsync(
        Guid instanceId,
        string bookmarkName,
        IDictionary<string, object?>? input = null);

    /// <summary>
    /// Cancels a running workflow instance
    /// </summary>
    /// <param name="instanceId">The instance ID to cancel</param>
    /// <param name="reason">Cancellation reason</param>
    Task CancelAsync(Guid instanceId, string? reason = null);

    /// <summary>
    /// Terminates a workflow instance immediately
    /// </summary>
    /// <param name="instanceId">The instance ID to terminate</param>
    /// <param name="reason">Termination reason</param>
    Task TerminateAsync(Guid instanceId, string? reason = null);

    /// <summary>
    /// Retries a faulted workflow instance
    /// </summary>
    /// <param name="instanceId">The instance ID to retry</param>
    Task<WorkflowExecutionResult> RetryAsync(Guid instanceId);

    /// <summary>
    /// Gets a workflow instance by ID
    /// </summary>
    Task<IWorkflowInstance?> GetInstanceAsync(Guid instanceId);

    /// <summary>
    /// Queries workflow instances
    /// </summary>
    Task<WorkflowInstanceQueryResult> QueryInstancesAsync(WorkflowInstanceQuery query);

    /// <summary>
    /// Sends a signal to a workflow instance
    /// </summary>
    Task SignalAsync(Guid instanceId, string signalName, object? data = null);

    /// <summary>
    /// Broadcasts a signal to all matching workflow instances
    /// </summary>
    Task BroadcastSignalAsync(string signalName, object? data = null, string? workflowId = null);

    /// <summary>
    /// Triggers event-based workflows
    /// </summary>
    Task<IList<WorkflowExecutionResult>> TriggerEventAsync(string eventName, object? eventData = null);

    /// <summary>
    /// Gets execution history for an instance
    /// </summary>
    Task<IList<WorkflowExecutionRecord>> GetHistoryAsync(Guid instanceId);
}

/// <summary>
/// Options for creating a workflow instance
/// </summary>
public class WorkflowInstanceOptions
{
    /// <summary>
    /// Tenant ID for multi-tenant support
    /// </summary>
    public int? TenantId { get; init; }

    /// <summary>
    /// User ID who is creating the instance
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Correlation ID for tracking
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Custom instance name
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Priority (higher = more urgent)
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Scheduled start time (null = immediate)
    /// </summary>
    public DateTime? ScheduledStartTime { get; init; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }

    /// <summary>
    /// Specific version to use (null = latest)
    /// </summary>
    public int? Version { get; init; }
}

/// <summary>
/// Result of a workflow execution
/// </summary>
public class WorkflowExecutionResult
{
    /// <summary>
    /// The workflow instance ID
    /// </summary>
    public Guid InstanceId { get; init; }

    /// <summary>
    /// Final status of the execution
    /// </summary>
    public WorkflowStatus Status { get; init; }

    /// <summary>
    /// Output data from the workflow
    /// </summary>
    public IDictionary<string, object?>? Output { get; init; }

    /// <summary>
    /// Error information if faulted
    /// </summary>
    public WorkflowError? Error { get; init; }

    /// <summary>
    /// Active bookmarks if suspended
    /// </summary>
    public IList<WorkflowBookmark>? Bookmarks { get; init; }

    /// <summary>
    /// Execution duration
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Number of activities executed
    /// </summary>
    public int ActivitiesExecuted { get; init; }

    /// <summary>
    /// Whether the workflow completed successfully
    /// </summary>
    public bool IsCompleted => Status == WorkflowStatus.Completed;

    /// <summary>
    /// Whether the workflow is still running or waiting
    /// </summary>
    public bool IsRunning => Status is WorkflowStatus.Running or WorkflowStatus.Suspended;
}

/// <summary>
/// Workflow error information
/// </summary>
public class WorkflowError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? ActivityId { get; init; }
    public string? StackTrace { get; init; }
    public IDictionary<string, object?>? Details { get; init; }
}

/// <summary>
/// Bookmark for resuming suspended workflows
/// </summary>
public class WorkflowBookmark
{
    public required string Name { get; init; }
    public string? ActivityId { get; init; }
    public DateTime CreatedAt { get; init; }
    public object? Data { get; init; }
}

/// <summary>
/// Query for workflow instances
/// </summary>
public class WorkflowInstanceQuery
{
    public string? WorkflowId { get; init; }
    public IList<WorkflowStatus>? Statuses { get; init; }
    public int? TenantId { get; init; }
    public string? CorrelationId { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? OrderBy { get; init; }
    public bool OrderDescending { get; init; }
}

/// <summary>
/// Query result for workflow instances
/// </summary>
public class WorkflowInstanceQueryResult
{
    public IList<IWorkflowInstance> Items { get; init; } = new List<IWorkflowInstance>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Execution history record
/// </summary>
public class WorkflowExecutionRecord
{
    public Guid Id { get; init; }
    public Guid InstanceId { get; init; }
    public string ActivityId { get; init; } = string.Empty;
    public string ActivityName { get; init; } = string.Empty;
    public string ActivityType { get; init; } = string.Empty;
    public ExecutionRecordType Type { get; init; }
    public DateTime Timestamp { get; init; }
    public TimeSpan? Duration { get; init; }
    public IDictionary<string, object?>? Input { get; init; }
    public IDictionary<string, object?>? Output { get; init; }
    public ActivityError? Error { get; init; }
}

/// <summary>
/// Execution record types
/// </summary>
public enum ExecutionRecordType
{
    ActivityStarted,
    ActivityCompleted,
    ActivityFaulted,
    ActivityCompensated,
    TransitionTaken,
    WorkflowStarted,
    WorkflowCompleted,
    WorkflowFaulted,
    WorkflowSuspended,
    WorkflowResumed,
    WorkflowCancelled
}
