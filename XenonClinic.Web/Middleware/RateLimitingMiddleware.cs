using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace XenonClinic.Web.Middleware;

/// <summary>
/// Rate limiting configuration and policies for the application
/// </summary>
public static class RateLimitingConfiguration
{
    public const string GlobalPolicy = "global";
    public const string AuthPolicy = "auth";
    public const string ApiPolicy = "api";
    public const string SensitivePolicy = "sensitive";

    /// <summary>
    /// Configure rate limiting services
    /// </summary>
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitOptions = configuration.GetSection("RateLimiting").Get<RateLimitOptions>()
            ?? new RateLimitOptions();

        services.AddRateLimiter(options =>
        {
            // Global rejection handler
            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<RateLimitOptions>>();
                var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

                logger?.LogWarning(
                    "Rate limit exceeded for {Path} from IP {IP}. CorrelationId: {CorrelationId}",
                    context.HttpContext.Request.Path,
                    context.HttpContext.Connection.RemoteIpAddress,
                    correlationId);

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers["Retry-After"] =
                    context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? ((int)retryAfter.TotalSeconds).ToString()
                        : "60";

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    StatusCode = 429,
                    Message = "Too many requests. Please try again later.",
                    RetryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                        ? (int)retry.TotalSeconds
                        : 60,
                    CorrelationId = correlationId
                }, cancellationToken);
            };

            // Global rate limit policy - applies to all requests
            options.AddFixedWindowLimiter(GlobalPolicy, opt =>
            {
                opt.PermitLimit = rateLimitOptions.GlobalPermitLimit;
                opt.Window = TimeSpan.FromSeconds(rateLimitOptions.GlobalWindowSeconds);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = rateLimitOptions.GlobalQueueLimit;
            });

            // Authentication endpoints - stricter limits to prevent brute force
            options.AddSlidingWindowLimiter(AuthPolicy, opt =>
            {
                opt.PermitLimit = rateLimitOptions.AuthPermitLimit;
                opt.Window = TimeSpan.FromSeconds(rateLimitOptions.AuthWindowSeconds);
                opt.SegmentsPerWindow = 4;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0; // No queuing for auth requests
            });

            // API endpoints - general API rate limiting
            options.AddTokenBucketLimiter(ApiPolicy, opt =>
            {
                opt.TokenLimit = rateLimitOptions.ApiTokenLimit;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = rateLimitOptions.ApiQueueLimit;
                opt.ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitOptions.ApiReplenishmentSeconds);
                opt.TokensPerPeriod = rateLimitOptions.ApiTokensPerPeriod;
                opt.AutoReplenishment = true;
            });

            // Sensitive operations - very strict limits
            options.AddConcurrencyLimiter(SensitivePolicy, opt =>
            {
                opt.PermitLimit = rateLimitOptions.SensitivePermitLimit;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });

            // Use IP address as the partition key for client identification
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var clientIp = GetClientIpAddress(httpContext);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.GlobalPermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.GlobalWindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateLimitOptions.GlobalQueueLimit
                    });
            });
        });

        return services;
    }

    /// <summary>
    /// Get client IP address, accounting for proxies
    /// </summary>
    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers (when behind a proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the list (original client)
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

/// <summary>
/// Rate limiting options loaded from configuration
/// </summary>
public class RateLimitOptions
{
    // Global policy settings
    public int GlobalPermitLimit { get; set; } = 100;
    public int GlobalWindowSeconds { get; set; } = 60;
    public int GlobalQueueLimit { get; set; } = 10;

    // Auth policy settings (stricter for login/register)
    public int AuthPermitLimit { get; set; } = 5;
    public int AuthWindowSeconds { get; set; } = 60;

    // API policy settings (token bucket)
    public int ApiTokenLimit { get; set; } = 50;
    public int ApiQueueLimit { get; set; } = 5;
    public int ApiReplenishmentSeconds { get; set; } = 10;
    public int ApiTokensPerPeriod { get; set; } = 10;

    // Sensitive operations policy
    public int SensitivePermitLimit { get; set; } = 3;
}

/// <summary>
/// Attribute to apply specific rate limit policy to controllers or actions
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RateLimitPolicyAttribute : Attribute
{
    public string PolicyName { get; }

    public RateLimitPolicyAttribute(string policyName)
    {
        PolicyName = policyName;
    }
}
