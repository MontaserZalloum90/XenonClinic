using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.WorkflowEngine.Application.Services;

namespace XenonClinic.WorkflowEngine.Api.Controllers;

/// <summary>
/// API controller for business rules operations.
/// </summary>
[ApiController]
[Route("api/workflow/rules")]
[Authorize(Policy = "ProcessDesigner")]
public class RulesController : ControllerBase
{
    private readonly IBusinessRulesEngine _rulesEngine;

    public RulesController(IBusinessRulesEngine rulesEngine)
    {
        _rulesEngine = rulesEngine;
    }

    /// <summary>
    /// Creates a new rule set.
    /// </summary>
    [HttpPost("sets")]
    public async Task<ActionResult<RuleSet>> CreateRuleSet(
        [FromBody] CreateRuleSetRequest request,
        CancellationToken cancellationToken)
    {
        var ruleSet = await _rulesEngine.CreateRuleSetAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRuleSet), new { key = ruleSet.Key }, ruleSet);
    }

    /// <summary>
    /// Gets a rule set by key.
    /// </summary>
    [HttpGet("sets/{key}")]
    public async Task<ActionResult<RuleSet>> GetRuleSet(
        string key,
        CancellationToken cancellationToken)
    {
        var ruleSet = await _rulesEngine.GetRuleSetAsync(key, cancellationToken);
        if (ruleSet == null)
        {
            return NotFound(new { message = $"Rule set '{key}' not found" });
        }
        return Ok(ruleSet);
    }

    /// <summary>
    /// Updates a rule set.
    /// </summary>
    [HttpPut("sets/{key}")]
    public async Task<ActionResult<RuleSet>> UpdateRuleSet(
        string key,
        [FromBody] UpdateRuleSetRequest request,
        CancellationToken cancellationToken)
    {
        var ruleSet = await _rulesEngine.UpdateRuleSetAsync(key, request, cancellationToken);
        return Ok(ruleSet);
    }

    /// <summary>
    /// Deletes a rule set.
    /// </summary>
    [HttpDelete("sets/{key}")]
    public async Task<IActionResult> DeleteRuleSet(
        string key,
        CancellationToken cancellationToken)
    {
        await _rulesEngine.DeleteRuleSetAsync(key, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Lists all rule sets.
    /// </summary>
    [HttpGet("sets")]
    public async Task<ActionResult<IList<RuleSet>>> ListRuleSets(
        [FromQuery] string? tenantId,
        CancellationToken cancellationToken)
    {
        var ruleSets = await _rulesEngine.ListRuleSetsAsync(tenantId, cancellationToken);
        return Ok(ruleSets);
    }

    /// <summary>
    /// Validates a rule set.
    /// </summary>
    [HttpPost("sets/{key}/validate")]
    public async Task<ActionResult<RuleValidationResult>> ValidateRuleSet(
        string key,
        CancellationToken cancellationToken)
    {
        var result = await _rulesEngine.ValidateRuleSetAsync(key, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Evaluates a rule set with provided facts.
    /// </summary>
    [HttpPost("evaluate")]
    [AllowAnonymous] // Allow evaluation from process execution
    public async Task<ActionResult<RuleEvaluationResult>> EvaluateRuleSet(
        [FromBody] RuleEvaluationContext context,
        CancellationToken cancellationToken)
    {
        var result = await _rulesEngine.EvaluateAsync(context, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tests a rule set with sample data.
    /// </summary>
    [HttpPost("sets/{key}/test")]
    public async Task<ActionResult<RuleTestResult>> TestRuleSet(
        string key,
        [FromBody] RuleTestRequest request,
        CancellationToken cancellationToken)
    {
        var context = new RuleEvaluationContext
        {
            RuleSetKey = key,
            Facts = request.Facts
        };

        var result = await _rulesEngine.EvaluateAsync(context, cancellationToken);

        return Ok(new RuleTestResult
        {
            Success = result.Executed,
            ExecutedRules = result.ExecutedRules,
            OutputVariables = result.OutputVariables,
            ExecutionTimeMs = result.ExecutionTimeMs
        });
    }

    /// <summary>
    /// Adds a rule to an existing rule set.
    /// </summary>
    [HttpPost("sets/{key}/rules")]
    public async Task<ActionResult<RuleSet>> AddRule(
        string key,
        [FromBody] RuleDefinition rule,
        CancellationToken cancellationToken)
    {
        var ruleSet = await _rulesEngine.GetRuleSetAsync(key, cancellationToken);
        if (ruleSet == null)
        {
            return NotFound(new { message = $"Rule set '{key}' not found" });
        }

        ruleSet.Rules.Add(rule);
        var updated = await _rulesEngine.UpdateRuleSetAsync(key, new UpdateRuleSetRequest
        {
            Rules = ruleSet.Rules
        }, cancellationToken);

        return Ok(updated);
    }

    /// <summary>
    /// Updates a specific rule within a rule set.
    /// </summary>
    [HttpPut("sets/{key}/rules/{ruleId}")]
    public async Task<ActionResult<RuleSet>> UpdateRule(
        string key,
        string ruleId,
        [FromBody] RuleDefinition rule,
        CancellationToken cancellationToken)
    {
        var ruleSet = await _rulesEngine.GetRuleSetAsync(key, cancellationToken);
        if (ruleSet == null)
        {
            return NotFound(new { message = $"Rule set '{key}' not found" });
        }

        var index = ruleSet.Rules.FindIndex(r => r.Id == ruleId);
        if (index < 0)
        {
            return NotFound(new { message = $"Rule '{ruleId}' not found in rule set" });
        }

        rule.Id = ruleId;
        ruleSet.Rules[index] = rule;

        var updated = await _rulesEngine.UpdateRuleSetAsync(key, new UpdateRuleSetRequest
        {
            Rules = ruleSet.Rules
        }, cancellationToken);

        return Ok(updated);
    }

    /// <summary>
    /// Deletes a rule from a rule set.
    /// </summary>
    [HttpDelete("sets/{key}/rules/{ruleId}")]
    public async Task<ActionResult<RuleSet>> DeleteRule(
        string key,
        string ruleId,
        CancellationToken cancellationToken)
    {
        var ruleSet = await _rulesEngine.GetRuleSetAsync(key, cancellationToken);
        if (ruleSet == null)
        {
            return NotFound(new { message = $"Rule set '{key}' not found" });
        }

        var rule = ruleSet.Rules.Find(r => r.Id == ruleId);
        if (rule == null)
        {
            return NotFound(new { message = $"Rule '{ruleId}' not found in rule set" });
        }

        ruleSet.Rules.Remove(rule);

        var updated = await _rulesEngine.UpdateRuleSetAsync(key, new UpdateRuleSetRequest
        {
            Rules = ruleSet.Rules
        }, cancellationToken);

        return Ok(updated);
    }

    /// <summary>
    /// Gets rule execution statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<RuleStatistics>> GetStatistics(
        [FromQuery] string? tenantId,
        CancellationToken cancellationToken)
    {
        var stats = await _rulesEngine.GetStatisticsAsync(tenantId, cancellationToken);
        return Ok(stats);
    }
}

public class RuleTestRequest
{
    public Dictionary<string, object> Facts { get; set; } = new();
}

public class RuleTestResult
{
    public bool Success { get; set; }
    public List<ExecutedRule> ExecutedRules { get; set; } = new();
    public Dictionary<string, object> OutputVariables { get; set; } = new();
    public long ExecutionTimeMs { get; set; }
}
