namespace XenonClinic.WorkflowEngine.Application.Services;

using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Domain.Entities;

/// <summary>
/// Service for managing human tasks.
/// </summary>
public interface IHumanTaskService
{
    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    Task<HumanTaskDetailDto?> GetTaskAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists tasks with filtering.
    /// </summary>
    Task<PagedResultDto<HumanTaskListDto>> ListTasksAsync(
        HumanTaskQueryDto query,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks assigned to or available for a user.
    /// </summary>
    Task<TaskInboxDto> GetTaskInboxAsync(
        string userId,
        List<string> userGroups,
        List<string> userRoles,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Claims a task.
    /// </summary>
    Task<HumanTaskDetailDto> ClaimTaskAsync(
        string taskId,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unclaims a task (returns to pool).
    /// </summary>
    Task<HumanTaskDetailDto> UnclaimTaskAsync(
        string taskId,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a task to a user.
    /// </summary>
    Task<HumanTaskDetailDto> AssignTaskAsync(
        string taskId,
        string assigneeUserId,
        int tenantId,
        string performedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delegates a task to another user.
    /// </summary>
    Task<HumanTaskDetailDto> DelegateTaskAsync(
        string taskId,
        string delegateUserId,
        int tenantId,
        string performedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a task with output variables.
    /// </summary>
    Task<HumanTaskDetailDto> CompleteTaskAsync(
        string taskId,
        Dictionary<string, object>? outputVariables,
        string? action,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a comment to a task.
    /// </summary>
    Task<TaskCommentDto> AddCommentAsync(
        string taskId,
        string content,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets comments for a task.
    /// </summary>
    Task<IList<TaskCommentDto>> GetCommentsAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an attachment to a task.
    /// </summary>
    Task<TaskAttachmentDto> AddAttachmentAsync(
        string taskId,
        string fileName,
        string contentType,
        string url,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attachments for a task.
    /// </summary>
    Task<IList<TaskAttachmentDto>> GetAttachmentsAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the form definition for a task.
    /// </summary>
    Task<TaskFormDto?> GetTaskFormAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the action history for a task.
    /// </summary>
    Task<IList<TaskActionDto>> GetTaskHistoryAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates task priority.
    /// </summary>
    Task<HumanTaskDetailDto> UpdatePriorityAsync(
        string taskId,
        TaskPriority priority,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates task due date.
    /// </summary>
    Task<HumanTaskDetailDto> UpdateDueDateAsync(
        string taskId,
        DateTime? dueDate,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);
}

#region DTOs

public class HumanTaskQueryDto
{
    public string? ProcessDefinitionId { get; set; }
    public Guid? ProcessInstanceId { get; set; }
    public string? AssigneeUserId { get; set; }
    public List<string>? CandidateGroups { get; set; }
    public List<HumanTaskStatus>? Statuses { get; set; }
    public TaskPriority? MinPriority { get; set; }
    public DateTime? DueBefore { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public string? SearchTerm { get; set; }
    public bool IncludeCompleted { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class HumanTaskListDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HumanTaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public string? AssigneeUserId { get; set; }
    public List<string> CandidateGroups { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? BusinessKey { get; set; }
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string ProcessDefinitionName { get; set; } = string.Empty;
    public Guid ProcessInstanceId { get; set; }
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
}

public class HumanTaskDetailDto : HumanTaskListDto
{
    public int TenantId { get; set; }
    public string ActivityInstanceId { get; set; } = string.Empty;
    public string TaskDefinitionKey { get; set; } = string.Empty;
    public List<string> CandidateUsers { get; set; } = new();
    public List<string> CandidateRoles { get; set; } = new();
    public string? FormKey { get; set; }
    public bool HasForm { get; set; }
    public Dictionary<string, object> Variables { get; set; } = new();
    public DateTime? ClaimedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
    public string? CompletionAction { get; set; }
    public List<TaskActionDefinitionDto> AvailableActions { get; set; } = new();
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
}

public class TaskActionDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Style { get; set; }
    public bool RequiresComment { get; set; }
    public bool IsDefault { get; set; }
}

public class TaskInboxDto
{
    public List<HumanTaskListDto> AssignedToMe { get; set; } = new();
    public List<HumanTaskListDto> Available { get; set; } = new();
    public int TotalAssigned { get; set; }
    public int TotalAvailable { get; set; }
    public int OverdueCount { get; set; }
    public int DueTodayCount { get; set; }
}

public class TaskCommentDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TaskAttachmentDto
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long? SizeBytes { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TaskFormDto
{
    public string? FormKey { get; set; }
    public Domain.Models.FormDefinition? FormDefinition { get; set; }
    public Dictionary<string, object> Variables { get; set; } = new();
}

public class TaskActionDto
{
    public string Id { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Comment { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

#endregion
