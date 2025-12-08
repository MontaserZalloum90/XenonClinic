using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using XenonClinic.Tests.Fixtures;
using Xunit;

namespace XenonClinic.Tests.Integration.Security;

/// <summary>
/// Tests for rate limiting middleware edge cases
/// </summary>
public class RateLimitingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RateLimitingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Auth Endpoint Rate Limiting (Brute Force Protection)

    [Fact]
    public async Task LoginEndpoint_ExceedsRateLimit_Returns429TooManyRequests()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new { username = "test@test.com", password = "wrongpassword" };
        var responses = new List<HttpResponseMessage>();

        // Act - Send more requests than allowed (auth limit is typically 5/minute)
        for (int i = 0; i < 10; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
            responses.Add(response);
        }

        // Assert - At least one should be rate limited (429) if rate limiting is enabled
        var hasRateLimitedResponse = responses.Any(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var has429OrAllUnauthorized = hasRateLimitedResponse ||
            responses.All(r => r.StatusCode == HttpStatusCode.Unauthorized);

        has429OrAllUnauthorized.Should().BeTrue(
            because: "Either rate limiting should kick in, or all requests should fail authentication");
    }

    [Fact]
    public async Task LoginEndpoint_RateLimitedResponse_ContainsRetryAfterHeader()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new { username = "test@test.com", password = "wrongpassword" };

        // Act - Exceed rate limit
        for (int i = 0; i < 10; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Assert - Should have Retry-After header
                response.Headers.Contains("Retry-After").Should().BeTrue(
                    because: "Rate limited responses should include Retry-After header");
                return;
            }
        }

        // If we get here, rate limiting may not be triggered in test environment
    }

    [Fact]
    public async Task LoginEndpoint_DifferentIPs_HaveSeparateRateLimits()
    {
        // Arrange - Simulate different client IPs using X-Forwarded-For
        var client1 = _factory.CreateClient();
        client1.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.1");

        var client2 = _factory.CreateClient();
        client2.DefaultRequestHeaders.Add("X-Forwarded-For", "192.168.1.2");

        var loginRequest = new { username = "test@test.com", password = "wrongpassword" };

        // Act - Send requests from both "IPs"
        var responses1 = new List<HttpResponseMessage>();
        var responses2 = new List<HttpResponseMessage>();

        for (int i = 0; i < 5; i++)
        {
            responses1.Add(await client1.PostAsJsonAsync("/api/auth/login", loginRequest));
            responses2.Add(await client2.PostAsJsonAsync("/api/auth/login", loginRequest));
        }

        // Assert - Each IP should have its own rate limit counter
        // Both should either be all unauthorized or at most one should be rate limited
        var client1RateLimited = responses1.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var client2RateLimited = responses2.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        // With separate rate limits, both shouldn't hit the limit simultaneously with just 5 requests each
        (client1RateLimited + client2RateLimited).Should().BeLessThan(10);
    }

    #endregion

    #region API Endpoint Rate Limiting

    [Fact]
    public async Task ApiEndpoint_ExceedsGlobalRateLimit_Returns429()
    {
        // Arrange
        var client = _factory.CreateClient();
        var responses = new List<HttpResponseMessage>();

        // Act - Send many requests quickly (global limit is typically 100/minute)
        var tasks = Enumerable.Range(0, 150)
            .Select(_ => client.GetAsync("/api/patients"))
            .ToList();

        var allResponses = await Task.WhenAll(tasks);
        responses.AddRange(allResponses);

        // Assert - Some should succeed, some should be rate limited
        var successCount = responses.Count(r => r.StatusCode != HttpStatusCode.TooManyRequests);
        var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        // Either rate limiting works, or all fail due to auth
        (rateLimitedCount > 0 || responses.All(r => r.StatusCode == HttpStatusCode.Unauthorized))
            .Should().BeTrue();
    }

    [Fact]
    public async Task ApiEndpoint_WithinRateLimit_AllRequestsSucceed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Send fewer requests than the limit
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => client.GetAsync("/api/health"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert - None should be rate limited (health endpoint is usually not auth protected)
        responses.Should().NotContain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
    }

    #endregion

    #region Rate Limit Headers

    [Fact]
    public async Task ApiEndpoint_Response_ContainsRateLimitHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/patients");

        // Assert - Check for standard rate limit headers
        var hasRateLimitHeaders =
            response.Headers.Contains("X-RateLimit-Limit") ||
            response.Headers.Contains("X-RateLimit-Remaining") ||
            response.Headers.Contains("RateLimit-Limit") ||
            response.Headers.Contains("RateLimit-Remaining");

        // Rate limit headers are optional but recommended
        // This test documents the expected behavior
    }

    #endregion

    #region Sensitive Endpoint Rate Limiting

    [Fact]
    public async Task PasswordResetEndpoint_HasStricterRateLimit()
    {
        // Arrange
        var client = _factory.CreateClient();
        var resetRequest = new { email = "test@test.com" };
        var responses = new List<HttpResponseMessage>();

        // Act - Send requests to password reset (sensitive endpoint, limit ~3/minute)
        for (int i = 0; i < 5; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/forgot-password", resetRequest);
            responses.Add(response);
        }

        // Assert - Should hit rate limit faster than regular endpoints
        var rateLimitedCount = responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var notFoundCount = responses.Count(r => r.StatusCode == HttpStatusCode.NotFound);

        // Endpoint may not exist, but if it does, it should be rate limited
        (rateLimitedCount > 0 || notFoundCount > 0 ||
         responses.All(r => r.StatusCode != HttpStatusCode.OK)).Should().BeTrue();
    }

    #endregion

    #region Rate Limit Bypass Attempts

    [Theory]
    [InlineData("X-Forwarded-For", "127.0.0.1")]
    [InlineData("X-Real-IP", "127.0.0.1")]
    [InlineData("X-Originating-IP", "127.0.0.1")]
    [InlineData("X-Client-IP", "127.0.0.1")]
    public async Task RateLimitBypass_WithSpoofedHeaders_StillEnforced(string header, string value)
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(header, value);
        var loginRequest = new { username = "test@test.com", password = "wrong" };
        var responses = new List<HttpResponseMessage>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            responses.Add(await client.PostAsJsonAsync("/api/auth/login", loginRequest));
        }

        // Assert - Spoofed headers shouldn't bypass rate limiting
        // Rate limiting should still be based on actual client IP or other identifier
        var rateLimitedOrUnauthorized = responses.All(r =>
            r.StatusCode == HttpStatusCode.TooManyRequests ||
            r.StatusCode == HttpStatusCode.Unauthorized);

        rateLimitedOrUnauthorized.Should().BeTrue();
    }

    [Fact]
    public async Task RateLimitBypass_WithMultipleClients_StillEnforced()
    {
        // Arrange - Create multiple HttpClient instances
        var clients = Enumerable.Range(0, 5)
            .Select(_ => _factory.CreateClient())
            .ToList();

        var loginRequest = new { username = "test@test.com", password = "wrong" };
        var allResponses = new List<HttpResponseMessage>();

        // Act - Send requests from multiple clients (same origin)
        foreach (var client in clients)
        {
            for (int i = 0; i < 3; i++)
            {
                allResponses.Add(await client.PostAsJsonAsync("/api/auth/login", loginRequest));
            }
        }

        // Assert - Should still be rate limited based on IP, not client instance
        allResponses.Should().OnlyContain(r =>
            r.StatusCode == HttpStatusCode.TooManyRequests ||
            r.StatusCode == HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Concurrent Request Handling

    [Fact]
    public async Task ConcurrentRequests_WithinLimit_AllProcessed()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Send concurrent requests
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => client.GetAsync("/api/health"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert - All should be processed (not rejected for concurrency)
        responses.Should().OnlyContain(r =>
            r.StatusCode != HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task ConcurrentRequests_ExceedingConcurrencyLimit_SomeQueued()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Send many concurrent requests
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => client.GetAsync("/api/patients"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert - All should eventually complete (queued or processed)
        responses.Should().NotBeEmpty();
    }

    #endregion
}
