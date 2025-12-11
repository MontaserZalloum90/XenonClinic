using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for role-based access control in workflows.
/// </summary>
public class WorkflowAuthorizationService : IWorkflowAuthorizationService
{
    private readonly ITenantService _tenantService;
    private readonly IExpressionEvaluator _expressionEvaluator;
    private readonly ILogger<WorkflowAuthorizationService> _logger;

    // In-memory storage - replace with database in production
    private readonly ConcurrentDictionary<string, WorkflowRole> _roles = new();
    private readonly ConcurrentDictionary<string, List<UserRoleAssignment>> _userRoles = new();
    private readonly ConcurrentDictionary<string, List<UserPermissionGrant>> _userPermissions = new();

    public WorkflowAuthorizationService(
        ITenantService tenantService,
        IExpressionEvaluator expressionEvaluator,
        ILogger<WorkflowAuthorizationService> logger)
    {
        _tenantService = tenantService;
        _expressionEvaluator = expressionEvaluator;
        _logger = logger;

        // Initialize built-in roles
        InitializeBuiltInRoles();
    }

    public async Task<bool> AuthorizeAsync(AuthorizationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Authorizing user {UserId} for {Permission} on {ResourceType}:{ResourceId}",
            request.UserId, request.Permission, request.ResourceType, request.ResourceId);

        // Get user's effective permissions
        var effectivePermissions = await GetEffectivePermissionsAsync(
            request.UserId, request.ResourceType, request.ResourceId, cancellationToken);

        var isAuthorized = effectivePermissions.Contains(request.Permission);

        if (!isAuthorized)
        {
            _logger.LogWarning("Authorization denied for user {UserId}: {Permission} on {ResourceType}:{ResourceId}",
                request.UserId, request.Permission, request.ResourceType, request.ResourceId);
        }

        return isAuthorized;
    }

    public async Task<bool> CanAccessResourceAsync(string userId, ResourceType resourceType, string resourceId, Permission permission, CancellationToken cancellationToken = default)
    {
        return await AuthorizeAsync(new AuthorizationRequest
        {
            UserId = userId,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Permission = permission
        }, cancellationToken);
    }

    public async Task<IList<Permission>> GetEffectivePermissionsAsync(string userId, ResourceType resourceType, string resourceId, CancellationToken cancellationToken = default)
    {
        var permissions = new HashSet<Permission>();

        // 1. Get permissions from assigned roles
        var roleAssignments = await GetUserRolesAsync(userId, cancellationToken);
        foreach (var assignment in roleAssignments)
        {
            // Check if assignment is expired
            if (assignment.ExpiresAt.HasValue && assignment.ExpiresAt.Value < DateTime.UtcNow)
                continue;

            // Check if scope matches
            if (!string.IsNullOrEmpty(assignment.Scope) && assignment.Scope != resourceId)
                continue;

            if (_roles.TryGetValue(assignment.RoleId, out var role) && role.IsActive)
            {
                // Add global permissions (handle null)
                var rolePermissions = role.Permissions ?? Enumerable.Empty<Permission>();
                foreach (var perm in rolePermissions)
                {
                    permissions.Add(perm);
                }

                // Add resource-specific permissions (handle null)
                var resourcePermissions = role.ResourcePermissions ?? Enumerable.Empty<ResourcePermission>();
                foreach (var resourcePerm in resourcePermissions)
                {
                    if (resourcePerm.ResourceType == resourceType)
                    {
                        // Check if it applies to this specific resource or all resources
                        if (string.IsNullOrEmpty(resourcePerm.ResourceId) || resourcePerm.ResourceId == resourceId)
                        {
                            // Evaluate condition if present
                            if (!string.IsNullOrEmpty(resourcePerm.Condition))
                            {
                                var context = new Dictionary<string, object>
                                {
                                    ["userId"] = userId,
                                    ["resourceType"] = resourceType.ToString(),
                                    ["resourceId"] = resourceId ?? ""
                                };

                                try
                                {
                                    var result = await _expressionEvaluator.EvaluateAsync(resourcePerm.Condition, context);
                                    // Check if result is truthy (true boolean or non-null/non-empty value)
                                    var isTruthy = result switch
                                    {
                                        bool b => b,
                                        string s => !string.IsNullOrEmpty(s) && !s.Equals("false", StringComparison.OrdinalIgnoreCase),
                                        null => false,
                                        _ => true
                                    };
                                    if (!isTruthy)
                                        continue;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to evaluate permission condition: {Condition}", resourcePerm.Condition);
                                    continue; // Fail closed - deny permission on expression error
                                }
                            }

                            foreach (var perm in resourcePerm.Permissions)
                            {
                                permissions.Add(perm);
                            }
                        }
                    }
                }
            }
        }

        // 2. Get direct permission grants
        if (_userPermissions.TryGetValue(userId, out var grants))
        {
            foreach (var grant in grants)
            {
                // Check if grant is expired
                if (grant.ExpiresAt.HasValue && grant.ExpiresAt.Value < DateTime.UtcNow)
                    continue;

                // Check if it applies to this resource
                if (grant.ResourceType == resourceType &&
                    (string.IsNullOrEmpty(resourceId) || grant.ResourceId == resourceId || grant.ResourceId == "*"))
                {
                    permissions.Add(grant.Permission);
                }
            }
        }

        return permissions.ToList();
    }

    public Task<WorkflowRole> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        // Validate role name is unique within tenant
        var existingRole = _roles.Values.FirstOrDefault(r =>
            r.TenantId == request.TenantId &&
            r.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));

        if (existingRole != null)
        {
            throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
        }

        var role = new WorkflowRole
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = request.TenantId,
            Name = request.Name,
            DisplayName = request.DisplayName ?? request.Name,
            Description = request.Description,
            Permissions = request.Permissions,
            ResourcePermissions = request.ResourcePermissions,
            IsSystemRole = request.IsSystemRole,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _roles[role.Id] = role;

        _logger.LogInformation("Created role {RoleId} ({RoleName}) for tenant {TenantId}",
            role.Id, role.Name, role.TenantId);

        return Task.FromResult(role);
    }

    public Task<WorkflowRole> UpdateRoleAsync(string roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (!_roles.TryGetValue(roleId, out var role))
        {
            throw new InvalidOperationException($"Role '{roleId}' not found");
        }

        if (role.IsSystemRole)
        {
            throw new InvalidOperationException("Cannot modify system roles");
        }

        if (request.DisplayName != null) role.DisplayName = request.DisplayName;
        if (request.Description != null) role.Description = request.Description;
        if (request.Permissions != null) role.Permissions = request.Permissions;
        if (request.ResourcePermissions != null) role.ResourcePermissions = request.ResourcePermissions;

        role.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Updated role {RoleId}", roleId);

        return Task.FromResult(role);
    }

    public Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default)
    {
        if (_roles.TryGetValue(roleId, out var role))
        {
            if (role.IsSystemRole)
            {
                throw new InvalidOperationException("Cannot delete system roles");
            }

            _roles.TryRemove(roleId, out _);

            // Remove all assignments for this role (get snapshot of keys to avoid concurrent modification)
            foreach (var userId in _userRoles.Keys.ToList())
            {
                if (_userRoles.TryGetValue(userId, out var userRoles))
                {
                    lock (userRoles)
                    {
                        userRoles.RemoveAll(a => a.RoleId == roleId);
                    }
                }
            }

            _logger.LogInformation("Deleted role {RoleId}", roleId);
        }

        return Task.CompletedTask;
    }

    public Task<WorkflowRole?> GetRoleAsync(string roleId, CancellationToken cancellationToken = default)
    {
        _roles.TryGetValue(roleId, out var role);
        return Task.FromResult(role);
    }

    public Task<IList<WorkflowRole>> ListRolesAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var roles = _roles.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(tenantId))
        {
            roles = roles.Where(r => r.TenantId == tenantId || r.TenantId == "default");
        }

        return Task.FromResult<IList<WorkflowRole>>(roles.OrderBy(r => r.Name).ToList());
    }

    public Task AssignRoleAsync(string userId, string roleId, string? scope = null, CancellationToken cancellationToken = default)
    {
        if (!_roles.ContainsKey(roleId))
        {
            throw new InvalidOperationException($"Role '{roleId}' not found");
        }

        var userRoles = _userRoles.GetOrAdd(userId, _ => new List<UserRoleAssignment>());

        lock (userRoles)
        {
            // Check if already assigned
            if (userRoles.Any(a => a.RoleId == roleId && a.Scope == scope))
            {
                return Task.CompletedTask; // Already assigned
            }

            userRoles.Add(new UserRoleAssignment
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                RoleId = roleId,
                RoleName = _roles[roleId].Name,
                Scope = scope,
                AssignedAt = DateTime.UtcNow
            });
        }

        _logger.LogInformation("Assigned role {RoleId} to user {UserId} with scope {Scope}",
            roleId, userId, scope ?? "global");

        return Task.CompletedTask;
    }

    public Task RemoveRoleAsync(string userId, string roleId, string? scope = null, CancellationToken cancellationToken = default)
    {
        if (_userRoles.TryGetValue(userId, out var userRoles))
        {
            lock (userRoles)
            {
                userRoles.RemoveAll(a => a.RoleId == roleId && a.Scope == scope);
            }

            _logger.LogInformation("Removed role {RoleId} from user {UserId} with scope {Scope}",
                roleId, userId, scope ?? "global");
        }

        return Task.CompletedTask;
    }

    public Task<IList<UserRoleAssignment>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (_userRoles.TryGetValue(userId, out var userRoles))
        {
            // Lock when reading to ensure thread safety
            lock (userRoles)
            {
                return Task.FromResult<IList<UserRoleAssignment>>(userRoles.ToList());
            }
        }

        return Task.FromResult<IList<UserRoleAssignment>>(new List<UserRoleAssignment>());
    }

    public Task GrantPermissionAsync(GrantPermissionRequest request, CancellationToken cancellationToken = default)
    {
        var userPermissions = _userPermissions.GetOrAdd(request.UserId, _ => new List<UserPermissionGrant>());

        lock (userPermissions)
        {
            // Check if already granted
            var existing = userPermissions.FirstOrDefault(p =>
                p.ResourceType == request.ResourceType &&
                p.ResourceId == request.ResourceId &&
                p.Permission == request.Permission);

            if (existing != null)
            {
                // Update expiry if different
                existing.ExpiresAt = request.ExpiresAt;
                return Task.CompletedTask;
            }

            userPermissions.Add(new UserPermissionGrant
            {
                Id = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                ResourceType = request.ResourceType,
                ResourceId = request.ResourceId,
                Permission = request.Permission,
                GrantedAt = DateTime.UtcNow,
                GrantedBy = request.GrantedBy,
                ExpiresAt = request.ExpiresAt
            });
        }

        _logger.LogInformation("Granted {Permission} on {ResourceType}:{ResourceId} to user {UserId}",
            request.Permission, request.ResourceType, request.ResourceId, request.UserId);

        return Task.CompletedTask;
    }

    public Task RevokePermissionAsync(string userId, ResourceType resourceType, string resourceId, Permission permission, CancellationToken cancellationToken = default)
    {
        if (_userPermissions.TryGetValue(userId, out var userPermissions))
        {
            lock (userPermissions)
            {
                userPermissions.RemoveAll(p =>
                    p.ResourceType == resourceType &&
                    p.ResourceId == resourceId &&
                    p.Permission == permission);
            }

            _logger.LogInformation("Revoked {Permission} on {ResourceType}:{ResourceId} from user {UserId}",
                permission, resourceType, resourceId, userId);
        }

        return Task.CompletedTask;
    }

    #region Private Methods

    private void InitializeBuiltInRoles()
    {
        var tenantId = "default";

        // Register built-in roles
        var systemAdmin = BuiltInRoles.GetSystemAdminRole(tenantId);
        _roles[systemAdmin.Id] = systemAdmin;

        var processDesigner = BuiltInRoles.GetProcessDesignerRole(tenantId);
        _roles[processDesigner.Id] = processDesigner;

        var taskWorker = BuiltInRoles.GetTaskWorkerRole(tenantId);
        _roles[taskWorker.Id] = taskWorker;

        var viewer = BuiltInRoles.GetViewerRole(tenantId);
        _roles[viewer.Id] = viewer;

        var auditor = BuiltInRoles.GetAuditorRole(tenantId);
        _roles[auditor.Id] = auditor;

        _logger.LogInformation("Initialized {Count} built-in roles", 5);
    }

    #endregion
}

/// <summary>
/// Attribute to mark methods that require authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class WorkflowAuthorizeAttribute : Attribute
{
    public Permission? Permission { get; set; }
    public ResourceType? ResourceType { get; set; }
    public string? ResourceIdParameter { get; set; }
    public string[]? Roles { get; set; }
    public string? Policy { get; set; }

    public WorkflowAuthorizeAttribute() { }

    public WorkflowAuthorizeAttribute(Permission permission)
    {
        Permission = permission;
    }

    public WorkflowAuthorizeAttribute(Permission permission, ResourceType resourceType)
    {
        Permission = permission;
        ResourceType = resourceType;
    }
}

/// <summary>
/// Authorization policy evaluator.
/// </summary>
public interface IAuthorizationPolicyProvider
{
    Task<AuthorizationPolicy?> GetPolicyAsync(string policyName, CancellationToken cancellationToken = default);
    Task<bool> EvaluatePolicyAsync(string policyName, AuthorizationRequest request, CancellationToken cancellationToken = default);
}

public class AuthorizationPolicy
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Permission> RequiredPermissions { get; set; } = new();
    public List<string> RequiredRoles { get; set; } = new();
    public string? Condition { get; set; } // Expression to evaluate
    public bool RequireAll { get; set; } = false; // True = AND, False = OR
}

/// <summary>
/// Default policy provider with common policies.
/// </summary>
public class DefaultAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly Dictionary<string, AuthorizationPolicy> _policies = new()
    {
        ["CanViewProcess"] = new AuthorizationPolicy
        {
            Name = "CanViewProcess",
            RequiredPermissions = new List<Permission> { Permission.ViewProcessDefinition, Permission.ViewProcessInstance }
        },
        ["CanManageProcess"] = new AuthorizationPolicy
        {
            Name = "CanManageProcess",
            RequiredPermissions = new List<Permission>
            {
                Permission.CreateProcessDefinition, Permission.EditProcessDefinition,
                Permission.DeleteProcessDefinition, Permission.DeployProcessDefinition
            },
            RequireAll = false
        },
        ["CanWorkOnTasks"] = new AuthorizationPolicy
        {
            Name = "CanWorkOnTasks",
            RequiredPermissions = new List<Permission> { Permission.ViewTask, Permission.ClaimTask, Permission.CompleteTask }
        },
        ["IsAdmin"] = new AuthorizationPolicy
        {
            Name = "IsAdmin",
            RequiredRoles = new List<string> { BuiltInRoles.SystemAdmin, BuiltInRoles.TenantAdmin }
        }
    };

    private readonly IWorkflowAuthorizationService _authorizationService;

    public DefaultAuthorizationPolicyProvider(IWorkflowAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName, CancellationToken cancellationToken = default)
    {
        _policies.TryGetValue(policyName, out var policy);
        return Task.FromResult(policy);
    }

    public async Task<bool> EvaluatePolicyAsync(string policyName, AuthorizationRequest request, CancellationToken cancellationToken = default)
    {
        var policy = await GetPolicyAsync(policyName, cancellationToken);
        if (policy == null)
            return false;

        // Check required roles
        if (policy.RequiredRoles.Count > 0)
        {
            var userRoles = await _authorizationService.GetUserRolesAsync(request.UserId, cancellationToken);
            var hasRequiredRole = userRoles.Any(ur => policy.RequiredRoles.Contains(ur.RoleId) || policy.RequiredRoles.Contains(ur.RoleName));

            if (policy.RequireAll && !hasRequiredRole)
                return false;
            if (!policy.RequireAll && hasRequiredRole)
                return true;
        }

        // Check required permissions
        if (policy.RequiredPermissions.Count > 0)
        {
            var effectivePermissions = await _authorizationService.GetEffectivePermissionsAsync(
                request.UserId, request.ResourceType, request.ResourceId, cancellationToken);

            if (policy.RequireAll)
            {
                return policy.RequiredPermissions.All(p => effectivePermissions.Contains(p));
            }
            else
            {
                return policy.RequiredPermissions.Any(p => effectivePermissions.Contains(p));
            }
        }

        return true;
    }
}
