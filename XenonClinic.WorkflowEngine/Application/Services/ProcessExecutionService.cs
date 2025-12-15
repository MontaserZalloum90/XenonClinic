namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Domain.Models;
using XenonClinic.WorkflowEngine.Infrastructure.Data;

/// <summary>
/// Core process execution engine.
/// </summary>
public class ProcessExecutionService : IProcessExecutionService
{
    private readonly WorkflowEngineDbContext _context;
    private readonly IExpressionEvaluator _expressionEvaluator;
    private readonly ILogger<ProcessExecutionService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(5);

    public ProcessExecutionService(
        WorkflowEngineDbContext context,
        IExpressionEvaluator expressionEvaluator,
        ILogger<ProcessExecutionService> logger)
    {
        _context = context;
        _expressionEvaluator = expressionEvaluator;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<ProcessInstanceDto> StartProcessAsync(
        StartProcessRequest request,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Find the process definition
        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d =>
                d.TenantId == tenantId &&
                d.Key == request.ProcessKey &&
                d.Status == ProcessDefinitionStatus.Active, cancellationToken);

        if (definition == null)
        {
            throw new KeyNotFoundException($"Process definition with key '{request.ProcessKey}' not found.");
        }

        // Get the version to use
        var versionNumber = request.Version ?? definition.PublishedVersion;
        if (!versionNumber.HasValue)
        {
            throw new InvalidOperationException($"Process definition '{request.ProcessKey}' has no published version.");
        }

        var version = await _context.ProcessVersions
            .FirstOrDefaultAsync(v =>
                v.ProcessDefinitionId == definition.Id &&
                v.Version == versionNumber.Value, cancellationToken);

        if (version == null)
        {
            throw new KeyNotFoundException($"Process version {versionNumber} not found.");
        }

        // Deserialize the model
        var model = JsonSerializer.Deserialize<ProcessModel>(version.ModelJson, _jsonOptions);
        if (model == null)
        {
            throw new InvalidOperationException("Failed to deserialize process model.");
        }

        var now = DateTime.UtcNow;
        var lockHolder = Guid.NewGuid().ToString();

        // Create process instance
        var instance = new ProcessInstance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProcessDefinitionId = definition.Id,
            ProcessVersion = versionNumber.Value,
            BusinessKey = request.BusinessKey,
            Status = ProcessInstanceStatus.Running,
            ActiveActivityIdsJson = "[]",
            StartedAt = now,
            StartedBy = userId,
            ParentInstanceId = request.ParentInstanceId,
            ParentActivityInstanceId = request.ParentActivityInstanceId,
            LockHolder = lockHolder,
            LockExpiry = now.Add(LockDuration)
        };

        _context.ProcessInstances.Add(instance);

        // Create initial variables
        if (request.Variables != null)
        {
            foreach (var kvp in request.Variables)
            {
                var variable = CreateVariable(instance.Id, kvp.Key, kvp.Value);
                _context.ProcessVariables.Add(variable);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Find and execute start event - check both Activities and Elements
        var startEvent = model.Activities?.OfType<StartEventDefinition>().FirstOrDefault()
            ?? model.Elements?.OfType<StartEventDefinition>().FirstOrDefault();
        if (startEvent == null)
        {
            throw new InvalidOperationException("Process model has no start event.");
        }

        try
        {
            await ExecuteActivityAsync(instance, model, startEvent, userId, cancellationToken);
        }
        finally
        {
            // Release lock
            instance.LockHolder = null;
            instance.LockExpiry = null;
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Started process instance {InstanceId} for definition {DefinitionKey} v{Version}",
            instance.Id, request.ProcessKey, versionNumber);

        return MapToDto(instance, definition);
    }

    public async Task<ProcessInstanceDetailDto?> GetInstanceAsync(
        Guid instanceId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var instance = await _context.ProcessInstances
            .Include(i => i.Variables)
            .Include(i => i.ActivityInstances)
            .FirstOrDefaultAsync(i => i.Id == instanceId && i.TenantId == tenantId, cancellationToken);

        if (instance == null)
            return null;

        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == instance.ProcessDefinitionId, cancellationToken);

        var activeTasks = await _context.HumanTasks
            .Where(t => t.ProcessInstanceId == instanceId &&
                       t.Status != HumanTaskStatus.Completed &&
                       t.Status != HumanTaskStatus.Cancelled)
            .ToListAsync(cancellationToken);

        return MapToDetailDto(instance, definition, activeTasks);
    }

    public async Task<PagedResultDto<ProcessInstanceSummaryDto>> ListInstancesAsync(
        ProcessInstanceQueryDto query,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.ProcessInstances.Where(i => i.TenantId == tenantId);

        if (!string.IsNullOrEmpty(query.ProcessDefinitionId))
            dbQuery = dbQuery.Where(i => i.ProcessDefinitionId == query.ProcessDefinitionId);

        if (!string.IsNullOrEmpty(query.BusinessKey))
            dbQuery = dbQuery.Where(i => i.BusinessKey == query.BusinessKey);

        if (query.Statuses?.Count > 0)
            dbQuery = dbQuery.Where(i => query.Statuses.Contains(i.Status));

        if (!string.IsNullOrEmpty(query.StartedBy))
            dbQuery = dbQuery.Where(i => i.StartedBy == query.StartedBy);

        if (query.StartedAfter.HasValue)
            dbQuery = dbQuery.Where(i => i.StartedAt >= query.StartedAfter.Value);

        if (query.StartedBefore.HasValue)
            dbQuery = dbQuery.Where(i => i.StartedAt <= query.StartedBefore.Value);

        if (!query.IncludeSubProcesses)
            dbQuery = dbQuery.Where(i => i.ParentInstanceId == null);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var instances = await dbQuery
            .OrderByDescending(i => i.StartedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        // Get definitions for display names
        var definitionIds = instances.Select(i => i.ProcessDefinitionId).Distinct().ToList();
        var definitions = await _context.ProcessDefinitions
            .Where(d => definitionIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        // Get active task counts
        var instanceIds = instances.Select(i => i.Id).ToList();
        var taskCounts = await _context.HumanTasks
            .Where(t => instanceIds.Contains(t.ProcessInstanceId) &&
                       t.Status != HumanTaskStatus.Completed &&
                       t.Status != HumanTaskStatus.Cancelled)
            .GroupBy(t => t.ProcessInstanceId)
            .Select(g => new { InstanceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.InstanceId, x => x.Count, cancellationToken);

        var items = instances.Select(i =>
        {
            definitions.TryGetValue(i.ProcessDefinitionId, out var def);
            taskCounts.TryGetValue(i.Id, out var taskCount);
            return MapToSummaryDto(i, def, taskCount);
        }).ToList();

        return new PagedResultDto<ProcessInstanceSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
        };
    }

    public async Task<ProcessInstanceDto> SignalAsync(
        Guid instanceId,
        string signalName,
        Dictionary<string, object>? variables,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var instance = await GetAndLockInstanceAsync(instanceId, tenantId, cancellationToken);

        try
        {
            // Set variables if provided
            if (variables != null)
            {
                await SetVariablesInternalAsync(instance.Id, variables, cancellationToken);
            }

            // Find intermediate catch events waiting for this signal
            var model = await GetProcessModelAsync(instance.ProcessDefinitionId, instance.ProcessVersion, cancellationToken);
            var activeActivityIds = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new();

            foreach (var activityId in activeActivityIds.ToList())
            {
                var activity = GetActivityFromModel(model, activityId);
                if (activity is IntermediateCatchEventDefinition catchEvent &&
                    catchEvent.EventType == "signal" &&
                    catchEvent.SignalRef == signalName)
                {
                    // Continue from this activity
                    var activityInstance = await _context.ActivityInstances
                        .FirstOrDefaultAsync(ai =>
                            ai.ProcessInstanceId == instanceId &&
                            ai.ActivityId == activityId &&
                            ai.Status == ActivityInstanceStatus.Active, cancellationToken);

                    if (activityInstance != null)
                    {
                        await CompleteActivityAsync(instance, model, activityInstance, userId, cancellationToken);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            await ReleaseLockAsync(instance, cancellationToken);
        }

        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == instance.ProcessDefinitionId, cancellationToken);

        return MapToDto(instance, definition);
    }

    public async Task CancelAsync(
        Guid instanceId,
        string? reason,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var instance = await GetAndLockInstanceAsync(instanceId, tenantId, cancellationToken);

        try
        {
            if (instance.Status == ProcessInstanceStatus.Completed ||
                instance.Status == ProcessInstanceStatus.Cancelled ||
                instance.Status == ProcessInstanceStatus.Terminated)
            {
                throw new InvalidOperationException($"Cannot cancel process in status {instance.Status}.");
            }

            instance.Status = ProcessInstanceStatus.Cancelled;
            instance.CompletedAt = DateTime.UtcNow;
            instance.ErrorMessage = reason ?? "Cancelled by user";

            // Cancel all active activity instances
            var activeActivities = await _context.ActivityInstances
                .Where(ai => ai.ProcessInstanceId == instanceId &&
                            ai.Status == ActivityInstanceStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var activity in activeActivities)
            {
                activity.Status = ActivityInstanceStatus.Cancelled;
                activity.CompletedAt = DateTime.UtcNow;
            }

            // Cancel all active human tasks
            var activeTasks = await _context.HumanTasks
                .Where(t => t.ProcessInstanceId == instanceId &&
                           t.Status != HumanTaskStatus.Completed &&
                           t.Status != HumanTaskStatus.Cancelled)
                .ToListAsync(cancellationToken);

            foreach (var task in activeTasks)
            {
                task.Status = HumanTaskStatus.Cancelled;
                task.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cancelled process instance {InstanceId} by user {UserId}: {Reason}",
                instanceId, userId, reason);
        }
        finally
        {
            await ReleaseLockAsync(instance, cancellationToken);
        }
    }

    public async Task SuspendAsync(
        Guid instanceId,
        string? reason,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var instance = await GetAndLockInstanceAsync(instanceId, tenantId, cancellationToken);

        try
        {
            if (instance.Status != ProcessInstanceStatus.Running)
            {
                throw new InvalidOperationException($"Cannot suspend process in status {instance.Status}.");
            }

            instance.Status = ProcessInstanceStatus.Suspended;
            instance.ErrorMessage = reason;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Suspended process instance {InstanceId} by user {UserId}: {Reason}",
                instanceId, userId, reason);
        }
        finally
        {
            await ReleaseLockAsync(instance, cancellationToken);
        }
    }

    public async Task<ProcessInstanceDto> ResumeAsync(
        Guid instanceId,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var instance = await GetAndLockInstanceAsync(instanceId, tenantId, cancellationToken);

        try
        {
            if (instance.Status != ProcessInstanceStatus.Suspended)
            {
                throw new InvalidOperationException($"Cannot resume process in status {instance.Status}.");
            }

            instance.Status = ProcessInstanceStatus.Running;
            instance.ErrorMessage = null;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Resumed process instance {InstanceId} by user {UserId}",
                instanceId, userId);
        }
        finally
        {
            await ReleaseLockAsync(instance, cancellationToken);
        }

        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == instance.ProcessDefinitionId, cancellationToken);

        return MapToDto(instance, definition);
    }

    public async Task<ProcessInstanceDto> RetryActivityAsync(
        Guid instanceId,
        string activityInstanceId,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var instance = await GetAndLockInstanceAsync(instanceId, tenantId, cancellationToken);

        try
        {
            var activityInstance = await _context.ActivityInstances
                .FirstOrDefaultAsync(ai =>
                    ai.Id == activityInstanceId &&
                    ai.ProcessInstanceId == instanceId, cancellationToken);

            if (activityInstance == null)
            {
                throw new KeyNotFoundException($"Activity instance '{activityInstanceId}' not found.");
            }

            if (activityInstance.Status != ActivityInstanceStatus.Failed)
            {
                throw new InvalidOperationException("Can only retry failed activities.");
            }

            // Reset the activity instance
            activityInstance.Status = ActivityInstanceStatus.Active;
            activityInstance.ErrorMessage = null;
            activityInstance.RetryCount++;

            // Get the model and re-execute
            var model = await GetProcessModelAsync(instance.ProcessDefinitionId, instance.ProcessVersion, cancellationToken);
            var activity = GetActivityFromModel(model, activityInstance.ActivityId);

            if (activity == null)
            {
                throw new InvalidOperationException($"Activity '{activityInstance.ActivityId}' not found in process model.");
            }

            // Re-execute the activity
            await ExecuteActivityLogicAsync(instance, model, activityInstance, activity, userId, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Retried activity {ActivityInstanceId} in process {InstanceId}",
                activityInstanceId, instanceId);
        }
        finally
        {
            await ReleaseLockAsync(instance, cancellationToken);
        }

        var definition = await _context.ProcessDefinitions
            .FirstOrDefaultAsync(d => d.Id == instance.ProcessDefinitionId, cancellationToken);

        return MapToDto(instance, definition);
    }

    public async Task<Dictionary<string, object>> GetVariablesAsync(
        Guid instanceId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var instance = await _context.ProcessInstances
            .Include(i => i.Variables)
            .FirstOrDefaultAsync(i => i.Id == instanceId && i.TenantId == tenantId, cancellationToken);

        if (instance == null)
        {
            throw new KeyNotFoundException($"Process instance '{instanceId}' not found.");
        }

        return instance.Variables.ToDictionary(
            v => v.Name,
            v => GetVariableValue(v));
    }

    public async Task SetVariablesAsync(
        Guid instanceId,
        Dictionary<string, object> variables,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var instance = await _context.ProcessInstances
            .FirstOrDefaultAsync(i => i.Id == instanceId && i.TenantId == tenantId, cancellationToken);

        if (instance == null)
        {
            throw new KeyNotFoundException($"Process instance '{instanceId}' not found.");
        }

        await SetVariablesInternalAsync(instanceId, variables, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<ActivityExecutionDto>> GetHistoryAsync(
        Guid instanceId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.ProcessInstances
            .AnyAsync(i => i.Id == instanceId && i.TenantId == tenantId, cancellationToken);

        if (!exists)
        {
            throw new KeyNotFoundException($"Process instance '{instanceId}' not found.");
        }

        var auditEvents = await _context.AuditEvents
            .Where(e => e.ProcessInstanceId == instanceId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        return auditEvents.Select(e => new ActivityExecutionDto
        {
            ActivityInstanceId = e.EntityId,
            ActivityId = e.EntityId, // Would need to join with ActivityInstance for actual activity ID
            ActivityName = e.EntityType,
            ActivityType = e.EntityType,
            EventType = e.EventType,
            Timestamp = e.Timestamp,
            UserId = e.UserId,
            Data = !string.IsNullOrEmpty(e.NewValuesJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(e.NewValuesJson, _jsonOptions)
                : null
        }).ToList();
    }

    #region Private Methods

    private async Task ExecuteActivityAsync(
        ProcessInstance instance,
        ProcessModel model,
        ActivityDefinition activity,
        string userId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Create activity instance
        var activityInstance = new ActivityInstance
        {
            Id = Guid.NewGuid().ToString(),
            ProcessInstanceId = instance.Id,
            ActivityId = activity.Id,
            ActivityName = activity.Name ?? activity.Id,
            ActivityType = activity.Type,
            Status = ActivityInstanceStatus.Active,
            StartedAt = now
        };

        _context.ActivityInstances.Add(activityInstance);

        // Update active activities
        var activeActivities = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new();
        activeActivities.Add(activity.Id);
        instance.ActiveActivityIdsJson = JsonSerializer.Serialize(activeActivities);

        await _context.SaveChangesAsync(cancellationToken);

        // Execute the activity based on type
        await ExecuteActivityLogicAsync(instance, model, activityInstance, activity, userId, cancellationToken);
    }

    private async Task ExecuteActivityLogicAsync(
        ProcessInstance instance,
        ProcessModel model,
        ActivityInstance activityInstance,
        ActivityDefinition activity,
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            switch (activity)
            {
                case StartEventDefinition:
                    // Start events complete immediately
                    await CompleteActivityAsync(instance, model, activityInstance, userId, cancellationToken);
                    break;

                case EndEventDefinition endEvent:
                    await HandleEndEventAsync(instance, model, activityInstance, endEvent, userId, cancellationToken);
                    break;

                case UserTaskDefinition userTask:
                    await CreateHumanTaskAsync(instance, activityInstance, userTask, userId, cancellationToken);
                    break;

                case ServiceTaskDefinition serviceTask:
                    await ExecuteServiceTaskAsync(instance, model, activityInstance, serviceTask, userId, cancellationToken);
                    break;

                case ScriptTaskDefinition scriptTask:
                    await ExecuteScriptTaskAsync(instance, model, activityInstance, scriptTask, userId, cancellationToken);
                    break;

                case ExclusiveGatewayDefinition exclusiveGateway:
                    await HandleExclusiveGatewayAsync(instance, model, activityInstance, exclusiveGateway, userId, cancellationToken);
                    break;

                case ParallelGatewayDefinition parallelGateway:
                    await HandleParallelGatewayAsync(instance, model, activityInstance, parallelGateway, userId, cancellationToken);
                    break;

                case IntermediateCatchEventDefinition:
                    // Wait for signal - stays active
                    break;

                case IntermediateThrowEventDefinition throwEvent:
                    // Signal thrown - complete immediately
                    await CompleteActivityAsync(instance, model, activityInstance, userId, cancellationToken);
                    break;

                case SubProcessDefinition subProcess:
                    await StartSubProcessAsync(instance, activityInstance, subProcess, userId, cancellationToken);
                    break;

                case CallActivityDefinition callActivity:
                    await StartCallActivityAsync(instance, activityInstance, callActivity, userId, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unsupported activity type: {Type}", activity.Type);
                    await CompleteActivityAsync(instance, model, activityInstance, userId, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing activity {ActivityId} in process {InstanceId}",
                activity.Id, instance.Id);

            activityInstance.Status = ActivityInstanceStatus.Failed;
            activityInstance.ErrorMessage = ex.Message;
            activityInstance.CompletedAt = DateTime.UtcNow;

            // Check retry policy
            if (activity.RetryPolicy != null && activityInstance.RetryCount < activity.RetryPolicy.MaxRetries)
            {
                // Schedule retry
                var delay = TimeSpan.FromSeconds(
                    activity.RetryPolicy.InitialIntervalSeconds *
                    Math.Pow(activity.RetryPolicy.BackoffMultiplier, activityInstance.RetryCount));

                var job = new AsyncJob
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = instance.TenantId,
                    ProcessInstanceId = instance.Id,
                    ActivityInstanceId = activityInstance.Id,
                    JobType = "retry-activity",
                    Status = JobStatus.Pending,
                    PayloadJson = JsonSerializer.Serialize(new { ActivityId = activity.Id }),
                    CreatedAt = DateTime.UtcNow,
                    NextRetryAt = DateTime.UtcNow.Add(delay),
                    RetryCount = activityInstance.RetryCount
                };

                _context.AsyncJobs.Add(job);
            }
            else
            {
                instance.Status = ProcessInstanceStatus.Failed;
                instance.ErrorMessage = ex.Message;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CompleteActivityAsync(
        ProcessInstance instance,
        ProcessModel model,
        ActivityInstance activityInstance,
        string userId,
        CancellationToken cancellationToken)
    {
        activityInstance.Status = ActivityInstanceStatus.Completed;
        activityInstance.CompletedAt = DateTime.UtcNow;

        // Remove from active activities
        var activeActivities = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new();
        activeActivities.Remove(activityInstance.ActivityId);
        instance.ActiveActivityIdsJson = JsonSerializer.Serialize(activeActivities);

        await _context.SaveChangesAsync(cancellationToken);

        // Find outgoing sequence flows
        var outgoingFlows = model.SequenceFlows?
            .Where(f => f.SourceActivityId == activityInstance.ActivityId)
            .ToList() ?? new();

        foreach (var flow in outgoingFlows)
        {
            var targetActivity = GetActivityFromModel(model, flow.TargetActivityId);
            if (targetActivity != null)
            {
                await ExecuteActivityAsync(instance, model, targetActivity, userId, cancellationToken);
            }
        }
    }

    private async Task HandleEndEventAsync(
        ProcessInstance instance,
        ProcessModel model,
        ActivityInstance activityInstance,
        EndEventDefinition endEvent,
        string userId,
        CancellationToken cancellationToken)
    {
        activityInstance.Status = ActivityInstanceStatus.Completed;
        activityInstance.CompletedAt = DateTime.UtcNow;

        // Remove from active activities
        var activeActivities = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new();
        activeActivities.Remove(activityInstance.ActivityId);

        // Check if this is the last active activity
        if (activeActivities.Count == 0)
        {
            instance.Status = endEvent.IsTerminate
                ? ProcessInstanceStatus.Terminated
                : ProcessInstanceStatus.Completed;
            instance.CompletedAt = DateTime.UtcNow;

            // If this is a sub-process, signal parent
            if (instance.ParentInstanceId.HasValue && instance.ParentActivityInstanceId != null)
            {
                var parentActivityInstance = await _context.ActivityInstances
                    .FirstOrDefaultAsync(ai => ai.Id == instance.ParentActivityInstanceId, cancellationToken);

                if (parentActivityInstance != null)
                {
                    var parentInstance = await _context.ProcessInstances
                        .FirstOrDefaultAsync(i => i.Id == instance.ParentInstanceId, cancellationToken);

                    if (parentInstance != null)
                    {
                        var parentModel = await GetProcessModelAsync(
                            parentInstance.ProcessDefinitionId,
                            parentInstance.ProcessVersion,
                            cancellationToken);

                        await CompleteActivityAsync(parentInstance, parentModel, parentActivityInstance, userId, cancellationToken);
                    }
                }
            }

            _logger.LogInformation(
                "Process instance {InstanceId} completed with status {Status}",
                instance.Id, instance.Status);
        }

        instance.ActiveActivityIdsJson = JsonSerializer.Serialize(activeActivities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateHumanTaskAsync(
        ProcessInstance instance,
        ActivityInstance activityInstance,
        UserTaskDefinition userTask,
        string userId,
        CancellationToken cancellationToken)
    {
        var variables = await GetVariablesDictionaryAsync(instance.Id, cancellationToken);

        var task = new HumanTask
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = instance.TenantId,
            ProcessInstanceId = instance.Id,
            ActivityInstanceId = activityInstance.Id,
            TaskDefinitionKey = userTask.Id,
            Name = EvaluateExpression(userTask.Name ?? userTask.Id, variables),
            Description = userTask.Description != null
                ? EvaluateExpression(userTask.Description, variables)
                : null,
            Status = HumanTaskStatus.Created,
            Priority = userTask.Priority,
            BusinessKey = instance.BusinessKey,
            CreatedAt = DateTime.UtcNow,
            FormKey = userTask.FormKey
        };

        // Handle assignment
        if (userTask.Assignment != null)
        {
            if (!string.IsNullOrEmpty(userTask.Assignment.AssigneeExpression))
            {
                task.AssigneeUserId = EvaluateExpression(userTask.Assignment.AssigneeExpression, variables);
                task.Status = HumanTaskStatus.Assigned;
            }

            if (userTask.Assignment.CandidateUserExpressions?.Count > 0)
            {
                task.CandidateUserIdsJson = JsonSerializer.Serialize(
                    userTask.Assignment.CandidateUserExpressions
                        .Select(e => EvaluateExpression(e, variables))
                        .ToList());
            }

            if (userTask.Assignment.CandidateGroupExpressions?.Count > 0)
            {
                task.CandidateGroupsJson = JsonSerializer.Serialize(
                    userTask.Assignment.CandidateGroupExpressions
                        .Select(e => EvaluateExpression(e, variables))
                        .ToList());
            }
        }

        // Calculate due date
        if (userTask.DueDateExpression != null)
        {
            var dueDateStr = EvaluateExpression(userTask.DueDateExpression, variables);
            if (DateTime.TryParse(dueDateStr, out var dueDate))
            {
                task.DueDate = dueDate;
            }
        }
        else if (userTask.DurationExpression != null)
        {
            var durationStr = EvaluateExpression(userTask.DurationExpression, variables);
            if (TimeSpan.TryParse(durationStr, out var duration))
            {
                task.DueDate = DateTime.UtcNow.Add(duration);
            }
        }

        // Store form definition if inline
        if (userTask.Form != null)
        {
            task.FormDefinitionJson = JsonSerializer.Serialize(userTask.Form, _jsonOptions);
        }

        _context.HumanTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created human task {TaskId} for activity {ActivityId} in process {InstanceId}",
            task.Id, userTask.Id, instance.Id);
    }

    private async Task ExecuteServiceTaskAsync(
        ProcessInstance instance,
        ProcessModel model,
        ActivityInstance activityInstance,
        ServiceTaskDefinition serviceTask,
        string userId,
        CancellationToken cancellationToken)
    {
        // In a full implementation, this would:
        // 1. Call the service endpoint or invoke the service
        // 2. Handle the response
        // 3. Map output variables

        // For now, complete immediately (placeholder for integration)
        _logger.LogInformation(
            "Executing service task {TaskId} (service: {ServiceName}) in process {InstanceId}",
            serviceTask.Id, serviceTask.ServiceName ?? serviceTask.HttpEndpoint, instance.Id);

        await CompleteActivityAsync(instance, model, activityInstance, userId, cancellationToken);
    }

    private async Task ExecuteScriptTaskAsync(
        ProcessInstance instance,
        ProcessModel model,
        ActivityInstance activityInstance,
        ScriptTaskDefinition scriptTask,
        string userId,
        CancellationToken cancellationToken)
    {
        var variables = await GetVariablesDictionaryAsync(instance.Id, cancellationToken);

        // Execute the script
        if (!string.IsNullOrEmpty(scriptTask.Script))
        {
            var result = await _expressionEvaluator.EvaluateAsync(scriptTask.Script, variables);

            // Store result in variable if specified
            if (!string.IsNullOrEmpty(scriptTask.ResultVariable))
            {
                var variable = CreateVariable(instance.Id, scriptTask.ResultVariable, result);
                var existing = await _context.ProcessVariables
                    .FirstOrDefaultAsync(v => v.ProcessInstanceId == instance.Id && v.Name == scriptTask.ResultVariable, cancellationToken);

                if (existing != null)
                {
                    _context.ProcessVariables.Remove(existing);
                }
                _context.ProcessVariables.Add(variable);
            }
        }

        await CompleteActivityAsync(instance, model, activityInstance, userId, cancellationToken);
    }

    private async Task HandleExclusiveGatewayAsync(
        ProcessInstance instance,
        ProcessModel model,
        ActivityInstance activityInstance,
        ExclusiveGatewayDefinition gateway,
        string userId,
        CancellationToken cancellationToken)
    {
        var variables = await GetVariablesDictionaryAsync(instance.Id, cancellationToken);

        // Find outgoing flows
        var outgoingFlows = model.SequenceFlows?
            .Where(f => f.SourceActivityId == gateway.Id)
            .ToList() ?? new();

        SequenceFlow? selectedFlow = null;
        SequenceFlow? defaultFlow = null;

        foreach (var flow in outgoingFlows)
        {
            if (flow.IsDefault)
            {
                defaultFlow = flow;
                continue;
            }

            if (!string.IsNullOrEmpty(flow.ConditionExpression))
            {
                var result = await _expressionEvaluator.EvaluateAsync(flow.ConditionExpression, variables);
                if (result is bool boolResult && boolResult)
                {
                    selectedFlow = flow;
                    break;
                }
            }
        }

        selectedFlow ??= defaultFlow;

        if (selectedFlow == null)
        {
            throw new InvalidOperationException($"No matching condition found for exclusive gateway '{gateway.Id}'.");
        }

        activityInstance.Status = ActivityInstanceStatus.Completed;
        activityInstance.CompletedAt = DateTime.UtcNow;

        var activeActivities = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new();
        activeActivities.Remove(gateway.Id);
        instance.ActiveActivityIdsJson = JsonSerializer.Serialize(activeActivities);

        await _context.SaveChangesAsync(cancellationToken);

        // Execute target activity
        var targetActivity = GetActivityFromModel(model, selectedFlow.TargetActivityId);
        if (targetActivity != null)
        {
            await ExecuteActivityAsync(instance, model, targetActivity, userId, cancellationToken);
        }
    }

    private async Task HandleParallelGatewayAsync(
        ProcessInstance instance,
        ProcessModel model,
        ActivityInstance activityInstance,
        ParallelGatewayDefinition gateway,
        string userId,
        CancellationToken cancellationToken)
    {
        var incomingFlows = model.SequenceFlows?
            .Where(f => f.TargetActivityId == gateway.Id)
            .ToList() ?? new();

        var outgoingFlows = model.SequenceFlows?
            .Where(f => f.SourceActivityId == gateway.Id)
            .ToList() ?? new();

        // For fork (more outgoing than incoming), execute all outgoing paths
        if (outgoingFlows.Count > incomingFlows.Count)
        {
            activityInstance.Status = ActivityInstanceStatus.Completed;
            activityInstance.CompletedAt = DateTime.UtcNow;

            var activeActivities = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new();
            activeActivities.Remove(gateway.Id);
            instance.ActiveActivityIdsJson = JsonSerializer.Serialize(activeActivities);

            await _context.SaveChangesAsync(cancellationToken);

            // Execute all outgoing paths
            foreach (var flow in outgoingFlows)
            {
                var targetActivity = GetActivityFromModel(model, flow.TargetActivityId);
                if (targetActivity != null)
                {
                    await ExecuteActivityAsync(instance, model, targetActivity, userId, cancellationToken);
                }
            }
        }
        // For join (more incoming than outgoing), wait for all incoming paths
        else
        {
            // Check if all incoming paths have completed
            var completedIncomingCount = await _context.ActivityInstances
                .CountAsync(ai =>
                    ai.ProcessInstanceId == instance.Id &&
                    incomingFlows.Select(f => f.SourceActivityId).Contains(ai.ActivityId) &&
                    ai.Status == ActivityInstanceStatus.Completed, cancellationToken);

            // Also count gateway instances for this gateway (in case of loops)
            var gatewayExecutions = await _context.ActivityInstances
                .CountAsync(ai =>
                    ai.ProcessInstanceId == instance.Id &&
                    ai.ActivityId == gateway.Id &&
                    ai.Status == ActivityInstanceStatus.Active, cancellationToken);

            if (gatewayExecutions >= incomingFlows.Count)
            {
                // All paths have arrived, continue
                activityInstance.Status = ActivityInstanceStatus.Completed;
                activityInstance.CompletedAt = DateTime.UtcNow;

                var activeActivities = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new();
                activeActivities.Remove(gateway.Id);
                instance.ActiveActivityIdsJson = JsonSerializer.Serialize(activeActivities);

                await _context.SaveChangesAsync(cancellationToken);

                // Execute outgoing paths
                foreach (var flow in outgoingFlows)
                {
                    var targetActivity = GetActivityFromModel(model, flow.TargetActivityId);
                    if (targetActivity != null)
                    {
                        await ExecuteActivityAsync(instance, model, targetActivity, userId, cancellationToken);
                    }
                }
            }
            // Else wait for more paths to arrive
        }
    }

    private async Task StartSubProcessAsync(
        ProcessInstance instance,
        ActivityInstance activityInstance,
        SubProcessDefinition subProcess,
        string userId,
        CancellationToken cancellationToken)
    {
        // For embedded sub-process, execute the start event within it
        var startEvent = subProcess.Activities?.OfType<StartEventDefinition>().FirstOrDefault()
            ?? subProcess.Elements?.OfType<StartEventDefinition>().FirstOrDefault();
        if (startEvent != null)
        {
            // Create a nested process model context
            var subModel = new ProcessModel
            {
                Activities = subProcess.Activities ?? subProcess.Elements?.ToList(),
                Elements = subProcess.Elements,
                SequenceFlows = subProcess.SequenceFlows
            };

            await ExecuteActivityAsync(instance, subModel, startEvent, userId, cancellationToken);
        }
    }

    private async Task StartCallActivityAsync(
        ProcessInstance instance,
        ActivityInstance activityInstance,
        CallActivityDefinition callActivity,
        string userId,
        CancellationToken cancellationToken)
    {
        var variables = await GetVariablesDictionaryAsync(instance.Id, cancellationToken);

        // Map input variables
        var inputVariables = new Dictionary<string, object>();
        if (callActivity.InputMappings != null)
        {
            foreach (var mapping in callActivity.InputMappings)
            {
                var value = await _expressionEvaluator.EvaluateAsync(mapping.Source, variables);
                inputVariables[mapping.Target] = value;
            }
        }

        // Start the called process
        var startRequest = new StartProcessRequest
        {
            ProcessKey = callActivity.CalledElement,
            Variables = inputVariables,
            ParentInstanceId = instance.Id,
            ParentActivityInstanceId = activityInstance.Id
        };

        await StartProcessAsync(startRequest, instance.TenantId, userId, cancellationToken);
    }

    private async Task<ProcessInstance> GetAndLockInstanceAsync(
        Guid instanceId,
        int tenantId,
        CancellationToken cancellationToken)
    {
        var lockHolder = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;

        // Try to acquire lock
        var instance = await _context.ProcessInstances
            .FirstOrDefaultAsync(i =>
                i.Id == instanceId &&
                i.TenantId == tenantId, cancellationToken);

        if (instance == null)
        {
            throw new KeyNotFoundException($"Process instance '{instanceId}' not found.");
        }

        // Check if locked by another process
        if (instance.LockHolder != null && instance.LockExpiry > now)
        {
            throw new InvalidOperationException("Process instance is locked by another operation.");
        }

        instance.LockHolder = lockHolder;
        instance.LockExpiry = now.Add(LockDuration);
        await _context.SaveChangesAsync(cancellationToken);

        return instance;
    }

    private async Task ReleaseLockAsync(ProcessInstance instance, CancellationToken cancellationToken)
    {
        instance.LockHolder = null;
        instance.LockExpiry = null;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<ProcessModel> GetProcessModelAsync(
        string definitionId,
        int version,
        CancellationToken cancellationToken)
    {
        var processVersion = await _context.ProcessVersions
            .FirstOrDefaultAsync(v =>
                v.ProcessDefinitionId == definitionId &&
                v.Version == version, cancellationToken);

        if (processVersion == null)
        {
            throw new KeyNotFoundException($"Process version {version} not found for definition '{definitionId}'.");
        }

        return JsonSerializer.Deserialize<ProcessModel>(processVersion.ModelJson, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize process model.");
    }

    private async Task<Dictionary<string, object>> GetVariablesDictionaryAsync(
        Guid instanceId,
        CancellationToken cancellationToken)
    {
        var variables = await _context.ProcessVariables
            .Where(v => v.ProcessInstanceId == instanceId)
            .ToListAsync(cancellationToken);

        return variables.ToDictionary(v => v.Name, v => GetVariableValue(v));
    }

    private async Task SetVariablesInternalAsync(
        Guid instanceId,
        Dictionary<string, object> variables,
        CancellationToken cancellationToken)
    {
        var existingVariables = await _context.ProcessVariables
            .Where(v => v.ProcessInstanceId == instanceId)
            .ToListAsync(cancellationToken);

        foreach (var kvp in variables)
        {
            var existing = existingVariables.FirstOrDefault(v => v.Name == kvp.Key);
            if (existing != null)
            {
                _context.ProcessVariables.Remove(existing);
            }

            var newVariable = CreateVariable(instanceId, kvp.Key, kvp.Value);
            _context.ProcessVariables.Add(newVariable);
        }
    }

    private ProcessVariable CreateVariable(Guid instanceId, string name, object value)
    {
        var variable = new ProcessVariable
        {
            Id = Guid.NewGuid(),
            ProcessInstanceId = instanceId,
            Name = name,
            Scope = "process"
        };

        switch (value)
        {
            case string s:
                variable.Type = VariableType.String;
                variable.StringValue = s;
                break;
            case int i:
                variable.Type = VariableType.Integer;
                variable.NumberValue = i;
                break;
            case long l:
                variable.Type = VariableType.Decimal;
                variable.NumberValue = l;
                break;
            case decimal d:
                variable.Type = VariableType.Decimal;
                variable.NumberValue = d;
                break;
            case double dbl:
                variable.Type = VariableType.Decimal;
                // Use Convert.ToDecimal to handle potential precision/overflow issues
                variable.NumberValue = double.IsNaN(dbl) || double.IsInfinity(dbl)
                    ? 0m
                    : Convert.ToDecimal(Math.Round(dbl, 10));
                break;
            case float flt:
                variable.Type = VariableType.Decimal;
                variable.NumberValue = float.IsNaN(flt) || float.IsInfinity(flt)
                    ? 0m
                    : Convert.ToDecimal(flt);
                break;
            case bool b:
                variable.Type = VariableType.Boolean;
                variable.BooleanValue = b;
                break;
            case DateTime dt:
                variable.Type = VariableType.DateTime;
                variable.DateTimeValue = dt;
                break;
            case null:
                variable.Type = VariableType.Object;
                variable.JsonValue = "null";
                break;
            default:
                variable.Type = VariableType.Object;
                variable.JsonValue = JsonSerializer.Serialize(value, _jsonOptions);
                break;
        }

        return variable;
    }

    private object GetVariableValue(ProcessVariable variable)
    {
        return variable.Type switch
        {
            VariableType.String => variable.StringValue ?? string.Empty,
            VariableType.Integer => variable.NumberValue ?? 0,
            VariableType.Decimal => variable.NumberValue ?? 0,
            VariableType.Boolean => variable.BooleanValue ?? false,
            VariableType.DateTime => variable.DateTimeValue ?? DateTime.MinValue,
            VariableType.Date => variable.DateTimeValue ?? DateTime.MinValue,
            VariableType.Object => variable.JsonValue != null && variable.JsonValue != "null"
                ? JsonSerializer.Deserialize<object>(variable.JsonValue, _jsonOptions)!
                : null!,
            VariableType.Array => variable.JsonValue != null
                ? JsonSerializer.Deserialize<object>(variable.JsonValue, _jsonOptions)!
                : new object[0],
            _ => variable.StringValue ?? string.Empty
        };
    }

    private string EvaluateExpression(string expression, Dictionary<string, object> variables)
    {
        // Simple variable substitution for now
        var result = expression;
        foreach (var kvp in variables)
        {
            result = result.Replace($"${{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
            result = result.Replace($"#{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
        }
        return result;
    }

    /// <summary>
    /// Gets an activity from the process model, checking both Activities and Elements collections.
    /// </summary>
    private static ActivityDefinition? GetActivityFromModel(ProcessModel model, string activityId)
    {
        if (string.IsNullOrEmpty(activityId))
            return null;

        // Check Activities collection first
        var activity = model.Activities?.FirstOrDefault(a => a.Id == activityId);
        if (activity != null)
            return activity;

        // Fall back to Elements collection
        return model.Elements?.FirstOrDefault(e => e.Id == activityId);
    }

    private ProcessInstanceDto MapToDto(ProcessInstance instance, ProcessDefinition? definition)
    {
        return new ProcessInstanceDto
        {
            Id = instance.Id,
            ProcessDefinitionId = instance.ProcessDefinitionId,
            ProcessKey = definition?.Key ?? "",
            ProcessName = definition?.Name ?? "",
            ProcessVersion = instance.ProcessVersion,
            Status = instance.Status,
            BusinessKey = instance.BusinessKey,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            StartedBy = instance.StartedBy,
            ActiveActivityIds = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new()
        };
    }

    private ProcessInstanceSummaryDto MapToSummaryDto(ProcessInstance instance, ProcessDefinition? definition, int activeTaskCount)
    {
        return new ProcessInstanceSummaryDto
        {
            Id = instance.Id,
            ProcessDefinitionId = instance.ProcessDefinitionId,
            ProcessKey = definition?.Key ?? "",
            ProcessName = definition?.Name ?? "",
            ProcessVersion = instance.ProcessVersion,
            Status = instance.Status,
            BusinessKey = instance.BusinessKey,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            StartedBy = instance.StartedBy,
            ActiveTaskCount = activeTaskCount
        };
    }

    private ProcessInstanceDetailDto MapToDetailDto(ProcessInstance instance, ProcessDefinition? definition, List<HumanTask> activeTasks)
    {
        return new ProcessInstanceDetailDto
        {
            Id = instance.Id,
            TenantId = instance.TenantId,
            ProcessDefinitionId = instance.ProcessDefinitionId,
            ProcessKey = definition?.Key ?? "",
            ProcessName = definition?.Name ?? "",
            ProcessVersion = instance.ProcessVersion,
            Status = instance.Status,
            BusinessKey = instance.BusinessKey,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            StartedBy = instance.InitiatorUserId,
            ParentInstanceId = instance.ParentInstanceId,
            ParentActivityInstanceId = instance.ParentActivityId,
            ErrorMessage = instance.ErrorJson,
            ActiveActivityIds = JsonSerializer.Deserialize<List<string>>(instance.ActiveActivityIdsJson) ?? new(),
            Variables = instance.Variables.ToDictionary(v => v.Name, v => GetVariableValue(v)),
            ActivityInstances = instance.ActivityInstances.Select(ai => new ActivityInstanceDto
            {
                Id = ai.Id.ToString(),
                ActivityId = ai.ActivityDefinitionId,
                ActivityName = ai.ActivityName,
                ActivityType = ai.ActivityType,
                Status = ai.Status,
                StartedAt = ai.StartedAt ?? DateTime.MinValue,
                CompletedAt = ai.CompletedAt,
                ErrorMessage = ai.ErrorMessage,
                RetryCount = ai.ExecutionCount
            }).ToList(),
            ActiveTasks = activeTasks.Select(t => new HumanTaskSummaryDto
            {
                Id = t.Id.ToString(),
                Name = t.Name,
                Status = t.Status,
                AssigneeUserId = t.AssigneeUserId,
                CreatedAt = t.CreatedAt,
                DueDate = t.DueDate,
                Priority = t.Priority
            }).ToList()
        };
    }

    #endregion
}
