using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Services;
using System.Security.Claims;

namespace XenonClinic.Web.Controllers.Api;

/// <summary>
/// API controller for managing feature flags
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger<FeatureFlagsController> _logger;

    public FeatureFlagsController(
        IFeatureFlagService featureFlagService,
        ILogger<FeatureFlagsController> logger)
    {
        _featureFlagService = featureFlagService;
        _logger = logger;
    }

    /// <summary>
    /// Get all feature flags (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var flags = await _featureFlagService.GetAllFeatureFlagsAsync(cancellationToken);
            return Ok(flags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature flags");
            return StatusCode(500, new { Message = "Error retrieving feature flags" });
        }
    }

    /// <summary>
    /// Get a specific feature flag
    /// </summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key, CancellationToken cancellationToken)
    {
        try
        {
            var flag = await _featureFlagService.GetFeatureFlagAsync(key, cancellationToken);
            if (flag == null)
                return NotFound(new { Message = $"Feature flag '{key}' not found" });

            return Ok(flag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature flag: {Key}", key);
            return StatusCode(500, new { Message = "Error retrieving feature flag" });
        }
    }

    /// <summary>
    /// Check if a feature is enabled for the current user
    /// </summary>
    [HttpGet("{key}/enabled")]
    public async Task<IActionResult> IsEnabled(string key, CancellationToken cancellationToken)
    {
        try
        {
            var context = BuildFeatureFlagContext();
            var isEnabled = await _featureFlagService.IsEnabledAsync(key, context, cancellationToken);

            return Ok(new { Key = key, IsEnabled = isEnabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature flag: {Key}", key);
            return StatusCode(500, new { Message = "Error checking feature flag" });
        }
    }

    /// <summary>
    /// Get all feature flag states for the current user
    /// </summary>
    [HttpGet("evaluate")]
    public async Task<IActionResult> EvaluateAll(CancellationToken cancellationToken)
    {
        try
        {
            var context = BuildFeatureFlagContext();
            var flags = await _featureFlagService.EvaluateAllFlagsAsync(context, cancellationToken);

            return Ok(new { Flags = flags, EvaluatedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating feature flags");
            return StatusCode(500, new { Message = "Error evaluating feature flags" });
        }
    }

    /// <summary>
    /// Create a new feature flag (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var flag = new FeatureFlag
            {
                Key = request.Key,
                Name = request.Name,
                Description = request.Description,
                IsEnabled = request.IsEnabled,
                RolloutPercentage = request.RolloutPercentage,
                EnabledTenantIds = request.EnabledTenantIds,
                EnabledCompanyIds = request.EnabledCompanyIds,
                EnabledUserIds = request.EnabledUserIds,
                EnabledRoles = request.EnabledRoles,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Environment = request.Environment,
                CreatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            };

            var created = await _featureFlagService.CreateFeatureFlagAsync(flag, cancellationToken);
            return CreatedAtAction(nameof(Get), new { key = created.Key }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature flag");
            return StatusCode(500, new { Message = "Error creating feature flag" });
        }
    }

    /// <summary>
    /// Update an existing feature flag (admin only)
    /// </summary>
    [HttpPut("{key}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(string key, [FromBody] UpdateFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var existingFlag = await _featureFlagService.GetFeatureFlagAsync(key, cancellationToken);
            if (existingFlag == null)
                return NotFound(new { Message = $"Feature flag '{key}' not found" });

            existingFlag.Name = request.Name ?? existingFlag.Name;
            existingFlag.Description = request.Description ?? existingFlag.Description;
            existingFlag.IsEnabled = request.IsEnabled ?? existingFlag.IsEnabled;
            existingFlag.RolloutPercentage = request.RolloutPercentage ?? existingFlag.RolloutPercentage;
            existingFlag.EnabledTenantIds = request.EnabledTenantIds ?? existingFlag.EnabledTenantIds;
            existingFlag.EnabledCompanyIds = request.EnabledCompanyIds ?? existingFlag.EnabledCompanyIds;
            existingFlag.EnabledUserIds = request.EnabledUserIds ?? existingFlag.EnabledUserIds;
            existingFlag.EnabledRoles = request.EnabledRoles ?? existingFlag.EnabledRoles;
            existingFlag.StartDate = request.StartDate ?? existingFlag.StartDate;
            existingFlag.EndDate = request.EndDate ?? existingFlag.EndDate;
            existingFlag.Environment = request.Environment ?? existingFlag.Environment;
            existingFlag.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var updated = await _featureFlagService.UpdateFeatureFlagAsync(existingFlag, cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature flag: {Key}", key);
            return StatusCode(500, new { Message = "Error updating feature flag" });
        }
    }

    /// <summary>
    /// Toggle a feature flag on/off (admin only)
    /// </summary>
    [HttpPost("{key}/toggle")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Toggle(string key, CancellationToken cancellationToken)
    {
        try
        {
            var flag = await _featureFlagService.GetFeatureFlagAsync(key, cancellationToken);
            if (flag == null)
                return NotFound(new { Message = $"Feature flag '{key}' not found" });

            flag.IsEnabled = !flag.IsEnabled;
            flag.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var updated = await _featureFlagService.UpdateFeatureFlagAsync(flag, cancellationToken);
            return Ok(new { Key = key, IsEnabled = updated.IsEnabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling feature flag: {Key}", key);
            return StatusCode(500, new { Message = "Error toggling feature flag" });
        }
    }

    /// <summary>
    /// Delete a feature flag (admin only)
    /// </summary>
    [HttpDelete("{key}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(string key, CancellationToken cancellationToken)
    {
        try
        {
            await _featureFlagService.DeleteFeatureFlagAsync(key, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feature flag: {Key}", key);
            return StatusCode(500, new { Message = "Error deleting feature flag" });
        }
    }

    private FeatureFlagContext BuildFeatureFlagContext()
    {
        return new FeatureFlagContext
        {
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            UserEmail = User.FindFirst(ClaimTypes.Email)?.Value,
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value),
            TenantId = HttpContext.Items.TryGetValue("TenantId", out var tid) && tid is int tenantId ? tenantId : null,
            CompanyId = HttpContext.Items.TryGetValue("CompanyId", out var cid) && cid is int companyId ? companyId : null,
            BranchId = HttpContext.Items.TryGetValue("BranchId", out var bid) && bid is int branchId ? branchId : null,
            Environment = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName
        };
    }
}

public class CreateFeatureFlagRequest
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public int RolloutPercentage { get; set; } = 100;
    public string? EnabledTenantIds { get; set; }
    public string? EnabledCompanyIds { get; set; }
    public string? EnabledUserIds { get; set; }
    public string? EnabledRoles { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Environment { get; set; }
}

public class UpdateFeatureFlagRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsEnabled { get; set; }
    public int? RolloutPercentage { get; set; }
    public string? EnabledTenantIds { get; set; }
    public string? EnabledCompanyIds { get; set; }
    public string? EnabledUserIds { get; set; }
    public string? EnabledRoles { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Environment { get; set; }
}
