namespace XenonClinic.WorkflowEngine.Application.Services;

using XenonClinic.WorkflowEngine.Domain.Entities;

/// <summary>
/// Service for workflow audit logging.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit event.
    /// </summary>
    Task LogAsync(AuditEventDto eventData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries audit events.
    /// </summary>
    Task<AuditQueryResult> QueryAsync(AuditQueryDto query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit trail for a specific process instance.
    /// </summary>
    Task<IList<AuditEventDto>> GetProcessAuditTrailAsync(
        Guid processInstanceId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit trail for a specific entity.
    /// </summary>
    Task<IList<AuditEventDto>> GetEntityAuditTrailAsync(
        string entityType,
        string entityId,
        int tenantId,
        CancellationToken cancellationToken = default);
}

#region DTOs

public class AuditEventDto
{
    public string Id { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public Guid? ProcessInstanceId { get; set; }
    public string? ActivityInstanceId { get; set; }
    public string? UserId { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? OldValues { get; set; }
    public Dictionary<string, object>? NewValues { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AuditQueryDto
{
    public int TenantId { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public Guid? ProcessInstanceId { get; set; }
    public string? UserId { get; set; }
    public string? EventType { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AuditQueryResult
{
    public List<AuditEventDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

#endregion

/// <summary>
/// Common audit event types.
/// </summary>
public static class AuditEventTypes
{
    // Process Definition
    public const string ProcessDefinitionCreated = "process.definition.created";
    public const string ProcessDefinitionUpdated = "process.definition.updated";
    public const string ProcessDefinitionDeleted = "process.definition.deleted";
    public const string ProcessVersionCreated = "process.version.created";
    public const string ProcessVersionPublished = "process.version.published";
    public const string ProcessVersionDeprecated = "process.version.deprecated";

    // Process Instance
    public const string ProcessStarted = "process.started";
    public const string ProcessCompleted = "process.completed";
    public const string ProcessCancelled = "process.cancelled";
    public const string ProcessSuspended = "process.suspended";
    public const string ProcessResumed = "process.resumed";
    public const string ProcessFailed = "process.failed";
    public const string ProcessTerminated = "process.terminated";

    // Activity
    public const string ActivityStarted = "activity.started";
    public const string ActivityCompleted = "activity.completed";
    public const string ActivityFailed = "activity.failed";
    public const string ActivityRetried = "activity.retried";
    public const string ActivitySkipped = "activity.skipped";

    // Human Task
    public const string TaskCreated = "task.created";
    public const string TaskClaimed = "task.claimed";
    public const string TaskUnclaimed = "task.unclaimed";
    public const string TaskAssigned = "task.assigned";
    public const string TaskDelegated = "task.delegated";
    public const string TaskCompleted = "task.completed";
    public const string TaskCancelled = "task.cancelled";
    public const string TaskCommentAdded = "task.comment.added";
    public const string TaskAttachmentAdded = "task.attachment.added";
    public const string TaskPriorityUpdated = "task.priority.updated";
    public const string TaskDueDateUpdated = "task.duedate.updated";

    // Variables
    public const string VariablesSet = "variables.set";
    public const string VariableUpdated = "variable.updated";

    // Timer
    public const string TimerScheduled = "timer.scheduled";
    public const string TimerFired = "timer.fired";
    public const string TimerCancelled = "timer.cancelled";

    // Signal/Message
    public const string SignalSent = "signal.sent";
    public const string SignalReceived = "signal.received";
    public const string MessageSent = "message.sent";
    public const string MessageReceived = "message.received";
}
