using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Services;

/// <summary>
/// Tests for MfaService.
/// </summary>
public class MfaServiceTests
{
    private readonly Mock<ILogger<MfaService>> _loggerMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly IMemoryCache _cache;
    private readonly MfaService _service;

    public MfaServiceTests()
    {
        _loggerMock = new Mock<ILogger<MfaService>>();
        _smsServiceMock = new Mock<ISmsService>();
        _emailServiceMock = new Mock<IEmailService>();
        _cache = new MemoryCache(new MemoryCacheOptions());

        _service = new MfaService(
            _cache,
            _loggerMock.Object,
            _smsServiceMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task GenerateTotpSecretAsync_ShouldReturnValidResult()
    {
        // Arrange
        var userId = "user123";

        // Act
        var result = await _service.GenerateTotpSecretAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Secret);
        Assert.NotEmpty(result.QrCodeUri);
        Assert.NotEmpty(result.ManualEntryKey);
        Assert.Contains("otpauth://totp/", result.QrCodeUri);
        Assert.Contains("XenonClinic", result.QrCodeUri);
    }

    [Fact]
    public async Task VerifyTotpCodeAsync_ShouldReturnFalse_WhenUserNotSetup()
    {
        // Arrange
        var userId = "nonexistent";
        var code = "123456";

        // Act
        var result = await _service.VerifyTotpCodeAsync(userId, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EnableTotpAsync_ShouldReturnFalse_WhenNoSetupInProgress()
    {
        // Arrange
        var userId = "user123";
        var code = "123456";

        // Act
        var result = await _service.EnableTotpAsync(userId, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SendSmsCodeAsync_ShouldSendSms()
    {
        // Arrange
        var userId = "user123";
        var phoneNumber = "+971501234567";

        // Act
        var code = await _service.SendSmsCodeAsync(userId, phoneNumber);

        // Assert
        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
        Assert.True(code.All(char.IsDigit));
        _smsServiceMock.Verify(s => s.SendAsync(phoneNumber, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailCodeAsync_ShouldSendEmail()
    {
        // Arrange
        var userId = "user123";
        var email = "user@example.com";

        // Act
        var code = await _service.SendEmailCodeAsync(userId, email);

        // Assert
        Assert.NotNull(code);
        Assert.Equal(6, code.Length);
        _emailServiceMock.Verify(s => s.SendAsync(It.Is<EmailMessage>(m => m.To == email)), Times.Once);
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnTrue_WhenCodeMatches()
    {
        // Arrange
        var userId = "user123";
        var phoneNumber = "+971501234567";
        var code = await _service.SendSmsCodeAsync(userId, phoneNumber);

        // Act
        var result = await _service.VerifyCodeAsync(userId, code, MfaCodePurpose.Login);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnFalse_WhenCodeInvalid()
    {
        // Arrange
        var userId = "user123";

        // Act
        var result = await _service.VerifyCodeAsync(userId, "wrongcode", MfaCodePurpose.Login);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GenerateBackupCodesAsync_ShouldGenerateRequestedCount()
    {
        // Arrange
        var userId = "user123";
        var count = 10;

        // Act
        var codes = await _service.GenerateBackupCodesAsync(userId, count);

        // Assert
        Assert.NotNull(codes);
        Assert.Equal(count, codes.Count());
        Assert.All(codes, code => Assert.Equal(10, code.Length)); // 5 bytes = 10 hex chars
    }

    [Fact]
    public async Task VerifyBackupCodeAsync_ShouldReturnTrue_WhenCodeValid()
    {
        // Arrange
        var userId = "user123";
        var codes = (await _service.GenerateBackupCodesAsync(userId, 5)).ToList();
        var codeToVerify = codes[0];

        // Act
        var result = await _service.VerifyBackupCodeAsync(userId, codeToVerify);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyBackupCodeAsync_ShouldRemoveUsedCode()
    {
        // Arrange
        var userId = "user123";
        var codes = (await _service.GenerateBackupCodesAsync(userId, 5)).ToList();
        var codeToVerify = codes[0];

        // Act
        await _service.VerifyBackupCodeAsync(userId, codeToVerify);
        var secondAttempt = await _service.VerifyBackupCodeAsync(userId, codeToVerify);

        // Assert
        Assert.False(secondAttempt);
    }

    [Fact]
    public async Task GetMfaStatusAsync_ShouldReturnDisabled_WhenNotSetup()
    {
        // Arrange
        var userId = "newuser";

        // Act
        var status = await _service.GetMfaStatusAsync(userId);

        // Assert
        Assert.False(status.IsEnabled);
        Assert.Equal(MfaMethod.None, status.EnabledMethod);
    }

    [Fact]
    public async Task DisableMfaAsync_ShouldDisableMfa()
    {
        // Arrange
        var userId = "user123";
        await _service.GenerateBackupCodesAsync(userId); // Setup some MFA data

        // Act
        await _service.DisableMfaAsync(userId);
        var status = await _service.GetMfaStatusAsync(userId);

        // Assert
        Assert.False(status.IsEnabled);
    }
}
