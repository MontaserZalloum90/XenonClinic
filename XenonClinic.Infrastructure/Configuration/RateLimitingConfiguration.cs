using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace XenonClinic.Infrastructure.Configuration;

/// <summary>
/// Rate limiting configuration to protect API from abuse.
/// </summary>
public static class RateLimitingConfiguration
{
    public const string FixedPolicy = "fixed";
    public const string SlidingPolicy = "sliding";
    public const string TokenBucketPolicy = "token";
    public const string ConcurrencyPolicy = "concurrency";

    /// <summary>
    /// Adds rate limiting services with predefined policies.
    /// </summary>
    public static IServiceCollection AddXenonRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global limiter - applies to all requests
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                // Use authenticated user ID or IP address
                var userId = context.User?.Identity?.Name;
                var key = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1000,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 50
                });
            });

            // Fixed window policy - simple rate limit
            options.AddFixedWindowLimiter(FixedPolicy, opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            // Sliding window policy - smoother rate limiting
            options.AddSlidingWindowLimiter(SlidingPolicy, opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow = 4;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            // Token bucket policy - allows bursting
            options.AddTokenBucketLimiter(TokenBucketPolicy, opt =>
            {
                opt.TokenLimit = 100;
                opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                opt.TokensPerPeriod = 20;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            // Concurrency policy - limits concurrent requests
            options.AddConcurrencyLimiter(ConcurrencyPolicy, opt =>
            {
                opt.PermitLimit = 50;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 25;
            });

            // Custom policy for authentication endpoints (stricter)
            options.AddPolicy("auth", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });

            // Custom policy for file uploads (stricter)
            options.AddPolicy("upload", context =>
            {
                var userId = context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(5),
                    QueueLimit = 5
                });
            });

            // On rejected - add headers
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.Headers.Add("X-RateLimit-RetryAfter", "60");

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.Add("Retry-After", retryAfter.TotalSeconds.ToString());
                }

                await context.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.",
                    cancellationToken: token);
            };
        });

        return services;
    }

    /// <summary>
    /// Adds rate limiting middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseXenonRateLimiting(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }
}
