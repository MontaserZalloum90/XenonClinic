namespace XenonClinic.WorkflowEngine.Core.StateMachine;

using XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Generic state machine interface for managing entity state transitions.
/// </summary>
/// <typeparam name="TState">The state type (enum or class)</typeparam>
public interface IStateMachine<TState> where TState : notnull
{
    /// <summary>
    /// State machine identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// State machine name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Initial state
    /// </summary>
    TState InitialState { get; }

    /// <summary>
    /// All valid states
    /// </summary>
    IReadOnlyList<StateDefinition<TState>> States { get; }

    /// <summary>
    /// All valid transitions
    /// </summary>
    IReadOnlyList<StateTransition<TState>> Transitions { get; }

    /// <summary>
    /// Gets a state definition by state value
    /// </summary>
    StateDefinition<TState>? GetState(TState state);

    /// <summary>
    /// Gets valid transitions from a given state
    /// </summary>
    IReadOnlyList<StateTransition<TState>> GetTransitionsFrom(TState fromState);

    /// <summary>
    /// Checks if a transition is valid
    /// </summary>
    bool CanTransition(TState fromState, TState toState);

    /// <summary>
    /// Gets the specific transition between two states
    /// </summary>
    StateTransition<TState>? GetTransition(TState fromState, TState toState);

    /// <summary>
    /// Validates a state machine configuration
    /// </summary>
    ValidationResult Validate();
}

/// <summary>
/// State definition with metadata
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
public class StateDefinition<TState> where TState : notnull
{
    /// <summary>
    /// The state value
    /// </summary>
    public required TState State { get; init; }

    /// <summary>
    /// Display name for the state
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description of the state
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Category (e.g., "Initial", "Active", "Terminal")
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Whether this is a terminal (final) state
    /// </summary>
    public bool IsFinal { get; init; }

    /// <summary>
    /// Display order for UI
    /// </summary>
    public int DisplayOrder { get; init; }

    /// <summary>
    /// Color code for UI (hex)
    /// </summary>
    public string? ColorCode { get; init; }

    /// <summary>
    /// Icon class for UI
    /// </summary>
    public string? IconClass { get; init; }

    /// <summary>
    /// Entry actions to execute when entering this state
    /// </summary>
    public IList<IStateAction<TState>>? OnEntry { get; init; }

    /// <summary>
    /// Exit actions to execute when leaving this state
    /// </summary>
    public IList<IStateAction<TState>>? OnExit { get; init; }

    /// <summary>
    /// Metadata
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }
}

/// <summary>
/// State transition definition
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
public class StateTransition<TState> where TState : notnull
{
    /// <summary>
    /// Transition identifier
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Source state
    /// </summary>
    public required TState FromState { get; init; }

    /// <summary>
    /// Target state
    /// </summary>
    public required TState ToState { get; init; }

    /// <summary>
    /// Trigger/event name that causes this transition
    /// </summary>
    public string? Trigger { get; init; }

    /// <summary>
    /// Display name for the transition
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Description of the transition
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Guards that must pass for this transition
    /// </summary>
    public IList<IStateGuard<TState>>? Guards { get; init; }

    /// <summary>
    /// Actions to execute during the transition
    /// </summary>
    public IList<IStateAction<TState>>? Actions { get; init; }

    /// <summary>
    /// Priority for selecting between multiple valid transitions
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Whether this requires confirmation
    /// </summary>
    public bool RequiresConfirmation { get; init; }

    /// <summary>
    /// Required permission to execute this transition
    /// </summary>
    public string? RequiredPermission { get; init; }

    /// <summary>
    /// Metadata
    /// </summary>
    public IDictionary<string, object?>? Metadata { get; init; }
}

/// <summary>
/// Guard for state transitions
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
public interface IStateGuard<TState> where TState : notnull
{
    /// <summary>
    /// Guard identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Guard name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Error message if guard fails
    /// </summary>
    string? FailureMessage { get; }

    /// <summary>
    /// Evaluates the guard
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="entity">The entity being transitioned</param>
    /// <param name="context">The state context</param>
    /// <returns>Guard result</returns>
    Task<StateGuardResult> EvaluateAsync<TEntity>(TEntity entity, IStateContext<TState, TEntity> context);
}

/// <summary>
/// Result of a state guard evaluation
/// </summary>
public class StateGuardResult
{
    /// <summary>
    /// Whether the guard passed
    /// </summary>
    public bool IsAllowed { get; init; }

    /// <summary>
    /// Reason if not allowed
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Error code if not allowed
    /// </summary>
    public string? ErrorCode { get; init; }

    public static StateGuardResult Allowed() => new() { IsAllowed = true };
    public static StateGuardResult Denied(string reason, string? errorCode = null)
        => new() { IsAllowed = false, Reason = reason, ErrorCode = errorCode };
}

/// <summary>
/// Action to execute during state transitions
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
public interface IStateAction<TState> where TState : notnull
{
    /// <summary>
    /// Action identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Action name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="entity">The entity being transitioned</param>
    /// <param name="context">The state context</param>
    Task ExecuteAsync<TEntity>(TEntity entity, IStateContext<TState, TEntity> context);
}

/// <summary>
/// Context for state machine operations
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IStateContext<TState, TEntity> where TState : notnull
{
    /// <summary>
    /// The entity being transitioned
    /// </summary>
    TEntity Entity { get; }

    /// <summary>
    /// Previous state (if transitioning)
    /// </summary>
    TState? PreviousState { get; }

    /// <summary>
    /// Current state
    /// </summary>
    TState CurrentState { get; }

    /// <summary>
    /// Target state (if transitioning)
    /// </summary>
    TState? TargetState { get; }

    /// <summary>
    /// Trigger that caused the transition
    /// </summary>
    string? Trigger { get; }

    /// <summary>
    /// User performing the transition
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Transition data/parameters
    /// </summary>
    IDictionary<string, object?>? Data { get; }

    /// <summary>
    /// Service provider for dependency injection
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Cancellation token
    /// </summary>
    CancellationToken CancellationToken { get; }
}
