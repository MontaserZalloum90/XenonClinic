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
/// Unit tests for the HumanTaskService.
/// </summary>
public class HumanTaskServiceTests : IDisposable
{
    private readonly WorkflowTestFixture _fixture;

    public HumanTaskServiceTests()
    {
        _fixture = new WorkflowTestFixture();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task StartProcess_ShouldCreateTask()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("task-creation-test");

        // Act
        var instance = await _fixture.StartProcessAsync("task-creation-test");
        var tasks = await _fixture.GetActiveTasksAsync(instance.Id);

        // Assert
        AssertThat(tasks)
            .HasCount(1)
            .First()
            .IsActive()
            .HasName("First Task");
    }

    [Fact]
    public async Task GetTaskAsync_WithExistingId_ShouldReturnTask()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("get-task-test");
        var instance = await _fixture.StartProcessAsync("get-task-test");
        var tasks = await _fixture.GetActiveTasksAsync(instance.Id);
        var taskId = tasks[0].Id;

        // Act
        var result = await _fixture.HumanTaskService.GetTaskAsync(taskId);

        // Assert
        Assert.NotNull(result);
        AssertThat(result).HasId(taskId);
    }

    [Fact]
    public async Task ClaimTaskAsync_ShouldAssignTask()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("claim-test");
        var instance = await _fixture.StartProcessAsync("claim-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);

        // Act
        await _fixture.HumanTaskService.ClaimTaskAsync(task.Id, "user1");
        var result = await _fixture.HumanTaskService.GetTaskAsync(task.Id);

        // Assert
        AssertThat(result!)
            .IsAssignedTo("user1")
            .IsClaimed();
    }

    [Fact]
    public async Task UnclaimTaskAsync_ShouldRemoveAssignment()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("unclaim-test");
        var instance = await _fixture.StartProcessAsync("unclaim-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);
        await _fixture.HumanTaskService.ClaimTaskAsync(task.Id, "user1");

        // Act
        await _fixture.HumanTaskService.UnclaimTaskAsync(task.Id);
        var result = await _fixture.HumanTaskService.GetTaskAsync(task.Id);

        // Assert
        AssertThat(result!)
            .IsUnassigned()
            .IsActive();
    }

    [Fact]
    public async Task AssignTaskAsync_ShouldAssignToUser()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("assign-test");
        var instance = await _fixture.StartProcessAsync("assign-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);

        // Act
        await _fixture.HumanTaskService.AssignTaskAsync(task.Id, "newAssignee");
        var result = await _fixture.HumanTaskService.GetTaskAsync(task.Id);

        // Assert
        AssertThat(result!)
            .IsAssignedTo("newAssignee");
    }

    [Fact]
    public async Task DelegateTaskAsync_ShouldTransferAssignment()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("delegate-test");
        var instance = await _fixture.StartProcessAsync("delegate-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);
        await _fixture.HumanTaskService.AssignTaskAsync(task.Id, "original");

        // Act
        await _fixture.HumanTaskService.DelegateTaskAsync(task.Id, "delegated");
        var result = await _fixture.HumanTaskService.GetTaskAsync(task.Id);

        // Assert
        AssertThat(result!)
            .IsAssignedTo("delegated");
    }

    [Fact]
    public async Task CompleteTaskAsync_ShouldCompleteTask()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("complete-test");
        var instance = await _fixture.StartProcessAsync("complete-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);

        // Act
        await _fixture.CompleteTaskAsync(task.Id);
        var result = await _fixture.HumanTaskService.GetTaskAsync(task.Id);

        // Assert
        AssertThat(result!)
            .IsCompleted();
    }

    [Fact]
    public async Task CompleteTaskAsync_WithVariables_ShouldPassVariablesToProcess()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("complete-vars-test");
        var instance = await _fixture.StartProcessAsync("complete-vars-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);
        var taskVars = new Dictionary<string, object>
        {
            ["taskResult"] = "approved",
            ["comments"] = "Looks good"
        };

        // Act
        await _fixture.CompleteTaskAsync(task.Id, taskVars);
        var processInstance = await _fixture.ProcessExecutionService.GetInstanceAsync(instance.Id);

        // Assert
        AssertThat(processInstance!)
            .HasVariable("taskResult", "approved")
            .HasVariable("comments", "Looks good");
    }

    [Fact]
    public async Task QueryTasksAsync_ShouldReturnFilteredTasks()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("query-tasks-test");
        await _fixture.StartProcessAsync("query-tasks-test");
        await _fixture.StartProcessAsync("query-tasks-test");

        var query = new TaskQuery
        {
            ProcessDefinitionKey = "query-tasks-test",
            Status = TaskStatus.Active,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _fixture.HumanTaskService.QueryTasksAsync(query);

        // Assert
        Assert.True(result.TotalCount >= 2);
        AssertThat(result.Tasks).AllHaveStatus(TaskStatus.Active);
    }

    [Fact]
    public async Task QueryTasksAsync_ByAssignee_ShouldFilterByAssignee()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("assignee-query-test");
        var instance1 = await _fixture.StartProcessAsync("assignee-query-test");
        var instance2 = await _fixture.StartProcessAsync("assignee-query-test");

        var task1 = await _fixture.WaitForTaskAsync(instance1.Id);
        var task2 = await _fixture.WaitForTaskAsync(instance2.Id);

        await _fixture.HumanTaskService.AssignTaskAsync(task1.Id, "userA");
        await _fixture.HumanTaskService.AssignTaskAsync(task2.Id, "userB");

        var query = new TaskQuery
        {
            AssigneeId = "userA"
        };

        // Act
        var result = await _fixture.HumanTaskService.QueryTasksAsync(query);

        // Assert
        Assert.True(result.TotalCount >= 1);
        Assert.All(result.Tasks, t => Assert.Equal("userA", t.AssigneeId));
    }

    [Fact]
    public async Task QueryTasksAsync_ByCandidateGroup_ShouldFilterByCandidateGroup()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("candidate-query-test");
        var instance = await _fixture.StartProcessAsync("candidate-query-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);

        await _fixture.HumanTaskService.AddCandidateGroupAsync(task.Id, "managers");

        var query = new TaskQuery
        {
            CandidateGroup = "managers"
        };

        // Act
        var result = await _fixture.HumanTaskService.QueryTasksAsync(query);

        // Assert
        Assert.True(result.TotalCount >= 1);
        Assert.All(result.Tasks, t => Assert.Contains("managers", t.CandidateGroups));
    }

    [Fact]
    public async Task AddCandidateUserAsync_ShouldAddCandidateUser()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("add-candidate-user-test");
        var instance = await _fixture.StartProcessAsync("add-candidate-user-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);

        // Act
        await _fixture.HumanTaskService.AddCandidateUserAsync(task.Id, "candidate1");
        var result = await _fixture.HumanTaskService.GetTaskAsync(task.Id);

        // Assert
        AssertThat(result!)
            .HasCandidateUser("candidate1");
    }

    [Fact]
    public async Task RemoveCandidateUserAsync_ShouldRemoveCandidateUser()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("remove-candidate-user-test");
        var instance = await _fixture.StartProcessAsync("remove-candidate-user-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);
        await _fixture.HumanTaskService.AddCandidateUserAsync(task.Id, "toRemove");

        // Act
        await _fixture.HumanTaskService.RemoveCandidateUserAsync(task.Id, "toRemove");
        var result = await _fixture.HumanTaskService.GetTaskAsync(task.Id);

        // Assert
        Assert.DoesNotContain("toRemove", result!.CandidateUsers);
    }

    [Fact]
    public async Task SetPriorityAsync_ShouldUpdateTaskPriority()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("priority-test");
        var instance = await _fixture.StartProcessAsync("priority-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);

        // Act
        await _fixture.HumanTaskService.SetPriorityAsync(task.Id, 100);
        var result = await _fixture.HumanTaskService.GetTaskAsync(task.Id);

        // Assert
        AssertThat(result!)
            .HasPriority(100);
    }

    [Fact]
    public async Task SetDueDateAsync_ShouldUpdateDueDate()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("due-date-test");
        var instance = await _fixture.StartProcessAsync("due-date-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);
        var dueDate = DateTime.UtcNow.AddDays(7);

        // Act
        await _fixture.HumanTaskService.SetDueDateAsync(task.Id, dueDate);
        var result = await _fixture.HumanTaskService.GetTaskAsync(task.Id);

        // Assert
        AssertThat(result!)
            .IsDueBefore(dueDate.AddMinutes(1));
    }

    [Fact]
    public async Task AddCommentAsync_ShouldAddComment()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("comment-test");
        var instance = await _fixture.StartProcessAsync("comment-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);

        // Act
        var comment = await _fixture.HumanTaskService.AddCommentAsync(task.Id, "user1", "This is a comment");

        // Assert
        Assert.NotNull(comment);
        Assert.Equal("This is a comment", comment.Text);
        Assert.Equal("user1", comment.AuthorId);
    }

    [Fact]
    public async Task GetCommentsAsync_ShouldReturnComments()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("get-comments-test");
        var instance = await _fixture.StartProcessAsync("get-comments-test");
        var task = await _fixture.WaitForTaskAsync(instance.Id);
        await _fixture.HumanTaskService.AddCommentAsync(task.Id, "user1", "Comment 1");
        await _fixture.HumanTaskService.AddCommentAsync(task.Id, "user2", "Comment 2");

        // Act
        var comments = await _fixture.HumanTaskService.GetCommentsAsync(task.Id);

        // Assert
        Assert.Equal(2, comments.Count);
    }

    [Fact]
    public async Task GetTaskCountAsync_ShouldReturnCounts()
    {
        // Arrange
        var definition = await _fixture.CreateSimpleProcessAsync("count-test");
        await _fixture.StartProcessAsync("count-test");
        await _fixture.StartProcessAsync("count-test");

        // Act
        var counts = await _fixture.HumanTaskService.GetTaskCountAsync("test-tenant");

        // Assert
        Assert.NotNull(counts);
        Assert.True(counts.TotalActive >= 2);
    }
}
