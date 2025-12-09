using System.Globalization;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Localization service for internationalization (i18n).
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Get the current culture.
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Get a localized string by key.
    /// </summary>
    string GetString(string key);

    /// <summary>
    /// Get a localized string by key with formatting arguments.
    /// </summary>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Get a localized string for a specific culture.
    /// </summary>
    string GetString(string key, CultureInfo culture);

    /// <summary>
    /// Get all strings for the current culture.
    /// </summary>
    IReadOnlyDictionary<string, string> GetAllStrings();

    /// <summary>
    /// Get all strings for a specific culture.
    /// </summary>
    IReadOnlyDictionary<string, string> GetAllStrings(CultureInfo culture);

    /// <summary>
    /// Get supported cultures.
    /// </summary>
    IEnumerable<CultureInfo> GetSupportedCultures();

    /// <summary>
    /// Set the current culture.
    /// </summary>
    void SetCulture(CultureInfo culture);

    /// <summary>
    /// Set the current culture by name.
    /// </summary>
    void SetCulture(string cultureName);

    /// <summary>
    /// Format a date according to current culture.
    /// </summary>
    string FormatDate(DateTime date, string? format = null);

    /// <summary>
    /// Format a number according to current culture.
    /// </summary>
    string FormatNumber(decimal number, int? decimals = null);

    /// <summary>
    /// Format currency according to current culture.
    /// </summary>
    string FormatCurrency(decimal amount, string? currencyCode = null);

    /// <summary>
    /// Check if a culture is supported.
    /// </summary>
    bool IsCultureSupported(string cultureName);
}

/// <summary>
/// Localization options.
/// </summary>
public class LocalizationOptions
{
    public string DefaultCulture { get; set; } = "en-US";
    public List<string> SupportedCultures { get; set; } = new() { "en-US", "ar-AE", "fr-FR" };
    public string ResourcesPath { get; set; } = "Resources";
    public bool FallbackToDefaultCulture { get; set; } = true;
}
