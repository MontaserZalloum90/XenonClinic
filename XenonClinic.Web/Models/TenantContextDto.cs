using System.Text.Json.Serialization;

namespace XenonClinic.Web.Models;

/// <summary>
/// Response DTO for the tenant context API
/// Contains all configuration needed by the frontend
/// </summary>
public class TenantContextDto
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyType { get; set; } = "CLINIC";
    public string? ClinicType { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;

    // Branding
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1F6FEB";
    public string SecondaryColor { get; set; } = "#6B7280";

    // User context
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> UserRoles { get; set; } = new();
    public List<string> UserPermissions { get; set; } = new();

    // Configuration (merged based on precedence)
    public Dictionary<string, FeatureConfig> Features { get; set; } = new();
    public Dictionary<string, string> Terminology { get; set; } = new();
    public List<NavItemDto> Navigation { get; set; } = new();

    // UI configuration
    public Dictionary<string, UISchemaDto> UISchemas { get; set; } = new();
    public Dictionary<string, FormLayoutDto> FormLayouts { get; set; } = new();
    public Dictionary<string, ListLayoutDto> ListLayouts { get; set; } = new();

    // Settings
    public TenantSettingsDto Settings { get; set; } = new();
}

public class FeatureConfig
{
    public bool Enabled { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
}

public class NavItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty; // terminology key
    public string Icon { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string FeatureCode { get; set; } = string.Empty;
    public List<string>? RequiredRoles { get; set; }
    public List<NavItemDto>? Children { get; set; }
    public NavBadgeDto? Badge { get; set; }
    public int SortOrder { get; set; }
}

public class NavBadgeDto
{
    public string Type { get; set; } = "count"; // count or dot
    public string? CountKey { get; set; }
}

public class UISchemaDto
{
    public string EntityName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DisplayNamePlural { get; set; } = string.Empty;
    public string PrimaryField { get; set; } = string.Empty;
    public List<FieldDefinitionDto> Fields { get; set; } = new();
    public DefaultSortDto? DefaultSort { get; set; }
}

public class FieldDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public object? DefaultValue { get; set; }
    public FieldValidationDto? Validation { get; set; }

    // For select/lookup
    public List<OptionDto>? Options { get; set; }
    public string? LookupEndpoint { get; set; }
    public string? LookupDisplayField { get; set; }
    public string? LookupValueField { get; set; }

    // Visibility/behavior
    public object? Visible { get; set; } // bool or ConditionalRule[]
    public object? Disabled { get; set; }
    public bool? ReadOnly { get; set; }

    // Display
    public string? Width { get; set; }
    public bool? Sortable { get; set; }
    public bool? Filterable { get; set; }
    public bool? Searchable { get; set; }

    // For currency/number
    public string? Currency { get; set; }
    public int? Decimals { get; set; }

    // For file upload
    public string? Accept { get; set; }
    public int? MaxSize { get; set; }
    public bool? Multiple { get; set; }
}

public class FieldValidationDto
{
    public bool? Required { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public decimal? Min { get; set; }
    public decimal? Max { get; set; }
    public string? Pattern { get; set; }
    public string? PatternMessage { get; set; }
    public string? Custom { get; set; }
}

public class OptionDto
{
    public object Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class DefaultSortDto
{
    public string Field { get; set; } = string.Empty;
    public string Direction { get; set; } = "asc";
}

public class FormLayoutDto
{
    public string EntityName { get; set; } = string.Empty;
    public List<FormSectionDto> Sections { get; set; } = new();
    public string? SubmitLabel { get; set; }
    public string? CancelLabel { get; set; }
    public bool? ShowDelete { get; set; }
    public string? DeleteConfirmMessage { get; set; }
}

public class FormSectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? Collapsible { get; set; }
    public bool? DefaultCollapsed { get; set; }
    public object? Visible { get; set; }
    public int? Columns { get; set; }
    public List<string> Fields { get; set; } = new();
}

public class ListLayoutDto
{
    public string EntityName { get; set; } = string.Empty;
    public List<ListColumnDto> Columns { get; set; } = new();
    public ListActionsDto Actions { get; set; } = new();
    public List<ListFilterDto> Filters { get; set; } = new();
    public int DefaultPageSize { get; set; } = 25;
    public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
    public bool ShowSearch { get; set; } = true;
    public List<string> SearchFields { get; set; } = new();
}

public class ListColumnDto
{
    public string Field { get; set; } = string.Empty;
    public object? Width { get; set; }
    public string? Align { get; set; }
    public string? Format { get; set; }
    public bool? Sortable { get; set; }
    public bool? Hidden { get; set; }
}

public class ListActionsDto
{
    public List<ListActionDto> Row { get; set; } = new();
    public List<ListActionDto> Bulk { get; set; } = new();
    public List<ListActionDto> Header { get; set; } = new();
}

public class ListActionDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Type { get; set; } = "secondary";
    public bool? RequiresSelection { get; set; }
    public string? ConfirmMessage { get; set; }
    public string? FeatureCode { get; set; }
    public List<string>? RequiredRoles { get; set; }
}

public class ListFilterDto
{
    public string Field { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public List<OptionDto>? Options { get; set; }
}

public class TenantSettingsDto
{
    public string Currency { get; set; } = "AED";
    public string Timezone { get; set; } = "Arabian Standard Time";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "HH:mm";
    public string Language { get; set; } = "en";
}

public class ConditionalRuleDto
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = "eq";
    public object? Value { get; set; }
}
