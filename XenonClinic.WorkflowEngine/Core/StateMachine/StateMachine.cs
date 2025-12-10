namespace XenonClinic.WorkflowEngine.Core.StateMachine;

using XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Implementation of a generic state machine.
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
public class StateMachine<TState> : IStateMachine<TState> where TState : notnull
{
    private readonly Dictionary<TState, StateDefinition<TState>> _states = new();
    private readonly List<StateTransition<TState>> _transitions = new();
    private readonly Dictionary<TState, List<StateTransition<TState>>> _transitionsBySource = new();

    public string Id { get; }
    public string Name { get; }
    public TState InitialState { get; }
    public IReadOnlyList<StateDefinition<TState>> States => _states.Values.ToList();
    public IReadOnlyList<StateTransition<TState>> Transitions => _transitions;

    public StateMachine(string id, string name, TState initialState)
    {
        Id = id;
        Name = name;
        InitialState = initialState;
    }

    /// <summary>
    /// Adds a state to the state machine
    /// </summary>
    public StateMachine<TState> AddState(StateDefinition<TState> state)
    {
        _states[state.State] = state;
        if (!_transitionsBySource.ContainsKey(state.State))
        {
            _transitionsBySource[state.State] = new List<StateTransition<TState>>();
        }
        return this;
    }

    /// <summary>
    /// Adds a transition to the state machine
    /// </summary>
    public StateMachine<TState> AddTransition(StateTransition<TState> transition)
    {
        _transitions.Add(transition);
        if (!_transitionsBySource.TryGetValue(transition.FromState, out var transitions))
        {
            transitions = new List<StateTransition<TState>>();
            _transitionsBySource[transition.FromState] = transitions;
        }
        transitions.Add(transition);
        return this;
    }

    public StateDefinition<TState>? GetState(TState state)
        => _states.TryGetValue(state, out var definition) ? definition : null;

    public IReadOnlyList<StateTransition<TState>> GetTransitionsFrom(TState fromState)
        => _transitionsBySource.TryGetValue(fromState, out var transitions)
            ? transitions
            : Array.Empty<StateTransition<TState>>();

    public bool CanTransition(TState fromState, TState toState)
        => GetTransition(fromState, toState) != null;

    public StateTransition<TState>? GetTransition(TState fromState, TState toState)
        => _transitionsBySource.TryGetValue(fromState, out var transitions)
            ? transitions.FirstOrDefault(t => EqualityComparer<TState>.Default.Equals(t.ToState, toState))
            : null;

    public ValidationResult Validate()
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        // Validate initial state exists
        if (!_states.ContainsKey(InitialState))
        {
            errors.Add(new ValidationError
            {
                Code = "INVALID_INITIAL_STATE",
                Message = $"Initial state '{InitialState}' is not defined"
            });
        }

        // Validate all transitions reference valid states
        foreach (var transition in _transitions)
        {
            if (!_states.ContainsKey(transition.FromState))
            {
                errors.Add(new ValidationError
                {
                    Code = "INVALID_FROM_STATE",
                    Message = $"Transition '{transition.Id}' references undefined from state '{transition.FromState}'"
                });
            }

            if (!_states.ContainsKey(transition.ToState))
            {
                errors.Add(new ValidationError
                {
                    Code = "INVALID_TO_STATE",
                    Message = $"Transition '{transition.Id}' references undefined to state '{transition.ToState}'"
                });
            }
        }

        // Check for unreachable states
        var reachableStates = FindReachableStates();
        foreach (var state in _states.Keys)
        {
            if (!reachableStates.Contains(state) && !EqualityComparer<TState>.Default.Equals(state, InitialState))
            {
                warnings.Add(new ValidationWarning
                {
                    Code = "UNREACHABLE_STATE",
                    Message = $"State '{state}' is not reachable from the initial state"
                });
            }
        }

        // Check for terminal states
        var hasTerminalState = _states.Values.Any(s => s.IsFinal);
        if (!hasTerminalState)
        {
            warnings.Add(new ValidationWarning
            {
                Code = "NO_TERMINAL_STATE",
                Message = "State machine has no terminal states defined"
            });
        }

        return new ValidationResult { Errors = errors, Warnings = warnings };
    }

    private HashSet<TState> FindReachableStates()
    {
        var reachable = new HashSet<TState>();
        var queue = new Queue<TState>();
        queue.Enqueue(InitialState);
        reachable.Add(InitialState);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var transition in GetTransitionsFrom(current))
            {
                if (reachable.Add(transition.ToState))
                {
                    queue.Enqueue(transition.ToState);
                }
            }
        }

        return reachable;
    }
}

/// <summary>
/// Fluent builder for state machines
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
public class StateMachineBuilder<TState> where TState : notnull
{
    private readonly string _id;
    private readonly string _name;
    private TState? _initialState;
    private readonly List<StateDefinition<TState>> _states = new();
    private readonly List<StateTransition<TState>> _transitions = new();

    public StateMachineBuilder(string id, string name)
    {
        _id = id;
        _name = name;
    }

    /// <summary>
    /// Sets the initial state
    /// </summary>
    public StateMachineBuilder<TState> WithInitialState(TState state)
    {
        _initialState = state;
        return this;
    }

    /// <summary>
    /// Adds a state
    /// </summary>
    public StateMachineBuilder<TState> AddState(TState state, string name, Action<StateBuilder<TState>>? configure = null)
    {
        var builder = new StateBuilder<TState>(state, name);
        configure?.Invoke(builder);
        _states.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Adds a transition
    /// </summary>
    public StateMachineBuilder<TState> AddTransition(TState from, TState to, Action<TransitionBuilder<TState>>? configure = null)
    {
        var builder = new TransitionBuilder<TState>(from, to);
        configure?.Invoke(builder);
        _transitions.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Builds the state machine
    /// </summary>
    public StateMachine<TState> Build()
    {
        if (_initialState == null)
        {
            throw new InvalidOperationException("Initial state must be set");
        }

        var stateMachine = new StateMachine<TState>(_id, _name, _initialState);
        foreach (var state in _states)
        {
            stateMachine.AddState(state);
        }
        foreach (var transition in _transitions)
        {
            stateMachine.AddTransition(transition);
        }

        return stateMachine;
    }
}

/// <summary>
/// Builder for state definitions
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
public class StateBuilder<TState> where TState : notnull
{
    private readonly TState _state;
    private readonly string _name;
    private string? _description;
    private string? _category;
    private bool _isFinal;
    private int _displayOrder;
    private string? _colorCode;
    private string? _iconClass;
    private readonly List<IStateAction<TState>> _onEntry = new();
    private readonly List<IStateAction<TState>> _onExit = new();
    private Dictionary<string, object?>? _metadata;

    public StateBuilder(TState state, string name)
    {
        _state = state;
        _name = name;
    }

    public StateBuilder<TState> WithDescription(string description) { _description = description; return this; }
    public StateBuilder<TState> WithCategory(string category) { _category = category; return this; }
    public StateBuilder<TState> AsFinal() { _isFinal = true; return this; }
    public StateBuilder<TState> WithDisplayOrder(int order) { _displayOrder = order; return this; }
    public StateBuilder<TState> WithColor(string colorCode) { _colorCode = colorCode; return this; }
    public StateBuilder<TState> WithIcon(string iconClass) { _iconClass = iconClass; return this; }
    public StateBuilder<TState> OnEntry(IStateAction<TState> action) { _onEntry.Add(action); return this; }
    public StateBuilder<TState> OnExit(IStateAction<TState> action) { _onExit.Add(action); return this; }
    public StateBuilder<TState> WithMetadata(string key, object? value)
    {
        _metadata ??= new Dictionary<string, object?>();
        _metadata[key] = value;
        return this;
    }

    public StateDefinition<TState> Build() => new()
    {
        State = _state,
        Name = _name,
        Description = _description,
        Category = _category,
        IsFinal = _isFinal,
        DisplayOrder = _displayOrder,
        ColorCode = _colorCode,
        IconClass = _iconClass,
        OnEntry = _onEntry.Count > 0 ? _onEntry : null,
        OnExit = _onExit.Count > 0 ? _onExit : null,
        Metadata = _metadata
    };
}

/// <summary>
/// Builder for state transitions
/// </summary>
/// <typeparam name="TState">The state type</typeparam>
public class TransitionBuilder<TState> where TState : notnull
{
    private readonly TState _from;
    private readonly TState _to;
    private string? _trigger;
    private string? _name;
    private string? _description;
    private readonly List<IStateGuard<TState>> _guards = new();
    private readonly List<IStateAction<TState>> _actions = new();
    private int _priority;
    private bool _requiresConfirmation;
    private string? _requiredPermission;
    private Dictionary<string, object?>? _metadata;

    public TransitionBuilder(TState from, TState to)
    {
        _from = from;
        _to = to;
    }

    public TransitionBuilder<TState> WithTrigger(string trigger) { _trigger = trigger; return this; }
    public TransitionBuilder<TState> WithName(string name) { _name = name; return this; }
    public TransitionBuilder<TState> WithDescription(string description) { _description = description; return this; }
    public TransitionBuilder<TState> WithGuard(IStateGuard<TState> guard) { _guards.Add(guard); return this; }
    public TransitionBuilder<TState> WithAction(IStateAction<TState> action) { _actions.Add(action); return this; }
    public TransitionBuilder<TState> WithPriority(int priority) { _priority = priority; return this; }
    public TransitionBuilder<TState> RequiresConfirmation() { _requiresConfirmation = true; return this; }
    public TransitionBuilder<TState> RequiresPermission(string permission) { _requiredPermission = permission; return this; }
    public TransitionBuilder<TState> WithMetadata(string key, object? value)
    {
        _metadata ??= new Dictionary<string, object?>();
        _metadata[key] = value;
        return this;
    }

    public StateTransition<TState> Build() => new()
    {
        FromState = _from,
        ToState = _to,
        Trigger = _trigger,
        Name = _name,
        Description = _description,
        Guards = _guards.Count > 0 ? _guards : null,
        Actions = _actions.Count > 0 ? _actions : null,
        Priority = _priority,
        RequiresConfirmation = _requiresConfirmation,
        RequiredPermission = _requiredPermission,
        Metadata = _metadata
    };
}
