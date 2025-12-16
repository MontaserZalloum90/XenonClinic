namespace XenonClinic.WorkflowEngine.Core.Abstractions
{
    /// <summary>
    /// Query options for listing workflow instances.
    /// </summary>
    public class WorkflowInstanceQuery
    {
        public string? WorkflowId { get; set; }
        public IReadOnlyList<WorkflowStatus>? Statuses { get; set; }
        public string? CorrelationId { get; set; }
        public int? TenantId { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? OrderBy { get; set; }
        public bool OrderDescending { get; set; }
    }

    /// <summary>
    /// Options used when starting a workflow instance.
    /// </summary>
    public class WorkflowInstanceOptions
    {
        public int? TenantId { get; set; }
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public int Priority { get; set; }
        public string? CorrelationId { get; set; }
        public DateTime? ScheduledStartTime { get; set; }
        public int? Version { get; set; }
        public IDictionary<string, object?>? Metadata { get; set; }
    }

    /// <summary>
    /// Contract for interacting with the workflow engine.
    /// </summary>
    public interface IWorkflowEngine
    {
        Task<PaginatedResult<IWorkflowInstance>> QueryInstancesAsync(WorkflowInstanceQuery query);
        Task<IWorkflowInstance?> GetInstanceAsync(Guid id);
        Task<IReadOnlyList<WorkflowExecutionRecord>> GetHistoryAsync(Guid id);
        Task<WorkflowExecutionResult> StartNewAsync(string workflowId, IDictionary<string, object?>? input, WorkflowInstanceOptions options);
        Task<WorkflowExecutionResult> ResumeAsync(Guid id, string bookmarkName, IDictionary<string, object?>? input);
        Task CancelAsync(Guid id, string? reason = null);
        Task TerminateAsync(Guid id, string? reason = null);
        Task<WorkflowExecutionResult> RetryAsync(Guid id);
        Task SignalAsync(Guid id, string signalName, IDictionary<string, object?>? data);
        Task BroadcastSignalAsync(string signalName, IDictionary<string, object?>? data, string? workflowId = null);
        Task<IReadOnlyList<WorkflowExecutionResult>> TriggerEventAsync(string eventName, IDictionary<string, object?>? eventData);
    }

    /// <summary>
    /// Represents a workflow instance snapshot.
    /// </summary>
    public interface IWorkflowInstance
    {
        Guid Id { get; }
        string WorkflowId { get; }
        string? Name { get; }
        int Version { get; }
        WorkflowStatus Status { get; }
        int Priority { get; }
        string? CorrelationId { get; }
        DateTime CreatedAt { get; }
        DateTime? StartedAt { get; }
        DateTime? CompletedAt { get; }
        string? CreatedBy { get; }
        int? TenantId { get; }
        string? CurrentActivityId { get; }
        IEnumerable<string>? CompletedActivityIds { get; }
        IDictionary<string, object?>? Input { get; }
        IDictionary<string, object?>? Output { get; }
        IDictionary<string, object?>? Variables { get; }
        DateTime? ScheduledStartTime { get; }
        int FaultCount { get; }
        WorkflowError? Error { get; }
        IEnumerable<WorkflowBookmark>? Bookmarks { get; }
        IEnumerable<WorkflowAuditEntry>? AuditEntries { get; }
    }

    /// <summary>
    /// Execution result returned from workflow operations.
    /// </summary>
    public class WorkflowExecutionResult
    {
        public Guid InstanceId { get; set; }
        public WorkflowStatus Status { get; set; }
        public IDictionary<string, object?>? Output { get; set; }
        public TimeSpan Duration { get; set; }
        public int ActivitiesExecuted { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsRunning { get; set; }
        public WorkflowError? Error { get; set; }
        public IEnumerable<WorkflowBookmark>? Bookmarks { get; set; }
    }

    /// <summary>
    /// Workflow execution timeline record.
    /// </summary>
    public class WorkflowExecutionRecord
    {
        public Guid Id { get; set; }
        public string? ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public string? ActivityType { get; set; }
        public WorkflowExecutionRecordType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan? Duration { get; set; }
        public IDictionary<string, object?>? Output { get; set; }
        public WorkflowActivityError? Error { get; set; }
    }

    public enum WorkflowExecutionRecordType
    {
        Started,
        Completed,
        Faulted,
        Suspended,
        Cancelled,
        Resumed
    }

    /// <summary>
    /// Overall instance status values.
    /// </summary>
    public enum WorkflowStatus
    {
        Created,
        Running,
        Suspended,
        Completed,
        Faulted,
        Cancelled,
        Terminated
    }

    public class WorkflowError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActivityId { get; set; }
    }

    public class WorkflowActivityError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActivityId { get; set; }
    }

    public class WorkflowBookmark
    {
        public string Name { get; set; } = string.Empty;
        public string? ActivityId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WorkflowAuditEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? Details { get; set; }
    }

    /// <summary>
    /// Exception thrown when a workflow cannot be located.
    /// </summary>
    public class WorkflowNotFoundException : Exception
    {
        public WorkflowNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when a workflow operation fails validation.
    /// </summary>
    public class WorkflowValidationException : Exception
    {
        public WorkflowValidationException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when a bookmark cannot be found.
    /// </summary>
    public class WorkflowBookmarkNotFoundException : Exception
    {
        public WorkflowBookmarkNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception thrown when a workflow instance is in an invalid state for the requested operation.
    /// </summary>
    public class WorkflowInvalidStateException : Exception
    {
        public WorkflowInvalidStateException(string message) : base(message) { }
    }

    /// <summary>
    /// Simple container for paginated results.
    /// </summary>
    public record PaginatedResult<T>(IReadOnlyList<T> Items, int TotalCount, int PageNumber, int PageSize);
}

namespace XenonClinic.WorkflowEngine.Persistence.Abstractions
{
    using XenonClinic.WorkflowEngine.Core.Abstractions;

    /// <summary>
    /// Query options for workflow definitions.
    /// </summary>
    public class WorkflowDefinitionQuery
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public IReadOnlyList<string>? Tags { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDraft { get; set; }
        public int? TenantId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? OrderBy { get; set; }
        public bool OrderDescending { get; set; }
    }

    /// <summary>
    /// Contract for persisting workflow definitions.
    /// </summary>
    public interface IWorkflowDefinitionStore
    {
        Task<PaginatedResult<IWorkflowDefinition>> ListAsync(WorkflowDefinitionQuery query);
        Task<IWorkflowDefinition?> GetAsync(string id, int? version = null);
        Task<IEnumerable<IWorkflowDefinition>> GetVersionsAsync(string id);
        Task PublishAsync(string id, int version);
        Task UnpublishAsync(string id, int version);
    }

    /// <summary>
    /// Workflow definition abstraction used by the API layer.
    /// </summary>
    public interface IWorkflowDefinition
    {
        string Id { get; }
        string Name { get; }
        string? Description { get; }
        string? Category { get; }
        int Version { get; }
        bool IsActive { get; }
        bool IsDraft { get; }
        IEnumerable<string>? Tags { get; }
        DateTime CreatedAt { get; }
        DateTime ModifiedAt { get; }
        int? TenantId { get; }
        IEnumerable<WorkflowParameterDefinition>? InputParameters { get; }
        IEnumerable<WorkflowParameterDefinition>? OutputParameters { get; }
        IEnumerable<WorkflowVariableDefinition>? Variables { get; }
        IEnumerable<WorkflowTriggerDefinition>? Triggers { get; }
    }

    public class WorkflowParameterDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public object? DefaultValue { get; set; }
        public string? Schema { get; set; }
    }

    public class WorkflowVariableDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public object? DefaultValue { get; set; }
        public WorkflowVariableScope Scope { get; set; }
    }

    public class WorkflowTriggerDefinition
    {
        public string Name { get; set; } = string.Empty;
        public WorkflowTriggerType Type { get; set; }
        public bool IsEnabled { get; set; }
        public IDictionary<string, object?>? Configuration { get; set; }
    }

    public enum WorkflowTriggerType
    {
        Manual,
        Event,
        Schedule,
        Signal
    }

    public enum WorkflowVariableScope
    {
        Instance,
        Workflow,
        Global
    }
}
