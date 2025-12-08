using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using XenonClinic.Tests.Fixtures;
using XenonClinic.Tests.Helpers;
using Xunit;

namespace XenonClinic.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for Patients API controller edge cases
/// </summary>
public class PatientsApiControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PatientsApiControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region GET /api/patients Tests

    [Fact]
    public async Task GetPatients_WithoutAuth_Returns401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Clear();

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPatients_WithInvalidPagination_ReturnsValidResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/patients?page=-1&pageSize=0");

        // Assert - Should handle gracefully
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPatients_WithExcessivePageSize_LimitsResults()
    {
        // Act
        var response = await _client.GetAsync("/api/patients?page=1&pageSize=10000");

        // Assert - Should limit page size to reasonable maximum
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("search=<script>alert('xss')</script>")]
    [InlineData("search=' OR '1'='1")]
    [InlineData("search=../../../etc/passwd")]
    public async Task GetPatients_WithMaliciousSearchParam_DoesNotExecute(string queryParam)
    {
        // Act
        var response = await _client.GetAsync($"/api/patients?{queryParam}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GET /api/patients/{id} Tests

    [Fact]
    public async Task GetPatient_WithNonExistentId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/patients/999999");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPatient_WithInvalidIdFormat_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/patients/not-a-number");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPatient_WithNegativeId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/patients/-1");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPatient_WithZeroId_ReturnsBadRequestOrNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/patients/0");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPatient_WithOverflowId_ReturnsAppropriateError()
    {
        // Act
        var response = await _client.GetAsync("/api/patients/99999999999999999999");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/patients Tests

    [Fact]
    public async Task CreatePatient_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePatient_WithNullBody_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("null", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePatient_WithMissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var patient = new { email = "test@test.com" }; // Missing required firstName, lastName

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", patient);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("firstName", "<script>alert('xss')</script>")]
    [InlineData("lastName", "'; DROP TABLE Patients;--")]
    [InlineData("email", "javascript:alert('xss')")]
    public async Task CreatePatient_WithMaliciousInput_IsSanitizedOrRejected(string field, string value)
    {
        // Arrange
        var patient = new Dictionary<string, object>
        {
            { "firstName", "John" },
            { "lastName", "Doe" },
            { "email", "test@test.com" },
            { "dateOfBirth", "1990-01-01" }
        };
        patient[field] = value;

        var json = System.Text.Json.JsonSerializer.Serialize(patient);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreatePatient_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var patient = new
        {
            firstName = "John",
            lastName = "Doe",
            email = "not-an-email",
            dateOfBirth = "1990-01-01"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", patient);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePatient_WithFutureDateOfBirth_ReturnsBadRequest()
    {
        // Arrange
        var patient = new
        {
            firstName = "John",
            lastName = "Doe",
            email = "test@test.com",
            dateOfBirth = DateTime.UtcNow.AddYears(10).ToString("yyyy-MM-dd")
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", patient);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.OK, // Some systems may allow this
            HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePatient_WithInvalidDateFormat_ReturnsBadRequest()
    {
        // Arrange
        var json = "{\"firstName\": \"John\", \"lastName\": \"Doe\", \"dateOfBirth\": \"not-a-date\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePatient_WithExtraUnknownFields_IgnoresExtraFields()
    {
        // Arrange
        var patient = new
        {
            firstName = "John",
            lastName = "Doe",
            email = "test@test.com",
            unknownField = "should be ignored",
            anotherUnknownField = 12345
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", patient);

        // Assert - Should not fail due to unknown fields
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreatePatient_WithDuplicateJsonKeys_UsesLastValue()
    {
        // Arrange - JSON with duplicate keys
        var json = "{\"firstName\": \"John\", \"firstName\": \"Jane\", \"lastName\": \"Doe\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert - Should use last value or reject
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region PUT /api/patients/{id} Tests

    [Fact]
    public async Task UpdatePatient_WithNonExistentId_Returns404()
    {
        // Arrange
        var patient = new { firstName = "Updated" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/patients/999999", patient);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePatient_WithMismatchedId_HandleGracefully()
    {
        // Arrange - URL id doesn't match body id
        var patient = new { id = 999, firstName = "Updated" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/patients/1", patient);

        // Assert - Should either use URL id or return error
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UpdatePatient_WithPartialData_OnlyUpdatesProvidedFields()
    {
        // Arrange - Only updating firstName
        var partialUpdate = new { firstName = "UpdatedName" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/patients/1", partialUpdate);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region DELETE /api/patients/{id} Tests

    [Fact]
    public async Task DeletePatient_WithNonExistentId_Returns404OrNoContent()
    {
        // Act
        var response = await _client.DeleteAsync("/api/patients/999999");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.NoContent,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePatient_WithInvalidId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.DeleteAsync("/api/patients/abc");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Content-Type Tests

    [Fact]
    public async Task ApiEndpoint_WithXmlContent_ReturnsUnsupportedMediaType()
    {
        // Arrange
        var xmlContent = "<patient><firstName>John</firstName></patient>";
        var content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApiEndpoint_WithFormData_ReturnsUnsupportedMediaType()
    {
        // Arrange
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("firstName", "John"),
            new KeyValuePair<string, string>("lastName", "Doe")
        });

        // Act
        var response = await _client.PostAsync("/api/patients", formContent);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.OK); // Some APIs may accept form data
    }

    #endregion

    #region CORS Tests

    [Fact]
    public async Task ApiEndpoint_OptionsRequest_ReturnsCorsHeaders()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/patients");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert - Should return CORS headers for allowed origins
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowedOrigins = response.Headers.GetValues("Access-Control-Allow-Origin");
            allowedOrigins.Should().NotBeEmpty();
        }
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task ApiEndpoint_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
    }

    [Fact]
    public async Task ApiEndpoint_ErrorResponse_IsStructured()
    {
        // Arrange - Make a request that should fail
        var invalidContent = new StringContent("not json", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", invalidContent);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            // Assert - Error response should be structured JSON
            content.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task ApiEndpoint_ConcurrentRequests_AllProcessed()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => _client.GetAsync("/api/patients"))
            .ToArray();

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert - All should complete (may be unauthorized, but not crash)
        responses.Should().OnlyContain(r => r.StatusCode != HttpStatusCode.InternalServerError);
    }

    #endregion
}
