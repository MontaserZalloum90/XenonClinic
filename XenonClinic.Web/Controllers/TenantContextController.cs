using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Infrastructure.Services;
using XenonClinic.Web.Models;

namespace XenonClinic.Web.Controllers;

/// <summary>
/// API controller for tenant context and configuration
/// </summary>
[ApiController]
[Route("api/tenant")]
[Authorize]
public class TenantContextController : ControllerBase
{
    private readonly TenantContextService _tenantContextService;
    private readonly ILogger<TenantContextController> _logger;

    public TenantContextController(
        TenantContextService tenantContextService,
        ILogger<TenantContextController> logger)
    {
        _tenantContextService = tenantContextService;
        _logger = logger;
    }

    /// <summary>
    /// Get the fully merged tenant context for the current user
    /// </summary>
    [HttpGet("context")]
    [ProducesResponseType(typeof(TenantContextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TenantContextDto>> GetContext()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "";
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Get tenant/company/branch from claims or use defaults
            var tenantIdClaim = User.FindFirstValue("TenantId");
            var companyIdClaim = User.FindFirstValue("CompanyId");
            var branchIdClaim = User.FindFirstValue("BranchId");

            // Default to 1 if not set (for demo/development)
            var tenantId = int.TryParse(tenantIdClaim, out var tid) ? tid : 1;
            var companyId = int.TryParse(companyIdClaim, out var cid) ? cid : 1;
            var branchId = int.TryParse(branchIdClaim, out var bid) ? bid : 1;

            var context = await _tenantContextService.GetTenantContextAsync(
                tenantId, companyId, branchId, userId, userName, roles);

            // Map to DTO
            var dto = MapToDto(context);
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant context not found");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant context");
            return StatusCode(500, new { message = "Error loading configuration" });
        }
    }

    private static TenantContextDto MapToDto(TenantContext context)
    {
        return new TenantContextDto
        {
            TenantId = context.TenantId,
            TenantName = context.TenantName,
            CompanyId = context.CompanyId,
            CompanyName = context.CompanyName,
            CompanyType = context.CompanyType,
            ClinicType = context.ClinicType,
            BranchId = context.BranchId,
            BranchName = context.BranchName,
            LogoUrl = context.LogoUrl,
            PrimaryColor = context.PrimaryColor,
            SecondaryColor = context.SecondaryColor,
            UserId = context.UserId,
            UserName = context.UserName,
            UserRoles = context.UserRoles,
            UserPermissions = context.UserPermissions,
            Features = context.Features.ToDictionary(
                kvp => kvp.Key,
                kvp => new Models.FeatureConfig
                {
                    Enabled = kvp.Value.Enabled,
                    Settings = kvp.Value.Settings
                }),
            Terminology = context.Terminology,
            Navigation = context.Navigation.Select(MapNavItem).ToList(),
            UISchemas = context.UISchemas.ToDictionary(
                kvp => kvp.Key,
                kvp => MapUISchema(kvp.Value)),
            FormLayouts = context.FormLayouts.ToDictionary(
                kvp => kvp.Key,
                kvp => MapFormLayout(kvp.Value)),
            ListLayouts = context.ListLayouts.ToDictionary(
                kvp => kvp.Key,
                kvp => MapListLayout(kvp.Value)),
            Settings = new TenantSettingsDto
            {
                Currency = context.Settings.Currency,
                Timezone = context.Settings.Timezone,
                DateFormat = context.Settings.DateFormat,
                TimeFormat = context.Settings.TimeFormat,
                Language = context.Settings.Language
            }
        };
    }

    private static NavItemDto MapNavItem(NavItem item)
    {
        return new NavItemDto
        {
            Id = item.Id,
            Label = item.Label,
            Icon = item.Icon,
            Route = item.Route,
            FeatureCode = item.FeatureCode,
            RequiredRoles = item.RequiredRoles,
            Children = item.Children?.Select(MapNavItem).ToList(),
            Badge = item.Badge != null ? new NavBadgeDto
            {
                Type = item.Badge.Type,
                CountKey = item.Badge.CountKey
            } : null,
            SortOrder = item.SortOrder
        };
    }

    private static UISchemaDto MapUISchema(UISchema schema)
    {
        return new UISchemaDto
        {
            EntityName = schema.EntityName,
            DisplayName = schema.DisplayName,
            DisplayNamePlural = schema.DisplayNamePlural,
            PrimaryField = schema.PrimaryField,
            Fields = schema.Fields.Select(f => new FieldDefinitionDto
            {
                Name = f.Name,
                Type = f.Type,
                Label = f.Label,
                Placeholder = f.Placeholder,
                HelpText = f.HelpText,
                DefaultValue = f.DefaultValue,
                Validation = f.Validation != null ? new FieldValidationDto
                {
                    Required = f.Validation.Required,
                    MinLength = f.Validation.MinLength,
                    MaxLength = f.Validation.MaxLength,
                    Min = f.Validation.Min,
                    Max = f.Validation.Max,
                    Pattern = f.Validation.Pattern,
                    PatternMessage = f.Validation.PatternMessage,
                    Custom = f.Validation.Custom
                } : null,
                Options = f.Options?.Select(o => new OptionDto
                {
                    Value = o.Value,
                    Label = o.Label
                }).ToList(),
                LookupEndpoint = f.LookupEndpoint,
                LookupDisplayField = f.LookupDisplayField,
                LookupValueField = f.LookupValueField,
                Visible = f.Visible,
                Disabled = f.Disabled,
                ReadOnly = f.ReadOnly,
                Width = f.Width,
                Sortable = f.Sortable,
                Filterable = f.Filterable,
                Searchable = f.Searchable,
                Currency = f.Currency,
                Decimals = f.Decimals,
                Accept = f.Accept,
                MaxSize = f.MaxSize,
                Multiple = f.Multiple
            }).ToList(),
            DefaultSort = schema.DefaultSort != null ? new DefaultSortDto
            {
                Field = schema.DefaultSort.Field,
                Direction = schema.DefaultSort.Direction
            } : null
        };
    }

    private static FormLayoutDto MapFormLayout(FormLayout layout)
    {
        return new FormLayoutDto
        {
            EntityName = layout.EntityName,
            Sections = layout.Sections.Select(s => new FormSectionDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Collapsible = s.Collapsible,
                DefaultCollapsed = s.DefaultCollapsed,
                Visible = s.Visible,
                Columns = s.Columns,
                Fields = s.Fields
            }).ToList(),
            SubmitLabel = layout.SubmitLabel,
            CancelLabel = layout.CancelLabel,
            ShowDelete = layout.ShowDelete,
            DeleteConfirmMessage = layout.DeleteConfirmMessage
        };
    }

    private static ListLayoutDto MapListLayout(ListLayout layout)
    {
        return new ListLayoutDto
        {
            EntityName = layout.EntityName,
            Columns = layout.Columns.Select(c => new ListColumnDto
            {
                Field = c.Field,
                Width = c.Width,
                Align = c.Align,
                Format = c.Format,
                Sortable = c.Sortable,
                Hidden = c.Hidden
            }).ToList(),
            Actions = new ListActionsDto
            {
                Row = layout.Actions.Row.Select(a => new ListActionDto
                {
                    Id = a.Id,
                    Label = a.Label,
                    Icon = a.Icon,
                    Type = a.Type,
                    RequiresSelection = a.RequiresSelection,
                    ConfirmMessage = a.ConfirmMessage,
                    FeatureCode = a.FeatureCode,
                    RequiredRoles = a.RequiredRoles
                }).ToList(),
                Bulk = layout.Actions.Bulk.Select(a => new ListActionDto
                {
                    Id = a.Id,
                    Label = a.Label,
                    Icon = a.Icon,
                    Type = a.Type,
                    RequiresSelection = a.RequiresSelection,
                    ConfirmMessage = a.ConfirmMessage,
                    FeatureCode = a.FeatureCode,
                    RequiredRoles = a.RequiredRoles
                }).ToList(),
                Header = layout.Actions.Header.Select(a => new ListActionDto
                {
                    Id = a.Id,
                    Label = a.Label,
                    Icon = a.Icon,
                    Type = a.Type,
                    RequiresSelection = a.RequiresSelection,
                    ConfirmMessage = a.ConfirmMessage,
                    FeatureCode = a.FeatureCode,
                    RequiredRoles = a.RequiredRoles
                }).ToList()
            },
            Filters = layout.Filters.Select(f => new ListFilterDto
            {
                Field = f.Field,
                Type = f.Type,
                Options = f.Options?.Select(o => new OptionDto
                {
                    Value = o.Value,
                    Label = o.Label
                }).ToList()
            }).ToList(),
            DefaultPageSize = layout.DefaultPageSize,
            PageSizeOptions = layout.PageSizeOptions,
            ShowSearch = layout.ShowSearch,
            SearchFields = layout.SearchFields
        };
    }
}
