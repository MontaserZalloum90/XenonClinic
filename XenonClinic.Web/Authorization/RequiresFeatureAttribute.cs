using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Web.Authorization;

/// <summary>
/// Authorization attribute that requires a specific feature to be enabled for the tenant
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequiresFeatureAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _featureCode;
    private readonly string[] _additionalFeatures;

    public RequiresFeatureAttribute(string featureCode, params string[] additionalFeatures)
    {
        _featureCode = featureCode;
        _additionalFeatures = additionalFeatures;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var tenantIdClaim = user.FindFirstValue("TenantId");
        var companyIdClaim = user.FindFirstValue("CompanyId");

        // Default to 1 if not set (for demo/development)
        var tenantId = int.TryParse(tenantIdClaim, out var tid) ? tid : 1;
        var companyId = int.TryParse(companyIdClaim, out var cid) ? cid : 1;

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<XenonClinicDbContext>();

        // Get company with types
        var company = await dbContext.Companies
            .Include(c => c.CompanyType)
                .ThenInclude(ct => ct!.Template)
            .Include(c => c.ClinicType)
                .ThenInclude(ct => ct!.Template)
            .FirstOrDefaultAsync(c => c.Id == companyId && c.TenantId == tenantId);

        if (company == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Get tenant feature overrides
        var tenantFeatures = await dbContext.TenantFeatures
            .Where(tf => tf.TenantId == tenantId)
            .ToDictionaryAsync(tf => tf.FeatureCode, tf => tf.Enabled);

        // Build enabled features set
        var enabledFeatures = new HashSet<string>();

        // 1. Add company type template features
        if (company.CompanyType?.Template != null &&
            !string.IsNullOrEmpty(company.CompanyType.Template.FeaturesJson))
        {
            var features = System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                company.CompanyType.Template.FeaturesJson);
            if (features != null)
            {
                foreach (var f in features)
                    enabledFeatures.Add(f);
            }
        }

        // 2. Add clinic type template features
        if (company.ClinicType?.Template != null &&
            !string.IsNullOrEmpty(company.ClinicType.Template.FeaturesJson))
        {
            var features = System.Text.Json.JsonSerializer.Deserialize<List<string>>(
                company.ClinicType.Template.FeaturesJson);
            if (features != null)
            {
                foreach (var f in features)
                    enabledFeatures.Add(f);
            }
        }

        // 3. Apply tenant overrides
        foreach (var (code, enabled) in tenantFeatures)
        {
            if (enabled)
                enabledFeatures.Add(code);
            else
                enabledFeatures.Remove(code);
        }

        // Check if required feature(s) are enabled
        var requiredFeatures = new[] { _featureCode }.Concat(_additionalFeatures);
        var allFeaturesEnabled = requiredFeatures.All(f => enabledFeatures.Contains(f));

        if (!allFeaturesEnabled)
        {
            context.Result = new ObjectResult(new
            {
                message = "Feature not enabled for your organization",
                featureCode = _featureCode
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}

/// <summary>
/// Authorization policy requirement for feature-based access
/// </summary>
public class FeatureRequirement : Microsoft.AspNetCore.Authorization.IAuthorizationRequirement
{
    public string FeatureCode { get; }

    public FeatureRequirement(string featureCode)
    {
        FeatureCode = featureCode;
    }
}
