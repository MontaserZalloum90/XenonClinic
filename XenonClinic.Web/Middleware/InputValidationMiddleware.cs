using System.Text.RegularExpressions;

namespace XenonClinic.Web.Middleware;

/// <summary>
/// Input validation and sanitization middleware to prevent XSS and injection attacks
/// </summary>
public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;
    private readonly InputValidationOptions _options;

    public InputValidationMiddleware(
        RequestDelegate next,
        ILogger<InputValidationMiddleware> logger,
        InputValidationOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new InputValidationOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for excluded paths
        if (ShouldSkipValidation(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Validate query string parameters
        if (_options.ValidateQueryStrings)
        {
            foreach (var param in context.Request.Query)
            {
                if (!ValidateInput(param.Value.ToString(), out var violation))
                {
                    _logger.LogWarning(
                        "Potentially malicious query parameter detected: {Key}={Value}. Violation: {Violation}",
                        param.Key,
                        param.Value,
                        violation);

                    if (_options.BlockMaliciousRequests)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            StatusCode = 400,
                            Message = "Invalid input detected",
                            Parameter = param.Key
                        });
                        return;
                    }
                }
            }
        }

        // Validate headers
        if (_options.ValidateHeaders)
        {
            foreach (var header in _options.HeadersToValidate)
            {
                if (context.Request.Headers.TryGetValue(header, out var headerValue))
                {
                    if (!ValidateInput(headerValue.ToString(), out var violation))
                    {
                        _logger.LogWarning(
                            "Potentially malicious header detected: {Header}. Violation: {Violation}",
                            header,
                            violation);

                        if (_options.BlockMaliciousRequests)
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                StatusCode = 400,
                                Message = "Invalid header value detected"
                            });
                            return;
                        }
                    }
                }
            }
        }

        // Validate path
        if (_options.ValidatePath)
        {
            if (!ValidatePath(context.Request.Path, out var violation))
            {
                _logger.LogWarning(
                    "Potentially malicious path detected: {Path}. Violation: {Violation}",
                    context.Request.Path,
                    violation);

                if (_options.BlockMaliciousRequests)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        StatusCode = 400,
                        Message = "Invalid request path"
                    });
                    return;
                }
            }
        }

        await _next(context);
    }

    private bool ShouldSkipValidation(PathString path)
    {
        return _options.ExcludedPaths.Any(p =>
            path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private bool ValidateInput(string input, out string? violation)
    {
        violation = null;

        if (string.IsNullOrEmpty(input))
            return true;

        // Check for SQL injection patterns
        if (_options.CheckSqlInjection && ContainsSqlInjection(input))
        {
            violation = "SQL injection pattern detected";
            return false;
        }

        // Check for XSS patterns
        if (_options.CheckXss && ContainsXss(input))
        {
            violation = "XSS pattern detected";
            return false;
        }

        // Check for path traversal
        if (_options.CheckPathTraversal && ContainsPathTraversal(input))
        {
            violation = "Path traversal pattern detected";
            return false;
        }

        // Check for command injection
        if (_options.CheckCommandInjection && ContainsCommandInjection(input))
        {
            violation = "Command injection pattern detected";
            return false;
        }

        return true;
    }

    private bool ValidatePath(string path, out string? violation)
    {
        violation = null;

        // Check for null bytes
        if (path.Contains('\0'))
        {
            violation = "Null byte in path";
            return false;
        }

        // Check for path traversal
        if (ContainsPathTraversal(path))
        {
            violation = "Path traversal attempt";
            return false;
        }

        // Check for double encoding
        if (path.Contains("%25") || path.Contains("%2e%2e") || path.Contains("%252e"))
        {
            violation = "Double encoding detected";
            return false;
        }

        return true;
    }

    private static bool ContainsSqlInjection(string input)
    {
        var sqlPatterns = new[]
        {
            @"(\s|^)(SELECT|INSERT|UPDATE|DELETE|DROP|TRUNCATE|ALTER|CREATE|EXEC|EXECUTE|UNION|DECLARE)(\s|$)",
            @"(--)|(;)|(/\*)|(\*/)",
            @"(\s|^)(OR|AND)(\s+)[\w\d]+(\s*)(=|<|>)",
            @"'(\s*)OR(\s+)'",
            @"(xp_|sp_|0x)",
            @"WAITFOR(\s+)DELAY",
            @"BENCHMARK\s*\(",
            @"SLEEP\s*\("
        };

        foreach (var pattern in sqlPatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                return true;
        }

        return false;
    }

    private static bool ContainsXss(string input)
    {
        var xssPatterns = new[]
        {
            @"<script[^>]*>",
            @"</script>",
            @"javascript:",
            @"vbscript:",
            @"on\w+\s*=",
            @"<iframe",
            @"<object",
            @"<embed",
            @"<svg[^>]*onload",
            @"expression\s*\(",
            @"url\s*\(\s*['""]?\s*data:",
            @"<img[^>]+onerror",
            @"<body[^>]+onload"
        };

        foreach (var pattern in xssPatterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                return true;
        }

        return false;
    }

    private static bool ContainsPathTraversal(string input)
    {
        var patterns = new[]
        {
            @"\.\./",
            @"\.\.\\",
            @"%2e%2e%2f",
            @"%2e%2e/",
            @"\.%2e/",
            @"%2e\./",
            @"\.\.%5c",
            @"\.\.%c0%af",
            @"\.\.%c1%9c"
        };

        foreach (var pattern in patterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                return true;
        }

        return false;
    }

    private static bool ContainsCommandInjection(string input)
    {
        var patterns = new[]
        {
            @"[;&|`$]",
            @"\$\(.*\)",
            @"`.*`",
            @"\|\|",
            @"&&",
            @">\s*/dev/",
            @"<\s*/etc/",
            @"\bcat\b.*\b/etc/",
            @"\bwget\b",
            @"\bcurl\b.*-[oO]",
            @"\bnc\b.*-[el]"
        };

        foreach (var pattern in patterns)
        {
            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                return true;
        }

        return false;
    }
}

/// <summary>
/// Options for input validation middleware
/// </summary>
public class InputValidationOptions
{
    public bool ValidateQueryStrings { get; set; } = true;
    public bool ValidateHeaders { get; set; } = true;
    public bool ValidatePath { get; set; } = true;
    public bool BlockMaliciousRequests { get; set; } = true;
    public bool CheckSqlInjection { get; set; } = true;
    public bool CheckXss { get; set; } = true;
    public bool CheckPathTraversal { get; set; } = true;
    public bool CheckCommandInjection { get; set; } = true;

    public string[] ExcludedPaths { get; set; } = new[]
    {
        "/swagger",
        "/api/docs",
        "/health",
        "/metrics"
    };

    public string[] HeadersToValidate { get; set; } = new[]
    {
        "Referer",
        "User-Agent",
        "X-Forwarded-For",
        "X-Forwarded-Host"
    };
}

/// <summary>
/// Extension methods for InputValidationMiddleware
/// </summary>
public static class InputValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseInputValidation(
        this IApplicationBuilder builder,
        InputValidationOptions? options = null)
    {
        return builder.UseMiddleware<InputValidationMiddleware>(options ?? new InputValidationOptions());
    }

    public static IApplicationBuilder UseInputValidation(
        this IApplicationBuilder builder,
        Action<InputValidationOptions> configureOptions)
    {
        var options = new InputValidationOptions();
        configureOptions(options);
        return builder.UseMiddleware<InputValidationMiddleware>(options);
    }
}
