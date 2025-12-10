using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xenon.Platform.Infrastructure.Services;
using Xunit;

namespace Xenon.Platform.Tests.Services;

/// <summary>
/// Tests for PasswordHashingService - validates password hashing and verification
/// with proper exception handling for corrupted/invalid hashes.
/// </summary>
public class PasswordHashingServiceTests
{
    private readonly Mock<ILogger<PasswordHashingService>> _loggerMock;
    private readonly PasswordHashingService _service;

    public PasswordHashingServiceTests()
    {
        _loggerMock = new Mock<ILogger<PasswordHashingService>>();
        _service = new PasswordHashingService(_loggerMock.Object);
    }

    #region HashPassword Tests

    [Fact]
    public void HashPassword_ShouldReturnValidBCryptHash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _service.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2"); // BCrypt hash prefix
        hash.Length.Should().Be(60); // BCrypt hash is always 60 chars
    }

    [Fact]
    public void HashPassword_ShouldGenerateUniqueHashes()
    {
        // Arrange
        var password = "SamePassword123!";

        // Act
        var hash1 = _service.HashPassword(password);
        var hash2 = _service.HashPassword(password);

        // Assert - same password should produce different hashes due to salt
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashPassword_ShouldThrowArgumentNullException_WhenPasswordIsNull()
    {
        // Act & Assert
        var act = () => _service.HashPassword(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("VeryLongPasswordWithSpecialChars!@#$%^&*()123456789")]
    [InlineData("Unicode: 密码 пароль")]
    public void HashPassword_ShouldHandleVariousPasswordFormats(string password)
    {
        // Act
        var hash = _service.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().HaveLength(60);
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsNull()
    {
        // Arrange
        var hash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.oMaIJLLmFyMp1G";

        // Act
        var result = _service.VerifyPassword(null!, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsEmpty()
    {
        // Arrange
        var hash = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.oMaIJLLmFyMp1G";

        // Act
        var result = _service.VerifyPassword(string.Empty, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsNull()
    {
        // Arrange
        var password = "SomePassword123!";

        // Act
        var result = _service.VerifyPassword(password, null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsEmpty()
    {
        // Arrange
        var password = "SomePassword123!";

        // Act
        var result = _service.VerifyPassword(password, string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Invalid Hash Format Tests (SaltParseException handling)

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashIsCorrupted()
    {
        // Arrange
        var password = "SomePassword123!";
        var corruptedHash = "$2a$12$INVALID_HASH_FORMAT";

        // Act
        var result = _service.VerifyPassword(password, corruptedHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldLogWarning_WhenHashIsCorrupted()
    {
        // Arrange
        var password = "SomePassword123!";
        var corruptedHash = "$2a$12$CORRUPTED";

        // Act
        _service.VerifyPassword(password, corruptedHash);

        // Assert - verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Theory]
    [InlineData("not-a-hash")]
    [InlineData("$1$invalid$hash")]
    [InlineData("plain_text")]
    [InlineData("12345")]
    public void VerifyPassword_ShouldHandleInvalidHashFormats_Gracefully(string invalidHash)
    {
        // Arrange
        var password = "SomePassword123!";

        // Act
        var result = _service.VerifyPassword(password, invalidHash);

        // Assert - should return false without throwing
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenHashHasInvalidVersion()
    {
        // Arrange - invalid BCrypt version prefix
        var password = "SomePassword123!";
        var invalidVersionHash = "$9z$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.oMaIJLLmFyMp1G";

        // Act
        var result = _service.VerifyPassword(password, invalidVersionHash);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Service Without Logger Tests

    [Fact]
    public void PasswordHashingService_ShouldWorkWithoutLogger()
    {
        // Arrange
        var serviceWithoutLogger = new PasswordHashingService();
        var password = "TestPassword123!";

        // Act
        var hash = serviceWithoutLogger.HashPassword(password);
        var result = serviceWithoutLogger.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldNotThrow_WhenLoggerIsNullAndHashIsCorrupted()
    {
        // Arrange
        var serviceWithoutLogger = new PasswordHashingService();
        var corruptedHash = "$2a$12$CORRUPTED";

        // Act & Assert - should not throw even without logger
        var act = () => serviceWithoutLogger.VerifyPassword("password", corruptedHash);
        act.Should().NotThrow();
    }

    #endregion

    #region End-to-End Tests

    [Fact]
    public void HashAndVerify_ShouldWorkEndToEnd()
    {
        // Arrange
        var password = "MySecurePassword!@#123";

        // Act
        var hash = _service.HashPassword(password);
        var verifyCorrect = _service.VerifyPassword(password, hash);
        var verifyIncorrect = _service.VerifyPassword("WrongPassword", hash);

        // Assert
        verifyCorrect.Should().BeTrue();
        verifyIncorrect.Should().BeFalse();
    }

    [Fact]
    public void MultipleVerifications_ShouldBeConsistent()
    {
        // Arrange
        var password = "ConsistentPassword123!";
        var hash = _service.HashPassword(password);

        // Act - verify multiple times
        var results = Enumerable.Range(0, 10)
            .Select(_ => _service.VerifyPassword(password, hash))
            .ToList();

        // Assert - all should be true
        results.Should().AllBeEquivalentTo(true);
    }

    #endregion
}
