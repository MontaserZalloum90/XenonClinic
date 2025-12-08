using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Filters;

/// <summary>
/// Authorization filter to ensure users can only access data from their assigned branches
/// </summary>
public class BranchAuthorizationFilter : IAsyncActionFilter
{
    private readonly IBranchScopedService _branchService;
    private readonly ILogger<BranchAuthorizationFilter> _logger;

    public BranchAuthorizationFilter(
        IBranchScopedService branchService,
        ILogger<BranchAuthorizationFilter> logger)
    {
        _branchService = branchService;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Check if action has a branchId parameter
        if (context.ActionArguments.TryGetValue("branchId", out var branchIdObj) &&
            branchIdObj is int branchId)
        {
            var hasAccess = await _branchService.HasAccessToBranchAsync(branchId);

            if (!hasAccess)
            {
                _logger.LogWarning(
                    "User attempted to access branch {BranchId} without authorization",
                    branchId);

                context.Result = new ForbidResult();
                return;
            }
        }

        await next();
    }
}
