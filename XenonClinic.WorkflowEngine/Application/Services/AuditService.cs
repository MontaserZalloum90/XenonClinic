namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Infrastructure.Data;

/// <summary>
/// Service implementation for workflow audit logging.
/// </summary>
public class AuditService : IAuditService
{
    private readonly WorkflowEngineDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuditService(
        WorkflowEngineDbContext context,
        ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task LogAsync(AuditEventDto eventData, CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditEvent
        {
            Id = string.IsNullOrEmpty(eventData.Id) ? Guid.NewGuid().ToString() : eventData.Id,
            TenantId = eventData.TenantId,
            EventType = eventData.EventType,
            EntityType = eventData.EntityType,
            EntityId = eventData.EntityId,
            ProcessInstanceId = eventData.ProcessInstanceId,
            ActivityInstanceId = eventData.ActivityInstanceId,
            UserId = eventData.UserId,
            CorrelationId = eventData.CorrelationId ?? Guid.NewGuid().ToString(),
            Timestamp = eventData.Timestamp == default ? DateTime.UtcNow : eventData.Timestamp,
            OldValuesJson = eventData.OldValues != null
                ? JsonSerializer.Serialize(eventData.OldValues, _jsonOptions)
                : null,
            NewValuesJson = eventData.NewValues != null
                ? JsonSerializer.Serialize(eventData.NewValues, _jsonOptions)
                : null,
            MetadataJson = eventData.Metadata != null
                ? JsonSerializer.Serialize(eventData.Metadata, _jsonOptions)
                : null
        };

        _context.AuditEvents.Add(auditEvent);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Audit event logged: {EventType} for {EntityType}/{EntityId}",
            eventData.EventType, eventData.EntityType, eventData.EntityId);
    }

    public async Task<AuditQueryResult> QueryAsync(AuditQueryDto query, CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.AuditEvents.Where(e => e.TenantId == query.TenantId);

        if (!string.IsNullOrEmpty(query.EntityType))
            dbQuery = dbQuery.Where(e => e.EntityType == query.EntityType);

        if (!string.IsNullOrEmpty(query.EntityId))
            dbQuery = dbQuery.Where(e => e.EntityId == query.EntityId);

        if (query.ProcessInstanceId.HasValue)
            dbQuery = dbQuery.Where(e => e.ProcessInstanceId == query.ProcessInstanceId);

        if (!string.IsNullOrEmpty(query.UserId))
            dbQuery = dbQuery.Where(e => e.UserId == query.UserId);

        if (!string.IsNullOrEmpty(query.EventType))
            dbQuery = dbQuery.Where(e => e.EventType == query.EventType);

        if (!string.IsNullOrEmpty(query.CorrelationId))
            dbQuery = dbQuery.Where(e => e.CorrelationId == query.CorrelationId);

        if (query.From.HasValue)
            dbQuery = dbQuery.Where(e => e.Timestamp >= query.From.Value);

        if (query.To.HasValue)
            dbQuery = dbQuery.Where(e => e.Timestamp <= query.To.Value);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(e => e.Timestamp)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new AuditQueryResult
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<IList<AuditEventDto>> GetProcessAuditTrailAsync(
        Guid processInstanceId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var events = await _context.AuditEvents
            .Where(e => e.TenantId == tenantId && e.ProcessInstanceId == processInstanceId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        return events.Select(MapToDto).ToList();
    }

    public async Task<IList<AuditEventDto>> GetEntityAuditTrailAsync(
        string entityType,
        string entityId,
        int tenantId,
        CancellationToken cancellationToken = default)
    {
        var events = await _context.AuditEvents
            .Where(e => e.TenantId == tenantId &&
                       e.EntityType == entityType &&
                       e.EntityId == entityId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        return events.Select(MapToDto).ToList();
    }

    private AuditEventDto MapToDto(AuditEvent entity)
    {
        return new AuditEventDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            EventType = entity.EventType,
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            ProcessInstanceId = entity.ProcessInstanceId,
            ActivityInstanceId = entity.ActivityInstanceId,
            UserId = entity.UserId,
            CorrelationId = entity.CorrelationId,
            Timestamp = entity.Timestamp,
            OldValues = DeserializeJson(entity.OldValuesJson),
            NewValues = DeserializeJson(entity.NewValuesJson),
            Metadata = DeserializeJson(entity.MetadataJson)
        };
    }

    private Dictionary<string, object>? DeserializeJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
