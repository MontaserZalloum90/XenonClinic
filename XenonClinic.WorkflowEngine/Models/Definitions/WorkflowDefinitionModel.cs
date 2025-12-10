namespace XenonClinic.WorkflowEngine.Models.Definitions;

using XenonClinic.WorkflowEngine.Core.Abstractions;

/// <summary>
/// Executable workflow definition model that implements IWorkflowDefinition.
/// </summary>
public class WorkflowDefinitionModel : IWorkflowDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; } = 1;
    public string? Category { get; set; }
    public IList<string>? Tags { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDraft { get; set; } = true;
    public string StartActivityId { get; set; } = string.Empty;
    public int? TenantId { get; set; }
    public IDictionary<string, object?>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Activities dictionary (mutable for building)
    /// </summary>
    public Dictionary<string, IActivity> Activities { get; set; } = new();

    /// <summary>
    /// Transitions list (mutable for building)
    /// </summary>
    public List<TransitionModel> Transitions { get; set; } = new();

    /// <summary>
    /// Input parameters
    /// </summary>
    public List<WorkflowParameter> InputParameters { get; set; } = new();

    /// <summary>
    /// Output parameters
    /// </summary>
    public List<WorkflowParameter> OutputParameters { get; set; } = new();

    /// <summary>
    /// Variables
    /// </summary>
    public List<WorkflowVariable> Variables { get; set; } = new();

    /// <summary>
    /// Error handlers
    /// </summary>
    public List<ErrorHandler> ErrorHandlers { get; set; } = new();

    /// <summary>
    /// Triggers
    /// </summary>
    public List<WorkflowTrigger> Triggers { get; set; } = new();

    // Interface implementations
    IReadOnlyDictionary<string, IActivity> IWorkflowDefinition.Activities => Activities;
    IReadOnlyList<ITransition> IWorkflowDefinition.Transitions => Transitions;
    IReadOnlyList<WorkflowParameter>? IWorkflowDefinition.InputParameters => InputParameters;
    IReadOnlyList<WorkflowParameter>? IWorkflowDefinition.OutputParameters => OutputParameters;
    IReadOnlyList<WorkflowVariable>? IWorkflowDefinition.Variables => Variables;
    IReadOnlyList<ErrorHandler>? IWorkflowDefinition.ErrorHandlers => ErrorHandlers;
    IReadOnlyList<WorkflowTrigger>? IWorkflowDefinition.Triggers => Triggers;

    public IActivity? GetActivity(string activityId)
    {
        return Activities.TryGetValue(activityId, out var activity) ? activity : null;
    }

    public IReadOnlyList<ITransition> GetOutgoingTransitions(string activityId)
    {
        return Transitions.Where(t => t.SourceActivityId == activityId)
            .OrderBy(t => t.Priority)
            .ToList();
    }

    public IReadOnlyList<ITransition> GetIncomingTransitions(string activityId)
    {
        return Transitions.Where(t => t.TargetActivityId == activityId).ToList();
    }

    public ValidationResult Validate()
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        // Check for start activity
        if (string.IsNullOrEmpty(StartActivityId))
        {
            errors.Add(new ValidationError
            {
                Code = "MISSING_START",
                Message = "Workflow must have a start activity"
            });
        }
        else if (!Activities.ContainsKey(StartActivityId))
        {
            errors.Add(new ValidationError
            {
                Code = "INVALID_START",
                Message = $"Start activity '{StartActivityId}' not found"
            });
        }

        // Check that all activities are reachable
        var reachable = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(StartActivityId);
        reachable.Add(StartActivityId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var transition in GetOutgoingTransitions(current))
            {
                if (reachable.Add(transition.TargetActivityId))
                {
                    queue.Enqueue(transition.TargetActivityId);
                }
            }
        }

        foreach (var activityId in Activities.Keys)
        {
            if (!reachable.Contains(activityId))
            {
                warnings.Add(new ValidationWarning
                {
                    Code = "UNREACHABLE_ACTIVITY",
                    Message = $"Activity '{activityId}' is not reachable from start",
                    ActivityId = activityId
                });
            }
        }

        // Check for end activities
        var hasEnd = Activities.Values.Any(a => a.Type == "end");
        if (!hasEnd)
        {
            warnings.Add(new ValidationWarning
            {
                Code = "NO_END_ACTIVITY",
                Message = "Workflow has no end activity"
            });
        }

        // Validate transitions reference valid activities
        foreach (var transition in Transitions)
        {
            if (!Activities.ContainsKey(transition.SourceActivityId))
            {
                errors.Add(new ValidationError
                {
                    Code = "INVALID_TRANSITION_SOURCE",
                    Message = $"Transition source '{transition.SourceActivityId}' not found",
                    PropertyPath = $"transitions[{transition.Id}].sourceActivityId"
                });
            }

            if (!Activities.ContainsKey(transition.TargetActivityId))
            {
                errors.Add(new ValidationError
                {
                    Code = "INVALID_TRANSITION_TARGET",
                    Message = $"Transition target '{transition.TargetActivityId}' not found",
                    PropertyPath = $"transitions[{transition.Id}].targetActivityId"
                });
            }
        }

        return new ValidationResult { Errors = errors, Warnings = warnings };
    }
}

/// <summary>
/// Transition model implementing ITransition
/// </summary>
public class TransitionModel : ITransition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SourceActivityId { get; set; } = string.Empty;
    public string TargetActivityId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Condition { get; set; }
    public int Priority { get; set; }
    public bool IsDefault { get; set; }
    public IList<ITransitionGuard>? Guards { get; set; }
    public IList<ITransitionAction>? Actions { get; set; }
}
