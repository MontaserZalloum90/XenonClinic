using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;

namespace Xenon.Platform.Api.Controllers.Tenant;

[ApiController]
[Route("api/tenant/usage")]
[Authorize(AuthenticationSchemes = "TenantScheme")]
public class UsageController : ControllerBase
{
    private readonly IUsageService _usageService;

    public UsageController(IUsageService usageService)
    {
        _usageService = usageService;
    }

    /// <summary>
    /// Get usage metrics for the tenant
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsage([FromQuery] int days = 30)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var result = await _usageService.GetUsageAsync(tenantId, days);

        if (result.IsFailure)
        {
            return NotFound(new { success = false, error = result.Error });
        }

        return Ok(new
        {
            success = true,
            data = result.Value
        });
    }

    /// <summary>
    /// Report usage snapshot (called by ERP periodically)
    /// </summary>
    [HttpPost("report")]
    public async Task<IActionResult> ReportUsage([FromBody] UsageReportRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var result = await _usageService.ReportUsageAsync(tenantId, request);

        if (result.IsFailure)
        {
            return BadRequest(new { success = false, error = result.Error });
        }

        return Ok(new { success = true, snapshotId = result.Value.SnapshotId });
    }
}
