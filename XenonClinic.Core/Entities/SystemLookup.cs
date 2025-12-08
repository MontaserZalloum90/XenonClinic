namespace XenonClinic.Core.Entities;

/// <summary>
/// Base class for all system lookup tables.
/// Provides common properties for dynamic, tenant-scoped lookup values.
/// </summary>
public abstract class SystemLookup
{
    public int Id { get; set; }

    /// <summary>
    /// Tenant ID for multi-tenancy. Null means system-wide default.
    /// </summary>
    public int? TenantId { get; set; }

    /// <summary>
    /// Display name for the lookup value (supports localization key).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description for admin reference.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for sorting in dropdowns and lists.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this lookup value is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a system default that cannot be deleted.
    /// </summary>
    public bool IsSystemDefault { get; set; } = false;

    /// <summary>
    /// Optional color code for visual representation (hex format).
    /// </summary>
    public string? ColorCode { get; set; }

    /// <summary>
    /// Optional icon class (e.g., Bootstrap Icons class name).
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Optional code/key for programmatic reference.
    /// </summary>
    public string? Code { get; set; }

    // Navigation property
    public Tenant? Tenant { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
