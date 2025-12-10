using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;

namespace Xenon.Platform.Api.Controllers.Public;

[ApiController]
[Route("api/public/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantAuthService _authService;

    public TenantsController(ITenantAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Create a new trial tenant (signup)
    /// </summary>
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] TenantSignupRequest request)
    {
        var result = await _authService.SignupAsync(request);

        if (result.IsFailure)
        {
            if (result.Error == "Email already registered")
            {
                return BadRequest(new { success = false, error = result.Error });
            }
            return StatusCode(500, new { success = false, error = result.Error });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                tenant = result.Value.Tenant,
                user = result.Value.User,
                token = result.Value.Token
            }
        });
    }

    /// <summary>
    /// Tenant login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] TenantLoginRequest request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request, clientIp);

        if (result.IsFailure)
        {
            return Unauthorized(new { success = false, error = result.Error });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                tenant = result.Value.Tenant,
                user = result.Value.User,
                token = result.Value.Token
            }
        });
    }

    /// <summary>
    /// Get current tenant context (requires tenant auth)
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "TenantScheme")]
    public async Task<IActionResult> GetCurrentTenant()
    {
        var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantIdClaim) || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var result = await _authService.GetCurrentTenantAsync(tenantId);

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
    /// Check if slug is available
    /// </summary>
    [HttpGet("check-slug")]
    public async Task<IActionResult> CheckSlug([FromQuery] string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest(new { success = false, error = "Slug is required" });
        }

        var isAvailable = await _authService.IsSlugAvailableAsync(slug);

        return Ok(new { success = true, data = new { slug, isAvailable } });
    }
}
