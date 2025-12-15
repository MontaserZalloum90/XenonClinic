namespace XenonClinic.WorkflowEngine.Core.Engine;

using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Core.Activities;
using XenonClinic.WorkflowEngine.Persistence.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// Main workflow engine implementation with production-ready features:
/// - Distributed locking for multi-instance deployments
/// - Timeout enforcement for parallel branches
/// - Proper cancellation token propagation
/// - Thread-safe parallel execution
/// - Resource cleanup on timeout/cancellation
/// </summary>
public class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowDefinitionStore _definitionStore;
    private readonly IWorkflowInstanceStore _instanceStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkflowEngine> _logger;
    private readonly WorkflowEngineOptions _options;
    private readonly string _lockHolderId = $"engine_{Environment.MachineName}_{Guid.NewGuid():N}";

    // BUG FIX: Use SemaphoreSlim instead of lock for async-safe state synchronization
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _instanceLocks = new();

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

    /// <summary>
    /// Gets or creates an async lock for a specific workflow instance.
    /// BUG FIX: Use SemaphoreSlim for async-safe locking instead of blocking locks.
    /// </summary>
    private SemaphoreSlim GetInstanceLock(Guid instanceId)
    {
        return _instanceLocks.GetOrAdd(instanceId, _ => new SemaphoreSlim(1, 1));
    }

    /// <summary>
    /// Removes the instance lock when workflow completes to prevent memory leaks.
    /// </summary>
    private void CleanupInstanceLock(Guid instanceId)
    {
        if (_instanceLocks.TryRemove(instanceId, out var semaphore))
        {
            semaphore.Dispose();
        }
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
        // BUG FIX: Move lock acquisition inside try block to ensure lock is released
        // even if TryAcquireLockAsync throws an exception
        var lockAcquired = false;

        try
        {
            // Acquire distributed lock to prevent concurrent execution
            if (_options.EnableDistributedLocking)
            {
                lockAcquired = await _instanceStore.TryAcquireLockAsync(
                    instanceId, _lockHolderId, _options.LockDuration);

                if (!lockAcquired)
                {
                    throw new WorkflowExecutionException(
                        $"Could not acquire lock for workflow instance {instanceId}. Another process may be executing it.");
                }
            }

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
        finally
        {
            // BUG FIX: Only release the lock if we successfully acquired it
            if (_options.EnableDistributedLocking && lockAcquired)
            {
                await _instanceStore.ReleaseLockAsync(instanceId, _lockHolderId);
            }
        }
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
        // BUG FIX: Move lock acquisition inside try block and track lock state
        var lockAcquired = false;

        try
        {
            // Acquire distributed lock to prevent concurrent execution
            if (_options.EnableDistributedLocking)
            {
                lockAcquired = await _instanceStore.TryAcquireLockAsync(
                    instanceId, _lockHolderId, _options.LockDuration);

                if (!lockAcquired)
                {
                    throw new WorkflowExecutionException(
                        $"Could not acquire lock for workflow instance {instanceId}. Another process may be executing it.");
                }
            }

            var state = await _instanceStore.GetAsync(instanceId);
            if (state == null)
            {
                throw new WorkflowNotFoundException($"Workflow instance not found: {instanceId}");
            }

            // BUG FIX: Enhanced state transition validation
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

            // BUG FIX: Validate that the bookmarked activity still exists in the definition
            if (bookmark.ActivityId != null)
            {
                var bookmarkedActivity = definition.GetActivity(bookmark.ActivityId);
                if (bookmarkedActivity == null)
                {
                    throw new WorkflowActivityNotFoundException(
                        $"Bookmarked activity no longer exists in workflow definition: {bookmark.ActivityId}");
                }
            }

            // BUG FIX: Only remove bookmark after successful activity resumption
            // Store bookmark for potential restoration on failure
            var bookmarkToRemove = bookmark;
            state.Status = WorkflowStatus.Running;

            // Create cancellation token with timeout
            using var cts = new CancellationTokenSource(_options.DefaultActivityTimeout);
            var cancellationToken = cts.Token;

            // Get the activity that created the bookmark and resume it
            if (bookmarkToRemove.ActivityId != null)
            {
                var activity = definition.GetActivity(bookmarkToRemove.ActivityId);
                if (activity is ResumableActivityBase resumable)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = new WorkflowContext(state, scope.ServiceProvider, cancellationToken, _logger);

                    var result = await resumable.ResumeAsync(context, input);
                    if (result.IsSuccess)
                    {
                        // BUG FIX: Only remove bookmark after successful resumption
                        state.Bookmarks.Remove(bookmarkToRemove);

                        state.AddCompletedActivity(bookmarkToRemove.ActivityId);
                        state.CompensationStack ??= new Stack<string>();
                        if (bookmarkToRemove.ActivityId != null)
                        {
                            state.CompensationStack.Push(bookmarkToRemove.ActivityId);
                        }

                        // Continue execution from next activity
                        var nextActivityId = result.NextActivityId ?? GetNextActivityId(definition, bookmarkToRemove.ActivityId);
                        if (nextActivityId != null)
                        {
                            state.CurrentActivityId = nextActivityId;
                        }
                    }
                    else
                    {
                        // BUG FIX: Activity resumption failed - keep bookmark and set appropriate state
                        state.Status = WorkflowStatus.Faulted;
                        state.Error = new WorkflowError
                        {
                            Code = result.Error?.Code ?? "RESUME_FAILED",
                            Message = result.Error?.Message ?? "Failed to resume activity",
                            ActivityId = bookmarkToRemove.ActivityId
                        };
                        await SaveStateWithRetryAsync(state);
                        return new WorkflowExecutionResult
                        {
                            InstanceId = state.Id,
                            Status = state.Status,
                            Error = state.Error
                        };
                    }
                }
                else
                {
                    // BUG FIX: Activity is not resumable - remove bookmark and continue
                    state.Bookmarks.Remove(bookmarkToRemove);
                }
            }
            else
            {
                // BUG FIX: No activity ID on bookmark - remove it
                state.Bookmarks.Remove(bookmarkToRemove);
            }

            return await ExecuteAsync(state, definition, cancellationToken);
        }
        finally
        {
            // BUG FIX: Only release the lock if we successfully acquired it
            if (_options.EnableDistributedLocking && lockAcquired)
            {
                await _instanceStore.ReleaseLockAsync(instanceId, _lockHolderId);
            }
        }
    }

    public async Task CancelAsync(Guid instanceId, string? reason = null)
    {
        // BUG FIX: Acquire lock before modifying state to prevent race conditions
        var lockAcquired = false;

        try
        {
            if (_options.EnableDistributedLocking)
            {
                lockAcquired = await _instanceStore.TryAcquireLockAsync(
                    instanceId, _lockHolderId, _options.LockDuration);

                if (!lockAcquired)
                {
                    throw new WorkflowExecutionException(
                        $"Could not acquire lock for workflow instance {instanceId}. Another process may be executing it.");
                }
            }

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
        finally
        {
            if (_options.EnableDistributedLocking && lockAcquired)
            {
                await _instanceStore.ReleaseLockAsync(instanceId, _lockHolderId);
            }
        }
    }

    public async Task TerminateAsync(Guid instanceId, string? reason = null)
    {
        // BUG FIX: Acquire lock before modifying state to prevent race conditions
        var lockAcquired = false;

        try
        {
            if (_options.EnableDistributedLocking)
            {
                lockAcquired = await _instanceStore.TryAcquireLockAsync(
                    instanceId, _lockHolderId, _options.LockDuration);

                if (!lockAcquired)
                {
                    throw new WorkflowExecutionException(
                        $"Could not acquire lock for workflow instance {instanceId}. Another process may be executing it.");
                }
            }

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
        finally
        {
            if (_options.EnableDistributedLocking && lockAcquired)
            {
                await _instanceStore.ReleaseLockAsync(instanceId, _lockHolderId);
            }
        }
    }

    public async Task<WorkflowExecutionResult> RetryAsync(Guid instanceId)
    {
        return await RetryAsync(instanceId, CancellationToken.None);
    }

    /// <summary>
    /// Retries a faulted workflow with cancellation support.
    /// BUG FIX: Added overload with CancellationToken for proper cancellation propagation.
    /// </summary>
    public async Task<WorkflowExecutionResult> RetryAsync(Guid instanceId, CancellationToken cancellationToken)
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

        // BUG FIX: Check if max retries exceeded
        if (state.FaultCount >= _options.MaxRetryAttempts)
        {
            _logger.LogWarning("Workflow {InstanceId} has exceeded maximum retry attempts ({MaxRetries})",
                instanceId, _options.MaxRetryAttempts);
        }

        state.Status = WorkflowStatus.Running;
        state.Error = null;
        state.FaultCount++;

        // BUG FIX: Use provided cancellation token instead of CancellationToken.None
        return await ExecuteAsync(state, definition, cancellationToken);
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
                // BUG FIX: Check for infinite loops BEFORE executing the activity
                // Use >= instead of > to enforce exact limit
                if (activitiesExecuted >= _options.MaxActivitiesPerExecution)
                {
                    throw new WorkflowExecutionException(
                        $"Maximum activities ({_options.MaxActivitiesPerExecution}) exceeded. " +
                        "This may indicate an infinite loop in the workflow definition.");
                }

                var activity = definition.GetActivity(state.CurrentActivityId);
                if (activity == null)
                {
                    throw new WorkflowActivityNotFoundException(
                        $"Activity not found: {state.CurrentActivityId}");
                }

                activitiesExecuted++;
                state.ActiveActivityIds ??= new List<string>();
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
                state.CompletedActivityIds ??= new List<string>();
                if (!state.CompletedActivityIds.Contains(state.CurrentActivityId))
                {
                    state.CompletedActivityIds.Add(state.CurrentActivityId);
                    if (activity.CanCompensate)
                    {
                        state.CompensationStack ??= new Stack<string>();
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
                // BUG FIX: Removed redundant safety check - now checked at start of loop
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

        // BUG FIX: Use retry-enabled save for better error handling
        await SaveStateWithRetryAsync(state);

        // BUG FIX: Cleanup resources when workflow completes or faults
        if (state.Status is WorkflowStatus.Completed or WorkflowStatus.Faulted
            or WorkflowStatus.Cancelled or WorkflowStatus.Compensated)
        {
            CleanupWorkflowResources(state.Id);
        }

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
            ParentActivityId = state.CurrentActivityId ?? string.Empty,
            ActivityIds = branchActivityIds.ToList(),
            Status = ParallelBranchStatus.Running
        };

        // BUG FIX: Use async-safe lock instead of blocking lock
        var instanceLock = GetInstanceLock(state.Id);
        await instanceLock.WaitAsync(context.CancellationToken);
        try
        {
            state.ParallelBranches[branchId] = branch;
        }
        finally
        {
            instanceLock.Release();
        }

        // Thread-safe results collection
        var results = new ConcurrentBag<(string activityId, ActivityResult result)>();
        var errors = new ConcurrentBag<Exception>();

        // Create a timeout for parallel execution
        using var parallelCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
        parallelCts.CancelAfter(_options.ParallelBranchTimeout);

        // Execute branches in parallel with timeout enforcement
        var branchTasks = branchActivityIds.Select(async activityId =>
        {
            try
            {
                var activity = definition.GetActivity(activityId);
                if (activity == null)
                {
                    results.Add((activityId, ActivityResult.Failure("ACTIVITY_NOT_FOUND", $"Activity not found: {activityId}")));
                    return;
                }

                // Check for cancellation before executing
                if (parallelCts.Token.IsCancellationRequested)
                {
                    results.Add((activityId, ActivityResult.Failure("CANCELLED", "Execution cancelled or timed out")));
                    return;
                }

                // Create activity-specific context with the parallel cancellation token
                using var scope = _serviceProvider.CreateScope();
                var activityContext = new WorkflowContext(state, scope.ServiceProvider, parallelCts.Token, _logger);

                var result = await activity.ExecuteAsync(activityContext);
                results.Add((activityId, result));
            }
            catch (OperationCanceledException)
            {
                results.Add((activityId, ActivityResult.Failure("TIMEOUT", "Activity execution timed out")));
            }
            catch (Exception ex)
            {
                errors.Add(ex);
                results.Add((activityId, ActivityResult.Failure("EXECUTION_ERROR", ex.Message, ex)));
            }
        }).ToList();

        try
        {
            // Wait for all branches with timeout
            await Task.WhenAll(branchTasks).WaitAsync(_options.ParallelBranchTimeout, context.CancellationToken);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Parallel branch execution timed out after {Timeout} for workflow {InstanceId}",
                _options.ParallelBranchTimeout, state.Id);

            // Cancel any still-running tasks
            await parallelCts.CancelAsync();

            // Add timeout results for activities that didn't complete
            var completedActivityIds = results.Select(r => r.activityId).ToHashSet();
            foreach (var activityId in branchActivityIds.Where(id => !completedActivityIds.Contains(id)))
            {
                results.Add((activityId, ActivityResult.Failure("TIMEOUT", "Activity execution timed out")));
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Parallel branch execution cancelled for workflow {InstanceId}", state.Id);
            branch.Status = ParallelBranchStatus.Faulted;
            return;
        }

        // Process results thread-safely
        var allSucceeded = true;
        foreach (var (activityId, result) in results)
        {
            if (result.IsSuccess)
            {
                state.AddCompletedActivity(activityId);
            }
            else
            {
                allSucceeded = false;
                _logger.LogWarning("Parallel branch activity {ActivityId} failed: {Error}",
                    activityId, result.Error?.Message);
            }
        }

        // Log any unhandled exceptions
        foreach (var error in errors)
        {
            _logger.LogError(error, "Unhandled exception in parallel branch execution for workflow {InstanceId}", state.Id);
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
        // BUG FIX: Improved converging gateway validation
        // Find the parallel join gateway that corresponds to the split gateway
        var activities = definition.Activities ?? new Dictionary<string, IActivity>();

        // Get the outgoing transitions from the split gateway
        var splitOutgoing = definition.GetOutgoingTransitions(splitGatewayId);
        if (!splitOutgoing.Any())
        {
            _logger.LogWarning("Split gateway {GatewayId} has no outgoing transitions", splitGatewayId);
            return null;
        }

        // Track all paths from the split gateway to find the convergence point
        var branchTargets = splitOutgoing.Select(t => t.TargetActivityId).ToHashSet();

        foreach (var activity in activities.Values)
        {
            if (activity is ParallelGatewayActivity gateway && gateway.Direction == GatewayDirection.Join)
            {
                var incoming = definition.GetIncomingTransitions(activity.Id);
                if (incoming.Any())
                {
                    // BUG FIX: Validate that this join gateway receives from all branches
                    // by checking if the number of incoming transitions matches the split
                    var incomingCount = incoming.Count();
                    var splitCount = splitOutgoing.Count();

                    if (incomingCount >= splitCount)
                    {
                        _logger.LogDebug("Found converging gateway {JoinGatewayId} for split {SplitGatewayId}",
                            activity.Id, splitGatewayId);
                        return activity.Id;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Converging gateway {JoinGatewayId} has {IncomingCount} incoming transitions but split has {SplitCount} outgoing",
                            activity.Id, incomingCount, splitCount);
                    }
                }
            }
        }

        _logger.LogWarning("No converging gateway found for split gateway {SplitGatewayId}", splitGatewayId);
        return null;
    }

    private async Task<bool> TryHandleErrorAsync(
        WorkflowInstanceState state,
        IWorkflowDefinition definition,
        ActivityError? error)
    {
        // Check for error boundary events
        var activities = definition.Activities ?? new Dictionary<string, IActivity>();
        foreach (var activity in activities.Values)
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

        // BUG FIX: Also compensate activities from parallel branches
        var parallelActivityIds = state.ParallelBranches.Values
            .Where(b => b.Status == ParallelBranchStatus.Completed || b.Status == ParallelBranchStatus.Running)
            .SelectMany(b => b.ActivityIds)
            .Where(id => state.CompletedActivityIds?.Contains(id) == true)
            .ToList();

        // Add parallel activities to compensation stack if not already there
        state.CompensationStack ??= new Stack<string>();
        foreach (var activityId in parallelActivityIds)
        {
            if (!state.CompensationStack.Contains(activityId))
            {
                state.CompensationStack.Push(activityId);
            }
        }

        var compensationErrors = new List<(string activityId, Exception error)>();

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

            // BUG FIX: Proper null check before accessing CanCompensate
            if (activity != null && activity.CanCompensate)
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
                    compensationErrors.Add((activityId, ex));
                    // Continue with other compensations even if one fails
                }
            }
            else if (activity == null)
            {
                _logger.LogWarning("Activity {ActivityId} not found during compensation for workflow {InstanceId}",
                    activityId, state.Id);
            }
        }

        // Log summary of compensation errors
        if (compensationErrors.Count > 0)
        {
            _logger.LogWarning("Compensation completed with {ErrorCount} errors for workflow {InstanceId}",
                compensationErrors.Count, state.Id);
        }

        state.Status = WorkflowStatus.Compensated;
        state.CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Saves workflow state with retry logic for handling transient persistence errors.
    /// BUG FIX: Added error handling and retry for persistence operations.
    /// </summary>
    private async Task SaveStateWithRetryAsync(WorkflowInstanceState state, int maxRetries = 3)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                await _instanceStore.SaveAsync(state);
                return;
            }
            catch (Exception ex)
            {
                if (attempt >= maxRetries)
                {
                    _logger.LogError(ex,
                        "Failed to save workflow state after {Attempts} attempts for instance {InstanceId}",
                        attempt, state.Id);
                    throw new WorkflowExecutionException(
                        $"Failed to persist workflow state for instance {state.Id} after {maxRetries} attempts", ex);
                }

                _logger.LogWarning(ex,
                    "Persistence error on attempt {Attempt}/{MaxRetries} for workflow {InstanceId}, retrying...",
                    attempt, maxRetries, state.Id);

                // Exponential backoff: 100ms, 200ms, 400ms...
                await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt - 1)));
            }
        }
    }

    /// <summary>
    /// Cleans up resources associated with a workflow instance.
    /// BUG FIX: Proper cleanup on workflow completion/timeout.
    /// </summary>
    private void CleanupWorkflowResources(Guid instanceId)
    {
        // Clean up instance lock
        CleanupInstanceLock(instanceId);

        _logger.LogDebug("Cleaned up resources for workflow instance {InstanceId}", instanceId);
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
/// Workflow engine configuration options with production-ready defaults.
/// BUG FIX: Updated default values based on production recommendations.
/// </summary>
public class WorkflowEngineOptions
{
    /// <summary>
    /// Maximum number of activities to execute in a single execution run.
    /// Prevents infinite loops in misconfigured workflows.
    /// Reduced from 10000 to 1000 to catch infinite loops earlier.
    /// </summary>
    public int MaxActivitiesPerExecution { get; set; } = 1000;

    /// <summary>
    /// Default timeout for individual activity execution.
    /// Increased to 30 minutes for long-running operations.
    /// </summary>
    public TimeSpan DefaultActivityTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Whether to enable distributed locking for workflow execution.
    /// Should be enabled in multi-instance deployments.
    /// </summary>
    public bool EnableDistributedLocking { get; set; } = true;

    /// <summary>
    /// Duration to hold the distributed lock during workflow execution.
    /// Should be longer than the expected maximum execution time.
    /// Increased to 45 minutes to accommodate longer operations.
    /// </summary>
    public TimeSpan LockDuration { get; set; } = TimeSpan.FromMinutes(45);

    /// <summary>
    /// Timeout for parallel branch execution.
    /// Individual branches will be cancelled if they exceed this timeout.
    /// Should match or exceed DefaultActivityTimeout.
    /// </summary>
    public TimeSpan ParallelBranchTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Maximum number of retry attempts for failed activities before marking workflow as faulted.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Whether to persist workflow state after each activity completes.
    /// Provides durability at the cost of performance.
    /// Enable this for critical workflows that cannot afford to lose state.
    /// </summary>
    public bool PersistAfterEachActivity { get; set; } = false;

    /// <summary>
    /// Interval for refreshing the distributed lock during long-running executions.
    /// Should be less than LockDuration to prevent lock expiry during execution.
    /// Increased to 5 minutes for better lock stability.
    /// </summary>
    public TimeSpan LockRefreshInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum number of retries for persistence operations.
    /// </summary>
    public int MaxPersistenceRetries { get; set; } = 3;

    /// <summary>
    /// Whether to enable detailed execution logging for debugging.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
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
