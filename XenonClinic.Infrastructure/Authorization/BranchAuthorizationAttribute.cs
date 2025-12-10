using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Authorization;

/// <summary>
/// Authorization attribute that validates branch access for the current request.
/// Can be applied to controllers or actions to enforce branch-level access control.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class BranchAuthorizationAttribute : ActionFilterAttribute
{
    private readonly string? _branchIdParameterName;
    private readonly bool _allowCompanyAdmin;

    /// <summary>
    /// Creates a branch authorization attribute.
    /// </summary>
    /// <param name="branchIdParameterName">
    /// The name of the route parameter, query string parameter, or request body property containing the branch ID.
    /// Defaults to "branchId".
    /// </param>
    /// <param name="allowCompanyAdmin">
    /// Whether company admins should be allowed to access all branches in their company.
    /// Defaults to true.
    /// </param>
    public BranchAuthorizationAttribute(string branchIdParameterName = "branchId", bool allowCompanyAdmin = true)
    {
        _branchIdParameterName = branchIdParameterName;
        _allowCompanyAdmin = allowCompanyAdmin;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContextAccessor>();

        // Super admins always have access
        if (tenantContext.IsSuperAdmin)
        {
            await next();
            return;
        }

        // Company admins have access if allowed
        if (_allowCompanyAdmin && tenantContext.IsCompanyAdmin)
        {
            await next();
            return;
        }

        // Try to extract branch ID from various sources
        int? branchId = ExtractBranchId(context);

        if (!branchId.HasValue)
        {
            // If no branch ID found, check if user has any branch access
            if (tenantContext.BranchId.HasValue || tenantContext.AccessibleBranchIds?.Count > 0)
            {
                await next();
                return;
            }

            context.Result = new ForbidResult();
            return;
        }

        // Validate branch access
        if (!tenantContext.HasBranchAccess(branchId.Value))
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }

    private int? ExtractBranchId(ActionExecutingContext context)
    {
        if (string.IsNullOrEmpty(_branchIdParameterName))
            return null;

        // Try route data
        if (context.RouteData.Values.TryGetValue(_branchIdParameterName, out var routeValue) &&
            TryParseInt(routeValue, out var routeBranchId))
        {
            return routeBranchId;
        }

        // Try action arguments
        if (context.ActionArguments.TryGetValue(_branchIdParameterName, out var argValue) &&
            TryParseInt(argValue, out var argBranchId))
        {
            return argBranchId;
        }

        // Try query string
        if (context.HttpContext.Request.Query.TryGetValue(_branchIdParameterName, out var queryValue) &&
            int.TryParse(queryValue.FirstOrDefault(), out var queryBranchId))
        {
            return queryBranchId;
        }

        // Try header
        if (context.HttpContext.Request.Headers.TryGetValue("X-Branch-Id", out var headerValue) &&
            int.TryParse(headerValue.FirstOrDefault(), out var headerBranchId))
        {
            return headerBranchId;
        }

        return null;
    }

    private static bool TryParseInt(object? value, out int result)
    {
        result = 0;
        if (value == null) return false;

        if (value is int intValue)
        {
            result = intValue;
            return true;
        }

        return int.TryParse(value.ToString(), out result);
    }
}

/// <summary>
/// Attribute that requires access to a specific branch ID from the request.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireBranchAccessAttribute : TypeFilterAttribute
{
    public RequireBranchAccessAttribute(string branchIdParameterName = "branchId")
        : base(typeof(BranchAccessFilter))
    {
        Arguments = new object[] { branchIdParameterName };
    }
}

/// <summary>
/// Filter that validates branch access for incoming requests.
/// </summary>
public class BranchAccessFilter : IAsyncActionFilter
{
    private readonly ITenantContextAccessor _tenantContext;
    private readonly string _branchIdParameterName;

    public BranchAccessFilter(ITenantContextAccessor tenantContext, string branchIdParameterName)
    {
        _tenantContext = tenantContext;
        _branchIdParameterName = branchIdParameterName;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Super admins bypass
        if (_tenantContext.IsSuperAdmin)
        {
            await next();
            return;
        }

        // Extract and validate branch ID
        if (context.ActionArguments.TryGetValue(_branchIdParameterName, out var branchIdObj) &&
            branchIdObj is int branchId)
        {
            if (!_tenantContext.HasBranchAccess(branchId))
            {
                context.Result = new ObjectResult(new
                {
                    success = false,
                    error = "Access to this branch is not authorized"
                })
                {
                    StatusCode = 403
                };
                return;
            }
        }

        await next();
    }
}
