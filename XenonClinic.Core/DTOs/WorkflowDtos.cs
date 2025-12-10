namespace XenonClinic.Core.DTOs;

#region Workflow Definition DTOs

/// <summary>
/// DTO for workflow definition list item.
/// </summary>
public class WorkflowDefinitionListDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public bool IsDraft { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for workflow definition details.
/// </summary>
public class WorkflowDefinitionDto : WorkflowDefinitionListDto
{
    public List<WorkflowParameterDto> InputParameters { get; set; } = new();
    public List<WorkflowParameterDto> OutputParameters { get; set; } = new();
    public List<WorkflowVariableDto> Variables { get; set; } = new();
    public List<WorkflowTriggerDto> Triggers { get; set; } = new();
    public int? TenantId { get; set; }
}

/// <summary>
/// DTO for workflow parameter.
/// </summary>
public class WorkflowParameterDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public object? DefaultValue { get; set; }
    public string? ValidationRule { get; set; }
}

/// <summary>
/// DTO for workflow variable.
/// </summary>
public class WorkflowVariableDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public object? DefaultValue { get; set; }
    public string? Scope { get; set; }
}

/// <summary>
/// DTO for workflow trigger.
/// </summary>
public class WorkflowTriggerDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object?> Config { get; set; } = new();
}

/// <summary>
/// Request DTO for listing workflow definitions.
/// </summary>
public class WorkflowDefinitionListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDraft { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Workflow Instance DTOs

/// <summary>
/// DTO for workflow instance list item.
/// </summary>
public class WorkflowInstanceListDto
{
    public Guid Id { get; set; }
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int Version { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for workflow instance details.
/// </summary>
public class WorkflowInstanceDto : WorkflowInstanceListDto
{
    public Dictionary<string, object?> Input { get; set; } = new();
    public Dictionary<string, object?> Output { get; set; } = new();
    public Dictionary<string, object?> Variables { get; set; } = new();
    public string? CurrentActivityId { get; set; }
    public string? CurrentActivityName { get; set; }
    public List<string> CompletedActivityIds { get; set; } = new();
    public WorkflowErrorDto? Error { get; set; }
    public List<WorkflowBookmarkDto> Bookmarks { get; set; } = new();
    public List<WorkflowAuditEntryDto> AuditEntries { get; set; } = new();
    public DateTime? ScheduledStartTime { get; set; }
    public int FaultCount { get; set; }
    public int? TenantId { get; set; }
}

/// <summary>
/// DTO for workflow error.
/// </summary>
public class WorkflowErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActivityId { get; set; }
    public string? ActivityName { get; set; }
}

/// <summary>
/// DTO for workflow bookmark.
/// </summary>
public class WorkflowBookmarkDto
{
    public string Name { get; set; } = string.Empty;
    public string? ActivityId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for workflow audit entry.
/// </summary>
public class WorkflowAuditEntryDto
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Details { get; set; }
}

/// <summary>
/// Request DTO for listing workflow instances.
/// </summary>
public class WorkflowInstanceListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? WorkflowId { get; set; }
    public List<string>? Statuses { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Workflow Execution DTOs

/// <summary>
/// Request DTO for starting a workflow.
/// </summary>
public class StartWorkflowRequestDto
{
    public string WorkflowId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public Dictionary<string, object?>? Input { get; set; }
    public int? Priority { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime? ScheduledStartTime { get; set; }
    public int? Version { get; set; }
    public Dictionary<string, object?>? Metadata { get; set; }
}

/// <summary>
/// Request DTO for resuming a workflow.
/// </summary>
public class ResumeWorkflowRequestDto
{
    public string BookmarkName { get; set; } = string.Empty;
    public Dictionary<string, object?>? Input { get; set; }
}

/// <summary>
/// Request DTO for cancelling a workflow.
/// </summary>
public class CancelWorkflowRequestDto
{
    public string? Reason { get; set; }
}

/// <summary>
/// Request DTO for sending a signal to a workflow.
/// </summary>
public class SendSignalRequestDto
{
    public string SignalName { get; set; } = string.Empty;
    public object? Data { get; set; }
}

/// <summary>
/// Request DTO for broadcasting a signal.
/// </summary>
public class BroadcastSignalRequestDto
{
    public string SignalName { get; set; } = string.Empty;
    public object? Data { get; set; }
    public string? WorkflowId { get; set; }
}

/// <summary>
/// Request DTO for triggering an event.
/// </summary>
public class TriggerEventRequestDto
{
    public string EventName { get; set; } = string.Empty;
    public object? EventData { get; set; }
}

/// <summary>
/// DTO for workflow execution result.
/// </summary>
public class WorkflowExecutionResultDto
{
    public Guid InstanceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object?>? Output { get; set; }
    public WorkflowErrorDto? Error { get; set; }
    public List<WorkflowBookmarkDto>? Bookmarks { get; set; }
    public double DurationMs { get; set; }
    public int ActivitiesExecuted { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsRunning { get; set; }
}

#endregion

#region Workflow Execution History DTOs

/// <summary>
/// DTO for workflow execution record.
/// </summary>
public class WorkflowExecutionRecordDto
{
    public Guid Id { get; set; }
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string RecordType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double? DurationMs { get; set; }
    public Dictionary<string, object?>? Output { get; set; }
    public WorkflowActivityErrorDto? Error { get; set; }
}

/// <summary>
/// DTO for workflow activity error.
/// </summary>
public class WorkflowActivityErrorDto
{
    public string? Code { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// DTO for workflow execution timeline (visualization).
/// </summary>
public class WorkflowExecutionTimelineDto
{
    public Guid InstanceId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double TotalDurationMs { get; set; }
    public List<WorkflowTimelineEntryDto> Entries { get; set; } = new();
}

/// <summary>
/// DTO for workflow timeline entry.
/// </summary>
public class WorkflowTimelineEntryDto
{
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double? DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}

#endregion

#region Workflow Statistics DTOs

/// <summary>
/// DTO for workflow statistics.
/// </summary>
public class WorkflowStatisticsDto
{
    public int TotalDefinitions { get; set; }
    public int ActiveDefinitions { get; set; }
    public int TotalInstances { get; set; }
    public Dictionary<string, int> InstancesByStatus { get; set; } = new();
    public int InstancesCreatedToday { get; set; }
    public int InstancesCompletedToday { get; set; }
    public int InstancesFaultedToday { get; set; }
    public double AverageExecutionTimeMs { get; set; }
    public double SuccessRate { get; set; }
    public List<WorkflowPerformanceDto> TopPerformers { get; set; } = new();
    public List<WorkflowPerformanceDto> BottomPerformers { get; set; } = new();
}

/// <summary>
/// DTO for workflow performance metrics.
/// </summary>
public class WorkflowPerformanceDto
{
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public int ExecutionCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageExecutionTimeMs { get; set; }
    public double MinExecutionTimeMs { get; set; }
    public double MaxExecutionTimeMs { get; set; }
}

#endregion

/// <summary>
/// Validation messages for workflow operations.
/// </summary>
public static class WorkflowValidationMessages
{
    public const string WorkflowIdRequired = "Workflow ID is required";
    public const string WorkflowNotFound = "Workflow definition not found";
    public const string InstanceNotFound = "Workflow instance not found";
    public const string InstanceInvalidState = "Workflow instance is not in a valid state for this operation";
    public const string BookmarkNotFound = "Bookmark not found";
    public const string BookmarkNameRequired = "Bookmark name is required";
    public const string SignalNameRequired = "Signal name is required";
    public const string EventNameRequired = "Event name is required";
    public const string InputValidationFailed = "Input validation failed";
    public const string UnauthorizedAccess = "You do not have permission to access this workflow";
}
