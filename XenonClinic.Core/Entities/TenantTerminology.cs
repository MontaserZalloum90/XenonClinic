namespace XenonClinic.Core.Entities;

/// <summary>
/// Tenant-level terminology overrides
/// </summary>
public class TenantTerminology
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
}
