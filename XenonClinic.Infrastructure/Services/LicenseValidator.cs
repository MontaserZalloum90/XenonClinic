using Microsoft.Extensions.Configuration;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for validating module licenses
/// </summary>
public class LicenseValidator : ILicenseValidator
{
    private readonly IConfiguration _configuration;

    public LicenseValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LicenseValidationResult ValidateModuleLicense(string moduleName)
    {
        var result = new LicenseValidationResult
        {
            ModuleName = moduleName
        };

        // Get license configuration
        var licenseConfig = _configuration.GetSection($"Modules:Licenses:{moduleName}");
        var licenseKey = licenseConfig.GetValue<string>("LicenseKey");
        var expiryDateStr = licenseConfig.GetValue<string>("ExpiryDate");
        var maxUsers = licenseConfig.GetValue<int?>("MaxUsers");

        result.LicenseKey = licenseKey;
        result.MaxUsers = maxUsers ?? 0;

        // Check if module is licensed
        if (string.IsNullOrEmpty(licenseKey))
        {
            result.IsValid = false;
            result.IsLicensed = false;
            result.Status = LicenseValidationStatus.NotLicensed;
            result.ValidationMessage = $"Module '{moduleName}' does not have a license key configured.";
            return result;
        }

        result.IsLicensed = true;

        // Parse expiry date
        DateTime? expiryDate = null;
        if (!string.IsNullOrEmpty(expiryDateStr) && DateTime.TryParse(expiryDateStr, out var parsedDate))
        {
            expiryDate = parsedDate;
            result.ExpiryDate = expiryDate;
        }

        // Check if license is expired
        if (expiryDate.HasValue)
        {
            var daysUntilExpiry = (expiryDate.Value.Date - DateTime.UtcNow.Date).Days;
            result.DaysUntilExpiry = daysUntilExpiry;

            if (expiryDate.Value.Date < DateTime.UtcNow.Date)
            {
                result.IsValid = false;
                result.IsExpired = true;
                result.Status = LicenseValidationStatus.Expired;
                result.ValidationMessage = $"Module '{moduleName}' license expired on {expiryDate.Value:yyyy-MM-dd}.";
                return result;
            }
        }

        // License is valid
        result.IsValid = true;
        result.IsExpired = false;
        result.Status = LicenseValidationStatus.Valid;
        result.ValidationMessage = expiryDate.HasValue
            ? $"Module '{moduleName}' license is valid until {expiryDate.Value:yyyy-MM-dd}."
            : $"Module '{moduleName}' has a perpetual license.";

        return result;
    }

    public bool IsModuleLicensed(string moduleName)
    {
        var licenseKey = _configuration.GetValue<string>($"Modules:Licenses:{moduleName}:LicenseKey");
        return !string.IsNullOrEmpty(licenseKey);
    }

    public bool IsLicenseExpired(string moduleName)
    {
        var expiryDateStr = _configuration.GetValue<string>($"Modules:Licenses:{moduleName}:ExpiryDate");

        if (string.IsNullOrEmpty(expiryDateStr))
        {
            return false; // Perpetual license
        }

        if (DateTime.TryParse(expiryDateStr, out var expiryDate))
        {
            return expiryDate.Date < DateTime.UtcNow.Date;
        }

        return false;
    }

    public int? GetDaysUntilExpiry(string moduleName)
    {
        var expiryDateStr = _configuration.GetValue<string>($"Modules:Licenses:{moduleName}:ExpiryDate");

        if (string.IsNullOrEmpty(expiryDateStr))
        {
            return null; // Perpetual license
        }

        if (DateTime.TryParse(expiryDateStr, out var expiryDate))
        {
            return (expiryDate.Date - DateTime.UtcNow.Date).Days;
        }

        return null;
    }

    public int? GetMaxUsers(string moduleName)
    {
        if (!IsModuleLicensed(moduleName))
        {
            return null;
        }

        var maxUsers = _configuration.GetValue<int?>($"Modules:Licenses:{moduleName}:MaxUsers");
        return maxUsers ?? 0; // 0 means unlimited
    }

    public bool ValidateUserLimit(string moduleName, int currentUserCount)
    {
        var maxUsers = GetMaxUsers(moduleName);

        if (!maxUsers.HasValue)
        {
            return false; // Not licensed
        }

        if (maxUsers.Value == 0)
        {
            return true; // Unlimited users
        }

        return currentUserCount <= maxUsers.Value;
    }
}
