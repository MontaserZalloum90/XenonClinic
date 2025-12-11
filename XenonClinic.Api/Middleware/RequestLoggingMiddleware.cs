using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;

namespace XenonClinic.Api.Middleware;

/// <summary>
/// Middleware that logs all HTTP requests and responses with timing information.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        IOptions<RequestLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for excluded paths
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = GetOrCreateCorrelationId(context);
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Log request
        if (_options.LogRequests)
        {
            await LogRequest(context, correlationId, requestId);
        }

        // Capture response body if enabled
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();

        if (_options.LogResponseBody)
        {
            context.Response.Body = responseBody;
        }

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Log response
            if (_options.LogResponses)
            {
                await LogResponse(context, correlationId, requestId, stopwatch.ElapsedMilliseconds, responseBody, originalBodyStream);
            }
            else if (_options.LogResponseBody)
            {
                await CopyResponseBody(responseBody, originalBodyStream);
            }

            // Log slow requests
            if (stopwatch.ElapsedMilliseconds > _options.SlowRequestThresholdMs)
            {
                _logger.LogWarning(
                    "Slow request detected - CorrelationId: {CorrelationId}, RequestId: {RequestId}, " +
                    "Method: {Method}, Path: {Path}, Duration: {Duration}ms",
                    correlationId, requestId, context.Request.Method, context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Request failed - CorrelationId: {CorrelationId}, RequestId: {RequestId}, " +
                "Method: {Method}, Path: {Path}, Duration: {Duration}ms",
                correlationId, requestId, context.Request.Method, context.Request.Path,
                stopwatch.ElapsedMilliseconds);

            if (_options.LogResponseBody)
            {
                await CopyResponseBody(responseBody, originalBodyStream);
            }

            throw;
        }
        finally
        {
            if (_options.LogResponseBody)
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }

    private bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;
        return _options.ExcludedPaths.Any(p =>
            pathValue.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId) &&
            !string.IsNullOrEmpty(correlationId))
        {
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            return correlationId!;
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = newCorrelationId;
        return newCorrelationId;
    }

    private async Task LogRequest(HttpContext context, string correlationId, string requestId)
    {
        var request = context.Request;
        var logMessage = new StringBuilder();
        logMessage.AppendLine($"HTTP Request - CorrelationId: {correlationId}, RequestId: {requestId}");
        logMessage.AppendLine($"  Method: {request.Method}");
        logMessage.AppendLine($"  Path: {request.Path}{request.QueryString}");
        logMessage.AppendLine($"  ContentType: {request.ContentType}");
        logMessage.AppendLine($"  ContentLength: {request.ContentLength}");

        if (_options.LogRequestHeaders)
        {
            foreach (var header in request.Headers.Where(h => !IsSecureHeader(h.Key)))
            {
                logMessage.AppendLine($"  Header: {header.Key}: {header.Value}");
            }
        }

        if (_options.LogRequestBody && request.ContentLength > 0 && request.ContentLength < _options.MaxBodyLogLength)
        {
            request.EnableBuffering();
            var body = await ReadRequestBody(request);
            if (!string.IsNullOrEmpty(body))
            {
                logMessage.AppendLine($"  Body: {SanitizeBody(body)}");
            }
        }

        _logger.LogInformation(logMessage.ToString());
    }

    private async Task LogResponse(HttpContext context, string correlationId, string requestId,
        long durationMs, MemoryStream responseBody, Stream originalBodyStream)
    {
        var logMessage = new StringBuilder();
        logMessage.AppendLine($"HTTP Response - CorrelationId: {correlationId}, RequestId: {requestId}");
        logMessage.AppendLine($"  StatusCode: {context.Response.StatusCode}");
        logMessage.AppendLine($"  Duration: {durationMs}ms");

        if (_options.LogResponseBody && responseBody.Length > 0 && responseBody.Length < _options.MaxBodyLogLength)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            using var bodyReader = new StreamReader(responseBody, leaveOpen: true);
            var body = await bodyReader.ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                logMessage.AppendLine($"  Body: {SanitizeBody(body)}");
            }
        }

        _logger.LogInformation(logMessage.ToString());

        await CopyResponseBody(responseBody, originalBodyStream);
    }

    private static async Task CopyResponseBody(MemoryStream responseBody, Stream originalBodyStream)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private bool IsSecureHeader(string headerName)
    {
        var secureHeaders = new[] { "Authorization", "Cookie", "X-API-Key", "Api-Key" };
        return secureHeaders.Any(h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
    }

    private string SanitizeBody(string body)
    {
        if (string.IsNullOrEmpty(body)) return body;

        // Mask sensitive fields
        var sensitiveFields = new[] { "password", "token", "secret", "apiKey", "creditCard" };
        foreach (var field in sensitiveFields)
        {
            body = System.Text.RegularExpressions.Regex.Replace(
                body,
                $"\"{field}\"\\s*:\\s*\"[^\"]*\"",
                $"\"{field}\":\"***MASKED***\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        // Truncate if too long
        if (body.Length > _options.MaxBodyLogLength)
        {
            body = body[.._options.MaxBodyLogLength] + "...[TRUNCATED]";
        }

        return body;
    }
}

/// <summary>
/// Configuration options for request logging middleware.
/// </summary>
public class RequestLoggingOptions
{
    public bool LogRequests { get; set; } = true;
    public bool LogResponses { get; set; } = true;
    public bool LogRequestHeaders { get; set; } = false;
    public bool LogRequestBody { get; set; } = false;
    public bool LogResponseBody { get; set; } = false;
    public int MaxBodyLogLength { get; set; } = 4096;
    public int SlowRequestThresholdMs { get; set; } = 1000;
    public List<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/swagger",
        "/favicon.ico"
    };
}

/// <summary>
/// Extension methods for request logging middleware.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }

    public static IServiceCollection AddRequestLogging(this IServiceCollection services,
        Action<RequestLoggingOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<RequestLoggingOptions>(_ => { });
        }

        return services;
    }
}
