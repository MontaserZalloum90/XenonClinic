using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xenon.Platform.Application;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

public class PlatformAuthService : IPlatformAuthService
{
    private readonly PlatformDbContext _context;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHashingService _passwordService;
    private readonly IAuditService _auditService;
    private readonly ILogger<PlatformAuthService> _logger;

    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 30;

    public PlatformAuthService(
        PlatformDbContext context,
        IJwtTokenService jwtService,
        IPasswordHashingService passwordService,
        IAuditService auditService,
        ILogger<PlatformAuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<AdminLoginResponse>> LoginAsync(AdminLoginRequest request, string? clientIpAddress = null)
    {
        var admin = await _context.PlatformAdmins
            .FirstOrDefaultAsync(a => a.Email.ToLower() == request.Email.ToLower());

        if (admin == null)
        {
            return "Invalid email or password";
        }

        if (admin.IsLockedOut)
        {
            return "Account is locked. Please try again later.";
        }

        if (!admin.IsActive)
        {
            return "Account is disabled";
        }

        if (!_passwordService.VerifyPassword(request.Password, admin.PasswordHash))
        {
            admin.FailedLoginAttempts++;
            if (admin.FailedLoginAttempts >= MaxFailedAttempts)
            {
                admin.LockoutEndAt = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            }
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("AdminLoginFailed", "PlatformAdmin", admin.Id,
                newValues: new { admin.Email, admin.FailedLoginAttempts },
                isSuccess: false, errorMessage: "Invalid password");

            return "Invalid email or password";
        }

        // Reset failed attempts and update login info
        admin.FailedLoginAttempts = 0;
        admin.LockoutEndAt = null;
        admin.LastLoginAt = DateTime.UtcNow;
        admin.LastLoginIp = clientIpAddress;
        await _context.SaveChangesAsync();

        var token = _jwtService.GeneratePlatformAdminToken(admin);

        await _auditService.LogAsync("AdminLoginSuccess", "PlatformAdmin", admin.Id,
            newValues: new { admin.Email });

        _logger.LogInformation("Platform admin {Email} logged in successfully", admin.Email);

        return new AdminLoginResponse
        {
            User = new AdminUserDto
            {
                Id = admin.Id,
                Email = admin.Email,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                Role = admin.Role,
                Permissions = admin.GetPermissions().ToList()
            },
            Token = token
        };
    }
}
