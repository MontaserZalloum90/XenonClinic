namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Engine for evaluating business rules in workflow contexts.
/// </summary>
public interface IBusinessRulesEngine
{
    /// <summary>
    /// Evaluates a rule set and returns the result.
    /// </summary>
    Task<RuleEvaluationResult> EvaluateAsync(
        string ruleSetId,
        Dictionary<string, object> facts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates a single rule expression.
    /// </summary>
    Task<bool> EvaluateConditionAsync(
        string condition,
        Dictionary<string, object> facts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates a decision table.
    /// </summary>
    Task<DecisionTableResult> EvaluateDecisionTableAsync(
        string decisionTableId,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a rule set by ID.
    /// </summary>
    Task<RuleSetDto?> GetRuleSetAsync(
        string ruleSetId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a rule set.
    /// </summary>
    Task<RuleSetDto> SaveRuleSetAsync(
        RuleSetDto ruleSet,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists rule sets.
    /// </summary>
    Task<IList<RuleSetDto>> ListRuleSetsAsync(
        int tenantId,
        string? category = null,
        CancellationToken cancellationToken = default);
}

#region DTOs

public class RuleSetDto
{
    public string Id { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public List<RuleDto> Rules { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RuleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }

    /// <summary>
    /// Condition expression that must be true for the rule to fire.
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Actions to execute when the rule fires.
    /// </summary>
    public List<RuleActionDto> Actions { get; set; } = new();

    /// <summary>
    /// Whether to stop processing subsequent rules if this one fires.
    /// </summary>
    public bool StopOnMatch { get; set; }

    public bool IsEnabled { get; set; } = true;
}

public class RuleActionDto
{
    /// <summary>
    /// Action type: "setVariable", "invoke", "return", "log".
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Target variable name for setVariable action.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Value or expression.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Service name for invoke action.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Method name for invoke action.
    /// </summary>
    public string? MethodName { get; set; }

    /// <summary>
    /// Parameters for invoke action.
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
}

public class RuleEvaluationResult
{
    public bool Success { get; set; }
    public List<string> FiredRules { get; set; } = new();
    public Dictionary<string, object> OutputVariables { get; set; } = new();
    public object? ReturnValue { get; set; }
    public List<string> Logs { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
}

public class DecisionTableResult
{
    public bool Success { get; set; }
    public List<Dictionary<string, object>> MatchedRows { get; set; } = new();
    public Dictionary<string, object> Outputs { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

#endregion
