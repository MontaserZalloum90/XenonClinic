namespace XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Represents a transition between activities in a workflow.
/// </summary>
public interface ITransition
{
    /// <summary>
    /// Unique identifier for this transition
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Source activity ID
    /// </summary>
    string SourceActivityId { get; }

    /// <summary>
    /// Target activity ID
    /// </summary>
    string TargetActivityId { get; }

    /// <summary>
    /// Optional name/label for the transition
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Condition expression that must be true for this transition to be taken
    /// </summary>
    string? Condition { get; }

    /// <summary>
    /// Priority for evaluating transitions (lower = higher priority)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Whether this is the default transition when no conditions match
    /// </summary>
    bool IsDefault { get; }

    /// <summary>
    /// Guards that must pass for this transition to be allowed
    /// </summary>
    IList<ITransitionGuard>? Guards { get; }

    /// <summary>
    /// Actions to execute when this transition is taken
    /// </summary>
    IList<ITransitionAction>? Actions { get; }
}

/// <summary>
/// A guard that determines if a transition can be taken.
/// </summary>
public interface ITransitionGuard
{
    /// <summary>
    /// Guard identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Guard name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Evaluates whether the transition is allowed
    /// </summary>
    /// <param name="context">The workflow context</param>
    /// <returns>True if the transition is allowed</returns>
    Task<bool> EvaluateAsync(IWorkflowContext context);
}

/// <summary>
/// An action that executes when a transition is taken.
/// </summary>
public interface ITransitionAction
{
    /// <summary>
    /// Action identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Action name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <param name="context">The workflow context</param>
    Task ExecuteAsync(IWorkflowContext context);
}

/// <summary>
/// Expression-based transition guard
/// </summary>
public class ExpressionGuard : ITransitionGuard
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "Expression Guard";
    public required string Expression { get; init; }

    private readonly Func<IWorkflowContext, Task<bool>>? _evaluator;

    public ExpressionGuard() { }

    public ExpressionGuard(string expression, Func<IWorkflowContext, Task<bool>> evaluator)
    {
        Expression = expression;
        _evaluator = evaluator;
    }

    public Task<bool> EvaluateAsync(IWorkflowContext context)
    {
        if (_evaluator != null)
        {
            return _evaluator(context);
        }

        // Default implementation uses expression evaluation
        // This will be replaced by the expression evaluator service
        return Task.FromResult(true);
    }
}

/// <summary>
/// Expression-based transition action
/// </summary>
public class ExpressionAction : ITransitionAction
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = "Expression Action";
    public required string Expression { get; init; }

    private readonly Func<IWorkflowContext, Task>? _executor;

    public ExpressionAction() { }

    public ExpressionAction(string expression, Func<IWorkflowContext, Task> executor)
    {
        Expression = expression;
        _executor = executor;
    }

    public Task ExecuteAsync(IWorkflowContext context)
    {
        if (_executor != null)
        {
            return _executor(context);
        }

        // Default implementation - no-op
        return Task.CompletedTask;
    }
}
