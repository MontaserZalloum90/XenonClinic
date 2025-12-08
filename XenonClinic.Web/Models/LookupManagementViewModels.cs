using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

/// <summary>
/// Base ViewModel for listing lookups of any type.
/// </summary>
public class LookupListViewModel
{
    public string LookupType { get; set; } = string.Empty;
    public string LookupTypeName { get; set; } = string.Empty;
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public List<LookupItemViewModel> Items { get; set; } = new();
    public bool CanCreate { get; set; } = true;
}

/// <summary>
/// ViewModel for individual lookup items in lists.
/// </summary>
public class LookupItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemDefault { get; set; }
    public string? ColorCode { get; set; }
    public string? IconClass { get; set; }
    public string? TenantName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool CanEdit { get; set; } = true;
    public bool CanDelete { get; set; } = true;
}

/// <summary>
/// ViewModel for creating/editing lookups.
/// </summary>
public class LookupEditViewModel
{
    public int Id { get; set; }
    public string LookupType { get; set; } = string.Empty;
    public string LookupTypeName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Name")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Code")]
    [StringLength(50)]
    [RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "Code must contain only uppercase letters, numbers, hyphens, and underscores")]
    public string? Code { get; set; }

    [Display(Name = "Display Order")]
    [Range(0, 9999)]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "System Default")]
    public bool IsSystemDefault { get; set; } = false;

    [Display(Name = "Color Code")]
    [StringLength(7)]
    [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color format. Use hex format (e.g., #1F6FEB)")]
    public string? ColorCode { get; set; }

    [Display(Name = "Icon Class")]
    [StringLength(100)]
    public string? IconClass { get; set; }

    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
    public bool IsEditMode => Id > 0;
}

/// <summary>
/// ViewModel for the main lookup management dashboard.
/// </summary>
public class LookupManagementDashboardViewModel
{
    public List<LookupTypeInfo> LookupTypes { get; set; } = new();
    public int? TenantId { get; set; }
    public string? TenantName { get; set; }
}

/// <summary>
/// Information about a lookup type for the dashboard.
/// </summary>
public class LookupTypeInfo
{
    public string TypeName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public string IconClass { get; set; } = "bi-list";
}
