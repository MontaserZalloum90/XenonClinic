using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for building merged tenant context based on configuration precedence:
/// System Defaults → CompanyType Template → ClinicType Template → Tenant Overrides
/// </summary>
public class TenantContextService
{
    private readonly XenonClinicDbContext _context;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public TenantContextService(XenonClinicDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get fully merged tenant context for a user
    /// </summary>
    public async Task<TenantContext> GetTenantContextAsync(
        int tenantId,
        int companyId,
        int branchId,
        string userId,
        string userName,
        IEnumerable<string> roles)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Settings)
            .Include(t => t.Features)
                .ThenInclude(f => f.Feature)
            .Include(t => t.Terminology)
            .Include(t => t.UISchemas)
            .Include(t => t.FormLayouts)
            .Include(t => t.ListLayouts)
            .Include(t => t.Navigation)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
            throw new InvalidOperationException($"Tenant {tenantId} not found");

        var company = await _context.Companies
            .Include(c => c.CompanyType)
                .ThenInclude(ct => ct!.Template)
            .Include(c => c.ClinicType)
                .ThenInclude(ct => ct!.Template)
            .FirstOrDefaultAsync(c => c.Id == companyId && c.TenantId == tenantId);

        if (company == null)
            throw new InvalidOperationException($"Company {companyId} not found for tenant {tenantId}");

        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == branchId);

        // Build merged context
        var context = new TenantContext
        {
            TenantId = tenantId,
            TenantName = tenant.Name,
            CompanyId = companyId,
            CompanyName = company.Name,
            CompanyType = company.CompanyTypeCode,
            ClinicType = company.ClinicTypeCode,
            BranchId = branchId,
            BranchName = branch?.Name ?? "Default",
            LogoUrl = company.LogoPath ?? tenant.LogoPath,
            PrimaryColor = company.PrimaryColor ?? tenant.PrimaryColor,
            SecondaryColor = company.SecondaryColor ?? tenant.SecondaryColor,
            UserId = userId,
            UserName = userName,
            UserRoles = roles.ToList()
        };

        // Merge features
        context.Features = await MergeFeaturesAsync(company, tenant);

        // Merge terminology
        context.Terminology = MergeTerminology(company, tenant);

        // Merge navigation
        context.Navigation = MergeNavigation(company, tenant, context.Features, roles);

        // Merge UI schemas
        context.UISchemas = MergeUISchemas(company, tenant);

        // Merge form layouts
        context.FormLayouts = MergeFormLayouts(company, tenant);

        // Merge list layouts
        context.ListLayouts = MergeListLayouts(company, tenant);

        // Settings
        context.Settings = new TenantSettings
        {
            Currency = company.Currency ?? tenant.Settings?.DefaultCurrency ?? "AED",
            Timezone = company.Timezone ?? tenant.Settings?.DefaultTimezone ?? "Arabian Standard Time",
            DateFormat = tenant.Settings?.DateFormat ?? "dd/MM/yyyy",
            TimeFormat = tenant.Settings?.TimeFormat ?? "HH:mm",
            Language = tenant.Settings?.DefaultLanguage ?? "en"
        };

        return context;
    }

    private async Task<Dictionary<string, FeatureConfig>> MergeFeaturesAsync(Company company, Tenant tenant)
    {
        var result = new Dictionary<string, FeatureConfig>();

        // 1. Get all features
        var allFeatures = await _context.Features.ToListAsync();

        // 2. Apply company type template features
        var companyTypeTemplate = company.CompanyType?.Template;
        if (companyTypeTemplate != null && !string.IsNullOrEmpty(companyTypeTemplate.FeaturesJson))
        {
            var templateFeatures = JsonSerializer.Deserialize<List<string>>(companyTypeTemplate.FeaturesJson, JsonOptions);
            if (templateFeatures != null)
            {
                foreach (var code in templateFeatures)
                {
                    result[code] = new FeatureConfig { Enabled = true };
                }
            }
        }

        // 3. Apply clinic type template features (additive)
        var clinicTypeTemplate = company.ClinicType?.Template;
        if (clinicTypeTemplate != null && !string.IsNullOrEmpty(clinicTypeTemplate.FeaturesJson))
        {
            var templateFeatures = JsonSerializer.Deserialize<List<string>>(clinicTypeTemplate.FeaturesJson, JsonOptions);
            if (templateFeatures != null)
            {
                foreach (var code in templateFeatures)
                {
                    result[code] = new FeatureConfig { Enabled = true };
                }
            }
        }

        // 4. Apply tenant overrides
        foreach (var tf in tenant.Features)
        {
            result[tf.FeatureCode] = new FeatureConfig
            {
                Enabled = tf.Enabled,
                Settings = !string.IsNullOrEmpty(tf.SettingsJson)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(tf.SettingsJson, JsonOptions)
                    : null
            };
        }

        return result;
    }

    private Dictionary<string, string> MergeTerminology(Company company, Tenant tenant)
    {
        var result = new Dictionary<string, string>();

        // 1. Apply system defaults
        foreach (var (key, value) in GetDefaultTerminology())
        {
            result[key] = value;
        }

        // 2. Apply company type template terminology
        var companyTypeTemplate = company.CompanyType?.Template;
        if (companyTypeTemplate != null && !string.IsNullOrEmpty(companyTypeTemplate.TerminologyJson))
        {
            var templateTerms = JsonSerializer.Deserialize<Dictionary<string, string>>(companyTypeTemplate.TerminologyJson, JsonOptions);
            if (templateTerms != null)
            {
                foreach (var (key, value) in templateTerms)
                {
                    result[key] = value;
                }
            }
        }

        // 3. Apply clinic type template terminology
        var clinicTypeTemplate = company.ClinicType?.Template;
        if (clinicTypeTemplate != null && !string.IsNullOrEmpty(clinicTypeTemplate.TerminologyJson))
        {
            var templateTerms = JsonSerializer.Deserialize<Dictionary<string, string>>(clinicTypeTemplate.TerminologyJson, JsonOptions);
            if (templateTerms != null)
            {
                foreach (var (key, value) in templateTerms)
                {
                    result[key] = value;
                }
            }
        }

        // 4. Apply tenant overrides
        foreach (var tt in tenant.Terminology)
        {
            result[tt.Key] = tt.Value;
        }

        return result;
    }

    private List<NavItem> MergeNavigation(
        Company company,
        Tenant tenant,
        Dictionary<string, FeatureConfig> features,
        IEnumerable<string> userRoles)
    {
        var navItems = new Dictionary<string, NavItem>();
        var rolesList = userRoles.ToList();

        // 1. Apply company type template navigation
        var companyTypeTemplate = company.CompanyType?.Template;
        if (companyTypeTemplate != null && !string.IsNullOrEmpty(companyTypeTemplate.NavigationJson))
        {
            var templateNav = JsonSerializer.Deserialize<List<NavItem>>(companyTypeTemplate.NavigationJson, JsonOptions);
            if (templateNav != null)
            {
                foreach (var item in templateNav)
                {
                    navItems[item.Id] = item;
                }
            }
        }

        // 2. Apply clinic type template navigation (merge/override by ID)
        var clinicTypeTemplate = company.ClinicType?.Template;
        if (clinicTypeTemplate != null && !string.IsNullOrEmpty(clinicTypeTemplate.NavigationJson))
        {
            var templateNav = JsonSerializer.Deserialize<List<NavItem>>(clinicTypeTemplate.NavigationJson, JsonOptions);
            if (templateNav != null)
            {
                foreach (var item in templateNav)
                {
                    navItems[item.Id] = item;
                }
            }
        }

        // 3. Apply tenant overrides
        if (tenant.Navigation != null && !string.IsNullOrEmpty(tenant.Navigation.NavigationJson))
        {
            var tenantNav = JsonSerializer.Deserialize<List<NavItem>>(tenant.Navigation.NavigationJson, JsonOptions);
            if (tenantNav != null)
            {
                foreach (var item in tenantNav)
                {
                    navItems[item.Id] = item;
                }
            }
        }

        // 4. Filter by enabled features and roles
        return navItems.Values
            .Where(item => IsNavItemVisible(item, features, rolesList))
            .OrderBy(item => item.SortOrder)
            .Select(item => FilterNavChildren(item, features, rolesList))
            .ToList();
    }

    private bool IsNavItemVisible(NavItem item, Dictionary<string, FeatureConfig> features, List<string> userRoles)
    {
        // Check feature is enabled
        if (!string.IsNullOrEmpty(item.FeatureCode))
        {
            if (!features.TryGetValue(item.FeatureCode, out var feature) || !feature.Enabled)
                return false;
        }

        // Check roles
        if (item.RequiredRoles != null && item.RequiredRoles.Count > 0)
        {
            if (!item.RequiredRoles.Any(r => userRoles.Contains(r)))
                return false;
        }

        return true;
    }

    private NavItem FilterNavChildren(NavItem item, Dictionary<string, FeatureConfig> features, List<string> userRoles)
    {
        if (item.Children == null || item.Children.Count == 0)
            return item;

        var filteredChildren = item.Children
            .Where(child => IsNavItemVisible(child, features, userRoles))
            .OrderBy(child => child.SortOrder)
            .Select(child => FilterNavChildren(child, features, userRoles))
            .ToList();

        return new NavItem
        {
            Id = item.Id,
            Label = item.Label,
            Icon = item.Icon,
            Route = item.Route,
            FeatureCode = item.FeatureCode,
            RequiredRoles = item.RequiredRoles,
            Children = filteredChildren.Count > 0 ? filteredChildren : null,
            Badge = item.Badge,
            SortOrder = item.SortOrder
        };
    }

    private Dictionary<string, UISchema> MergeUISchemas(Company company, Tenant tenant)
    {
        var result = new Dictionary<string, UISchema>();

        // 1. Apply company type template schemas
        var companyTypeTemplate = company.CompanyType?.Template;
        if (companyTypeTemplate != null && !string.IsNullOrEmpty(companyTypeTemplate.UISchemasJson))
        {
            var schemas = JsonSerializer.Deserialize<Dictionary<string, UISchema>>(companyTypeTemplate.UISchemasJson, JsonOptions);
            if (schemas != null)
            {
                foreach (var (key, value) in schemas)
                {
                    result[key] = value;
                }
            }
        }

        // 2. Apply clinic type template schemas
        var clinicTypeTemplate = company.ClinicType?.Template;
        if (clinicTypeTemplate != null && !string.IsNullOrEmpty(clinicTypeTemplate.UISchemasJson))
        {
            var schemas = JsonSerializer.Deserialize<Dictionary<string, UISchema>>(clinicTypeTemplate.UISchemasJson, JsonOptions);
            if (schemas != null)
            {
                foreach (var (key, value) in schemas)
                {
                    result[key] = value;
                }
            }
        }

        // 3. Apply tenant overrides
        foreach (var schema in tenant.UISchemas)
        {
            var parsed = JsonSerializer.Deserialize<UISchema>(schema.SchemaJson, JsonOptions);
            if (parsed != null)
            {
                result[schema.EntityName] = parsed;
            }
        }

        return result;
    }

    private Dictionary<string, FormLayout> MergeFormLayouts(Company company, Tenant tenant)
    {
        var result = new Dictionary<string, FormLayout>();

        // 1. Apply company type template layouts
        var companyTypeTemplate = company.CompanyType?.Template;
        if (companyTypeTemplate != null && !string.IsNullOrEmpty(companyTypeTemplate.FormLayoutsJson))
        {
            var layouts = JsonSerializer.Deserialize<Dictionary<string, FormLayout>>(companyTypeTemplate.FormLayoutsJson, JsonOptions);
            if (layouts != null)
            {
                foreach (var (key, value) in layouts)
                {
                    result[key] = value;
                }
            }
        }

        // 2. Apply clinic type template layouts
        var clinicTypeTemplate = company.ClinicType?.Template;
        if (clinicTypeTemplate != null && !string.IsNullOrEmpty(clinicTypeTemplate.FormLayoutsJson))
        {
            var layouts = JsonSerializer.Deserialize<Dictionary<string, FormLayout>>(clinicTypeTemplate.FormLayoutsJson, JsonOptions);
            if (layouts != null)
            {
                foreach (var (key, value) in layouts)
                {
                    result[key] = value;
                }
            }
        }

        // 3. Apply tenant overrides
        foreach (var layout in tenant.FormLayouts)
        {
            var parsed = JsonSerializer.Deserialize<FormLayout>(layout.LayoutJson, JsonOptions);
            if (parsed != null)
            {
                result[layout.EntityName] = parsed;
            }
        }

        return result;
    }

    private Dictionary<string, ListLayout> MergeListLayouts(Company company, Tenant tenant)
    {
        var result = new Dictionary<string, ListLayout>();

        // 1. Apply company type template layouts
        var companyTypeTemplate = company.CompanyType?.Template;
        if (companyTypeTemplate != null && !string.IsNullOrEmpty(companyTypeTemplate.ListLayoutsJson))
        {
            var layouts = JsonSerializer.Deserialize<Dictionary<string, ListLayout>>(companyTypeTemplate.ListLayoutsJson, JsonOptions);
            if (layouts != null)
            {
                foreach (var (key, value) in layouts)
                {
                    result[key] = value;
                }
            }
        }

        // 2. Apply clinic type template layouts
        var clinicTypeTemplate = company.ClinicType?.Template;
        if (clinicTypeTemplate != null && !string.IsNullOrEmpty(clinicTypeTemplate.ListLayoutsJson))
        {
            var layouts = JsonSerializer.Deserialize<Dictionary<string, ListLayout>>(clinicTypeTemplate.ListLayoutsJson, JsonOptions);
            if (layouts != null)
            {
                foreach (var (key, value) in layouts)
                {
                    result[key] = value;
                }
            }
        }

        // 3. Apply tenant overrides
        foreach (var layout in tenant.ListLayouts)
        {
            var parsed = JsonSerializer.Deserialize<ListLayout>(layout.LayoutJson, JsonOptions);
            if (parsed != null)
            {
                result[layout.EntityName] = parsed;
            }
        }

        return result;
    }

    private static Dictionary<string, string> GetDefaultTerminology()
    {
        return new Dictionary<string, string>
        {
            // Entity names
            ["entity.patient.singular"] = "Patient",
            ["entity.patient.plural"] = "Patients",
            ["entity.visit.singular"] = "Visit",
            ["entity.visit.plural"] = "Visits",
            ["entity.appointment.singular"] = "Appointment",
            ["entity.appointment.plural"] = "Appointments",
            ["entity.invoice.singular"] = "Invoice",
            ["entity.invoice.plural"] = "Invoices",

            // Roles
            ["role.doctor"] = "Doctor",
            ["role.nurse"] = "Nurse",
            ["role.receptionist"] = "Receptionist",

            // Navigation
            ["nav.dashboard"] = "Dashboard",
            ["nav.patients"] = "Patients",
            ["nav.appointments"] = "Appointments",
            ["nav.visits"] = "Visits",
            ["nav.billing"] = "Billing",
            ["nav.inventory"] = "Inventory",
            ["nav.laboratory"] = "Laboratory",
            ["nav.reports"] = "Reports",
            ["nav.hr"] = "HR",
            ["nav.settings"] = "Settings",

            // Actions
            ["action.add"] = "Add",
            ["action.edit"] = "Edit",
            ["action.delete"] = "Delete",
            ["action.save"] = "Save",
            ["action.cancel"] = "Cancel",
            ["action.view"] = "View",
            ["action.export"] = "Export",
            ["action.import"] = "Import",

            // Common
            ["common.loading"] = "Loading...",
            ["common.noResults"] = "No results found",
            ["common.search"] = "Search",
            ["common.filter"] = "Filter",
            ["common.confirm"] = "Confirm",
            ["confirm.delete"] = "Are you sure you want to delete this item?",
            ["confirm.bulkDelete"] = "Are you sure you want to delete the selected items?"
        };
    }
}

// Internal models for JSON deserialization
public class TenantContext
{
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyType { get; set; } = "CLINIC";
    public string? ClinicType { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1F6FEB";
    public string SecondaryColor { get; set; } = "#6B7280";
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> UserRoles { get; set; } = new();
    public List<string> UserPermissions { get; set; } = new();
    public Dictionary<string, FeatureConfig> Features { get; set; } = new();
    public Dictionary<string, string> Terminology { get; set; } = new();
    public List<NavItem> Navigation { get; set; } = new();
    public Dictionary<string, UISchema> UISchemas { get; set; } = new();
    public Dictionary<string, FormLayout> FormLayouts { get; set; } = new();
    public Dictionary<string, ListLayout> ListLayouts { get; set; } = new();
    public TenantSettings Settings { get; set; } = new();
}

public class FeatureConfig
{
    public bool Enabled { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
}

public class NavItem
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string FeatureCode { get; set; } = string.Empty;
    public List<string>? RequiredRoles { get; set; }
    public List<NavItem>? Children { get; set; }
    public NavBadge? Badge { get; set; }
    public int SortOrder { get; set; }
}

public class NavBadge
{
    public string Type { get; set; } = "count";
    public string? CountKey { get; set; }
}

public class UISchema
{
    public string EntityName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DisplayNamePlural { get; set; } = string.Empty;
    public string PrimaryField { get; set; } = string.Empty;
    public List<FieldDefinition> Fields { get; set; } = new();
    public DefaultSort? DefaultSort { get; set; }
}

public class FieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public string? Label { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public object? DefaultValue { get; set; }
    public FieldValidation? Validation { get; set; }
    public List<Option>? Options { get; set; }
    public string? LookupEndpoint { get; set; }
    public string? LookupDisplayField { get; set; }
    public string? LookupValueField { get; set; }
    public object? Visible { get; set; }
    public object? Disabled { get; set; }
    public bool? ReadOnly { get; set; }
    public string? Width { get; set; }
    public bool? Sortable { get; set; }
    public bool? Filterable { get; set; }
    public bool? Searchable { get; set; }
    public string? Currency { get; set; }
    public int? Decimals { get; set; }
    public string? Accept { get; set; }
    public int? MaxSize { get; set; }
    public bool? Multiple { get; set; }
}

public class FieldValidation
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

public class Option
{
    public object Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class DefaultSort
{
    public string Field { get; set; } = string.Empty;
    public string Direction { get; set; } = "asc";
}

public class FormLayout
{
    public string EntityName { get; set; } = string.Empty;
    public List<FormSection> Sections { get; set; } = new();
    public string? SubmitLabel { get; set; }
    public string? CancelLabel { get; set; }
    public bool? ShowDelete { get; set; }
    public string? DeleteConfirmMessage { get; set; }
}

public class FormSection
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

public class ListLayout
{
    public string EntityName { get; set; } = string.Empty;
    public List<ListColumn> Columns { get; set; } = new();
    public ListActions Actions { get; set; } = new();
    public List<ListFilter> Filters { get; set; } = new();
    public int DefaultPageSize { get; set; } = 25;
    public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
    public bool ShowSearch { get; set; } = true;
    public List<string> SearchFields { get; set; } = new();
}

public class ListColumn
{
    public string Field { get; set; } = string.Empty;
    public object? Width { get; set; }
    public string? Align { get; set; }
    public string? Format { get; set; }
    public bool? Sortable { get; set; }
    public bool? Hidden { get; set; }
}

public class ListActions
{
    public List<ListAction> Row { get; set; } = new();
    public List<ListAction> Bulk { get; set; } = new();
    public List<ListAction> Header { get; set; } = new();
}

public class ListAction
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

public class ListFilter
{
    public string Field { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public List<Option>? Options { get; set; }
}

public class TenantSettings
{
    public string Currency { get; set; } = "AED";
    public string Timezone { get; set; } = "Arabian Standard Time";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "HH:mm";
    public string Language { get; set; } = "en";
}
