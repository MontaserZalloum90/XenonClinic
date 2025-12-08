using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Interface for audit logging operations
/// </summary>
public interface IAuditService
{
    Task LogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task LogAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        string? description = null,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<AuditLog?> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Audit service implementation for compliance logging
/// </summary>
public class AuditService : IAuditService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<AuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        ClinicDbContext context,
        ILogger<AuditService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Log an audit entry
    /// </summary>
    public async Task LogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        try
        {
            // Enrich with HTTP context if available
            EnrichFromHttpContext(auditLog);

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug(
                "Audit log created: {Action} on {EntityType}:{EntityId} by {UserId}",
                auditLog.Action,
                auditLog.EntityType,
                auditLog.EntityId,
                auditLog.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log: {Action}", auditLog.Action);
            // Don't throw - audit logging should not break the main flow
        }
    }

    /// <summary>
    /// Log an audit entry with simplified parameters
    /// </summary>
    public async Task LogAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        string? description = null,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            OldValues = oldValues != null ? System.Text.Json.JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? System.Text.Json.JsonSerializer.Serialize(newValues) : null,
            Timestamp = DateTime.UtcNow
        };

        await LogAsync(auditLog, cancellationToken);
    }

    /// <summary>
    /// Get paginated audit logs with optional filters
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(a => a.UserId == userId);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get a specific audit log by ID
    /// </summary>
    public async Task<AuditLog?> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Get complete history for a specific entity
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Enrich audit log with information from HTTP context
    /// </summary>
    private void EnrichFromHttpContext(AuditLog auditLog)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        // Correlation ID
        if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
        {
            auditLog.CorrelationId = correlationId?.ToString();
        }

        // User information
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            auditLog.UserId ??= httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            auditLog.UserName ??= httpContext.User.Identity.Name;
            auditLog.UserEmail ??= httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }

        // Request information
        auditLog.IpAddress ??= GetClientIpAddress(httpContext);
        auditLog.UserAgent ??= httpContext.Request.Headers.UserAgent.ToString();
        auditLog.HttpMethod ??= httpContext.Request.Method;
        auditLog.RequestPath ??= httpContext.Request.Path.ToString();

        // Multi-tenancy context
        if (httpContext.Items.TryGetValue("TenantId", out var tenantId) && tenantId is int tid)
        {
            auditLog.TenantId ??= tid;
        }

        if (httpContext.Items.TryGetValue("CompanyId", out var companyId) && companyId is int cid)
        {
            auditLog.CompanyId ??= cid;
        }

        if (httpContext.Items.TryGetValue("BranchId", out var branchId) && branchId is int bid)
        {
            auditLog.BranchId ??= bid;
        }
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

/// <summary>
/// Extension methods for audit logging on entities
/// </summary>
public static class AuditServiceExtensions
{
    public static async Task LogCreateAsync<T>(
        this IAuditService auditService,
        T entity,
        string? description = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;
        var entityId = GetEntityId(entity);

        await auditService.LogAsync(
            AuditActions.Create,
            entityType,
            entityId,
            description ?? $"Created {entityType}",
            null,
            entity,
            cancellationToken);
    }

    public static async Task LogUpdateAsync<T>(
        this IAuditService auditService,
        T oldEntity,
        T newEntity,
        string? description = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;
        var entityId = GetEntityId(newEntity);

        await auditService.LogAsync(
            AuditActions.Update,
            entityType,
            entityId,
            description ?? $"Updated {entityType}",
            oldEntity,
            newEntity,
            cancellationToken);
    }

    public static async Task LogDeleteAsync<T>(
        this IAuditService auditService,
        T entity,
        string? description = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;
        var entityId = GetEntityId(entity);

        await auditService.LogAsync(
            AuditActions.Delete,
            entityType,
            entityId,
            description ?? $"Deleted {entityType}",
            entity,
            null,
            cancellationToken);
    }

    public static async Task LogAccessAsync(
        this IAuditService auditService,
        string entityType,
        string entityId,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogAsync(
            AuditActions.Read,
            entityType,
            entityId,
            description ?? $"Accessed {entityType}",
            null,
            null,
            cancellationToken);
    }

    private static string? GetEntityId<T>(T entity) where T : class
    {
        // Try to get Id property
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null)
        {
            return idProperty.GetValue(entity)?.ToString();
        }

        return null;
    }
}
