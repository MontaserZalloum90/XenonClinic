namespace XenonClinic.WorkflowEngine.Domain.Models;

using System.Text.Json.Serialization;

/// <summary>
/// The complete process model that defines a workflow.
/// </summary>
public class ProcessModel
{
    /// <summary>
    /// Unique key identifying this process definition
    /// </summary>
    public string ProcessDefinitionKey { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of the process
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Documentation/description of the process
    /// </summary>
    public string? Documentation { get; set; }

    /// <summary>
    /// ID of the start activity
    /// </summary>
    public string StartActivityId { get; set; } = string.Empty;

    /// <summary>
    /// All activities in the process
    /// </summary>
    public Dictionary<string, ActivityDefinition> Activities { get; set; } = new();

    /// <summary>
    /// All elements (alias for Activities)
    /// </summary>
    public Dictionary<string, ActivityDefinition>? Elements { get; set; }

    /// <summary>
    /// Sequence flows connecting activities
    /// </summary>
    public List<SequenceFlow> SequenceFlows { get; set; } = new();

    /// <summary>
    /// Variable definitions
    /// </summary>
    public List<VariableDefinition> Variables { get; set; } = new();

    /// <summary>
    /// Process-level settings
    /// </summary>
    public ProcessSettings Settings { get; set; } = new();

    /// <summary>
    /// Process triggers (how the process can be started)
    /// </summary>
    public List<ProcessTrigger> Triggers { get; set; } = new();

    /// <summary>
    /// Error handlers
    /// </summary>
    public List<ErrorHandler> ErrorHandlers { get; set; } = new();

    /// <summary>
    /// Visual layout for designer
    /// </summary>
    public Dictionary<string, NodeLayout> Layout { get; set; } = new();

    /// <summary>
    /// Gets all outgoing sequence flows from an activity.
    /// </summary>
    public IEnumerable<SequenceFlow> GetOutgoingFlows(string activityId)
        => SequenceFlows.Where(f => f.SourceActivityId == activityId);

    /// <summary>
    /// Gets all incoming sequence flows to an activity.
    /// </summary>
    public IEnumerable<SequenceFlow> GetIncomingFlows(string activityId)
        => SequenceFlows.Where(f => f.TargetActivityId == activityId);
}

/// <summary>
/// Base class for all activity definitions.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StartEventDefinition), "startEvent")]
[JsonDerivedType(typeof(EndEventDefinition), "endEvent")]
[JsonDerivedType(typeof(UserTaskDefinition), "userTask")]
[JsonDerivedType(typeof(ServiceTaskDefinition), "serviceTask")]
[JsonDerivedType(typeof(ScriptTaskDefinition), "scriptTask")]
[JsonDerivedType(typeof(ExclusiveGatewayDefinition), "exclusiveGateway")]
[JsonDerivedType(typeof(ParallelGatewayDefinition), "parallelGateway")]
[JsonDerivedType(typeof(InclusiveGatewayDefinition), "inclusiveGateway")]
[JsonDerivedType(typeof(SubProcessDefinition), "subProcess")]
[JsonDerivedType(typeof(CallActivityDefinition), "callActivity")]
[JsonDerivedType(typeof(TimerEventDefinition), "timerEvent")]
[JsonDerivedType(typeof(MessageEventDefinition), "messageEvent")]
[JsonDerivedType(typeof(SignalEventDefinition), "signalEvent")]
[JsonDerivedType(typeof(IntermediateCatchEventDefinition), "intermediateCatchEvent")]
[JsonDerivedType(typeof(IntermediateThrowEventDefinition), "intermediateThrowEvent")]
public abstract class ActivityDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>
    /// Documentation for the activity
    /// </summary>
    public string? Documentation { get; set; }

    /// <summary>
    /// Whether this activity is asynchronous (creates a job)
    /// </summary>
    public bool IsAsync { get; set; }

    /// <summary>
    /// Retry policy for failures
    /// </summary>
    public RetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// Timeout for execution
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Boundary events attached to this activity
    /// </summary>
    public List<BoundaryEventDefinition>? BoundaryEvents { get; set; }

    /// <summary>
    /// Input variable mappings
    /// </summary>
    public Dictionary<string, string>? InputMappings { get; set; }

    /// <summary>
    /// Output variable mappings
    /// </summary>
    public Dictionary<string, string>? OutputMappings { get; set; }

    /// <summary>
    /// Custom properties
    /// </summary>
    public Dictionary<string, object>? Properties { get; set; }
}

#region Events

public class StartEventDefinition : ActivityDefinition
{
    public override string Type => "startEvent";

    /// <summary>
    /// Form for collecting initial data
    /// </summary>
    public string? FormKey { get; set; }
    public FormDefinition? Form { get; set; }
}

public class EndEventDefinition : ActivityDefinition
{
    public override string Type => "endEvent";

    /// <summary>
    /// If true, terminates the entire process (including parallel branches)
    /// </summary>
    public bool IsTerminate { get; set; }

    /// <summary>
    /// Error code for error end events
    /// </summary>
    public string? ErrorCode { get; set; }
}

public class TimerEventDefinition : ActivityDefinition
{
    public override string Type => "timerEvent";

    public TimerEventType TimerType { get; set; }

    /// <summary>
    /// For Date type: ISO 8601 date/time or expression
    /// </summary>
    public string? DateExpression { get; set; }

    /// <summary>
    /// For Duration type: ISO 8601 duration (PT1H, P1D) or expression
    /// </summary>
    public string? DurationExpression { get; set; }

    /// <summary>
    /// For Cycle type: cron expression or ISO 8601 repeating interval
    /// </summary>
    public string? CycleExpression { get; set; }
}

public enum TimerEventType
{
    Date,
    Duration,
    Cycle
}

public class MessageEventDefinition : ActivityDefinition
{
    public override string Type => "messageEvent";

    public string MessageName { get; set; } = string.Empty;

    /// <summary>
    /// Expression to correlate incoming messages
    /// </summary>
    public string? CorrelationKeyExpression { get; set; }
}

public class SignalEventDefinition : ActivityDefinition
{
    public override string Type => "signalEvent";

    public string SignalName { get; set; } = string.Empty;
}

public class IntermediateCatchEventDefinition : ActivityDefinition
{
    public override string Type => "intermediateCatchEvent";

    public string EventType { get; set; } = string.Empty;
    public string? SignalRef { get; set; }
    public string? MessageRef { get; set; }
    public string? TimerExpression { get; set; }
    public string? CorrelationKeyExpression { get; set; }
}

public class IntermediateThrowEventDefinition : ActivityDefinition
{
    public override string Type => "intermediateThrowEvent";

    public string EventType { get; set; } = string.Empty;
    public string? SignalRef { get; set; }
    public string? MessageRef { get; set; }
    public Dictionary<string, object>? MessagePayload { get; set; }
}

public class BoundaryEventDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AttachedToActivityId { get; set; } = string.Empty;
    public BoundaryEventType EventType { get; set; }

    /// <summary>
    /// If true, the attached activity is cancelled when this fires
    /// </summary>
    public bool CancelActivity { get; set; } = true;

    // Timer-specific
    public string? TimerDuration { get; set; }
    public string? TimerDate { get; set; }

    // Error-specific
    public string? ErrorCode { get; set; }

    // Target activity when boundary fires
    public string? TargetActivityId { get; set; }
}

public enum BoundaryEventType
{
    Timer,
    Error,
    Message,
    Signal,
    Escalation
}

#endregion

#region Tasks

public class UserTaskDefinition : ActivityDefinition
{
    public override string Type => "userTask";

    // Assignment
    public AssignmentStrategy AssignmentStrategy { get; set; } = AssignmentStrategy.Candidates;

    /// <summary>
    /// Expression to determine assignee: ${initiator}, ${variables.managerId}
    /// </summary>
    public string? AssigneeExpression { get; set; }

    /// <summary>
    /// Assignment configuration (alias/alternative to AssignmentStrategy)
    /// </summary>
    public string? Assignment { get; set; }

    public List<string>? CandidateUsers { get; set; }
    public List<string>? CandidateGroups { get; set; }
    public List<string>? CandidateRoles { get; set; }

    // Form
    public string? FormKey { get; set; }
    public FormDefinition? Form { get; set; }

    // Available actions
    public List<TaskActionDefinition> Actions { get; set; } = new()
    {
        new() { Id = "complete", Name = "Complete", IsPrimary = true }
    };

    // SLA
    public string? DueDateExpression { get; set; }
    public string? DurationExpression { get; set; }
    public TimeSpan? DueIn { get; set; }
    public int DefaultPriority { get; set; } = 5;
    public int? Priority { get; set; }

    // Escalation
    public List<EscalationRule>? Escalations { get; set; }

    // Reminders
    public List<ReminderRule>? Reminders { get; set; }
}

public enum AssignmentStrategy
{
    /// <summary>Directly assigned to a user</summary>
    Direct,
    /// <summary>Available to candidates who can claim</summary>
    Candidates,
    /// <summary>Round-robin among candidates</summary>
    RoundRobin,
    /// <summary>Load-balanced based on current task count</summary>
    LoadBalanced,
    /// <summary>Custom expression</summary>
    Expression
}

public class TaskActionDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPrimary { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }

    /// <summary>
    /// Condition for when this action is available
    /// </summary>
    public string? ConditionExpression { get; set; }

    /// <summary>
    /// Whether this action requires a comment
    /// </summary>
    public bool RequiresComment { get; set; }

    /// <summary>
    /// Confirmation message (if not null, shows confirm dialog)
    /// </summary>
    public string? ConfirmationMessage { get; set; }

    /// <summary>
    /// Variables to set when this action is taken
    /// </summary>
    public Dictionary<string, object>? OutputVariables { get; set; }
}

public class EscalationRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public TimeSpan After { get; set; }
    public EscalationAction Action { get; set; }
    public string? TargetExpression { get; set; }
    public int? NewPriority { get; set; }
    public string? NotificationTemplate { get; set; }
}

public enum EscalationAction
{
    Notify,
    Reassign,
    IncreasePriority,
    AddCandidate
}

public class ReminderRule
{
    public TimeSpan BeforeDue { get; set; }
    public string? NotificationTemplate { get; set; }
    public bool RepeatDaily { get; set; }
}

public class ServiceTaskDefinition : ActivityDefinition
{
    public override string Type => "serviceTask";

    /// <summary>
    /// Connector type: "http", "email", "database", etc.
    /// </summary>
    public string ConnectorType { get; set; } = "http";

    /// <summary>
    /// Service name
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// HTTP endpoint URL
    /// </summary>
    public string? HttpEndpoint { get; set; }

    /// <summary>
    /// Connector-specific configuration
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();

    /// <summary>
    /// Variable to store the result
    /// </summary>
    public string? ResultVariable { get; set; }

    /// <summary>
    /// Error handling
    /// </summary>
    public ServiceTaskErrorHandling ErrorHandling { get; set; } = new();
}

public class ServiceTaskErrorHandling
{
    /// <summary>
    /// Expression to determine if response is an error
    /// </summary>
    public string? ErrorCondition { get; set; }

    /// <summary>
    /// Error codes that should be caught by boundary event
    /// </summary>
    public List<string>? CatchableErrors { get; set; }

    /// <summary>
    /// If true, ignore errors and continue
    /// </summary>
    public bool IgnoreErrors { get; set; }
}

public class ScriptTaskDefinition : ActivityDefinition
{
    public override string Type => "scriptTask";

    /// <summary>
    /// Script language: "javascript", "csharp-expression"
    /// </summary>
    public string Language { get; set; } = "javascript";

    public string Script { get; set; } = string.Empty;

    public string? ResultVariable { get; set; }
}

#endregion

#region Gateways

public class ExclusiveGatewayDefinition : ActivityDefinition
{
    public override string Type => "exclusiveGateway";

    /// <summary>
    /// ID of the default outgoing flow (when no conditions match)
    /// </summary>
    public string? DefaultFlowId { get; set; }
}

public class ParallelGatewayDefinition : ActivityDefinition
{
    public override string Type => "parallelGateway";

    public GatewayDirection Direction { get; set; } = GatewayDirection.Unspecified;
}

public class InclusiveGatewayDefinition : ActivityDefinition
{
    public override string Type => "inclusiveGateway";

    public string? DefaultFlowId { get; set; }
    public GatewayDirection Direction { get; set; } = GatewayDirection.Unspecified;
}

public enum GatewayDirection
{
    Unspecified,
    Diverging,  // Split
    Converging  // Join
}

#endregion

#region Subprocess

public class SubProcessDefinition : ActivityDefinition
{
    public override string Type => "subProcess";

    /// <summary>
    /// Embedded process model
    /// </summary>
    public ProcessModel? EmbeddedProcess { get; set; }

    /// <summary>
    /// Activities in the subprocess (alias for EmbeddedProcess.Activities)
    /// </summary>
    public Dictionary<string, ActivityDefinition>? Activities { get; set; }

    /// <summary>
    /// Elements in the subprocess (alias for EmbeddedProcess.Activities)
    /// </summary>
    public Dictionary<string, ActivityDefinition>? Elements { get; set; }

    /// <summary>
    /// Sequence flows in the subprocess (alias for EmbeddedProcess.SequenceFlows)
    /// </summary>
    public List<SequenceFlow>? SequenceFlows { get; set; }

    /// <summary>
    /// Multi-instance configuration
    /// </summary>
    public MultiInstanceConfig? MultiInstance { get; set; }
}

public class CallActivityDefinition : ActivityDefinition
{
    public override string Type => "callActivity";

    /// <summary>
    /// Key of the process to call
    /// </summary>
    public string CalledProcessKey { get; set; } = string.Empty;

    /// <summary>
    /// Called element (alias for CalledProcessKey)
    /// </summary>
    public string? CalledElement { get; set; }

    /// <summary>
    /// Specific version to call (null = latest)
    /// </summary>
    public int? CalledProcessVersion { get; set; }

    public MultiInstanceConfig? MultiInstance { get; set; }

    /// <summary>
    /// Business key expression for child instances
    /// </summary>
    public string? BusinessKeyExpression { get; set; }
}

public class MultiInstanceConfig
{
    public bool IsSequential { get; set; }

    /// <summary>
    /// Expression that returns collection to iterate
    /// </summary>
    public string CollectionExpression { get; set; } = string.Empty;

    /// <summary>
    /// Variable name for current element
    /// </summary>
    public string ElementVariable { get; set; } = "item";

    /// <summary>
    /// Variable name for loop counter
    /// </summary>
    public string IndexVariable { get; set; } = "loopCounter";

    /// <summary>
    /// Completion condition expression
    /// </summary>
    public string? CompletionCondition { get; set; }
}

#endregion

#region Supporting Types

public class SequenceFlow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SourceActivityId { get; set; } = string.Empty;
    public string TargetActivityId { get; set; } = string.Empty;
    public string? Name { get; set; }

    /// <summary>
    /// Condition expression for this flow
    /// </summary>
    public string? ConditionExpression { get; set; }

    /// <summary>
    /// Is this the default flow from a gateway?
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Evaluation order (lower = first)
    /// </summary>
    public int Order { get; set; }
}

public class VariableDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string? Description { get; set; }
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public string? ValidationExpression { get; set; }
    public string? ValidationMessage { get; set; }
}

public class ProcessSettings
{
    public string? Category { get; set; }
    public bool TrackHistory { get; set; } = true;
    public bool EnableAudit { get; set; } = true;
    public int? MaxInstancesPerBusinessKey { get; set; }
    public TimeSpan? DefaultTaskTimeout { get; set; }
    public TimeSpan? InstanceTimeout { get; set; }
    public string? BusinessCalendar { get; set; }
    public Dictionary<string, object>? CustomProperties { get; set; }
}

public class ProcessTrigger
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ProcessTriggerType Type { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? Name { get; set; }

    // For message/signal triggers
    public string? MessageName { get; set; }
    public string? SignalName { get; set; }
    public string? CorrelationKeyExpression { get; set; }

    // For timer triggers
    public string? CronExpression { get; set; }

    // For API triggers
    public string? ApiPath { get; set; }
}

public enum ProcessTriggerType
{
    Manual,
    Message,
    Signal,
    Timer,
    Api
}

public class ErrorHandler
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<string>? ErrorCodes { get; set; }
    public string? HandlerActivityId { get; set; }
    public bool Compensate { get; set; }
    public int? RetryCount { get; set; }
    public TimeSpan? RetryDelay { get; set; }
}

public class RetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(30);
    public int InitialIntervalSeconds { get; set; } = 30;
    public double BackoffMultiplier { get; set; } = 2.0;
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(30);
    public List<string>? RetryableErrors { get; set; }
}

public class NodeLayout
{
    public double X { get; set; }
    public double Y { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
}

#endregion
