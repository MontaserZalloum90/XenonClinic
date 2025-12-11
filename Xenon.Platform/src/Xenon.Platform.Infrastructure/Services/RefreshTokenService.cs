using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Persistence;

namespace Xenon.Platform.Infrastructure.Services;

/// <summary>
/// Service for managing refresh tokens with security best practices:
/// - Token rotation
/// - Family-based revocation
/// - Device fingerprinting
/// </summary>
public interface IRefreshTokenService
{
    Task<(string token, RefreshToken entity)> GenerateRefreshTokenAsync(
        string tokenType,
        Guid userId,
        Guid? tenantId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? deviceFingerprint = null);

    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);

    Task<(string newToken, RefreshToken newEntity)?> RotateRefreshTokenAsync(
        string oldToken,
        string? ipAddress = null);

    Task RevokeTokenAsync(Guid tokenId, string reason);
    Task RevokeAllUserTokensAsync(Guid userId, string tokenType, string reason);
    Task RevokeTokenFamilyAsync(Guid tokenId, string reason);
    Task CleanupExpiredTokensAsync();
    Task<int> GetActiveTokenCountAsync(Guid userId, string tokenType);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly PlatformDbContext _context;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly int _tokenExpiryDays;
    private readonly int _maxActiveTokensPerUser;

    public RefreshTokenService(
        PlatformDbContext context,
        IConfiguration configuration,
        ILogger<RefreshTokenService> logger)
    {
        _context = context;
        _logger = logger;
        _tokenExpiryDays = int.Parse(configuration["Security:RefreshTokenExpiryDays"] ?? "30");
        _maxActiveTokensPerUser = int.Parse(configuration["Security:MaxActiveTokensPerUser"] ?? "5");
    }

    public async Task<(string token, RefreshToken entity)> GenerateRefreshTokenAsync(
        string tokenType,
        Guid userId,
        Guid? tenantId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? deviceFingerprint = null)
    {
        // Generate a secure random token
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(tokenBytes);

        // Hash the token for storage
        var tokenHash = HashToken(token);

        var refreshToken = new RefreshToken
        {
            TokenHash = tokenHash,
            TokenType = tokenType,
            UserId = userId,
            TenantId = tenantId,
            ExpiresAt = DateTime.UtcNow.AddDays(_tokenExpiryDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            DeviceFingerprint = deviceFingerprint
        };

        _context.RefreshTokens.Add(refreshToken);

        // Enforce max active tokens per user
        await EnforceMaxActiveTokensAsync(userId, tokenType);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Generated refresh token for {TokenType} user {UserId}",
            tokenType, userId);

        return (token, refreshToken);
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
    {
        var tokenHash = HashToken(token);

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token not found");
            return null;
        }

        if (!refreshToken.IsActive)
        {
            // Potential token reuse attack - revoke the entire token family
            if (refreshToken.IsRevoked)
            {
                _logger.LogWarning(
                    "Attempted reuse of revoked refresh token {TokenId}. Revoking token family.",
                    refreshToken.Id);

                await RevokeTokenFamilyAsync(refreshToken.Id, "Token reuse detected - security breach");
            }

            return null;
        }

        // BUG FIX: Check if user has invalidated all tokens after this token was created
        // This ensures that calling InvalidateAllTokensAsync actually invalidates refresh tokens
        var tokenInvalidatedAt = await GetUserTokenInvalidatedAtAsync(refreshToken.UserId, refreshToken.TokenType);
        if (tokenInvalidatedAt.HasValue && refreshToken.CreatedAt < tokenInvalidatedAt.Value)
        {
            _logger.LogWarning(
                "Refresh token {TokenId} was created before user's token invalidation time. Token is invalid.",
                refreshToken.Id);

            // Revoke this token since it's been invalidated
            refreshToken.Revoke("Token invalidated by user");
            await _context.SaveChangesAsync();

            return null;
        }

        return refreshToken;
    }

    /// <summary>
    /// BUG FIX: Helper method to get user's TokenInvalidatedAt timestamp based on token type.
    /// This ensures refresh tokens respect the token invalidation mechanism.
    /// </summary>
    private async Task<DateTime?> GetUserTokenInvalidatedAtAsync(Guid userId, string tokenType)
    {
        if (tokenType == RefreshTokenTypes.PlatformAdmin)
        {
            var admin = await _context.PlatformAdmins
                .AsNoTracking()
                .Where(a => a.Id == userId)
                .Select(a => a.TokenInvalidatedAt)
                .FirstOrDefaultAsync();
            return admin;
        }
        else if (tokenType == RefreshTokenTypes.TenantAdmin)
        {
            var admin = await _context.TenantAdmins
                .AsNoTracking()
                .Where(a => a.Id == userId)
                .Select(a => a.TokenInvalidatedAt)
                .FirstOrDefaultAsync();
            return admin;
        }

        return null;
    }

    public async Task<(string newToken, RefreshToken newEntity)?> RotateRefreshTokenAsync(
        string oldToken,
        string? ipAddress = null)
    {
        var oldRefreshToken = await ValidateRefreshTokenAsync(oldToken);

        if (oldRefreshToken == null)
        {
            return null;
        }

        // Generate new token
        var (newToken, newEntity) = await GenerateRefreshTokenAsync(
            oldRefreshToken.TokenType,
            oldRefreshToken.UserId,
            oldRefreshToken.TenantId,
            ipAddress,
            oldRefreshToken.UserAgent,
            oldRefreshToken.DeviceFingerprint);

        // Revoke old token
        oldRefreshToken.Revoke("Token rotated", newEntity.Id);
        oldRefreshToken.LastUsedAt = DateTime.UtcNow;
        oldRefreshToken.LastUsedByIp = ipAddress;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Rotated refresh token {OldTokenId} to {NewTokenId} for user {UserId}",
            oldRefreshToken.Id, newEntity.Id, oldRefreshToken.UserId);

        return (newToken, newEntity);
    }

    public async Task RevokeTokenAsync(Guid tokenId, string reason)
    {
        var token = await _context.RefreshTokens.FindAsync(tokenId);

        if (token != null && !token.IsRevoked)
        {
            token.Revoke(reason);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Revoked refresh token {TokenId} for user {UserId}. Reason: {Reason}",
                tokenId, token.UserId, reason);
        }
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, string tokenType, string reason)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.TokenType == tokenType && !t.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revoke(reason);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Revoked all {Count} refresh tokens for {TokenType} user {UserId}. Reason: {Reason}",
            tokens.Count, tokenType, userId, reason);
    }

    public async Task RevokeTokenFamilyAsync(Guid tokenId, string reason)
    {
        // Find the root token in the family
        var currentToken = await _context.RefreshTokens.FindAsync(tokenId);

        if (currentToken == null)
            return;

        // Trace back to the root
        var rootId = currentToken.Id;
        var visitedIds = new HashSet<Guid> { rootId };

        // Find all tokens that were replaced by this one
        var parentToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.ReplacedByTokenId == rootId);

        while (parentToken != null && !visitedIds.Contains(parentToken.Id))
        {
            visitedIds.Add(parentToken.Id);
            rootId = parentToken.Id;
            parentToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.ReplacedByTokenId == parentToken.Id);
        }

        // Now revoke all descendants from the root
        await RevokeTokenAndDescendantsAsync(rootId, reason, visitedIds);
    }

    private async Task RevokeTokenAndDescendantsAsync(Guid tokenId, string reason, HashSet<Guid> visited)
    {
        var token = await _context.RefreshTokens.FindAsync(tokenId);

        if (token == null || visited.Contains(tokenId))
            return;

        visited.Add(tokenId);

        if (!token.IsRevoked)
        {
            token.Revoke(reason);
        }

        // Find and revoke all child tokens
        var childToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Id == token.ReplacedByTokenId);

        if (childToken != null && !visited.Contains(childToken.Id))
        {
            await RevokeTokenAndDescendantsAsync(childToken.Id, reason, visited);
        }

        await _context.SaveChangesAsync();
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-7); // Keep expired tokens for 7 days for audit

        var expiredTokens = await _context.RefreshTokens
            .Where(t => t.ExpiresAt < cutoffDate || (t.IsRevoked && t.RevokedAt < cutoffDate))
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();

        if (expiredTokens.Count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired refresh tokens", expiredTokens.Count);
        }
    }

    public async Task<int> GetActiveTokenCountAsync(Guid userId, string tokenType)
    {
        return await _context.RefreshTokens
            .CountAsync(t => t.UserId == userId &&
                            t.TokenType == tokenType &&
                            !t.IsRevoked &&
                            t.ExpiresAt > DateTime.UtcNow);
    }

    private async Task EnforceMaxActiveTokensAsync(Guid userId, string tokenType)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId &&
                       t.TokenType == tokenType &&
                       !t.IsRevoked &&
                       t.ExpiresAt > DateTime.UtcNow)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        // Remove oldest tokens if over limit
        var tokensToRevoke = activeTokens.Count - _maxActiveTokensPerUser + 1;

        if (tokensToRevoke > 0)
        {
            foreach (var token in activeTokens.Take(tokensToRevoke))
            {
                token.Revoke("Max active sessions exceeded");
            }
        }
    }

    private static string HashToken(string token)
    {
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
