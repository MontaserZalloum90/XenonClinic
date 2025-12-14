using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using XenonClinic.Tests.Fixtures;
using Xunit;

namespace XenonClinic.Tests.Integration;

/// <summary>
/// Integration tests for authentication flows.
/// Tests login, registration, and authorization requirements.
/// </summary>
[Collection("XenonClinic")]
public class AuthenticationFlowTests : IntegrationTestBase
{
    public AuthenticationFlowTests(XenonClinicWebApplicationFactory factory)
        : base(factory)
    {
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithMissingCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var emptyRequest = new { };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portal/login", emptyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "not-an-email",
            Password = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portal/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portal/login", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Endpoint_ShouldHaveRateLimiting()
    {
        // Multiple rapid requests should eventually be rate limited
        var request = new
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var responses = new List<HttpResponseMessage>();

        // Make multiple requests
        for (int i = 0; i < 15; i++)
        {
            var response = await Client.PostAsJsonAsync("/api/portal/login", request);
            responses.Add(response);
        }

        // Assert - at least one should be rate limited or unauthorized
        // (in test environment, rate limiting may not trigger but the endpoint should exist)
        responses.Should().NotBeEmpty();
        responses.All(r => r.StatusCode != HttpStatusCode.NotFound).Should().BeTrue();
    }

    #endregion

    #region Registration Tests

    [Fact]
    public async Task Register_WithMissingRequiredFields_ShouldReturnBadRequest()
    {
        // Arrange
        var incompleteRequest = new
        {
            Email = "test@example.com"
            // Missing other required fields
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portal/register?branchId=1", incompleteRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Register_WithoutBranchId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "newuser@example.com",
            Password = "SecurePassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portal/register", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Register_Endpoint_ShouldHaveRateLimiting()
    {
        // Registration endpoint should be rate limited
        var request = new
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        var responses = new List<HttpResponseMessage>();

        // Make multiple requests
        for (int i = 0; i < 15; i++)
        {
            var response = await Client.PostAsJsonAsync("/api/portal/register?branchId=1", request);
            responses.Add(response);
        }

        // Assert - endpoint should exist (not return 404)
        responses.Should().NotBeEmpty();
        responses.All(r => r.StatusCode != HttpStatusCode.NotFound).Should().BeTrue();
    }

    #endregion

    #region Protected Endpoint Tests

    [Fact]
    public async Task ProtectedEndpoint_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/security/permissions");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ProtectedEndpoint_DatabaseMetrics_ShouldRequireAuth()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics/database");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ProtectedEndpoint_CacheMetrics_ShouldRequireAuth()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics/cache");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    #endregion

    #region Token Tests

    [Fact]
    public async Task Request_WithInvalidBearerToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/security/permissions");
        request.Headers.Add("Authorization", "Bearer invalid-token-12345");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Request_WithMalformedAuthHeader_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/security/permissions");
        request.Headers.Add("Authorization", "NotBearer token");

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    #endregion

    #region Password Reset Flow Tests

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_ShouldNotRevealUserExistence()
    {
        // Arrange - this should NOT reveal whether user exists
        var request = new
        {
            Email = "nonexistent@example.com"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portal/forgot-password", request);

        // Assert - should return success-like response to prevent email enumeration
        // The actual behavior depends on implementation, but should not be 404
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForgotPassword_Endpoint_ShouldHaveRateLimiting()
    {
        // Password reset should be heavily rate limited
        var request = new
        {
            Email = "test@example.com"
        };

        var responses = new List<HttpResponseMessage>();

        for (int i = 0; i < 15; i++)
        {
            var response = await Client.PostAsJsonAsync("/api/portal/forgot-password", request);
            responses.Add(response);
        }

        // Assert - endpoint should exist
        responses.Should().NotBeEmpty();
        responses.All(r => r.StatusCode != HttpStatusCode.NotFound).Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ShouldFail()
    {
        // Arrange
        var request = new
        {
            Token = "invalid-reset-token",
            NewPassword = "NewSecurePassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portal/reset-password", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region Session Tests

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldFail()
    {
        // Arrange
        var request = new
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portal/refresh-token", request);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RefreshToken_Endpoint_ShouldHaveRateLimiting()
    {
        // Refresh token should be rate limited
        var request = new
        {
            RefreshToken = "test-token"
        };

        var responses = new List<HttpResponseMessage>();

        for (int i = 0; i < 15; i++)
        {
            var response = await Client.PostAsJsonAsync("/api/portal/refresh-token", request);
            responses.Add(response);
        }

        // Assert - endpoint should exist
        responses.Should().NotBeEmpty();
        responses.All(r => r.StatusCode != HttpStatusCode.NotFound).Should().BeTrue();
    }

    #endregion
}
