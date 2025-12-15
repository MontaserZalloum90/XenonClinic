namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Domain.Models;
using XenonClinic.WorkflowEngine.Infrastructure.Data;

/// <summary>
/// Service implementation for managing human tasks.
/// </summary>
public class HumanTaskService : IHumanTaskService
{
    private readonly WorkflowEngineDbContext _context;
    private readonly IProcessExecutionService _executionService;
    private readonly ILogger<HumanTaskService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HumanTaskService(
        WorkflowEngineDbContext context,
        IProcessExecutionService executionService,
        ILogger<HumanTaskService> logger)
    {
        _context = context;
        _executionService = executionService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<HumanTaskDetailDto?> GetTaskAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var task = await _context.HumanTasks
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId, cancellationToken);

        if (task == null)
            return null;

        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Instances.Any(i => i.Id == task.ProcessInstanceId), cancellationToken);

        var variables = await _executionService.GetVariablesAsync(task.ProcessInstanceId, tenantId, cancellationToken);

        return MapToDetailDto(task, definition, variables);
    }

    public async Task<PagedResultDto<HumanTaskListDto>> ListTasksAsync(
        HumanTaskQueryDto query,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.HumanTasks.Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrEmpty(query.ProcessDefinitionId))
        {
            var instanceIds = await _context.ProcessInstances
                .Where(i => i.ProcessDefinitionId == query.ProcessDefinitionId)
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);
            dbQuery = dbQuery.Where(t => instanceIds.Contains(t.ProcessInstanceId));
        }

        if (query.ProcessInstanceId.HasValue)
            dbQuery = dbQuery.Where(t => t.ProcessInstanceId == query.ProcessInstanceId.Value);

        if (!string.IsNullOrEmpty(query.AssigneeUserId))
            dbQuery = dbQuery.Where(t => t.AssigneeUserId == query.AssigneeUserId);

        if (query.Statuses?.Count > 0)
            dbQuery = dbQuery.Where(t => query.Statuses.Contains(t.Status));
        else if (!query.IncludeCompleted)
            dbQuery = dbQuery.Where(t => t.Status != HumanTaskStatus.Completed && t.Status != HumanTaskStatus.Cancelled);

        if (query.MinPriority.HasValue)
            dbQuery = dbQuery.Where(t => t.Priority >= query.MinPriority.Value);

        if (query.DueBefore.HasValue)
            dbQuery = dbQuery.Where(t => t.DueDate <= query.DueBefore.Value);

        if (query.CreatedAfter.HasValue)
            dbQuery = dbQuery.Where(t => t.CreatedAt >= query.CreatedAfter.Value);

        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLowerInvariant();
            dbQuery = dbQuery.Where(t =>
                t.Name.ToLower().Contains(term) ||
                (t.Description != null && t.Description.ToLower().Contains(term)) ||
                (t.BusinessKey != null && t.BusinessKey.ToLower().Contains(term)));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        // Apply sorting
        dbQuery = query.SortBy?.ToLowerInvariant() switch
        {
            "priority" => query.SortDescending
                ? dbQuery.OrderByDescending(t => t.Priority)
                : dbQuery.OrderBy(t => t.Priority),
            "duedate" => query.SortDescending
                ? dbQuery.OrderByDescending(t => t.DueDate)
                : dbQuery.OrderBy(t => t.DueDate),
            "name" => query.SortDescending
                ? dbQuery.OrderByDescending(t => t.Name)
                : dbQuery.OrderBy(t => t.Name),
            _ => query.SortDescending
                ? dbQuery.OrderByDescending(t => t.CreatedAt)
                : dbQuery.OrderBy(t => t.CreatedAt)
        };

        var tasks = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        // Get process definitions for display names
        var processInstanceIds = tasks.Select(t => t.ProcessInstanceId).Distinct().ToList();
        var instances = await _context.ProcessInstances
            .Where(i => processInstanceIds.Contains(i.Id))
            .Select(i => new { i.Id, i.ProcessDefinitionId })
            .ToListAsync(cancellationToken);

        var definitionIds = instances.Select(i => i.ProcessDefinitionId).Distinct().ToList();
        var definitions = await _context.ProcessDefinitions
            .Where(d => definitionIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        var instanceToDefinition = instances.ToDictionary(i => i.Id, i => i.ProcessDefinitionId);

        var items = tasks.Select(t =>
        {
            instanceToDefinition.TryGetValue(t.ProcessInstanceId, out var defId);
            definitions.TryGetValue(defId ?? "", out var def);
            return MapToListDto(t, def);
        }).ToList();

        return new PagedResultDto<HumanTaskListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
        };
    }

    public async Task<TaskInboxDto> GetTaskInboxAsync(
        string userId,
        List<string> userGroups,
        List<string> userRoles,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var endOfToday = now.Date.AddDays(1);

        // Get tasks assigned to the user
        var assignedTasks = await _context.HumanTasks
            .Where(t => t.TenantId == tenantId &&
                       t.AssigneeUserId == userId &&
                       t.Status != HumanTaskStatus.Completed &&
                       t.Status != HumanTaskStatus.Cancelled)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .Take(50)
            .ToListAsync(cancellationToken);

        // Get unassigned tasks the user can claim
        var availableQuery = _context.HumanTasks
            .Where(t => t.TenantId == tenantId &&
                       t.AssigneeUserId == null &&
                       t.Status == HumanTaskStatus.Created);

        // Filter by candidate users/groups/roles
        var availableTasks = await availableQuery
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate)
            .Take(100)
            .ToListAsync(cancellationToken);

        // Filter in memory for complex candidate matching
        availableTasks = availableTasks.Where(t =>
        {
            var candidateUsers = DeserializeList(t.CandidateUserIdsJson);
            var candidateGroups = DeserializeList(t.CandidateGroupsJson);
            var candidateRoles = DeserializeList(t.CandidateRolesJson);

            // No restrictions means anyone can claim
            if (candidateUsers.Count == 0 && candidateGroups.Count == 0 && candidateRoles.Count == 0)
                return true;

            // Check if user is a candidate
            if (candidateUsers.Contains(userId))
                return true;

            // Check if user is in a candidate group
            if (candidateGroups.Any(g => userGroups.Contains(g)))
                return true;

            // Check if user has a candidate role
            if (candidateRoles.Any(r => userRoles.Contains(r)))
                return true;

            return false;
        }).Take(50).ToList();

        // Get definitions for display
        var allTasks = assignedTasks.Concat(availableTasks).ToList();
        var processInstanceIds = allTasks.Select(t => t.ProcessInstanceId).Distinct().ToList();
        var instances = await _context.ProcessInstances
            .Where(i => processInstanceIds.Contains(i.Id))
            .Select(i => new { i.Id, i.ProcessDefinitionId })
            .ToListAsync(cancellationToken);

        var definitionIds = instances.Select(i => i.ProcessDefinitionId).Distinct().ToList();
        var definitions = await _context.ProcessDefinitions
            .Where(d => definitionIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        var instanceToDefinition = instances.ToDictionary(i => i.Id, i => i.ProcessDefinitionId);

        Func<HumanTask, HumanTaskListDto> mapTask = t =>
        {
            instanceToDefinition.TryGetValue(t.ProcessInstanceId, out var defId);
            definitions.TryGetValue(defId ?? "", out var def);
            return MapToListDto(t, def);
        };

        var allAssignedTasks = assignedTasks.Concat(availableTasks).ToList();
        var overdueCount = allAssignedTasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < now);
        var dueTodayCount = allAssignedTasks.Count(t => t.DueDate.HasValue && t.DueDate.Value >= now && t.DueDate.Value < endOfToday);

        return new TaskInboxDto
        {
            AssignedToMe = assignedTasks.Select(mapTask).ToList(),
            Available = availableTasks.Select(mapTask).ToList(),
            TotalAssigned = assignedTasks.Count,
            TotalAvailable = availableTasks.Count,
            OverdueCount = overdueCount,
            DueTodayCount = dueTodayCount
        };
    }

    public async Task<HumanTaskDetailDto> ClaimTaskAsync(
        string taskId,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(taskId, tenantId, cancellationToken);

        if (task.Status != HumanTaskStatus.Created)
        {
            throw new InvalidOperationException($"Cannot claim task in status {task.Status}.");
        }

        if (task.AssigneeUserId != null)
        {
            throw new InvalidOperationException("Task is already assigned.");
        }

        // Verify user can claim
        var candidateUsers = DeserializeList(task.CandidateUserIdsJson);
        var candidateGroups = DeserializeList(task.CandidateGroupsJson);
        var candidateRoles = DeserializeList(task.CandidateRolesJson);

        // If there are restrictions, verify user is allowed
        // (In a full implementation, would check user's groups and roles)

        task.AssigneeUserId = userId;
        task.Status = HumanTaskStatus.Assigned;
        task.ClaimedAt = DateTime.UtcNow;

        await AddActionAsync(task, TaskActionTypes.Claim, userId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} claimed task {TaskId}", userId, taskId);

        var definition = await GetDefinitionForTaskAsync(task, cancellationToken);
        var variables = await _executionService.GetVariablesAsync(task.ProcessInstanceId, tenantId, cancellationToken);

        return MapToDetailDto(task, definition, variables);
    }

    public async Task<HumanTaskDetailDto> UnclaimTaskAsync(
        string taskId,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(taskId, tenantId, cancellationToken);

        if (task.Status == HumanTaskStatus.Completed || task.Status == HumanTaskStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot unclaim task in status {task.Status}.");
        }

        if (task.AssigneeUserId != userId)
        {
            throw new InvalidOperationException("Can only unclaim tasks assigned to you.");
        }

        task.AssigneeUserId = null;
        task.Status = HumanTaskStatus.Created;
        task.ClaimedAt = null;

        await AddActionAsync(task, TaskActionTypes.Unclaim, userId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} unclaimed task {TaskId}", userId, taskId);

        var definition = await GetDefinitionForTaskAsync(task, cancellationToken);
        var variables = await _executionService.GetVariablesAsync(task.ProcessInstanceId, tenantId, cancellationToken);

        return MapToDetailDto(task, definition, variables);
    }

    public async Task<HumanTaskDetailDto> AssignTaskAsync(
        string taskId,
        string assigneeUserId,
        int tenantId,
        string performedBy,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(taskId, tenantId, cancellationToken);

        if (task.Status == HumanTaskStatus.Completed || task.Status == HumanTaskStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot assign task in status {task.Status}.");
        }

        var previousAssignee = task.AssigneeUserId;
        task.AssigneeUserId = assigneeUserId;
        task.Status = HumanTaskStatus.Assigned;
        task.ClaimedAt ??= DateTime.UtcNow;

        await AddActionAsync(task, TaskActionTypes.Assign, performedBy,
            new Dictionary<string, object>
            {
                ["previousAssignee"] = previousAssignee ?? "",
                ["newAssignee"] = assigneeUserId
            }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task {TaskId} assigned to {AssigneeUserId} by {PerformedBy}",
            taskId, assigneeUserId, performedBy);

        var definition = await GetDefinitionForTaskAsync(task, cancellationToken);
        var variables = await _executionService.GetVariablesAsync(task.ProcessInstanceId, tenantId, cancellationToken);

        return MapToDetailDto(task, definition, variables);
    }

    public async Task<HumanTaskDetailDto> DelegateTaskAsync(
        string taskId,
        string delegateUserId,
        int tenantId,
        string performedBy,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(taskId, tenantId, cancellationToken);

        if (task.Status == HumanTaskStatus.Completed || task.Status == HumanTaskStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot delegate task in status {task.Status}.");
        }

        if (task.AssigneeUserId != performedBy)
        {
            throw new InvalidOperationException("Can only delegate tasks assigned to you.");
        }

        var previousAssignee = task.AssigneeUserId;
        task.AssigneeUserId = delegateUserId;
        task.Status = HumanTaskStatus.Delegated;

        await AddActionAsync(task, TaskActionTypes.Delegate, performedBy,
            new Dictionary<string, object>
            {
                ["previousAssignee"] = previousAssignee ?? "",
                ["delegateTo"] = delegateUserId
            }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task {TaskId} delegated to {DelegateUserId} by {PerformedBy}",
            taskId, delegateUserId, performedBy);

        var definition = await GetDefinitionForTaskAsync(task, cancellationToken);
        var variables = await _executionService.GetVariablesAsync(task.ProcessInstanceId, tenantId, cancellationToken);

        return MapToDetailDto(task, definition, variables);
    }

    public async Task<HumanTaskDetailDto> CompleteTaskAsync(
        string taskId,
        Dictionary<string, object>? outputVariables,
        string? action,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(taskId, tenantId, cancellationToken);

        if (task.Status == HumanTaskStatus.Completed || task.Status == HumanTaskStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot complete task in status {task.Status}.");
        }

        // Verify the user can complete this task
        if (task.AssigneeUserId != null && task.AssigneeUserId != userId)
        {
            throw new InvalidOperationException("Task is assigned to another user.");
        }

        // Set output variables on the process instance
        if (outputVariables != null && outputVariables.Count > 0)
        {
            await _executionService.SetVariablesAsync(
                task.ProcessInstanceId,
                outputVariables,
                tenantId,
                userId,
                cancellationToken);
        }

        // Mark task as completed
        task.Status = HumanTaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        task.CompletedBy = userId;
        task.CompletionAction = action;
        task.OutputVariablesJson = outputVariables != null
            ? JsonSerializer.Serialize(outputVariables, _jsonOptions)
            : null;

        await AddActionAsync(task, action ?? TaskActionTypes.Complete, userId,
            outputVariables, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task {TaskId} completed by {UserId} with action {Action}",
            taskId, userId, action ?? "complete");

        // Continue the process execution
        var activityInstance = await _context.ActivityInstances
            .FirstOrDefaultAsync(ai => ai.Id == task.ActivityInstanceId, cancellationToken);

        if (activityInstance != null)
        {
            var instance = await _context.ProcessInstances
                .FirstOrDefaultAsync(i => i.Id == task.ProcessInstanceId, cancellationToken);

            if (instance != null)
            {
                var version = await _context.ProcessVersions
                    .FirstOrDefaultAsync(v =>
                        v.ProcessDefinitionId == instance.ProcessDefinitionId &&
                        v.Version == instance.ProcessVersion, cancellationToken);

                if (version != null)
                {
                    var model = JsonSerializer.Deserialize<ProcessModel>(version.ModelJson, _jsonOptions);
                    if (model != null)
                    {
                        // Complete the activity to continue the process
                        activityInstance.Status = ActivityInstanceStatus.Completed;
                        activityInstance.CompletedAt = DateTime.UtcNow;

                        var activeActivities = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new();
                        activeActivities.Remove(activityInstance.ActivityId);
                        instance.ActiveActivityIdsJson = JsonSerializer.Serialize(activeActivities);

                        await _context.SaveChangesAsync(cancellationToken);

                        // Find and execute next activities
                        var outgoingFlows = model.SequenceFlows?
                            .Where(f => f.SourceActivityId == activityInstance.ActivityId)
                            .ToList() ?? new();

                        // Determine which flow to take based on action
                        SequenceFlow? selectedFlow = null;
                        if (!string.IsNullOrEmpty(action))
                        {
                            selectedFlow = outgoingFlows.FirstOrDefault(f =>
                                f.Id == action ||
                                f.Name?.Equals(action, StringComparison.OrdinalIgnoreCase) == true);
                        }

                        if (selectedFlow == null && outgoingFlows.Count > 0)
                        {
                            selectedFlow = outgoingFlows.FirstOrDefault(f => f.IsDefault) ?? outgoingFlows.First();
                        }

                        if (selectedFlow != null)
                        {
                            // Signal the process to continue
                            await _executionService.SignalAsync(
                                task.ProcessInstanceId,
                                $"task-complete-{task.Id}",
                                outputVariables,
                                tenantId,
                                userId,
                                cancellationToken);
                        }
                    }
                }
            }
        }

        var definition = await GetDefinitionForTaskAsync(task, cancellationToken);
        var variables = await _executionService.GetVariablesAsync(task.ProcessInstanceId, tenantId, cancellationToken);

        return MapToDetailDto(task, definition, variables);
    }

    public async Task<TaskCommentDto> AddCommentAsync(
        string taskId,
        string content,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(taskId, tenantId, cancellationToken);

        var comment = new TaskComment
        {
            Id = Guid.NewGuid().ToString(),
            TaskId = taskId,
            Content = content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskComments.Add(comment);
        await AddActionAsync(task, TaskActionTypes.Comment, userId,
            new Dictionary<string, object> { ["commentId"] = comment.Id }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new TaskCommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            UserId = comment.UserId,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<IList<TaskCommentDto>> GetCommentsAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.HumanTasks
            .AnyAsync(t => t.Id == taskId && t.TenantId == tenantId, cancellationToken);

        if (!exists)
        {
            throw new KeyNotFoundException($"Task '{taskId}' not found.");
        }

        var comments = await _context.TaskComments
            .Where(c => c.TaskId == taskId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(c => new TaskCommentDto
        {
            Id = c.Id,
            Content = c.Content,
            UserId = c.UserId,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<TaskAttachmentDto> AddAttachmentAsync(
        string taskId,
        string fileName,
        string contentType,
        string url,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(taskId, tenantId, cancellationToken);

        var attachment = new TaskAttachment
        {
            Id = Guid.NewGuid().ToString(),
            TaskId = taskId,
            FileName = fileName,
            ContentType = contentType,
            Url = url,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskAttachments.Add(attachment);
        await AddActionAsync(task, TaskActionTypes.AddAttachment, userId,
            new Dictionary<string, object>
            {
                ["attachmentId"] = attachment.Id,
                ["fileName"] = fileName
            }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new TaskAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            Url = attachment.Url,
            SizeBytes = attachment.SizeBytes,
            UserId = attachment.UserId,
            CreatedAt = attachment.CreatedAt
        };
    }

    public async Task<IList<TaskAttachmentDto>> GetAttachmentsAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.HumanTasks
            .AnyAsync(t => t.Id == taskId && t.TenantId == tenantId, cancellationToken);

        if (!exists)
        {
            throw new KeyNotFoundException($"Task '{taskId}' not found.");
        }

        var attachments = await _context.TaskAttachments
            .Where(a => a.TaskId == taskId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return attachments.Select(a => new TaskAttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            ContentType = a.ContentType,
            Url = a.Url,
            SizeBytes = a.SizeBytes,
            UserId = a.UserId,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task<TaskFormDto?> GetTaskFormAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var task = await _context.HumanTasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId, cancellationToken);

        if (task == null)
            return null;

        var variables = await _executionService.GetVariablesAsync(task.ProcessInstanceId, tenantId, cancellationToken);

        FormDefinition? formDefinition = null;
        if (!string.IsNullOrEmpty(task.FormDefinitionJson))
        {
            formDefinition = JsonSerializer.Deserialize<FormDefinition>(task.FormDefinitionJson, _jsonOptions);
        }

        return new TaskFormDto
        {
            FormKey = task.FormKey,
            FormDefinition = formDefinition,
            Variables = variables
        };
    }

    public async Task<IList<TaskActionDto>> GetTaskHistoryAsync(
        string taskId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.HumanTasks
            .AnyAsync(t => t.Id == taskId && t.TenantId == tenantId, cancellationToken);

        if (!exists)
        {
            throw new KeyNotFoundException($"Task '{taskId}' not found.");
        }

        var actions = await _context.TaskActions
            .Where(a => a.TaskId == taskId)
            .OrderBy(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        return actions.Select(a => new TaskActionDto
        {
            Id = a.Id,
            ActionType = a.ActionType,
            UserId = a.UserId,
            Timestamp = a.Timestamp,
            Comment = a.Comment,
            Data = !string.IsNullOrEmpty(a.DataJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(a.DataJson, _jsonOptions)
                : null
        }).ToList();
    }

    public async Task<HumanTaskDetailDto> UpdatePriorityAsync(
        string taskId,
        TaskPriority priority,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(taskId, tenantId, cancellationToken);

        if (task.Status == HumanTaskStatus.Completed || task.Status == HumanTaskStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot update task in status {task.Status}.");
        }

        var previousPriority = task.Priority;
        task.Priority = priority;

        await AddActionAsync(task, TaskActionTypes.UpdatePriority, userId,
            new Dictionary<string, object>
            {
                ["previousPriority"] = previousPriority.ToString(),
                ["newPriority"] = priority.ToString()
            }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var definition = await GetDefinitionForTaskAsync(task, cancellationToken);
        var variables = await _executionService.GetVariablesAsync(task.ProcessInstanceId, tenantId, cancellationToken);

        return MapToDetailDto(task, definition, variables);
    }

    public async Task<HumanTaskDetailDto> UpdateDueDateAsync(
        string taskId,
        DateTime? dueDate,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var task = await GetTaskOrThrowAsync(taskId, tenantId, cancellationToken);

        if (task.Status == HumanTaskStatus.Completed || task.Status == HumanTaskStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot update task in status {task.Status}.");
        }

        var previousDueDate = task.DueDate;
        task.DueDate = dueDate;

        await AddActionAsync(task, TaskActionTypes.UpdateDueDate, userId,
            new Dictionary<string, object>
            {
                ["previousDueDate"] = previousDueDate?.ToString("O") ?? "",
                ["newDueDate"] = dueDate?.ToString("O") ?? ""
            }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var definition = await GetDefinitionForTaskAsync(task, cancellationToken);
        var variables = await _executionService.GetVariablesAsync(task.ProcessInstanceId, tenantId, cancellationToken);

        return MapToDetailDto(task, definition, variables);
    }

    #region Private Methods

    private async Task<HumanTask> GetTaskOrThrowAsync(string taskId, int tenantId, CancellationToken cancellationToken)
    {
        var task = await _context.HumanTasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.TenantId == tenantId, cancellationToken);

        if (task == null)
        {
            throw new KeyNotFoundException($"Task '{taskId}' not found.");
        }

        return task;
    }

    private async Task<ProcessDefinition?> GetDefinitionForTaskAsync(HumanTask task, CancellationToken cancellationToken)
    {
        var instance = await _context.ProcessInstances
            .FirstOrDefaultAsync(i => i.Id == task.ProcessInstanceId, cancellationToken);

        if (instance == null)
            return null;

        return await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == instance.ProcessDefinitionId, cancellationToken);
    }

    private Task AddActionAsync(
        HumanTask task,
        string actionType,
        string userId,
        Dictionary<string, object>? data = null,
        CancellationToken cancellationToken = default)
    {
        // Check cancellation before doing work
        cancellationToken.ThrowIfCancellationRequested();

        var action = new TaskAction
        {
            Id = Guid.NewGuid().ToString(),
            TaskId = task.Id,
            ActionType = actionType,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            DataJson = data != null ? JsonSerializer.Serialize(data, _jsonOptions) : null
        };

        _context.TaskActions.Add(action);
        return Task.CompletedTask;
    }

    private Task AddActionAsync(
        HumanTask task,
        string actionType,
        string userId,
        CancellationToken cancellationToken)
    {
        return AddActionAsync(task, actionType, userId, null, cancellationToken);
    }

    private List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, _jsonOptions) ?? new List<string>();
        }
        catch (JsonException ex)
        {
            // Log the deserialization failure for debugging
            _logger.LogWarning(ex, "Failed to deserialize JSON list: {Json}", json?.Substring(0, Math.Min(100, json?.Length ?? 0)));
            return new List<string>();
        }
    }

    private HumanTaskListDto MapToListDto(HumanTask task, ProcessDefinition? definition)
    {
        return new HumanTaskListDto
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            AssigneeUserId = task.AssigneeUserId,
            CandidateGroups = DeserializeList(task.CandidateGroupsJson),
            CreatedAt = task.CreatedAt,
            DueDate = task.DueDate,
            BusinessKey = task.BusinessKey,
            ProcessDefinitionKey = definition?.Key ?? "",
            ProcessDefinitionName = definition?.Name ?? "",
            ProcessInstanceId = task.ProcessInstanceId
        };
    }

    private HumanTaskDetailDto MapToDetailDto(HumanTask task, ProcessDefinition? definition, Dictionary<string, object> variables)
    {
        List<TaskActionDefinitionDto> availableActions = new();

        // Parse form definition for actions
        if (!string.IsNullOrEmpty(task.FormDefinitionJson))
        {
            // Would extract actions from form definition
        }

        // Default actions
        if (availableActions.Count == 0)
        {
            availableActions.Add(new TaskActionDefinitionDto
            {
                Id = "complete",
                Label = "Complete",
                Style = "primary",
                IsDefault = true
            });
        }

        return new HumanTaskDetailDto
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            AssigneeUserId = task.AssigneeUserId,
            CandidateUsers = DeserializeList(task.CandidateUserIdsJson),
            CandidateGroups = DeserializeList(task.CandidateGroupsJson),
            CandidateRoles = DeserializeList(task.CandidateRolesJson),
            CreatedAt = task.CreatedAt,
            DueDate = task.DueDate,
            BusinessKey = task.BusinessKey,
            ProcessDefinitionKey = definition?.Key ?? "",
            ProcessDefinitionName = definition?.Name ?? "",
            ProcessInstanceId = task.ProcessInstanceId,
            TenantId = task.TenantId,
            ActivityInstanceId = task.ActivityInstanceId,
            TaskDefinitionKey = task.TaskDefinitionKey,
            FormKey = task.FormKey,
            HasForm = !string.IsNullOrEmpty(task.FormKey) || !string.IsNullOrEmpty(task.FormDefinitionJson),
            Variables = variables,
            ClaimedAt = task.ClaimedAt,
            CompletedAt = task.CompletedAt,
            CompletedBy = task.CompletedBy,
            CompletionAction = task.CompletionAction,
            AvailableActions = availableActions,
            CommentCount = task.Comments?.Count ?? 0,
            AttachmentCount = task.Attachments?.Count ?? 0
        };
    }

    #endregion
}
