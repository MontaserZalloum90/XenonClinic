using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Role-Based Access Control service interface
/// </summary>
public interface IRbacService
{
    #region Permission Management

    /// <summary>
    /// Get all permissions
    /// </summary>
    Task<List<PermissionDto>> GetAllPermissionsAsync();

    /// <summary>
    /// Get permissions by category
    /// </summary>
    Task<List<PermissionDto>> GetPermissionsByCategoryAsync(string category);

    /// <summary>
    /// Get permission by code
    /// </summary>
    Task<PermissionDto?> GetPermissionByCodeAsync(string code);

    /// <summary>
    /// Initialize default permissions (system setup)
    /// </summary>
    Task InitializeDefaultPermissionsAsync();

    #endregion

    #region Role Management

    /// <summary>
    /// Get all roles
    /// </summary>
    Task<List<RoleDto>> GetAllRolesAsync(int? branchId = null);

    /// <summary>
    /// Get role by ID
    /// </summary>
    Task<RoleDto?> GetRoleByIdAsync(int roleId);

    /// <summary>
    /// Get role by name
    /// </summary>
    Task<RoleDto?> GetRoleByNameAsync(string roleName);

    /// <summary>
    /// Create a new role
    /// </summary>
    Task<RoleDto> CreateRoleAsync(SaveRoleDto request, int createdByUserId);

    /// <summary>
    /// Update role
    /// </summary>
    Task<RoleDto> UpdateRoleAsync(int roleId, SaveRoleDto request, int updatedByUserId);

    /// <summary>
    /// Delete role
    /// </summary>
    Task<bool> DeleteRoleAsync(int roleId, int deletedByUserId);

    /// <summary>
    /// Duplicate role with new name
    /// </summary>
    Task<RoleDto> DuplicateRoleAsync(int roleId, string newName, int createdByUserId);

    /// <summary>
    /// Initialize default roles (system setup)
    /// </summary>
    Task InitializeDefaultRolesAsync();

    #endregion

    #region User Role Assignment

    /// <summary>
    /// Get user's roles and permissions
    /// </summary>
    Task<UserRoleDto> GetUserRolesAsync(int userId);

    /// <summary>
    /// Assign roles to user
    /// </summary>
    Task<bool> AssignRolesToUserAsync(AssignRolesDto request, int assignedByUserId);

    /// <summary>
    /// Remove role from user
    /// </summary>
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId, int removedByUserId);

    /// <summary>
    /// Get users in role
    /// </summary>
    Task<List<UserRoleDto>> GetUsersInRoleAsync(int roleId);

    /// <summary>
    /// Check if user has role
    /// </summary>
    Task<bool> UserHasRoleAsync(int userId, string roleName);

    #endregion

    #region Access Control

    /// <summary>
    /// Check if user has permission
    /// </summary>
    Task<bool> HasPermissionAsync(int userId, string permissionCode);

    /// <summary>
    /// Check if user has any of the specified permissions
    /// </summary>
    Task<bool> HasAnyPermissionAsync(int userId, params string[] permissionCodes);

    /// <summary>
    /// Check if user has all of the specified permissions
    /// </summary>
    Task<bool> HasAllPermissionsAsync(int userId, params string[] permissionCodes);

    /// <summary>
    /// Check resource access with full context
    /// </summary>
    Task<AccessCheckResultDto> CheckAccessAsync(AccessCheckRequestDto request);

    /// <summary>
    /// Get user's effective permissions (from all roles + direct)
    /// </summary>
    Task<List<string>> GetEffectivePermissionsAsync(int userId);

    /// <summary>
    /// Validate emergency access request
    /// </summary>
    Task<bool> ValidateEmergencyAccessAsync(int userId, int patientId, string justification);

    #endregion

    #region Data Access Rules

    /// <summary>
    /// Get data access rules
    /// </summary>
    Task<List<DataAccessRuleDto>> GetDataAccessRulesAsync(string? resourceType = null);

    /// <summary>
    /// Create data access rule
    /// </summary>
    Task<DataAccessRuleDto> CreateDataAccessRuleAsync(CreateDataAccessRuleDto request, int createdByUserId);

    /// <summary>
    /// Update data access rule
    /// </summary>
    Task<DataAccessRuleDto> UpdateDataAccessRuleAsync(int ruleId, CreateDataAccessRuleDto request, int updatedByUserId);

    /// <summary>
    /// Delete data access rule
    /// </summary>
    Task<bool> DeleteDataAccessRuleAsync(int ruleId, int deletedByUserId);

    /// <summary>
    /// Evaluate data access rules for a query
    /// </summary>
    Task<string?> GetDataFilterAsync(int userId, string resourceType);

    #endregion

    #region Permission Matrix

    /// <summary>
    /// Get full permission matrix
    /// </summary>
    Task<PermissionMatrixDto> GetPermissionMatrixAsync();

    /// <summary>
    /// Bulk update role permissions
    /// </summary>
    Task<bool> BulkUpdatePermissionsAsync(BulkPermissionUpdateDto request, int updatedByUserId);

    #endregion

    #region Caching

    /// <summary>
    /// Clear permission cache for user
    /// </summary>
    Task ClearUserCacheAsync(int userId);

    /// <summary>
    /// Clear all permission caches
    /// </summary>
    Task ClearAllCachesAsync();

    #endregion
}
