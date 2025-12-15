using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XenonClinic.WorkflowEngine.Domain.Entities;
using ProcessInstanceState = XenonClinic.WorkflowEngine.Domain.Entities.ProcessInstanceStatus;

namespace XenonClinic.WorkflowEngine.Tests.Testing;

/// <summary>
/// Fluent assertion helpers for workflow testing.
/// </summary>
public static class WorkflowAssertions
{
    /// <summary>
    /// Creates a new process instance assertion context.
    /// </summary>
    public static ProcessInstanceAssertion AssertThat(ProcessInstance instance)
    {
        return new ProcessInstanceAssertion(instance);
    }

    /// <summary>
    /// Creates a new human task assertion context.
    /// </summary>
    public static HumanTaskAssertion AssertThat(HumanTask task)
    {
        return new HumanTaskAssertion(task);
    }

    /// <summary>
    /// Creates a new process definition assertion context.
    /// </summary>
    public static ProcessDefinitionAssertion AssertThat(ProcessDefinition definition)
    {
        return new ProcessDefinitionAssertion(definition);
    }

    /// <summary>
    /// Creates a new task list assertion context.
    /// </summary>
    public static TaskListAssertion AssertThat(IList<HumanTask> tasks)
    {
        return new TaskListAssertion(tasks);
    }
}

/// <summary>
/// Fluent assertions for process instances.
/// </summary>
public class ProcessInstanceAssertion
{
    private readonly ProcessInstance _instance;

    public ProcessInstanceAssertion(ProcessInstance instance)
    {
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    public ProcessInstanceAssertion IsNotNull()
    {
        if (_instance == null)
            throw new AssertionException("Expected process instance to not be null");
        return this;
    }

    public ProcessInstanceAssertion HasId(string expectedId)
    {
        if (_instance.Id != expectedId)
            throw new AssertionException($"Expected instance ID '{expectedId}', but was '{_instance.Id}'");
        return this;
    }

    public ProcessInstanceAssertion IsInState(ProcessInstanceState expectedState)
    {
        if (_instance.Status != expectedState)
            throw new AssertionException($"Expected state '{expectedState}', but was '{_instance.Status}'");
        return this;
    }

    public ProcessInstanceAssertion IsActive()
    {
        return IsInState(ProcessInstanceState.Running);
    }

    public ProcessInstanceAssertion IsCompleted()
    {
        return IsInState(ProcessInstanceState.Completed);
    }

    public ProcessInstanceAssertion IsSuspended()
    {
        return IsInState(ProcessInstanceState.Suspended);
    }

    public ProcessInstanceAssertion IsTerminated()
    {
        return IsInState(ProcessInstanceState.Terminated);
    }

    public ProcessInstanceAssertion HasBusinessKey(string expectedKey)
    {
        if (_instance.BusinessKey != expectedKey)
            throw new AssertionException($"Expected business key '{expectedKey}', but was '{_instance.BusinessKey}'");
        return this;
    }

    public ProcessInstanceAssertion HasVariable(string name)
    {
        if (!_instance.Variables.ContainsKey(name))
            throw new AssertionException($"Expected variable '{name}' to exist");
        return this;
    }

    public ProcessInstanceAssertion HasVariable(string name, object expectedValue)
    {
        HasVariable(name);
        var actualValue = _instance.Variables[name];
        if (!Equals(actualValue, expectedValue))
            throw new AssertionException($"Expected variable '{name}' to be '{expectedValue}', but was '{actualValue}'");
        return this;
    }

    public ProcessInstanceAssertion HasVariableOfType<T>(string name)
    {
        HasVariable(name);
        var value = _instance.Variables[name];
        if (value is not T)
            throw new AssertionException($"Expected variable '{name}' to be of type '{typeof(T).Name}', but was '{value?.GetType().Name}'");
        return this;
    }

    public ProcessInstanceAssertion DoesNotHaveVariable(string name)
    {
        if (_instance.Variables.ContainsKey(name))
            throw new AssertionException($"Expected variable '{name}' to not exist");
        return this;
    }

    public ProcessInstanceAssertion HasProcessDefinitionKey(string expectedKey)
    {
        if (_instance.ProcessDefinitionKey != expectedKey)
            throw new AssertionException($"Expected process definition key '{expectedKey}', but was '{_instance.ProcessDefinitionKey}'");
        return this;
    }

    public ProcessInstanceAssertion StartedAfter(DateTime time)
    {
        if (_instance.StartTime <= time)
            throw new AssertionException($"Expected start time after '{time}', but was '{_instance.StartTime}'");
        return this;
    }

    public ProcessInstanceAssertion EndedBefore(DateTime time)
    {
        if (!_instance.EndTime.HasValue)
            throw new AssertionException("Expected instance to have ended");
        if (_instance.EndTime.Value >= time)
            throw new AssertionException($"Expected end time before '{time}', but was '{_instance.EndTime}'");
        return this;
    }

    public ProcessInstanceAssertion HasEnded()
    {
        if (!_instance.EndTime.HasValue)
            throw new AssertionException("Expected instance to have ended");
        return this;
    }

    public ProcessInstanceAssertion HasNotEnded()
    {
        if (_instance.EndTime.HasValue)
            throw new AssertionException($"Expected instance to not have ended, but ended at '{_instance.EndTime}'");
        return this;
    }

    public ProcessInstanceAssertion IsAtActivity(string activityId)
    {
        if (_instance.CurrentActivityId != activityId)
            throw new AssertionException($"Expected current activity '{activityId}', but was '{_instance.CurrentActivityId}'");
        return this;
    }
}

/// <summary>
/// Fluent assertions for human tasks.
/// </summary>
public class HumanTaskAssertion
{
    private readonly HumanTask _task;

    public HumanTaskAssertion(HumanTask task)
    {
        _task = task ?? throw new ArgumentNullException(nameof(task));
    }

    public HumanTaskAssertion IsNotNull()
    {
        if (_task == null)
            throw new AssertionException("Expected task to not be null");
        return this;
    }

    public HumanTaskAssertion HasId(string expectedId)
    {
        if (_task.Id != expectedId)
            throw new AssertionException($"Expected task ID '{expectedId}', but was '{_task.Id}'");
        return this;
    }

    public HumanTaskAssertion HasStatus(TaskStatus expectedStatus)
    {
        if (_task.Status != expectedStatus)
            throw new AssertionException($"Expected status '{expectedStatus}', but was '{_task.Status}'");
        return this;
    }

    public HumanTaskAssertion IsActive()
    {
        return HasStatus(TaskStatus.Active);
    }

    public HumanTaskAssertion IsCompleted()
    {
        return HasStatus(TaskStatus.Completed);
    }

    public HumanTaskAssertion IsClaimed()
    {
        return HasStatus(TaskStatus.Claimed);
    }

    public HumanTaskAssertion IsAssignedTo(string userId)
    {
        if (_task.AssigneeId != userId)
            throw new AssertionException($"Expected assignee '{userId}', but was '{_task.AssigneeId}'");
        return this;
    }

    public HumanTaskAssertion IsUnassigned()
    {
        if (!string.IsNullOrEmpty(_task.AssigneeId))
            throw new AssertionException($"Expected task to be unassigned, but was assigned to '{_task.AssigneeId}'");
        return this;
    }

    public HumanTaskAssertion HasName(string expectedName)
    {
        if (_task.Name != expectedName)
            throw new AssertionException($"Expected name '{expectedName}', but was '{_task.Name}'");
        return this;
    }

    public HumanTaskAssertion HasTaskDefinitionKey(string expectedKey)
    {
        if (_task.TaskDefinitionKey != expectedKey)
            throw new AssertionException($"Expected task definition key '{expectedKey}', but was '{_task.TaskDefinitionKey}'");
        return this;
    }

    public HumanTaskAssertion HasPriority(int expectedPriority)
    {
        if (_task.Priority != expectedPriority)
            throw new AssertionException($"Expected priority '{expectedPriority}', but was '{_task.Priority}'");
        return this;
    }

    public HumanTaskAssertion HasPriorityGreaterThan(int priority)
    {
        if (_task.Priority <= priority)
            throw new AssertionException($"Expected priority greater than '{priority}', but was '{_task.Priority}'");
        return this;
    }

    public HumanTaskAssertion IsDueBefore(DateTime dueDate)
    {
        if (!_task.DueDate.HasValue)
            throw new AssertionException("Expected task to have a due date");
        if (_task.DueDate.Value >= dueDate)
            throw new AssertionException($"Expected due date before '{dueDate}', but was '{_task.DueDate}'");
        return this;
    }

    public HumanTaskAssertion HasCandidateUser(string userId)
    {
        if (!_task.CandidateUsers.Contains(userId))
            throw new AssertionException($"Expected candidate user '{userId}' to be present");
        return this;
    }

    public HumanTaskAssertion HasCandidateGroup(string groupId)
    {
        if (!_task.CandidateGroups.Contains(groupId))
            throw new AssertionException($"Expected candidate group '{groupId}' to be present");
        return this;
    }

    public HumanTaskAssertion HasFormKey(string expectedFormKey)
    {
        if (_task.FormKey != expectedFormKey)
            throw new AssertionException($"Expected form key '{expectedFormKey}', but was '{_task.FormKey}'");
        return this;
    }

    public HumanTaskAssertion BelongsToProcess(string processInstanceId)
    {
        if (_task.ProcessInstanceId != processInstanceId)
            throw new AssertionException($"Expected process instance ID '{processInstanceId}', but was '{_task.ProcessInstanceId}'");
        return this;
    }
}

/// <summary>
/// Fluent assertions for process definitions.
/// </summary>
public class ProcessDefinitionAssertion
{
    private readonly ProcessDefinition _definition;

    public ProcessDefinitionAssertion(ProcessDefinition definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    public ProcessDefinitionAssertion IsNotNull()
    {
        if (_definition == null)
            throw new AssertionException("Expected process definition to not be null");
        return this;
    }

    public ProcessDefinitionAssertion HasKey(string expectedKey)
    {
        if (_definition.Key != expectedKey)
            throw new AssertionException($"Expected key '{expectedKey}', but was '{_definition.Key}'");
        return this;
    }

    public ProcessDefinitionAssertion HasVersion(int expectedVersion)
    {
        if (_definition.Version != expectedVersion)
            throw new AssertionException($"Expected version '{expectedVersion}', but was '{_definition.Version}'");
        return this;
    }

    public ProcessDefinitionAssertion IsDeployed()
    {
        if (!_definition.IsDeployed)
            throw new AssertionException("Expected definition to be deployed");
        return this;
    }

    public ProcessDefinitionAssertion IsNotDeployed()
    {
        if (_definition.IsDeployed)
            throw new AssertionException("Expected definition to not be deployed");
        return this;
    }

    public ProcessDefinitionAssertion IsActive()
    {
        if (!_definition.IsActive)
            throw new AssertionException("Expected definition to be active");
        return this;
    }

    public ProcessDefinitionAssertion HasName(string expectedName)
    {
        if (_definition.Name != expectedName)
            throw new AssertionException($"Expected name '{expectedName}', but was '{_definition.Name}'");
        return this;
    }

    public ProcessDefinitionAssertion HasCategory(string expectedCategory)
    {
        if (_definition.Category != expectedCategory)
            throw new AssertionException($"Expected category '{expectedCategory}', but was '{_definition.Category}'");
        return this;
    }

    public ProcessDefinitionAssertion HasElement(string elementId)
    {
        if (_definition.ProcessModel?.Elements.All(e => e.Id != elementId) ?? true)
            throw new AssertionException($"Expected element '{elementId}' to exist");
        return this;
    }

    public ProcessDefinitionAssertion HasStartEvent()
    {
        var hasStart = _definition.ProcessModel?.Elements.Any(e => e.Type == "startEvent") ?? false;
        if (!hasStart)
            throw new AssertionException("Expected at least one start event");
        return this;
    }

    public ProcessDefinitionAssertion HasEndEvent()
    {
        var hasEnd = _definition.ProcessModel?.Elements.Any(e => e.Type == "endEvent") ?? false;
        if (!hasEnd)
            throw new AssertionException("Expected at least one end event");
        return this;
    }
}

/// <summary>
/// Fluent assertions for task lists.
/// </summary>
public class TaskListAssertion
{
    private readonly IList<HumanTask> _tasks;

    public TaskListAssertion(IList<HumanTask> tasks)
    {
        _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
    }

    public TaskListAssertion HasCount(int expectedCount)
    {
        if (_tasks.Count != expectedCount)
            throw new AssertionException($"Expected {expectedCount} tasks, but found {_tasks.Count}");
        return this;
    }

    public TaskListAssertion IsEmpty()
    {
        return HasCount(0);
    }

    public TaskListAssertion IsNotEmpty()
    {
        if (_tasks.Count == 0)
            throw new AssertionException("Expected tasks list to not be empty");
        return this;
    }

    public TaskListAssertion HasTaskWithDefinitionKey(string taskDefinitionKey)
    {
        if (_tasks.All(t => t.TaskDefinitionKey != taskDefinitionKey))
            throw new AssertionException($"Expected task with definition key '{taskDefinitionKey}'");
        return this;
    }

    public TaskListAssertion HasTaskAssignedTo(string userId)
    {
        if (_tasks.All(t => t.AssigneeId != userId))
            throw new AssertionException($"Expected task assigned to '{userId}'");
        return this;
    }

    public TaskListAssertion AllHaveStatus(TaskStatus status)
    {
        if (_tasks.Any(t => t.Status != status))
            throw new AssertionException($"Expected all tasks to have status '{status}'");
        return this;
    }

    public HumanTaskAssertion First()
    {
        if (_tasks.Count == 0)
            throw new AssertionException("Cannot get first task from empty list");
        return new HumanTaskAssertion(_tasks[0]);
    }

    public HumanTaskAssertion Single()
    {
        if (_tasks.Count != 1)
            throw new AssertionException($"Expected exactly one task, but found {_tasks.Count}");
        return new HumanTaskAssertion(_tasks[0]);
    }
}

/// <summary>
/// Exception thrown when an assertion fails.
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}
