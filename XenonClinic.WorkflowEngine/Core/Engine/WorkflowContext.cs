namespace XenonClinic.WorkflowEngine.Core.Engine;

using XenonClinic.WorkflowEngine.Core.Abstractions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Default implementation of the workflow context.
/// </summary>
public class WorkflowContext : IWorkflowContext
{
    private readonly ILogger? _logger;
    private readonly WorkflowInstanceState _state;

    public Guid InstanceId => _state.Id;
    public string WorkflowId => _state.WorkflowId;
    public int Version => _state.Version;
    public int? TenantId => _state.TenantId;
    public string? UserId => _state.CreatedBy;
    public string? CorrelationId => _state.CorrelationId;
    public DateTime CreatedAt => _state.CreatedAt;
    public WorkflowStatus Status => _state.Status;
    public IDictionary<string, object?> Input => _state.Input;
    public IDictionary<string, object?> Variables => _state.Variables;
    public IDictionary<string, object?> Output => _state.Output;
    public IServiceProvider ServiceProvider { get; }
    public CancellationToken CancellationToken { get; }

    public WorkflowContext(
        WorkflowInstanceState state,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken,
        ILogger? logger = null)
    {
        _state = state;
        ServiceProvider = serviceProvider;
        CancellationToken = cancellationToken;
        _logger = logger;
    }

    public T? GetVariable<T>(string name)
    {
        var variables = _state.Variables ?? new Dictionary<string, object?>();
        if (variables.TryGetValue(name, out var value))
        {
            if (value == null) return default;
            if (value is T typed) return typed;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException)
            {
                return default;
            }
            catch (FormatException)
            {
                return default;
            }
            catch (OverflowException)
            {
                return default;
            }
        }
        return default;
    }

    public void SetVariable(string name, object? value)
    {
        _state.Variables ??= new Dictionary<string, object?>();
        _state.Variables[name] = value;
    }

    public T? GetInput<T>(string name)
    {
        var input = _state.Input ?? new Dictionary<string, object?>();
        if (input.TryGetValue(name, out var value))
        {
            if (value == null) return default;
            if (value is T typed) return typed;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException)
            {
                return default;
            }
            catch (FormatException)
            {
                return default;
            }
            catch (OverflowException)
            {
                return default;
            }
        }
        return default;
    }

    public void SetOutput(string name, object? value)
    {
        _state.Output ??= new Dictionary<string, object?>();
        _state.Output[name] = value;
    }

    public void Log(Abstractions.LogLevel level, string message, params object[] args)
    {
        string formattedMessage;
        try
        {
            formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        }
        catch (FormatException)
        {
            formattedMessage = message; // Fall back to unformatted message
        }

        _state.LogEntries ??= new List<WorkflowLogEntry>();
        _state.LogEntries.Add(new WorkflowLogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = formattedMessage,
            ActivityId = _state.CurrentActivityId
        });

        var msLogLevel = level switch
        {
            Abstractions.LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            Abstractions.LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            Abstractions.LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            Abstractions.LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            Abstractions.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            Abstractions.LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };

        _logger?.Log(msLogLevel, "[{WorkflowId}:{InstanceId}] {Message}", WorkflowId, InstanceId, formattedMessage);
    }

    public void AddAuditEntry(string action, string? details = null)
    {
        _state.AuditEntries ??= new List<WorkflowAuditEntry>();
        _state.AuditEntries.Add(new WorkflowAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = action,
            Details = details,
            UserId = UserId,
            ActivityId = _state.CurrentActivityId
        });
    }
}
