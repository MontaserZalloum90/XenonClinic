namespace XenonClinic.Api.Middleware;

/// <summary>
/// Middleware that manages correlation IDs for request tracing across services.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    public const string CorrelationIdHeader = "X-Correlation-ID";
    public const string RequestIdHeader = "X-Request-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        var requestId = Guid.NewGuid().ToString("N")[..12];

        // Set correlation ID in accessor for use throughout the request
        correlationIdAccessor.Set(correlationId, requestId);

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            context.Response.Headers[RequestIdHeader] = requestId;
            return Task.CompletedTask;
        });

        // Add to logging scope
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestId"] = requestId
        }))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingId) &&
            !string.IsNullOrEmpty(existingId))
        {
            return existingId!;
        }

        return Guid.NewGuid().ToString();
    }
}

/// <summary>
/// Provides access to the current correlation ID.
/// </summary>
public interface ICorrelationIdAccessor
{
    string CorrelationId { get; }
    string RequestId { get; }
    void Set(string correlationId, string requestId);
}

/// <summary>
/// Default implementation of correlation ID accessor.
/// </summary>
public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private string _correlationId = string.Empty;
    private string _requestId = string.Empty;

    public string CorrelationId => _correlationId;
    public string RequestId => _requestId;

    public void Set(string correlationId, string requestId)
    {
        _correlationId = correlationId;
        _requestId = requestId;
    }
}

/// <summary>
/// Extension methods for correlation ID middleware.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }

    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
        return services;
    }
}
