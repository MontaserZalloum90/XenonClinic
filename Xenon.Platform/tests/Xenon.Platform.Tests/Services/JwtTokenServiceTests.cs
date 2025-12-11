using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xenon.Platform.Domain.Entities;
using Xenon.Platform.Domain.Enums;
using Xenon.Platform.Infrastructure.Services;
using Xunit;

namespace Xenon.Platform.Tests.Services;

/// <summary>
/// Tests for JwtTokenService - validates JWT token generation, validation,
/// and 2FA token handling.
/// </summary>
public class JwtTokenServiceTests
{
    private readonly IJwtTokenService _service;
    private readonly string _secretKey = "this-is-a-very-long-secret-key-for-testing-purposes-at-least-32-chars";

    public JwtTokenServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = _secretKey,
                ["Jwt:Issuer"] = "xenon-platform-test",
                ["Jwt:TenantAudience"] = "xenon-tenant-test",
                ["Jwt:AdminAudience"] = "xenon-admin-test",
                ["Jwt:TwoFactorAudience"] = "xenon-2fa-test",
                ["Jwt:TenantTokenExpiryHours"] = "24",
                ["Jwt:AdminTokenExpiryHours"] = "8",
                ["Jwt:TwoFactorTokenExpiryMinutes"] = "5"
            })
            .Build();

        _service = new JwtTokenService(configuration);
    }

    #region Tenant Token Tests

    [Fact]
    public void GenerateTenantToken_ShouldReturnValidToken()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestTenantAdmin(tenant);

        // Act
        var token = _service.GenerateTenantToken(admin, tenant);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateTenantToken_ShouldContainRequiredClaims()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestTenantAdmin(tenant);

        // Act
        var token = _service.GenerateTenantToken(admin, tenant);
        var principal = _service.ValidateToken(token, "xenon-tenant-test");

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst("sub")?.Value.Should().Be(admin.Id.ToString());
        principal.FindFirst("email")?.Value.Should().Be(admin.Email);
        principal.FindFirst("tenant_id")?.Value.Should().Be(tenant.Id.ToString());
        principal.FindFirst("tenant_slug")?.Value.Should().Be(tenant.Slug);
        principal.FindFirst("role")?.Value.Should().Be(admin.Role);
        principal.FindFirst("realm")?.Value.Should().Be("tenant");
    }

    [Fact]
    public void GenerateTenantToken_ShouldSetCorrectExpiry()
    {
        // Arrange
        var tenant = CreateTestTenant();
        var admin = CreateTestTenantAdmin(tenant);

        // Act
        var token = _service.GenerateTenantToken(admin, tenant);
        var expiry = _service.GetTokenExpiry(token);

        // Assert
        expiry.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Platform Admin Token Tests

    [Fact]
    public void GeneratePlatformAdminToken_ShouldReturnValidToken()
    {
        // Arrange
        var admin = CreateTestPlatformAdmin();

        // Act
        var token = _service.GeneratePlatformAdminToken(admin);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GeneratePlatformAdminToken_ShouldContainRequiredClaims()
    {
        // Arrange
        var admin = CreateTestPlatformAdmin();

        // Act
        var token = _service.GeneratePlatformAdminToken(admin);
        var principal = _service.ValidateToken(token, "xenon-admin-test");

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst("sub")?.Value.Should().Be(admin.Id.ToString());
        principal.FindFirst("email")?.Value.Should().Be(admin.Email);
        principal.FindFirst("role")?.Value.Should().Be(admin.Role);
        principal.FindFirst("realm")?.Value.Should().Be("platform-admin");
    }

    [Fact]
    public void GeneratePlatformAdminToken_ShouldIncludePermissions()
    {
        // Arrange
        var admin = CreateTestPlatformAdmin();
        admin.Permissions = "tenants:read,tenants:write,reports:read";

        // Act
        var token = _service.GeneratePlatformAdminToken(admin);
        var principal = _service.ValidateToken(token, "xenon-admin-test");

        // Assert
        principal.Should().NotBeNull();
        var permissions = principal!.FindAll("permission").Select(c => c.Value).ToList();
        permissions.Should().Contain("tenants:read");
        permissions.Should().Contain("tenants:write");
        permissions.Should().Contain("reports:read");
    }

    [Fact]
    public void GeneratePlatformAdminToken_ShouldSetCorrectExpiry()
    {
        // Arrange
        var admin = CreateTestPlatformAdmin();

        // Act
        var token = _service.GeneratePlatformAdminToken(admin);
        var expiry = _service.GetTokenExpiry(token);

        // Assert
        expiry.Should().BeCloseTo(DateTime.UtcNow.AddHours(8), TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Two-Factor Token Tests

    [Fact]
    public void GenerateTwoFactorToken_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var token = _service.GenerateTwoFactorToken(userId, "TenantAdmin", "test@example.com");

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateTwoFactorToken_ShouldContainCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";

        // Act
        var token = _service.GenerateTwoFactorToken(userId, "TenantAdmin", email);
        var principal = _service.ValidateTwoFactorToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst("sub")?.Value.Should().Be(userId.ToString());
        principal.FindFirst("email")?.Value.Should().Be(email);
        principal.FindFirst("user_type")?.Value.Should().Be("TenantAdmin");
        principal.FindFirst("purpose")?.Value.Should().Be("2fa-verification");
    }

    [Fact]
    public void GenerateTwoFactorToken_ShouldHaveShortExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var token = _service.GenerateTwoFactorToken(userId, "TenantAdmin", "test@example.com");
        var expiry = _service.GetTokenExpiry(token);

        // Assert
        expiry.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void ValidateTwoFactorToken_ShouldReturnNull_ForRegularToken()
    {
        // Arrange
        var admin = CreateTestPlatformAdmin();
        var regularToken = _service.GeneratePlatformAdminToken(admin);

        // Act
        var result = _service.ValidateTwoFactorToken(regularToken);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public void ValidateToken_ShouldReturnNull_ForInvalidToken()
    {
        // Act
        var result = _service.ValidateToken("invalid-token", "xenon-tenant-test");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_ForWrongAudience()
    {
        // Arrange
        var admin = CreateTestPlatformAdmin();
        var token = _service.GeneratePlatformAdminToken(admin);

        // Act
        var result = _service.ValidateToken(token, "xenon-tenant-test"); // Wrong audience

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_ForTamperedToken()
    {
        // Arrange
        var admin = CreateTestPlatformAdmin();
        var token = _service.GeneratePlatformAdminToken(admin);
        var tamperedToken = token + "tampered";

        // Act
        var result = _service.ValidateToken(tamperedToken, "xenon-admin-test");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Get Token Expiry Tests

    [Fact]
    public void GetTokenExpiry_ShouldReturnCorrectTime()
    {
        // Arrange
        var admin = CreateTestPlatformAdmin();
        var token = _service.GeneratePlatformAdminToken(admin);

        // Act
        var expiry = _service.GetTokenExpiry(token);

        // Assert
        expiry.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GetTokenExpiry_ShouldReturnMinValue_ForInvalidToken()
    {
        // Act
        var expiry = _service.GetTokenExpiry("invalid-token");

        // Assert
        expiry.Should().Be(DateTime.MinValue);
    }

    #endregion

    #region Helper Methods

    private static Tenant CreateTestTenant()
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Clinic",
            Slug = "test-clinic",
            CompanyType = CompanyType.Clinic,
            Status = TenantStatus.Active
        };
    }

    private static TenantAdmin CreateTestTenantAdmin(Tenant tenant)
    {
        return new TenantAdmin
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Tenant = tenant,
            Email = "admin@test.com",
            FirstName = "Test",
            LastName = "Admin",
            Role = "Owner",
            IsActive = true
        };
    }

    private static PlatformAdmin CreateTestPlatformAdmin()
    {
        return new PlatformAdmin
        {
            Id = Guid.NewGuid(),
            Email = "admin@xenon.ae",
            FirstName = "Platform",
            LastName = "Admin",
            Role = "SUPER_ADMIN",
            IsActive = true
        };
    }

    #endregion
}
