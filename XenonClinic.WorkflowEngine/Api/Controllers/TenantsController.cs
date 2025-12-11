using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.WorkflowEngine.Application.Services;

namespace XenonClinic.WorkflowEngine.Api.Controllers;

/// <summary>
/// API controller for tenant management operations.
/// </summary>
[ApiController]
[Route("api/workflow/tenants")]
[Authorize(Policy = "WorkflowAdmin")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Gets the current tenant context.
    /// </summary>
    [HttpGet("current")]
    [AllowAnonymous]
    public ActionResult<TenantContext> GetCurrentTenant()
    {
        var tenant = _tenantService.GetCurrentTenant();
        return Ok(tenant);
    }

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    [HttpGet("{tenantId}")]
    public async Task<ActionResult<TenantInfo>> GetTenant(
        string tenantId,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.GetTenantAsync(tenantId, cancellationToken);
        if (tenant == null)
        {
            return NotFound(new { message = $"Tenant '{tenantId}' not found" });
        }
        return Ok(tenant);
    }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TenantInfo>> CreateTenant(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.CreateTenantAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTenant), new { tenantId = tenant.Id }, tenant);
    }

    /// <summary>
    /// Updates tenant settings.
    /// </summary>
    [HttpPut("{tenantId}")]
    public async Task<ActionResult<TenantInfo>> UpdateTenant(
        string tenantId,
        [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.UpdateTenantAsync(tenantId, request, cancellationToken);
        return Ok(tenant);
    }

    /// <summary>
    /// Deletes a tenant.
    /// </summary>
    [HttpDelete("{tenantId}")]
    public async Task<IActionResult> DeleteTenant(
        string tenantId,
        CancellationToken cancellationToken)
    {
        await _tenantService.DeleteTenantAsync(tenantId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Lists all tenants.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IList<TenantInfo>>> ListTenants(
        CancellationToken cancellationToken)
    {
        var tenants = await _tenantService.ListTenantsAsync(cancellationToken);
        return Ok(tenants);
    }

    /// <summary>
    /// Gets tenant usage statistics.
    /// </summary>
    [HttpGet("{tenantId}/usage")]
    public async Task<ActionResult<TenantUsage>> GetTenantUsage(
        string tenantId,
        CancellationToken cancellationToken)
    {
        var usage = await _tenantService.GetTenantUsageAsync(tenantId, cancellationToken);
        return Ok(usage);
    }

    /// <summary>
    /// Updates tenant limits.
    /// </summary>
    [HttpPut("{tenantId}/limits")]
    public async Task<ActionResult<TenantInfo>> UpdateTenantLimits(
        string tenantId,
        [FromBody] TenantLimits limits,
        CancellationToken cancellationToken)
    {
        var request = new UpdateTenantRequest { Limits = limits };
        var tenant = await _tenantService.UpdateTenantAsync(tenantId, request, cancellationToken);
        return Ok(tenant);
    }

    /// <summary>
    /// Suspends a tenant.
    /// </summary>
    [HttpPost("{tenantId}/suspend")]
    public async Task<IActionResult> SuspendTenant(
        string tenantId,
        CancellationToken cancellationToken)
    {
        await _tenantService.SuspendTenantAsync(tenantId, cancellationToken);
        return Ok(new { message = "Tenant suspended" });
    }

    /// <summary>
    /// Activates a suspended tenant.
    /// </summary>
    [HttpPost("{tenantId}/activate")]
    public async Task<IActionResult> ActivateTenant(
        string tenantId,
        CancellationToken cancellationToken)
    {
        await _tenantService.ActivateTenantAsync(tenantId, cancellationToken);
        return Ok(new { message = "Tenant activated" });
    }
}
