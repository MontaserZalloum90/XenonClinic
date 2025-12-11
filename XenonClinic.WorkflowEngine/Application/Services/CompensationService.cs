using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for managing compensation (rollback) handlers in workflows.
/// </summary>
public class CompensationService : ICompensationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExpressionEvaluator _expressionEvaluator;
    private readonly IAuditService _auditService;
    private readonly ILogger<CompensationService> _logger;

    // In-memory storage - replace with database in production
    private readonly ConcurrentDictionary<string, List<CompensationHandler>> _handlers = new();
    private readonly ConcurrentDictionary<string, CompensationState> _states = new();
    private readonly ConcurrentDictionary<string, List<CompensationRecord>> _history = new();
    private readonly ConcurrentDictionary<string, Saga> _sagas = new();

    public CompensationService(
        IHttpClientFactory httpClientFactory,
        IExpressionEvaluator expressionEvaluator,
        IAuditService auditService,
        ILogger<CompensationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _expressionEvaluator = expressionEvaluator;
        _auditService = auditService;
        _logger = logger;
    }

    public Task RegisterCompensationHandlerAsync(RegisterCompensationRequest request, CancellationToken cancellationToken = default)
    {
        var handler = new CompensationHandler
        {
            Id = Guid.NewGuid().ToString(),
            ProcessDefinitionId = request.ProcessDefinitionId,
            ActivityId = request.ActivityId,
            Type = request.Type,
            HandlerClass = request.HandlerClass,
            Script = request.Script,
            ScriptLanguage = request.ScriptLanguage,
            ServiceEndpoint = request.ServiceEndpoint,
            Configuration = request.Configuration,
            ExecutionOrder = request.ExecutionOrder,
            IsAsync = request.IsAsync,
            TimeoutSeconds = request.TimeoutSeconds,
            MaxRetries = request.MaxRetries,
            CreatedAt = DateTime.UtcNow
        };

        var key = $"{request.ProcessDefinitionId}:{request.ActivityId}";
        var handlers = _handlers.GetOrAdd(key, _ => new List<CompensationHandler>());
        lock (handlers)
        {
            handlers.Add(handler);
        }

        _logger.LogInformation("Registered compensation handler for activity {ActivityId} in process {ProcessDefinitionId}",
            request.ActivityId, request.ProcessDefinitionId);

        return Task.CompletedTask;
    }

    public async Task<CompensationExecution> TriggerCompensationAsync(TriggerCompensationRequest request, CancellationToken cancellationToken = default)
    {
        var execution = new CompensationExecution
        {
            Id = Guid.NewGuid().ToString(),
            ProcessInstanceId = request.ProcessInstanceId,
            Status = CompensationExecutionStatus.Running,
            Reason = request.Reason,
            TriggeredBy = request.TriggeredBy,
            StartedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Starting compensation for process instance {ProcessInstanceId}, reason: {Reason}",
            request.ProcessInstanceId, request.Reason);

        try
        {
            // Get compensation state
            var state = await GetOrCreateCompensationStateAsync(request.ProcessInstanceId, cancellationToken);
            state.IsCompensating = true;

            // Get compensable activities (handle null)
            var activitiesToCompensate = (state.CompensableActivities ?? Enumerable.Empty<CompensableActivity>())
                .Where(a => a != null && a.HasCompensationHandler && !a.IsCompensated)
                .ToList();

            // Filter from specific activity if requested
            if (!string.IsNullOrEmpty(request.FromActivityId))
            {
                var fromIndex = activitiesToCompensate.FindIndex(a => a.ActivityId == request.FromActivityId);
                if (fromIndex >= 0)
                {
                    activitiesToCompensate = activitiesToCompensate.Take(fromIndex + 1).ToList();
                }
            }

            // Order based on compensation mode
            activitiesToCompensate = request.Mode switch
            {
                CompensationMode.ReverseOrder => activitiesToCompensate.OrderByDescending(a => a.CompletedAt).ToList(),
                CompensationMode.DefinedOrder => activitiesToCompensate.ToList(), // Already in defined order
                _ => activitiesToCompensate
            };

            execution.TotalActivities = activitiesToCompensate.Count;

            // Execute compensations
            if (request.Mode == CompensationMode.ParallelAll)
            {
                var tasks = activitiesToCompensate.Select(a =>
                    CompensateActivityInternalAsync(execution, a, request.Variables ?? new Dictionary<string, object>(), cancellationToken));
                var results = await Task.WhenAll(tasks);

                // Process results
                foreach (var result in results)
                {
                    execution.Results.Add(result);
                    if (result.Success)
                    {
                        execution.CompensatedActivities++;
                    }
                    else
                    {
                        execution.FailedActivities++;
                    }
                }
            }
            else
            {
                foreach (var activity in activitiesToCompensate)
                {
                    var result = await CompensateActivityInternalAsync(execution, activity, request.Variables, cancellationToken);
                    execution.Results.Add(result);

                    if (!result.Success)
                    {
                        execution.FailedActivities++;
                        if (!request.ContinueOnError)
                        {
                            _logger.LogWarning("Compensation stopped due to failure at activity {ActivityId}",
                                activity.ActivityId);
                            break;
                        }
                    }
                    else
                    {
                        execution.CompensatedActivities++;
                        activity.IsCompensated = true;
                    }
                }
            }

            // Determine final status
            execution.Status = execution.FailedActivities > 0
                ? (execution.CompensatedActivities > 0
                    ? CompensationExecutionStatus.PartiallyCompleted
                    : CompensationExecutionStatus.Failed)
                : CompensationExecutionStatus.Completed;

            execution.CompletedAt = DateTime.UtcNow;
            state.IsCompensating = false;
            state.LastCompensationStatus = execution.Status;
            state.LastCompensationAt = DateTime.UtcNow;

            _logger.LogInformation("Compensation completed for process instance {ProcessInstanceId}: Status={Status}, Compensated={Compensated}, Failed={Failed}",
                request.ProcessInstanceId, execution.Status, execution.CompensatedActivities, execution.FailedActivities);

            // Log audit event
            await _auditService.LogAsync(new AuditLogRequest
            {
                EventType = "Compensation.Completed",
                EntityType = "ProcessInstance",
                EntityId = request.ProcessInstanceId,
                Description = $"Compensation {execution.Status}: {execution.CompensatedActivities} compensated, {execution.FailedActivities} failed",
                NewValues = new Dictionary<string, object>
                {
                    ["executionId"] = execution.Id,
                    ["status"] = execution.Status.ToString(),
                    ["reason"] = request.Reason ?? ""
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            execution.Status = CompensationExecutionStatus.Failed;
            execution.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Compensation failed for process instance {ProcessInstanceId}", request.ProcessInstanceId);
        }

        return execution;
    }

    public async Task<CompensationResult> CompensateActivityAsync(string processInstanceId, string activityInstanceId, CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateCompensationStateAsync(processInstanceId, cancellationToken);
        var activity = (state.CompensableActivities ?? Enumerable.Empty<CompensableActivity>())
            .FirstOrDefault(a => a?.ActivityInstanceId == activityInstanceId);

        if (activity == null)
        {
            return new CompensationResult
            {
                ActivityInstanceId = activityInstanceId,
                Success = false,
                ErrorCode = "ACTIVITY_NOT_FOUND",
                ErrorMessage = "Activity not found or not compensable"
            };
        }

        var execution = new CompensationExecution
        {
            ProcessInstanceId = processInstanceId,
            TotalActivities = 1
        };

        var result = await CompensateActivityInternalAsync(execution, activity, new Dictionary<string, object>(), cancellationToken);

        if (result.Success)
        {
            activity.IsCompensated = true;
        }

        return result;
    }

    public Task<CompensationState?> GetCompensationStateAsync(string processInstanceId, CancellationToken cancellationToken = default)
    {
        _states.TryGetValue(processInstanceId, out var state);
        return Task.FromResult(state);
    }

    public Task<IList<CompensationRecord>> GetCompensationHistoryAsync(string processInstanceId, CancellationToken cancellationToken = default)
    {
        if (_history.TryGetValue(processInstanceId, out var records))
        {
            // Lock when reading to avoid concurrent modification
            lock (records)
            {
                return Task.FromResult<IList<CompensationRecord>>(records.OrderByDescending(r => r.CompensatedAt).ToList());
            }
        }
        return Task.FromResult<IList<CompensationRecord>>(new List<CompensationRecord>());
    }

    #region Saga Methods

    public Task<Saga> CreateSagaAsync(CreateSagaRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var saga = new Saga
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name ?? "Unnamed Saga",
            ProcessInstanceId = request.ProcessInstanceId,
            Status = SagaStatus.Created,
            Context = request.Context ?? new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow
        };

        var steps = request.Steps ?? Enumerable.Empty<CreateSagaStepRequest>();
        foreach (var stepRequest in steps.Where(s => s != null).OrderBy(s => s.Order))
        {
            saga.Steps.Add(new SagaStep
            {
                Id = Guid.NewGuid().ToString(),
                Name = stepRequest.Name,
                Order = stepRequest.Order,
                Status = SagaStepStatus.Pending,
                Action = stepRequest.Action,
                Compensation = stepRequest.Compensation,
                InputData = stepRequest.InputData
            });
        }

        _sagas[saga.Id] = saga;

        _logger.LogInformation("Created saga {SagaId} with {StepCount} steps", saga.Id, saga.Steps.Count);

        return Task.FromResult(saga);
    }

    public async Task<SagaStepResult> ExecuteSagaStepAsync(string sagaId, string stepId, CancellationToken cancellationToken = default)
    {
        if (!_sagas.TryGetValue(sagaId, out var saga))
        {
            throw new InvalidOperationException($"Saga '{sagaId}' not found");
        }

        var step = saga.Steps.FirstOrDefault(s => s.Id == stepId);
        if (step == null)
        {
            throw new InvalidOperationException($"Step '{stepId}' not found in saga '{sagaId}'");
        }

        if (saga.Status == SagaStatus.Created)
        {
            saga.Status = SagaStatus.Running;
            saga.StartedAt = DateTime.UtcNow;
        }

        var result = new SagaStepResult
        {
            SagaId = sagaId,
            StepId = stepId
        };

        var sw = Stopwatch.StartNew();
        step.Status = SagaStepStatus.Running;

        try
        {
            _logger.LogDebug("Executing saga step {StepId} ({StepName})", stepId, step.Name);

            // Merge context with input data
            var inputData = new Dictionary<string, object>(saga.Context);
            foreach (var kvp in step.InputData)
            {
                inputData[kvp.Key] = kvp.Value;
            }

            // Execute the action
            var (success, output, error) = await ExecuteSagaActionAsync(step.Action, inputData, cancellationToken);

            sw.Stop();
            result.ExecutionTime = sw.Elapsed;
            result.Success = success;
            result.OutputData = output;

            if (success)
            {
                step.Status = SagaStepStatus.Completed;
                step.ExecutedAt = DateTime.UtcNow;
                step.OutputData = output;
                saga.CurrentStepIndex = saga.Steps.IndexOf(step);

                // Update context with output
                foreach (var kvp in output)
                {
                    saga.Context[kvp.Key] = kvp.Value;
                }

                // Check if saga is complete
                if (saga.Steps.All(s => s.Status == SagaStepStatus.Completed))
                {
                    saga.Status = SagaStatus.Completed;
                    saga.CompletedAt = DateTime.UtcNow;
                }

                result.NewStatus = step.Status;
            }
            else
            {
                step.Status = SagaStepStatus.Failed;
                step.ErrorMessage = error;
                saga.Status = SagaStatus.Failed;
                saga.FailedStepId = stepId;
                saga.FailureReason = error;

                result.ErrorMessage = error;
                result.NewStatus = step.Status;
            }

            _logger.LogInformation("Saga step {StepId} completed: Success={Success}", stepId, success);
        }
        catch (Exception ex)
        {
            sw.Stop();
            step.Status = SagaStepStatus.Failed;
            step.ErrorMessage = ex.Message;
            saga.Status = SagaStatus.Failed;
            saga.FailedStepId = stepId;
            saga.FailureReason = ex.Message;

            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ExecutionTime = sw.Elapsed;
            result.NewStatus = SagaStepStatus.Failed;

            _logger.LogError(ex, "Saga step {StepId} failed", stepId);
        }

        return result;
    }

    public async Task<SagaCompensationResult> CompensateSagaAsync(string sagaId, CancellationToken cancellationToken = default)
    {
        if (!_sagas.TryGetValue(sagaId, out var saga))
        {
            throw new InvalidOperationException($"Saga '{sagaId}' not found");
        }

        var result = new SagaCompensationResult
        {
            SagaId = sagaId
        };

        var sw = Stopwatch.StartNew();
        saga.Status = SagaStatus.Compensating;

        _logger.LogInformation("Starting saga compensation for {SagaId}", sagaId);

        // Get completed steps in reverse order
        var stepsToCompensate = saga.Steps
            .Where(s => s.Status == SagaStepStatus.Completed)
            .OrderByDescending(s => s.Order)
            .ToList();

        foreach (var step in stepsToCompensate)
        {
            try
            {
                step.Status = SagaStepStatus.Compensating;

                // Merge output data as input for compensation
                var inputData = new Dictionary<string, object>(saga.Context);
                foreach (var kvp in step.OutputData)
                {
                    inputData[kvp.Key] = kvp.Value;
                }

                var (success, output, error) = await ExecuteSagaActionAsync(step.Compensation, inputData, cancellationToken);

                if (success)
                {
                    step.Status = SagaStepStatus.Compensated;
                    step.CompensatedAt = DateTime.UtcNow;
                    result.CompensatedSteps++;
                }
                else
                {
                    step.Status = SagaStepStatus.Failed;
                    step.ErrorMessage = error;
                    result.FailedCompensations++;
                }

                result.StepResults.Add(new SagaStepResult
                {
                    SagaId = sagaId,
                    StepId = step.Id,
                    Success = success,
                    NewStatus = step.Status,
                    OutputData = output,
                    ErrorMessage = error
                });
            }
            catch (Exception ex)
            {
                step.Status = SagaStepStatus.Failed;
                step.ErrorMessage = ex.Message;
                result.FailedCompensations++;

                result.StepResults.Add(new SagaStepResult
                {
                    SagaId = sagaId,
                    StepId = step.Id,
                    Success = false,
                    NewStatus = SagaStepStatus.Failed,
                    ErrorMessage = ex.Message
                });

                _logger.LogError(ex, "Failed to compensate saga step {StepId}", step.Id);
            }
        }

        sw.Stop();
        result.TotalTime = sw.Elapsed;
        result.Success = result.FailedCompensations == 0;
        result.FinalStatus = result.Success ? SagaStatus.Compensated : SagaStatus.Failed;
        saga.Status = result.FinalStatus;
        saga.CompletedAt = DateTime.UtcNow;

        _logger.LogInformation("Saga compensation completed for {SagaId}: Compensated={Compensated}, Failed={Failed}",
            sagaId, result.CompensatedSteps, result.FailedCompensations);

        return result;
    }

    #endregion

    #region Private Methods

    private Task<CompensationState> GetOrCreateCompensationStateAsync(string processInstanceId, CancellationToken cancellationToken)
    {
        var state = _states.GetOrAdd(processInstanceId, id => new CompensationState
        {
            ProcessInstanceId = id,
            CompensableActivities = new List<CompensableActivity>()
        });
        return Task.FromResult(state);
    }

    private async Task<CompensationResult> CompensateActivityInternalAsync(
        CompensationExecution execution,
        CompensableActivity activity,
        Dictionary<string, object> variables,
        CancellationToken cancellationToken)
    {
        var result = new CompensationResult
        {
            ActivityInstanceId = activity.ActivityInstanceId,
            ActivityId = activity.ActivityId,
            ActivityName = activity.ActivityName,
            ExecutedAt = DateTime.UtcNow
        };

        var sw = Stopwatch.StartNew();

        try
        {
            // Find compensation handler
            var key = $"*:{activity.ActivityId}"; // Would match with actual process definition ID
            var handlers = _handlers.Values
                .SelectMany(h => h)
                .Where(h => h.ActivityId == activity.ActivityId)
                .OrderBy(h => h.ExecutionOrder)
                .ToList();

            if (!handlers.Any())
            {
                _logger.LogWarning("No compensation handler found for activity {ActivityId}", activity.ActivityId);
                result.Success = true; // No handler means nothing to compensate
                result.OutputVariables = new Dictionary<string, object>();
            }
            else
            {
                foreach (var handler in handlers)
                {
                    await ExecuteCompensationHandlerAsync(handler, activity, variables, cancellationToken);
                }
                result.Success = true;
            }

            // Record in history
            var record = new CompensationRecord
            {
                Id = Guid.NewGuid().ToString(),
                ProcessInstanceId = execution.ProcessInstanceId,
                ExecutionId = execution.Id,
                ActivityInstanceId = activity.ActivityInstanceId,
                ActivityId = activity.ActivityId,
                ActivityName = activity.ActivityName,
                Success = result.Success,
                CompensatedAt = DateTime.UtcNow,
                InputData = activity.ExecutionData,
                OutputData = result.OutputVariables
            };

            var records = _history.GetOrAdd(execution.ProcessInstanceId, _ => new List<CompensationRecord>());
            lock (records)
            {
                records.Add(record);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorCode = "COMPENSATION_FAILED";
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Compensation failed for activity {ActivityId}", activity.ActivityId);
        }

        sw.Stop();
        result.ExecutionTime = sw.Elapsed;
        return result;
    }

    private async Task ExecuteCompensationHandlerAsync(
        CompensationHandler handler,
        CompensableActivity activity,
        Dictionary<string, object> variables,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Executing compensation handler {HandlerId} for activity {ActivityId}",
            handler.Id, handler.ActivityId);

        switch (handler.Type)
        {
            case CompensationType.Script:
                await ExecuteScriptCompensationAsync(handler, activity, variables, cancellationToken);
                break;

            case CompensationType.ServiceCall:
                await ExecuteServiceCallCompensationAsync(handler, activity, variables, cancellationToken);
                break;

            case CompensationType.Handler:
                // Would instantiate and execute handler class
                _logger.LogInformation("Would execute handler class: {HandlerClass}", handler.HandlerClass);
                break;

            case CompensationType.Manual:
                _logger.LogInformation("Manual compensation required for activity {ActivityId}", handler.ActivityId);
                break;
        }
    }

    private async Task ExecuteScriptCompensationAsync(
        CompensationHandler handler,
        CompensableActivity activity,
        Dictionary<string, object> variables,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(handler.Script))
            return;

        var context = new Dictionary<string, object>(variables);
        context["activity"] = activity;
        context["executionData"] = activity.ExecutionData;

        await _expressionEvaluator.EvaluateAsync(handler.Script, context);
    }

    private async Task ExecuteServiceCallCompensationAsync(
        CompensationHandler handler,
        CompensableActivity activity,
        Dictionary<string, object> variables,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(handler.ServiceEndpoint))
            return;

        var client = _httpClientFactory.CreateClient("CompensationService");
        client.Timeout = TimeSpan.FromSeconds(handler.TimeoutSeconds);

        var payload = new
        {
            activityId = activity.ActivityId,
            activityInstanceId = activity.ActivityInstanceId,
            executionData = activity.ExecutionData,
            variables
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(handler.ServiceEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<(bool success, Dictionary<string, object> output, string? error)> ExecuteSagaActionAsync(
        SagaAction action,
        Dictionary<string, object> inputData,
        CancellationToken cancellationToken)
    {
        var output = new Dictionary<string, object>();

        try
        {
            switch (action.Type)
            {
                case SagaActionType.ServiceCall:
                    if (string.IsNullOrEmpty(action.ServiceEndpoint))
                    {
                        return (true, output, null);
                    }

                    var client = _httpClientFactory.CreateClient("SagaService");
                    client.Timeout = TimeSpan.FromSeconds(action.TimeoutSeconds);

                    var method = action.HttpMethod?.ToUpperInvariant() switch
                    {
                        "GET" => HttpMethod.Get,
                        "PUT" => HttpMethod.Put,
                        "DELETE" => HttpMethod.Delete,
                        _ => HttpMethod.Post
                    };

                    using (var request = new HttpRequestMessage(method, action.ServiceEndpoint))
                    {
                        foreach (var header in action.Headers)
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }

                        if (method != HttpMethod.Get)
                        {
                            request.Content = new StringContent(
                                JsonSerializer.Serialize(inputData),
                                Encoding.UTF8,
                                "application/json");
                        }

                        var response = await client.SendAsync(request, cancellationToken);

                        if (!response.IsSuccessStatusCode)
                        {
                            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                            return (false, output, $"HTTP {(int)response.StatusCode}: {errorContent}");
                        }

                        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            try
                            {
                                output = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)
                                    ?? new Dictionary<string, object>();
                            }
                            catch
                            {
                                output["response"] = responseContent;
                            }
                        }
                    }

                    break;

                case SagaActionType.Script:
                    if (!string.IsNullOrEmpty(action.Script))
                    {
                        var result = await _expressionEvaluator.EvaluateAsync(action.Script, inputData);
                        if (result is Dictionary<string, object> dictResult)
                        {
                            output = dictResult;
                        }
                        else if (result != null)
                        {
                            output["result"] = result;
                        }
                    }
                    break;

                case SagaActionType.Handler:
                    // Would instantiate and execute handler type
                    _logger.LogInformation("Would execute saga handler: {HandlerType}", action.HandlerType);
                    break;
            }

            // Evaluate success condition if provided
            if (!string.IsNullOrEmpty(action.SuccessCondition))
            {
                var context = new Dictionary<string, object>(inputData);
                foreach (var kvp in output)
                {
                    context[kvp.Key] = kvp.Value;
                }

                var conditionResult = await _expressionEvaluator.EvaluateAsync(action.SuccessCondition, context);
                if (conditionResult is bool boolResult && !boolResult)
                {
                    return (false, output, "Success condition not met");
                }
            }

            return (true, output, null);
        }
        catch (Exception ex)
        {
            return (false, output, ex.Message);
        }
    }

    #endregion

    private class CompensationHandler
    {
        public string Id { get; set; } = string.Empty;
        public string ProcessDefinitionId { get; set; } = string.Empty;
        public string ActivityId { get; set; } = string.Empty;
        public CompensationType Type { get; set; }
        public string? HandlerClass { get; set; }
        public string? Script { get; set; }
        public string? ScriptLanguage { get; set; }
        public string? ServiceEndpoint { get; set; }
        public Dictionary<string, string> Configuration { get; set; } = new();
        public int ExecutionOrder { get; set; }
        public bool IsAsync { get; set; }
        public int TimeoutSeconds { get; set; }
        public int MaxRetries { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
