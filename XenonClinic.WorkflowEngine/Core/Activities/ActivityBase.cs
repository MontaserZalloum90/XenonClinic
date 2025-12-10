namespace XenonClinic.WorkflowEngine.Core.Activities;

using XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Base class for all activities providing common functionality.
/// </summary>
public abstract class ActivityBase : IActivity
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public abstract string Type { get; }
    public virtual bool CanCompensate => false;

    /// <summary>
    /// Input mappings (source expression -> activity input)
    /// </summary>
    public IDictionary<string, string>? InputMappings { get; init; }

    /// <summary>
    /// Output mappings (activity output -> target variable)
    /// </summary>
    public IDictionary<string, string>? OutputMappings { get; init; }

    public abstract Task<ActivityResult> ExecuteAsync(IWorkflowContext context);

    public virtual Task<ActivityResult> CompensateAsync(IWorkflowContext context)
    {
        return Task.FromResult(ActivityResult.Success());
    }

    /// <summary>
    /// Maps inputs from context to activity
    /// </summary>
    protected IDictionary<string, object?> MapInputs(IWorkflowContext context)
    {
        var inputs = new Dictionary<string, object?>();
        if (InputMappings != null)
        {
            foreach (var mapping in InputMappings)
            {
                var value = ResolveExpression(mapping.Value, context);
                inputs[mapping.Key] = value;
            }
        }
        return inputs;
    }

    /// <summary>
    /// Maps outputs from activity to context
    /// </summary>
    protected void MapOutputs(IDictionary<string, object?>? output, IWorkflowContext context)
    {
        if (output == null || OutputMappings == null) return;

        foreach (var mapping in OutputMappings)
        {
            if (output.TryGetValue(mapping.Key, out var value))
            {
                context.SetVariable(mapping.Value, value);
            }
        }
    }

    /// <summary>
    /// Resolves a simple expression from context
    /// </summary>
    protected object? ResolveExpression(string expression, IWorkflowContext context)
    {
        // Simple expression resolution - can be extended with full expression evaluator
        if (expression.StartsWith("input."))
        {
            var key = expression[6..];
            return context.Input.TryGetValue(key, out var value) ? value : null;
        }
        if (expression.StartsWith("var."))
        {
            var key = expression[4..];
            return context.Variables.TryGetValue(key, out var value) ? value : null;
        }
        if (expression.StartsWith("output."))
        {
            var key = expression[7..];
            return context.Output.TryGetValue(key, out var value) ? value : null;
        }
        // Literal value
        return expression;
    }
}

/// <summary>
/// Activity that can be resumed after suspension
/// </summary>
public abstract class ResumableActivityBase : ActivityBase
{
    /// <summary>
    /// Bookmark name for resumption
    /// </summary>
    public string? BookmarkName { get; init; }

    /// <summary>
    /// Resumes the activity with provided input
    /// </summary>
    public abstract Task<ActivityResult> ResumeAsync(IWorkflowContext context, IDictionary<string, object?>? input);
}
