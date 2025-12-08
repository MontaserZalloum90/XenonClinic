using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Web.Middleware;

/// <summary>
/// Middleware to resolve and set the current tenant context for each request
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
        UserManager<ApplicationUser> userManager,
        ClinicDbContext dbContext)
    {
        // Skip tenant resolution for unauthenticated requests
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        try
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user != null)
            {
                // Set tenant context in HttpContext items for easy access
                context.Items["CurrentUserId"] = user.Id;
                context.Items["CurrentTenantId"] = user.TenantId;
                context.Items["CurrentCompanyId"] = user.CompanyId;
                context.Items["CurrentBranchId"] = user.PrimaryBranchId;
                context.Items["IsSuperAdmin"] = user.IsSuperAdmin;

                // Load tenant settings if user belongs to a tenant
                if (user.TenantId.HasValue)
                {
                    var tenant = await dbContext.Tenants
                        .Include(t => t.Settings)
                        .FirstOrDefaultAsync(t => t.Id == user.TenantId);

                    if (tenant != null)
                    {
                        context.Items["CurrentTenant"] = tenant;
                        context.Items["TenantSettings"] = tenant.Settings;

                        // Check if tenant is active
                        if (!tenant.IsActive)
                        {
                            _logger.LogWarning("User {UserId} attempted to access inactive tenant {TenantId}",
                                user.Id, user.TenantId);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsJsonAsync(new { error = "Tenant is inactive" });
                            return;
                        }

                        // Check subscription validity
                        if (tenant.SubscriptionEndDate.HasValue &&
                            tenant.SubscriptionEndDate.Value < DateTime.UtcNow)
                        {
                            _logger.LogWarning("User {UserId} attempted to access expired tenant {TenantId}",
                                user.Id, user.TenantId);
                            context.Items["TenantExpired"] = true;
                        }
                    }
                }

                // Load company if user belongs to one
                if (user.CompanyId.HasValue)
                {
                    var company = await dbContext.Companies
                        .FirstOrDefaultAsync(c => c.Id == user.CompanyId);

                    if (company != null)
                    {
                        context.Items["CurrentCompany"] = company;

                        if (!company.IsActive)
                        {
                            _logger.LogWarning("User {UserId} attempted to access inactive company {CompanyId}",
                                user.Id, user.CompanyId);
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsJsonAsync(new { error = "Company is inactive" });
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant context");
            // Continue without tenant context on error
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for TenantResolutionMiddleware
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}

/// <summary>
/// Extension methods for HttpContext to access tenant information
/// </summary>
public static class HttpContextTenantExtensions
{
    public static int? GetCurrentTenantId(this HttpContext context)
    {
        return context.Items["CurrentTenantId"] as int?;
    }

    public static int? GetCurrentCompanyId(this HttpContext context)
    {
        return context.Items["CurrentCompanyId"] as int?;
    }

    public static int? GetCurrentBranchId(this HttpContext context)
    {
        return context.Items["CurrentBranchId"] as int?;
    }

    public static bool IsSuperAdmin(this HttpContext context)
    {
        return context.Items["IsSuperAdmin"] as bool? ?? false;
    }

    public static Tenant? GetCurrentTenant(this HttpContext context)
    {
        return context.Items["CurrentTenant"] as Tenant;
    }

    public static Company? GetCurrentCompany(this HttpContext context)
    {
        return context.Items["CurrentCompany"] as Company;
    }

    public static TenantSettings? GetTenantSettings(this HttpContext context)
    {
        return context.Items["TenantSettings"] as TenantSettings;
    }

    public static bool IsTenantExpired(this HttpContext context)
    {
        return context.Items["TenantExpired"] as bool? ?? false;
    }
}
