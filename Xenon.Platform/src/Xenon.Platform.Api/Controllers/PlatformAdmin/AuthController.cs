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
}
