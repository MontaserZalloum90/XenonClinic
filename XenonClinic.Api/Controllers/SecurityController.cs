using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Api.Middleware;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// API controller for RBAC and security management.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SecurityController : BaseApiController
{
    private readonly IRbacService _rbacService;
    private readonly ISecurityConfigurationService _securityConfigService;
    private readonly ITenantContextAccessor _tenantContext;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(
        IRbacService rbacService,
        ISecurityConfigurationService securityConfigService,
        ITenantContextAccessor tenantContext,
        ICurrentUserContext userContext,
        ILogger<SecurityController> logger)
    {
        _rbacService = rbacService;
        _securityConfigService = securityConfigService;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _logger = logger;
    }

    #region Permissions

    /// <summary>
    /// Get all permissions.
    /// </summary>
    [HttpGet("permissions")]
    [Authorize(Policy = "RoleManage")]
    [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissions([FromQuery] string? category = null)
    {
        var permissions = await _rbacService.GetAllPermissionsAsync(category);
        return ApiOk(permissions);
    }

    /// <summary>
    /// Get permission matrix for UI display.
    /// </summary>
    [HttpGet("permissions/matrix")]
    [Authorize(Policy = "RoleManage")]
    [ProducesResponseType(typeof(ApiResponse<PermissionMatrixDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionMatrix()
    {
        var matrix = await _rbacService.GetPermissionMatrixAsync();
        return ApiOk(matrix);
    }

    #endregion

    #region Roles

    /// <summary>
    /// Get all roles.
    /// </summary>
    [HttpGet("roles")]
    [Authorize(Policy = "RoleManage")]
    [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _rbacService.GetAllRolesAsync();
        return ApiOk(roles);
    }

    /// <summary>
    /// Get role by ID.
    /// </summary>
    [HttpGet("roles/{id:int}")]
    [Authorize(Policy = "RoleManage")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRole(int id)
    {
        var role = await _rbacService.GetRoleByIdAsync(id);
        if (role == null)
        {
            return ApiNotFound("Role not found");
        }
        return ApiOk(role);
    }

    /// <summary>
    /// Create a new role.
    /// </summary>
    [HttpPost("roles")]
    [Authorize(Policy = "RoleManage")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRole([FromBody] SaveRoleDto dto)
    {
        var role = await _rbacService.CreateRoleAsync(dto);
        _logger.LogInformation("Role created: {RoleId} by User: {UserId}", role.Id, _userContext.UserId);
        return ApiCreated(role, $"/api/security/roles/{role.Id}");
    }

    /// <summary>
    /// Update a role.
    /// </summary>
    [HttpPut("roles/{id:int}")]
    [Authorize(Policy = "RoleManage")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] SaveRoleDto dto)
    {
        var role = await _rbacService.UpdateRoleAsync(id, dto);
        if (role == null)
        {
            return ApiNotFound("Role not found");
        }
        _logger.LogInformation("Role updated: {RoleId} by User: {UserId}", id, _userContext.UserId);
        return ApiOk(role);
    }

    /// <summary>
    /// Delete a role.
    /// </summary>
    [HttpDelete("roles/{id:int}")]
    [Authorize(Policy = "RoleManage")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var success = await _rbacService.DeleteRoleAsync(id);
        if (!success)
        {
            return ApiBadRequest("Cannot delete role. It may be a system role or have users assigned.");
        }
        _logger.LogInformation("Role deleted: {RoleId} by User: {UserId}", id, _userContext.UserId);
        return ApiOk("Role deleted successfully");
    }

    /// <summary>
    /// Bulk update role permissions.
    /// </summary>
    [HttpPost("roles/permissions/bulk")]
    [Authorize(Policy = "RoleManage")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkUpdatePermissions([FromBody] BulkPermissionUpdateDto dto)
    {
        await _rbacService.BulkUpdateRolePermissionsAsync(dto);
        _logger.LogInformation("Bulk permission update for Role: {RoleId} by User: {UserId}", dto.RoleId, _userContext.UserId);
        return ApiOk("Permissions updated successfully");
    }

    #endregion

    #region User Roles

    /// <summary>
    /// Get roles and permissions for a user.
    /// </summary>
    [HttpGet("users/{userId:int}/roles")]
    [Authorize(Policy = "UserManage")]
    [ProducesResponseType(typeof(ApiResponse<UserRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoles(int userId)
    {
        var userRoles = await _rbacService.GetUserRolesAsync(userId);
        return ApiOk(userRoles);
    }

    /// <summary>
    /// Assign roles to a user.
    /// </summary>
    [HttpPost("users/roles")]
    [Authorize(Policy = "UserManage")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRoles([FromBody] AssignRolesDto dto)
    {
        await _rbacService.AssignRolesToUserAsync(dto);
        _logger.LogInformation("Roles assigned to User: {UserId} by Admin: {AdminId}", dto.UserId, _userContext.UserId);
        return ApiOk("Roles assigned successfully");
    }

    /// <summary>
    /// Get effective permissions for current user.
    /// </summary>
    [HttpGet("my-permissions")]
    [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPermissions()
    {
        var userId = int.TryParse(_userContext.UserId, out var parsedUserId) ? parsedUserId : 0;
        var permissions = await _rbacService.GetUserEffectivePermissionsAsync(userId);
        return ApiOk(permissions);
    }

    #endregion

    #region Access Control

    /// <summary>
    /// Check if user has access to a resource.
    /// </summary>
    [HttpPost("access-check")]
    [ProducesResponseType(typeof(ApiResponse<AccessCheckResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckAccess([FromBody] AccessCheckRequestDto request)
    {
        if (request.UserId == 0)
        {
            request.UserId = int.TryParse(_userContext.UserId, out var parsedUserId) ? parsedUserId : 0;
        }
        request.BranchId ??= _tenantContext.BranchId;

        var result = await _rbacService.CheckAccessAsync(request);
        return ApiOk(result);
    }

    /// <summary>
    /// Get data access rules.
    /// </summary>
    [HttpGet("data-access-rules")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<List<DataAccessRuleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDataAccessRules([FromQuery] string? resourceType = null)
    {
        var rules = await _rbacService.GetDataAccessRulesAsync(resourceType);
        return ApiOk(rules);
    }

    /// <summary>
    /// Create data access rule.
    /// </summary>
    [HttpPost("data-access-rules")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DataAccessRuleDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDataAccessRule([FromBody] CreateDataAccessRuleDto dto)
    {
        var createdBy = int.TryParse(_userContext.UserId, out var parsedUserId) ? parsedUserId : 0;
        var rule = await _rbacService.CreateDataAccessRuleAsync(dto, createdBy);
        return ApiCreated(rule, $"/api/security/data-access-rules/{rule.Id}");
    }

    #endregion

    #region Security Settings

    /// <summary>
    /// Get security settings.
    /// </summary>
    [HttpGet("settings")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<SecuritySettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSecuritySettings()
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var settings = await _securityConfigService.GetSecuritySettingsAsync(branchId);
        return ApiOk(settings);
    }

    /// <summary>
    /// Update security settings.
    /// </summary>
    [HttpPut("settings")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<SecuritySettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSecuritySettings([FromBody] SecuritySettingsDto settings)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        settings.BranchId = branchId;
        var updated = await _securityConfigService.UpdateSecuritySettingsAsync(branchId, settings);
        _logger.LogInformation("Security settings updated for Branch: {BranchId} by User: {UserId}", branchId, _userContext.UserId);
        return ApiOk(updated);
    }

    /// <summary>
    /// Get password policy.
    /// </summary>
    [HttpGet("password-policy")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PasswordPolicyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPasswordPolicy()
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var policy = await _securityConfigService.GetPasswordPolicyAsync(branchId);
        return ApiOk(policy);
    }

    /// <summary>
    /// Update password policy.
    /// </summary>
    [HttpPut("password-policy")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<PasswordPolicyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePasswordPolicy([FromBody] PasswordPolicyDto policy)
    {
        var branchId = _tenantContext.BranchId ?? 0;
        policy.BranchId = branchId;
        var updated = await _securityConfigService.UpdatePasswordPolicyAsync(branchId, policy);
        return ApiOk(updated);
    }

    /// <summary>
    /// Validate a password against policy.
    /// BUG FIX: Removed AllowAnonymous to prevent unauthenticated password enumeration.
    /// Users can only validate their own password, not test passwords for other users.
    /// </summary>
    [HttpPost("validate-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PasswordValidationResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ValidatePassword([FromBody] ValidatePasswordRequest request)
    {
        // BUG FIX: Require authenticated user and only allow validating own password
        var currentUserId = _userContext.UserId;
        if (currentUserId == null)
        {
            return ApiUnauthorized("Authentication required");
        }

        // BUG FIX: Prevent testing passwords for other users (security vulnerability)
        // Compare as strings since currentUserId is string and request.UserId is int
        if (request.UserId.HasValue && request.UserId.Value.ToString() != currentUserId)
        {
            return ApiForbidden("Cannot validate password for other users");
        }

        var branchId = _tenantContext.BranchId ?? request.BranchId ?? 0;
        var result = await _securityConfigService.ValidatePasswordAsync(request.Password, branchId, request.UserId);
        return ApiOk(result);
    }

    #endregion

    #region API Keys

    /// <summary>
    /// Get API keys.
    /// </summary>
    [HttpGet("api-keys")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<List<ApiKeyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKeys()
    {
        var branchId = _tenantContext.BranchId ?? 0;
        var keys = await _securityConfigService.GetApiKeysAsync(branchId);
        return ApiOk(keys);
    }

    /// <summary>
    /// Create API key.
    /// </summary>
    [HttpPost("api-keys")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ApiKeyDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyDto dto)
    {
        dto.BranchId = _tenantContext.BranchId ?? dto.BranchId;
        var key = await _securityConfigService.CreateApiKeyAsync(dto);
        _logger.LogInformation("API key created: {KeyName} for Branch: {BranchId} by User: {UserId}",
            dto.Name, dto.BranchId, _userContext.UserId);
        return ApiCreated(key, $"/api/security/api-keys/{key.Id}");
    }

    /// <summary>
    /// Revoke API key.
    /// </summary>
    [HttpDelete("api-keys/{id:int}")]
    [Authorize(Policy = "SecurityAdmin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeApiKey(int id)
    {
        await _securityConfigService.RevokeApiKeyAsync(id);
        _logger.LogInformation("API key revoked: {KeyId} by User: {UserId}", id, _userContext.UserId);
        return ApiOk("API key revoked successfully");
    }

    #endregion
}

/// <summary>
/// Password validation request
/// </summary>
public class ValidatePasswordRequest
{
    public string Password { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public int? UserId { get; set; }
}
