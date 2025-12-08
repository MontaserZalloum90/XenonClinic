using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Services;

namespace XenonClinic.Web.Controllers.Api;

/// <summary>
/// API controller for accessing audit logs (compliance/admin only)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[EnableRateLimiting(RateLimitingConfiguration.SensitivePolicy)]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated audit logs with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AuditLogListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? userId,
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure reasonable page size
            pageSize = Math.Min(pageSize, 100);

            var logs = await _auditService.GetAuditLogsAsync(
                fromDate,
                toDate,
                userId,
                entityType,
                action,
                page,
                pageSize,
                cancellationToken);

            return Ok(new AuditLogListResponse
            {
                Data = logs,
                Page = page,
                PageSize = pageSize,
                Filters = new AuditLogFilters
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    UserId = userId,
                    EntityType = entityType,
                    Action = action
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, new { Message = "Error retrieving audit logs" });
        }
    }

    /// <summary>
    /// Get a specific audit log by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuditLog), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLog(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var log = await _auditService.GetAuditLogByIdAsync(id, cancellationToken);

            if (log == null)
            {
                return NotFound(new { Message = $"Audit log {id} not found" });
            }

            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log {Id}", id);
            return StatusCode(500, new { Message = "Error retrieving audit log" });
        }
    }

    /// <summary>
    /// Get complete history for a specific entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(IEnumerable<AuditLog>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityHistory(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _auditService.GetEntityHistoryAsync(entityType, entityId, cancellationToken);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity history for {EntityType}:{EntityId}", entityType, entityId);
            return StatusCode(500, new { Message = "Error retrieving entity history" });
        }
    }

    /// <summary>
    /// Get audit log statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AuditStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditStats(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            fromDate ??= DateTime.UtcNow.AddDays(-7);
            toDate ??= DateTime.UtcNow;

            var logs = await _auditService.GetAuditLogsAsync(
                fromDate,
                toDate,
                pageSize: 10000, // Get all for stats
                cancellationToken: cancellationToken);

            var logsList = logs.ToList();

            var stats = new AuditStatsResponse
            {
                TotalEntries = logsList.Count,
                FromDate = fromDate.Value,
                ToDate = toDate.Value,
                ByAction = logsList.GroupBy(l => l.Action).ToDictionary(g => g.Key, g => g.Count()),
                ByEntityType = logsList.Where(l => l.EntityType != null).GroupBy(l => l.EntityType!).ToDictionary(g => g.Key, g => g.Count()),
                ByUser = logsList.Where(l => l.UserId != null).GroupBy(l => l.UserId!).ToDictionary(g => g.Key, g => g.Count()),
                FailedOperations = logsList.Count(l => !l.IsSuccess),
                UniqueUsers = logsList.Where(l => l.UserId != null).Select(l => l.UserId).Distinct().Count()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit statistics");
            return StatusCode(500, new { Message = "Error retrieving audit statistics" });
        }
    }
}

/// <summary>
/// Response model for paginated audit logs
/// </summary>
public class AuditLogListResponse
{
    public IEnumerable<AuditLog> Data { get; set; } = Enumerable.Empty<AuditLog>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public AuditLogFilters? Filters { get; set; }
}

/// <summary>
/// Applied filters for audit log query
/// </summary>
public class AuditLogFilters
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? UserId { get; set; }
    public string? EntityType { get; set; }
    public string? Action { get; set; }
}

/// <summary>
/// Response model for audit statistics
/// </summary>
public class AuditStatsResponse
{
    public int TotalEntries { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Dictionary<string, int> ByAction { get; set; } = new();
    public Dictionary<string, int> ByEntityType { get; set; } = new();
    public Dictionary<string, int> ByUser { get; set; } = new();
    public int FailedOperations { get; set; }
    public int UniqueUsers { get; set; }
}
