namespace XenonClinic.WorkflowEngine.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents an instance of an activity within a process execution.
/// </summary>
[Table("WF_ActivityInstances")]
public class ActivityInstance
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProcessInstanceId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ActivityDefinitionId { get; set; } = string.Empty;

    /// <summary>
    /// Alias for ActivityDefinitionId
    /// </summary>
    [MaxLength(100)]
    public string? ActivityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActivityType { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ActivityName { get; set; }

    public ActivityInstanceStatus Status { get; set; } = ActivityInstanceStatus.Created;

    /// <summary>
    /// Number of execution attempts (for retries)
    /// </summary>
    public int ExecutionCount { get; set; }

    /// <summary>
    /// Number of retry attempts (alias for ExecutionCount)
    /// </summary>
    public int RetryCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [MaxLength(100)]
    public string? CompletedBy { get; set; }

    /// <summary>
    /// Outcome for decision points (e.g., which branch was taken)
    /// </summary>
    [MaxLength(100)]
    public string? Outcome { get; set; }

    [MaxLength(100)]
    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorDetails { get; set; }

    /// <summary>
    /// For multi-instance activities: loop counter
    /// </summary>
    public int? LoopCounter { get; set; }

    /// <summary>
    /// Parent activity instance (for embedded subprocess/multi-instance)
    /// </summary>
    public Guid? ParentActivityInstanceId { get; set; }

    /// <summary>
    /// Input data for this activity execution (JSON)
    /// </summary>
    public string? InputJson { get; set; }

    /// <summary>
    /// Output data from this activity execution (JSON)
    /// </summary>
    public string? OutputJson { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProcessInstanceId))]
    public virtual ProcessInstance? ProcessInstance { get; set; }

    [ForeignKey(nameof(ParentActivityInstanceId))]
    public virtual ActivityInstance? ParentActivityInstance { get; set; }

    public virtual ICollection<ActivityInstance> ChildActivityInstances { get; set; } = new List<ActivityInstance>();

    /// <summary>
    /// Associated human task (if this is a user task)
    /// </summary>
    public virtual HumanTask? HumanTask { get; set; }
}

public enum ActivityInstanceStatus
{
    /// <summary>Activity instance created</summary>
    Created,
    /// <summary>Ready to execute</summary>
    Ready,
    /// <summary>Currently executing</summary>
    Active,
    /// <summary>In completion phase</summary>
    Completing,
    /// <summary>Successfully completed</summary>
    Completed,
    /// <summary>Rolling back</summary>
    Compensating,
    /// <summary>Rollback complete</summary>
    Compensated,
    /// <summary>In failure state</summary>
    Failing,
    /// <summary>Failed (can be retried)</summary>
    Failed,
    /// <summary>Cancelled</summary>
    Cancelled,
    /// <summary>Skipped (for conditional branches)</summary>
    Skipped
}

/// <summary>
/// Activity type constants
/// </summary>
public static class ActivityTypes
{
    // Events
    public const string StartEvent = "startEvent";
    public const string EndEvent = "endEvent";
    public const string TimerStartEvent = "timerStartEvent";
    public const string MessageStartEvent = "messageStartEvent";
    public const string TimerIntermediateEvent = "timerIntermediateEvent";
    public const string MessageIntermediateEvent = "messageIntermediateEvent";
    public const string SignalIntermediateEvent = "signalIntermediateEvent";
    public const string ErrorBoundaryEvent = "errorBoundaryEvent";
    public const string TimerBoundaryEvent = "timerBoundaryEvent";

    // Tasks
    public const string UserTask = "userTask";
    public const string ServiceTask = "serviceTask";
    public const string ScriptTask = "scriptTask";
    public const string SendTask = "sendTask";
    public const string ReceiveTask = "receiveTask";
    public const string ManualTask = "manualTask";
    public const string BusinessRuleTask = "businessRuleTask";

    // Gateways
    public const string ExclusiveGateway = "exclusiveGateway";
    public const string ParallelGateway = "parallelGateway";
    public const string InclusiveGateway = "inclusiveGateway";
    public const string EventBasedGateway = "eventBasedGateway";

    // Subprocess
    public const string SubProcess = "subProcess";
    public const string CallActivity = "callActivity";

    // Multi-instance marker
    public const string MultiInstanceBody = "multiInstanceBody";
}
