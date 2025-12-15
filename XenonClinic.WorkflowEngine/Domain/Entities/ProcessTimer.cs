namespace XenonClinic.WorkflowEngine.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a scheduled timer for a process instance.
/// </summary>
[Table("WF_ProcessTimers")]
public class ProcessTimer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProcessInstanceId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ActivityDefinitionId { get; set; } = string.Empty;

    public TimerType Type { get; set; }

    /// <summary>
    /// When the timer should fire
    /// </summary>
    public DateTime FireAt { get; set; }

    /// <summary>
    /// For recurring timers: cron expression or ISO 8601 repeating interval
    /// </summary>
    [MaxLength(100)]
    public string? RecurrenceExpression { get; set; }

    /// <summary>
    /// Number of times this timer has fired
    /// </summary>
    public int ExecutionCount { get; set; }

    /// <summary>
    /// Maximum executions for recurring timers (null = unlimited)
    /// </summary>
    public int? MaxExecutions { get; set; }

    public TimerStatus Status { get; set; } = TimerStatus.Pending;

    public DateTime? LastFiredAt { get; set; }

    public DateTime? NextFireAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Bookmark name for resuming the process
    /// </summary>
    [MaxLength(200)]
    public string? BookmarkName { get; set; }

    /// <summary>
    /// Additional data for the timer (JSON)
    /// </summary>
    public string? DataJson { get; set; }

    [ForeignKey(nameof(ProcessInstanceId))]
    public virtual ProcessInstance? ProcessInstance { get; set; }
}

public enum TimerType
{
    /// <summary>Fire at specific date/time</summary>
    Date,
    /// <summary>Fire after duration from creation</summary>
    Duration,
    /// <summary>Fire on schedule (cron)</summary>
    Cycle
}

public enum TimerStatus
{
    Pending,
    Scheduled,
    Triggered,
    Fired,
    Cancelled,
    Expired
}

/// <summary>
/// Represents an async job for background processing.
/// </summary>
[Table("WF_AsyncJobs")]
public class AsyncJob
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public int TenantId { get; set; }

    public Guid? ProcessInstanceId { get; set; }

    public Guid? ActivityInstanceId { get; set; }

    [Required]
    [MaxLength(50)]
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Job payload (JSON)
    /// </summary>
    [Required]
    public string JobDataJson { get; set; } = "{}";

    /// <summary>
    /// Job payload (alias for JobDataJson)
    /// </summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// Job result (JSON)
    /// </summary>
    public string? ResultJson { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Pending;

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; } = 3;

    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Exponential backoff base (seconds)
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 30;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorStackTrace { get; set; }

    /// <summary>
    /// Lock holder for distributed processing
    /// </summary>
    [MaxLength(200)]
    public string? LockOwner { get; set; }

    public DateTime? LockExpiry { get; set; }

    /// <summary>
    /// Priority (higher = processed first)
    /// </summary>
    public int Priority { get; set; } = 5;

    [ForeignKey(nameof(ProcessInstanceId))]
    public virtual ProcessInstance? ProcessInstance { get; set; }

    [ForeignKey(nameof(ActivityInstanceId))]
    public virtual ActivityInstance? ActivityInstance { get; set; }
}

public enum JobStatus
{
    Pending,
    Running,
    Processing,
    Completed,
    Failed,
    Cancelled,
    Retrying,
    DeadLetter
}

/// <summary>
/// Job type constants
/// </summary>
public static class JobTypes
{
    public const string ExecuteActivity = "execute-activity";
    public const string CompleteTask = "complete-task";
    public const string SendNotification = "send-notification";
    public const string SendEmail = "send-email";
    public const string CallWebhook = "call-webhook";
    public const string FireTimer = "fire-timer";
    public const string ExecuteScript = "execute-script";
    public const string CleanupInstance = "cleanup-instance";
}
