using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Middleware;

/// <summary>
/// Middleware that resolves tenant context from the authenticated user
/// and makes it available synchronously for DbContext query filters.
/// Must run after authentication middleware.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextAccessor tenantContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        try
        {
            // Only resolve tenant for authenticated requests
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user != null)
                {
                    tenantContextAccessor.SetTenantContext(
                        tenantId: user.TenantId,
                        companyId: user.CompanyId,
                        isSuperAdmin: user.IsSuperAdmin);

                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug(
                            "Tenant context resolved: TenantId={TenantId}, CompanyId={CompanyId}, IsSuperAdmin={IsSuperAdmin}",
                            user.TenantId, user.CompanyId, user.IsSuperAdmin);
                    }
                }
            }

            await _next(context);
        }
        finally
        {
            // Clean up tenant context at the end of the request
            tenantContextAccessor.Clear();
        }
    }
}

/// <summary>
/// Extension methods for registering the tenant resolution middleware.
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    /// <summary>
    /// Adds tenant resolution middleware to the pipeline.
    /// Must be called after UseAuthentication() and UseAuthorization().
    /// </summary>
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
