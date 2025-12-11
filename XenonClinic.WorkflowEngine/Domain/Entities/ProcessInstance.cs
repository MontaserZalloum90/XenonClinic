namespace XenonClinic.WorkflowEngine.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a running instance of a process.
/// </summary>
[Table("WF_ProcessInstances")]
public class ProcessInstance
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public int TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProcessDefinitionId { get; set; } = string.Empty;

    public int ProcessVersion { get; set; }

    /// <summary>
    /// User-friendly name for this instance
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// External reference (e.g., order number, ticket ID)
    /// </summary>
    [MaxLength(200)]
    public string? BusinessKey { get; set; }

    public ProcessInstanceStatus Status { get; set; } = ProcessInstanceStatus.Created;

    /// <summary>
    /// Parent instance ID if this is a subprocess
    /// </summary>
    public Guid? ParentInstanceId { get; set; }

    /// <summary>
    /// Activity ID in parent that spawned this subprocess
    /// </summary>
    [MaxLength(100)]
    public string? ParentActivityId { get; set; }

    /// <summary>
    /// User who started the process
    /// </summary>
    [MaxLength(100)]
    public string? InitiatorUserId { get; set; }

    public int Priority { get; set; } = 5;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? SuspendedAt { get; set; }

    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    [MaxLength(1000)]
    public string? SuspensionReason { get; set; }

    /// <summary>
    /// Expected completion date for SLA tracking
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Currently active activity IDs (JSON array)
    /// </summary>
    public string ActiveActivityIdsJson { get; set; } = "[]";

    /// <summary>
    /// Completed activity IDs (JSON array)
    /// </summary>
    public string CompletedActivityIdsJson { get; set; } = "[]";

    /// <summary>
    /// Error information if faulted (JSON)
    /// </summary>
    public string? ErrorJson { get; set; }

    /// <summary>
    /// Number of times the instance has faulted
    /// </summary>
    public int FaultCount { get; set; }

    /// <summary>
    /// Lock holder for distributed execution
    /// </summary>
    [MaxLength(200)]
    public string? LockHolder { get; set; }

    public DateTime? LockExpiry { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    [ForeignKey(nameof(TenantId))]
    public virtual Tenant? Tenant { get; set; }

    [ForeignKey(nameof(ProcessDefinitionId))]
    public virtual ProcessDefinition? ProcessDefinition { get; set; }

    [ForeignKey(nameof(ParentInstanceId))]
    public virtual ProcessInstance? ParentInstance { get; set; }

    public virtual ICollection<ProcessInstance> ChildInstances { get; set; } = new List<ProcessInstance>();
    public virtual ICollection<ProcessVariable> Variables { get; set; } = new List<ProcessVariable>();
    public virtual ICollection<ActivityInstance> ActivityInstances { get; set; } = new List<ActivityInstance>();
    public virtual ICollection<ProcessTimer> Timers { get; set; } = new List<ProcessTimer>();
}

public enum ProcessInstanceStatus
{
    /// <summary>Instance created but not yet started</summary>
    Created,
    /// <summary>Actively executing</summary>
    Running,
    /// <summary>Waiting for external event, timer, or human task</summary>
    Waiting,
    /// <summary>Paused by user/admin</summary>
    Suspended,
    /// <summary>Finished successfully</summary>
    Completed,
    /// <summary>Cancelled by user/admin</summary>
    Cancelled,
    /// <summary>Terminated due to unrecoverable error</summary>
    Terminated,
    /// <summary>In error state, can be retried</summary>
    Faulted
}

/// <summary>
/// Represents a process variable value.
/// </summary>
[Table("WF_ProcessVariables")]
public class ProcessVariable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProcessInstanceId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public VariableType Type { get; set; }

    /// <summary>
    /// Scope: null = process level, otherwise activity ID
    /// </summary>
    [MaxLength(100)]
    public string? Scope { get; set; }

    // Type-specific value storage
    public string? StringValue { get; set; }
    public decimal? NumberValue { get; set; }
    public bool? BooleanValue { get; set; }
    public DateTime? DateTimeValue { get; set; }

    /// <summary>
    /// For complex types (Object, Array)
    /// </summary>
    public string? JsonValue { get; set; }

    /// <summary>
    /// For file references
    /// </summary>
    [MaxLength(500)]
    public string? FileReference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    // Navigation
    [ForeignKey(nameof(ProcessInstanceId))]
    public virtual ProcessInstance? ProcessInstance { get; set; }

    /// <summary>
    /// Gets the typed value of this variable.
    /// </summary>
    public object? GetValue()
    {
        return Type switch
        {
            VariableType.String => StringValue,
            VariableType.Integer => NumberValue.HasValue ? (long)NumberValue.Value : null,
            VariableType.Decimal => NumberValue,
            VariableType.Boolean => BooleanValue,
            VariableType.Date => DateTimeValue?.Date,
            VariableType.DateTime => DateTimeValue,
            VariableType.Object or VariableType.Array => JsonValue,
            VariableType.File => FileReference,
            _ => null
        };
    }

    /// <summary>
    /// Sets the value with automatic type detection.
    /// </summary>
    public void SetValue(object? value)
    {
        // Clear all values first
        StringValue = null;
        NumberValue = null;
        BooleanValue = null;
        DateTimeValue = null;
        JsonValue = null;
        FileReference = null;

        if (value == null) return;

        switch (Type)
        {
            case VariableType.String:
                StringValue = value.ToString();
                break;
            case VariableType.Integer:
            case VariableType.Decimal:
                NumberValue = Convert.ToDecimal(value);
                break;
            case VariableType.Boolean:
                BooleanValue = Convert.ToBoolean(value);
                break;
            case VariableType.Date:
            case VariableType.DateTime:
                DateTimeValue = Convert.ToDateTime(value);
                break;
            case VariableType.Object:
            case VariableType.Array:
                JsonValue = value is string s ? s : System.Text.Json.JsonSerializer.Serialize(value);
                break;
            case VariableType.File:
                FileReference = value.ToString();
                break;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}

public enum VariableType
{
    String,
    Integer,
    Decimal,
    Boolean,
    Date,
    DateTime,
    Object,
    Array,
    File
}
