namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Evaluates expressions in workflow contexts.
/// </summary>
public interface IExpressionEvaluator
{
    /// <summary>
    /// Evaluates an expression with the given context.
    /// </summary>
    Task<object> EvaluateAsync(string expression, IDictionary<string, object> context);

    /// <summary>
    /// Evaluates an expression and returns a typed result.
    /// </summary>
    Task<T> EvaluateAsync<T>(string expression, IDictionary<string, object> context);

    /// <summary>
    /// Evaluates a boolean condition.
    /// </summary>
    Task<bool> EvaluateConditionAsync(string expression, IDictionary<string, object> context);

    /// <summary>
    /// Validates an expression syntax without evaluating it.
    /// </summary>
    ExpressionValidationResult Validate(string expression);
}

public class ExpressionValidationResult
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public List<string> Variables { get; set; } = new();
}
