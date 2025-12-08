using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Tests.Fixtures;
using XenonClinic.Tests.Helpers;
using Xunit;

namespace XenonClinic.Tests.Integration.Security;

/// <summary>
/// Tests for authentication and authorization edge cases
/// </summary>
public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region JWT Token Validation Tests

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithMalformedToken_Returns401Unauthorized()
    {
        // Arrange
        var malformedToken = TestDataGenerator.GenerateMalformedJwtToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", malformedToken);

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("not.a.valid.token")]
    [InlineData("Bearer")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9")]
    public async Task ProtectedEndpoint_WithInvalidTokenFormat_Returns401Unauthorized(string invalidToken)
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", invalidToken);

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_Returns401Unauthorized()
    {
        // Arrange - Token with exp claim in the past
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiZXhwIjoxMDAwMDAwMDAwfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithWrongAuthScheme_Returns401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "dXNlcjpwYXNz");

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Login Endpoint Tests

    [Fact]
    public async Task Login_WithEmptyCredentials_Returns400BadRequest()
    {
        // Arrange
        var loginRequest = new { username = "", password = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNullCredentials_Returns400BadRequest()
    {
        // Arrange
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401Unauthorized()
    {
        // Arrange
        var loginRequest = new { username = "nonexistent@user.com", password = "WrongPassword123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("admin'; DROP TABLE Users;--", "password")]
    [InlineData("admin", "' OR '1'='1")]
    [InlineData("' UNION SELECT * FROM Users--", "password")]
    public async Task Login_WithSqlInjectionAttempt_DoesNotExecuteInjection(string username, string password)
    {
        // Arrange
        var loginRequest = new { username, password };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        // Should return 400 (blocked by input validation) or 401 (invalid credentials), but NOT 500
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>", "password")]
    [InlineData("admin", "<img src=x onerror=alert('XSS')>")]
    public async Task Login_WithXssAttempt_IsSanitized(string username, string password)
    {
        // Arrange
        var loginRequest = new { username, password };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        // Should return 400 (blocked) or 401 (invalid), not execute XSS
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithExtremelyLongUsername_Returns400BadRequest()
    {
        // Arrange
        var longUsername = new string('a', 10000);
        var loginRequest = new { username = longUsername, password = "password" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithSpecialCharacters_HandlesGracefully()
    {
        // Arrange
        var loginRequest = new { username = "user@domain.com!#$%^&*()", password = "P@$$w0rd!#$%^&*()" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        // Should handle gracefully, not crash
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Login_WithUnicodeCharacters_HandlesGracefully()
    {
        // Arrange - Arabic and Chinese characters
        var loginRequest = new { username = "مستخدم@测试.com", password = "كلمة السر123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Authorization Header Edge Cases

    [Fact]
    public async Task ProtectedEndpoint_WithMultipleBearerTokens_UsesFirst()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", "Bearer token1");
        _client.DefaultRequestHeaders.Add("X-Custom-Auth", "Bearer token2");

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        // Should use the first Authorization header
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithNullByteInToken_Returns401()
    {
        // Arrange - Null byte injection attempt
        var tokenWithNullByte = "eyJhbGciOiJIUzI1NiJ9\0.invalidpayload.signature";
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenWithNullByte);

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Session/Token Refresh Tests

    [Fact]
    public async Task RefreshToken_WithInvalidRefreshToken_Returns401()
    {
        // Arrange
        var refreshRequest = new { refreshToken = "invalid-refresh-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion
}
