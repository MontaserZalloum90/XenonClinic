using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;

namespace Xenon.Platform.Api.Controllers.PlatformAdmin;

[ApiController]
[Route("api/platform-admin/tenants")]
[Authorize(Policy = "PlatformAdminPolicy")]
public class TenantsAdminController : ControllerBase
{
    private readonly ITenantManagementService _tenantService;

    public TenantsAdminController(ITenantManagementService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Get all tenants with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTenants(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? companyType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _tenantService.GetTenantsAsync(new TenantListQuery
        {
            Search = search,
            Status = status,
            CompanyType = companyType,
            Page = page,
            PageSize = pageSize
        });

        return Ok(new
        {
            success = true,
            data = new
            {
                items = result.Items,
                total = result.Total,
                page = result.Page,
                pageSize = result.PageSize,
                totalPages = result.TotalPages
            }
        });
    }

    /// <summary>
    /// Get tenant details by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTenant(Guid id)
    {
        var result = await _tenantService.GetTenantDetailsAsync(id);

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
    /// Suspend a tenant
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> SuspendTenant(Guid id, [FromBody] SuspendTenantRequest? request)
    {
        var result = await _tenantService.SuspendTenantAsync(id, request ?? new SuspendTenantRequest());

        if (result.IsFailure)
        {
            if (result.Error == "Tenant not found")
            {
                return NotFound(new { success = false, error = result.Error });
            }
            return BadRequest(new { success = false, error = result.Error });
        }

        return Ok(new { success = true, message = "Tenant suspended successfully" });
    }

    /// <summary>
    /// Activate a tenant
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateTenant(Guid id)
    {
        var result = await _tenantService.ActivateTenantAsync(id);

        if (result.IsFailure)
        {
            if (result.Error == "Tenant not found")
            {
                return NotFound(new { success = false, error = result.Error });
            }
            return BadRequest(new { success = false, error = result.Error });
        }

        return Ok(new { success = true, message = "Tenant activated successfully" });
    }

    /// <summary>
    /// Extend trial period
    /// </summary>
    [HttpPost("{id:guid}/extend-trial")]
    public async Task<IActionResult> ExtendTrial(Guid id, [FromBody] ExtendTrialRequest request)
    {
        var result = await _tenantService.ExtendTrialAsync(id, request);

        if (result.IsFailure)
        {
            if (result.Error == "Tenant not found")
            {
                return NotFound(new { success = false, error = result.Error });
            }
            return BadRequest(new { success = false, error = result.Error });
        }

        return Ok(new
        {
            success = true,
            message = $"Trial extended by {request.Days} days",
            data = new { newTrialEndDate = result.Value.NewTrialEndDate }
        });
    }

    /// <summary>
    /// Get tenant usage summary
    /// </summary>
    [HttpGet("{id:guid}/usage")]
    public async Task<IActionResult> GetTenantUsage(Guid id, [FromQuery] int days = 30)
    {
        var result = await _tenantService.GetTenantUsageAsync(id, new TenantUsageQuery { Days = days });

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
}
