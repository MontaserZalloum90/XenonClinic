namespace XenonClinic.WorkflowEngine.Persistence.EfCore;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Models.Definitions;
using XenonClinic.WorkflowEngine.Persistence.Abstractions;

/// <summary>
/// EF Core implementation of workflow instance store with distributed locking support.
/// </summary>
public class EfCoreWorkflowInstanceStore : IWorkflowInstanceStore
{
    private readonly WorkflowDbContext _context;
    private readonly ILogger<EfCoreWorkflowInstanceStore> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public EfCoreWorkflowInstanceStore(
        WorkflowDbContext context,
        ILogger<EfCoreWorkflowInstanceStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkflowInstanceState?> GetAsync(Guid id)
    {
        var entity = await _context.WorkflowInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (entity == null)
            return null;

        return MapToState(entity);
    }

    public async Task SaveAsync(WorkflowInstanceState instance)
    {
        var entity = await _context.WorkflowInstances.FindAsync(instance.Id);

        if (entity == null)
        {
            entity = new WorkflowInstanceEntity { Id = instance.Id };
            _context.WorkflowInstances.Add(entity);
        }

        MapToEntity(instance, entity);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict saving workflow instance {InstanceId}", instance.Id);
            throw new WorkflowConcurrencyException($"Workflow instance {instance.Id} was modified by another process", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        // Use ExecuteDelete for efficiency
        await _context.ExecutionHistory.Where(h => h.InstanceId == id).ExecuteDeleteAsync();
        await _context.Bookmarks.Where(b => b.InstanceId == id).ExecuteDeleteAsync();
        await _context.Timers.Where(t => t.InstanceId == id).ExecuteDeleteAsync();
        await _context.WorkflowInstances.Where(i => i.Id == id).ExecuteDeleteAsync();
    }

    public async Task<WorkflowInstanceQueryResult> QueryAsync(WorkflowInstanceQuery query)
    {
        var queryable = _context.WorkflowInstances.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(query.WorkflowId))
            queryable = queryable.Where(i => i.WorkflowId == query.WorkflowId);

        if (query.Statuses?.Count > 0)
        {
            var statusStrings = query.Statuses.Select(s => s.ToString()).ToList();
            queryable = queryable.Where(i => statusStrings.Contains(i.Status));
        }

        if (query.TenantId.HasValue)
            queryable = queryable.Where(i => i.TenantId == query.TenantId);

        if (!string.IsNullOrEmpty(query.CorrelationId))
            queryable = queryable.Where(i => i.CorrelationId == query.CorrelationId);

        if (query.CreatedAfter.HasValue)
            queryable = queryable.Where(i => i.CreatedAt >= query.CreatedAfter);

        if (query.CreatedBefore.HasValue)
            queryable = queryable.Where(i => i.CreatedAt <= query.CreatedBefore);

        var total = await queryable.CountAsync();

        queryable = query.OrderDescending
            ? queryable.OrderByDescending(i => i.CreatedAt)
            : queryable.OrderBy(i => i.CreatedAt);

        var entities = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new WorkflowInstanceQueryResult
        {
            Items = entities.Select(e => (IWorkflowInstance)MapToState(e)).ToList(),
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IList<WorkflowExecutionRecord>> GetHistoryAsync(Guid instanceId)
    {
        var entities = await _context.ExecutionHistory
            .AsNoTracking()
            .Where(h => h.InstanceId == instanceId)
            .OrderBy(h => h.Timestamp)
            .ToListAsync();

        return entities.Select(MapToExecutionRecord).ToList();
    }

    public async Task AddHistoryAsync(WorkflowExecutionRecord record)
    {
        var entity = new WorkflowExecutionHistoryEntity
        {
            Id = record.Id,
            InstanceId = record.InstanceId,
            ActivityId = record.ActivityId,
            ActivityName = record.ActivityName,
            ActivityType = record.ActivityType,
            RecordType = record.Type.ToString(),
            Timestamp = record.Timestamp,
            DurationMs = record.Duration.HasValue ? (long)record.Duration.Value.TotalMilliseconds : null,
            OutputJson = record.Output != null ? JsonSerializer.Serialize(record.Output, JsonOptions) : null,
            ErrorJson = record.Error != null ? JsonSerializer.Serialize(record.Error, JsonOptions) : null
        };

        _context.ExecutionHistory.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IList<WorkflowInstanceState>> GetByBookmarkAsync(string bookmarkName)
    {
        var instanceIds = await _context.Bookmarks
            .AsNoTracking()
            .Where(b => b.Name == bookmarkName)
            .Select(b => b.InstanceId)
            .ToListAsync();

        var entities = await _context.WorkflowInstances
            .AsNoTracking()
            .Where(i => instanceIds.Contains(i.Id))
            .ToListAsync();

        return entities.Select(MapToState).ToList();
    }

    public async Task<IList<WorkflowInstanceState>> GetScheduledAsync(DateTime until)
    {
        var entities = await _context.WorkflowInstances
            .AsNoTracking()
            .Where(i => i.Status == "Pending" &&
                       i.ScheduledStartTime != null &&
                       i.ScheduledStartTime <= until)
            .ToListAsync();

        return entities.Select(MapToState).ToList();
    }

    public async Task<bool> TryAcquireLockAsync(Guid instanceId, string lockHolder, TimeSpan duration)
    {
        var now = DateTime.UtcNow;
        var expiry = now.Add(duration);

        // Use optimistic concurrency with a single update
        var updated = await _context.WorkflowInstances
            .Where(i => i.Id == instanceId &&
                       (i.LockHolder == null || i.LockExpiry == null || i.LockExpiry < now || i.LockHolder == lockHolder))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.LockHolder, lockHolder)
                .SetProperty(i => i.LockExpiry, expiry));

        if (updated > 0)
        {
            _logger.LogDebug("Acquired lock for workflow {InstanceId} by {LockHolder} until {Expiry}",
                instanceId, lockHolder, expiry);
            return true;
        }

        _logger.LogDebug("Failed to acquire lock for workflow {InstanceId} by {LockHolder}",
            instanceId, lockHolder);
        return false;
    }

    public async Task ReleaseLockAsync(Guid instanceId, string lockHolder)
    {
        var updated = await _context.WorkflowInstances
            .Where(i => i.Id == instanceId && i.LockHolder == lockHolder)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.LockHolder, (string?)null)
                .SetProperty(i => i.LockExpiry, (DateTime?)null));

        if (updated > 0)
        {
            _logger.LogDebug("Released lock for workflow {InstanceId} by {LockHolder}",
                instanceId, lockHolder);
        }
    }

    #region Mapping

    private static WorkflowInstanceState MapToState(WorkflowInstanceEntity entity)
    {
        WorkflowStateData stateData;
        try
        {
            stateData = JsonSerializer.Deserialize<WorkflowStateData>(entity.StateJson ?? "{}", JsonOptions)
                ?? new WorkflowStateData();
        }
        catch (JsonException)
        {
            stateData = new WorkflowStateData();
        }

        var state = new WorkflowInstanceState
        {
            Id = entity.Id,
            WorkflowId = entity.WorkflowId,
            Version = entity.Version,
            Name = entity.Name,
            Status = Enum.TryParse<WorkflowStatus>(entity.Status, out var status) ? status : WorkflowStatus.Pending,
            TenantId = entity.TenantId,
            CreatedBy = entity.CreatedBy,
            CorrelationId = entity.CorrelationId,
            Priority = entity.Priority,
            CreatedAt = entity.CreatedAt,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            ScheduledStartTime = entity.ScheduledStartTime,
            CurrentActivityId = entity.CurrentActivityId,
            FaultCount = entity.FaultCount,
            Input = stateData.Input ?? new Dictionary<string, object?>(),
            Output = stateData.Output ?? new Dictionary<string, object?>(),
            Variables = stateData.Variables ?? new Dictionary<string, object?>(),
            Metadata = stateData.Metadata ?? new Dictionary<string, object?>(),
            CompletedActivityIds = stateData.CompletedActivityIds ?? new List<string>(),
            ActiveActivityIds = stateData.ActiveActivityIds ?? new List<string>(),
            Bookmarks = stateData.Bookmarks ?? new List<WorkflowBookmark>(),
            LogEntries = stateData.LogEntries ?? new List<WorkflowLogEntry>(),
            AuditEntries = stateData.AuditEntries ?? new List<WorkflowAuditEntry>()
        };

        if (!string.IsNullOrEmpty(entity.ErrorJson))
        {
            try
            {
                state.Error = JsonSerializer.Deserialize<WorkflowError>(entity.ErrorJson, JsonOptions);
            }
            catch (JsonException)
            {
                // Keep Error as null if deserialization fails
            }
        }

        return state;
    }

    private static void MapToEntity(WorkflowInstanceState state, WorkflowInstanceEntity entity)
    {
        entity.WorkflowId = state.WorkflowId;
        entity.Version = state.Version;
        entity.Name = state.Name;
        entity.Status = state.Status.ToString();
        entity.TenantId = state.TenantId;
        entity.CreatedBy = state.CreatedBy;
        entity.CorrelationId = state.CorrelationId;
        entity.Priority = state.Priority;
        entity.CreatedAt = state.CreatedAt;
        entity.StartedAt = state.StartedAt;
        entity.CompletedAt = state.CompletedAt;
        entity.ScheduledStartTime = state.ScheduledStartTime;
        entity.CurrentActivityId = state.CurrentActivityId;
        entity.FaultCount = state.FaultCount;

        var stateData = new WorkflowStateData
        {
            Input = state.Input,
            Output = state.Output,
            Variables = state.Variables,
            Metadata = state.Metadata,
            CompletedActivityIds = state.CompletedActivityIds?.ToList() ?? new List<string>(),
            ActiveActivityIds = state.ActiveActivityIds?.ToList() ?? new List<string>(),
            Bookmarks = state.Bookmarks?.ToList() ?? new List<WorkflowBookmark>(),
            LogEntries = state.LogEntries?.ToList() ?? new List<WorkflowLogEntry>(),
            AuditEntries = state.AuditEntries?.ToList() ?? new List<WorkflowAuditEntry>()
        };

        entity.StateJson = JsonSerializer.Serialize(stateData, JsonOptions);
        entity.ErrorJson = state.Error != null ? JsonSerializer.Serialize(state.Error, JsonOptions) : null;
    }

    private static WorkflowExecutionRecord MapToExecutionRecord(WorkflowExecutionHistoryEntity entity)
    {
        Dictionary<string, object?>? output = null;
        ActivityError? error = null;

        if (entity.OutputJson != null)
        {
            try
            {
                output = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.OutputJson, JsonOptions);
            }
            catch (JsonException)
            {
                // Keep output as null if deserialization fails
            }
        }

        if (entity.ErrorJson != null)
        {
            try
            {
                error = JsonSerializer.Deserialize<ActivityError>(entity.ErrorJson, JsonOptions);
            }
            catch (JsonException)
            {
                // Keep error as null if deserialization fails
            }
        }

        return new WorkflowExecutionRecord
        {
            Id = entity.Id,
            InstanceId = entity.InstanceId,
            ActivityId = entity.ActivityId,
            ActivityName = entity.ActivityName,
            ActivityType = entity.ActivityType,
            Type = Enum.TryParse<ExecutionRecordType>(entity.RecordType, out var type)
                ? type : ExecutionRecordType.ActivityStarted,
            Timestamp = entity.Timestamp,
            Duration = entity.DurationMs.HasValue ? TimeSpan.FromMilliseconds(entity.DurationMs.Value) : null,
            Output = output,
            Error = error
        };
    }

    #endregion
}

/// <summary>
/// Internal class for serializing workflow state data
/// </summary>
internal class WorkflowStateData
{
    public IDictionary<string, object?>? Input { get; set; }
    public IDictionary<string, object?>? Output { get; set; }
    public IDictionary<string, object?>? Variables { get; set; }
    public IDictionary<string, object?>? Metadata { get; set; }
    public List<string>? CompletedActivityIds { get; set; }
    public List<string>? ActiveActivityIds { get; set; }
    public List<WorkflowBookmark>? Bookmarks { get; set; }
    public List<WorkflowLogEntry>? LogEntries { get; set; }
    public List<WorkflowAuditEntry>? AuditEntries { get; set; }
}

/// <summary>
/// Exception thrown when a concurrency conflict is detected
/// </summary>
public class WorkflowConcurrencyException : Exception
{
    public WorkflowConcurrencyException(string message) : base(message) { }
    public WorkflowConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// EF Core implementation of workflow definition store.
/// </summary>
public class EfCoreWorkflowDefinitionStore : IWorkflowDefinitionStore
{
    private readonly WorkflowDbContext _context;
    private readonly ILogger<EfCoreWorkflowDefinitionStore> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public EfCoreWorkflowDefinitionStore(
        WorkflowDbContext context,
        ILogger<EfCoreWorkflowDefinitionStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IWorkflowDefinition?> GetAsync(string id, int? version = null)
    {
        WorkflowDefinitionEntity? entity;

        if (version.HasValue)
        {
            entity = await _context.WorkflowDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id && d.Version == version.Value);
        }
        else
        {
            // Return latest active, non-draft version
            entity = await _context.WorkflowDefinitions
                .AsNoTracking()
                .Where(d => d.Id == id && d.IsActive && !d.IsDraft)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync();

            // Fall back to latest version if no active published version
            entity ??= await _context.WorkflowDefinitions
                .AsNoTracking()
                .Where(d => d.Id == id)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync();
        }

        if (entity == null)
            return null;

        return DeserializeDefinition(entity);
    }

    public async Task<IList<IWorkflowDefinition>> GetVersionsAsync(string id)
    {
        var entities = await _context.WorkflowDefinitions
            .AsNoTracking()
            .Where(d => d.Id == id)
            .OrderByDescending(d => d.Version)
            .ToListAsync();

        return entities.Select(DeserializeDefinition).Cast<IWorkflowDefinition>().ToList();
    }

    public async Task<IWorkflowDefinition> SaveAsync(IWorkflowDefinition definition)
    {
        var entity = await _context.WorkflowDefinitions
            .FirstOrDefaultAsync(d => d.Id == definition.Id && d.Version == definition.Version);

        if (entity == null)
        {
            entity = new WorkflowDefinitionEntity
            {
                Id = definition.Id,
                Version = definition.Version,
                CreatedAt = DateTime.UtcNow
            };
            _context.WorkflowDefinitions.Add(entity);
        }

        entity.Name = definition.Name;
        entity.Description = definition.Description;
        entity.Category = definition.Category;
        entity.IsActive = definition.IsActive;
        entity.IsDraft = definition.IsDraft;
        entity.TenantId = definition.TenantId;
        // Note: CreatedBy is not available in IWorkflowDefinition
        entity.UpdatedAt = DateTime.UtcNow;
        entity.DefinitionJson = JsonSerializer.Serialize(definition, JsonOptions);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Saved workflow definition {WorkflowId} v{Version}", definition.Id, definition.Version);

        return definition;
    }

    public async Task DeleteAsync(string id)
    {
        await _context.WorkflowDefinitions
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.IsActive, false));
    }

    public async Task<WorkflowDefinitionListResult> ListAsync(WorkflowDefinitionQuery query)
    {
        var queryable = _context.WorkflowDefinitions.AsNoTracking().AsQueryable();

        if (query.TenantId.HasValue)
            queryable = queryable.Where(d => d.TenantId == query.TenantId);

        if (query.IsActive.HasValue)
            queryable = queryable.Where(d => d.IsActive == query.IsActive);

        if (query.IsDraft.HasValue)
            queryable = queryable.Where(d => d.IsDraft == query.IsDraft);

        if (!string.IsNullOrEmpty(query.Category))
            queryable = queryable.Where(d => d.Category == query.Category);

        if (!string.IsNullOrEmpty(query.SearchTerm))
            queryable = queryable.Where(d =>
                d.Name.Contains(query.SearchTerm) ||
                (d.Description != null && d.Description.Contains(query.SearchTerm)));

        // Group by ID and take latest version
        var grouped = queryable
            .GroupBy(d => d.Id)
            .Select(g => g.OrderByDescending(d => d.Version).First());

        var total = await grouped.CountAsync();

        var entities = await grouped
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new WorkflowDefinitionListResult
        {
            Items = entities.Select(DeserializeDefinition).Cast<IWorkflowDefinition>().ToList(),
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IList<IWorkflowDefinition>> GetByTriggerAsync(TriggerType triggerType, string? triggerValue = null)
    {
        // This requires deserializing to check triggers, so we do it in memory
        var entities = await _context.WorkflowDefinitions
            .AsNoTracking()
            .Where(d => d.IsActive && !d.IsDraft)
            .ToListAsync();

        var results = entities
            .Select(DeserializeDefinition)
            .Where(d => d.Triggers?.Any(t => t.Type == triggerType && t.IsEnabled) == true)
            .GroupBy(d => d.Id)
            .Select(g => g.OrderByDescending(d => d.Version).First())
            .Cast<IWorkflowDefinition>()
            .ToList();

        return results;
    }

    public async Task PublishAsync(string id, int version)
    {
        await _context.WorkflowDefinitions
            .Where(d => d.Id == id && d.Version == version)
            .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.IsDraft, false));

        _logger.LogInformation("Published workflow definition {WorkflowId} v{Version}", id, version);
    }

    public async Task UnpublishAsync(string id, int version)
    {
        await _context.WorkflowDefinitions
            .Where(d => d.Id == id && d.Version == version)
            .ExecuteUpdateAsync(setters => setters.SetProperty(d => d.IsDraft, true));

        _logger.LogInformation("Unpublished workflow definition {WorkflowId} v{Version}", id, version);
    }

    private static WorkflowDefinitionModel DeserializeDefinition(WorkflowDefinitionEntity entity)
    {
        WorkflowDefinitionModel definition;
        try
        {
            definition = JsonSerializer.Deserialize<WorkflowDefinitionModel>(entity.DefinitionJson ?? "{}", JsonOptions)
                ?? new WorkflowDefinitionModel();
        }
        catch (JsonException)
        {
            definition = new WorkflowDefinitionModel();
        }

        // Ensure metadata from entity is applied
        definition.Id = entity.Id;
        definition.Version = entity.Version;
        definition.Name = entity.Name;
        definition.Description = entity.Description;
        definition.Category = entity.Category;
        definition.IsActive = entity.IsActive;
        definition.IsDraft = definition.IsDraft;
        definition.TenantId = entity.TenantId;
        definition.CreatedAt = entity.CreatedAt;
        definition.ModifiedAt = entity.UpdatedAt;

        return definition;
    }
}

/// <summary>
/// EF Core implementation of workflow timer store.
/// </summary>
public class EfCoreWorkflowTimerStore : IWorkflowTimerStore
{
    private readonly WorkflowDbContext _context;
    private readonly ILogger<EfCoreWorkflowTimerStore> _logger;

    public EfCoreWorkflowTimerStore(
        WorkflowDbContext context,
        ILogger<EfCoreWorkflowTimerStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ScheduleAsync(WorkflowTimer timer)
    {
        var entity = new WorkflowTimerEntity
        {
            Id = timer.Id,
            InstanceId = timer.InstanceId,
            BookmarkName = timer.BookmarkName,
            FireAt = timer.FireAt,
            CronExpression = timer.CronExpression,
            IsTriggered = timer.IsTriggered,
            TriggeredAt = timer.TriggeredAt
        };

        _context.Timers.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogDebug("Scheduled timer {TimerId} for workflow {InstanceId} at {FireAt}",
            timer.Id, timer.InstanceId, timer.FireAt);
    }

    public async Task<IList<WorkflowTimer>> GetDueTimersAsync(DateTime until)
    {
        var entities = await _context.Timers
            .AsNoTracking()
            .Where(t => !t.IsTriggered && t.FireAt <= until)
            .ToListAsync();

        return entities.Select(e => new WorkflowTimer
        {
            Id = e.Id,
            InstanceId = e.InstanceId,
            BookmarkName = e.BookmarkName,
            FireAt = e.FireAt,
            CronExpression = e.CronExpression,
            IsTriggered = e.IsTriggered,
            TriggeredAt = e.TriggeredAt
        }).ToList();
    }

    public async Task MarkTriggeredAsync(Guid timerId)
    {
        await _context.Timers
            .Where(t => t.Id == timerId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.IsTriggered, true)
                .SetProperty(t => t.TriggeredAt, DateTime.UtcNow));
    }

    public async Task CancelAsync(Guid instanceId, string? bookmarkName = null)
    {
        var query = _context.Timers.Where(t => t.InstanceId == instanceId);

        if (!string.IsNullOrEmpty(bookmarkName))
            query = query.Where(t => t.BookmarkName == bookmarkName);

        await query.ExecuteDeleteAsync();
    }
}
