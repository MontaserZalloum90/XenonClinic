using Xenon.Platform.Application.DTOs;

namespace Xenon.Platform.Application.Interfaces;

/// <summary>
/// Service for platform admin authentication operations
/// </summary>
public interface IPlatformAuthService
{
    /// <summary>
    /// Authenticate a platform admin and return a JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="clientIpAddress">Client IP for audit logging</param>
    /// <returns>Authentication result with token and user info</returns>
    Task<Result<AdminLoginResponse>> LoginAsync(AdminLoginRequest request, string? clientIpAddress = null);

    /// <summary>
    /// Logout a platform admin and invalidate their tokens
    /// </summary>
    /// <param name="adminId">The admin ID from JWT claims</param>
    /// <returns>Success or failure result</returns>
    Task<Result> LogoutAsync(Guid adminId);
}
