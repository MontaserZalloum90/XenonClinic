using System.Text.RegularExpressions;
using System.Web;

namespace XenonClinic.Api.Security;

/// <summary>
/// Service for sanitizing user input to prevent XSS, SQL injection, and other attacks.
/// </summary>
public interface IInputSanitizer
{
    /// <summary>
    /// Sanitizes a string input by removing dangerous characters.
    /// </summary>
    string Sanitize(string? input);

    /// <summary>
    /// Sanitizes HTML content while preserving safe tags.
    /// </summary>
    string SanitizeHtml(string? input);

    /// <summary>
    /// Validates and sanitizes an email address.
    /// </summary>
    (bool isValid, string? sanitized) SanitizeEmail(string? email);

    /// <summary>
    /// Validates and sanitizes a phone number.
    /// </summary>
    (bool isValid, string? sanitized) SanitizePhone(string? phone);

    /// <summary>
    /// Sanitizes a search query.
    /// </summary>
    string SanitizeSearchQuery(string? query);

    /// <summary>
    /// Validates that a string contains only allowed characters.
    /// </summary>
    bool IsValidInput(string? input, InputType inputType);
}

/// <summary>
/// Input types for validation.
/// </summary>
public enum InputType
{
    AlphaNumeric,
    Alpha,
    Numeric,
    Email,
    Phone,
    Url,
    Filename,
    Code // For codes like patient IDs, invoice numbers
}

/// <summary>
/// Implementation of input sanitization service.
/// </summary>
public class InputSanitizer : IInputSanitizer
{
    // Common dangerous patterns
    private static readonly Regex SqlInjectionPattern = new(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|ALTER|CREATE|EXEC|EXECUTE)\b)|(--)|(;)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex XssPattern = new(
        @"<script[^>]*>.*?</script>|<[^>]+on\w+\s*=|javascript:|vbscript:|data:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex HtmlTagPattern = new(
        @"<[^>]+>",
        RegexOptions.Compiled);

    private static readonly Regex EmailPattern = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    private static readonly Regex PhonePattern = new(
        @"^\+?[\d\s\-()]{7,20}$",
        RegexOptions.Compiled);

    private static readonly Regex AlphaNumericPattern = new(
        @"^[a-zA-Z0-9\s\-_]+$",
        RegexOptions.Compiled);

    private static readonly Regex AlphaPattern = new(
        @"^[a-zA-Z\s]+$",
        RegexOptions.Compiled);

    private static readonly Regex NumericPattern = new(
        @"^[\d]+$",
        RegexOptions.Compiled);

    private static readonly Regex UrlPattern = new(
        @"^https?://[^\s/$.?#].[^\s]*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex FilenamePattern = new(
        @"^[a-zA-Z0-9._\-]+$",
        RegexOptions.Compiled);

    private static readonly Regex CodePattern = new(
        @"^[a-zA-Z0-9\-_]+$",
        RegexOptions.Compiled);

    // Safe HTML tags for rich text
    private static readonly HashSet<string> SafeHtmlTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "b", "i", "u", "strong", "em", "ul", "ol", "li", "a", "span", "div"
    };

    public string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Trim whitespace
        var result = input.Trim();

        // Remove null bytes
        result = result.Replace("\0", string.Empty);

        // HTML encode to prevent XSS
        result = HttpUtility.HtmlEncode(result);

        // Remove SQL injection patterns
        result = SqlInjectionPattern.Replace(result, string.Empty);

        // Limit length
        if (result.Length > 10000)
            result = result[..10000];

        return result;
    }

    public string SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Remove script tags and event handlers
        var result = XssPattern.Replace(input, string.Empty);

        // Process HTML tags - keep only safe ones
        result = Regex.Replace(result, @"<(/?)(\w+)[^>]*>", match =>
        {
            var tagName = match.Groups[2].Value;
            if (SafeHtmlTags.Contains(tagName))
            {
                return $"<{match.Groups[1].Value}{tagName}>";
            }
            return string.Empty;
        });

        return result;
    }

    public (bool isValid, string? sanitized) SanitizeEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return (false, null);

        var trimmed = email.Trim().ToLowerInvariant();

        if (!EmailPattern.IsMatch(trimmed))
            return (false, null);

        // Additional checks
        if (trimmed.Length > 254)
            return (false, null);

        return (true, trimmed);
    }

    public (bool isValid, string? sanitized) SanitizePhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone))
            return (false, null);

        // Remove common formatting characters for validation
        var cleaned = Regex.Replace(phone.Trim(), @"[\s\-().]", string.Empty);

        if (!Regex.IsMatch(cleaned, @"^\+?\d{7,15}$"))
            return (false, null);

        // Return normalized format
        return (true, cleaned);
    }

    public string SanitizeSearchQuery(string? query)
    {
        if (string.IsNullOrEmpty(query))
            return string.Empty;

        var result = query.Trim();

        // Remove special regex characters
        result = Regex.Escape(result);

        // Remove SQL wildcards used maliciously
        result = result.Replace("%", string.Empty).Replace("_", string.Empty);

        // Limit length
        if (result.Length > 100)
            result = result[..100];

        return result;
    }

    public bool IsValidInput(string? input, InputType inputType)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return inputType switch
        {
            InputType.AlphaNumeric => AlphaNumericPattern.IsMatch(input),
            InputType.Alpha => AlphaPattern.IsMatch(input),
            InputType.Numeric => NumericPattern.IsMatch(input),
            InputType.Email => EmailPattern.IsMatch(input),
            InputType.Phone => PhonePattern.IsMatch(input),
            InputType.Url => UrlPattern.IsMatch(input),
            InputType.Filename => FilenamePattern.IsMatch(input) && !input.Contains(".."),
            InputType.Code => CodePattern.IsMatch(input),
            _ => false
        };
    }
}

/// <summary>
/// Extension methods for input sanitization.
/// </summary>
public static class InputSanitizerExtensions
{
    public static IServiceCollection AddInputSanitization(this IServiceCollection services)
    {
        services.AddSingleton<IInputSanitizer, InputSanitizer>();
        return services;
    }
}
