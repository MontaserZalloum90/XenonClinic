using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace XenonClinic.Api.Security;

/// <summary>
/// Rate limiting configuration and policies for the API.
/// </summary>
public static class RateLimitingConfiguration
{
    public const string GlobalPolicy = "global";
    public const string AuthPolicy = "auth";
    public const string PublicPolicy = "public";
    public const string ApiPolicy = "api";
    public const string SensitivePolicy = "sensitive";

    /// <summary>
    /// Adds rate limiting services with predefined policies.
    /// </summary>
    public static IServiceCollection AddXenonRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection("RateLimiting").Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global rate limiter - applies to all requests
            limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var clientId = GetClientIdentifier(context);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = options.GlobalPermitLimit,
                        Window = TimeSpan.FromMinutes(options.GlobalWindowMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = options.GlobalQueueLimit
                    });
            });

            // Auth endpoints - stricter limits for login/signup
            limiterOptions.AddFixedWindowLimiter(AuthPolicy, authOptions =>
            {
                authOptions.AutoReplenishment = true;
                authOptions.PermitLimit = options.AuthPermitLimit;
                authOptions.Window = TimeSpan.FromMinutes(options.AuthWindowMinutes);
                authOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                authOptions.QueueLimit = 0; // No queuing for auth
            });

            // Public endpoints - moderate limits
            limiterOptions.AddFixedWindowLimiter(PublicPolicy, publicOptions =>
            {
                publicOptions.AutoReplenishment = true;
                publicOptions.PermitLimit = options.PublicPermitLimit;
                publicOptions.Window = TimeSpan.FromMinutes(options.PublicWindowMinutes);
                publicOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                publicOptions.QueueLimit = options.PublicQueueLimit;
            });

            // API endpoints - standard limits
            limiterOptions.AddTokenBucketLimiter(ApiPolicy, apiOptions =>
            {
                apiOptions.AutoReplenishment = true;
                apiOptions.TokenLimit = options.ApiTokenLimit;
                apiOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(options.ApiReplenishmentSeconds);
                apiOptions.TokensPerPeriod = options.ApiTokensPerPeriod;
                apiOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                apiOptions.QueueLimit = options.ApiQueueLimit;
            });

            // Sensitive endpoints - very strict limits
            limiterOptions.AddSlidingWindowLimiter(SensitivePolicy, sensitiveOptions =>
            {
                sensitiveOptions.AutoReplenishment = true;
                sensitiveOptions.PermitLimit = options.SensitivePermitLimit;
                sensitiveOptions.Window = TimeSpan.FromMinutes(options.SensitiveWindowMinutes);
                sensitiveOptions.SegmentsPerWindow = 4;
                sensitiveOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                sensitiveOptions.QueueLimit = 0;
            });

            // Custom rejection response
            limiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds
                    : 60;

                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter).ToString();

                var response = new
                {
                    success = false,
                    error = "Too many requests. Please try again later.",
                    retryAfterSeconds = (int)retryAfter
                };

                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };
        });

        return services;
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get authenticated user ID first
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("sub")?.Value
                ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }
        }

        // Fall back to IP address
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            return $"ip:{forwarded.Split(',')[0].Trim()}";
        }

        return $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
    }
}

/// <summary>
/// Rate limiting configuration options.
/// </summary>
public class RateLimitingOptions
{
    // Global limits
    public int GlobalPermitLimit { get; set; } = 100;
    public int GlobalWindowMinutes { get; set; } = 1;
    public int GlobalQueueLimit { get; set; } = 10;

    // Auth limits (login, signup, password reset)
    public int AuthPermitLimit { get; set; } = 10;
    public int AuthWindowMinutes { get; set; } = 1;

    // Public endpoint limits
    public int PublicPermitLimit { get; set; } = 30;
    public int PublicWindowMinutes { get; set; } = 1;
    public int PublicQueueLimit { get; set; } = 2;

    // API limits (token bucket)
    public int ApiTokenLimit { get; set; } = 100;
    public int ApiReplenishmentSeconds { get; set; } = 10;
    public int ApiTokensPerPeriod { get; set; } = 20;
    public int ApiQueueLimit { get; set; } = 5;

    // Sensitive endpoint limits
    public int SensitivePermitLimit { get; set; } = 5;
    public int SensitiveWindowMinutes { get; set; } = 1;
}
