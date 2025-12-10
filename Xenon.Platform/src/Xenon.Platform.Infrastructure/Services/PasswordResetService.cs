using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

/// <summary>
/// Service for managing password reset tokens and operations.
/// </summary>
public interface IPasswordResetService
{
    Task<(string token, PasswordResetToken entity)?> CreateResetTokenAsync(
        string email,
        string userType,
        string? ipAddress = null,
        string? userAgent = null);

    Task<PasswordResetToken?> ValidateResetTokenAsync(string token, string email);

    Task<bool> ResetPasswordAsync(
        string token,
        string email,
        string newPassword,
        string? ipAddress = null);

    Task InvalidateAllTokensForUserAsync(Guid userId, string userType);
    Task CleanupExpiredTokensAsync();
}

public class PasswordResetService : IPasswordResetService
{
    private readonly PlatformDbContext _context;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IPasswordPolicyService _passwordPolicyService;
    private readonly ISecurityEventService _securityEventService;
    private readonly ILogger<PasswordResetService> _logger;
    private readonly int _tokenExpiryHours;
    private readonly int _maxTokensPerDay;

    public PasswordResetService(
        PlatformDbContext context,
        IPasswordHashingService passwordHashingService,
        IPasswordPolicyService passwordPolicyService,
        ISecurityEventService securityEventService,
        IConfiguration configuration,
        ILogger<PasswordResetService> logger)
    {
        _context = context;
        _passwordHashingService = passwordHashingService;
        _passwordPolicyService = passwordPolicyService;
        _securityEventService = securityEventService;
        _logger = logger;
        _tokenExpiryHours = int.Parse(configuration["Security:PasswordResetExpiryHours"] ?? "24");
        _maxTokensPerDay = int.Parse(configuration["Security:MaxPasswordResetTokensPerDay"] ?? "3");
    }

    public async Task<(string token, PasswordResetToken entity)?> CreateResetTokenAsync(
        string email,
        string userType,
        string? ipAddress = null,
        string? userAgent = null)
    {
        // Find the user
        Guid? userId = null;

        if (userType == RefreshTokenTypes.PlatformAdmin)
        {
            var admin = await _context.PlatformAdmins
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower() && a.IsActive);
            userId = admin?.Id;
        }
        else if (userType == RefreshTokenTypes.TenantAdmin)
        {
            var admin = await _context.TenantAdmins
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower() && a.IsActive);
            userId = admin?.Id;
        }

        if (!userId.HasValue)
        {
            // Don't reveal if user exists - log but return null
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
            return null;
        }

        // Check rate limiting - max tokens per day
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);
        var recentTokenCount = await _context.PasswordResetTokens
            .CountAsync(t => t.UserId == userId.Value &&
                            t.UserType == userType &&
                            t.CreatedAt > oneDayAgo);

        if (recentTokenCount >= _maxTokensPerDay)
        {
            _logger.LogWarning(
                "Password reset rate limit exceeded for user {UserId}. Max {Max} per day.",
                userId, _maxTokensPerDay);

            await _securityEventService.LogEventAsync(
                SecurityEventType.PasswordResetFailed,
                new SecurityEventContext
                {
                    UserId = userId,
                    UserType = userType,
                    Email = email,
                    IpAddress = ipAddress,
                    IsSuccessful = false,
                    ErrorMessage = "Rate limit exceeded"
                });

            return null;
        }

        // Invalidate any existing tokens for this user
        await InvalidateAllTokensForUserAsync(userId.Value, userType);

        // Generate secure token
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var tokenHash = HashToken(token);

        var resetToken = new PasswordResetToken
        {
            TokenHash = tokenHash,
            UserType = userType,
            UserId = userId.Value,
            Email = email.ToLower(),
            ExpiresAt = DateTime.UtcNow.AddHours(_tokenExpiryHours),
            RequestedByIp = ipAddress,
            UserAgent = userAgent
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        await _securityEventService.LogEventAsync(
            SecurityEventType.PasswordResetRequested,
            new SecurityEventContext
            {
                UserId = userId,
                UserType = userType,
                Email = email,
                IpAddress = ipAddress,
                Details = "Password reset token created"
            });

        _logger.LogInformation(
            "Password reset token created for {UserType} user {Email}",
            userType, email);

        return (token, resetToken);
    }

    public async Task<PasswordResetToken?> ValidateResetTokenAsync(string token, string email)
    {
        var tokenHash = HashToken(token);

        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash &&
                                      t.Email == email.ToLower());

        if (resetToken == null)
        {
            _logger.LogWarning("Password reset token not found for email: {Email}", email);
            return null;
        }

        if (!resetToken.IsValid)
        {
            if (resetToken.IsUsed)
            {
                _logger.LogWarning(
                    "Attempted to reuse password reset token for {Email}",
                    email);
            }
            else if (resetToken.IsExpired)
            {
                _logger.LogInformation(
                    "Expired password reset token used for {Email}",
                    email);
            }

            return null;
        }

        return resetToken;
    }

    public async Task<bool> ResetPasswordAsync(
        string token,
        string email,
        string newPassword,
        string? ipAddress = null)
    {
        var resetToken = await ValidateResetTokenAsync(token, email);

        if (resetToken == null)
        {
            await _securityEventService.LogEventAsync(
                SecurityEventType.PasswordResetFailed,
                new SecurityEventContext
                {
                    Email = email,
                    IpAddress = ipAddress,
                    IsSuccessful = false,
                    ErrorMessage = "Invalid or expired token"
                });

            return false;
        }

        // Validate new password
        var validationResult = _passwordPolicyService.Validate(newPassword);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning(
                "Password reset failed due to policy violation for {Email}",
                email);
            return false;
        }

        // Update the password
        var passwordHash = _passwordHashingService.HashPassword(newPassword);
        bool updated = false;

        if (resetToken.UserType == RefreshTokenTypes.PlatformAdmin)
        {
            var admin = await _context.PlatformAdmins.FindAsync(resetToken.UserId);
            if (admin != null)
            {
                admin.PasswordHash = passwordHash;
                admin.FailedLoginAttempts = 0;
                admin.LockoutEndAt = null;
                updated = true;
            }
        }
        else if (resetToken.UserType == RefreshTokenTypes.TenantAdmin)
        {
            var admin = await _context.TenantAdmins.FindAsync(resetToken.UserId);
            if (admin != null)
            {
                admin.PasswordHash = passwordHash;
                admin.FailedLoginAttempts = 0;
                admin.LockoutEndAt = null;
                updated = true;
            }
        }

        if (!updated)
        {
            _logger.LogError(
                "Password reset failed - user not found: {UserId}",
                resetToken.UserId);
            return false;
        }

        // Mark token as used
        resetToken.MarkAsUsed(ipAddress);

        await _context.SaveChangesAsync();

        await _securityEventService.LogEventAsync(
            SecurityEventType.PasswordResetCompleted,
            new SecurityEventContext
            {
                UserId = resetToken.UserId,
                UserType = resetToken.UserType,
                Email = email,
                IpAddress = ipAddress,
                Details = "Password reset completed successfully"
            });

        _logger.LogInformation(
            "Password reset completed for {UserType} user {Email}",
            resetToken.UserType, email);

        return true;
    }

    public async Task InvalidateAllTokensForUserAsync(Guid userId, string userType)
    {
        var activeTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId &&
                       t.UserType == userType &&
                       !t.IsUsed &&
                       t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsDeleted = true;
            token.DeletedAt = DateTime.UtcNow;
        }

        if (activeTokens.Count > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Invalidated {Count} active password reset tokens for user {UserId}",
                activeTokens.Count, userId);
        }
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-7); // Keep for audit

        var expiredTokens = await _context.PasswordResetTokens
            .Where(t => t.ExpiresAt < cutoffDate || (t.IsUsed && t.UsedAt < cutoffDate))
            .ToListAsync();

        _context.PasswordResetTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();

        if (expiredTokens.Count > 0)
        {
            _logger.LogInformation(
                "Cleaned up {Count} expired password reset tokens",
                expiredTokens.Count);
        }
    }

    private static string HashToken(string token)
    {
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
