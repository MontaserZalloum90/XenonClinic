namespace XenonClinic.WorkflowEngine.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Audit event for compliance and debugging.
/// </summary>
[Table("WF_AuditEvents")]
public class AuditEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public int TenantId { get; set; }

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    public Guid? ProcessInstanceId { get; set; }

    public Guid? TaskId { get; set; }

    [MaxLength(100)]
    public string? ActivityId { get; set; }

    [MaxLength(100)]
    public string? UserId { get; set; }

    [MaxLength(200)]
    public string? UserDisplayName { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? Action { get; set; }

    /// <summary>
    /// Human-readable summary
    /// </summary>
    [MaxLength(500)]
    public string? Summary { get; set; }

    /// <summary>
    /// Previous state (JSON)
    /// </summary>
    public string? OldValuesJson { get; set; }

    /// <summary>
    /// New state (JSON)
    /// </summary>
    public string? NewValuesJson { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Correlation ID for tracing
    /// </summary>
    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Additional context data (JSON)
    /// </summary>
    public string? AdditionalDataJson { get; set; }

    /// <summary>
    /// Duration of the action in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Whether this event indicates an error
    /// </summary>
    public bool IsError { get; set; }

    [MaxLength(100)]
    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Audit event type constants
/// </summary>
public static class AuditEventTypes
{
    // Process lifecycle
    public const string ProcessCreated = "ProcessCreated";
    public const string ProcessStarted = "ProcessStarted";
    public const string ProcessCompleted = "ProcessCompleted";
    public const string ProcessCancelled = "ProcessCancelled";
    public const string ProcessSuspended = "ProcessSuspended";
    public const string ProcessResumed = "ProcessResumed";
    public const string ProcessFaulted = "ProcessFaulted";
    public const string ProcessRetried = "ProcessRetried";
    public const string ProcessMigrated = "ProcessMigrated";

    // Activity lifecycle
    public const string ActivityStarted = "ActivityStarted";
    public const string ActivityCompleted = "ActivityCompleted";
    public const string ActivityFailed = "ActivityFailed";
    public const string ActivityRetried = "ActivityRetried";
    public const string ActivitySkipped = "ActivitySkipped";
    public const string ActivityCancelled = "ActivityCancelled";

    // Task lifecycle
    public const string TaskCreated = "TaskCreated";
    public const string TaskClaimed = "TaskClaimed";
    public const string TaskUnclaimed = "TaskUnclaimed";
    public const string TaskStarted = "TaskStarted";
    public const string TaskCompleted = "TaskCompleted";
    public const string TaskDelegated = "TaskDelegated";
    public const string TaskEscalated = "TaskEscalated";
    public const string TaskReassigned = "TaskReassigned";
    public const string TaskSuspended = "TaskSuspended";
    public const string TaskResumed = "TaskResumed";
    public const string TaskCommented = "TaskCommented";
    public const string TaskAttachmentAdded = "TaskAttachmentAdded";

    // Variable events
    public const string VariableCreated = "VariableCreated";
    public const string VariableUpdated = "VariableUpdated";
    public const string VariableDeleted = "VariableDeleted";

    // Timer events
    public const string TimerScheduled = "TimerScheduled";
    public const string TimerFired = "TimerFired";
    public const string TimerCancelled = "TimerCancelled";

    // Signal/Message events
    public const string SignalReceived = "SignalReceived";
    public const string SignalBroadcast = "SignalBroadcast";
    public const string MessageReceived = "MessageReceived";
    public const string MessageCorrelated = "MessageCorrelated";

    // Definition events
    public const string DefinitionCreated = "DefinitionCreated";
    public const string DefinitionUpdated = "DefinitionUpdated";
    public const string DefinitionVersionCreated = "DefinitionVersionCreated";
    public const string DefinitionPublished = "DefinitionPublished";
    public const string DefinitionDeprecated = "DefinitionDeprecated";
    public const string DefinitionDeleted = "DefinitionDeleted";

    // Admin events
    public const string InstanceMigrated = "InstanceMigrated";
    public const string InstanceForceCompleted = "InstanceForceCompleted";
    public const string JobRetried = "JobRetried";
    public const string JobCancelled = "JobCancelled";
}

/// <summary>
/// Entity type constants for audit
/// </summary>
public static class AuditEntityTypes
{
    public const string ProcessDefinition = "ProcessDefinition";
    public const string ProcessVersion = "ProcessVersion";
    public const string ProcessInstance = "ProcessInstance";
    public const string ActivityInstance = "ActivityInstance";
    public const string HumanTask = "HumanTask";
    public const string ProcessVariable = "ProcessVariable";
    public const string ProcessTimer = "ProcessTimer";
    public const string AsyncJob = "AsyncJob";
}

/// <summary>
/// Aggregated metrics for analytics.
/// </summary>
[Table("WF_ProcessMetrics")]
public class ProcessMetric
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public int TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProcessDefinitionId { get; set; } = string.Empty;

    public int ProcessVersion { get; set; }

    /// <summary>
    /// Start of the aggregation period
    /// </summary>
    public DateTime PeriodStart { get; set; }

    [MaxLength(10)]
    public string Granularity { get; set; } = "Hour"; // Hour, Day

    // Instance counts
    public int InstancesStarted { get; set; }
    public int InstancesCompleted { get; set; }
    public int InstancesCancelled { get; set; }
    public int InstancesFaulted { get; set; }

    // Timing metrics (seconds)
    public double AvgDurationSeconds { get; set; }
    public double MinDurationSeconds { get; set; }
    public double MaxDurationSeconds { get; set; }
    public double P50DurationSeconds { get; set; }
    public double P95DurationSeconds { get; set; }
    public double P99DurationSeconds { get; set; }

    // Task metrics
    public int TasksCreated { get; set; }
    public int TasksCompleted { get; set; }
    public double AvgTaskDurationSeconds { get; set; }

    // SLA metrics
    public int SlaMetCount { get; set; }
    public int SlaMissedCount { get; set; }

    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
}
