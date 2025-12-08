namespace XenonClinic.Core.Entities;

/// <summary>
/// Feature flag entity for gradual rollouts and A/B testing
/// </summary>
public class FeatureFlag
{
    public int Id { get; set; }

    /// <summary>
    /// Unique key for the feature flag (e.g., "new-dashboard", "beta-reports")
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the feature
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this feature flag controls
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the feature is globally enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Percentage of users to enable for (0-100) for gradual rollouts
    /// </summary>
    public int RolloutPercentage { get; set; } = 100;

    /// <summary>
    /// Optional: Only enable for specific tenant IDs (comma-separated)
    /// </summary>
    public string? EnabledTenantIds { get; set; }

    /// <summary>
    /// Optional: Only enable for specific company IDs (comma-separated)
    /// </summary>
    public string? EnabledCompanyIds { get; set; }

    /// <summary>
    /// Optional: Only enable for specific user IDs (comma-separated)
    /// </summary>
    public string? EnabledUserIds { get; set; }

    /// <summary>
    /// Optional: Only enable for specific roles (comma-separated)
    /// </summary>
    public string? EnabledRoles { get; set; }

    /// <summary>
    /// Start date for the feature (null = immediate)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for the feature (null = indefinite)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Environment filter (Development, Staging, Production, or null for all)
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// When the flag was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the flag was last modified
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who created this flag
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Who last modified this flag
    /// </summary>
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Context for evaluating feature flags
/// </summary>
public class FeatureFlagContext
{
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public int? TenantId { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string? Environment { get; set; }
    public Dictionary<string, object>? CustomAttributes { get; set; }
}
