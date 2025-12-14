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

    /// <summary>
    /// Creates a new rule set.
    /// </summary>
    Task<RuleSet> CreateRuleSetAsync(
        CreateRuleSetRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing rule set.
    /// </summary>
    Task<RuleSet> UpdateRuleSetAsync(
        string key,
        UpdateRuleSetRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a rule set by key.
    /// </summary>
    Task<RuleSet?> GetRuleSetAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a rule set by key.
    /// </summary>
    Task DeleteRuleSetAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists rule sets by tenant ID.
    /// </summary>
    Task<IList<RuleSet>> ListRuleSetsAsync(
        string? tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a rule set.
    /// </summary>
    Task<RuleValidationResult> ValidateRuleSetAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates rules using a context object.
    /// </summary>
    Task<RuleEvaluationResult> EvaluateAsync(
        RuleEvaluationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets rule execution statistics.
    /// </summary>
    Task<RuleStatistics> GetStatisticsAsync(
        string? tenantId,
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
    public bool Executed { get; set; }
    public List<string> FiredRules { get; set; } = new();
    public List<ExecutedRule> ExecutedRules { get; set; } = new();
    public Dictionary<string, object> OutputVariables { get; set; } = new();
    public object? ReturnValue { get; set; }
    public List<string> Logs { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public long ExecutionTimeMs => (long)Duration.TotalMilliseconds;
}

public class DecisionTableResult
{
    public bool Success { get; set; }
    public List<Dictionary<string, object>> MatchedRows { get; set; } = new();
    public Dictionary<string, object> Outputs { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Rule set entity for the controller API.
/// </summary>
public class RuleSet
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public List<RuleDefinition> Rules { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Rule definition within a rule set.
/// </summary>
public class RuleDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public string Condition { get; set; } = string.Empty;
    public List<RuleActionDto> Actions { get; set; } = new();
    public bool StopOnMatch { get; set; }
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Request to create a new rule set.
/// </summary>
public class CreateRuleSetRequest
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<RuleDefinition>? Rules { get; set; }
}

/// <summary>
/// Request to update an existing rule set.
/// </summary>
public class UpdateRuleSetRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public List<RuleDefinition>? Rules { get; set; }
}

/// <summary>
/// Result of rule set validation.
/// </summary>
public class RuleValidationResult
{
    public bool IsValid { get; set; }
    public List<RuleValidationIssue> Issues { get; set; } = new();
}

/// <summary>
/// A validation issue found in a rule set.
/// </summary>
public class RuleValidationIssue
{
    public string RuleId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ValidationIssueSeverity Severity { get; set; }
}

/// <summary>
/// Severity of a validation issue.
/// </summary>
public enum ValidationIssueSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Context for evaluating rules.
/// </summary>
public class RuleEvaluationContext
{
    public string RuleSetKey { get; set; } = string.Empty;
    public Dictionary<string, object> Facts { get; set; } = new();
    public bool StopOnFirstMatch { get; set; }
}

/// <summary>
/// Statistics about rule execution.
/// </summary>
public class RuleStatistics
{
    public int TotalRuleSets { get; set; }
    public int TotalRules { get; set; }
    public long TotalEvaluations { get; set; }
    public long SuccessfulEvaluations { get; set; }
    public long FailedEvaluations { get; set; }
    public double AverageEvaluationTimeMs { get; set; }
    public DateTime? LastEvaluationAt { get; set; }
}

/// <summary>
/// Information about an executed rule.
/// </summary>
public class ExecutedRule
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public bool Matched { get; set; }
    public bool Executed { get; set; }
    public TimeSpan Duration { get; set; }
}

#endregion
