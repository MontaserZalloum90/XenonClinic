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

    // Navigation
    public ICollection<Company> Companies { get; set; } = new List<Company>();
    public ClinicTypeTemplate? Template { get; set; }
}
