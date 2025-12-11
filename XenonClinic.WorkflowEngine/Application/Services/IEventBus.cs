using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Event bus interface for workflow event publishing and subscription.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : WorkflowEvent;

    /// <summary>
    /// Publishes an event with a specific topic.
    /// </summary>
    Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default) where TEvent : WorkflowEvent;

    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : WorkflowEvent;

    /// <summary>
    /// Subscribes to events on a specific topic.
    /// </summary>
    IDisposable Subscribe<TEvent>(string topic, Func<TEvent, CancellationToken, Task> handler) where TEvent : WorkflowEvent;

    /// <summary>
    /// Subscribes to all events matching a pattern.
    /// </summary>
    IDisposable SubscribePattern(string pattern, Func<WorkflowEvent, CancellationToken, Task> handler);
}

/// <summary>
/// Base class for all workflow events.
/// </summary>
public abstract class WorkflowEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TenantId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string Source { get; set; } = "WorkflowEngine";
    public Dictionary<string, object> Metadata { get; set; } = new();

    public abstract string EventType { get; }
}

#region Process Events

public class ProcessStartedEvent : WorkflowEvent
{
    public override string EventType => "process.started";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string BusinessKey { get; set; } = string.Empty;
    public string StartedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Variables { get; set; } = new();
}

public class ProcessCompletedEvent : WorkflowEvent
{
    public override string EventType => "process.completed";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string BusinessKey { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> OutputVariables { get; set; } = new();
}

public class ProcessFailedEvent : WorkflowEvent
{
    public override string EventType => "process.failed";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string BusinessKey { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string FailedActivityId { get; set; } = string.Empty;
}

public class ProcessCancelledEvent : WorkflowEvent
{
    public override string EventType => "process.cancelled";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string BusinessKey { get; set; } = string.Empty;
    public string CancelledBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class ProcessSuspendedEvent : WorkflowEvent
{
    public override string EventType => "process.suspended";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string SuspendedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class ProcessResumedEvent : WorkflowEvent
{
    public override string EventType => "process.resumed";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ResumedBy { get; set; } = string.Empty;
}

#endregion

#region Activity Events

public class ActivityStartedEvent : WorkflowEvent
{
    public override string EventType => "activity.started";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ActivityInstanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
}

public class ActivityCompletedEvent : WorkflowEvent
{
    public override string EventType => "activity.completed";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ActivityInstanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> OutputVariables { get; set; } = new();
}

public class ActivityFailedEvent : WorkflowEvent
{
    public override string EventType => "activity.failed";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ActivityInstanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int RetryCount { get; set; }
}

#endregion

#region Task Events

public class TaskCreatedEvent : WorkflowEvent
{
    public override string EventType => "task.created";
    public string TaskId { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string TaskDefinitionKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public List<string> CandidateUsers { get; set; } = new();
    public List<string> CandidateGroups { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; }
}

public class TaskAssignedEvent : WorkflowEvent
{
    public override string EventType => "task.assigned";
    public string TaskId { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string PreviousAssignee { get; set; } = string.Empty;
    public string NewAssignee { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
}

public class TaskCompletedEvent : WorkflowEvent
{
    public override string EventType => "task.completed";
    public string TaskId { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string TaskDefinitionKey { get; set; } = string.Empty;
    public string CompletedBy { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> OutputVariables { get; set; } = new();
}

public class TaskDelegatedEvent : WorkflowEvent
{
    public override string EventType => "task.delegated";
    public string TaskId { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string DelegatedFrom { get; set; } = string.Empty;
    public string DelegatedTo { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class TaskDueDateApproachingEvent : WorkflowEvent
{
    public override string EventType => "task.due_date_approaching";
    public string TaskId { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TimeSpan TimeRemaining { get; set; }
}

public class TaskOverdueEvent : WorkflowEvent
{
    public override string EventType => "task.overdue";
    public string TaskId { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public TimeSpan OverdueBy { get; set; }
}

#endregion

#region Timer Events

public class TimerFiredEvent : WorkflowEvent
{
    public override string EventType => "timer.fired";
    public string TimerId { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string TimerType { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
}

public class TimerCancelledEvent : WorkflowEvent
{
    public override string EventType => "timer.cancelled";
    public string TimerId { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

#endregion

#region Integration Events

public class MessageReceivedEvent : WorkflowEvent
{
    public override string EventType => "message.received";
    public string MessageName { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public Dictionary<string, object> MessagePayload { get; set; } = new();
}

public class SignalReceivedEvent : WorkflowEvent
{
    public override string EventType => "signal.received";
    public string SignalName { get; set; } = string.Empty;
    public List<string> AffectedProcessInstanceIds { get; set; } = new();
    public Dictionary<string, object> SignalPayload { get; set; } = new();
}

public class WebhookReceivedEvent : WorkflowEvent
{
    public override string EventType => "webhook.received";
    public string WebhookId { get; set; } = string.Empty;
    public string WebhookName { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public Dictionary<string, object> Payload { get; set; } = new();
}

public class ExternalServiceCalledEvent : WorkflowEvent
{
    public override string EventType => "external_service.called";
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceEndpoint { get; set; } = string.Empty;
    public int ResponseStatusCode { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public bool Success { get; set; }
}

#endregion

#region Subscription Management

public class EventSubscription : IDisposable
{
    private readonly Action _unsubscribe;
    private bool _disposed;

    public EventSubscription(Action unsubscribe)
    {
        _unsubscribe = unsubscribe;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _unsubscribe();
            _disposed = true;
        }
    }
}

#endregion
