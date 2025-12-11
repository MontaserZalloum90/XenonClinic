using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using XenonClinic.WorkflowEngine.Application.Services;
using XenonClinic.WorkflowEngine.Tests.Testing;

namespace XenonClinic.WorkflowEngine.Tests.Services;

/// <summary>
/// Unit tests for the BusinessRulesEngine.
/// </summary>
public class BusinessRulesEngineTests : IDisposable
{
    private readonly WorkflowTestFixture _fixture;

    public BusinessRulesEngineTests()
    {
        _fixture = new WorkflowTestFixture();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task CreateRuleSetAsync_ShouldCreateRuleSet()
    {
        // Arrange
        var request = new CreateRuleSetRequest
        {
            TenantId = "test-tenant",
            Key = "approval-rules",
            Name = "Approval Rules",
            Description = "Rules for approval workflow",
            Rules = new List<RuleDefinition>
            {
                new RuleDefinition
                {
                    Id = "rule1",
                    Name = "Auto-approve small amounts",
                    Priority = 1,
                    Condition = "amount < 1000",
                    Actions = new List<RuleAction>
                    {
                        new RuleAction { Type = "setVariable", Target = "approved", Value = "true" }
                    }
                }
            }
        };

        // Act
        var result = await _fixture.RulesEngine.CreateRuleSetAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("approval-rules", result.Key);
        Assert.Single(result.Rules);
    }

    [Fact]
    public async Task EvaluateRuleSetAsync_WithMatchingCondition_ShouldExecuteActions()
    {
        // Arrange
        var ruleSet = await CreateTestRuleSetAsync("eval-test");
        var context = new RuleEvaluationContext
        {
            RuleSetKey = "eval-test",
            Facts = new Dictionary<string, object>
            {
                ["amount"] = 500 // Less than 1000, should match
            }
        };

        // Act
        var result = await _fixture.RulesEngine.EvaluateAsync(context);

        // Assert
        Assert.True(result.Executed);
        Assert.Contains(result.ExecutedRules, r => r.RuleId == "auto-approve");
        Assert.True(result.OutputVariables.ContainsKey("approved"));
    }

    [Fact]
    public async Task EvaluateRuleSetAsync_WithNoMatchingCondition_ShouldNotExecuteActions()
    {
        // Arrange
        var ruleSet = await CreateTestRuleSetAsync("no-match-test");
        var context = new RuleEvaluationContext
        {
            RuleSetKey = "no-match-test",
            Facts = new Dictionary<string, object>
            {
                ["amount"] = 5000 // Greater than 1000, should not match
            }
        };

        // Act
        var result = await _fixture.RulesEngine.EvaluateAsync(context);

        // Assert
        Assert.Empty(result.ExecutedRules.FindAll(r => r.RuleId == "auto-approve"));
    }

    [Fact]
    public async Task EvaluateRuleSetAsync_WithMultipleRules_ShouldEvaluateInPriorityOrder()
    {
        // Arrange
        var request = new CreateRuleSetRequest
        {
            TenantId = "test-tenant",
            Key = "priority-test",
            Name = "Priority Test Rules",
            Rules = new List<RuleDefinition>
            {
                new RuleDefinition
                {
                    Id = "low-priority",
                    Name = "Low Priority Rule",
                    Priority = 10,
                    Condition = "true",
                    Actions = new List<RuleAction>
                    {
                        new RuleAction { Type = "setVariable", Target = "lastRule", Value = "low" }
                    }
                },
                new RuleDefinition
                {
                    Id = "high-priority",
                    Name = "High Priority Rule",
                    Priority = 1,
                    Condition = "true",
                    Actions = new List<RuleAction>
                    {
                        new RuleAction { Type = "setVariable", Target = "firstRule", Value = "high" }
                    }
                }
            }
        };

        await _fixture.RulesEngine.CreateRuleSetAsync(request);

        var context = new RuleEvaluationContext
        {
            RuleSetKey = "priority-test",
            Facts = new Dictionary<string, object>()
        };

        // Act
        var result = await _fixture.RulesEngine.EvaluateAsync(context);

        // Assert
        Assert.Equal(2, result.ExecutedRules.Count);
        Assert.Equal("high-priority", result.ExecutedRules[0].RuleId);
        Assert.Equal("low-priority", result.ExecutedRules[1].RuleId);
    }

    [Fact]
    public async Task EvaluateRuleSetAsync_WithStopOnFirstMatch_ShouldStopAfterFirstMatch()
    {
        // Arrange
        var request = new CreateRuleSetRequest
        {
            TenantId = "test-tenant",
            Key = "stop-first-test",
            Name = "Stop First Test",
            EvaluationMode = RuleEvaluationMode.FirstMatch,
            Rules = new List<RuleDefinition>
            {
                new RuleDefinition
                {
                    Id = "first",
                    Priority = 1,
                    Condition = "true",
                    Actions = new List<RuleAction>
                    {
                        new RuleAction { Type = "setVariable", Target = "matched", Value = "first" }
                    }
                },
                new RuleDefinition
                {
                    Id = "second",
                    Priority = 2,
                    Condition = "true",
                    Actions = new List<RuleAction>
                    {
                        new RuleAction { Type = "setVariable", Target = "matched", Value = "second" }
                    }
                }
            }
        };

        await _fixture.RulesEngine.CreateRuleSetAsync(request);

        var context = new RuleEvaluationContext
        {
            RuleSetKey = "stop-first-test",
            Facts = new Dictionary<string, object>()
        };

        // Act
        var result = await _fixture.RulesEngine.EvaluateAsync(context);

        // Assert
        Assert.Single(result.ExecutedRules);
        Assert.Equal("first", result.ExecutedRules[0].RuleId);
    }

    [Fact]
    public async Task ValidateRuleSetAsync_WithValidRules_ShouldReturnValid()
    {
        // Arrange
        var ruleSet = await CreateTestRuleSetAsync("validate-test");

        // Act
        var result = await _fixture.RulesEngine.ValidateRuleSetAsync("validate-test");

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task GetRuleSetAsync_WithExistingKey_ShouldReturnRuleSet()
    {
        // Arrange
        await CreateTestRuleSetAsync("get-test");

        // Act
        var result = await _fixture.RulesEngine.GetRuleSetAsync("get-test");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("get-test", result.Key);
    }

    [Fact]
    public async Task UpdateRuleSetAsync_ShouldUpdateRuleSet()
    {
        // Arrange
        await CreateTestRuleSetAsync("update-test");
        var updateRequest = new UpdateRuleSetRequest
        {
            Name = "Updated Rules",
            Description = "Updated description"
        };

        // Act
        var result = await _fixture.RulesEngine.UpdateRuleSetAsync("update-test", updateRequest);

        // Assert
        Assert.Equal("Updated Rules", result.Name);
        Assert.Equal("Updated description", result.Description);
    }

    [Fact]
    public async Task DeleteRuleSetAsync_ShouldDeleteRuleSet()
    {
        // Arrange
        await CreateTestRuleSetAsync("delete-test");

        // Act
        await _fixture.RulesEngine.DeleteRuleSetAsync("delete-test");
        var result = await _fixture.RulesEngine.GetRuleSetAsync("delete-test");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ListRuleSetsAsync_ShouldReturnRuleSets()
    {
        // Arrange
        await CreateTestRuleSetAsync("list-test-1");
        await CreateTestRuleSetAsync("list-test-2");

        // Act
        var result = await _fixture.RulesEngine.ListRuleSetsAsync("test-tenant");

        // Assert
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public async Task EvaluateRuleSetAsync_WithComplexExpression_ShouldEvaluateCorrectly()
    {
        // Arrange
        var request = new CreateRuleSetRequest
        {
            TenantId = "test-tenant",
            Key = "complex-expr-test",
            Name = "Complex Expression Test",
            Rules = new List<RuleDefinition>
            {
                new RuleDefinition
                {
                    Id = "complex",
                    Priority = 1,
                    Condition = "(amount > 100 && category == \"premium\") || vipCustomer == true",
                    Actions = new List<RuleAction>
                    {
                        new RuleAction { Type = "setVariable", Target = "discount", Value = "20" }
                    }
                }
            }
        };

        await _fixture.RulesEngine.CreateRuleSetAsync(request);

        var context = new RuleEvaluationContext
        {
            RuleSetKey = "complex-expr-test",
            Facts = new Dictionary<string, object>
            {
                ["amount"] = 200,
                ["category"] = "premium",
                ["vipCustomer"] = false
            }
        };

        // Act
        var result = await _fixture.RulesEngine.EvaluateAsync(context);

        // Assert
        Assert.True(result.Executed);
        Assert.Contains(result.ExecutedRules, r => r.RuleId == "complex");
    }

    [Fact]
    public async Task EvaluateRuleSetAsync_WithDecisionTable_ShouldEvaluateTable()
    {
        // Arrange
        var request = new CreateRuleSetRequest
        {
            TenantId = "test-tenant",
            Key = "decision-table-test",
            Name = "Decision Table Test",
            DecisionTable = new DecisionTable
            {
                HitPolicy = HitPolicy.First,
                Inputs = new List<DecisionTableInput>
                {
                    new DecisionTableInput { Name = "age", Type = "number" },
                    new DecisionTableInput { Name = "income", Type = "number" }
                },
                Outputs = new List<DecisionTableOutput>
                {
                    new DecisionTableOutput { Name = "riskLevel", Type = "string" }
                },
                Rules = new List<DecisionTableRule>
                {
                    new DecisionTableRule
                    {
                        InputEntries = new List<string> { "< 25", "> 50000" },
                        OutputEntries = new List<object> { "low" }
                    },
                    new DecisionTableRule
                    {
                        InputEntries = new List<string> { ">= 25", "< 30000" },
                        OutputEntries = new List<object> { "high" }
                    }
                }
            }
        };

        await _fixture.RulesEngine.CreateRuleSetAsync(request);

        var context = new RuleEvaluationContext
        {
            RuleSetKey = "decision-table-test",
            Facts = new Dictionary<string, object>
            {
                ["age"] = 20,
                ["income"] = 60000
            }
        };

        // Act
        var result = await _fixture.RulesEngine.EvaluateAsync(context);

        // Assert
        Assert.True(result.OutputVariables.ContainsKey("riskLevel"));
        Assert.Equal("low", result.OutputVariables["riskLevel"]);
    }

    #region Helper Methods

    private async Task<RuleSet> CreateTestRuleSetAsync(string key)
    {
        var request = new CreateRuleSetRequest
        {
            TenantId = "test-tenant",
            Key = key,
            Name = $"Test Rules - {key}",
            Description = "Test rule set",
            Rules = new List<RuleDefinition>
            {
                new RuleDefinition
                {
                    Id = "auto-approve",
                    Name = "Auto-approve small amounts",
                    Priority = 1,
                    Condition = "amount < 1000",
                    Actions = new List<RuleAction>
                    {
                        new RuleAction { Type = "setVariable", Target = "approved", Value = "true" }
                    }
                },
                new RuleDefinition
                {
                    Id = "require-review",
                    Name = "Require review for large amounts",
                    Priority = 2,
                    Condition = "amount >= 1000",
                    Actions = new List<RuleAction>
                    {
                        new RuleAction { Type = "setVariable", Target = "requiresReview", Value = "true" }
                    }
                }
            }
        };

        return await _fixture.RulesEngine.CreateRuleSetAsync(request);
    }

    #endregion
}
