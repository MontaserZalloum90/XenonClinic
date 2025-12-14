namespace XenonClinic.Api.Middleware;

/// <summary>
/// Middleware that adds security headers to all HTTP responses.
/// Implements OWASP recommended security headers for web applications.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersOptions? options = null)
    {
        _next = next;
        _options = options ?? new SecurityHeadersOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers before processing the request
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            // Prevent clickjacking attacks
            if (_options.EnableFrameOptions && !headers.ContainsKey("X-Frame-Options"))
            {
                headers["X-Frame-Options"] = _options.FrameOptions;
            }

            // Prevent MIME type sniffing
            if (_options.EnableContentTypeOptions && !headers.ContainsKey("X-Content-Type-Options"))
            {
                headers["X-Content-Type-Options"] = "nosniff";
            }

            // Enable XSS filtering in older browsers
            if (_options.EnableXssProtection && !headers.ContainsKey("X-XSS-Protection"))
            {
                headers["X-XSS-Protection"] = "1; mode=block";
            }

            // Control referrer information
            if (_options.EnableReferrerPolicy && !headers.ContainsKey("Referrer-Policy"))
            {
                headers["Referrer-Policy"] = _options.ReferrerPolicy;
            }

            // Content Security Policy
            if (_options.EnableContentSecurityPolicy && !headers.ContainsKey("Content-Security-Policy"))
            {
                headers["Content-Security-Policy"] = _options.ContentSecurityPolicy;
            }

            // Permissions Policy (formerly Feature-Policy)
            if (_options.EnablePermissionsPolicy && !headers.ContainsKey("Permissions-Policy"))
            {
                headers["Permissions-Policy"] = _options.PermissionsPolicy;
            }

            // Strict Transport Security (HSTS)
            if (_options.EnableHsts && context.Request.IsHttps && !headers.ContainsKey("Strict-Transport-Security"))
            {
                headers["Strict-Transport-Security"] = _options.HstsValue;
            }

            // Cache control for sensitive data
            if (_options.EnableCacheControl && IsSensitiveEndpoint(context.Request.Path))
            {
                headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
                headers["Pragma"] = "no-cache";
                headers["Expires"] = "0";
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static bool IsSensitiveEndpoint(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? "";
        return pathValue.Contains("/api/security") ||
               pathValue.Contains("/api/portal") ||
               pathValue.Contains("/api/patient") ||
               pathValue.Contains("/api/hr") ||
               pathValue.Contains("/api/financial");
    }
}

/// <summary>
/// Configuration options for security headers middleware.
/// </summary>
public class SecurityHeadersOptions
{
    /// <summary>
    /// Enable X-Frame-Options header. Default: true
    /// </summary>
    public bool EnableFrameOptions { get; set; } = true;

    /// <summary>
    /// X-Frame-Options header value. Default: DENY
    /// Options: DENY, SAMEORIGIN, ALLOW-FROM uri
    /// </summary>
    public string FrameOptions { get; set; } = "DENY";

    /// <summary>
    /// Enable X-Content-Type-Options header. Default: true
    /// </summary>
    public bool EnableContentTypeOptions { get; set; } = true;

    /// <summary>
    /// Enable X-XSS-Protection header. Default: true
    /// Note: Modern browsers have deprecated this in favor of CSP
    /// </summary>
    public bool EnableXssProtection { get; set; } = true;

    /// <summary>
    /// Enable Referrer-Policy header. Default: true
    /// </summary>
    public bool EnableReferrerPolicy { get; set; } = true;

    /// <summary>
    /// Referrer-Policy header value. Default: strict-origin-when-cross-origin
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Enable Content-Security-Policy header. Default: true
    /// </summary>
    public bool EnableContentSecurityPolicy { get; set; } = true;

    /// <summary>
    /// Content-Security-Policy header value.
    /// Default policy restricts content sources for security.
    /// </summary>
    public string ContentSecurityPolicy { get; set; } =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "form-action 'self'; " +
        "base-uri 'self'; " +
        "object-src 'none'";

    /// <summary>
    /// Enable Permissions-Policy header. Default: true
    /// </summary>
    public bool EnablePermissionsPolicy { get; set; } = true;

    /// <summary>
    /// Permissions-Policy header value.
    /// Restricts browser features that can be used.
    /// </summary>
    public string PermissionsPolicy { get; set; } =
        "accelerometer=(), " +
        "camera=(), " +
        "geolocation=(), " +
        "gyroscope=(), " +
        "magnetometer=(), " +
        "microphone=(), " +
        "payment=(), " +
        "usb=()";

    /// <summary>
    /// Enable Strict-Transport-Security header. Default: true
    /// Only applies to HTTPS requests.
    /// </summary>
    public bool EnableHsts { get; set; } = true;

    /// <summary>
    /// HSTS header value. Default: max-age of 1 year with includeSubDomains
    /// </summary>
    public string HstsValue { get; set; } = "max-age=31536000; includeSubDomains";

    /// <summary>
    /// Enable cache control headers for sensitive endpoints. Default: true
    /// </summary>
    public bool EnableCacheControl { get; set; } = true;
}

/// <summary>
/// Extension methods for security headers middleware.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds security headers middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }

    /// <summary>
    /// Adds security headers middleware with custom options.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder builder,
        SecurityHeadersOptions options)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>(options);
    }

    /// <summary>
    /// Adds security headers middleware with configuration action.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder builder,
        Action<SecurityHeadersOptions> configure)
    {
        var options = new SecurityHeadersOptions();
        configure(options);
        return builder.UseMiddleware<SecurityHeadersMiddleware>(options);
    }
}
