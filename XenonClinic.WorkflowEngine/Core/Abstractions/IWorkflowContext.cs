namespace XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Represents the execution context for a workflow instance.
/// Contains all data and services available during workflow execution.
/// </summary>
public interface IWorkflowContext
{
    /// <summary>
    /// Unique identifier for this workflow instance
    /// </summary>
    Guid InstanceId { get; }

    /// <summary>
    /// The workflow definition ID being executed
    /// </summary>
    string WorkflowId { get; }

    /// <summary>
    /// Version of the workflow definition
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Tenant ID for multi-tenant support
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// User ID who initiated the workflow
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Correlation ID for tracking across systems
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// When the workflow instance was created
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Current workflow state/status
    /// </summary>
    WorkflowStatus Status { get; }

    /// <summary>
    /// Input data provided when workflow was started
    /// </summary>
    IDictionary<string, object?> Input { get; }

    /// <summary>
    /// Variables stored during workflow execution
    /// </summary>
    IDictionary<string, object?> Variables { get; }

    /// <summary>
    /// Output data from the workflow
    /// </summary>
    IDictionary<string, object?> Output { get; }

    /// <summary>
    /// Gets a variable value with type conversion
    /// </summary>
    T? GetVariable<T>(string name);

    /// <summary>
    /// Sets a variable value
    /// </summary>
    void SetVariable(string name, object? value);

    /// <summary>
    /// Gets an input value with type conversion
    /// </summary>
    T? GetInput<T>(string name);

    /// <summary>
    /// Sets an output value
    /// </summary>
    void SetOutput(string name, object? value);

    /// <summary>
    /// Service provider for resolving dependencies
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Cancellation token for the workflow execution
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Logs execution information
    /// </summary>
    void Log(LogLevel level, string message, params object[] args);

    /// <summary>
    /// Adds an audit entry
    /// </summary>
    void AddAuditEntry(string action, string? details = null);
}

/// <summary>
/// Workflow execution status
/// </summary>
public enum WorkflowStatus
{
    /// <summary>Workflow is pending execution</summary>
    Pending = 0,

    /// <summary>Workflow is currently running</summary>
    Running = 1,

    /// <summary>Workflow is suspended waiting for external input</summary>
    Suspended = 2,

    /// <summary>Workflow completed successfully</summary>
    Completed = 3,

    /// <summary>Workflow failed with an error</summary>
    Faulted = 4,

    /// <summary>Workflow was cancelled</summary>
    Cancelled = 5,

    /// <summary>Workflow is being compensated (rollback)</summary>
    Compensating = 6,

    /// <summary>Workflow compensation completed</summary>
    Compensated = 7
}

/// <summary>
/// Log levels for workflow logging
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}
