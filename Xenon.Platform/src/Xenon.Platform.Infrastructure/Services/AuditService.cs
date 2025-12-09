using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, Guid? entityId = null,
        object? oldValues = null, object? newValues = null,
        Guid? tenantId = null, bool isSuccess = true, string? errorMessage = null);
}

public class AuditService : IAuditService
{
    private readonly PlatformDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(PlatformDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityType, Guid? entityId = null,
        object? oldValues = null, object? newValues = null,
        Guid? tenantId = null, bool isSuccess = true, string? errorMessage = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            PerformedBy = user?.FindFirst("sub")?.Value,
            PerformedByEmail = user?.FindFirst("email")?.Value,
            PerformedByRole = user?.FindFirst("role")?.Value,
            TenantId = tenantId ?? (Guid.TryParse(user?.FindFirst("tenant_id")?.Value, out var tid) ? tid : null),
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
