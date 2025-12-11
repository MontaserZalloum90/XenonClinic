namespace XenonClinic.WorkflowEngine.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XenonClinic.WorkflowEngine.Application.Services;
using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Core.Engine;
using XenonClinic.WorkflowEngine.Core.StateMachine;
using XenonClinic.WorkflowEngine.Infrastructure.Data;
using XenonClinic.WorkflowEngine.Persistence.Abstractions;
using XenonClinic.WorkflowEngine.Persistence.EfCore;
using XenonClinic.WorkflowEngine.Services;
using XenonClinic.WorkflowEngine.Validation;

/// <summary>
/// Extension methods for registering workflow engine services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the workflow engine services to the service collection.
    /// </summary>
    public static IServiceCollection AddWorkflowEngine(
        this IServiceCollection services,
        Action<WorkflowEngineBuilder>? configure = null)
    {
        var builder = new WorkflowEngineBuilder(services);
        configure?.Invoke(builder);

        // Core engine
        services.TryAddSingleton<WorkflowEngineOptions>();
        services.TryAddScoped<IWorkflowEngine, Core.Engine.WorkflowEngine>();

        // State machine
        services.TryAddScoped(typeof(IStateMachineExecutor<>), typeof(StateMachineExecutor<>));

        // Validation
        services.TryAddScoped<IWorkflowValidator, WorkflowValidator>();

        // Designer services
        services.TryAddScoped<IWorkflowDesignerService, WorkflowDesignerService>();

        // Enterprise workflow services - Phase 1
        services.TryAddScoped<IProcessDefinitionService, ProcessDefinitionService>();
        services.TryAddScoped<IProcessExecutionService, ProcessExecutionService>();
        services.TryAddScoped<IHumanTaskService, HumanTaskService>();
        services.TryAddScoped<IExpressionEvaluator, ExpressionEvaluator>();
        services.TryAddScoped<IAuditService, AuditService>();

        // Enterprise workflow services - Phase 2
        services.TryAddScoped<ITimerService, TimerService>();
        services.TryAddScoped<IJobProcessor, JobProcessor>();
        services.TryAddScoped<IServiceTaskExecutor, ServiceTaskExecutor>();
        services.TryAddScoped<IBusinessRulesEngine, BusinessRulesEngine>();
        services.TryAddScoped<IMonitoringService, MonitoringService>();

        // HTTP client for service tasks
        services.AddHttpClient("WorkflowServiceTask");

        return services;
    }

    /// <summary>
    /// Adds in-memory stores for development/testing.
    /// </summary>
    public static WorkflowEngineBuilder UseInMemoryStores(this WorkflowEngineBuilder builder)
    {
        builder.Services.AddSingleton<IWorkflowDefinitionStore, InMemoryWorkflowDefinitionStore>();
        builder.Services.AddSingleton<IWorkflowInstanceStore, InMemoryWorkflowInstanceStore>();
        builder.Services.AddSingleton<IWorkflowTimerStore, InMemoryWorkflowTimerStore>();
        return builder;
    }

    /// <summary>
    /// Adds EF Core stores for production use with the specified connection string.
    /// Provides transaction guarantees, distributed locking, and persistence.
    /// </summary>
    public static WorkflowEngineBuilder UseEfCoreStores(
        this WorkflowEngineBuilder builder,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        builder.Services.AddDbContext<WorkflowDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

            configureOptions?.Invoke(options);
        });

        builder.Services.AddScoped<IWorkflowDefinitionStore, EfCoreWorkflowDefinitionStore>();
        builder.Services.AddScoped<IWorkflowInstanceStore, EfCoreWorkflowInstanceStore>();
        builder.Services.AddScoped<IWorkflowTimerStore, EfCoreWorkflowTimerStore>();

        return builder;
    }

    /// <summary>
    /// Adds EF Core stores using an existing DbContext options configuration.
    /// </summary>
    public static WorkflowEngineBuilder UseEfCoreStores(
        this WorkflowEngineBuilder builder,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        builder.Services.AddDbContext<WorkflowDbContext>(configureOptions);

        builder.Services.AddScoped<IWorkflowDefinitionStore, EfCoreWorkflowDefinitionStore>();
        builder.Services.AddScoped<IWorkflowInstanceStore, EfCoreWorkflowInstanceStore>();
        builder.Services.AddScoped<IWorkflowTimerStore, EfCoreWorkflowTimerStore>();

        return builder;
    }

    /// <summary>
    /// Adds the enterprise workflow engine with full persistence using the specified connection string.
    /// This includes process definitions, instances, human tasks, timers, and audit events.
    /// </summary>
    public static WorkflowEngineBuilder UseEnterpriseWorkflowEngine(
        this WorkflowEngineBuilder builder,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureOptions = null)
    {
        builder.Services.AddDbContext<WorkflowEngineDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

            configureOptions?.Invoke(options);
        });

        return builder;
    }

    /// <summary>
    /// Adds the enterprise workflow engine using an existing DbContext options configuration.
    /// </summary>
    public static WorkflowEngineBuilder UseEnterpriseWorkflowEngine(
        this WorkflowEngineBuilder builder,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        builder.Services.AddDbContext<WorkflowEngineDbContext>(configureOptions);
        return builder;
    }

    /// <summary>
    /// Adds background services for processing timers and jobs.
    /// Call this method to enable automatic processing of scheduled timers and background jobs.
    /// </summary>
    public static WorkflowEngineBuilder AddBackgroundProcessing(this WorkflowEngineBuilder builder)
    {
        builder.Services.AddHostedService<Infrastructure.BackgroundServices.WorkflowBackgroundService>();
        builder.Services.AddHostedService<Infrastructure.BackgroundServices.WorkflowCleanupService>();
        return builder;
    }

    /// <summary>
    /// Configures workflow engine options.
    /// </summary>
    public static WorkflowEngineBuilder ConfigureOptions(
        this WorkflowEngineBuilder builder,
        Action<WorkflowEngineOptions> configure)
    {
        var options = new WorkflowEngineOptions();
        configure(options);
        // Use Replace to override the default TryAddSingleton registration
        builder.Services.Replace(ServiceDescriptor.Singleton(options));
        return builder;
    }

    /// <summary>
    /// Registers a task handler.
    /// </summary>
    public static WorkflowEngineBuilder AddTaskHandler<THandler>(this WorkflowEngineBuilder builder)
        where THandler : class, Core.Activities.ITaskHandler
    {
        builder.Services.AddScoped<THandler>();
        return builder;
    }

    /// <summary>
    /// Registers a state machine.
    /// </summary>
    public static WorkflowEngineBuilder AddStateMachine<TState>(
        this WorkflowEngineBuilder builder,
        string id,
        Func<StateMachineBuilder<TState>, StateMachine<TState>> configure)
        where TState : notnull
    {
        var machineBuilder = new StateMachineBuilder<TState>(id, id);
        var machine = configure(machineBuilder);
        builder.Services.AddSingleton<IStateMachine<TState>>(machine);
        return builder;
    }
}

/// <summary>
/// Builder for configuring the workflow engine.
/// </summary>
public class WorkflowEngineBuilder
{
    public IServiceCollection Services { get; }

    public WorkflowEngineBuilder(IServiceCollection services)
    {
        Services = services;
    }
}

#region In-Memory Implementations

/// <summary>
/// In-memory implementation of workflow definition store.
/// </summary>
public class InMemoryWorkflowDefinitionStore : IWorkflowDefinitionStore
{
    private readonly Dictionary<string, List<IWorkflowDefinition>> _definitions = new();
    private readonly object _lock = new();

    public Task<IWorkflowDefinition?> GetAsync(string id, int? version = null)
    {
        lock (_lock)
        {
            if (!_definitions.TryGetValue(id, out var versions))
                return Task.FromResult<IWorkflowDefinition?>(null);

            if (version.HasValue)
                return Task.FromResult<IWorkflowDefinition?>(versions.FirstOrDefault(d => d.Version == version.Value));

            // Return latest active version
            return Task.FromResult<IWorkflowDefinition?>(
                versions.Where(d => d.IsActive && !d.IsDraft)
                    .OrderByDescending(d => d.Version)
                    .FirstOrDefault()
                ?? versions.OrderByDescending(d => d.Version).FirstOrDefault());
        }
    }

    public Task<IList<IWorkflowDefinition>> GetVersionsAsync(string id)
    {
        lock (_lock)
        {
            if (!_definitions.TryGetValue(id, out var versions))
                return Task.FromResult<IList<IWorkflowDefinition>>(new List<IWorkflowDefinition>());

            return Task.FromResult<IList<IWorkflowDefinition>>(versions.OrderByDescending(d => d.Version).ToList());
        }
    }

    public Task<IWorkflowDefinition> SaveAsync(IWorkflowDefinition definition)
    {
        lock (_lock)
        {
            if (!_definitions.TryGetValue(definition.Id, out var versions))
            {
                versions = new List<IWorkflowDefinition>();
                _definitions[definition.Id] = versions;
            }

            var existing = versions.FirstOrDefault(d => d.Version == definition.Version);
            if (existing != null)
            {
                versions.Remove(existing);
            }

            versions.Add(definition);
            return Task.FromResult(definition);
        }
    }

    public Task DeleteAsync(string id)
    {
        lock (_lock)
        {
            if (_definitions.TryGetValue(id, out var versions))
            {
                foreach (var version in versions.ToList())
                {
                    if (version is Models.Definitions.WorkflowDefinitionModel model)
                    {
                        model.IsActive = false;
                    }
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task<WorkflowDefinitionListResult> ListAsync(WorkflowDefinitionQuery query)
    {
        lock (_lock)
        {
            var allDefinitions = _definitions.Values
                .SelectMany(v => v)
                .Where(d => !query.TenantId.HasValue || d.TenantId == query.TenantId)
                .Where(d => !query.IsActive.HasValue || d.IsActive == query.IsActive)
                .Where(d => !query.IsDraft.HasValue || d.IsDraft == query.IsDraft)
                .Where(d => string.IsNullOrEmpty(query.Category) || d.Category == query.Category)
                .Where(d => string.IsNullOrEmpty(query.SearchTerm) ||
                           d.Name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                           (d.Description?.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

            // Group by ID and take latest version
            var grouped = allDefinitions
                .GroupBy(d => d.Id)
                .Select(g => g.OrderByDescending(d => d.Version).First())
                .ToList();

            var total = grouped.Count;
            var items = grouped
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return Task.FromResult(new WorkflowDefinitionListResult
            {
                Items = items,
                TotalCount = total,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
    }

    public Task<IList<IWorkflowDefinition>> GetByTriggerAsync(TriggerType triggerType, string? triggerValue = null)
    {
        lock (_lock)
        {
            var results = _definitions.Values
                .SelectMany(v => v)
                .Where(d => d.IsActive && !d.IsDraft)
                .Where(d => d.Triggers?.Any(t => t.Type == triggerType && t.IsEnabled) == true)
                .GroupBy(d => d.Id)
                .Select(g => g.OrderByDescending(d => d.Version).First())
                .ToList();

            return Task.FromResult<IList<IWorkflowDefinition>>(results);
        }
    }

    public Task PublishAsync(string id, int version)
    {
        lock (_lock)
        {
            if (_definitions.TryGetValue(id, out var versions))
            {
                var definition = versions.FirstOrDefault(d => d.Version == version);
                if (definition is Models.Definitions.WorkflowDefinitionModel model)
                {
                    model.IsDraft = false;
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task UnpublishAsync(string id, int version)
    {
        lock (_lock)
        {
            if (_definitions.TryGetValue(id, out var versions))
            {
                var definition = versions.FirstOrDefault(d => d.Version == version);
                if (definition is Models.Definitions.WorkflowDefinitionModel model)
                {
                    model.IsDraft = true;
                }
            }
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory implementation of workflow instance store.
/// </summary>
public class InMemoryWorkflowInstanceStore : IWorkflowInstanceStore
{
    private readonly Dictionary<Guid, WorkflowInstanceState> _instances = new();
    private readonly Dictionary<Guid, List<WorkflowExecutionRecord>> _history = new();
    private readonly Dictionary<Guid, (string holder, DateTime expiry)> _locks = new();
    private readonly object _lock = new();

    public Task<WorkflowInstanceState?> GetAsync(Guid id)
    {
        lock (_lock)
        {
            return Task.FromResult(_instances.TryGetValue(id, out var instance) ? instance : null);
        }
    }

    public Task SaveAsync(WorkflowInstanceState instance)
    {
        lock (_lock)
        {
            _instances[instance.Id] = instance;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            _instances.Remove(id);
            _history.Remove(id);
        }
        return Task.CompletedTask;
    }

    public Task<WorkflowInstanceQueryResult> QueryAsync(WorkflowInstanceQuery query)
    {
        lock (_lock)
        {
            var filtered = _instances.Values.AsEnumerable();

            if (!string.IsNullOrEmpty(query.WorkflowId))
                filtered = filtered.Where(i => i.WorkflowId == query.WorkflowId);

            if (query.Statuses?.Count > 0)
                filtered = filtered.Where(i => query.Statuses.Contains(i.Status));

            if (query.TenantId.HasValue)
                filtered = filtered.Where(i => i.TenantId == query.TenantId);

            if (!string.IsNullOrEmpty(query.CorrelationId))
                filtered = filtered.Where(i => i.CorrelationId == query.CorrelationId);

            if (query.CreatedAfter.HasValue)
                filtered = filtered.Where(i => i.CreatedAt >= query.CreatedAfter);

            if (query.CreatedBefore.HasValue)
                filtered = filtered.Where(i => i.CreatedAt <= query.CreatedBefore);

            var total = filtered.Count();

            if (query.OrderDescending)
                filtered = filtered.OrderByDescending(i => i.CreatedAt);
            else
                filtered = filtered.OrderBy(i => i.CreatedAt);

            var items = filtered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Cast<IWorkflowInstance>()
                .ToList();

            return Task.FromResult(new WorkflowInstanceQueryResult
            {
                Items = items,
                TotalCount = total,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
    }

    public Task<IList<WorkflowExecutionRecord>> GetHistoryAsync(Guid instanceId)
    {
        lock (_lock)
        {
            if (_history.TryGetValue(instanceId, out var records))
                return Task.FromResult<IList<WorkflowExecutionRecord>>(records);

            return Task.FromResult<IList<WorkflowExecutionRecord>>(new List<WorkflowExecutionRecord>());
        }
    }

    public Task AddHistoryAsync(WorkflowExecutionRecord record)
    {
        lock (_lock)
        {
            if (!_history.TryGetValue(record.InstanceId, out var records))
            {
                records = new List<WorkflowExecutionRecord>();
                _history[record.InstanceId] = records;
            }
            records.Add(record);
        }
        return Task.CompletedTask;
    }

    public Task<IList<WorkflowInstanceState>> GetByBookmarkAsync(string bookmarkName)
    {
        lock (_lock)
        {
            var results = _instances.Values
                .Where(i => i.Bookmarks.Any(b => b.Name == bookmarkName))
                .ToList();

            return Task.FromResult<IList<WorkflowInstanceState>>(results);
        }
    }

    public Task<IList<WorkflowInstanceState>> GetScheduledAsync(DateTime until)
    {
        lock (_lock)
        {
            var results = _instances.Values
                .Where(i => i.Status == WorkflowStatus.Pending &&
                           i.ScheduledStartTime.HasValue &&
                           i.ScheduledStartTime <= until)
                .ToList();

            return Task.FromResult<IList<WorkflowInstanceState>>(results);
        }
    }

    public Task<bool> TryAcquireLockAsync(Guid instanceId, string lockHolder, TimeSpan duration)
    {
        lock (_lock)
        {
            if (_locks.TryGetValue(instanceId, out var existing))
            {
                if (existing.expiry > DateTime.UtcNow && existing.holder != lockHolder)
                    return Task.FromResult(false);
            }

            _locks[instanceId] = (lockHolder, DateTime.UtcNow.Add(duration));
            return Task.FromResult(true);
        }
    }

    public Task ReleaseLockAsync(Guid instanceId, string lockHolder)
    {
        lock (_lock)
        {
            if (_locks.TryGetValue(instanceId, out var existing) && existing.holder == lockHolder)
            {
                _locks.Remove(instanceId);
            }
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// In-memory implementation of workflow timer store.
/// </summary>
public class InMemoryWorkflowTimerStore : IWorkflowTimerStore
{
    private readonly Dictionary<Guid, WorkflowTimer> _timers = new();
    private readonly object _lock = new();

    public Task ScheduleAsync(WorkflowTimer timer)
    {
        lock (_lock)
        {
            _timers[timer.Id] = timer;
        }
        return Task.CompletedTask;
    }

    public Task<IList<WorkflowTimer>> GetDueTimersAsync(DateTime until)
    {
        lock (_lock)
        {
            var due = _timers.Values
                .Where(t => !t.IsTriggered && t.FireAt <= until)
                .ToList();

            return Task.FromResult<IList<WorkflowTimer>>(due);
        }
    }

    public Task MarkTriggeredAsync(Guid timerId)
    {
        lock (_lock)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsTriggered = true;
                timer.TriggeredAt = DateTime.UtcNow;
            }
        }
        return Task.CompletedTask;
    }

    public Task CancelAsync(Guid instanceId, string? bookmarkName = null)
    {
        lock (_lock)
        {
            var toRemove = _timers.Values
                .Where(t => t.InstanceId == instanceId &&
                           (bookmarkName == null || t.BookmarkName == bookmarkName))
                .Select(t => t.Id)
                .ToList();

            foreach (var id in toRemove)
            {
                _timers.Remove(id);
            }
        }
        return Task.CompletedTask;
    }
}

#endregion
