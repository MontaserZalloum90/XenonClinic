namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents different types of patient cases in audiology practice
/// </summary>
public class CaseType
{
    public int Id { get; set; }
    public int TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Icon class for UI display (e.g., "bi bi-headphones")
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Color code for visual identification (e.g., "#1F6FEB")
    /// </summary>
    public string? ColorCode { get; set; }

    /// <summary>
    /// Expected duration in days for this case type
    /// </summary>
    public int? ExpectedDurationDays { get; set; }

    /// <summary>
    /// Whether this case type requires immediate attention
    /// </summary>
    public bool RequiresUrgentAttention { get; set; } = false;

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Case> Cases { get; set; } = new List<Case>();
}
