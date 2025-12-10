namespace XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Represents a running instance of a workflow.
/// </summary>
public interface IWorkflowInstance
{
    /// <summary>
    /// Unique identifier for this instance
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The workflow definition ID
    /// </summary>
    string WorkflowId { get; }

    /// <summary>
    /// Version of the workflow definition
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Custom name for this instance
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Current status
    /// </summary>
    WorkflowStatus Status { get; }

    /// <summary>
    /// Tenant ID
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// User who created the instance
    /// </summary>
    string? CreatedBy { get; }

    /// <summary>
    /// Correlation ID for tracking
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Priority
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// When the instance was created
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// When execution started
    /// </summary>
    DateTime? StartedAt { get; }

    /// <summary>
    /// When execution completed/faulted/cancelled
    /// </summary>
    DateTime? CompletedAt { get; }

    /// <summary>
    /// Scheduled start time
    /// </summary>
    DateTime? ScheduledStartTime { get; }

    /// <summary>
    /// Input data
    /// </summary>
    IDictionary<string, object?> Input { get; }

    /// <summary>
    /// Current variables
    /// </summary>
    IDictionary<string, object?> Variables { get; }

    /// <summary>
    /// Output data
    /// </summary>
    IDictionary<string, object?> Output { get; }

    /// <summary>
    /// Current activity being executed
    /// </summary>
    string? CurrentActivityId { get; }

    /// <summary>
    /// All active activity IDs (for parallel execution)
    /// </summary>
    IList<string> ActiveActivityIds { get; }

    /// <summary>
    /// Completed activity IDs
    /// </summary>
    IList<string> CompletedActivityIds { get; }

    /// <summary>
    /// Active bookmarks
    /// </summary>
    IList<WorkflowBookmark> Bookmarks { get; }

    /// <summary>
    /// Error information if faulted
    /// </summary>
    WorkflowError? Error { get; }

    /// <summary>
    /// Fault count (for retry tracking)
    /// </summary>
    int FaultCount { get; }

    /// <summary>
    /// Metadata
    /// </summary>
    IDictionary<string, object?> Metadata { get; }

    /// <summary>
    /// Execution log entries
    /// </summary>
    IList<WorkflowLogEntry> LogEntries { get; }

    /// <summary>
    /// Audit trail entries
    /// </summary>
    IList<WorkflowAuditEntry> AuditEntries { get; }
}

/// <summary>
/// Log entry for workflow execution
/// </summary>
public class WorkflowLogEntry
{
    public DateTime Timestamp { get; init; }
    public LogLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ActivityId { get; init; }
}

/// <summary>
/// Audit entry for workflow changes
/// </summary>
public class WorkflowAuditEntry
{
    public DateTime Timestamp { get; init; }
    public required string Action { get; init; }
    public string? Details { get; init; }
    public string? UserId { get; init; }
    public string? ActivityId { get; init; }
}

/// <summary>
/// Mutable workflow instance state for internal use
/// </summary>
public class WorkflowInstanceState : IWorkflowInstance
{
    public Guid Id { get; set; }
    public string WorkflowId { get; set; } = string.Empty;
    public int Version { get; set; }
    public string? Name { get; set; }
    public WorkflowStatus Status { get; set; }
    public int? TenantId { get; set; }
    public string? CreatedBy { get; set; }
    public string? CorrelationId { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ScheduledStartTime { get; set; }
    public IDictionary<string, object?> Input { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> Variables { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> Output { get; set; } = new Dictionary<string, object?>();
    public string? CurrentActivityId { get; set; }
    public IList<string> ActiveActivityIds { get; set; } = new List<string>();
    public IList<string> CompletedActivityIds { get; set; } = new List<string>();
    public IList<WorkflowBookmark> Bookmarks { get; set; } = new List<WorkflowBookmark>();
    public WorkflowError? Error { get; set; }
    public int FaultCount { get; set; }
    public IDictionary<string, object?> Metadata { get; set; } = new Dictionary<string, object?>();
    public IList<WorkflowLogEntry> LogEntries { get; set; } = new List<WorkflowLogEntry>();
    public IList<WorkflowAuditEntry> AuditEntries { get; set; } = new List<WorkflowAuditEntry>();

    /// <summary>
    /// Compensation stack for rollback
    /// </summary>
    public Stack<string> CompensationStack { get; set; } = new();

    /// <summary>
    /// Parallel execution branches
    /// </summary>
    public IDictionary<string, ParallelBranch> ParallelBranches { get; set; } = new Dictionary<string, ParallelBranch>();
}

/// <summary>
/// Parallel execution branch state
/// </summary>
public class ParallelBranch
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string ParentActivityId { get; init; } = string.Empty;
    public IList<string> ActivityIds { get; init; } = new List<string>();
    public ParallelBranchStatus Status { get; set; }
    public IDictionary<string, object?>? Output { get; set; }
}

/// <summary>
/// Parallel branch status
/// </summary>
public enum ParallelBranchStatus
{
    Pending,
    Running,
    Completed,
    Faulted
}
