using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
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

    // Claim types for branch context from headers or tokens
    private const string BranchIdHeaderName = "X-Branch-Id";
    private const string BranchIdClaimType = "branch_id";
    private const string CompanyAdminClaimType = "is_company_admin";

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
        UserManager<ApplicationUser> userManager,
        ClinicDbContext dbContext)
    {
        try
        {
            // Only resolve tenant for authenticated requests
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user != null)
                {
                    // Get branch context from various sources
                    int? selectedBranchId = GetSelectedBranchId(context, user);
                    bool isCompanyAdmin = GetIsCompanyAdmin(context, user);

                    // Load accessible branches for the user
                    IReadOnlyList<int>? accessibleBranchIds = null;
                    if (!user.IsSuperAdmin && !isCompanyAdmin)
                    {
                        accessibleBranchIds = await LoadAccessibleBranchIds(dbContext, user.Id);
                    }

                    // Validate selected branch is accessible
                    if (selectedBranchId.HasValue && accessibleBranchIds != null)
                    {
                        if (!accessibleBranchIds.Contains(selectedBranchId.Value))
                        {
                            _logger.LogWarning(
                                "User {UserId} attempted to access BranchId {BranchId} which is not in their accessible branches",
                                user.Id, selectedBranchId.Value);
                            selectedBranchId = null; // Reset to prevent unauthorized access
                        }
                    }

                    // Set full context including branch information
                    tenantContextAccessor.SetContext(
                        tenantId: user.TenantId,
                        companyId: user.CompanyId,
                        branchId: selectedBranchId,
                        accessibleBranchIds: accessibleBranchIds,
                        isSuperAdmin: user.IsSuperAdmin,
                        isCompanyAdmin: isCompanyAdmin);

                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug(
                            "Tenant context resolved: TenantId={TenantId}, CompanyId={CompanyId}, BranchId={BranchId}, AccessibleBranches={BranchCount}, IsSuperAdmin={IsSuperAdmin}, IsCompanyAdmin={IsCompanyAdmin}",
                            user.TenantId, user.CompanyId, selectedBranchId,
                            accessibleBranchIds?.Count ?? -1, user.IsSuperAdmin, isCompanyAdmin);
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

    /// <summary>
    /// Gets the selected branch ID from various sources (header, claim, or default).
    /// </summary>
    private int? GetSelectedBranchId(HttpContext context, ApplicationUser user)
    {
        // 1. Check header for branch selection (for multi-branch operations)
        if (context.Request.Headers.TryGetValue(BranchIdHeaderName, out var headerValue) &&
            int.TryParse(headerValue.FirstOrDefault(), out var branchIdFromHeader))
        {
            return branchIdFromHeader;
        }

        // 2. Check claim from token
        var branchIdClaim = context.User?.FindFirst(BranchIdClaimType)?.Value;
        if (!string.IsNullOrEmpty(branchIdClaim) && int.TryParse(branchIdClaim, out var branchIdFromClaim))
        {
            return branchIdFromClaim;
        }

        // 3. Use user's default branch if set
        return user.DefaultBranchId;
    }

    /// <summary>
    /// Determines if the user is a company admin with access to all branches.
    /// </summary>
    private bool GetIsCompanyAdmin(HttpContext context, ApplicationUser user)
    {
        // Check claim
        var isCompanyAdminClaim = context.User?.FindFirst(CompanyAdminClaimType)?.Value;
        if (!string.IsNullOrEmpty(isCompanyAdminClaim) &&
            bool.TryParse(isCompanyAdminClaim, out var isCompanyAdmin))
        {
            return isCompanyAdmin;
        }

        // Fall back to user property
        return user.IsCompanyAdmin;
    }

    /// <summary>
    /// Loads the branch IDs the user has access to from the database.
    /// </summary>
    private async Task<IReadOnlyList<int>> LoadAccessibleBranchIds(ClinicDbContext dbContext, string userId)
    {
        return await dbContext.UserBranches
            .Where(ub => ub.UserId == userId)
            .Select(ub => ub.BranchId)
            .ToListAsync();
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
