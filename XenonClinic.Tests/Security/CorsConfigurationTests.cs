using FluentAssertions;
using XenonClinic.Infrastructure.Configuration;
using Xunit;

namespace XenonClinic.Tests.Security;

/// <summary>
/// Tests for CORS configuration.
/// </summary>
public class CorsConfigurationTests
{
    [Fact]
    public void CorsOptions_ShouldHaveSecureDefaults()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.AllowedOrigins.Should().BeEmpty(); // Secure by default
        options.AllowCredentials.Should().BeTrue();
        options.PreflightMaxAge.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CorsOptions_AllowedMethods_ShouldIncludeStandardMethods()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.AllowedMethods.Should().Contain("GET");
        options.AllowedMethods.Should().Contain("POST");
        options.AllowedMethods.Should().Contain("PUT");
        options.AllowedMethods.Should().Contain("DELETE");
        options.AllowedMethods.Should().Contain("OPTIONS");
    }

    [Fact]
    public void CorsOptions_AllowedHeaders_ShouldIncludeContentType()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.AllowedHeaders.Should().Contain("Content-Type");
    }

    [Fact]
    public void CorsOptions_AllowedHeaders_ShouldIncludeAuthorization()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.AllowedHeaders.Should().Contain("Authorization");
    }

    [Fact]
    public void CorsOptions_AllowedHeaders_ShouldIncludeCorrelationId()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.AllowedHeaders.Should().Contain("X-Correlation-ID");
    }

    [Fact]
    public void CorsOptions_AllowedHeaders_ShouldIncludeTenantId()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.AllowedHeaders.Should().Contain("X-Tenant-ID");
    }

    [Fact]
    public void CorsOptions_ExposedHeaders_ShouldIncludeCorrelationId()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.ExposedHeaders.Should().Contain("X-Correlation-ID");
    }

    [Fact]
    public void CorsOptions_ExposedHeaders_ShouldIncludeRequestId()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.ExposedHeaders.Should().Contain("X-Request-ID");
    }

    [Fact]
    public void CorsOptions_ExposedHeaders_ShouldIncludePaginationHeaders()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert
        options.ExposedHeaders.Should().Contain("X-Pagination-Total");
        options.ExposedHeaders.Should().Contain("X-Pagination-Page");
    }

    [Fact]
    public void CorsOptions_PreflightMaxAge_ShouldBeReasonable()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert - 10 minutes default
        options.PreflightMaxAge.Should().Be(600);
    }

    [Fact]
    public void PolicyNames_ShouldBeDefined()
    {
        // Assert
        CorsConfiguration.DefaultPolicyName.Should().NotBeNullOrEmpty();
        CorsConfiguration.AllowAllPolicyName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CorsOptions_CanConfigureCustomOrigins()
    {
        // Arrange & Act
        var options = new CorsOptions
        {
            AllowedOrigins = new[] { "https://app.xenonclinic.com", "https://admin.xenonclinic.com" }
        };

        // Assert
        options.AllowedOrigins.Should().HaveCount(2);
        options.AllowedOrigins.Should().Contain("https://app.xenonclinic.com");
    }

    [Fact]
    public void CorsOptions_CanDisableCredentials()
    {
        // Arrange & Act
        var options = new CorsOptions
        {
            AllowCredentials = false
        };

        // Assert
        options.AllowCredentials.Should().BeFalse();
    }

    [Fact]
    public void CorsOptions_CanCustomizeAllowedMethods()
    {
        // Arrange & Act
        var options = new CorsOptions
        {
            AllowedMethods = new[] { "GET", "POST" }
        };

        // Assert
        options.AllowedMethods.Should().HaveCount(2);
        options.AllowedMethods.Should().NotContain("DELETE");
    }

    [Fact]
    public void CorsOptions_EmptyOrigins_ShouldDenyAllByDefault()
    {
        // Arrange & Act
        var options = new CorsOptions();

        // Assert - empty origins means deny all cross-origin
        options.AllowedOrigins.Should().BeEmpty();
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://localhost:3000")]
    [InlineData("https://app.xenonclinic.com")]
    public void CorsOptions_ValidOrigins_ShouldBeAccepted(string origin)
    {
        // Arrange & Act
        var options = new CorsOptions
        {
            AllowedOrigins = new[] { origin }
        };

        // Assert
        options.AllowedOrigins.Should().Contain(origin);
    }
}
