namespace XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Represents a workflow definition containing all activities and transitions.
/// This is the blueprint for creating workflow instances.
/// </summary>
public interface IWorkflowDefinition
{
    /// <summary>
    /// Unique identifier for this workflow definition
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Human-readable name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of what this workflow does
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Version number (incremented on changes)
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Category for organization
    /// </summary>
    string? Category { get; }

    /// <summary>
    /// Tags for searching/filtering
    /// </summary>
    IList<string>? Tags { get; }

    /// <summary>
    /// Whether this workflow definition is active
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Whether this is a draft version
    /// </summary>
    bool IsDraft { get; }

    /// <summary>
    /// The starting activity ID
    /// </summary>
    string StartActivityId { get; }

    /// <summary>
    /// All activities in this workflow
    /// </summary>
    IReadOnlyDictionary<string, IActivity> Activities { get; }

    /// <summary>
    /// All transitions between activities
    /// </summary>
    IReadOnlyList<ITransition> Transitions { get; }

    /// <summary>
    /// Input parameter definitions
    /// </summary>
    IReadOnlyList<WorkflowParameter>? InputParameters { get; }

    /// <summary>
    /// Output parameter definitions
    /// </summary>
    IReadOnlyList<WorkflowParameter>? OutputParameters { get; }

    /// <summary>
    /// Variable definitions
    /// </summary>
    IReadOnlyList<WorkflowVariable>? Variables { get; }

    /// <summary>
    /// Global error handlers
    /// </summary>
    IReadOnlyList<ErrorHandler>? ErrorHandlers { get; }

    /// <summary>
    /// Triggers that can start this workflow
    /// </summary>
    IReadOnlyList<WorkflowTrigger>? Triggers { get; }

    /// <summary>
    /// Tenant ID for multi-tenant workflows (null for global)
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// Metadata for the workflow
    /// </summary>
    IDictionary<string, object?>? Metadata { get; }

    /// <summary>
    /// When this definition was created
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// When this definition was last modified
    /// </summary>
    DateTime? ModifiedAt { get; }

    /// <summary>
    /// Gets an activity by ID
    /// </summary>
    IActivity? GetActivity(string activityId);

    /// <summary>
    /// Gets outgoing transitions from an activity
    /// </summary>
    IReadOnlyList<ITransition> GetOutgoingTransitions(string activityId);

    /// <summary>
    /// Gets incoming transitions to an activity
    /// </summary>
    IReadOnlyList<ITransition> GetIncomingTransitions(string activityId);

    /// <summary>
    /// Validates the workflow definition
    /// </summary>
    ValidationResult Validate();
}

/// <summary>
/// Workflow parameter definition
/// </summary>
public class WorkflowParameter
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Parameter display name
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Parameter description
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Parameter type (string, number, boolean, object, array)
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Whether this parameter is required
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Default value if not provided
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Validation schema (JSON Schema format)
    /// </summary>
    public string? Schema { get; init; }
}

/// <summary>
/// Workflow variable definition
/// </summary>
public class WorkflowVariable
{
    /// <summary>
    /// Variable name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Variable type
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Default value
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Scope of the variable (workflow, activity)
    /// </summary>
    public VariableScope Scope { get; init; } = VariableScope.Workflow;
}

/// <summary>
/// Variable scope
/// </summary>
public enum VariableScope
{
    /// <summary>Available throughout the workflow</summary>
    Workflow,
    /// <summary>Available only within a specific activity</summary>
    Activity
}

/// <summary>
/// Error handler definition
/// </summary>
public class ErrorHandler
{
    /// <summary>
    /// Error codes to handle (empty = all errors)
    /// </summary>
    public IList<string>? ErrorCodes { get; init; }

    /// <summary>
    /// Activity to execute when error occurs
    /// </summary>
    public string? HandlerActivityId { get; init; }

    /// <summary>
    /// Whether to retry the failed activity
    /// </summary>
    public RetryPolicy? RetryPolicy { get; init; }

    /// <summary>
    /// Whether to compensate (rollback) on error
    /// </summary>
    public bool Compensate { get; init; }

    /// <summary>
    /// Whether to terminate the workflow on error
    /// </summary>
    public bool Terminate { get; init; }
}

/// <summary>
/// Workflow trigger definition
/// </summary>
public class WorkflowTrigger
{
    /// <summary>
    /// Trigger type (manual, scheduled, event, webhook)
    /// </summary>
    public required TriggerType Type { get; init; }

    /// <summary>
    /// Trigger name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Trigger configuration
    /// </summary>
    public IDictionary<string, object?>? Configuration { get; init; }

    /// <summary>
    /// Whether this trigger is enabled
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}

/// <summary>
/// Trigger types
/// </summary>
public enum TriggerType
{
    /// <summary>Manually triggered</summary>
    Manual,
    /// <summary>Scheduled (cron expression)</summary>
    Scheduled,
    /// <summary>Event-based trigger</summary>
    Event,
    /// <summary>Webhook trigger</summary>
    Webhook,
    /// <summary>Entity change trigger</summary>
    EntityChange,
    /// <summary>Signal-based trigger</summary>
    Signal
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether the validation passed
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Validation errors
    /// </summary>
    public IList<ValidationError> Errors { get; init; } = new List<ValidationError>();

    /// <summary>
    /// Validation warnings (non-blocking)
    /// </summary>
    public IList<ValidationWarning> Warnings { get; init; } = new List<ValidationWarning>();

    public static ValidationResult Success() => new();

    public static ValidationResult Failed(params ValidationError[] errors)
        => new() { Errors = errors.ToList() };
}

/// <summary>
/// Validation error
/// </summary>
public class ValidationError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? PropertyPath { get; init; }
    public string? ActivityId { get; init; }
}

/// <summary>
/// Validation warning
/// </summary>
public class ValidationWarning
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? PropertyPath { get; init; }
    public string? ActivityId { get; init; }
}
