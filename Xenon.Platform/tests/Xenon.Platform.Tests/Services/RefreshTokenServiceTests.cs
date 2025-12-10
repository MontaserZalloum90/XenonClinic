using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;
using Xunit;

namespace Xenon.Platform.Tests.Services;

/// <summary>
/// Tests for RefreshTokenService - validates token generation, validation,
/// rotation, and revocation with security best practices.
/// </summary>
public class RefreshTokenServiceTests : IDisposable
{
    private readonly PlatformDbContext _context;
    private readonly Mock<ILogger<RefreshTokenService>> _loggerMock;
    private readonly IRefreshTokenService _service;

    public RefreshTokenServiceTests()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PlatformDbContext(options);
        _loggerMock = new Mock<ILogger<RefreshTokenService>>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Security:RefreshTokenExpiryDays"] = "30",
                ["Security:MaxActiveTokensPerUser"] = "5"
            })
            .Build();

        _service = new RefreshTokenService(_context, configuration, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Token Generation Tests

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldCreateToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenType = RefreshTokenTypes.PlatformAdmin;

        // Act
        var (token, entity) = await _service.GenerateRefreshTokenAsync(
            tokenType, userId, null, "192.168.1.1", "Mozilla/5.0");

        // Assert
        token.Should().NotBeNullOrEmpty();
        entity.Should().NotBeNull();
        entity.UserId.Should().Be(userId);
        entity.TokenType.Should().Be(tokenType);
        entity.CreatedByIp.Should().Be("192.168.1.1");
        entity.UserAgent.Should().Be("Mozilla/5.0");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldSetCorrectExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var (_, entity) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.TenantAdmin, userId);

        // Assert
        entity.ExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(30),
            TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldStoreHashedToken()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var (token, entity) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Assert
        entity.TokenHash.Should().NotBe(token);
        entity.TokenHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldPersistToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var (_, entity) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Assert
        var storedToken = await _context.RefreshTokens.FindAsync(entity.Id);
        storedToken.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ShouldEnforceMaxActiveTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Generate 6 tokens (max is 5)
        for (int i = 0; i < 6; i++)
        {
            await _service.GenerateRefreshTokenAsync(
                RefreshTokenTypes.PlatformAdmin, userId);
        }

        // Assert
        var activeTokenCount = await _service.GetActiveTokenCountAsync(
            userId, RefreshTokenTypes.PlatformAdmin);

        activeTokenCount.Should().BeLessOrEqualTo(5);
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public async Task ValidateRefreshTokenAsync_ShouldReturnToken_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (token, _) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Act
        var result = await _service.ValidateRefreshTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ShouldReturnNull_WhenTokenNotFound()
    {
        // Act
        var result = await _service.ValidateRefreshTokenAsync("invalid-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ShouldReturnNull_WhenTokenRevoked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (token, entity) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        await _service.RevokeTokenAsync(entity.Id, "Test revocation");

        // Act
        var result = await _service.ValidateRefreshTokenAsync(token);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ShouldReturnNull_WhenTokenExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (token, entity) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Manually expire the token
        entity.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ValidateRefreshTokenAsync(token);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Token Rotation Tests

    [Fact]
    public async Task RotateRefreshTokenAsync_ShouldGenerateNewToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (oldToken, _) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Act
        var result = await _service.RotateRefreshTokenAsync(oldToken, "192.168.1.2");

        // Assert
        result.Should().NotBeNull();
        result!.Value.newToken.Should().NotBe(oldToken);
        result.Value.newEntity.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_ShouldRevokeOldToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (oldToken, oldEntity) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Act
        await _service.RotateRefreshTokenAsync(oldToken);

        // Assert
        var updatedOldToken = await _context.RefreshTokens.FindAsync(oldEntity.Id);
        updatedOldToken!.IsRevoked.Should().BeTrue();
        updatedOldToken.RevokedReason.Should().Be("Token rotated");
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_ShouldLinkOldToNewToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (oldToken, oldEntity) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Act
        var result = await _service.RotateRefreshTokenAsync(oldToken);

        // Assert
        var updatedOldToken = await _context.RefreshTokens.FindAsync(oldEntity.Id);
        updatedOldToken!.ReplacedByTokenId.Should().Be(result!.Value.newEntity.Id);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_ShouldReturnNull_WhenTokenInvalid()
    {
        // Act
        var result = await _service.RotateRefreshTokenAsync("invalid-token");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Token Revocation Tests

    [Fact]
    public async Task RevokeTokenAsync_ShouldRevokeToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (_, entity) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Act
        await _service.RevokeTokenAsync(entity.Id, "Manual revocation");

        // Assert
        var revokedToken = await _context.RefreshTokens.FindAsync(entity.Id);
        revokedToken!.IsRevoked.Should().BeTrue();
        revokedToken.RevokedAt.Should().NotBeNull();
        revokedToken.RevokedReason.Should().Be("Manual revocation");
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_ShouldRevokeAllTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Create multiple tokens
        await _service.GenerateRefreshTokenAsync(RefreshTokenTypes.PlatformAdmin, userId);
        await _service.GenerateRefreshTokenAsync(RefreshTokenTypes.PlatformAdmin, userId);
        await _service.GenerateRefreshTokenAsync(RefreshTokenTypes.PlatformAdmin, userId);

        // Act
        await _service.RevokeAllUserTokensAsync(userId, RefreshTokenTypes.PlatformAdmin, "Logout all");

        // Assert
        var activeCount = await _service.GetActiveTokenCountAsync(userId, RefreshTokenTypes.PlatformAdmin);
        activeCount.Should().Be(0);
    }

    [Fact]
    public async Task RevokeTokenFamilyAsync_ShouldRevokeRelatedTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (token1, entity1) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Rotate to create a chain
        var result2 = await _service.RotateRefreshTokenAsync(token1);
        var result3 = await _service.RotateRefreshTokenAsync(result2!.Value.newToken);

        // Act - Revoke from the first token
        await _service.RevokeTokenFamilyAsync(entity1.Id, "Security breach");

        // Assert - All tokens in the family should be revoked
        var activeCount = await _service.GetActiveTokenCountAsync(userId, RefreshTokenTypes.PlatformAdmin);
        activeCount.Should().Be(0);
    }

    #endregion

    #region Cleanup Tests

    [Fact]
    public async Task CleanupExpiredTokensAsync_ShouldRemoveOldTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var (_, entity) = await _service.GenerateRefreshTokenAsync(
            RefreshTokenTypes.PlatformAdmin, userId);

        // Make token old
        entity.ExpiresAt = DateTime.UtcNow.AddDays(-10);
        await _context.SaveChangesAsync();

        // Act
        await _service.CleanupExpiredTokensAsync();

        // Assert
        var removedToken = await _context.RefreshTokens.FindAsync(entity.Id);
        removedToken.Should().BeNull();
    }

    #endregion
}
