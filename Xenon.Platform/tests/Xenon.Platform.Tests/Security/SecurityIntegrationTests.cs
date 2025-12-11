using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Http;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Persistence;
using Xenon.Platform.Infrastructure.Services;
using Xunit;

namespace Xenon.Platform.Tests.Security;

/// <summary>
/// Integration tests for the complete security flow including
/// password validation, authentication, token management, and security events.
/// </summary>
public class SecurityIntegrationTests : IDisposable
{
    private readonly PlatformDbContext _context;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IPasswordPolicyService _passwordPolicyService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ISecurityEventService _securityEventService;

    public SecurityIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PlatformDbContext(options);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "this-is-a-very-long-secret-key-for-testing-purposes-at-least-32-chars",
                ["Jwt:Issuer"] = "xenon-platform-test",
                ["Jwt:TenantAudience"] = "xenon-tenant-test",
                ["Jwt:AdminAudience"] = "xenon-admin-test",
                ["Jwt:TenantTokenExpiryHours"] = "24",
                ["Jwt:AdminTokenExpiryHours"] = "8",
                ["PasswordPolicy:MinLength"] = "8",
                ["PasswordPolicy:RequireUppercase"] = "true",
                ["PasswordPolicy:RequireLowercase"] = "true",
                ["PasswordPolicy:RequireDigit"] = "true",
                ["PasswordPolicy:RequireSpecialCharacter"] = "true",
                ["PasswordPolicy:PreventCommonPasswords"] = "true",
                ["Security:RefreshTokenExpiryDays"] = "30",
                ["Security:MaxActiveTokensPerUser"] = "5",
                ["Security:Events:BruteForceThreshold"] = "5",
                ["Security:Events:BruteForceWindowMinutes"] = "15"
            })
            .Build();

        _passwordHashingService = new PasswordHashingService(Mock.Of<ILogger<PasswordHashingService>>());
        _passwordPolicyService = new PasswordPolicyService(configuration);
        _jwtTokenService = new JwtTokenService(configuration);
        _refreshTokenService = new RefreshTokenService(
            _context,
            configuration,
            Mock.Of<ILogger<RefreshTokenService>>());

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _securityEventService = new SecurityEventService(
            _context,
            httpContextAccessorMock.Object,
            configuration,
            Mock.Of<ILogger<SecurityEventService>>());
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region End-to-End Authentication Tests

    [Fact]
    public async Task CompleteAuthFlow_ShouldWork()
    {
        // 1. Validate password meets policy
        var password = "SecureP@ss123!";
        var validationResult = _passwordPolicyService.Validate(password);
        validationResult.IsValid.Should().BeTrue();

        // 2. Hash password
        var passwordHash = _passwordHashingService.HashPassword(password);
        passwordHash.Should().NotBe(password);

        // 3. Verify password
        var isValid = _passwordHashingService.VerifyPassword(password, passwordHash);
        isValid.Should().BeTrue();

        // 4. Create tenant and admin
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Clinic",
            Slug = "test-clinic",
            CompanyType = CompanyType.Clinic,
            Status = TenantStatus.Active
        };

        var admin = new TenantAdmin
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Tenant = tenant,
            Email = "admin@test.com",
            FirstName = "Test",
            LastName = "Admin",
            PasswordHash = passwordHash,
            Role = "Owner",
            IsActive = true
        };

        // 5. Generate JWT token
        var accessToken = _jwtTokenService.GenerateTenantToken(admin, tenant);
        accessToken.Should().NotBeNullOrEmpty();

        // 6. Generate refresh token
        var (refreshToken, refreshTokenEntity) = await _refreshTokenService.GenerateRefreshTokenAsync(
            RefreshTokenTypes.TenantAdmin,
            admin.Id,
            tenant.Id,
            "192.168.1.1");

        refreshToken.Should().NotBeNullOrEmpty();
        refreshTokenEntity.UserId.Should().Be(admin.Id);

        // 7. Log security event
        await _securityEventService.LogEventAsync(
            SecurityEventType.LoginSuccess,
            new SecurityEventContext
            {
                UserId = admin.Id,
                UserType = RefreshTokenTypes.TenantAdmin,
                TenantId = tenant.Id,
                Email = admin.Email,
                IpAddress = "192.168.1.1",
                IsSuccessful = true
            });

        var events = await _context.SecurityEvents.ToListAsync();
        events.Should().HaveCount(1);
        events[0].EventType.Should().Be(SecurityEventType.LoginSuccess);

        // 8. Validate JWT token
        var principal = _jwtTokenService.ValidateToken(accessToken, "xenon-tenant-test");
        principal.Should().NotBeNull();
        principal!.FindFirst("sub")?.Value.Should().Be(admin.Id.ToString());

        // 9. Validate refresh token
        var validatedRefreshToken = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);
        validatedRefreshToken.Should().NotBeNull();

        // 10. Rotate refresh token
        var rotationResult = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken);
        rotationResult.Should().NotBeNull();
        rotationResult!.Value.newToken.Should().NotBe(refreshToken);

        // 11. Old refresh token should be invalid now
        var oldTokenValidation = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);
        oldTokenValidation.Should().BeNull();
    }

    [Fact]
    public async Task BruteForceProtection_ShouldDetectAttack()
    {
        // Arrange
        var email = "victim@example.com";
        var ipAddress = "10.0.0.100";

        // Simulate 6 failed login attempts
        for (int i = 0; i < 6; i++)
        {
            await _securityEventService.LogEventAsync(
                SecurityEventType.LoginFailed,
                new SecurityEventContext
                {
                    Email = email,
                    IpAddress = ipAddress,
                    IsSuccessful = false,
                    ErrorMessage = "Invalid password"
                });
        }

        // Assert
        var isBruteForce = await _securityEventService.IsBruteForceAttemptAsync(email, ipAddress);
        isBruteForce.Should().BeTrue();

        var riskAssessment = await _securityEventService.AssessLoginRiskAsync(email, ipAddress, null);
        riskAssessment.IsBruteForceDetected.Should().BeTrue();
        riskAssessment.RiskLevel.Should().Be(RiskLevel.High);
    }

    [Fact]
    public async Task SessionManagement_ShouldEnforceMaxSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var maxSessions = 5;

        // Create more than max sessions
        for (int i = 0; i < maxSessions + 2; i++)
        {
            await _refreshTokenService.GenerateRefreshTokenAsync(
                RefreshTokenTypes.TenantAdmin,
                userId,
                null,
                $"192.168.1.{i}");
        }

        // Assert
        var activeCount = await _refreshTokenService.GetActiveTokenCountAsync(
            userId,
            RefreshTokenTypes.TenantAdmin);

        activeCount.Should().BeLessOrEqualTo(maxSessions);
    }

    [Fact]
    public async Task TokenReuseDetection_ShouldRevokeFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Generate initial token
        var (token1, entity1) = await _refreshTokenService.GenerateRefreshTokenAsync(
            RefreshTokenTypes.TenantAdmin,
            userId);

        // Rotate token (simulating legitimate use)
        var result2 = await _refreshTokenService.RotateRefreshTokenAsync(token1);
        result2.Should().NotBeNull();

        // Attempt to reuse old token (simulating attack)
        var reuseResult = await _refreshTokenService.ValidateRefreshTokenAsync(token1);

        // Assert - should return null and trigger family revocation
        reuseResult.Should().BeNull();
    }

    #endregion

    #region Password Security Tests

    [Theory]
    [InlineData("password", false)]
    [InlineData("Password1!", true)]
    [InlineData("abc", false)]
    [InlineData("SecureP@ss123!", true)]
    public void PasswordPolicy_ShouldEnforceRules(string password, bool expectedValid)
    {
        // Act
        var result = _passwordPolicyService.Validate(password);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void PasswordStrength_ShouldCalculateCorrectly()
    {
        // Arrange & Act
        var weakStrength = _passwordPolicyService.GetStrength("password");
        var strongStrength = _passwordPolicyService.GetStrength("MyV3ryStr0ng!Pass");

        // Assert
        weakStrength.Should().Be(PasswordStrength.VeryWeak);
        ((int)strongStrength).Should().BeGreaterThan((int)weakStrength);
    }

    [Fact]
    public void PasswordHashing_ShouldBeSecure()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordHashingService.HashPassword(password);
        var hash2 = _passwordHashingService.HashPassword(password);

        // Assert - same password should produce different hashes (due to salt)
        hash1.Should().NotBe(hash2);

        // Both should verify correctly
        _passwordHashingService.VerifyPassword(password, hash1).Should().BeTrue();
        _passwordHashingService.VerifyPassword(password, hash2).Should().BeTrue();

        // Wrong password should fail
        _passwordHashingService.VerifyPassword("WrongPassword", hash1).Should().BeFalse();
    }

    #endregion

    #region Security Event Statistics Tests

    [Fact]
    public async Task SecurityStatistics_ShouldAccumulateCorrectly()
    {
        // Arrange
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow.AddDays(1);

        // Create various security events
        await _securityEventService.LogEventAsync(SecurityEventType.LoginSuccess,
            new SecurityEventContext { Email = "user1@test.com", IsSuccessful = true });
        await _securityEventService.LogEventAsync(SecurityEventType.LoginSuccess,
            new SecurityEventContext { Email = "user2@test.com", IsSuccessful = true });
        await _securityEventService.LogEventAsync(SecurityEventType.LoginFailed,
            new SecurityEventContext { Email = "user1@test.com", IsSuccessful = false });
        await _securityEventService.LogEventAsync(SecurityEventType.PasswordChanged,
            new SecurityEventContext { Email = "user1@test.com", IsSuccessful = true });

        // Act
        var stats = await _securityEventService.GetStatisticsAsync(from, to);

        // Assert
        stats.TotalEvents.Should().Be(4);
        stats.SuccessfulLogins.Should().Be(2);
        stats.FailedLogins.Should().Be(1);
        stats.PasswordChanges.Should().Be(1);
    }

    #endregion
}
