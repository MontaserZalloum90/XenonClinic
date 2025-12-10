namespace XenonClinic.WorkflowEngine.Core.StateMachine;

using Microsoft.Extensions.Logging;

/// <summary>
/// Executes state machine transitions with guard evaluation and action execution.
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
public interface IStateMachineExecutor<TState> where TState : notnull
{
    /// <summary>
    /// Executes a state transition
    /// </summary>
    Task<StateTransitionResult<TState>> TransitionAsync<TEntity>(
        IStateMachine<TState> stateMachine,
        TEntity entity,
        TState currentState,
        TState targetState,
        StateTransitionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available transitions from the current state
    /// </summary>
    Task<IList<AvailableTransition<TState>>> GetAvailableTransitionsAsync<TEntity>(
        IStateMachine<TState> stateMachine,
        TEntity entity,
        TState currentState,
        StateTransitionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a transition is possible
    /// </summary>
    Task<StateTransitionValidation<TState>> ValidateTransitionAsync<TEntity>(
        IStateMachine<TState> stateMachine,
        TEntity entity,
        TState currentState,
        TState targetState,
        StateTransitionOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for state transitions
/// </summary>
public class StateTransitionOptions
{
    /// <summary>
    /// User performing the transition
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Trigger/event causing the transition
    /// </summary>
    public string? Trigger { get; init; }

    /// <summary>
    /// Additional data for the transition
    /// </summary>
    public IDictionary<string, object?>? Data { get; init; }

    /// <summary>
    /// Service provider for resolving dependencies
    /// </summary>
    public IServiceProvider? ServiceProvider { get; init; }

    /// <summary>
    /// Whether to skip guard evaluation
    /// </summary>
    public bool SkipGuards { get; init; }

    /// <summary>
    /// Whether to skip action execution
    /// </summary>
    public bool SkipActions { get; init; }
}

/// <summary>
/// Result of a state transition
/// </summary>
public class StateTransitionResult<TState> where TState : notnull
{
    /// <summary>
    /// Whether the transition was successful
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Previous state
    /// </summary>
    public TState? PreviousState { get; init; }

    /// <summary>
    /// New state (same as previous if transition failed)
    /// </summary>
    public TState? NewState { get; init; }

    /// <summary>
    /// Error message if transition failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if transition failed
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Guard that blocked the transition
    /// </summary>
    public string? BlockingGuard { get; init; }

    /// <summary>
    /// Executed actions
    /// </summary>
    public IList<string>? ExecutedActions { get; init; }

    /// <summary>
    /// Timestamp of the transition
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static StateTransitionResult<TState> Success(TState previousState, TState newState, IList<string>? executedActions = null)
        => new() { IsSuccess = true, PreviousState = previousState, NewState = newState, ExecutedActions = executedActions };

    public static StateTransitionResult<TState> Failed(TState currentState, string errorMessage, string? errorCode = null, string? blockingGuard = null)
        => new() { IsSuccess = false, PreviousState = currentState, NewState = currentState, ErrorMessage = errorMessage, ErrorCode = errorCode, BlockingGuard = blockingGuard };
}

/// <summary>
/// Validation result for a potential transition
/// </summary>
public class StateTransitionValidation<TState> where TState : notnull
{
    /// <summary>
    /// Whether the transition is valid
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Whether the transition exists in the state machine
    /// </summary>
    public bool TransitionExists { get; init; }

    /// <summary>
    /// Guard evaluation results
    /// </summary>
    public IList<GuardEvaluationResult>? GuardResults { get; init; }

    /// <summary>
    /// Error message if invalid
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if invalid
    /// </summary>
    public string? ErrorCode { get; init; }
}

/// <summary>
/// Result of a single guard evaluation
/// </summary>
public class GuardEvaluationResult
{
    public required string GuardId { get; init; }
    public required string GuardName { get; init; }
    public bool Passed { get; init; }
    public string? FailureReason { get; init; }
}

/// <summary>
/// An available transition from the current state
/// </summary>
public class AvailableTransition<TState> where TState : notnull
{
    /// <summary>
    /// The transition
    /// </summary>
    public required StateTransition<TState> Transition { get; init; }

    /// <summary>
    /// Target state
    /// </summary>
    public required TState TargetState { get; init; }

    /// <summary>
    /// Target state definition
    /// </summary>
    public StateDefinition<TState>? TargetStateDefinition { get; init; }

    /// <summary>
    /// Whether guards have been evaluated
    /// </summary>
    public bool GuardsEvaluated { get; init; }

    /// <summary>
    /// Whether all guards passed
    /// </summary>
    public bool AllGuardsPassed { get; init; }

    /// <summary>
    /// Guard evaluation results
    /// </summary>
    public IList<GuardEvaluationResult>? GuardResults { get; init; }
}

/// <summary>
/// Default implementation of the state machine executor
/// </summary>
public class StateMachineExecutor<TState> : IStateMachineExecutor<TState> where TState : notnull
{
    private readonly ILogger<StateMachineExecutor<TState>> _logger;

    public StateMachineExecutor(ILogger<StateMachineExecutor<TState>> logger)
    {
        _logger = logger;
    }

    public async Task<StateTransitionResult<TState>> TransitionAsync<TEntity>(
        IStateMachine<TState> stateMachine,
        TEntity entity,
        TState currentState,
        TState targetState,
        StateTransitionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new StateTransitionOptions();

        _logger.LogDebug("Attempting transition from {FromState} to {ToState} in state machine {StateMachine}",
            currentState, targetState, stateMachine.Id);

        // Check if transition exists
        var transition = stateMachine.GetTransition(currentState, targetState);
        if (transition == null)
        {
            var validTransitions = stateMachine.GetTransitionsFrom(currentState)
                .Select(t => t.ToState.ToString())
                .ToList();

            var errorMessage = validTransitions.Count > 0
                ? $"Invalid transition from '{currentState}' to '{targetState}'. Valid transitions: {string.Join(", ", validTransitions)}"
                : $"No transitions available from state '{currentState}'";

            _logger.LogWarning("Invalid transition attempted: {ErrorMessage}", errorMessage);

            return StateTransitionResult<TState>.Failed(currentState, errorMessage, "INVALID_TRANSITION");
        }

        // Create context
        var context = new StateContext<TState, TEntity>(entity, currentState, targetState, options);

        // Evaluate guards
        if (!options.SkipGuards && transition.Guards?.Count > 0)
        {
            foreach (var guard in transition.Guards)
            {
                try
                {
                    var result = await guard.EvaluateAsync(entity, context);
                    if (!result.IsAllowed)
                    {
                        var message = result.Reason ?? guard.FailureMessage ?? "Guard evaluation failed";
                        _logger.LogInformation("Transition blocked by guard {GuardName}: {Reason}",
                            guard.Name, message);

                        return StateTransitionResult<TState>.Failed(
                            currentState, message, result.ErrorCode ?? "GUARD_FAILED", guard.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Guard {GuardName} threw an exception", guard.Name);
                    return StateTransitionResult<TState>.Failed(
                        currentState, $"Guard evaluation error: {ex.Message}", "GUARD_ERROR", guard.Id);
                }
            }
        }

        var executedActions = new List<string>();

        try
        {
            // Execute exit actions from current state
            if (!options.SkipActions)
            {
                var currentStateDef = stateMachine.GetState(currentState);
                if (currentStateDef?.OnExit?.Count > 0)
                {
                    foreach (var action in currentStateDef.OnExit)
                    {
                        await action.ExecuteAsync(entity, context);
                        executedActions.Add($"exit:{action.Id}");
                    }
                }

                // Execute transition actions
                if (transition.Actions?.Count > 0)
                {
                    foreach (var action in transition.Actions)
                    {
                        await action.ExecuteAsync(entity, context);
                        executedActions.Add($"transition:{action.Id}");
                    }
                }

                // Execute entry actions for target state
                var targetStateDef = stateMachine.GetState(targetState);
                if (targetStateDef?.OnEntry?.Count > 0)
                {
                    foreach (var action in targetStateDef.OnEntry)
                    {
                        await action.ExecuteAsync(entity, context);
                        executedActions.Add($"entry:{action.Id}");
                    }
                }
            }

            _logger.LogInformation("Transition successful from {FromState} to {ToState} in state machine {StateMachine}",
                currentState, targetState, stateMachine.Id);

            return StateTransitionResult<TState>.Success(currentState, targetState, executedActions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing transition actions from {FromState} to {ToState}",
                currentState, targetState);

            return StateTransitionResult<TState>.Failed(
                currentState, $"Action execution error: {ex.Message}", "ACTION_ERROR");
        }
    }

    public async Task<IList<AvailableTransition<TState>>> GetAvailableTransitionsAsync<TEntity>(
        IStateMachine<TState> stateMachine,
        TEntity entity,
        TState currentState,
        StateTransitionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new StateTransitionOptions();
        var transitions = stateMachine.GetTransitionsFrom(currentState);
        var available = new List<AvailableTransition<TState>>();

        foreach (var transition in transitions)
        {
            var guardResults = new List<GuardEvaluationResult>();
            var allPassed = true;

            if (transition.Guards?.Count > 0)
            {
                var context = new StateContext<TState, TEntity>(entity, currentState, transition.ToState, options);

                foreach (var guard in transition.Guards)
                {
                    try
                    {
                        var result = await guard.EvaluateAsync(entity, context);
                        guardResults.Add(new GuardEvaluationResult
                        {
                            GuardId = guard.Id,
                            GuardName = guard.Name,
                            Passed = result.IsAllowed,
                            FailureReason = result.Reason
                        });

                        if (!result.IsAllowed)
                        {
                            allPassed = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        guardResults.Add(new GuardEvaluationResult
                        {
                            GuardId = guard.Id,
                            GuardName = guard.Name,
                            Passed = false,
                            FailureReason = $"Error: {ex.Message}"
                        });
                        allPassed = false;
                    }
                }
            }

            available.Add(new AvailableTransition<TState>
            {
                Transition = transition,
                TargetState = transition.ToState,
                TargetStateDefinition = stateMachine.GetState(transition.ToState),
                GuardsEvaluated = transition.Guards?.Count > 0,
                AllGuardsPassed = allPassed,
                GuardResults = guardResults.Count > 0 ? guardResults : null
            });
        }

        return available;
    }

    public async Task<StateTransitionValidation<TState>> ValidateTransitionAsync<TEntity>(
        IStateMachine<TState> stateMachine,
        TEntity entity,
        TState currentState,
        TState targetState,
        StateTransitionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var transition = stateMachine.GetTransition(currentState, targetState);
        if (transition == null)
        {
            return new StateTransitionValidation<TState>
            {
                IsValid = false,
                TransitionExists = false,
                ErrorMessage = $"No transition defined from '{currentState}' to '{targetState}'",
                ErrorCode = "TRANSITION_NOT_DEFINED"
            };
        }

        if (transition.Guards == null || transition.Guards.Count == 0)
        {
            return new StateTransitionValidation<TState>
            {
                IsValid = true,
                TransitionExists = true
            };
        }

        options ??= new StateTransitionOptions();
        var context = new StateContext<TState, TEntity>(entity, currentState, targetState, options);
        var guardResults = new List<GuardEvaluationResult>();
        var allPassed = true;

        foreach (var guard in transition.Guards)
        {
            try
            {
                var result = await guard.EvaluateAsync(entity, context);
                guardResults.Add(new GuardEvaluationResult
                {
                    GuardId = guard.Id,
                    GuardName = guard.Name,
                    Passed = result.IsAllowed,
                    FailureReason = result.Reason
                });

                if (!result.IsAllowed)
                {
                    allPassed = false;
                }
            }
            catch (Exception ex)
            {
                guardResults.Add(new GuardEvaluationResult
                {
                    GuardId = guard.Id,
                    GuardName = guard.Name,
                    Passed = false,
                    FailureReason = $"Error: {ex.Message}"
                });
                allPassed = false;
            }
        }

        return new StateTransitionValidation<TState>
        {
            IsValid = allPassed,
            TransitionExists = true,
            GuardResults = guardResults,
            ErrorMessage = allPassed ? null : "One or more guards failed",
            ErrorCode = allPassed ? null : "GUARD_FAILED"
        };
    }
}

/// <summary>
/// Default implementation of state context
/// </summary>
internal class StateContext<TState, TEntity> : IStateContext<TState, TEntity> where TState : notnull
{
    public TEntity Entity { get; }
    public TState? PreviousState { get; }
    public TState CurrentState { get; }
    public TState? TargetState { get; }
    public string? Trigger { get; }
    public string? UserId { get; }
    public IDictionary<string, object?>? Data { get; }
    public IServiceProvider ServiceProvider { get; }
    public CancellationToken CancellationToken { get; }

    public StateContext(TEntity entity, TState currentState, TState? targetState, StateTransitionOptions options)
    {
        Entity = entity;
        CurrentState = currentState;
        TargetState = targetState;
        Trigger = options.Trigger;
        UserId = options.UserId;
        Data = options.Data;
        ServiceProvider = options.ServiceProvider ?? EmptyServiceProvider.Instance;
        CancellationToken = default;
    }
}

/// <summary>
/// Empty service provider for when none is provided
/// </summary>
internal class EmptyServiceProvider : IServiceProvider
{
    public static EmptyServiceProvider Instance { get; } = new();
    public object? GetService(Type serviceType) => null;
}
