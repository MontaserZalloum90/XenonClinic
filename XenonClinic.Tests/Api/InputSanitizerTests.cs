using FluentAssertions;
using XenonClinic.Api.Security;
using Xunit;

namespace XenonClinic.Tests.Api;

/// <summary>
/// Tests for the InputSanitizer security service.
/// </summary>
public class InputSanitizerTests
{
    private readonly IInputSanitizer _sanitizer;

    public InputSanitizerTests()
    {
        _sanitizer = new InputSanitizer();
    }

    #region Basic Sanitization Tests

    [Fact]
    public void Sanitize_NullInput_ReturnsEmpty()
    {
        var result = _sanitizer.Sanitize(null);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_EmptyInput_ReturnsEmpty()
    {
        var result = _sanitizer.Sanitize(string.Empty);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_WhitespaceInput_ReturnsEmpty()
    {
        var result = _sanitizer.Sanitize("   ");
        result.Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_NormalText_ReturnsUnchanged()
    {
        var input = "Hello World 123";
        var result = _sanitizer.Sanitize(input);
        result.Should().Be(input);
    }

    [Fact]
    public void Sanitize_TrimsWhitespace()
    {
        var result = _sanitizer.Sanitize("  Hello World  ");
        result.Should().Be("Hello World");
    }

    [Fact]
    public void Sanitize_RemovesNullBytes()
    {
        var input = "Hello\0World";
        var result = _sanitizer.Sanitize(input);
        result.Should().NotContain("\0");
    }

    [Fact]
    public void Sanitize_LongInput_TruncatesTo10000()
    {
        var input = new string('a', 15000);
        var result = _sanitizer.Sanitize(input);
        result.Length.Should().BeLessOrEqualTo(10000);
    }

    #endregion

    #region XSS Prevention Tests

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    [InlineData("javascript:alert('xss')")]
    public void Sanitize_XssPayload_EncodesHtml(string xssPayload)
    {
        var result = _sanitizer.Sanitize(xssPayload);
        result.Should().NotContain("<script");
        result.Should().NotContain("onerror=");
        result.Should().NotContain("javascript:");
    }

    [Fact]
    public void Sanitize_HtmlTags_AreEncoded()
    {
        var result = _sanitizer.Sanitize("<b>Bold</b>");
        result.Should().Contain("&lt;");
        result.Should().Contain("&gt;");
    }

    #endregion

    #region SQL Injection Prevention Tests

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("1; SELECT * FROM Users")]
    [InlineData("UNION SELECT password FROM users")]
    [InlineData("1' OR '1'='1")]
    public void Sanitize_SqlInjection_RemovesDangerousPatterns(string sqlPayload)
    {
        var result = _sanitizer.Sanitize(sqlPayload);
        result.ToUpperInvariant().Should().NotContain("DROP");
        result.ToUpperInvariant().Should().NotContain("UNION");
    }

    [Fact]
    public void Sanitize_SqlComments_Removed()
    {
        var result = _sanitizer.Sanitize("username -- comment");
        result.Should().NotContain("--");
    }

    #endregion

    #region HTML Sanitization Tests

    [Fact]
    public void SanitizeHtml_NullInput_ReturnsEmpty()
    {
        var result = _sanitizer.SanitizeHtml(null);
        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeHtml_SafeTags_Preserved()
    {
        var input = "<p>Paragraph</p><b>Bold</b><i>Italic</i>";
        var result = _sanitizer.SanitizeHtml(input);
        result.Should().Contain("<p>");
        result.Should().Contain("<b>");
        result.Should().Contain("<i>");
    }

    [Fact]
    public void SanitizeHtml_ScriptTags_Removed()
    {
        var input = "<p>Text</p><script>alert('xss')</script>";
        var result = _sanitizer.SanitizeHtml(input);
        result.Should().NotContain("<script");
        result.Should().Contain("<p>");
    }

    [Fact]
    public void SanitizeHtml_EventHandlers_Removed()
    {
        var input = "<img src='test.jpg' onerror='alert(1)'>";
        var result = _sanitizer.SanitizeHtml(input);
        result.Should().NotContain("onerror");
    }

    [Fact]
    public void SanitizeHtml_UnsafeTags_Removed()
    {
        var input = "<iframe src='evil.com'></iframe><p>Text</p>";
        var result = _sanitizer.SanitizeHtml(input);
        result.Should().NotContain("<iframe");
        result.Should().Contain("<p>");
    }

    #endregion

    #region Email Sanitization Tests

    [Fact]
    public void SanitizeEmail_ValidEmail_ReturnsValid()
    {
        var (isValid, sanitized) = _sanitizer.SanitizeEmail("Test@Example.Com");

        isValid.Should().BeTrue();
        sanitized.Should().Be("test@example.com");
    }

    [Fact]
    public void SanitizeEmail_NullEmail_ReturnsInvalid()
    {
        var (isValid, sanitized) = _sanitizer.SanitizeEmail(null);

        isValid.Should().BeFalse();
        sanitized.Should().BeNull();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@domain.com")]
    [InlineData("invalid@domain")]
    public void SanitizeEmail_InvalidEmail_ReturnsInvalid(string email)
    {
        var (isValid, _) = _sanitizer.SanitizeEmail(email);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void SanitizeEmail_TooLongEmail_ReturnsInvalid()
    {
        var longEmail = new string('a', 250) + "@example.com";
        var (isValid, _) = _sanitizer.SanitizeEmail(longEmail);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void SanitizeEmail_WithWhitespace_TrimsAndValidates()
    {
        var (isValid, sanitized) = _sanitizer.SanitizeEmail("  user@example.com  ");

        isValid.Should().BeTrue();
        sanitized.Should().Be("user@example.com");
    }

    #endregion

    #region Phone Sanitization Tests

    [Fact]
    public void SanitizePhone_ValidPhone_ReturnsNormalized()
    {
        var (isValid, sanitized) = _sanitizer.SanitizePhone("+1 (555) 123-4567");

        isValid.Should().BeTrue();
        sanitized.Should().Be("+15551234567");
    }

    [Fact]
    public void SanitizePhone_NullPhone_ReturnsInvalid()
    {
        var (isValid, sanitized) = _sanitizer.SanitizePhone(null);

        isValid.Should().BeFalse();
        sanitized.Should().BeNull();
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("abc123")] // Contains letters
    [InlineData("12345678901234567890")] // Too long
    public void SanitizePhone_InvalidPhone_ReturnsInvalid(string phone)
    {
        var (isValid, _) = _sanitizer.SanitizePhone(phone);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void SanitizePhone_InternationalFormat_Valid()
    {
        var (isValid, sanitized) = _sanitizer.SanitizePhone("+44 20 7946 0958");

        isValid.Should().BeTrue();
        sanitized.Should().Be("+442079460958");
    }

    #endregion

    #region Search Query Sanitization Tests

    [Fact]
    public void SanitizeSearchQuery_NullQuery_ReturnsEmpty()
    {
        var result = _sanitizer.SanitizeSearchQuery(null);
        result.Should().BeEmpty();
    }

    [Fact]
    public void SanitizeSearchQuery_NormalQuery_ReturnsEscaped()
    {
        var result = _sanitizer.SanitizeSearchQuery("John Doe");
        result.Should().Be("John Doe");
    }

    [Fact]
    public void SanitizeSearchQuery_SpecialChars_AreEscaped()
    {
        var result = _sanitizer.SanitizeSearchQuery("test.*");
        result.Should().Contain(@"\.");
        result.Should().Contain(@"\*");
    }

    [Fact]
    public void SanitizeSearchQuery_SqlWildcards_Removed()
    {
        var result = _sanitizer.SanitizeSearchQuery("test%_query");
        result.Should().NotContain("%");
        result.Should().NotContain("_");
    }

    [Fact]
    public void SanitizeSearchQuery_LongQuery_Truncated()
    {
        var longQuery = new string('a', 200);
        var result = _sanitizer.SanitizeSearchQuery(longQuery);
        result.Length.Should().BeLessOrEqualTo(100);
    }

    #endregion

    #region Input Type Validation Tests

    [Theory]
    [InlineData("Hello123", InputType.AlphaNumeric, true)]
    [InlineData("Hello_123-test", InputType.AlphaNumeric, true)]
    [InlineData("Hello@123", InputType.AlphaNumeric, false)]
    public void IsValidInput_AlphaNumeric_ValidatesCorrectly(string input, InputType type, bool expected)
    {
        var result = _sanitizer.IsValidInput(input, type);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Hello World", InputType.Alpha, true)]
    [InlineData("Hello123", InputType.Alpha, false)]
    public void IsValidInput_Alpha_ValidatesCorrectly(string input, InputType type, bool expected)
    {
        var result = _sanitizer.IsValidInput(input, type);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("12345", InputType.Numeric, true)]
    [InlineData("123.45", InputType.Numeric, false)]
    [InlineData("123abc", InputType.Numeric, false)]
    public void IsValidInput_Numeric_ValidatesCorrectly(string input, InputType type, bool expected)
    {
        var result = _sanitizer.IsValidInput(input, type);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("https://example.com", InputType.Url, true)]
    [InlineData("http://example.com/path", InputType.Url, true)]
    [InlineData("ftp://example.com", InputType.Url, false)]
    [InlineData("invalid-url", InputType.Url, false)]
    public void IsValidInput_Url_ValidatesCorrectly(string input, InputType type, bool expected)
    {
        var result = _sanitizer.IsValidInput(input, type);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("file.txt", InputType.Filename, true)]
    [InlineData("file-name_123.pdf", InputType.Filename, true)]
    [InlineData("../etc/passwd", InputType.Filename, false)]
    [InlineData("file with space.txt", InputType.Filename, false)]
    public void IsValidInput_Filename_ValidatesCorrectly(string input, InputType type, bool expected)
    {
        var result = _sanitizer.IsValidInput(input, type);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("PAT-001", InputType.Code, true)]
    [InlineData("INV_2024_001", InputType.Code, true)]
    [InlineData("Code With Space", InputType.Code, false)]
    public void IsValidInput_Code_ValidatesCorrectly(string input, InputType type, bool expected)
    {
        var result = _sanitizer.IsValidInput(input, type);
        result.Should().Be(expected);
    }

    [Fact]
    public void IsValidInput_NullInput_ReturnsFalse()
    {
        var result = _sanitizer.IsValidInput(null, InputType.AlphaNumeric);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidInput_EmptyInput_ReturnsFalse()
    {
        var result = _sanitizer.IsValidInput(string.Empty, InputType.AlphaNumeric);
        result.Should().BeFalse();
    }

    #endregion
}
