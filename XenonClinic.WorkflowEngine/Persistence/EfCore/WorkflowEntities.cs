namespace XenonClinic.WorkflowEngine.Persistence.EfCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Entity for storing workflow definition metadata.
/// </summary>
[Table("WorkflowDefinitions")]
public class WorkflowDefinitionEntity
{
    [Key]
    [MaxLength(100)]
    public string Id { get; set; } = string.Empty;

    public int Version { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDraft { get; set; } = true;

    public int? TenantId { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// JSON serialized workflow definition
    /// </summary>
    [Required]
    public string DefinitionJson { get; set; } = string.Empty;

    /// <summary>
    /// Concurrency token for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// Entity for storing workflow instance state.
/// </summary>
[Table("WorkflowInstances")]
public class WorkflowInstanceEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string WorkflowId { get; set; } = string.Empty;

    public int Version { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    public int? TenantId { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [MaxLength(200)]
    public string? CorrelationId { get; set; }

    public int Priority { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ScheduledStartTime { get; set; }

    [MaxLength(100)]
    public string? CurrentActivityId { get; set; }

    public int FaultCount { get; set; }

    /// <summary>
    /// JSON serialized state data (input, output, variables, etc.)
    /// </summary>
    [Required]
    public string StateJson { get; set; } = "{}";

    /// <summary>
    /// JSON serialized error information
    /// </summary>
    public string? ErrorJson { get; set; }

    /// <summary>
    /// Lock holder ID for distributed locking
    /// </summary>
    [MaxLength(200)]
    public string? LockHolder { get; set; }

    /// <summary>
    /// Lock expiration time
    /// </summary>
    public DateTime? LockExpiry { get; set; }

    /// <summary>
    /// Concurrency token for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// Entity for storing workflow execution history.
/// </summary>
[Table("WorkflowExecutionHistory")]
public class WorkflowExecutionHistoryEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid InstanceId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ActivityId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ActivityName { get; set; }

    [MaxLength(100)]
    public string? ActivityType { get; set; }

    [Required]
    [MaxLength(50)]
    public string RecordType { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public long? DurationMs { get; set; }

    /// <summary>
    /// JSON serialized output data
    /// </summary>
    public string? OutputJson { get; set; }

    /// <summary>
    /// JSON serialized error data
    /// </summary>
    public string? ErrorJson { get; set; }

    [ForeignKey(nameof(InstanceId))]
    public WorkflowInstanceEntity? Instance { get; set; }
}

/// <summary>
/// Entity for storing workflow bookmarks.
/// </summary>
[Table("WorkflowBookmarks")]
public class WorkflowBookmarkEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid InstanceId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ActivityId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(InstanceId))]
    public WorkflowInstanceEntity? Instance { get; set; }
}

/// <summary>
/// Entity for storing workflow timers.
/// </summary>
[Table("WorkflowTimers")]
public class WorkflowTimerEntity
{
    [Key]
    public Guid Id { get; set; }

    public Guid InstanceId { get; set; }

    [Required]
    [MaxLength(200)]
    public string BookmarkName { get; set; } = string.Empty;

    public DateTime FireAt { get; set; }

    [MaxLength(100)]
    public string? CronExpression { get; set; }

    public bool IsTriggered { get; set; }

    public DateTime? TriggeredAt { get; set; }

    [ForeignKey(nameof(InstanceId))]
    public WorkflowInstanceEntity? Instance { get; set; }
}
