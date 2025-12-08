namespace XenonClinic.Web.Middleware;

/// <summary>
/// Middleware to add security headers to all responses (similar to Helmet.js)
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
        // Add security headers before the response is sent
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            // X-Content-Type-Options: Prevents MIME type sniffing
            if (_options.EnableXContentTypeOptions)
            {
                headers["X-Content-Type-Options"] = "nosniff";
            }

            // X-Frame-Options: Prevents clickjacking
            if (_options.EnableXFrameOptions)
            {
                headers["X-Frame-Options"] = _options.XFrameOptionsValue;
            }

            // X-XSS-Protection: Legacy XSS protection for older browsers
            if (_options.EnableXXssProtection)
            {
                headers["X-XSS-Protection"] = "1; mode=block";
            }

            // Referrer-Policy: Controls referrer information
            if (_options.EnableReferrerPolicy)
            {
                headers["Referrer-Policy"] = _options.ReferrerPolicyValue;
            }

            // Content-Security-Policy: Prevents XSS and data injection attacks
            if (_options.EnableContentSecurityPolicy && !string.IsNullOrEmpty(_options.ContentSecurityPolicyValue))
            {
                headers["Content-Security-Policy"] = _options.ContentSecurityPolicyValue;
            }

            // Permissions-Policy (formerly Feature-Policy): Controls browser features
            if (_options.EnablePermissionsPolicy && !string.IsNullOrEmpty(_options.PermissionsPolicyValue))
            {
                headers["Permissions-Policy"] = _options.PermissionsPolicyValue;
            }

            // Strict-Transport-Security: Forces HTTPS
            if (_options.EnableHsts && context.Request.IsHttps)
            {
                var hstsValue = $"max-age={_options.HstsMaxAgeSeconds}";
                if (_options.HstsIncludeSubDomains) hstsValue += "; includeSubDomains";
                if (_options.HstsPreload) hstsValue += "; preload";
                headers["Strict-Transport-Security"] = hstsValue;
            }

            // X-Permitted-Cross-Domain-Policies: Controls Adobe Flash and PDF behavior
            if (_options.EnableXPermittedCrossDomainPolicies)
            {
                headers["X-Permitted-Cross-Domain-Policies"] = "none";
            }

            // Cache-Control for sensitive pages
            if (_options.EnableNoCacheForSensitivePages && IsSensitivePath(context.Request.Path))
            {
                headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
                headers["Pragma"] = "no-cache";
                headers["Expires"] = "0";
            }

            // Remove potentially dangerous headers
            if (_options.RemoveServerHeader)
            {
                headers.Remove("Server");
            }

            if (_options.RemoveXPoweredByHeader)
            {
                headers.Remove("X-Powered-By");
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private bool IsSensitivePath(PathString path)
    {
        var sensitivePatterns = new[]
        {
            "/api/auth",
            "/api/admin",
            "/api/patients",
            "/account",
            "/login",
            "/logout"
        };

        return sensitivePatterns.Any(p =>
            path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Configuration options for security headers
/// </summary>
public class SecurityHeadersOptions
{
    // X-Content-Type-Options
    public bool EnableXContentTypeOptions { get; set; } = true;

    // X-Frame-Options
    public bool EnableXFrameOptions { get; set; } = true;
    public string XFrameOptionsValue { get; set; } = "SAMEORIGIN"; // DENY, SAMEORIGIN, or ALLOW-FROM uri

    // X-XSS-Protection
    public bool EnableXXssProtection { get; set; } = true;

    // Referrer-Policy
    public bool EnableReferrerPolicy { get; set; } = true;
    public string ReferrerPolicyValue { get; set; } = "strict-origin-when-cross-origin";

    // Content-Security-Policy
    public bool EnableContentSecurityPolicy { get; set; } = true;
    public string ContentSecurityPolicyValue { get; set; } =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self' http://localhost:* https://localhost:* ws://localhost:* wss://localhost:*; " +
        "frame-ancestors 'self'; " +
        "form-action 'self'; " +
        "base-uri 'self'; " +
        "object-src 'none'";

    // Permissions-Policy
    public bool EnablePermissionsPolicy { get; set; } = true;
    public string PermissionsPolicyValue { get; set; } =
        "accelerometer=(), " +
        "camera=(), " +
        "geolocation=(), " +
        "gyroscope=(), " +
        "magnetometer=(), " +
        "microphone=(), " +
        "payment=(), " +
        "usb=()";

    // Strict-Transport-Security (HSTS)
    public bool EnableHsts { get; set; } = true;
    public int HstsMaxAgeSeconds { get; set; } = 31536000; // 1 year
    public bool HstsIncludeSubDomains { get; set; } = true;
    public bool HstsPreload { get; set; } = false;

    // X-Permitted-Cross-Domain-Policies
    public bool EnableXPermittedCrossDomainPolicies { get; set; } = true;

    // Cache control for sensitive pages
    public bool EnableNoCacheForSensitivePages { get; set; } = true;

    // Remove potentially dangerous headers
    public bool RemoveServerHeader { get; set; } = true;
    public bool RemoveXPoweredByHeader { get; set; } = true;
}

/// <summary>
/// Extension methods for SecurityHeadersMiddleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder builder,
        SecurityHeadersOptions? options = null)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>(options ?? new SecurityHeadersOptions());
    }

    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder builder,
        Action<SecurityHeadersOptions> configureOptions)
    {
        var options = new SecurityHeadersOptions();
        configureOptions(options);
        return builder.UseMiddleware<SecurityHeadersMiddleware>(options);
    }
}
