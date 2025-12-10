using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;

namespace Xenon.Platform.Api.Controllers.Tenant;

[ApiController]
[Route("api/tenant/license")]
[Authorize(AuthenticationSchemes = "TenantScheme")]
public class LicenseController : ControllerBase
{
    private readonly ILicenseService _licenseService;

    public LicenseController(ILicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    /// <summary>
    /// Get license limits and current usage for the tenant (used by ERP)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLicense()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var result = await _licenseService.GetLicenseAsync(tenantId);

        if (result.IsFailure)
        {
            return NotFound(new { success = false, error = result.Error });
        }

        return Ok(new { success = true, data = result.Value });
    }

    /// <summary>
    /// Update usage counters (called by ERP to report current usage)
    /// </summary>
    [HttpPost("usage")]
    public async Task<IActionResult> UpdateUsage([FromBody] UsageUpdateRequest request)
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var result = await _licenseService.UpdateUsageAsync(tenantId, request);

        if (result.IsFailure)
        {
            return NotFound(new { success = false, error = result.Error });
        }

        return Ok(new { success = true, data = result.Value });
    }
}
