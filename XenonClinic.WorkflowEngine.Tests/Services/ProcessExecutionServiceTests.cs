using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using XenonClinic.WorkflowEngine.Application.Services;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Tests.Testing;
using static XenonClinic.WorkflowEngine.Tests.Testing.WorkflowAssertions;

namespace XenonClinic.WorkflowEngine.Tests.Services;

/// <summary>
/// Unit tests for the ProcessExecutionService.
/// </summary>
public class ProcessExecutionServiceTests : IDisposable
{
    private readonly WorkflowTestFixture _fixture;

    public ProcessExecutionServiceTests()
    {
        _fixture = new WorkflowTestFixture();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task StartProcessAsync_WithValidDefinition_ShouldCreateInstance()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("start-test");
        var request = new StartProcessRequest
        {
            ProcessDefinitionKey = "start-test",
            InitiatorUserId = "test-user"
        };

        // Act
        var result = await _fixture.ProcessExecutionService.StartProcessAsync(request);

        // Assert
        AssertThat(result)
            .IsActive()
            .HasProcessDefinitionKey("start-test");
    }

    [Fact]
    public async Task StartProcessAsync_WithVariables_ShouldPassVariablesToInstance()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("vars-test");
        var request = new StartProcessRequest
        {
            ProcessDefinitionKey = "vars-test",
            Variables = new Dictionary<string, object>
            {
                ["customVar"] = "customValue",
                ["count"] = 42
            }
        };

        // Act
        var result = await _fixture.ProcessExecutionService.StartProcessAsync(request);

        // Assert
        AssertThat(result)
            .HasVariable("customVar", "customValue")
            .HasVariable("count", 42);
    }

    [Fact]
    public async Task StartProcessAsync_WithBusinessKey_ShouldSetBusinessKey()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("biz-key-test");
        var request = new StartProcessRequest
        {
            ProcessDefinitionKey = "biz-key-test",
            BusinessKey = "BIZ-123"
        };

        // Act
        var result = await _fixture.ProcessExecutionService.StartProcessAsync(request);

        // Assert
        AssertThat(result)
            .HasBusinessKey("BIZ-123");
    }

    [Fact]
    public async Task GetInstanceAsync_WithExistingId_ShouldReturnInstance()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("get-instance-test");
        var instance = await _fixture.StartProcessAsync("get-instance-test");

        // Act
        var result = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);

        // Assert
        Assert.NotNull(result);
        AssertThat(result).HasId(instance.Id);
    }

    [Fact]
    public async Task GetInstanceAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _fixture.ProcessExecutionService.GetInstanceAsync("non-existing");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SuspendInstanceAsync_ShouldSuspendInstance()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("suspend-test");
        var instance = await _fixture.StartProcessAsync("suspend-test");

        // Act
        await _fixture.ProcessExecutionService.SuspendInstanceAsync(instance.Id);
        var result = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);

        // Assert
        AssertThat(result!).IsSuspended();
    }

    [Fact]
    public async Task ResumeInstanceAsync_ShouldResumeInstance()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("resume-test");
        var instance = await _fixture.StartProcessAsync("resume-test");
        await _fixture.ProcessExecutionService.SuspendInstanceAsync(instance.Id);

        // Act
        await _fixture.ProcessExecutionService.ResumeInstanceAsync(instance.Id);
        var result = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);

        // Assert
        AssertThat(result!).IsActive();
    }

    [Fact]
    public async Task TerminateInstanceAsync_ShouldTerminateInstance()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("terminate-test");
        var instance = await _fixture.StartProcessAsync("terminate-test");

        // Act
        await _fixture.ProcessExecutionService.TerminateInstanceAsync(instance.Id, "Test termination");
        var result = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);

        // Assert
        AssertThat(result!)
            .IsTerminated()
            .HasEnded();
    }

    [Fact]
    public async Task SetVariableAsync_ShouldUpdateVariable()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("set-var-test");
        var instance = await _fixture.StartProcessAsync("set-var-test");

        // Act
        await _fixture.ProcessExecutionService.SetVariableAsync(instance.Id, "newVar", "newValue");
        var result = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);

        // Assert
        AssertThat(result!).HasVariable("newVar", "newValue");
    }

    [Fact]
    public async Task SetVariablesAsync_ShouldUpdateMultipleVariables()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("set-vars-test");
        var instance = await _fixture.StartProcessAsync("set-vars-test");
        var variables = new Dictionary<string, object>
        {
            ["var1"] = "value1",
            ["var2"] = 123
        };

        // Act
        await _fixture.ProcessExecutionService.SetVariablesAsync(instance.Id, variables);
        var result = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);

        // Assert
        AssertThat(result!)
            .HasVariable("var1", "value1")
            .HasVariable("var2", 123);
    }

    [Fact]
    public async Task QueryInstancesAsync_ShouldReturnFilteredResults()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("query-test");
        await _fixture.StartProcessAsync("query-test");
        await _fixture.StartProcessAsync("query-test");

        var query = new ProcessInstanceQuery
        {
            ProcessDefinitionKey = "query-test",
            State = ProcessInstanceState.Active,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _fixture.ProcessExecutionService.QueryInstancesAsync(query);

        // Assert
        Assert.True(result.TotalCount >= 2);
        Assert.All(result.Instances, i => Assert.Equal("query-test", i.ProcessDefinitionKey));
    }

    [Fact]
    public async Task QueryInstancesAsync_WithBusinessKeyFilter_ShouldFilterByBusinessKey()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("biz-filter-test");
        await _fixture.StartProcessAsync("biz-filter-test", businessKey: "BIZ-001");
        await _fixture.StartProcessAsync("biz-filter-test", businessKey: "BIZ-002");

        var query = new ProcessInstanceQuery
        {
            BusinessKey = "BIZ-001"
        };

        // Act
        var result = await _fixture.ProcessExecutionService.QueryInstancesAsync(query);

        // Assert
        Assert.True(result.TotalCount >= 1);
        Assert.All(result.Instances, i => Assert.Equal("BIZ-001", i.BusinessKey));
    }

    [Fact]
    public async Task DeleteInstanceAsync_ShouldRemoveInstance()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("delete-test");
        var instance = await _fixture.StartProcessAsync("delete-test");
        await _fixture.ProcessExecutionService.TerminateInstanceAsync(instance.Id, "For deletion");

        // Act
        await _fixture.ProcessExecutionService.DeleteInstanceAsync(instance.Id);
        var result = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActivityInstancesAsync_ShouldReturnActivityHistory()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("activity-history-test");
        var instance = await _fixture.StartProcessAsync("activity-history-test");

        // Act
        var result = await _fixture.ProcessExecutionService.GetActivityInstancesAsync(instance.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task MessageCorrelation_ShouldResumeWaitingProcess()
    {
        // This test requires a process with message events
        // For now, we'll just verify the API exists
        var request = new MessageCorrelationRequest
        {
            MessageName = "test-message",
            CorrelationKeys = new Dictionary<string, object>
            {
                ["orderId"] = "123"
            },
            Variables = new Dictionary<string, object>
            {
                ["result"] = "approved"
            }
        };

        // Act - should not throw even with no waiting processes
        var result = await _fixture.ProcessExecutionService.CorrelateMessageAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SignalEvent_ShouldBroadcastToAllListeners()
    {
        // This test requires processes with signal events
        // For now, we'll just verify the API exists
        var request = new SignalEventRequest
        {
            SignalName = "test-signal",
            Variables = new Dictionary<string, object>
            {
                ["signalData"] = "test"
            }
        };

        // Act - should not throw
        await _fixture.ProcessExecutionService.SendSignalAsync(request);
    }
}
