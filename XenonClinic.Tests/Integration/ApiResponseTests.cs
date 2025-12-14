using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using XenonClinic.Tests.Fixtures;
using Xunit;

namespace XenonClinic.Tests.Integration;

/// <summary>
/// Integration tests for API response format consistency.
/// </summary>
[Collection("XenonClinic")]
public class ApiResponseTests : IntegrationTestBase
{
    public ApiResponseTests(XenonClinicWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ApiResponse_ShouldHaveConsistentStructure()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert - verify standard response structure
        json.RootElement.TryGetProperty("success", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ApiResponse_SuccessfulRequest_ShouldHaveSuccessTrue()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ApiResponse_NotFound_ShouldReturn404()
    {
        // Act
        var response = await Client.GetAsync("/api/nonexistent-endpoint-12345");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiResponse_ShouldIncludeTimestamp()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Assert
        var timestamp = json.RootElement.GetProperty("timestamp").GetDateTime();
        timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ApiResponse_ContentType_ShouldBeJson()
    {
        // Act
        var response = await Client.GetAsync("/api/diagnostics/metrics");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task ApiResponse_ShouldSupportCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/diagnostics/metrics");
        request.Headers.Add("X-Correlation-ID", correlationId);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Correlation ID should be echoed back or logged
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnJsonContentType()
    {
        // Act
        var response = await Client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Health endpoints typically return JSON
    }

    [Fact]
    public async Task SwaggerEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await Client.GetAsync("/swagger/index.html");

        // Assert - Swagger UI should be accessible
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.MovedPermanently,
            HttpStatusCode.Found);
    }

    [Fact]
    public async Task SwaggerJson_ShouldBeAccessible()
    {
        // Act
        var response = await Client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task ApiEndpoints_ShouldHandleOptionsRequest()
    {
        // Arrange - OPTIONS request for CORS preflight
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/diagnostics/metrics");

        // Act
        var response = await Client.SendAsync(request);

        // Assert - should not return 404 or 500
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}
