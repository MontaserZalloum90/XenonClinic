using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Web.Middleware;

/// <summary>
/// Middleware to validate module licenses before allowing access to module-specific routes
/// </summary>
public class LicenseValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LicenseValidationMiddleware> _logger;

    // Map controller names to module names
    private static readonly Dictionary<string, string> ControllerModuleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Cases", "CaseManagement" },
        { "CaseActivities", "CaseManagement" },
        { "CaseNotes", "CaseManagement" },
        { "AudiologyVisits", "Audiology" },
        { "Audiograms", "Audiology" },
        { "HearingDevices", "Audiology" },
        { "Lab", "Laboratory" },
        { "LabOrders", "Laboratory" },
        { "LabResults", "Laboratory" },
        { "HR", "HR" },
        { "Employees", "HR" },
        { "Attendance", "HR" },
        { "Finance", "Financial" },
        { "Invoices", "Financial" },
        { "Expenses", "Financial" },
        { "Inventory", "Inventory" },
        { "InventoryItems", "Inventory" },
        { "StockMovements", "Inventory" },
        { "Sales", "Sales" },
        { "SalesOrders", "Sales" },
        { "Quotations", "Sales" },
        { "Procurement", "Procurement" },
        { "PurchaseOrders", "Procurement" },
        { "Suppliers", "Procurement" }
    };

    public LicenseValidationMiddleware(
        RequestDelegate next,
        ILogger<LicenseValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ILicenseValidator licenseValidator,
        IModuleManager moduleManager)
    {
        // Skip validation for static files, health checks, etc.
        if (ShouldSkipValidation(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Extract controller name from route
        var controllerName = context.GetRouteValue("controller")?.ToString();

        if (!string.IsNullOrEmpty(controllerName) && ControllerModuleMap.TryGetValue(controllerName, out var moduleName))
        {
            // Check if module is enabled
            if (!moduleManager.IsModuleEnabled(moduleName))
            {
                _logger.LogWarning("Access denied: Module '{ModuleName}' is not enabled", moduleName);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync($"Access denied: The {moduleName} module is not enabled.");
                return;
            }

            // Validate license
            var validationResult = licenseValidator.ValidateModuleLicense(moduleName);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "License validation failed for module '{ModuleName}': {Message}",
                    moduleName,
                    validationResult.ValidationMessage);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                if (validationResult.Status == LicenseValidationStatus.Expired)
                {
                    await context.Response.WriteAsync(
                        $"Access denied: The {moduleName} module license expired on {validationResult.ExpiryDate:yyyy-MM-dd}. " +
                        "Please contact your administrator to renew the license.");
                }
                else if (validationResult.Status == LicenseValidationStatus.NotLicensed)
                {
                    await context.Response.WriteAsync(
                        $"Access denied: The {moduleName} module is not licensed. " +
                        "Please contact your administrator to purchase a license.");
                }
                else
                {
                    await context.Response.WriteAsync(
                        $"Access denied: {validationResult.ValidationMessage}");
                }

                return;
            }

            // Check if license is expiring soon (within 30 days) - just log a warning
            if (validationResult.DaysUntilExpiry.HasValue && validationResult.DaysUntilExpiry.Value <= 30 && validationResult.DaysUntilExpiry.Value > 0)
            {
                _logger.LogWarning(
                    "Module '{ModuleName}' license will expire in {Days} days on {ExpiryDate}",
                    moduleName,
                    validationResult.DaysUntilExpiry.Value,
                    validationResult.ExpiryDate);
            }
        }

        await _next(context);
    }

    private static bool ShouldSkipValidation(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;

        // Skip static files and common paths
        return pathValue.StartsWith("/css/") ||
               pathValue.StartsWith("/js/") ||
               pathValue.StartsWith("/lib/") ||
               pathValue.StartsWith("/images/") ||
               pathValue.StartsWith("/fonts/") ||
               pathValue.StartsWith("/favicon.ico") ||
               pathValue.StartsWith("/health") ||
               pathValue.StartsWith("/api/health") ||
               pathValue.StartsWith("/account/") ||
               pathValue.StartsWith("/home") ||
               pathValue.StartsWith("/admin/modules") || // Allow access to module management
               pathValue == "/" ||
               pathValue == string.Empty;
    }
}

/// <summary>
/// Extension method for adding license validation middleware
/// </summary>
public static class LicenseValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseLicenseValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LicenseValidationMiddleware>();
    }
}
