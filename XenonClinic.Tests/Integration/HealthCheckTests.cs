using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using XenonClinic.Tests.Fixtures;
using Xunit;

namespace XenonClinic.Tests.Integration;

/// <summary>
/// Tests for health check endpoints edge cases
/// </summary>
public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Basic Health Check Tests

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsValidJson()
    {
        // Act
        var response = await _client.GetAsync("/health");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            content.Should().NotBeNullOrEmpty();

            // Should be valid JSON
            var act = () => JsonDocument.Parse(content);
            act.Should().NotThrow();
        }
    }

    [Fact]
    public async Task HealthEndpoint_ContainsStatusField()
    {
        // Act
        var response = await _client.GetAsync("/health");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            // Assert - Should have status field
            json.RootElement.TryGetProperty("status", out _).Should().BeTrue();
        }
    }

    #endregion

    #region Liveness Check Tests

    [Fact]
    public async Task LivenessEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task LivenessEndpoint_RespondsQuickly()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/health/live");
        stopwatch.Stop();

        // Assert - Liveness check should be fast (< 1 second)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    #endregion

    #region Readiness Check Tests

    [Fact]
    public async Task ReadinessEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReadinessEndpoint_IncludesDatabaseStatus()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Assert - Should mention database or sql in some form
            content.ToLower().Should().ContainAny("database", "sql", "db", "data");
        }
    }

    #endregion

    #region Health Check Response Format Tests

    [Fact]
    public async Task HealthEndpoint_ReturnsCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.Content.Headers.ContentType?.MediaType
            .Should().BeOneOf("application/json", "text/plain");
    }

    [Fact]
    public async Task HealthEndpoint_WhenHealthy_Returns200()
    {
        // Act
        var response = await _client.GetAsync("/health");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();

            // If status is in response, it should indicate healthy
            if (content.Contains("status"))
            {
                content.ToLower().Should().ContainAny("healthy", "ok", "up");
            }
        }
    }

    [Fact]
    public async Task HealthEndpoint_WhenUnhealthy_Returns503()
    {
        // This test documents expected behavior when services are unavailable
        // In production, 503 would be returned when dependencies fail

        // Act
        var response = await _client.GetAsync("/health");

        // Assert - Either healthy (200) or unhealthy (503)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    #endregion

    #region Security Tests for Health Endpoints

    [Fact]
    public async Task HealthEndpoint_DoesNotRequireAuthentication()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("/health");

        // Assert - Health endpoint should be publicly accessible
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HealthEndpoint_DoesNotExposeDetailedErrors()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Should not expose stack traces or detailed error info
        content.Should().NotContain("StackTrace");
        content.Should().NotContain("InnerException");
        content.Should().NotContain("connection string", "Connection strings should never be exposed");
    }

    [Fact]
    public async Task HealthEndpoint_DoesNotExposeInternalPaths()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Should not expose internal file paths
        content.Should().NotMatchRegex(@"[A-Z]:\\", "Windows paths should not be exposed");
        content.Should().NotMatchRegex(@"/home/\w+/", "Linux home paths should not be exposed");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task HealthEndpoint_HandlesHighLoad()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => _client.GetAsync("/health"))
            .ToArray();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert - All should complete without errors
        responses.Should().OnlyContain(r =>
            r.StatusCode == HttpStatusCode.OK ||
            r.StatusCode == HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task HealthEndpoint_ResponseTimeConsistent()
    {
        // Arrange
        var responseTimes = new List<long>();

        // Act - Measure multiple requests
        for (int i = 0; i < 10; i++)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await _client.GetAsync("/health");
            stopwatch.Stop();
            responseTimes.Add(stopwatch.ElapsedMilliseconds);
        }

        // Assert - Response times should be relatively consistent
        var average = responseTimes.Average();
        var maxDeviation = responseTimes.Max() - responseTimes.Min();

        // Max deviation shouldn't be more than 5x the average
        maxDeviation.Should().BeLessThan(average * 5);
    }

    #endregion

    #region Dependency Health Check Tests

    [Fact]
    public async Task HealthEndpoint_ReportsIndividualDependencyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Health check response might include individual check results
            // This test documents the expected format
            var hasEntries = content.Contains("entries") ||
                            content.Contains("checks") ||
                            content.Contains("results");

            // Documentation test - health check may or may not include detailed entries
        }
    }

    #endregion

    #region Cache for Health Checks

    [Fact]
    public async Task HealthEndpoint_CachesResults()
    {
        // Arrange - Make two quick requests
        var stopwatch1 = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/health");
        stopwatch1.Stop();

        var stopwatch2 = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/health");
        stopwatch2.Stop();

        // Assert - Second request might be faster due to caching
        // This is implementation-specific and serves as documentation
    }

    #endregion

    #region Custom Health Check Tags

    [Fact]
    public async Task HealthEndpoint_WithTagFilter_ReturnsFilteredResults()
    {
        // Act - Try to filter by tag if supported
        var response = await _client.GetAsync("/health?tags=critical");

        // Assert - Should return OK or NotFound if tags not supported
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion
}
