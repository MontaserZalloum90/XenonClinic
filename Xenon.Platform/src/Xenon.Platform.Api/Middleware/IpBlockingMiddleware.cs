using System.Net;
using System.Text.Json;
using Xenon.Platform.Infrastructure.Services;

namespace Xenon.Platform.Api.Middleware;

/// <summary>
/// Middleware that blocks requests from IP addresses that have been flagged
/// for suspicious behavior. Integrates with IpBlockingService.
/// </summary>
public class IpBlockingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpBlockingMiddleware> _logger;

    public IpBlockingMiddleware(RequestDelegate next, ILogger<IpBlockingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IIpBlockingService ipBlockingService)
    {
        var ipAddress = GetClientIpAddress(context);

        if (!string.IsNullOrEmpty(ipAddress) && ipBlockingService.IsBlocked(ipAddress))
        {
            _logger.LogWarning(
                "Blocked request from IP {IpAddress} to {Path}",
                ipAddress, context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                error = "Your IP address has been temporarily blocked due to suspicious activity. Please try again later."
            };

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        await _next(context);
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded header first (for load balancers/proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the chain (original client)
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        // Check for real IP header (nginx)
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString();
    }
}

/// <summary>
/// Extension methods for registering IP blocking middleware.
/// </summary>
public static class IpBlockingMiddlewareExtensions
{
    public static IApplicationBuilder UseIpBlocking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IpBlockingMiddleware>();
    }
}
