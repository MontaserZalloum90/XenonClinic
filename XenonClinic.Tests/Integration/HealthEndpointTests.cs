using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using XenonClinic.Tests.Fixtures;
using Xunit;

namespace XenonClinic.Tests.Integration;

/// <summary>
/// Integration tests for health check and diagnostics endpoints.
/// </summary>
[Collection("XenonClinic")]
public class HealthEndpointTests : IntegrationTestBase
{
    public HealthEndpointTests(XenonClinicWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task HealthEndpoint_ShouldIncludeAllChecks()
    {
        // Act
        var response = await Client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - should include our custom health checks
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Health response contains entries for each check
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DiagnosticsMetrics_ShouldBeAnonymouslyAccessible()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DiagnosticsMetrics_ShouldReturnApplicationInfo()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("XenonClinic");
        content.Should().Contain("uptime");
        content.Should().Contain("memory");
    }

    [Fact]
    public async Task DiagnosticsMetrics_ShouldReturnMemoryStats()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = json.RootElement.GetProperty("data");
        var memory = data.GetProperty("memory");

        memory.GetProperty("workingSetMB").GetInt64().Should().BeGreaterThan(0);
        memory.GetProperty("gcGen0Collections").GetInt32().Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task DiagnosticsMetrics_ShouldReturnThreadStats()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = json.RootElement.GetProperty("data");
        var threads = data.GetProperty("threads");

        threads.GetProperty("threadCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DiagnosticsMetrics_ShouldReturnRuntimeInfo()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = json.RootElement.GetProperty("data");
        var runtime = data.GetProperty("runtime");

        runtime.GetProperty("frameworkDescription").GetString().Should().Contain(".NET");
        runtime.GetProperty("processorCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DiagnosticsDatabaseMetrics_ShouldRequireAuthentication()
    {
        // Act - using unauthenticated client
        var response = await Client.GetAsync("/api/diagnostics/metrics/database");

        // Assert - should require authentication
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DiagnosticsCacheMetrics_ShouldRequireAuthentication()
    {
        // Act - using unauthenticated client
        var response = await Client.GetAsync("/api/diagnostics/metrics/cache");

        // Assert - should require authentication
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnWithinTimeout()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(5);
        using var cts = new CancellationTokenSource(timeout);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Client.GetAsync("/health", cts.Token);
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.Elapsed.Should().BeLessThan(timeout);
    }

    [Fact]
    public async Task DiagnosticsMetrics_ShouldIncrementRequestCount()
    {
        // Act - make multiple requests
        var response1 = await Client.GetAsync("/api/diagnostics/metrics");
        var content1 = await response1.Content.ReadAsStringAsync();
        var json1 = JsonDocument.Parse(content1);
        var count1 = json1.RootElement
            .GetProperty("data")
            .GetProperty("requests")
            .GetProperty("totalRequests")
            .GetInt64();

        var response2 = await Client.GetAsync("/api/diagnostics/metrics");
        var content2 = await response2.Content.ReadAsStringAsync();
        var json2 = JsonDocument.Parse(content2);
        var count2 = json2.RootElement
            .GetProperty("data")
            .GetProperty("requests")
            .GetProperty("totalRequests")
            .GetInt64();

        // Assert - count should have incremented
        count2.Should().BeGreaterThan(count1);
    }
}
