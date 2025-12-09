namespace XenonClinic.Core.Entities;

/// <summary>
/// Workflow definition/template.
/// </summary>
public class WorkflowDefinition : AuditableEntityWithId
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }

    public string EntityType { get; set; } = string.Empty; // e.g., "PurchaseOrder", "LeaveRequest"
    public string Version { get; set; } = "1.0";
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }

    public int? SLAHours { get; set; }
    public bool AllowParallelApproval { get; set; }
    public bool RequireAllApprovers { get; set; }

    public int CompanyId { get; set; }

    public virtual ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    public virtual ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}

/// <summary>
/// Step within a workflow definition.
/// </summary>
public class WorkflowStep : AuditableEntityWithId
{
    public int WorkflowDefinitionId { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public StepType Type { get; set; }
    public ApproverType ApproverType { get; set; }

    // Approver configuration
    public int? ApproverRoleId { get; set; }
    public int? ApproverEmployeeId { get; set; }
    public int? ApproverDepartmentId { get; set; }
    public string? ApproverExpression { get; set; } // Dynamic approver logic

    public bool AllowDelegation { get; set; } = true;
    public bool AllowEscalation { get; set; } = true;
    public int? EscalationHours { get; set; }
    public int? EscalateToRoleId { get; set; }

    public bool CanReject { get; set; } = true;
    public bool CanRequestInfo { get; set; } = true;
    public bool CanDelegate { get; set; } = true;
    public bool CanSkip { get; set; }
    public string? SkipCondition { get; set; }

    public int? NextStepOnApprove { get; set; }
    public int? NextStepOnReject { get; set; }
    public string? ConditionalRouting { get; set; } // JSON routing rules

    public string? NotificationTemplate { get; set; }
    public string? ReminderTemplate { get; set; }
    public int? ReminderIntervalHours { get; set; }

    public virtual WorkflowDefinition? WorkflowDefinition { get; set; }
}

/// <summary>
/// Instance of a running workflow.
/// </summary>
public class WorkflowInstance : AuditableEntityWithId
{
    public int WorkflowDefinitionId { get; set; }
    public string InstanceNumber { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? EntityReference { get; set; }

    public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
    public int CurrentStepId { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? InitiatedBy { get; set; }
    public int? InitiatedByEmployeeId { get; set; }

    public string? CurrentApprover { get; set; }
    public DateTime? DueDate { get; set; }

    public string? FinalOutcome { get; set; }
    public string? FinalComments { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual WorkflowDefinition? WorkflowDefinition { get; set; }
    public virtual ICollection<WorkflowStepInstance> StepInstances { get; set; } = new List<WorkflowStepInstance>();
    public virtual ICollection<WorkflowHistory> History { get; set; } = new List<WorkflowHistory>();
}

/// <summary>
/// Instance of a workflow step.
/// </summary>
public class WorkflowStepInstance : AuditableEntityWithId
{
    public int WorkflowInstanceId { get; set; }
    public int WorkflowStepId { get; set; }
    public int StepOrder { get; set; }

    public StepInstanceStatus Status { get; set; } = StepInstanceStatus.Pending;

    public string? AssignedTo { get; set; }
    public int? AssignedToEmployeeId { get; set; }
    public int? AssignedToRoleId { get; set; }

    public DateTime? AssignedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? ActionAt { get; set; }

    public string? Action { get; set; } // Approve, Reject, RequestInfo, Delegate
    public string? Comments { get; set; }
    public string? ActionBy { get; set; }
    public int? ActionByEmployeeId { get; set; }

    public string? DelegatedTo { get; set; }
    public int? DelegatedToEmployeeId { get; set; }
    public string? DelegationReason { get; set; }

    public int EscalationCount { get; set; }
    public DateTime? LastEscalationAt { get; set; }
    public int ReminderCount { get; set; }
    public DateTime? LastReminderAt { get; set; }

    public virtual WorkflowInstance? WorkflowInstance { get; set; }
    public virtual WorkflowStep? WorkflowStep { get; set; }
}

/// <summary>
/// History/audit trail for workflow.
/// </summary>
public class WorkflowHistory : AuditableEntityWithId
{
    public int WorkflowInstanceId { get; set; }
    public int? WorkflowStepInstanceId { get; set; }

    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty; // Started, Approved, Rejected, Delegated, Escalated, etc.
    public string? ActionBy { get; set; }
    public int? ActionByEmployeeId { get; set; }

    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? Comments { get; set; }
    public string? Details { get; set; } // JSON additional data

    public virtual WorkflowInstance? WorkflowInstance { get; set; }
}

/// <summary>
/// Approval delegation.
/// </summary>
public class ApprovalDelegation : AuditableEntityWithId
{
    public int DelegatorEmployeeId { get; set; }
    public int DelegateEmployeeId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string? WorkflowTypes { get; set; } // JSON array of workflow codes, null = all
    public string? Reason { get; set; }
    public bool IsActive { get; set; } = true;

    public int CompanyId { get; set; }
}

/// <summary>
/// Task/work item in a queue.
/// </summary>
public class WorkflowTask : AuditableEntityWithId
{
    public string TaskNumber { get; set; } = string.Empty;
    public int? WorkflowStepInstanceId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkflowTaskType TaskType { get; set; }
    public WorkflowTaskPriority Priority { get; set; } = WorkflowTaskPriority.Normal;
    public WorkflowTaskStatus Status { get; set; } = WorkflowTaskStatus.Pending;

    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? EntityReference { get; set; }
    public string? EntityUrl { get; set; }

    public string? AssignedTo { get; set; }
    public int? AssignedToEmployeeId { get; set; }
    public int? AssignedToRoleId { get; set; }
    public int? AssignedToDepartmentId { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }

    public string? AvailableActions { get; set; } // JSON array: ["Approve", "Reject"]
    public string? ActionTaken { get; set; }
    public string? ActionComments { get; set; }

    public int CompanyId { get; set; }
    public int BranchId { get; set; }

    public virtual WorkflowStepInstance? WorkflowStepInstance { get; set; }
}

/// <summary>
/// Notification for workflow events.
/// </summary>
public class WorkflowNotification : AuditableEntityWithId
{
    public int? WorkflowInstanceId { get; set; }
    public int? WorkflowTaskId { get; set; }

    public NotificationType Type { get; set; }
    public string? RecipientUserId { get; set; }
    public string? RecipientEmail { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ErrorMessage { get; set; }

    public int CompanyId { get; set; }
}

// Enums
public enum StepType
{
    Approval = 1,
    Review = 2,
    Notification = 3,
    Task = 4,
    Condition = 5,
    Parallel = 6,
    Wait = 7
}

public enum ApproverType
{
    Specific = 1,       // Specific employee
    Role = 2,           // Anyone with role
    Manager = 3,        // Requester's manager
    DepartmentHead = 4, // Department head
    Dynamic = 5,        // Based on expression/rules
    Group = 6           // Any one from a group
}

public enum WorkflowStatus
{
    Pending = 1,
    InProgress = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
    OnHold = 6,
    Expired = 7
}

public enum StepInstanceStatus
{
    Pending = 1,
    InProgress = 2,
    Approved = 3,
    Rejected = 4,
    Skipped = 5,
    Delegated = 6,
    Escalated = 7,
    Expired = 8
}

public enum WorkflowTaskType
{
    Approval = 1,
    Review = 2,
    Action = 3,
    Information = 4
}

public enum WorkflowTaskPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum WorkflowTaskStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Expired = 5
}

public enum NotificationType
{
    Email = 1,
    InApp = 2,
    SMS = 3,
    Push = 4
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Read = 4
}
