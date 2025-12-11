using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;

namespace Xenon.Platform.Api.Controllers.PlatformAdmin;

[ApiController]
[Route("api/platform-admin/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IPlatformAuthService _authService;

    public AuthController(IPlatformAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Platform admin login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
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
                user = result.Value.User,
                token = result.Value.Token
            }
        });
    }

    /// <summary>
    /// Platform admin logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
        {
            return Unauthorized(new { success = false, error = "Invalid token" });
        }

        var result = await _authService.LogoutAsync(adminId);

        if (result.IsFailure)
        {
            return BadRequest(new { success = false, error = result.Error });
        }

        return Ok(new { success = true, message = "Logged out successfully" });
    }
}
