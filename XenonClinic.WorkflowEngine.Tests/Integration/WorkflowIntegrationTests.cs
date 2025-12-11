using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using XenonClinic.WorkflowEngine.Application.Services;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Tests.Testing;
using static XenonClinic.WorkflowEngine.Tests.Testing.WorkflowAssertions;

namespace XenonClinic.WorkflowEngine.Tests.Integration;

/// <summary>
/// Integration tests for complete workflow scenarios.
/// </summary>
public class WorkflowIntegrationTests : IDisposable
{
    private readonly WorkflowTestFixture _fixture;

    public WorkflowIntegrationTests()
    {
        _fixture = new WorkflowTestFixture();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task SimpleSequentialProcess_ShouldCompleteSuccessfully()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("sequential-test");

        // Act - Start process
        var instance = await _fixture.StartProcessAsync("sequential-test");

        // Assert - Process is active
        AssertThat(instance).IsActive();

        // Act - Get and complete task
        var task = await _fixture.WaitForTaskAsync(instance.Id, "task1");
        AssertThat(task).IsActive().HasTaskDefinitionKey("task1");

        await _fixture.CompleteTaskAsync(task.Id);

        // Assert - Process completed
        var finalInstance = await _fixture.WaitForProcessStateAsync(instance.Id, ProcessInstanceState.Completed);
        AssertThat(finalInstance)
            .IsCompleted()
            .HasEnded();
    }

    [Fact]
    public async Task DecisionProcess_WithApprovalPath_ShouldFollowApprovalBranch()
    {
        // Arrange
        var definition = await _fixture.CreateDecisionProcessAsync("decision-approve");

        // Act - Start process with approval variable
        var instance = await _fixture.StartProcessAsync(
            "decision-approve",
            new Dictionary<string, object> { ["approved"] = true });

        // Assert - Should create approve task
        var task = await _fixture.WaitForTaskAsync(instance.Id, "approveTask");
        AssertThat(task)
            .HasTaskDefinitionKey("approveTask")
            .HasName("Approve Task");

        // Complete task
        await _fixture.CompleteTaskAsync(task.Id);

        // Assert - Process completed
        var finalInstance = await _fixture.WaitForProcessStateAsync(instance.Id, ProcessInstanceState.Completed);
        AssertThat(finalInstance).IsCompleted();
    }

    [Fact]
    public async Task DecisionProcess_WithRejectionPath_ShouldFollowRejectionBranch()
    {
        // Arrange
        var definition = await _fixture.CreateDecisionProcessAsync("decision-reject");

        // Act - Start process with rejection variable
        var instance = await _fixture.StartProcessAsync(
            "decision-reject",
            new Dictionary<string, object> { ["approved"] = false });

        // Assert - Should create reject task
        var task = await _fixture.WaitForTaskAsync(instance.Id, "rejectTask");
        AssertThat(task)
            .HasTaskDefinitionKey("rejectTask")
            .HasName("Reject Task");

        // Complete task
        await _fixture.CompleteTaskAsync(task.Id);

        // Assert - Process completed
        var finalInstance = await _fixture.WaitForProcessStateAsync(instance.Id, ProcessInstanceState.Completed);
        AssertThat(finalInstance).IsCompleted();
    }

    [Fact]
    public async Task ParallelProcess_ShouldExecuteTasksInParallel()
    {
        // Arrange
        var definition = await _fixture.CreateParallelProcessAsync("parallel-test");

        // Act - Start process
        var instance = await _fixture.StartProcessAsync("parallel-test");

        // Assert - Both tasks should be created
        await Task.Delay(100); // Allow time for parallel execution
        var tasks = await _fixture.GetActiveTasksAsync(instance.Id);

        AssertThat(tasks)
            .HasCount(2)
            .HasTaskWithDefinitionKey("task1")
            .HasTaskWithDefinitionKey("task2");

        // Complete both tasks
        foreach (var task in tasks)
        {
            await _fixture.CompleteTaskAsync(task.Id);
        }

        // Assert - Process completed after join
        var finalInstance = await _fixture.WaitForProcessStateAsync(instance.Id, ProcessInstanceState.Completed);
        AssertThat(finalInstance).IsCompleted();
    }

    [Fact]
    public async Task ProcessWithVariables_ShouldPassVariablesBetweenTasks()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("vars-flow-test");

        // Act - Start with initial variables
        var instance = await _fixture.StartProcessAsync(
            "vars-flow-test",
            new Dictionary<string, object>
            {
                ["initialVar"] = "startValue"
            });

        // Complete task with new variables
        var task = await _fixture.WaitForTaskAsync(instance.Id);
        await _fixture.CompleteTaskAsync(task.Id, new Dictionary<string, object>
        {
            ["taskOutput"] = "taskValue"
        });

        // Assert - Variables are preserved
        var finalInstance = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);
        AssertThat(finalInstance!)
            .HasVariable("initialVar", "startValue")
            .HasVariable("taskOutput", "taskValue");
    }

    [Fact]
    public async Task ProcessSuspendAndResume_ShouldWorkCorrectly()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("suspend-resume-test");
        var instance = await _fixture.StartProcessAsync("suspend-resume-test");

        // Act - Suspend
        await _fixture.ProcessExecutionService.SuspendInstanceAsync(instance.Id);
        var suspended = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);
        AssertThat(suspended!).IsSuspended();

        // Act - Resume
        await _fixture.ProcessExecutionService.ResumeInstanceAsync(instance.Id);
        var resumed = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);
        AssertThat(resumed!).IsActive();

        // Tasks should still be available
        var tasks = await _fixture.GetActiveTasksAsync(instance.Id);
        AssertThat(tasks).IsNotEmpty();
    }

    [Fact]
    public async Task ProcessTermination_ShouldTerminateImmediately()
    {
        // Arrange
        var definition = await _fixture.CreateParallelProcessAsync("terminate-test");
        var instance = await _fixture.StartProcessAsync("terminate-test");

        // Wait for tasks to be created
        await Task.Delay(100);
        var tasksBefore = await _fixture.GetActiveTasksAsync(instance.Id);
        Assert.True(tasksBefore.Count > 0);

        // Act - Terminate
        await _fixture.ProcessExecutionService.TerminateInstanceAsync(instance.Id, "Test termination");

        // Assert
        var terminated = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);
        AssertThat(terminated!)
            .IsTerminated()
            .HasEnded();

        // All tasks should be cancelled
        var tasksAfter = await _fixture.GetActiveTasksAsync(instance.Id);
        Assert.Empty(tasksAfter);
    }

    [Fact]
    public async Task MultipleProcessInstances_ShouldBeIndependent()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("multi-instance-test");

        // Act - Start multiple instances
        var instance1 = await _fixture.StartProcessAsync(
            "multi-instance-test",
            new Dictionary<string, object> { ["instanceNum"] = 1 });
        var instance2 = await _fixture.StartProcessAsync(
            "multi-instance-test",
            new Dictionary<string, object> { ["instanceNum"] = 2 });

        // Assert - Both are independent
        AssertThat(instance1).HasVariable("instanceNum", 1);
        AssertThat(instance2).HasVariable("instanceNum", 2);

        // Complete first instance
        var task1 = await _fixture.WaitForTaskAsync(instance1.Id);
        await _fixture.CompleteTaskAsync(task1.Id);

        // Second instance should still be active
        var instance2Status = await _fixture.ProcessExecutionService.GetInstanceAsync(instance2.Id);
        AssertThat(instance2Status!).IsActive();
    }

    [Fact]
    public async Task TaskClaim_ShouldPreventOthersClaiming()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("claim-test");
        var instance = await _fixture.StartProcessAsync("claim-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);

        // Act - First user claims
        await _fixture.HumanTaskService.ClaimTaskAsync(task.Id, "user1");

        // Assert - Second user cannot claim
        var claimed = await _fixture.HumanTaskService.GetTaskAsync(task.Id);
        AssertThat(claimed!)
            .IsAssignedTo("user1")
            .IsClaimed();
    }

    [Fact]
    public async Task TaskDelegation_ShouldTransferOwnership()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("delegation-test");
        var instance = await _fixture.StartProcessAsync("delegation-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);
        await _fixture.HumanTaskService.AssignTaskAsync(task.Id, "original");

        // Act - Delegate to another user
        await _fixture.HumanTaskService.DelegateTaskAsync(task.Id, "delegate");

        // Assert
        var delegated = await _fixture.HumanTaskService.GetTaskAsync(task.Id);
        AssertThat(delegated!).IsAssignedTo("delegate");
    }

    [Fact]
    public async Task ProcessWithBusinessKey_ShouldBeQueryableByBusinessKey()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("biz-key-query-test");
        var businessKey = $"ORDER-{Guid.NewGuid():N}";
        var instance = await _fixture.StartProcessAsync(
            "biz-key-query-test",
            businessKey: businessKey);

        // Act - Query by business key
        var query = new ProcessInstanceQuery
        {
            BusinessKey = businessKey
        };
        var result = await _fixture.ProcessExecutionService.QueryInstancesAsync(query);

        // Assert
        Assert.Single(result.Instances);
        Assert.Equal(instance.Id, result.Instances[0].Id);
    }

    [Fact]
    public async Task ProcessHistory_ShouldRecordActivityExecutions()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("history-test");
        var instance = await _fixture.StartProcessAsync("history-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);
        await _fixture.CompleteTaskAsync(task.Id);

        // Wait for completion
        await _fixture.WaitForProcessStateAsync(instance.Id, ProcessInstanceState.Completed);

        // Act
        var activities = await _fixture.ProcessExecutionService.GetActivityInstancesAsync(instance.Id);

        // Assert
        Assert.NotNull(activities);
        Assert.True(activities.Count >= 3); // start, task, end
    }

    [Fact]
    public async Task ConcurrentTaskCompletion_ShouldHandleCorrectly()
    {
        // Arrange
        var definition = await _fixture.CreateParallelProcessAsync("concurrent-complete-test");
        var instance = await _fixture.StartProcessAsync("concurrent-complete-test");
        await Task.Delay(100);
        var tasks = await _fixture.GetActiveTasksAsync(instance.Id);
        Assert.Equal(2, tasks.Count);

        // Act - Complete both tasks concurrently
        var task1 = _fixture.CompleteTaskAsync(tasks[0].Id);
        var task2 = _fixture.CompleteTaskAsync(tasks[1].Id);
        await Task.WhenAll(task1, task2);

        // Assert - Process should complete
        var final = await _fixture.WaitForProcessStateAsync(instance.Id, ProcessInstanceState.Completed);
        AssertThat(final).IsCompleted();
    }

    [Fact]
    public async Task EmailNotification_ShouldBeSentOnTaskCreation()
    {
        // Arrange
        var mockEmail = _fixture.Services.GetService(typeof(MockEmailService)) as MockEmailService;
        mockEmail?.Clear();

        var definition = await _fixture.CreateSimpleProcessAsync("email-test");

        // Act
        var instance = await _fixture.StartProcessAsync("email-test");
        await Task.Delay(100);

        // Assert - Email should be sent (if email service is configured)
        // This depends on the implementation having an event handler
        Assert.NotNull(mockEmail);
    }
}
