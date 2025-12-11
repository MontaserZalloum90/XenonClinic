namespace XenonClinic.WorkflowEngine.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a human task that requires user action.
/// </summary>
[Table("WF_HumanTasks")]
public class HumanTask
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ActivityInstanceId { get; set; }

    public Guid ProcessInstanceId { get; set; }

    public int TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProcessDefinitionKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ActivityDefinitionId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>
    /// Reference to form definition
    /// </summary>
    [MaxLength(100)]
    public string? FormKey { get; set; }

    /// <summary>
    /// Embedded form definition (JSON)
    /// </summary>
    public string? FormDefinitionJson { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Normal;

    public HumanTaskStatus Status { get; set; } = HumanTaskStatus.Created;

    // Assignment
    /// <summary>
    /// Currently assigned user
    /// </summary>
    [MaxLength(100)]
    public string? AssigneeUserId { get; set; }

    /// <summary>
    /// Candidate users who can claim (JSON array)
    /// </summary>
    public string? CandidateUserIdsJson { get; set; }

    /// <summary>
    /// Candidate groups who can claim (JSON array)
    /// </summary>
    public string? CandidateGroupIdsJson { get; set; }

    /// <summary>
    /// Candidate roles who can claim (JSON array)
    /// </summary>
    public string? CandidateRoleIdsJson { get; set; }

    /// <summary>
    /// Original assignee (before delegation)
    /// </summary>
    [MaxLength(100)]
    public string? OwnerUserId { get; set; }

    // Timing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClaimedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Reminder sent timestamp
    /// </summary>
    public DateTime? ReminderSentAt { get; set; }

    /// <summary>
    /// Escalation timestamp
    /// </summary>
    public DateTime? EscalatedAt { get; set; }

    // Resolution
    [MaxLength(100)]
    public string? CompletedBy { get; set; }

    /// <summary>
    /// Outcome/action taken (e.g., "approved", "rejected")
    /// </summary>
    [MaxLength(100)]
    public string? Outcome { get; set; }

    /// <summary>
    /// Comment provided on completion
    /// </summary>
    public string? CompletionComment { get; set; }

    // Context
    /// <summary>
    /// Copy of business key for quick access
    /// </summary>
    [MaxLength(200)]
    public string? BusinessKey { get; set; }

    /// <summary>
    /// Task-local variables (JSON)
    /// </summary>
    public string? LocalVariablesJson { get; set; }

    /// <summary>
    /// Available actions for this task (JSON array)
    /// </summary>
    public string? AvailableActionsJson { get; set; }

    /// <summary>
    /// Custom metadata (JSON)
    /// </summary>
    public string? MetadataJson { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ActivityInstanceId))]
    public virtual ActivityInstance? ActivityInstance { get; set; }

    [ForeignKey(nameof(ProcessInstanceId))]
    public virtual ProcessInstance? ProcessInstance { get; set; }

    [ForeignKey(nameof(TenantId))]
    public virtual Tenant? Tenant { get; set; }

    public virtual ICollection<TaskAction> Actions { get; set; } = new List<TaskAction>();
    public virtual ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public virtual ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
}

public enum HumanTaskStatus
{
    /// <summary>Task created, not yet available</summary>
    Created,
    /// <summary>Available for claim</summary>
    Ready,
    /// <summary>Claimed by a user</summary>
    Reserved,
    /// <summary>User actively working on it</summary>
    InProgress,
    /// <summary>Paused</summary>
    Suspended,
    /// <summary>Successfully completed</summary>
    Completed,
    /// <summary>Failed</summary>
    Failed,
    /// <summary>Error occurred</summary>
    Error,
    /// <summary>Cancelled (process moved on)</summary>
    Exited,
    /// <summary>Superseded by process change</summary>
    Obsolete
}

public enum TaskPriority
{
    Low = 1,
    BelowNormal = 3,
    Normal = 5,
    AboveNormal = 7,
    High = 8,
    Urgent = 10
}

/// <summary>
/// Records actions taken on a task (audit trail)
/// </summary>
[Table("WF_TaskActions")]
public class TaskAction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? UserDisplayName { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? Outcome { get; set; }

    public string? Comment { get; set; }

    /// <summary>
    /// Variables submitted with this action (JSON)
    /// </summary>
    public string? VariablesJson { get; set; }

    /// <summary>
    /// For delegation: target user
    /// </summary>
    [MaxLength(100)]
    public string? DelegatedToUserId { get; set; }

    /// <summary>
    /// For reassignment: previous assignee
    /// </summary>
    [MaxLength(100)]
    public string? PreviousAssigneeUserId { get; set; }

    [ForeignKey(nameof(TaskId))]
    public virtual HumanTask? Task { get; set; }
}

/// <summary>
/// Task action type constants
/// </summary>
public static class TaskActionTypes
{
    public const string Create = "create";
    public const string Claim = "claim";
    public const string Unclaim = "unclaim";
    public const string Start = "start";
    public const string Stop = "stop";
    public const string Complete = "complete";
    public const string Delegate = "delegate";
    public const string Resolve = "resolve";
    public const string Forward = "forward";
    public const string Suspend = "suspend";
    public const string Resume = "resume";
    public const string Escalate = "escalate";
    public const string AddComment = "addComment";
    public const string AddAttachment = "addAttachment";
    public const string SetPriority = "setPriority";
    public const string SetDueDate = "setDueDate";
    public const string UpdateVariables = "updateVariables";
}

/// <summary>
/// Comment on a task
/// </summary>
[Table("WF_TaskComments")]
public class TaskComment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskId { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? UserDisplayName { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// If true, only visible to assignees/candidates
    /// </summary>
    public bool IsInternal { get; set; }

    [ForeignKey(nameof(TaskId))]
    public virtual HumanTask? Task { get; set; }
}

/// <summary>
/// Attachment on a task
/// </summary>
[Table("WF_TaskAttachments")]
public class TaskAttachment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContentType { get; set; }

    [MaxLength(500)]
    public string? Url { get; set; }

    /// <summary>
    /// Reference to file storage
    /// </summary>
    [MaxLength(500)]
    public string? FileReference { get; set; }

    public long? SizeBytes { get; set; }

    [Required]
    [MaxLength(100)]
    public string UploadedBy { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TaskId))]
    public virtual HumanTask? Task { get; set; }
}

/// <summary>
/// Denormalized table for efficient candidate queries
/// </summary>
[Table("WF_TaskCandidates")]
public class TaskCandidate
{
    public Guid TaskId { get; set; }

    [MaxLength(10)]
    public string CandidateType { get; set; } = string.Empty; // "user", "group", "role"

    [MaxLength(100)]
    public string CandidateId { get; set; } = string.Empty;

    [ForeignKey(nameof(TaskId))]
    public virtual HumanTask? Task { get; set; }
}
