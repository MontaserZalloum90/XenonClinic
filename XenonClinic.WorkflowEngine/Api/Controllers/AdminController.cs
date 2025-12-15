using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.WorkflowEngine.Application.Services;

namespace XenonClinic.WorkflowEngine.Api.Controllers;

/// <summary>
/// API controller for workflow engine administration.
/// </summary>
[ApiController]
[Route("api/workflow/admin")]
[Authorize(Policy = "WorkflowAdmin")]
public class AdminController : ControllerBase
{
    private readonly IDistributedExecutionService _distributedService;
    private readonly IWorkflowCacheService _cacheService;
    private readonly IWorkflowAuthorizationService _authService;

    public AdminController(
        IDistributedExecutionService distributedService,
        IWorkflowCacheService cacheService,
        IWorkflowAuthorizationService authService)
    {
        _distributedService = distributedService;
        _cacheService = cacheService;
        _authService = authService;
    }

    #region Cluster Management

    /// <summary>
    /// Gets the current cluster state.
    /// </summary>
    [HttpGet("cluster")]
    public async Task<ActionResult<ClusterState>> GetClusterState(CancellationToken cancellationToken)
    {
        var state = await _distributedService.GetClusterStateAsync(cancellationToken);
        return Ok(state);
    }

    /// <summary>
    /// Gets all nodes in the cluster.
    /// </summary>
    [HttpGet("cluster/nodes")]
    public async Task<ActionResult<IList<ClusterNode>>> GetNodes(CancellationToken cancellationToken)
    {
        var state = await _distributedService.GetClusterStateAsync(cancellationToken);
        return Ok(state.Nodes);
    }

    /// <summary>
    /// Gets a specific node by ID.
    /// </summary>
    [HttpGet("cluster/nodes/{nodeId}")]
    public async Task<ActionResult<ClusterNode>> GetNode(
        string nodeId,
        CancellationToken cancellationToken)
    {
        var node = await _distributedService.GetNodeAsync(nodeId, cancellationToken);
        if (node == null)
        {
            return NotFound(new { message = $"Node '{nodeId}' not found" });
        }
        return Ok(node);
    }

    /// <summary>
    /// Removes a node from the cluster.
    /// </summary>
    [HttpDelete("cluster/nodes/{nodeId}")]
    public async Task<IActionResult> RemoveNode(
        string nodeId,
        CancellationToken cancellationToken)
    {
        await _distributedService.DeregisterNodeAsync(nodeId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets the current cluster leader.
    /// </summary>
    [HttpGet("cluster/leader")]
    public async Task<ActionResult<object>> GetLeader(CancellationToken cancellationToken)
    {
        var state = await _distributedService.GetClusterStateAsync(cancellationToken);
        if (string.IsNullOrEmpty(state.LeaderNodeId))
        {
            return Ok(new { hasLeader = false });
        }

        var leader = await _distributedService.GetNodeAsync(state.LeaderNodeId, cancellationToken);
        return Ok(new { hasLeader = true, leader });
    }

    // Note: Lock management endpoints removed as GetActiveLocksAsync and ForceReleaseLockAsync
    // are not yet implemented in IDistributedExecutionService

    #endregion

    #region Cache Management

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    [HttpGet("cache/statistics")]
    public async Task<ActionResult<CacheStatistics>> GetCacheStatistics(
        CancellationToken cancellationToken)
    {
        var stats = await _cacheService.GetStatisticsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Clears the entire cache.
    /// </summary>
    [HttpDelete("cache")]
    public async Task<IActionResult> ClearCache(CancellationToken cancellationToken)
    {
        await _cacheService.ClearAsync(cancellationToken);
        return Ok(new { message = "Cache cleared" });
    }

    /// <summary>
    /// Clears cache entries by pattern.
    /// </summary>
    [HttpDelete("cache/pattern")]
    public async Task<IActionResult> ClearCacheByPattern(
        [FromQuery] string pattern,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return BadRequest(new { message = "Pattern is required" });
        }
        await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
        return Ok(new { message = $"Cache entries matching '{pattern}' cleared" });
    }

    /// <summary>
    /// Invalidates cache for a specific tenant.
    /// </summary>
    [HttpDelete("cache/tenant/{tenantId}")]
    public async Task<IActionResult> InvalidateTenantCache(
        string tenantId,
        CancellationToken cancellationToken)
    {
        await _cacheService.InvalidateTenantCacheAsync(tenantId, cancellationToken);
        return Ok(new { message = $"Cache for tenant '{tenantId}' invalidated" });
    }

    #endregion

    #region Authorization Management

    /// <summary>
    /// Lists all workflow roles.
    /// </summary>
    [HttpGet("roles")]
    public async Task<ActionResult<IList<WorkflowRole>>> ListRoles(
        [FromQuery] string? tenantId,
        CancellationToken cancellationToken)
    {
        var roles = await _authService.ListRolesAsync(tenantId, cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Creates a new workflow role.
    /// </summary>
    [HttpPost("roles")]
    public async Task<ActionResult<WorkflowRole>> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _authService.CreateRoleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRole), new { roleId = role.Id }, role);
    }

    /// <summary>
    /// Gets a role by ID.
    /// </summary>
    [HttpGet("roles/{roleId}")]
    public async Task<ActionResult<WorkflowRole>> GetRole(
        string roleId,
        CancellationToken cancellationToken)
    {
        var role = await _authService.GetRoleAsync(roleId, cancellationToken);
        if (role == null)
        {
            return NotFound(new { message = $"Role '{roleId}' not found" });
        }
        return Ok(role);
    }

    /// <summary>
    /// Updates a role.
    /// </summary>
    [HttpPut("roles/{roleId}")]
    public async Task<ActionResult<WorkflowRole>> UpdateRole(
        string roleId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _authService.UpdateRoleAsync(roleId, request, cancellationToken);
        return Ok(role);
    }

    /// <summary>
    /// Deletes a role.
    /// </summary>
    [HttpDelete("roles/{roleId}")]
    public async Task<IActionResult> DeleteRole(
        string roleId,
        CancellationToken cancellationToken)
    {
        await _authService.DeleteRoleAsync(roleId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    [HttpPost("users/{userId}/roles")]
    public async Task<IActionResult> AssignRole(
        string userId,
        [FromBody] RoleAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        await _authService.AssignRoleAsync(userId, request.RoleId, request.Scope, cancellationToken);
        return Ok(new { message = "Role assigned" });
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    [HttpDelete("users/{userId}/roles/{roleId}")]
    public async Task<IActionResult> RevokeRole(
        string userId,
        string roleId,
        [FromQuery] string? scope,
        CancellationToken cancellationToken)
    {
        await _authService.RemoveRoleAsync(userId, roleId, scope, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets roles for a user.
    /// </summary>
    [HttpGet("users/{userId}/roles")]
    public async Task<ActionResult<IList<UserRoleAssignment>>> GetUserRoles(
        string userId,
        CancellationToken cancellationToken)
    {
        var roles = await _authService.GetUserRolesAsync(userId, cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Gets effective permissions for a user on a resource.
    /// </summary>
    [HttpGet("users/{userId}/permissions")]
    public async Task<ActionResult<IList<Permission>>> GetUserPermissions(
        string userId,
        [FromQuery] ResourceType resourceType,
        [FromQuery] string resourceId,
        CancellationToken cancellationToken)
    {
        var permissions = await _authService.GetEffectivePermissionsAsync(
            userId, resourceType, resourceId, cancellationToken);
        return Ok(permissions);
    }

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    [HttpPost("authorize")]
    public async Task<ActionResult<AuthorizationResult>> Authorize(
        [FromBody] AuthorizationRequest request,
        CancellationToken cancellationToken)
    {
        var authorized = await _authService.AuthorizeAsync(request, cancellationToken);
        return Ok(new AuthorizationResult
        {
            Authorized = authorized,
            UserId = request.UserId,
            Permission = request.Permission,
            ResourceType = request.ResourceType,
            ResourceId = request.ResourceId
        });
    }

    #endregion

    #region Health & Diagnostics

    /// <summary>
    /// Gets workflow engine health status.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<ActionResult<HealthStatus>> GetHealth(CancellationToken cancellationToken)
    {
        var clusterState = await _distributedService.GetClusterStateAsync(cancellationToken);
        var cacheStats = await _cacheService.GetStatisticsAsync(cancellationToken);

        var health = new HealthStatus
        {
            Status = clusterState.Health == ClusterHealth.Healthy ? "Healthy" : "Degraded",
            ClusterHealthy = clusterState.Health == ClusterHealth.Healthy,
            NodeCount = clusterState.Nodes?.Count ?? 0,
            HasLeader = !string.IsNullOrEmpty(clusterState.LeaderNodeId),
            CacheEntries = cacheStats.TotalEntries,
            CacheHitRatio = cacheStats.TotalHits + cacheStats.TotalMisses > 0
                ? (double)cacheStats.TotalHits / (cacheStats.TotalHits + cacheStats.TotalMisses)
                : 0
        };

        return Ok(health);
    }

    /// <summary>
    /// Gets workflow engine version information.
    /// </summary>
    [HttpGet("version")]
    [AllowAnonymous]
    public ActionResult<VersionInfo> GetVersion()
    {
        var assembly = typeof(AdminController).Assembly;
        var version = assembly.GetName().Version;

        DateTime buildDate;
        try
        {
            var location = assembly.Location;
            buildDate = !string.IsNullOrEmpty(location) && System.IO.File.Exists(location)
                ? System.IO.File.GetLastWriteTime(location)
                : DateTime.UtcNow;
        }
        catch
        {
            buildDate = DateTime.UtcNow;
        }

        return Ok(new VersionInfo
        {
            Version = version?.ToString() ?? "1.0.0",
            AssemblyName = assembly.GetName().Name ?? "XenonClinic.WorkflowEngine",
            BuildDate = buildDate
        });
    }

    #endregion
}

public class RoleAssignmentRequest
{
    public string RoleId { get; set; } = string.Empty;
    public string? Scope { get; set; }
}

public class AuthorizationResult
{
    public bool Authorized { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Permission Permission { get; set; }
    public ResourceType ResourceType { get; set; }
    public string? ResourceId { get; set; }
}

public class HealthStatus
{
    public string Status { get; set; } = "Unknown";
    public bool ClusterHealthy { get; set; }
    public int NodeCount { get; set; }
    public bool HasLeader { get; set; }
    public long CacheEntries { get; set; }
    public double CacheHitRatio { get; set; }
}

public class VersionInfo
{
    public string Version { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public DateTime BuildDate { get; set; }
}
