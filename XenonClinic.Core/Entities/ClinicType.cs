namespace XenonClinic.Core.Entities;

/// <summary>
/// Defines the type of clinic (AUDIOLOGY, DENTAL, VET, etc.)
/// Only used when CompanyType = CLINIC
/// </summary>
public class ClinicType
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<Company> Companies { get; set; } = new List<Company>();
    public ClinicTypeTemplate? Template { get; set; }
}
