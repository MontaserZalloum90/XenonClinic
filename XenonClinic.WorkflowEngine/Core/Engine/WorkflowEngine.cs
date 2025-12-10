namespace XenonClinic.WorkflowEngine.Core.Engine;

using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Core.Activities;
using XenonClinic.WorkflowEngine.Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

/// <summary>
/// Main workflow engine implementation.
/// </summary>
public class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowDefinitionStore _definitionStore;
    private readonly IWorkflowInstanceStore _instanceStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowEngine> _logger;
    private readonly WorkflowEngineOptions _options;

    public WorkflowEngine(
        IWorkflowDefinitionStore definitionStore,
        IWorkflowInstanceStore instanceStore,
        IServiceProvider serviceProvider,
        ILogger<WorkflowEngine> logger,
        WorkflowEngineOptions? options = null)
    {
        _definitionStore = definitionStore;
        _instanceStore = instanceStore;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options ?? new WorkflowEngineOptions();
    }

    public async Task<IWorkflowInstance> CreateInstanceAsync(
        string workflowId,
        IDictionary<string, object?>? input = null,
        WorkflowInstanceOptions? options = null)
    {
        options ??= new WorkflowInstanceOptions();

        var definition = await _definitionStore.GetAsync(workflowId, options.Version);
        if (definition == null)
        {
            throw new WorkflowNotFoundException($"Workflow definition not found: {workflowId}");
        }

        // Validate inputs
        if (definition.InputParameters != null)
        {
            foreach (var param in definition.InputParameters.Where(p => p.IsRequired))
            {
                if (input == null || !input.ContainsKey(param.Name) || input[param.Name] == null)
                {
                    throw new WorkflowValidationException($"Required input parameter missing: {param.Name}");
                }
            }
        }

        var state = new WorkflowInstanceState
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            Version = definition.Version,
            Name = options.Name ?? $"{definition.Name} - {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
            Status = WorkflowStatus.Pending,
            TenantId = options.TenantId ?? definition.TenantId,
            CreatedBy = options.UserId,
            CorrelationId = options.CorrelationId,
            Priority = options.Priority,
            CreatedAt = DateTime.UtcNow,
            ScheduledStartTime = options.ScheduledStartTime,
            Input = input ?? new Dictionary<string, object?>(),
            Metadata = options.Metadata ?? new Dictionary<string, object?>()
        };

        // Initialize variables with defaults
        if (definition.Variables != null)
        {
            foreach (var variable in definition.Variables)
            {
                state.Variables[variable.Name] = variable.DefaultValue;
            }
        }

        // Apply input parameter defaults
        if (definition.InputParameters != null)
        {
            foreach (var param in definition.InputParameters)
            {
                if (param.DefaultValue != null && !state.Input.ContainsKey(param.Name))
                {
                    state.Input[param.Name] = param.DefaultValue;
                }
            }
        }

        await _instanceStore.SaveAsync(state);

        _logger.LogInformation("Workflow instance created: {InstanceId} for workflow {WorkflowId}",
            state.Id, workflowId);

        return state;
    }

    public async Task<WorkflowExecutionResult> StartAsync(Guid instanceId)
    {
        return await StartAsync(instanceId, CancellationToken.None);
    }

    private async Task<WorkflowExecutionResult> StartAsync(Guid instanceId, CancellationToken cancellationToken)
    {
        var state = await _instanceStore.GetAsync(instanceId);
        if (state == null)
        {
            throw new WorkflowNotFoundException($"Workflow instance not found: {instanceId}");
        }

        if (state.Status != WorkflowStatus.Pending)
        {
            throw new WorkflowInvalidStateException($"Cannot start workflow in state: {state.Status}");
        }

        var definition = await _definitionStore.GetAsync(state.WorkflowId, state.Version);
        if (definition == null)
        {
            throw new WorkflowNotFoundException($"Workflow definition not found: {state.WorkflowId} v{state.Version}");
        }

        state.Status = WorkflowStatus.Running;
        state.StartedAt = DateTime.UtcNow;
        state.CurrentActivityId = definition.StartActivityId;

        return await ExecuteAsync(state, definition, cancellationToken);
    }

    public async Task<WorkflowExecutionResult> StartNewAsync(
        string workflowId,
        IDictionary<string, object?>? input = null,
        WorkflowInstanceOptions? options = null)
    {
        var instance = await CreateInstanceAsync(workflowId, input, options);
        return await StartAsync(instance.Id, CancellationToken.None);
    }

    public async Task<WorkflowExecutionResult> ResumeAsync(
        Guid instanceId,
        string bookmarkName,
        IDictionary<string, object?>? input = null)
    {
        var state = await _instanceStore.GetAsync(instanceId);
        if (state == null)
        {
            throw new WorkflowNotFoundException($"Workflow instance not found: {instanceId}");
        }

        if (state.Status != WorkflowStatus.Suspended)
        {
            throw new WorkflowInvalidStateException($"Cannot resume workflow in state: {state.Status}");
        }

        var bookmark = state.Bookmarks.FirstOrDefault(b => b.Name == bookmarkName);
        if (bookmark == null)
        {
            throw new WorkflowBookmarkNotFoundException($"Bookmark not found: {bookmarkName}");
        }

        var definition = await _definitionStore.GetAsync(state.WorkflowId, state.Version);
        if (definition == null)
        {
            throw new WorkflowNotFoundException($"Workflow definition not found: {state.WorkflowId} v{state.Version}");
        }

        // Remove the bookmark
        state.Bookmarks.Remove(bookmark);
        state.Status = WorkflowStatus.Running;

        // Create cancellation token with timeout
        using var cts = new CancellationTokenSource(_options.DefaultActivityTimeout);
        var cancellationToken = cts.Token;

        // Get the activity that created the bookmark and resume it
        if (bookmark.ActivityId != null)
        {
            var activity = definition.GetActivity(bookmark.ActivityId);
            if (activity is ResumableActivityBase resumable)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = new WorkflowContext(state, scope.ServiceProvider, cancellationToken, _logger);

                var result = await resumable.ResumeAsync(context, input);
                if (result.IsSuccess)
                {
                    state.CompletedActivityIds.Add(bookmark.ActivityId);
                    state.CompensationStack.Push(bookmark.ActivityId);

                    // Continue execution from next activity
                    var nextActivityId = result.NextActivityId ?? GetNextActivityId(definition, bookmark.ActivityId);
                    if (nextActivityId != null)
                    {
                        state.CurrentActivityId = nextActivityId;
                    }
                }
            }
        }

        return await ExecuteAsync(state, definition, cancellationToken);
    }

    public async Task CancelAsync(Guid instanceId, string? reason = null)
    {
        var state = await _instanceStore.GetAsync(instanceId);
        if (state == null)
        {
            throw new WorkflowNotFoundException($"Workflow instance not found: {instanceId}");
        }

        if (state.Status is WorkflowStatus.Completed or WorkflowStatus.Cancelled or WorkflowStatus.Compensated)
        {
            throw new WorkflowInvalidStateException($"Cannot cancel workflow in state: {state.Status}");
        }

        state.Status = WorkflowStatus.Cancelled;
        state.CompletedAt = DateTime.UtcNow;
        state.AuditEntries.Add(new WorkflowAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = "Cancelled",
            Details = reason
        });

        await _instanceStore.SaveAsync(state);

        _logger.LogInformation("Workflow instance cancelled: {InstanceId}, Reason: {Reason}",
            instanceId, reason ?? "Not specified");
    }

    public async Task TerminateAsync(Guid instanceId, string? reason = null)
    {
        var state = await _instanceStore.GetAsync(instanceId);
        if (state == null)
        {
            throw new WorkflowNotFoundException($"Workflow instance not found: {instanceId}");
        }

        state.Status = WorkflowStatus.Faulted;
        state.CompletedAt = DateTime.UtcNow;
        state.Error = new WorkflowError
        {
            Code = "TERMINATED",
            Message = reason ?? "Workflow terminated"
        };

        await _instanceStore.SaveAsync(state);

        _logger.LogWarning("Workflow instance terminated: {InstanceId}, Reason: {Reason}",
            instanceId, reason ?? "Not specified");
    }

    public async Task<WorkflowExecutionResult> RetryAsync(Guid instanceId)
    {
        var state = await _instanceStore.GetAsync(instanceId);
        if (state == null)
        {
            throw new WorkflowNotFoundException($"Workflow instance not found: {instanceId}");
        }

        if (state.Status != WorkflowStatus.Faulted)
        {
            throw new WorkflowInvalidStateException($"Cannot retry workflow in state: {state.Status}");
        }

        var definition = await _definitionStore.GetAsync(state.WorkflowId, state.Version);
        if (definition == null)
        {
            throw new WorkflowNotFoundException($"Workflow definition not found: {state.WorkflowId} v{state.Version}");
        }

        state.Status = WorkflowStatus.Running;
        state.Error = null;
        state.FaultCount++;

        return await ExecuteAsync(state, definition, CancellationToken.None);
    }

    public async Task<IWorkflowInstance?> GetInstanceAsync(Guid instanceId)
    {
        return await _instanceStore.GetAsync(instanceId);
    }

    public async Task<WorkflowInstanceQueryResult> QueryInstancesAsync(WorkflowInstanceQuery query)
    {
        return await _instanceStore.QueryAsync(query);
    }

    public async Task SignalAsync(Guid instanceId, string signalName, object? data = null)
    {
        var state = await _instanceStore.GetAsync(instanceId);
        if (state == null)
        {
            throw new WorkflowNotFoundException($"Workflow instance not found: {instanceId}");
        }

        var signalBookmark = state.Bookmarks.FirstOrDefault(b =>
            b.Name.StartsWith($"signal_{signalName}_"));

        if (signalBookmark != null)
        {
            var input = new Dictionary<string, object?> { ["signalData"] = data };
            await ResumeAsync(instanceId, signalBookmark.Name, input);
        }

        _logger.LogInformation("Signal sent to workflow {InstanceId}: {SignalName}", instanceId, signalName);
    }

    public async Task BroadcastSignalAsync(string signalName, object? data = null, string? workflowId = null)
    {
        var query = new WorkflowInstanceQuery
        {
            Statuses = new List<WorkflowStatus> { WorkflowStatus.Suspended },
            WorkflowId = workflowId
        };

        var instances = await _instanceStore.QueryAsync(query);
        foreach (var instance in instances.Items)
        {
            try
            {
                await SignalAsync(instance.Id, signalName, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting signal to instance {InstanceId}", instance.Id);
            }
        }
    }

    public async Task<IList<WorkflowExecutionResult>> TriggerEventAsync(string eventName, object? eventData = null)
    {
        var results = new List<WorkflowExecutionResult>();

        // Find workflow definitions with matching event triggers
        var definitions = await _definitionStore.GetByTriggerAsync(TriggerType.Event, eventName);

        foreach (var definition in definitions)
        {
            try
            {
                var input = new Dictionary<string, object?>
                {
                    ["eventName"] = eventName,
                    ["eventData"] = eventData
                };

                var result = await StartNewAsync(definition.Id, input);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering workflow {WorkflowId} for event {EventName}",
                    definition.Id, eventName);
            }
        }

        return results;
    }

    public async Task<IList<WorkflowExecutionRecord>> GetHistoryAsync(Guid instanceId)
    {
        return await _instanceStore.GetHistoryAsync(instanceId);
    }

    private async Task<WorkflowExecutionResult> ExecuteAsync(
        WorkflowInstanceState state,
        IWorkflowDefinition definition,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var activitiesExecuted = 0;

        // Create a linked token with timeout if no token provided
        using var timeoutCts = cancellationToken == default
            ? new CancellationTokenSource(_options.DefaultActivityTimeout)
            : null;
        using var linkedCts = timeoutCts != null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token)
            : null;
        var effectiveToken = linkedCts?.Token ?? cancellationToken;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = new WorkflowContext(state, scope.ServiceProvider, effectiveToken, _logger);

            while (state.Status == WorkflowStatus.Running && state.CurrentActivityId != null)
            {
                // Check for cancellation at the start of each activity
                if (effectiveToken.IsCancellationRequested)
                {
                    state.Status = WorkflowStatus.Cancelled;
                    state.CompletedAt = DateTime.UtcNow;
                    state.AuditEntries.Add(new WorkflowAuditEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        Action = "Cancelled",
                        Details = "Workflow cancelled due to cancellation request"
                    });
                    break;
                }
                var activity = definition.GetActivity(state.CurrentActivityId);
                if (activity == null)
                {
                    throw new WorkflowActivityNotFoundException(
                        $"Activity not found: {state.CurrentActivityId}");
                }

                activitiesExecuted++;
                state.ActiveActivityIds.Clear();
                state.ActiveActivityIds.Add(state.CurrentActivityId);

                // Record execution start
                await RecordExecutionAsync(state.Id, state.CurrentActivityId, activity,
                    ExecutionRecordType.ActivityStarted, null, null, null);

                var activityStopwatch = Stopwatch.StartNew();
                ActivityResult result;

                try
                {
                    result = await activity.ExecuteAsync(context);
                }
                catch (Exception ex)
                {
                    result = ActivityResult.Failure("EXECUTION_ERROR", ex.Message, ex);
                }

                activityStopwatch.Stop();

                // Record execution result
                await RecordExecutionAsync(state.Id, state.CurrentActivityId, activity,
                    result.IsSuccess ? ExecutionRecordType.ActivityCompleted : ExecutionRecordType.ActivityFaulted,
                    activityStopwatch.Elapsed, result.Output, result.Error);

                if (!result.IsSuccess)
                {
                    // Try to handle error with boundary events
                    var handled = await TryHandleErrorAsync(state, definition, result.Error);
                    if (!handled)
                    {
                        state.Status = WorkflowStatus.Faulted;
                        state.Error = new WorkflowError
                        {
                            Code = result.Error?.Code ?? "UNKNOWN",
                            Message = result.Error?.Message ?? "Activity execution failed",
                            ActivityId = state.CurrentActivityId,
                            StackTrace = result.Error?.Exception?.StackTrace
                        };
                        break;
                    }
                    continue;
                }

                if (result.Suspend)
                {
                    state.Status = WorkflowStatus.Suspended;
                    if (result.BookmarkName != null)
                    {
                        state.Bookmarks.Add(new WorkflowBookmark
                        {
                            Name = result.BookmarkName,
                            ActivityId = state.CurrentActivityId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    break;
                }

                // Track completed activity
                if (!state.CompletedActivityIds.Contains(state.CurrentActivityId))
                {
                    state.CompletedActivityIds.Add(state.CurrentActivityId);
                    if (activity.CanCompensate)
                    {
                        state.CompensationStack.Push(state.CurrentActivityId);
                    }
                }

                // Determine next activity
                string? nextActivityId = null;

                if (result.ParallelNextActivityIds?.Count > 0)
                {
                    // Parallel execution
                    await ExecuteParallelBranchesAsync(state, definition, result.ParallelNextActivityIds, context);
                    // Find converging gateway
                    nextActivityId = FindConvergingGateway(definition, state.CurrentActivityId);
                }
                else if (result.NextActivityId != null)
                {
                    nextActivityId = result.NextActivityId;
                }
                else
                {
                    nextActivityId = GetNextActivityId(definition, state.CurrentActivityId);
                }

                if (nextActivityId == null)
                {
                    // Check if this is an end activity
                    if (activity.Type == "end")
                    {
                        state.Status = WorkflowStatus.Completed;
                        state.CompletedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // No more activities - implicit end
                        state.Status = WorkflowStatus.Completed;
                        state.CompletedAt = DateTime.UtcNow;
                    }
                    break;
                }

                state.CurrentActivityId = nextActivityId;

                // Safety check for infinite loops
                if (activitiesExecuted > _options.MaxActivitiesPerExecution)
                {
                    throw new WorkflowExecutionException(
                        $"Maximum activities ({_options.MaxActivitiesPerExecution}) exceeded");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Workflow execution error: {InstanceId}", state.Id);
            state.Status = WorkflowStatus.Faulted;
            state.Error = new WorkflowError
            {
                Code = "EXECUTION_ERROR",
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };
        }

        stopwatch.Stop();
        await _instanceStore.SaveAsync(state);

        _logger.LogInformation(
            "Workflow execution finished: {InstanceId}, Status: {Status}, Duration: {Duration}ms, Activities: {Count}",
            state.Id, state.Status, stopwatch.ElapsedMilliseconds, activitiesExecuted);

        return new WorkflowExecutionResult
        {
            InstanceId = state.Id,
            Status = state.Status,
            Output = state.Output,
            Error = state.Error,
            Bookmarks = state.Bookmarks.ToList(),
            Duration = stopwatch.Elapsed,
            ActivitiesExecuted = activitiesExecuted
        };
    }

    private async Task ExecuteParallelBranchesAsync(
        WorkflowInstanceState state,
        IWorkflowDefinition definition,
        IList<string> branchActivityIds,
        WorkflowContext context)
    {
        var branchId = Guid.NewGuid().ToString();
        var branch = new ParallelBranch
        {
            Id = branchId,
            ParentActivityId = state.CurrentActivityId!,
            ActivityIds = branchActivityIds.ToList(),
            Status = ParallelBranchStatus.Running
        };

        state.ParallelBranches[branchId] = branch;

        // Execute branches in parallel using Task.WhenAll
        var branchTasks = branchActivityIds.Select(async activityId =>
        {
            var activity = definition.GetActivity(activityId);
            if (activity == null)
            {
                return (activityId, ActivityResult.Failure("ACTIVITY_NOT_FOUND", $"Activity not found: {activityId}"));
            }

            // Check for cancellation before executing
            if (context.CancellationToken.IsCancellationRequested)
            {
                return (activityId, ActivityResult.Failure("CANCELLED", "Execution cancelled"));
            }

            var result = await activity.ExecuteAsync(context);
            return (activityId, result);
        }).ToList();

        var results = await Task.WhenAll(branchTasks);

        // Process results
        var allSucceeded = true;
        foreach (var (activityId, result) in results)
        {
            if (result.IsSuccess)
            {
                // Thread-safe add to completed activities
                lock (state.CompletedActivityIds)
                {
                    state.CompletedActivityIds.Add(activityId);
                }
            }
            else
            {
                allSucceeded = false;
                _logger.LogWarning("Parallel branch activity {ActivityId} failed: {Error}",
                    activityId, result.Error?.Message);
            }
        }

        branch.Status = allSucceeded ? ParallelBranchStatus.Completed : ParallelBranchStatus.Faulted;
    }

    private string? GetNextActivityId(IWorkflowDefinition definition, string currentActivityId)
    {
        var transitions = definition.GetOutgoingTransitions(currentActivityId);
        var defaultTransition = transitions.FirstOrDefault(t => t.IsDefault)
            ?? transitions.FirstOrDefault();
        return defaultTransition?.TargetActivityId;
    }

    private string? FindConvergingGateway(IWorkflowDefinition definition, string splitGatewayId)
    {
        // Find the parallel join gateway that corresponds to the split
        foreach (var activity in definition.Activities.Values)
        {
            if (activity is ParallelGatewayActivity gateway && gateway.Direction == GatewayDirection.Join)
            {
                var incoming = definition.GetIncomingTransitions(activity.Id);
                if (incoming.Any())
                {
                    return activity.Id;
                }
            }
        }
        return null;
    }

    private async Task<bool> TryHandleErrorAsync(
        WorkflowInstanceState state,
        IWorkflowDefinition definition,
        ActivityError? error)
    {
        // Check for error boundary events
        foreach (var activity in definition.Activities.Values)
        {
            if (activity is ErrorBoundaryActivity boundary
                && boundary.AttachedToActivityId == state.CurrentActivityId
                && boundary.HandlesError(error?.Code))
            {
                if (boundary.ErrorHandlerActivityId != null)
                {
                    state.CurrentActivityId = boundary.ErrorHandlerActivityId;
                    return true;
                }
            }
        }

        // Check global error handlers
        if (definition.ErrorHandlers != null)
        {
            foreach (var handler in definition.ErrorHandlers)
            {
                if (handler.ErrorCodes == null || handler.ErrorCodes.Count == 0
                    || (error?.Code != null && handler.ErrorCodes.Contains(error.Code)))
                {
                    if (handler.Compensate)
                    {
                        await CompensateAsync(state, definition);
                        return true;
                    }

                    if (handler.HandlerActivityId != null)
                    {
                        state.CurrentActivityId = handler.HandlerActivityId;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private async Task CompensateAsync(
        WorkflowInstanceState state,
        IWorkflowDefinition definition,
        CancellationToken cancellationToken = default)
    {
        state.Status = WorkflowStatus.Compensating;

        using var scope = _serviceProvider.CreateScope();
        var context = new WorkflowContext(state, scope.ServiceProvider, cancellationToken, _logger);

        while (state.CompensationStack.Count > 0)
        {
            // Check for cancellation - but allow compensation to proceed in critical cases
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Compensation interrupted by cancellation request at {InstanceId}",
                    state.Id);
                break;
            }

            var activityId = state.CompensationStack.Pop();
            var activity = definition.GetActivity(activityId);

            if (activity?.CanCompensate == true)
            {
                try
                {
                    await activity.CompensateAsync(context);
                    await RecordExecutionAsync(state.Id, activityId, activity,
                        ExecutionRecordType.ActivityCompensated, null, null, null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Compensation failed for activity {ActivityId}", activityId);
                }
            }
        }

        state.Status = WorkflowStatus.Compensated;
        state.CompletedAt = DateTime.UtcNow;
    }

    private async Task RecordExecutionAsync(
        Guid instanceId,
        string activityId,
        IActivity activity,
        ExecutionRecordType type,
        TimeSpan? duration,
        IDictionary<string, object?>? output,
        ActivityError? error)
    {
        var record = new WorkflowExecutionRecord
        {
            Id = Guid.NewGuid(),
            InstanceId = instanceId,
            ActivityId = activityId,
            ActivityName = activity.Name,
            ActivityType = activity.Type,
            Type = type,
            Timestamp = DateTime.UtcNow,
            Duration = duration,
            Output = output,
            Error = error
        };

        await _instanceStore.AddHistoryAsync(record);
    }
}

/// <summary>
/// Workflow engine configuration options
/// </summary>
public class WorkflowEngineOptions
{
    /// <summary>
    /// Maximum number of activities to execute in a single execution run
    /// </summary>
    public int MaxActivitiesPerExecution { get; set; } = 10000;

    /// <summary>
    /// Default timeout for activity execution
    /// </summary>
    public TimeSpan DefaultActivityTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Whether to enable distributed locking for workflow execution
    /// </summary>
    public bool EnableDistributedLocking { get; set; } = true;
}

#region Exceptions

public class WorkflowException : Exception
{
    public WorkflowException(string message) : base(message) { }
    public WorkflowException(string message, Exception innerException) : base(message, innerException) { }
}

public class WorkflowNotFoundException : WorkflowException
{
    public WorkflowNotFoundException(string message) : base(message) { }
}

public class WorkflowValidationException : WorkflowException
{
    public WorkflowValidationException(string message) : base(message) { }
}

public class WorkflowInvalidStateException : WorkflowException
{
    public WorkflowInvalidStateException(string message) : base(message) { }
}

public class WorkflowBookmarkNotFoundException : WorkflowException
{
    public WorkflowBookmarkNotFoundException(string message) : base(message) { }
}

public class WorkflowActivityNotFoundException : WorkflowException
{
    public WorkflowActivityNotFoundException(string message) : base(message) { }
}

public class WorkflowExecutionException : WorkflowException
{
    public WorkflowExecutionException(string message) : base(message) { }
}

#endregion
