using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.Controllers.PlatformAdmin;

[ApiController]
[Route("api/platform-admin/auth")]
public class AuthController : ControllerBase
{
    private readonly PlatformDbContext _context;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHashingService _passwordService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        PlatformDbContext context,
        IJwtTokenService jwtService,
        IPasswordHashingService passwordService,
        IAuditService auditService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Platform admin login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
        var admin = await _context.PlatformAdmins
            .FirstOrDefaultAsync(a => a.Email.ToLower() == request.Email.ToLower());

        if (admin == null)
        {
            return Unauthorized(new { success = false, error = "Invalid email or password" });
        }

        if (admin.IsLockedOut)
        {
            return Unauthorized(new { success = false, error = "Account is locked. Please try again later." });
        }

        if (!admin.IsActive)
        {
            return Unauthorized(new { success = false, error = "Account is disabled" });
        }

        if (!_passwordService.VerifyPassword(request.Password, admin.PasswordHash))
        {
            admin.FailedLoginAttempts++;
            if (admin.FailedLoginAttempts >= 5)
            {
                admin.LockoutEndAt = DateTime.UtcNow.AddMinutes(30);
            }
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("AdminLoginFailed", "PlatformAdmin", admin.Id,
                newValues: new { admin.Email, admin.FailedLoginAttempts },
                isSuccess: false, errorMessage: "Invalid password");

            return Unauthorized(new { success = false, error = "Invalid email or password" });
        }

        // Reset failed attempts
        admin.FailedLoginAttempts = 0;
        admin.LockoutEndAt = null;
        admin.LastLoginAt = DateTime.UtcNow;
        admin.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _context.SaveChangesAsync();

        var token = _jwtService.GeneratePlatformAdminToken(admin);

        await _auditService.LogAsync("AdminLoginSuccess", "PlatformAdmin", admin.Id,
            newValues: new { admin.Email });

        return Ok(new
        {
            success = true,
            data = new
            {
                user = new
                {
                    admin.Id,
                    admin.Email,
                    admin.FirstName,
                    admin.LastName,
                    admin.Role,
                    permissions = admin.GetPermissions()
                },
                token
            }
        });
    }
}

public class AdminLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
