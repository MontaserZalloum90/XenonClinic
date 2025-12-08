using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

/// <summary>
/// ViewModel for tenant theme color customization
/// </summary>
public class TenantThemeViewModel
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Primary color is required")]
    [Display(Name = "Primary Color")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color format. Use hex format (e.g., #1F6FEB)")]
    public string PrimaryColor { get; set; } = "#1F6FEB";

    [Required(ErrorMessage = "Secondary color is required")]
    [Display(Name = "Secondary Color")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color format. Use hex format (e.g., #6B7280)")]
    public string SecondaryColor { get; set; } = "#6B7280";
}

/// <summary>
/// ViewModel for company theme color customization with inheritance
/// </summary>
public class CompanyThemeViewModel
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;

    [Display(Name = "Use Tenant Theme")]
    public bool UseTenantTheme { get; set; } = true;

    [Display(Name = "Primary Color")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color format. Use hex format (e.g., #1F6FEB)")]
    public string? PrimaryColor { get; set; }

    [Display(Name = "Secondary Color")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color format. Use hex format (e.g., #6B7280)")]
    public string? SecondaryColor { get; set; }

    // Tenant default colors for preview
    public string TenantPrimaryColor { get; set; } = "#1F6FEB";
    public string TenantSecondaryColor { get; set; } = "#6B7280";

    // Current effective colors (for preview)
    public string EffectivePrimaryColor { get; set; } = "#1F6FEB";
    public string EffectiveSecondaryColor { get; set; } = "#6B7280";
    public string ColorSource { get; set; } = "Default";
}

/// <summary>
/// ViewModel for branch theme color customization with inheritance
/// </summary>
public class BranchThemeViewModel
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;

    [Display(Name = "Use Company/Tenant Theme")]
    public bool UseInheritedTheme { get; set; } = true;

    [Display(Name = "Primary Color")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color format. Use hex format (e.g., #1F6FEB)")]
    public string? PrimaryColor { get; set; }

    [Display(Name = "Secondary Color")]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color format. Use hex format (e.g., #6B7280)")]
    public string? SecondaryColor { get; set; }

    // Inherited default colors for preview
    public string InheritedPrimaryColor { get; set; } = "#1F6FEB";
    public string InheritedSecondaryColor { get; set; } = "#6B7280";

    // Current effective colors (for preview)
    public string EffectivePrimaryColor { get; set; } = "#1F6FEB";
    public string EffectiveSecondaryColor { get; set; } = "#6B7280";
    public string ColorSource { get; set; } = "Default";
}
