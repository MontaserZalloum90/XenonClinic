using FluentAssertions;
using System.Security.Claims;
using Xenon.Platform.Api.Extensions;
using Xunit;

namespace Xenon.Platform.Tests.Extensions;

/// <summary>
/// Tests for ClaimsPrincipalExtensions - validates claim extraction from JWT tokens.
/// </summary>
public class ClaimsPrincipalExtensionsTests
{
    #region GetTenantId Tests

    [Fact]
    public void GetTenantId_ShouldReturnTenantId_WhenClaimExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = CreateClaimsPrincipal(new Claim("tenant_id", tenantId.ToString()));

        // Act
        var result = user.GetTenantId();

        // Assert
        result.Should().Be(tenantId);
    }

    [Fact]
    public void GetTenantId_ShouldReturnNull_WhenClaimDoesNotExist()
    {
        // Arrange
        var user = CreateClaimsPrincipal();

        // Act
        var result = user.GetTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTenantId_ShouldReturnNull_WhenClaimIsEmpty()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("tenant_id", ""));

        // Act
        var result = user.GetTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTenantId_ShouldReturnNull_WhenClaimIsNotValidGuid()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("tenant_id", "not-a-guid"));

        // Act
        var result = user.GetTenantId();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abc-def-ghi")]
    [InlineData("00000000-0000-0000-0000-00000000000g")]
    public void GetTenantId_ShouldReturnNull_ForInvalidGuidFormats(string invalidGuid)
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("tenant_id", invalidGuid));

        // Act
        var result = user.GetTenantId();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetRequiredTenantId Tests

    [Fact]
    public void GetRequiredTenantId_ShouldReturnTenantId_WhenClaimExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var user = CreateClaimsPrincipal(new Claim("tenant_id", tenantId.ToString()));

        // Act
        var result = user.GetRequiredTenantId();

        // Assert
        result.Should().Be(tenantId);
    }

    [Fact]
    public void GetRequiredTenantId_ShouldThrowUnauthorizedAccessException_WhenClaimDoesNotExist()
    {
        // Arrange
        var user = CreateClaimsPrincipal();

        // Act & Assert
        var act = () => user.GetRequiredTenantId();
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*Tenant ID*not found*");
    }

    [Fact]
    public void GetRequiredTenantId_ShouldThrowUnauthorizedAccessException_WhenClaimIsInvalid()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("tenant_id", "invalid"));

        // Act & Assert
        var act = () => user.GetRequiredTenantId();
        act.Should().Throw<UnauthorizedAccessException>();
    }

    #endregion

    #region GetUserId Tests

    [Fact]
    public void GetUserId_ShouldReturnUserId_FromNameIdentifierClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateClaimsPrincipal(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_ShouldReturnUserId_FromSubClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateClaimsPrincipal(new Claim("sub", userId.ToString()));

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_ShouldPreferNameIdentifier_OverSubClaim()
    {
        // Arrange
        var nameIdentifierId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        var user = CreateClaimsPrincipal(
            new Claim(ClaimTypes.NameIdentifier, nameIdentifierId.ToString()),
            new Claim("sub", subId.ToString()));

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().Be(nameIdentifierId);
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenNoUserIdClaim()
    {
        // Arrange
        var user = CreateClaimsPrincipal();

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserId_ShouldReturnNull_WhenClaimIsNotValidGuid()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim(ClaimTypes.NameIdentifier, "not-a-guid"));

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetEmail Tests

    [Fact]
    public void GetEmail_ShouldReturnEmail_FromEmailClaimType()
    {
        // Arrange
        var email = "user@example.com";
        var user = CreateClaimsPrincipal(new Claim(ClaimTypes.Email, email));

        // Act
        var result = user.GetEmail();

        // Assert
        result.Should().Be(email);
    }

    [Fact]
    public void GetEmail_ShouldReturnEmail_FromEmailClaim()
    {
        // Arrange
        var email = "user@example.com";
        var user = CreateClaimsPrincipal(new Claim("email", email));

        // Act
        var result = user.GetEmail();

        // Assert
        result.Should().Be(email);
    }

    [Fact]
    public void GetEmail_ShouldPreferEmailClaimType_OverEmailClaim()
    {
        // Arrange
        var emailClaimType = "primary@example.com";
        var emailClaim = "secondary@example.com";
        var user = CreateClaimsPrincipal(
            new Claim(ClaimTypes.Email, emailClaimType),
            new Claim("email", emailClaim));

        // Act
        var result = user.GetEmail();

        // Assert
        result.Should().Be(emailClaimType);
    }

    [Fact]
    public void GetEmail_ShouldReturnNull_WhenNoEmailClaim()
    {
        // Arrange
        var user = CreateClaimsPrincipal();

        // Act
        var result = user.GetEmail();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetRealm Tests

    [Fact]
    public void GetRealm_ShouldReturnRealm_WhenClaimExists()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("realm", "tenant"));

        // Act
        var result = user.GetRealm();

        // Assert
        result.Should().Be("tenant");
    }

    [Fact]
    public void GetRealm_ShouldReturnNull_WhenNoRealmClaim()
    {
        // Arrange
        var user = CreateClaimsPrincipal();

        // Act
        var result = user.GetRealm();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region IsPlatformAdmin Tests

    [Fact]
    public void IsPlatformAdmin_ShouldReturnTrue_WhenRealmIsPlatformAdmin()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("realm", "platform-admin"));

        // Act
        var result = user.IsPlatformAdmin();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPlatformAdmin_ShouldReturnFalse_WhenRealmIsTenant()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("realm", "tenant"));

        // Act
        var result = user.IsPlatformAdmin();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPlatformAdmin_ShouldReturnFalse_WhenNoRealmClaim()
    {
        // Arrange
        var user = CreateClaimsPrincipal();

        // Act
        var result = user.IsPlatformAdmin();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Platform-Admin")]
    [InlineData("PLATFORM-ADMIN")]
    [InlineData("admin")]
    public void IsPlatformAdmin_ShouldBeCaseSensitive(string realm)
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("realm", realm));

        // Act
        var result = user.IsPlatformAdmin();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsTenantUser Tests

    [Fact]
    public void IsTenantUser_ShouldReturnTrue_WhenRealmIsTenant()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("realm", "tenant"));

        // Act
        var result = user.IsTenantUser();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTenantUser_ShouldReturnFalse_WhenRealmIsPlatformAdmin()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim("realm", "platform-admin"));

        // Act
        var result = user.IsTenantUser();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsTenantUser_ShouldReturnFalse_WhenNoRealmClaim()
    {
        // Arrange
        var user = CreateClaimsPrincipal();

        // Act
        var result = user.IsTenantUser();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region End-to-End Tests

    [Fact]
    public void AllExtensions_ShouldWorkTogether_ForValidTenantUser()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var email = "tenant@clinic.com";

        var user = CreateClaimsPrincipal(
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("realm", "tenant"));

        // Act & Assert
        user.GetTenantId().Should().Be(tenantId);
        user.GetRequiredTenantId().Should().Be(tenantId);
        user.GetUserId().Should().Be(userId);
        user.GetEmail().Should().Be(email);
        user.GetRealm().Should().Be("tenant");
        user.IsTenantUser().Should().BeTrue();
        user.IsPlatformAdmin().Should().BeFalse();
    }

    [Fact]
    public void AllExtensions_ShouldWorkTogether_ForValidPlatformAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "admin@platform.com";

        var user = CreateClaimsPrincipal(
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("realm", "platform-admin"));

        // Act & Assert
        user.GetTenantId().Should().BeNull();
        user.GetUserId().Should().Be(userId);
        user.GetEmail().Should().Be(email);
        user.GetRealm().Should().Be("platform-admin");
        user.IsPlatformAdmin().Should().BeTrue();
        user.IsTenantUser().Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static ClaimsPrincipal CreateClaimsPrincipal(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuthentication");
        return new ClaimsPrincipal(identity);
    }

    #endregion
}
