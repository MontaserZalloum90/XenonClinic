using System.Diagnostics;
using System.Text;

namespace XenonClinic.Web.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with correlation IDs
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly RequestResponseLoggingOptions _options;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        RequestResponseLoggingOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new RequestResponseLoggingOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or extract correlation ID
        var correlationId = GetOrGenerateCorrelationId(context);
        context.Items["CorrelationId"] = correlationId;

        // Add correlation ID to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            return Task.CompletedTask;
        });

        // Skip logging for excluded paths
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestBody = string.Empty;
        var originalResponseBodyStream = context.Response.Body;

        try
        {
            // Log request
            if (_options.LogRequestBody && context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();
                requestBody = await ReadRequestBodyAsync(context.Request);
            }

            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Log request start
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestMethod"] = context.Request.Method,
                ["RequestPath"] = context.Request.Path.ToString(),
                ["RequestQueryString"] = context.Request.QueryString.ToString(),
                ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
                ["ClientIP"] = GetClientIpAddress(context)
            }))
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path} started. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);

                if (_options.LogRequestBody && !string.IsNullOrEmpty(requestBody))
                {
                    var sanitizedBody = SanitizeBody(requestBody);
                    _logger.LogDebug("Request body: {Body}", sanitizedBody);
                }

                // Execute the next middleware
                await _next(context);

                stopwatch.Stop();

                // Read response body
                var responseBody = string.Empty;
                if (_options.LogResponseBody)
                {
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                }

                // Copy response to original stream
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);

                // Log response
                var level = context.Response.StatusCode >= 500 ? LogLevel.Error
                    : context.Response.StatusCode >= 400 ? LogLevel.Warning
                    : LogLevel.Information;

                _logger.Log(level,
                    "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);

                if (_options.LogResponseBody && !string.IsNullOrEmpty(responseBody) && context.Response.StatusCode >= 400)
                {
                    var sanitizedResponse = SanitizeBody(responseBody);
                    _logger.LogDebug("Response body: {Body}", sanitizedResponse);
                }
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "HTTP {Method} {Path} failed with exception in {ElapsedMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                correlationId);

            context.Response.Body = originalResponseBodyStream;
            throw;
        }
    }

    private string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Check for existing correlation ID in headers
        foreach (var headerName in _options.CorrelationIdHeaders)
        {
            if (context.Request.Headers.TryGetValue(headerName, out var correlationId) &&
                !string.IsNullOrEmpty(correlationId))
            {
                return correlationId.ToString();
            }
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString("N")[..12];
    }

    private bool ShouldSkipLogging(PathString path)
    {
        return _options.ExcludedPaths.Any(p =>
            path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        request.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        // Truncate if too large
        if (body.Length > _options.MaxBodyLogLength)
        {
            body = body[.._options.MaxBodyLogLength] + "... [truncated]";
        }

        request.Body.Seek(0, SeekOrigin.Begin);
        return body;
    }

    private string SanitizeBody(string body)
    {
        if (string.IsNullOrEmpty(body))
            return body;

        // Truncate if too large
        if (body.Length > _options.MaxBodyLogLength)
        {
            body = body[.._options.MaxBodyLogLength] + "... [truncated]";
        }

        // Mask sensitive fields
        foreach (var field in _options.SensitiveFields)
        {
            body = MaskSensitiveField(body, field);
        }

        return body;
    }

    private static string MaskSensitiveField(string body, string fieldName)
    {
        // Simple regex-based masking for JSON fields
        var patterns = new[]
        {
            $@"""{fieldName}""\s*:\s*""[^""]*""",
            $@"'{fieldName}'\s*:\s*'[^']*'"
        };

        foreach (var pattern in patterns)
        {
            body = System.Text.RegularExpressions.Regex.Replace(
                body,
                pattern,
                $@"""{fieldName}"": ""[REDACTED]""",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return body;
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

/// <summary>
/// Options for request/response logging
/// </summary>
public class RequestResponseLoggingOptions
{
    public bool LogRequestBody { get; set; } = true;
    public bool LogResponseBody { get; set; } = true;
    public int MaxBodyLogLength { get; set; } = 4096;

    public string[] CorrelationIdHeaders { get; set; } = new[]
    {
        "X-Correlation-ID",
        "X-Request-ID",
        "Request-Id"
    };

    public string[] ExcludedPaths { get; set; } = new[]
    {
        "/health",
        "/healthz",
        "/metrics",
        "/swagger",
        "/api/docs",
        "/_framework",
        "/favicon.ico"
    };

    public string[] SensitiveFields { get; set; } = new[]
    {
        "password",
        "token",
        "secret",
        "apikey",
        "api_key",
        "authorization",
        "creditcard",
        "credit_card",
        "ssn",
        "social_security",
        "emiratesid"
    };
}

/// <summary>
/// Extension methods for RequestResponseLoggingMiddleware
/// </summary>
public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(
        this IApplicationBuilder builder,
        RequestResponseLoggingOptions? options = null)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>(options ?? new RequestResponseLoggingOptions());
    }

    public static IApplicationBuilder UseRequestResponseLogging(
        this IApplicationBuilder builder,
        Action<RequestResponseLoggingOptions> configureOptions)
    {
        var options = new RequestResponseLoggingOptions();
        configureOptions(options);
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>(options);
    }
}
