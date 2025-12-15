namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

/// <summary>
/// Business rules engine implementation.
/// </summary>
public class BusinessRulesEngine : IBusinessRulesEngine
{
    private readonly IExpressionEvaluator _expressionEvaluator;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BusinessRulesEngine> _logger;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, RuleSetDto> _ruleSets = new();
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public BusinessRulesEngine(
        IExpressionEvaluator expressionEvaluator,
        IMemoryCache cache,
        ILogger<BusinessRulesEngine> logger)
    {
        _expressionEvaluator = expressionEvaluator;
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<RuleEvaluationResult> EvaluateAsync(
        string ruleSetId,
        Dictionary<string, object> facts,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new RuleEvaluationResult { Success = true };

        try
        {
            // Get rule set (would typically come from database)
            if (!_ruleSets.TryGetValue(ruleSetId, out var ruleSet))
            {
                result.Success = false;
                result.ErrorMessage = $"Rule set '{ruleSetId}' not found";
                return result;
            }

            if (!ruleSet.IsActive)
            {
                result.Success = false;
                result.ErrorMessage = $"Rule set '{ruleSetId}' is not active";
                return result;
            }

            // Create working copy of facts
            var workingFacts = new Dictionary<string, object>(facts);

            // Evaluate rules in priority order
            var orderedRules = (ruleSet.Rules ?? Enumerable.Empty<RuleDto>())
                .Where(r => r != null && r.IsEnabled)
                .OrderBy(r => r.Priority)
                .ToList();

            foreach (var rule in orderedRules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Skip rules with empty conditions
                    if (string.IsNullOrWhiteSpace(rule.Condition))
                    {
                        result.Logs.Add($"Skipping rule '{rule.Name}' - no condition defined");
                        continue;
                    }

                    var conditionResult = await _expressionEvaluator.EvaluateConditionAsync(
                        rule.Condition, workingFacts);

                    if (conditionResult)
                    {
                        result.FiredRules.Add(rule.Name ?? rule.Id);
                        result.Logs.Add($"Rule '{rule.Name}' fired");

                        // Execute actions (handle null actions list)
                        var actions = rule.Actions ?? Enumerable.Empty<RuleActionDto>();
                        foreach (var action in actions)
                        {
                            if (action == null) continue;
                            await ExecuteActionAsync(action, workingFacts, result, cancellationToken);
                        }

                        if (rule.StopOnMatch)
                        {
                            result.Logs.Add($"Processing stopped after rule '{rule.Name}'");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error evaluating rule '{RuleName}' in set '{RuleSetId}'",
                        rule.Name, ruleSetId);
                    result.Logs.Add($"Error in rule '{rule.Name}': {ex.Message}");
                }
            }

            // Copy output variables
            foreach (var kvp in workingFacts)
            {
                if (!facts.ContainsKey(kvp.Key))
                {
                    result.OutputVariables[kvp.Key] = kvp.Value;
                }
            }

            _logger.LogInformation(
                "Evaluated rule set '{RuleSetId}': {FiredCount} rules fired in {Duration}ms",
                ruleSetId, result.FiredRules.Count, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating rule set '{RuleSetId}'", ruleSetId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        result.Duration = stopwatch.Elapsed;
        return result;
    }

    public async Task<bool> EvaluateConditionAsync(
        string condition,
        Dictionary<string, object> facts,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _expressionEvaluator.EvaluateConditionAsync(condition, facts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating condition: {Condition}", condition);
            return false;
        }
    }

    public async Task<DecisionTableResult> EvaluateDecisionTableAsync(
        string decisionTableId,
        Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default)
    {
        var result = new DecisionTableResult { Success = true };

        try
        {
            // Decision table would be loaded from storage
            // For now, return empty result
            _logger.LogInformation(
                "Evaluating decision table '{DecisionTableId}' with {InputCount} inputs",
                decisionTableId, inputs.Count);

            // Would evaluate each row's input columns against the inputs
            // and return matching rows' output values
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating decision table '{DecisionTableId}'", decisionTableId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public Task<RuleSetDto?> GetRuleSetAsync(
        string ruleSetId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        if (_ruleSets.TryGetValue(ruleSetId, out var ruleSet) && ruleSet.TenantId == tenantId)
        {
            return Task.FromResult<RuleSetDto?>(ruleSet);
        }

        return Task.FromResult<RuleSetDto?>(null);
    }

    public Task<RuleSetDto> SaveRuleSetAsync(
        RuleSetDto ruleSet,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        if (string.IsNullOrEmpty(ruleSet.Id))
        {
            ruleSet.Id = Guid.NewGuid().ToString();
            ruleSet.CreatedAt = now;
        }

        ruleSet.UpdatedAt = now;
        ruleSet.Version++;

        _ruleSets[ruleSet.Id] = ruleSet;

        // Clear cache
        _cache.Remove($"ruleset:{ruleSet.Id}");

        _logger.LogInformation(
            "Saved rule set '{RuleSetId}' version {Version}",
            ruleSet.Id, ruleSet.Version);

        return Task.FromResult(ruleSet);
    }

    public Task<IList<RuleSetDto>> ListRuleSetsAsync(
        int tenantId,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = _ruleSets.Values.Where(r => r.TenantId == tenantId);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(r => r.Category == category);
        }

        return Task.FromResult<IList<RuleSetDto>>(query.ToList());
    }

    // In-memory storage for RuleSet entities (using key-based access)
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, RuleSet> _ruleSetsByKey = new();
    private long _totalEvaluations;
    private long _successfulEvaluations;
    private long _failedEvaluations;
    private DateTime? _lastEvaluationAt;

    public Task<RuleSet> CreateRuleSetAsync(
        CreateRuleSetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
        {
            throw new ArgumentException("Rule set key is required", nameof(request));
        }

        if (_ruleSetsByKey.ContainsKey(request.Key))
        {
            throw new InvalidOperationException($"Rule set with key '{request.Key}' already exists");
        }

        var ruleSet = new RuleSet
        {
            Id = Guid.NewGuid().ToString(),
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Rules = request.Rules ?? new List<RuleDefinition>(),
            IsActive = true,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _ruleSetsByKey[request.Key] = ruleSet;

        _logger.LogInformation("Created rule set {Key} with ID {RuleSetId}", request.Key, ruleSet.Id);

        return Task.FromResult(ruleSet);
    }

    public Task<RuleSet> UpdateRuleSetAsync(
        string key,
        UpdateRuleSetRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_ruleSetsByKey.TryGetValue(key, out var ruleSet))
        {
            throw new InvalidOperationException($"Rule set with key '{key}' not found");
        }

        if (request.Name != null) ruleSet.Name = request.Name;
        if (request.Description != null) ruleSet.Description = request.Description;
        if (request.Category != null) ruleSet.Category = request.Category;
        if (request.IsActive.HasValue) ruleSet.IsActive = request.IsActive.Value;
        if (request.Rules != null) ruleSet.Rules = request.Rules;

        ruleSet.UpdatedAt = DateTime.UtcNow;
        ruleSet.Version++;

        _logger.LogInformation("Updated rule set {Key} to version {Version}", key, ruleSet.Version);

        return Task.FromResult(ruleSet);
    }

    public Task<RuleSet?> GetRuleSetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        _ruleSetsByKey.TryGetValue(key, out var ruleSet);
        return Task.FromResult(ruleSet);
    }

    public Task DeleteRuleSetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (!_ruleSetsByKey.TryRemove(key, out _))
        {
            throw new InvalidOperationException($"Rule set with key '{key}' not found");
        }

        _logger.LogInformation("Deleted rule set {Key}", key);

        return Task.CompletedTask;
    }

    public Task<IList<RuleSet>> ListRuleSetsAsync(
        string? tenantId,
        CancellationToken cancellationToken = default)
    {
        // For now, return all rule sets (tenant filtering would be applied in production)
        var ruleSets = _ruleSetsByKey.Values.ToList();
        return Task.FromResult<IList<RuleSet>>(ruleSets);
    }

    public Task<RuleValidationResult> ValidateRuleSetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (!_ruleSetsByKey.TryGetValue(key, out var ruleSet))
        {
            return Task.FromResult(new RuleValidationResult
            {
                IsValid = false,
                Issues = new List<RuleValidationIssue>
                {
                    new RuleValidationIssue
                    {
                        Code = "RULE_SET_NOT_FOUND",
                        Message = $"Rule set with key '{key}' not found",
                        Severity = ValidationIssueSeverity.Error
                    }
                }
            });
        }

        var issues = new List<RuleValidationIssue>();

        // Validate each rule
        foreach (var rule in ruleSet.Rules ?? Enumerable.Empty<RuleDefinition>())
        {
            if (string.IsNullOrWhiteSpace(rule.Condition))
            {
                issues.Add(new RuleValidationIssue
                {
                    RuleId = rule.Id,
                    Code = "EMPTY_CONDITION",
                    Message = $"Rule '{rule.Name}' has no condition defined",
                    Severity = ValidationIssueSeverity.Warning
                });
            }

            if ((rule.Actions == null || rule.Actions.Count == 0) && !rule.StopOnMatch)
            {
                issues.Add(new RuleValidationIssue
                {
                    RuleId = rule.Id,
                    Code = "NO_ACTIONS",
                    Message = $"Rule '{rule.Name}' has no actions defined",
                    Severity = ValidationIssueSeverity.Info
                });
            }
        }

        return Task.FromResult(new RuleValidationResult
        {
            IsValid = !issues.Any(i => i.Severity == ValidationIssueSeverity.Error),
            Issues = issues
        });
    }

    public async Task<RuleEvaluationResult> EvaluateAsync(
        RuleEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _totalEvaluations);
        _lastEvaluationAt = DateTime.UtcNow;

        if (!_ruleSetsByKey.TryGetValue(context.RuleSetKey, out var ruleSet))
        {
            Interlocked.Increment(ref _failedEvaluations);
            return new RuleEvaluationResult
            {
                Success = false,
                ErrorMessage = $"Rule set '{context.RuleSetKey}' not found"
            };
        }

        // Convert RuleSet to RuleSetDto for evaluation
        var ruleSetDto = new RuleSetDto
        {
            Id = ruleSet.Key,
            Name = ruleSet.Name,
            IsActive = ruleSet.IsActive,
            Rules = ruleSet.Rules?.Select(r => new RuleDto
            {
                Id = r.Id,
                Name = r.Name,
                Priority = r.Priority,
                Condition = r.Condition,
                Actions = r.Actions,
                StopOnMatch = context.StopOnFirstMatch || r.StopOnMatch,
                IsEnabled = r.IsEnabled
            }).ToList() ?? new List<RuleDto>()
        };

        // Store temporarily and evaluate
        var originalRuleSet = _ruleSets.GetValueOrDefault(ruleSet.Key);
        _ruleSets[ruleSet.Key] = ruleSetDto;

        try
        {
            var result = await EvaluateAsync(ruleSet.Key, context.Facts, cancellationToken);
            if (result.Success)
                Interlocked.Increment(ref _successfulEvaluations);
            else
                Interlocked.Increment(ref _failedEvaluations);
            return result;
        }
        finally
        {
            // Restore original or remove
            if (originalRuleSet != null)
                _ruleSets[ruleSet.Key] = originalRuleSet;
            else
                _ruleSets.TryRemove(ruleSet.Key, out _);
        }
    }

    public Task<RuleStatistics> GetStatisticsAsync(
        string? tenantId,
        CancellationToken cancellationToken = default)
    {
        var stats = new RuleStatistics
        {
            TotalRuleSets = _ruleSetsByKey.Count + _ruleSets.Count,
            TotalRules = _ruleSetsByKey.Values.Sum(r => r.Rules?.Count ?? 0) +
                         _ruleSets.Values.Sum(r => r.Rules?.Count ?? 0),
            TotalEvaluations = Interlocked.Read(ref _totalEvaluations),
            SuccessfulEvaluations = Interlocked.Read(ref _successfulEvaluations),
            FailedEvaluations = Interlocked.Read(ref _failedEvaluations),
            AverageEvaluationTimeMs = 0, // Would need to track this over time
            LastEvaluationAt = _lastEvaluationAt
        };

        return Task.FromResult(stats);
    }

    #region Private Methods

    private async Task ExecuteActionAsync(
        RuleActionDto action,
        Dictionary<string, object> facts,
        RuleEvaluationResult result,
        CancellationToken cancellationToken)
    {
        switch (action.Type.ToLowerInvariant())
        {
            case "setvariable":
                await ExecuteSetVariableAsync(action, facts, result);
                break;

            case "return":
                result.ReturnValue = await EvaluateValueAsync(action.Value, facts);
                break;

            case "log":
                var logMessage = action.Value?.ToString() ?? "";
                result.Logs.Add(logMessage);
                _logger.LogInformation("Rule log: {Message}", logMessage);
                break;

            case "invoke":
                await ExecuteInvokeAsync(action, facts, result, cancellationToken);
                break;

            default:
                _logger.LogWarning("Unknown rule action type: {ActionType}", action.Type);
                break;
        }
    }

    private async Task ExecuteSetVariableAsync(
        RuleActionDto action,
        Dictionary<string, object> facts,
        RuleEvaluationResult result)
    {
        if (string.IsNullOrEmpty(action.Target))
            return;

        var value = await EvaluateValueAsync(action.Value, facts);
        facts[action.Target] = value;
        result.OutputVariables[action.Target] = value;

        result.Logs.Add($"Set variable '{action.Target}' = {value}");
    }

    private Task ExecuteInvokeAsync(
        RuleActionDto action,
        Dictionary<string, object> facts,
        RuleEvaluationResult result,
        CancellationToken cancellationToken)
    {
        // Would invoke a registered service
        result.Logs.Add($"Would invoke {action.ServiceName}.{action.MethodName}");
        return Task.CompletedTask;
    }

    private async Task<object?> EvaluateValueAsync(object? value, Dictionary<string, object> facts)
    {
        if (value == null)
            return null;

        if (value is string strValue)
        {
            // Check if it's an expression
            if (strValue.StartsWith("${") || strValue.StartsWith("#{"))
            {
                try
                {
                    return await _expressionEvaluator.EvaluateAsync(strValue, facts);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to evaluate expression: {Expression}", strValue);
                    return strValue; // Return original value on failure
                }
            }
        }

        return value;
    }

    #endregion
}
