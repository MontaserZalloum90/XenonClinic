namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Provides access to the current user's context information.
/// Centralizes user context retrieval to eliminate code duplication across services.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Gets the current authenticated user.
    /// </summary>
    Task<IApplicationUser?> GetCurrentUserAsync();

    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
    Task<string?> GetCurrentUserIdAsync();

    /// <summary>
    /// Gets the current user's tenant ID.
    /// </summary>
    Task<int?> GetCurrentTenantIdAsync();

    /// <summary>
    /// Gets the current user's company ID.
    /// </summary>
    Task<int?> GetCurrentCompanyIdAsync();

    /// <summary>
    /// Gets the current user's primary branch ID.
    /// </summary>
    Task<int?> GetCurrentBranchIdAsync();

    /// <summary>
    /// Gets all branch IDs the current user has access to.
    /// </summary>
    Task<List<int>> GetAccessibleBranchIdsAsync();

    /// <summary>
    /// Checks if the current user is a super admin.
    /// </summary>
    Task<bool> IsSuperAdminAsync();

    /// <summary>
    /// Checks if the current user is in the specified role.
    /// </summary>
    Task<bool> IsInRoleAsync(string role);

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the current user's ID synchronously (cached value).
    /// Returns null if user is not authenticated.
    /// BUG FIX: Added to support synchronous access patterns in controllers.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Requires the user to be authenticated and returns their ID.
    /// BUG FIX: Throws UnauthorizedAccessException if user is not authenticated,
    /// preventing "system" fallback in audit trails.
    /// </summary>
    string RequireUserId();

    /// <summary>
    /// Gets the current user's primary branch ID synchronously.
    /// BUG FIX: Added for controllers that need synchronous access.
    /// </summary>
    int? BranchId { get; }

    /// <summary>
    /// Gets the current user's tenant ID synchronously.
    /// BUG FIX: Added for controllers that need synchronous access.
    /// </summary>
    int? TenantId { get; }

    /// <summary>
    /// Gets the current user's company ID synchronously.
    /// BUG FIX: Added for controllers that need synchronous access.
    /// </summary>
    int? CompanyId { get; }

    /// <summary>
    /// Checks if the current user is in the specified role synchronously.
    /// BUG FIX: Added for controllers that need synchronous role checks.
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Checks if the current user has the specified role (alias for IsInRole).
    /// </summary>
    bool HasRole(string role) => IsInRole(role);
}

/// <summary>
/// BUG FIX: Alias interface for backwards compatibility with controllers
/// that reference ICurrentUserService. This consolidates user context access.
/// </summary>
public interface ICurrentUserService : ICurrentUserContext
{
}
