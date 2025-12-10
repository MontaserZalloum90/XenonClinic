namespace XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Base interface for all workflow activities.
/// Activities are the building blocks of workflows.
/// </summary>
public interface IActivity
{
    /// <summary>
    /// Unique identifier for this activity instance
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Human-readable name of the activity
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of what this activity does
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Activity type identifier (e.g., "task", "gateway", "event")
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Whether this activity can be compensated (rolled back)
    /// </summary>
    bool CanCompensate { get; }

    /// <summary>
    /// Executes the activity
    /// </summary>
    /// <param name="context">The workflow context</param>
    /// <returns>Result of the activity execution</returns>
    Task<ActivityResult> ExecuteAsync(IWorkflowContext context);

    /// <summary>
    /// Compensates (rolls back) the activity if supported
    /// </summary>
    /// <param name="context">The workflow context</param>
    /// <returns>Result of the compensation</returns>
    Task<ActivityResult> CompensateAsync(IWorkflowContext context);
}

/// <summary>
/// Result of an activity execution
/// </summary>
public class ActivityResult
{
    /// <summary>
    /// Whether the activity completed successfully
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Output data from the activity
    /// </summary>
    public IDictionary<string, object?>? Output { get; init; }

    /// <summary>
    /// Next activity/transition to take (for gateways/decisions)
    /// </summary>
    public string? NextActivityId { get; init; }

    /// <summary>
    /// Multiple next activities (for parallel gateways)
    /// </summary>
    public IList<string>? ParallelNextActivityIds { get; init; }

    /// <summary>
    /// Whether to suspend the workflow (for wait activities)
    /// </summary>
    public bool Suspend { get; init; }

    /// <summary>
    /// Bookmark name for resuming suspended workflows
    /// </summary>
    public string? BookmarkName { get; init; }

    /// <summary>
    /// Error information if the activity failed
    /// </summary>
    public ActivityError? Error { get; init; }

    /// <summary>
    /// Duration of the activity execution
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static ActivityResult Success(IDictionary<string, object?>? output = null)
        => new() { IsSuccess = true, Output = output };

    /// <summary>
    /// Creates a successful result with next activity
    /// </summary>
    public static ActivityResult SuccessWithNext(string nextActivityId, IDictionary<string, object?>? output = null)
        => new() { IsSuccess = true, NextActivityId = nextActivityId, Output = output };

    /// <summary>
    /// Creates a successful result for parallel execution
    /// </summary>
    public static ActivityResult Parallel(IList<string> nextActivityIds, IDictionary<string, object?>? output = null)
        => new() { IsSuccess = true, ParallelNextActivityIds = nextActivityIds, Output = output };

    /// <summary>
    /// Creates a result that suspends the workflow
    /// </summary>
    public static ActivityResult Waiting(string bookmarkName)
        => new() { IsSuccess = true, Suspend = true, BookmarkName = bookmarkName };

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static ActivityResult Failure(string errorCode, string message, Exception? exception = null)
        => new()
        {
            IsSuccess = false,
            Error = new ActivityError
            {
                Code = errorCode,
                Message = message,
                Exception = exception
            }
        };
}

/// <summary>
/// Activity execution error information
/// </summary>
public class ActivityError
{
    /// <summary>
    /// Error code for categorization
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional exception that caused the error
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Additional error details
    /// </summary>
    public IDictionary<string, object?>? Details { get; init; }
}

/// <summary>
/// Interface for activities that support async execution
/// </summary>
public interface IAsyncActivity : IActivity
{
    /// <summary>
    /// Timeout for the activity execution
    /// </summary>
    TimeSpan? Timeout { get; }

    /// <summary>
    /// Retry policy for failed executions
    /// </summary>
    RetryPolicy? RetryPolicy { get; }
}

/// <summary>
/// Retry policy configuration
/// </summary>
public class RetryPolicy
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Initial delay between retries
    /// </summary>
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum delay between retries
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Exponential backoff multiplier
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Error codes that should trigger a retry
    /// </summary>
    public IList<string>? RetryableErrorCodes { get; init; }
}
