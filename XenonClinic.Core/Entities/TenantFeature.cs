namespace XenonClinic.Core.Entities;

/// <summary>
/// Features enabled per tenant (can override templates)
/// </summary>
public class TenantFeature
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string FeatureCode { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string? SettingsJson { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public Feature Feature { get; set; } = null!;
}
