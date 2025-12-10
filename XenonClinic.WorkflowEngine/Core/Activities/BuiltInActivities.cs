namespace XenonClinic.WorkflowEngine.Core.Activities;

using XenonClinic.WorkflowEngine.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

/// <summary>
/// Start event activity - marks the beginning of a workflow
/// </summary>
public class StartActivity : ActivityBase
{
    public override string Type => "start";

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        context.Log(LogLevel.Information, "Workflow started: {WorkflowId}", context.WorkflowId);
        return Task.FromResult(ActivityResult.Success());
    }
}

/// <summary>
/// End event activity - marks the completion of a workflow
/// </summary>
public class EndActivity : ActivityBase
{
    public override string Type => "end";

    /// <summary>
    /// Output mappings to workflow output
    /// </summary>
    public IDictionary<string, string>? FinalOutputMappings { get; init; }

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        // Map final outputs
        if (FinalOutputMappings != null)
        {
            foreach (var mapping in FinalOutputMappings)
            {
                var value = ResolveExpression(mapping.Value, context);
                context.SetOutput(mapping.Key, value);
            }
        }

        context.Log(LogLevel.Information, "Workflow completed: {WorkflowId}", context.WorkflowId);
        return Task.FromResult(ActivityResult.Success());
    }
}

/// <summary>
/// Task activity - executes a custom task
/// </summary>
public class TaskActivity : ActivityBase
{
    public override string Type => "task";

    // Allowed assembly prefixes for task handlers (security whitelist)
    private static readonly string[] AllowedHandlerAssemblyPrefixes = new[]
    {
        "XenonClinic.",
        "Xenon.Platform.",
        "XenonClinic.WorkflowEngine."
    };

    /// <summary>
    /// Validates that a handler type is from an allowed assembly.
    /// </summary>
    private static bool IsAllowedHandlerType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return false;

        // Type name must start with one of our allowed prefixes
        return AllowedHandlerAssemblyPrefixes.Any(prefix =>
            typeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The task handler type to execute
    /// </summary>
    public string? TaskHandler { get; init; }

    /// <summary>
    /// Task parameters
    /// </summary>
    public IDictionary<string, object?>? Parameters { get; init; }

    public override async Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        if (string.IsNullOrEmpty(TaskHandler))
        {
            return ActivityResult.Failure("MISSING_HANDLER", "Task handler is not specified");
        }

        // Security: Validate handler type before loading
        if (!IsAllowedHandlerType(TaskHandler))
        {
            return ActivityResult.Failure("INVALID_HANDLER", $"Task handler type not allowed: {TaskHandler}");
        }

        // Resolve handler from DI
        var handlerType = System.Type.GetType(TaskHandler);
        if (handlerType == null)
        {
            return ActivityResult.Failure("INVALID_HANDLER", $"Task handler type not found: {TaskHandler}");
        }

        // Security: Verify the type implements ITaskHandler
        if (!typeof(ITaskHandler).IsAssignableFrom(handlerType))
        {
            return ActivityResult.Failure("INVALID_HANDLER", $"Type does not implement ITaskHandler: {TaskHandler}");
        }

        var handler = context.ServiceProvider.GetService(handlerType) as ITaskHandler;
        if (handler == null)
        {
            return ActivityResult.Failure("HANDLER_NOT_REGISTERED", $"Task handler not registered in DI: {TaskHandler}");
        }

        var inputs = MapInputs(context);
        if (Parameters != null)
        {
            foreach (var param in Parameters)
            {
                inputs[param.Key] = param.Value;
            }
        }

        var result = await handler.ExecuteAsync(inputs, context);
        MapOutputs(result.Output, context);

        return result;
    }
}

/// <summary>
/// Interface for task handlers
/// </summary>
public interface ITaskHandler
{
    Task<ActivityResult> ExecuteAsync(IDictionary<string, object?> input, IWorkflowContext context);
}

/// <summary>
/// Service task activity - calls an external service
/// </summary>
public class ServiceTaskActivity : ActivityBase
{
    public override string Type => "serviceTask";
    public override bool CanCompensate => true;

    // Allowed assembly prefixes for service types (security whitelist)
    private static readonly string[] AllowedServiceAssemblyPrefixes = new[]
    {
        "XenonClinic.",
        "Xenon.Platform.",
        "XenonClinic.Core.",
        "XenonClinic.Infrastructure."
    };

    /// <summary>
    /// Validates that a service type is from an allowed assembly.
    /// </summary>
    private static bool IsAllowedServiceType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return false;

        // Type name must start with one of our allowed prefixes
        return AllowedServiceAssemblyPrefixes.Any(prefix =>
            typeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Service interface type
    /// </summary>
    public required string ServiceType { get; init; }

    /// <summary>
    /// Method to invoke
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// Method parameters
    /// </summary>
    public IList<ServiceParameter>? MethodParameters { get; init; }

    /// <summary>
    /// Compensation method name
    /// </summary>
    public string? CompensationMethod { get; init; }

    public override async Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        // Security: Validate service type before loading
        if (!IsAllowedServiceType(ServiceType))
        {
            return ActivityResult.Failure("INVALID_SERVICE", $"Service type not allowed: {ServiceType}");
        }

        var serviceType = System.Type.GetType(ServiceType);
        if (serviceType == null)
        {
            return ActivityResult.Failure("INVALID_SERVICE", $"Service type not found: {ServiceType}");
        }

        var service = context.ServiceProvider.GetService(serviceType);
        if (service == null)
        {
            return ActivityResult.Failure("SERVICE_NOT_FOUND", $"Service not registered: {ServiceType}");
        }

        var method = serviceType.GetMethod(MethodName);
        if (method == null)
        {
            return ActivityResult.Failure("METHOD_NOT_FOUND", $"Method not found: {MethodName}");
        }

        try
        {
            var parameters = ResolveMethodParameters(context);
            var result = method.Invoke(service, parameters);

            if (result is Task task)
            {
                await task;
                var taskType = task.GetType();
                if (taskType.IsGenericType)
                {
                    var resultProperty = taskType.GetProperty("Result");
                    result = resultProperty?.GetValue(task);
                }
                else
                {
                    result = null;
                }
            }

            var output = new Dictionary<string, object?> { ["result"] = result };
            MapOutputs(output, context);

            return ActivityResult.Success(output);
        }
        catch (Exception ex)
        {
            return ActivityResult.Failure("SERVICE_ERROR", ex.Message, ex);
        }
    }

    public override async Task<ActivityResult> CompensateAsync(IWorkflowContext context)
    {
        if (string.IsNullOrEmpty(CompensationMethod))
        {
            return ActivityResult.Success();
        }

        var serviceType = System.Type.GetType(ServiceType);
        if (serviceType == null) return ActivityResult.Success();

        var service = context.ServiceProvider.GetService(serviceType);
        if (service == null) return ActivityResult.Success();

        var method = serviceType.GetMethod(CompensationMethod);
        if (method == null) return ActivityResult.Success();

        try
        {
            var result = method.Invoke(service, Array.Empty<object>());
            if (result is Task task)
            {
                await task;
            }
            return ActivityResult.Success();
        }
        catch (Exception ex)
        {
            return ActivityResult.Failure("COMPENSATION_ERROR", ex.Message, ex);
        }
    }

    private object?[] ResolveMethodParameters(IWorkflowContext context)
    {
        if (MethodParameters == null || MethodParameters.Count == 0)
        {
            return Array.Empty<object?>();
        }

        return MethodParameters.Select(p =>
        {
            if (p.Expression != null)
            {
                return ResolveExpression(p.Expression, context);
            }
            return p.Value;
        }).ToArray();
    }
}

/// <summary>
/// Service parameter definition
/// </summary>
public class ServiceParameter
{
    /// <summary>
    /// Static value
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// Dynamic expression
    /// </summary>
    public string? Expression { get; init; }
}

/// <summary>
/// Script task activity - executes a script/expression
/// </summary>
public class ScriptActivity : ActivityBase
{
    public override string Type => "script";

    /// <summary>
    /// Script language (csharp, javascript, expression)
    /// </summary>
    public string Language { get; init; } = "expression";

    /// <summary>
    /// Script content
    /// </summary>
    public required string Script { get; init; }

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        // For now, just support simple variable assignments
        // Full script execution would require a script engine
        try
        {
            // Simple expression evaluation
            var parts = Script.Split('=', 2);
            if (parts.Length == 2)
            {
                var variable = parts[0].Trim();
                var expression = parts[1].Trim();
                var value = ResolveExpression(expression, context);
                context.SetVariable(variable, value);
            }

            return Task.FromResult(ActivityResult.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(ActivityResult.Failure("SCRIPT_ERROR", ex.Message, ex));
        }
    }
}

/// <summary>
/// Exclusive gateway (XOR) - routes to one of multiple paths based on conditions
/// </summary>
public class ExclusiveGatewayActivity : ActivityBase
{
    public override string Type => "exclusiveGateway";

    /// <summary>
    /// Conditions for each outgoing path (target activity ID -> condition expression)
    /// </summary>
    public required IDictionary<string, string> Conditions { get; init; }

    /// <summary>
    /// Default path if no conditions match
    /// </summary>
    public string? DefaultPath { get; init; }

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        foreach (var condition in Conditions.OrderBy(c => c.Key))
        {
            if (ConditionEvaluator.Evaluate(condition.Value, context, ResolveExpression))
            {
                return Task.FromResult(ActivityResult.SuccessWithNext(condition.Key));
            }
        }

        if (!string.IsNullOrEmpty(DefaultPath))
        {
            return Task.FromResult(ActivityResult.SuccessWithNext(DefaultPath));
        }

        return Task.FromResult(ActivityResult.Failure("NO_PATH", "No condition matched and no default path defined"));
    }
}

/// <summary>
/// Shared condition evaluator for gateway activities
/// </summary>
internal static class ConditionEvaluator
{
    // Operators ordered by length (longest first) to avoid partial matches
    private static readonly string[] Operators = { "==", "!=", ">=", "<=", ">", "<" };

    public static bool Evaluate(string condition, IWorkflowContext context, Func<string, IWorkflowContext, object?> resolver)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return false;

        // Try to evaluate as boolean expression first
        var resolved = resolver(condition, context);
        if (resolved is bool boolValue)
            return boolValue;
        if (resolved is string strValue && bool.TryParse(strValue, out var parsedBool))
            return parsedBool;

        // Find the operator in the condition
        string? foundOperator = null;
        int operatorIndex = -1;

        foreach (var op in Operators)
        {
            var idx = condition.IndexOf(op, StringComparison.Ordinal);
            if (idx >= 0)
            {
                foundOperator = op;
                operatorIndex = idx;
                break;
            }
        }

        if (foundOperator == null || operatorIndex < 0)
            return false;

        var leftExpression = condition.Substring(0, operatorIndex).Trim();
        var rightExpression = condition.Substring(operatorIndex + foundOperator.Length).Trim();

        var left = resolver(leftExpression, context);
        var right = resolver(rightExpression.Trim('\'', '"'), context);

        return foundOperator switch
        {
            "==" => AreEqual(left, right),
            "!=" => !AreEqual(left, right),
            ">=" => CompareValues(left, right) >= 0,
            "<=" => CompareValues(left, right) <= 0,
            ">" => CompareValues(left, right) > 0,
            "<" => CompareValues(left, right) < 0,
            _ => false
        };
    }

    private static bool AreEqual(object? left, object? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left == null || right == null) return left == null && right == null;

        // Try direct equals
        if (Equals(left, right)) return true;

        // Try numeric comparison
        if (TryGetNumericValue(left, out var leftNum) && TryGetNumericValue(right, out var rightNum))
        {
            return Math.Abs(leftNum - rightNum) < double.Epsilon;
        }

        // String comparison (case-insensitive)
        return string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static int CompareValues(object? left, object? right)
    {
        if (left == null && right == null) return 0;
        if (left == null) return -1;
        if (right == null) return 1;

        // Handle numeric comparisons
        if (TryGetNumericValue(left, out var leftNum) && TryGetNumericValue(right, out var rightNum))
        {
            return leftNum.CompareTo(rightNum);
        }

        // Handle string comparisons
        if (left is string leftStr && right is string rightStr)
        {
            return string.Compare(leftStr, rightStr, StringComparison.OrdinalIgnoreCase);
        }

        // Try IComparable
        if (left is IComparable comparable)
        {
            try
            {
                var rightConverted = Convert.ChangeType(right, left.GetType());
                return comparable.CompareTo(rightConverted);
            }
            catch
            {
                return string.Compare(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }

        return string.Compare(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetNumericValue(object? value, out double result)
    {
        result = 0;
        if (value == null) return false;

        if (value is double d) { result = d; return true; }
        if (value is int i) { result = i; return true; }
        if (value is long l) { result = l; return true; }
        if (value is decimal dec) { result = (double)dec; return true; }
        if (value is float f) { result = f; return true; }

        if (value is string s && double.TryParse(s, out var parsed))
        {
            result = parsed;
            return true;
        }

        return false;
    }
}

/// <summary>
/// Parallel gateway - splits execution into multiple parallel branches or joins them
/// </summary>
public class ParallelGatewayActivity : ActivityBase
{
    public override string Type => "parallelGateway";

    /// <summary>
    /// Gateway direction (split or join)
    /// </summary>
    public GatewayDirection Direction { get; init; } = GatewayDirection.Split;

    /// <summary>
    /// Outgoing paths for split gateway
    /// </summary>
    public IList<string>? OutgoingPaths { get; init; }

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        if (Direction == GatewayDirection.Split && OutgoingPaths?.Count > 0)
        {
            return Task.FromResult(ActivityResult.Parallel(OutgoingPaths));
        }

        // Join - just continue to next activity
        return Task.FromResult(ActivityResult.Success());
    }
}

/// <summary>
/// Inclusive gateway (OR) - routes to one or more paths based on conditions
/// </summary>
public class InclusiveGatewayActivity : ActivityBase
{
    public override string Type => "inclusiveGateway";

    /// <summary>
    /// Conditions for each outgoing path
    /// </summary>
    public required IDictionary<string, string> Conditions { get; init; }

    /// <summary>
    /// Default path if no conditions match
    /// </summary>
    public string? DefaultPath { get; init; }

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        var matchingPaths = new List<string>();

        foreach (var condition in Conditions)
        {
            if (ConditionEvaluator.Evaluate(condition.Value, context, ResolveExpression))
            {
                matchingPaths.Add(condition.Key);
            }
        }

        if (matchingPaths.Count == 0 && !string.IsNullOrEmpty(DefaultPath))
        {
            matchingPaths.Add(DefaultPath);
        }

        if (matchingPaths.Count == 0)
        {
            return Task.FromResult(ActivityResult.Failure("NO_PATH", "No condition matched"));
        }

        if (matchingPaths.Count == 1)
        {
            return Task.FromResult(ActivityResult.SuccessWithNext(matchingPaths[0]));
        }

        return Task.FromResult(ActivityResult.Parallel(matchingPaths));
    }
}

/// <summary>
/// Gateway direction
/// </summary>
public enum GatewayDirection
{
    Split,
    Join
}

/// <summary>
/// User task activity - waits for user input
/// </summary>
public class UserTaskActivity : ResumableActivityBase
{
    public override string Type => "userTask";

    /// <summary>
    /// Form definition for user input
    /// </summary>
    public FormDefinition? Form { get; init; }

    /// <summary>
    /// Assignee (user ID or expression)
    /// </summary>
    public string? Assignee { get; init; }

    /// <summary>
    /// Candidate groups that can claim the task
    /// </summary>
    public IList<string>? CandidateGroups { get; init; }

    /// <summary>
    /// Due date expression
    /// </summary>
    public string? DueDate { get; init; }

    /// <summary>
    /// Priority (1-5)
    /// </summary>
    public int Priority { get; init; } = 3;

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        var bookmark = BookmarkName ?? $"userTask_{Id}";
        context.Log(LogLevel.Information, "User task created: {TaskName}, Assignee: {Assignee}", Name, Assignee);

        return Task.FromResult(ActivityResult.Waiting(bookmark));
    }

    public override Task<ActivityResult> ResumeAsync(IWorkflowContext context, IDictionary<string, object?>? input)
    {
        if (input != null)
        {
            MapOutputs(input, context);
        }

        context.Log(LogLevel.Information, "User task completed: {TaskName}", Name);
        return Task.FromResult(ActivityResult.Success(input));
    }
}

/// <summary>
/// Form definition for user tasks
/// </summary>
public class FormDefinition
{
    /// <summary>
    /// Form key/identifier
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    /// Form fields
    /// </summary>
    public IList<FormField>? Fields { get; init; }
}

/// <summary>
/// Form field definition
/// </summary>
public class FormField
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public string Type { get; init; } = "text";
    public bool Required { get; init; }
    public object? DefaultValue { get; init; }
    public IList<FormFieldOption>? Options { get; init; }
    public string? Validation { get; init; }
}

/// <summary>
/// Form field option (for select/radio)
/// </summary>
public class FormFieldOption
{
    public required string Value { get; init; }
    public required string Label { get; init; }
}

/// <summary>
/// Timer event activity - waits for a specified duration or until a specific time
/// </summary>
public class TimerActivity : ResumableActivityBase
{
    public override string Type => "timer";

    /// <summary>
    /// Duration to wait (ISO 8601 format, e.g., "PT1H" for 1 hour)
    /// </summary>
    public string? Duration { get; init; }

    /// <summary>
    /// Specific date/time to wait until
    /// </summary>
    public string? DateTime { get; init; }

    /// <summary>
    /// Cron expression for recurring timers
    /// </summary>
    public string? Cron { get; init; }

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        var bookmark = BookmarkName ?? $"timer_{Id}";
        var fireTime = CalculateFireTime();

        context.SetVariable($"_timer_{Id}_fireTime", fireTime);
        context.Log(LogLevel.Information, "Timer scheduled: {TimerName}, Fire at: {FireTime}", Name, fireTime);

        return Task.FromResult(ActivityResult.Waiting(bookmark));
    }

    public override Task<ActivityResult> ResumeAsync(IWorkflowContext context, IDictionary<string, object?>? input)
    {
        context.Log(LogLevel.Information, "Timer fired: {TimerName}", Name);
        return Task.FromResult(ActivityResult.Success());
    }

    private DateTime CalculateFireTime()
    {
        if (!string.IsNullOrEmpty(Duration))
        {
            // Parse ISO 8601 duration
            return System.DateTime.UtcNow.Add(System.Xml.XmlConvert.ToTimeSpan(Duration));
        }

        if (!string.IsNullOrEmpty(DateTime))
        {
            return System.DateTime.Parse(DateTime);
        }

        return System.DateTime.UtcNow;
    }
}

/// <summary>
/// Signal receive activity - waits for an external signal
/// </summary>
public class SignalReceiveActivity : ResumableActivityBase
{
    public override string Type => "signalReceive";

    /// <summary>
    /// Signal name to wait for
    /// </summary>
    public required string SignalName { get; init; }

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        var bookmark = BookmarkName ?? $"signal_{SignalName}_{Id}";
        context.Log(LogLevel.Information, "Waiting for signal: {SignalName}", SignalName);

        return Task.FromResult(ActivityResult.Waiting(bookmark));
    }

    public override Task<ActivityResult> ResumeAsync(IWorkflowContext context, IDictionary<string, object?>? input)
    {
        if (input != null)
        {
            MapOutputs(input, context);
        }

        context.Log(LogLevel.Information, "Signal received: {SignalName}", SignalName);
        return Task.FromResult(ActivityResult.Success(input));
    }
}

/// <summary>
/// Signal throw activity - sends a signal
/// </summary>
public class SignalThrowActivity : ActivityBase
{
    public override string Type => "signalThrow";

    /// <summary>
    /// Signal name to throw
    /// </summary>
    public required string SignalName { get; init; }

    /// <summary>
    /// Signal payload expression
    /// </summary>
    public string? PayloadExpression { get; init; }

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        var payload = PayloadExpression != null
            ? ResolveExpression(PayloadExpression, context)
            : null;

        // The workflow engine will handle broadcasting the signal
        context.SetVariable("_signal_" + SignalName, new { Signal = SignalName, Payload = payload });
        context.Log(LogLevel.Information, "Signal thrown: {SignalName}", SignalName);

        return Task.FromResult(ActivityResult.Success(new Dictionary<string, object?>
        {
            ["signalName"] = SignalName,
            ["payload"] = payload
        }));
    }
}

/// <summary>
/// Sub-process activity - executes another workflow as a sub-process
/// </summary>
public class SubProcessActivity : ActivityBase
{
    public override string Type => "subProcess";
    public override bool CanCompensate => true;

    /// <summary>
    /// Sub-workflow ID to execute
    /// </summary>
    public required string SubWorkflowId { get; init; }

    /// <summary>
    /// Whether to wait for sub-process completion
    /// </summary>
    public bool WaitForCompletion { get; init; } = true;

    public override async Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        var inputs = MapInputs(context);

        // Get workflow engine from DI and start sub-process
        var engine = context.ServiceProvider.GetService<IWorkflowEngine>();
        if (engine == null)
        {
            return ActivityResult.Failure("ENGINE_NOT_FOUND", "Workflow engine not available");
        }

        var result = await engine.StartNewAsync(SubWorkflowId, inputs, new WorkflowInstanceOptions
        {
            TenantId = context.TenantId,
            UserId = context.UserId,
            CorrelationId = context.CorrelationId
        });

        if (WaitForCompletion)
        {
            if (!result.IsCompleted)
            {
                // Store instance ID for later resumption
                context.SetVariable($"_subprocess_{Id}_instanceId", result.InstanceId);

                if (result.Status == WorkflowStatus.Suspended)
                {
                    return ActivityResult.Waiting($"subprocess_{Id}_{result.InstanceId}");
                }

                if (result.Status == WorkflowStatus.Faulted)
                {
                    return ActivityResult.Failure("SUBPROCESS_FAILED",
                        result.Error?.Message ?? "Sub-process failed",
                        null);
                }
            }
        }

        MapOutputs(result.Output, context);
        return ActivityResult.Success(result.Output);
    }

    public override async Task<ActivityResult> CompensateAsync(IWorkflowContext context)
    {
        var instanceIdVar = context.Variables.TryGetValue($"_subprocess_{Id}_instanceId", out var idObj);
        if (!instanceIdVar || idObj is not Guid instanceId) return ActivityResult.Success();

        var engine = context.ServiceProvider.GetService<IWorkflowEngine>();
        if (engine == null) return ActivityResult.Success();

        await engine.CancelAsync(instanceId, "Parent workflow compensation");
        return ActivityResult.Success();
    }
}

/// <summary>
/// Error boundary event - catches errors from attached activities
/// </summary>
public class ErrorBoundaryActivity : ActivityBase
{
    public override string Type => "errorBoundary";

    /// <summary>
    /// Activity ID this boundary is attached to
    /// </summary>
    public required string AttachedToActivityId { get; init; }

    /// <summary>
    /// Error codes to catch (empty = all errors)
    /// </summary>
    public IList<string>? ErrorCodes { get; init; }

    /// <summary>
    /// Error handling activity ID
    /// </summary>
    public string? ErrorHandlerActivityId { get; init; }

    public override Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        // This activity doesn't execute directly - it's handled by the workflow engine
        return Task.FromResult(ActivityResult.Success());
    }

    /// <summary>
    /// Checks if this boundary handles the given error
    /// </summary>
    public bool HandlesError(string? errorCode)
    {
        if (ErrorCodes == null || ErrorCodes.Count == 0) return true;
        return errorCode != null && ErrorCodes.Contains(errorCode);
    }
}

/// <summary>
/// Multi-instance activity - executes an activity multiple times (loop)
/// </summary>
public class MultiInstanceActivity : ActivityBase
{
    public override string Type => "multiInstance";

    /// <summary>
    /// The activity to execute multiple times
    /// </summary>
    public required IActivity InnerActivity { get; init; }

    /// <summary>
    /// Whether to execute sequentially or in parallel
    /// </summary>
    public bool IsSequential { get; init; } = true;

    /// <summary>
    /// Collection expression to iterate over
    /// </summary>
    public string? CollectionExpression { get; init; }

    /// <summary>
    /// Variable name for current item
    /// </summary>
    public string ItemVariable { get; init; } = "item";

    /// <summary>
    /// Variable name for current index
    /// </summary>
    public string IndexVariable { get; init; } = "index";

    /// <summary>
    /// Completion condition expression
    /// </summary>
    public string? CompletionCondition { get; init; }

    public override async Task<ActivityResult> ExecuteAsync(IWorkflowContext context)
    {
        var collection = ResolveExpression(CollectionExpression ?? "[]", context);
        if (collection is not System.Collections.IEnumerable enumerable)
        {
            return ActivityResult.Failure("INVALID_COLLECTION", "Collection expression did not resolve to an enumerable");
        }

        var items = enumerable.Cast<object?>().ToList();
        var results = new List<IDictionary<string, object?>>();
        var index = 0;

        foreach (var item in items)
        {
            context.SetVariable(ItemVariable, item);
            context.SetVariable(IndexVariable, index);

            var result = await InnerActivity.ExecuteAsync(context);
            if (!result.IsSuccess)
            {
                return result;
            }

            if (result.Output != null)
            {
                results.Add(result.Output);
            }

            // Check completion condition
            if (!string.IsNullOrEmpty(CompletionCondition))
            {
                var shouldComplete = ResolveExpression(CompletionCondition, context);
                if (shouldComplete is true)
                {
                    break;
                }
            }

            index++;
        }

        return ActivityResult.Success(new Dictionary<string, object?>
        {
            ["results"] = results,
            ["count"] = results.Count
        });
    }
}
