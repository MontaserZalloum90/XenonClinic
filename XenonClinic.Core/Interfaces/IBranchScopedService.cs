namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service to get the current user's branch context
/// </summary>
public interface IBranchScopedService
{
    /// <summary>
    /// Gets the current user's primary branch ID
    /// </summary>
    Task<int?> GetCurrentUserBranchIdAsync();

    /// <summary>
    /// Gets all branch IDs accessible by the current user
    /// </summary>
    Task<List<int>> GetUserBranchIdsAsync();

    /// <summary>
    /// Checks if the current user has access to a specific branch
    /// </summary>
    Task<bool> HasAccessToBranchAsync(int branchId);
}
