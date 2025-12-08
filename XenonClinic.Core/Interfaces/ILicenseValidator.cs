namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for validating module licenses
/// </summary>
public interface ILicenseValidator
{
    /// <summary>
    /// Validates if a module's license is valid and not expired
    /// </summary>
    /// <param name="moduleName">Name of the module to validate</param>
    /// <returns>License validation result</returns>
    LicenseValidationResult ValidateModuleLicense(string moduleName);

    /// <summary>
    /// Checks if a module is licensed (has a valid license key)
    /// </summary>
    /// <param name="moduleName">Name of the module</param>
    /// <returns>True if module has a license key</returns>
    bool IsModuleLicensed(string moduleName);

    /// <summary>
    /// Checks if a module's license is expired
    /// </summary>
    /// <param name="moduleName">Name of the module</param>
    /// <returns>True if license is expired</returns>
    bool IsLicenseExpired(string moduleName);

    /// <summary>
    /// Gets the number of days until a license expires
    /// </summary>
    /// <param name="moduleName">Name of the module</param>
    /// <returns>Days until expiry, null if perpetual or no license</returns>
    int? GetDaysUntilExpiry(string moduleName);

    /// <summary>
    /// Gets the maximum allowed users for a module
    /// </summary>
    /// <param name="moduleName">Name of the module</param>
    /// <returns>Maximum users, 0 for unlimited, null if not licensed</returns>
    int? GetMaxUsers(string moduleName);

    /// <summary>
    /// Validates if current user count is within license limits
    /// </summary>
    /// <param name="moduleName">Name of the module</param>
    /// <param name="currentUserCount">Current number of users</param>
    /// <returns>True if within limits</returns>
    bool ValidateUserLimit(string moduleName, int currentUserCount);
}

/// <summary>
/// Result of license validation
/// </summary>
public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string? LicenseKey { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public bool IsExpired { get; set; }
    public bool IsLicensed { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public string? ValidationMessage { get; set; }
    public LicenseValidationStatus Status { get; set; }
}

/// <summary>
/// License validation status codes
/// </summary>
public enum LicenseValidationStatus
{
    Valid,
    NotLicensed,
    Expired,
    InvalidKey,
    UserLimitExceeded
}
