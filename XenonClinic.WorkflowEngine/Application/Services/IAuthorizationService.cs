using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for role-based access control in workflows.
/// </summary>
public interface IWorkflowAuthorizationService
{
    /// <summary>
    /// Checks if a user has permission to perform an operation.
    /// </summary>
    Task<bool> AuthorizeAsync(AuthorizationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user can access a specific resource.
    /// </summary>
    Task<bool> CanAccessResourceAsync(string userId, ResourceType resourceType, string resourceId, Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the effective permissions for a user on a resource.
    /// </summary>
    Task<IList<Permission>> GetEffectivePermissionsAsync(string userId, ResourceType resourceType, string resourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new role.
    /// </summary>
    Task<WorkflowRole> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a role.
    /// </summary>
    Task<WorkflowRole> UpdateRoleAsync(string roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role.
    /// </summary>
    Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by ID.
    /// </summary>
    Task<WorkflowRole?> GetRoleAsync(string roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all roles.
    /// </summary>
    Task<IList<WorkflowRole>> ListRolesAsync(string? tenantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    Task AssignRoleAsync(string userId, string roleId, string? scope = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    Task RemoveRoleAsync(string userId, string roleId, string? scope = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets roles assigned to a user.
    /// </summary>
    Task<IList<UserRoleAssignment>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants a specific permission to a user on a resource.
    /// </summary>
    Task GrantPermissionAsync(GrantPermissionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific permission from a user on a resource.
    /// </summary>
    Task RevokePermissionAsync(string userId, ResourceType resourceType, string resourceId, Permission permission, CancellationToken cancellationToken = default);
}

#region Enums

public enum Permission
{
    // Process Definition permissions
    ViewProcessDefinition,
    CreateProcessDefinition,
    EditProcessDefinition,
    DeleteProcessDefinition,
    DeployProcessDefinition,
    ExportProcessDefinition,
    ImportProcessDefinition,

    // Process Instance permissions
    ViewProcessInstance,
    StartProcessInstance,
    CancelProcessInstance,
    SuspendProcessInstance,
    ResumeProcessInstance,
    DeleteProcessInstance,
    MigrateProcessInstance,

    // Task permissions
    ViewTask,
    ClaimTask,
    CompleteTask,
    DelegateTask,
    ReassignTask,
    AddTaskComment,

    // Variable permissions
    ViewVariables,
    EditVariables,

    // Audit permissions
    ViewAuditLog,
    ExportAuditLog,

    // Admin permissions
    ManageUsers,
    ManageRoles,
    ManageTenants,
    ViewMonitoring,
    ConfigureSystem
}

public enum ResourceType
{
    ProcessDefinition,
    ProcessInstance,
    Task,
    Tenant,
    User,
    Role,
    Webhook,
    Connector,
    DocumentTemplate,
    System
}

#endregion

#region Request DTOs

public class AuthorizationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public Permission Permission { get; set; }
    public ResourceType ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

public class CreateRoleRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public List<Permission> Permissions { get; set; } = new();
    public List<ResourcePermission> ResourcePermissions { get; set; } = new();
    public bool IsSystemRole { get; set; } = false;
}

public class UpdateRoleRequest
{
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public List<Permission>? Permissions { get; set; }
    public List<ResourcePermission>? ResourcePermissions { get; set; }
}

public class GrantPermissionRequest
{
    public string UserId { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public Permission Permission { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? GrantedBy { get; set; }
}

#endregion

#region Entity DTOs

public class WorkflowRole
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public List<Permission> Permissions { get; set; } = new();
    public List<ResourcePermission> ResourcePermissions { get; set; } = new();
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class ResourcePermission
{
    public ResourceType ResourceType { get; set; }
    public string? ResourceId { get; set; } // null means all resources of this type
    public List<Permission> Permissions { get; set; } = new();
    public string? Condition { get; set; } // Expression to evaluate
}

public class UserRoleAssignment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Scope { get; set; } // null means global, can be process definition ID, etc.
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UserPermissionGrant
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public ResourceType ResourceType { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public Permission Permission { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string? GrantedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

#endregion

#region Built-in Roles

public static class BuiltInRoles
{
    public const string SystemAdmin = "system-admin";
    public const string TenantAdmin = "tenant-admin";
    public const string ProcessDesigner = "process-designer";
    public const string ProcessAdmin = "process-admin";
    public const string TaskWorker = "task-worker";
    public const string Viewer = "viewer";
    public const string Auditor = "auditor";

    public static WorkflowRole GetSystemAdminRole(string tenantId) => new()
    {
        Id = SystemAdmin,
        TenantId = tenantId,
        Name = SystemAdmin,
        DisplayName = "System Administrator",
        Description = "Full access to all workflow engine features",
        IsSystemRole = true,
        Permissions = new List<Permission>
        {
            Permission.ViewProcessDefinition, Permission.CreateProcessDefinition, Permission.EditProcessDefinition,
            Permission.DeleteProcessDefinition, Permission.DeployProcessDefinition, Permission.ExportProcessDefinition,
            Permission.ImportProcessDefinition, Permission.ViewProcessInstance, Permission.StartProcessInstance,
            Permission.CancelProcessInstance, Permission.SuspendProcessInstance, Permission.ResumeProcessInstance,
            Permission.DeleteProcessInstance, Permission.MigrateProcessInstance, Permission.ViewTask, Permission.ClaimTask,
            Permission.CompleteTask, Permission.DelegateTask, Permission.ReassignTask, Permission.AddTaskComment,
            Permission.ViewVariables, Permission.EditVariables, Permission.ViewAuditLog, Permission.ExportAuditLog,
            Permission.ManageUsers, Permission.ManageRoles, Permission.ManageTenants, Permission.ViewMonitoring,
            Permission.ConfigureSystem
        }
    };

    public static WorkflowRole GetProcessDesignerRole(string tenantId) => new()
    {
        Id = ProcessDesigner,
        TenantId = tenantId,
        Name = ProcessDesigner,
        DisplayName = "Process Designer",
        Description = "Can create and manage process definitions",
        IsSystemRole = true,
        Permissions = new List<Permission>
        {
            Permission.ViewProcessDefinition, Permission.CreateProcessDefinition, Permission.EditProcessDefinition,
            Permission.DeployProcessDefinition, Permission.ExportProcessDefinition, Permission.ImportProcessDefinition,
            Permission.ViewProcessInstance, Permission.StartProcessInstance, Permission.ViewTask, Permission.ViewVariables
        }
    };

    public static WorkflowRole GetTaskWorkerRole(string tenantId) => new()
    {
        Id = TaskWorker,
        TenantId = tenantId,
        Name = TaskWorker,
        DisplayName = "Task Worker",
        Description = "Can view and complete assigned tasks",
        IsSystemRole = true,
        Permissions = new List<Permission>
        {
            Permission.ViewProcessInstance, Permission.ViewTask, Permission.ClaimTask, Permission.CompleteTask,
            Permission.AddTaskComment, Permission.ViewVariables
        }
    };

    public static WorkflowRole GetViewerRole(string tenantId) => new()
    {
        Id = Viewer,
        TenantId = tenantId,
        Name = Viewer,
        DisplayName = "Viewer",
        Description = "Read-only access to workflows",
        IsSystemRole = true,
        Permissions = new List<Permission>
        {
            Permission.ViewProcessDefinition, Permission.ViewProcessInstance, Permission.ViewTask, Permission.ViewVariables
        }
    };

    public static WorkflowRole GetAuditorRole(string tenantId) => new()
    {
        Id = Auditor,
        TenantId = tenantId,
        Name = Auditor,
        DisplayName = "Auditor",
        Description = "Can view and export audit logs",
        IsSystemRole = true,
        Permissions = new List<Permission>
        {
            Permission.ViewProcessDefinition, Permission.ViewProcessInstance, Permission.ViewTask,
            Permission.ViewVariables, Permission.ViewAuditLog, Permission.ExportAuditLog
        }
    };
}

#endregion
