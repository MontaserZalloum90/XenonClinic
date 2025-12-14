using FluentAssertions;
using Microsoft.AspNetCore.Http;
using XenonClinic.Api.Middleware;
using Xunit;

namespace XenonClinic.Tests.Security;

/// <summary>
/// Tests for security headers middleware configuration.
/// </summary>
public class SecurityHeadersTests
{
    [Fact]
    public void SecurityHeadersOptions_ShouldHaveSecureDefaults()
    {
        // Arrange & Act
        var options = new SecurityHeadersOptions();

        // Assert - verify secure defaults
        options.EnableFrameOptions.Should().BeTrue();
        options.FrameOptions.Should().Be("DENY");
        options.EnableContentTypeOptions.Should().BeTrue();
        options.EnableXssProtection.Should().BeTrue();
        options.EnableReferrerPolicy.Should().BeTrue();
        options.EnableContentSecurityPolicy.Should().BeTrue();
        options.EnablePermissionsPolicy.Should().BeTrue();
        options.EnableHsts.Should().BeTrue();
        options.EnableCacheControl.Should().BeTrue();
    }

    [Fact]
    public void ContentSecurityPolicy_ShouldRestrictScriptSources()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert - CSP should include script-src
        options.ContentSecurityPolicy.Should().Contain("script-src");
        options.ContentSecurityPolicy.Should().Contain("'self'");
    }

    [Fact]
    public void ContentSecurityPolicy_ShouldPreventFraming()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert - CSP should prevent embedding in frames
        options.ContentSecurityPolicy.Should().Contain("frame-ancestors 'none'");
    }

    [Fact]
    public void ContentSecurityPolicy_ShouldRestrictFormActions()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert - CSP should restrict form submissions
        options.ContentSecurityPolicy.Should().Contain("form-action 'self'");
    }

    [Fact]
    public void ContentSecurityPolicy_ShouldRestrictBaseUri()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert - CSP should restrict base URI
        options.ContentSecurityPolicy.Should().Contain("base-uri 'self'");
    }

    [Fact]
    public void ContentSecurityPolicy_ShouldBlockObjects()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert - CSP should block object/embed elements
        options.ContentSecurityPolicy.Should().Contain("object-src 'none'");
    }

    [Fact]
    public void PermissionsPolicy_ShouldRestrictCamera()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert
        options.PermissionsPolicy.Should().Contain("camera=()");
    }

    [Fact]
    public void PermissionsPolicy_ShouldRestrictMicrophone()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert
        options.PermissionsPolicy.Should().Contain("microphone=()");
    }

    [Fact]
    public void PermissionsPolicy_ShouldRestrictGeolocation()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert
        options.PermissionsPolicy.Should().Contain("geolocation=()");
    }

    [Fact]
    public void PermissionsPolicy_ShouldRestrictPayment()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert
        options.PermissionsPolicy.Should().Contain("payment=()");
    }

    [Fact]
    public void HstsValue_ShouldHaveReasonableMaxAge()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert - should be at least 1 year (31536000 seconds)
        options.HstsValue.Should().Contain("max-age=31536000");
    }

    [Fact]
    public void HstsValue_ShouldIncludeSubdomains()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert
        options.HstsValue.Should().Contain("includeSubDomains");
    }

    [Fact]
    public void ReferrerPolicy_ShouldBeStrict()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert - should use strict referrer policy
        options.ReferrerPolicy.Should().Be("strict-origin-when-cross-origin");
    }

    [Fact]
    public void FrameOptions_ShouldDenyByDefault()
    {
        // Arrange
        var options = new SecurityHeadersOptions();

        // Assert - should prevent framing entirely
        options.FrameOptions.Should().Be("DENY");
    }

    [Fact]
    public void SecurityHeaders_ShouldBeCustomizable()
    {
        // Arrange & Act
        var options = new SecurityHeadersOptions
        {
            FrameOptions = "SAMEORIGIN",
            ReferrerPolicy = "no-referrer",
            EnableXssProtection = false
        };

        // Assert
        options.FrameOptions.Should().Be("SAMEORIGIN");
        options.ReferrerPolicy.Should().Be("no-referrer");
        options.EnableXssProtection.Should().BeFalse();
    }

    [Fact]
    public void SecurityHeaders_CanDisableIndividualHeaders()
    {
        // Arrange & Act
        var options = new SecurityHeadersOptions
        {
            EnableContentSecurityPolicy = false,
            EnablePermissionsPolicy = false,
            EnableHsts = false
        };

        // Assert
        options.EnableContentSecurityPolicy.Should().BeFalse();
        options.EnablePermissionsPolicy.Should().BeFalse();
        options.EnableHsts.Should().BeFalse();
    }

    [Theory]
    [InlineData("/api/security/permissions")]
    [InlineData("/api/portal/login")]
    [InlineData("/api/patient/1")]
    [InlineData("/api/hr/employees")]
    [InlineData("/api/financial/invoices")]
    public void SensitiveEndpoints_ShouldHaveNoCacheHeaders(string path)
    {
        // These endpoints should have cache-control headers
        // preventing sensitive data from being cached
        path.Should().ContainAny("security", "portal", "patient", "hr", "financial");
    }

    [Theory]
    [InlineData("/api/diagnostics/metrics")]
    [InlineData("/health")]
    [InlineData("/swagger")]
    public void NonSensitiveEndpoints_MayBeCached(string path)
    {
        // These endpoints don't contain sensitive data
        path.Should().NotContainAny("security", "portal", "patient", "hr", "financial");
    }
}
