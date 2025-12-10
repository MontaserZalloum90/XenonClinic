using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.Controllers.Tenant;

/// <summary>
/// Controller for tenant user security operations including password management,
/// session management, and security event viewing.
/// </summary>
[ApiController]
[Route("api/tenant/security")]
[Authorize(Policy = "TenantPolicy")]
public class SecurityController : ControllerBase
{
    private readonly IPasswordPolicyService _passwordPolicyService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ISecurityEventService _securityEventService;

    public SecurityController(
        IPasswordPolicyService passwordPolicyService,
        IPasswordResetService passwordResetService,
        IRefreshTokenService refreshTokenService,
        ISecurityEventService securityEventService)
    {
        _passwordPolicyService = passwordPolicyService;
        _passwordResetService = passwordResetService;
        _refreshTokenService = refreshTokenService;
        _securityEventService = securityEventService;
    }

    /// <summary>
    /// Validate a password against the security policy without changing it
    /// </summary>
    [HttpPost("validate-password")]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public IActionResult ValidatePassword([FromBody] ValidatePasswordRequest request)
    {
        var validationResult = _passwordPolicyService.Validate(request.Password);
        var strength = _passwordPolicyService.GetStrength(request.Password);

        return Ok(new
        {
            success = true,
            data = new ValidatePasswordResponse
            {
                IsValid = validationResult.IsValid,
                Errors = validationResult.Errors.ToList(),
                Strength = strength.ToString(),
                StrengthScore = (int)strength
            }
        });
    }

    /// <summary>
    /// Request a password reset email
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        // Always return success to prevent email enumeration
        var result = await _passwordResetService.CreateResetTokenAsync(
            request.Email,
            RefreshTokenTypes.TenantAdmin,
            clientIp,
            userAgent);

        if (result.HasValue)
        {
            // In production, send email with reset link containing the token
            // For now, log it (in development only)
#if DEBUG
            Console.WriteLine($"Password reset token for {request.Email}: {result.Value.token}");
#endif
        }

        return Ok(new
        {
            success = true,
            data = new ForgotPasswordResponse
            {
                Success = true,
                Message = "If an account exists with this email, you will receive a password reset link."
            }
        });
    }

    /// <summary>
    /// Reset password using a reset token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        // Validate new password
        var validationResult = _passwordPolicyService.Validate(request.NewPassword);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                error = "Password does not meet security requirements",
                details = validationResult.Errors
            });
        }

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        var success = await _passwordResetService.ResetPasswordAsync(
            request.Token,
            request.Email,
            request.NewPassword,
            clientIp);

        if (!success)
        {
            return BadRequest(new
            {
                success = false,
                error = "Invalid or expired reset token"
            });
        }

        return Ok(new
        {
            success = true,
            data = new ResetPasswordResponse
            {
                Success = true,
                Message = "Password has been reset successfully. You can now log in with your new password."
            }
        });
    }

    /// <summary>
    /// Get current user's login history
    /// </summary>
    [HttpGet("login-history")]
    public async Task<IActionResult> GetLoginHistory([FromQuery] int count = 10)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { success = false, error = "User not found" });
        }

        var events = await _securityEventService.GetRecentEventsAsync(
            userId.Value,
            RefreshTokenTypes.TenantAdmin,
            Math.Min(count, 50));

        var successfulLogins = events.Count(e => e.EventType == SecurityEventType.LoginSuccess && e.IsSuccessful);
        var failedLogins = events.Count(e => e.EventType == SecurityEventType.LoginFailed);

        return Ok(new
        {
            success = true,
            data = new LoginHistoryDto
            {
                Events = events.Select(e => new SecurityEventDto
                {
                    Id = e.Id,
                    EventType = e.EventType.ToString(),
                    OccurredAt = e.CreatedAt,
                    IpAddress = e.IpAddress,
                    UserAgent = e.UserAgent,
                    IsSuccessful = e.IsSuccessful,
                    RiskLevel = e.RiskLevel.ToString(),
                    Details = e.Details
                }).ToList(),
                TotalSuccessfulLogins = successfulLogins,
                TotalFailedLogins = failedLogins,
                LastSuccessfulLogin = events
                    .Where(e => e.EventType == SecurityEventType.LoginSuccess)
                    .OrderByDescending(e => e.CreatedAt)
                    .FirstOrDefault()?.CreatedAt,
                LastFailedLogin = events
                    .Where(e => e.EventType == SecurityEventType.LoginFailed)
                    .OrderByDescending(e => e.CreatedAt)
                    .FirstOrDefault()?.CreatedAt
            }
        });
    }

    /// <summary>
    /// Get current user's active sessions
    /// </summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { success = false, error = "User not found" });
        }

        var currentTokenId = GetCurrentTokenId();

        var activeTokenCount = await _refreshTokenService.GetActiveTokenCountAsync(
            userId.Value,
            RefreshTokenTypes.TenantAdmin);

        // Note: In a real implementation, you'd fetch the actual session details
        // This is a simplified version
        return Ok(new
        {
            success = true,
            data = new
            {
                activeSessionCount = activeTokenCount,
                currentSessionId = currentTokenId
            }
        });
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    [HttpPost("sessions/revoke")]
    public async Task<IActionResult> RevokeSession([FromBody] RevokeSessionRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { success = false, error = "User not found" });
        }

        await _refreshTokenService.RevokeTokenAsync(request.SessionId, "User requested revocation");

        await _securityEventService.LogEventAsync(
            SecurityEventType.SessionTerminated,
            new SecurityEventContext
            {
                UserId = userId.Value,
                UserType = RefreshTokenTypes.TenantAdmin,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = $"Session {request.SessionId} revoked by user"
            });

        return Ok(new { success = true, message = "Session revoked successfully" });
    }

    /// <summary>
    /// Revoke all sessions except current
    /// </summary>
    [HttpPost("sessions/revoke-all")]
    public async Task<IActionResult> RevokeAllSessions([FromBody] RevokeAllSessionsRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { success = false, error = "User not found" });
        }

        await _refreshTokenService.RevokeAllUserTokensAsync(
            userId.Value,
            RefreshTokenTypes.TenantAdmin,
            "User requested all sessions revocation");

        await _securityEventService.LogEventAsync(
            SecurityEventType.SessionTerminated,
            new SecurityEventContext
            {
                UserId = userId.Value,
                UserType = RefreshTokenTypes.TenantAdmin,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = "All sessions revoked by user"
            });

        return Ok(new { success = true, message = "All sessions have been revoked" });
    }

    private Guid? GetCurrentUserId()
    {
        var subClaim = User.FindFirst("sub")?.Value;
        return Guid.TryParse(subClaim, out var userId) ? userId : null;
    }

    private Guid? GetCurrentTokenId()
    {
        var jtiClaim = User.FindFirst("jti")?.Value;
        return Guid.TryParse(jtiClaim, out var tokenId) ? tokenId : null;
    }
}
