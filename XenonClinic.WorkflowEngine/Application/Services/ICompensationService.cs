using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for managing compensation (rollback) handlers in workflows.
/// </summary>
public interface ICompensationService
{
    /// <summary>
    /// Registers a compensation handler for an activity.
    /// </summary>
    Task RegisterCompensationHandlerAsync(RegisterCompensationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers compensation for a process instance.
    /// </summary>
    Task<CompensationExecution> TriggerCompensationAsync(TriggerCompensationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers compensation for a specific activity.
    /// </summary>
    Task<CompensationResult> CompensateActivityAsync(string processInstanceId, string activityInstanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the compensation state for a process instance.
    /// </summary>
    Task<CompensationState?> GetCompensationStateAsync(string processInstanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets compensation history for a process instance.
    /// </summary>
    Task<IList<CompensationRecord>> GetCompensationHistoryAsync(string processInstanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a saga for distributed transaction compensation.
    /// </summary>
    Task<Saga> CreateSagaAsync(CreateSagaRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a saga step.
    /// </summary>
    Task<SagaStepResult> ExecuteSagaStepAsync(string sagaId, string stepId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers saga compensation (rollback all completed steps).
    /// </summary>
    Task<SagaCompensationResult> CompensateSagaAsync(string sagaId, CancellationToken cancellationToken = default);
}

#region Compensation DTOs

public class RegisterCompensationRequest
{
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public CompensationType Type { get; set; } = CompensationType.Script;
    public string? HandlerClass { get; set; }
    public string? Script { get; set; }
    public string? ScriptLanguage { get; set; } = "javascript";
    public string? ServiceEndpoint { get; set; }
    public Dictionary<string, string> Configuration { get; set; } = new();
    public int ExecutionOrder { get; set; } = 0;
    public bool IsAsync { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 300;
    public int MaxRetries { get; set; } = 3;
}

public enum CompensationType
{
    Script,
    ServiceCall,
    Handler,
    SubProcess,
    Manual
}

public class TriggerCompensationRequest
{
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string? FromActivityId { get; set; } // Start compensation from this activity
    public string? Reason { get; set; }
    public CompensationMode Mode { get; set; } = CompensationMode.ReverseOrder;
    public bool ContinueOnError { get; set; } = false;
    public Dictionary<string, object> Variables { get; set; } = new();
    public string? TriggeredBy { get; set; }
}

public enum CompensationMode
{
    ReverseOrder,      // Compensate in reverse order of execution
    ParallelAll,       // Compensate all in parallel
    DefinedOrder,      // Use explicit order defined in handlers
    SelectiveOnly      // Only compensate explicitly selected activities
}

public class CompensationExecution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProcessInstanceId { get; set; } = string.Empty;
    public CompensationExecutionStatus Status { get; set; } = CompensationExecutionStatus.Running;

    // Progress
    public int TotalActivities { get; set; }
    public int CompensatedActivities { get; set; }
    public int FailedActivities { get; set; }
    public int SkippedActivities { get; set; }

    // Results
    public List<CompensationResult> Results { get; set; } = new();

    // Timing
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

    // Metadata
    public string? Reason { get; set; }
    public string? TriggeredBy { get; set; }
}

public enum CompensationExecutionStatus
{
    Running,
    Completed,
    PartiallyCompleted,
    Failed,
    Cancelled
}

public class CompensationResult
{
    public string ActivityInstanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ExecutionTime { get; set; }
    public int RetryCount { get; set; }
    public Dictionary<string, object> OutputVariables { get; set; } = new();
}

public class CompensationState
{
    public string ProcessInstanceId { get; set; } = string.Empty;
    public bool IsCompensating { get; set; }
    public CompensationExecutionStatus? LastCompensationStatus { get; set; }
    public DateTime? LastCompensationAt { get; set; }
    public List<CompensableActivity> CompensableActivities { get; set; } = new();
    public List<CompensationExecution> PendingCompensations { get; set; } = new();
}

public class CompensableActivity
{
    public string ActivityInstanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public bool HasCompensationHandler { get; set; }
    public bool IsCompensated { get; set; }
    public Dictionary<string, object> ExecutionData { get; set; } = new(); // Data needed for compensation
}

public class CompensationRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ExecutionId { get; set; } = string.Empty;
    public string ActivityInstanceId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CompensatedAt { get; set; } = DateTime.UtcNow;
    public string? CompensatedBy { get; set; }
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> OutputData { get; set; } = new();
}

#endregion

#region Saga DTOs

public class Saga
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? ProcessInstanceId { get; set; }
    public SagaStatus Status { get; set; } = SagaStatus.Created;
    public List<SagaStep> Steps { get; set; } = new();
    public int CurrentStepIndex { get; set; } = -1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailedStepId { get; set; }
    public string? FailureReason { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

public enum SagaStatus
{
    Created,
    Running,
    Completed,
    Compensating,
    Compensated,
    Failed
}

public class SagaStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public SagaStepStatus Status { get; set; } = SagaStepStatus.Pending;

    // Forward action
    public SagaAction Action { get; set; } = new();

    // Compensation action
    public SagaAction Compensation { get; set; } = new();

    // Execution data
    public DateTime? ExecutedAt { get; set; }
    public DateTime? CompensatedAt { get; set; }
    public Dictionary<string, object> InputData { get; set; } = new();
    public Dictionary<string, object> OutputData { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}

public enum SagaStepStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Compensating,
    Compensated,
    Skipped
}

public class SagaAction
{
    public SagaActionType Type { get; set; } = SagaActionType.ServiceCall;
    public string? ServiceEndpoint { get; set; }
    public string? HandlerType { get; set; }
    public string? Script { get; set; }
    public string? HttpMethod { get; set; } = "POST";
    public Dictionary<string, string> Headers { get; set; } = new();
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public string? SuccessCondition { get; set; } // Expression to determine success
}

public enum SagaActionType
{
    ServiceCall,
    Script,
    Handler,
    Message
}

public class CreateSagaRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ProcessInstanceId { get; set; }
    public List<CreateSagaStepRequest> Steps { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
}

public class CreateSagaStepRequest
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public SagaAction Action { get; set; } = new();
    public SagaAction Compensation { get; set; } = new();
    public Dictionary<string, object> InputData { get; set; } = new();
}

public class SagaStepResult
{
    public string SagaId { get; set; } = string.Empty;
    public string StepId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public SagaStepStatus NewStatus { get; set; }
    public Dictionary<string, object> OutputData { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}

public class SagaCompensationResult
{
    public string SagaId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public SagaStatus FinalStatus { get; set; }
    public List<SagaStepResult> StepResults { get; set; } = new();
    public int CompensatedSteps { get; set; }
    public int FailedCompensations { get; set; }
    public TimeSpan TotalTime { get; set; }
}

#endregion
