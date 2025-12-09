using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Localization service for internationalization (i18n).
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;
    private readonly LocalizationOptions _options;
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _resources = new();
    private CultureInfo _currentCulture;

    public LocalizationService(
        ILogger<LocalizationService> logger,
        IOptions<LocalizationOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _currentCulture = new CultureInfo(_options.DefaultCulture);

        LoadResources();
    }

    public CultureInfo CurrentCulture => _currentCulture;

    public string GetString(string key)
    {
        return GetString(key, _currentCulture);
    }

    public string GetString(string key, params object[] args)
    {
        var template = GetString(key);
        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    public string GetString(string key, CultureInfo culture)
    {
        var cultureName = culture.Name;

        // Try exact culture match
        if (_resources.TryGetValue(cultureName, out var exactResources) &&
            exactResources.TryGetValue(key, out var exactValue))
        {
            return exactValue;
        }

        // Try parent culture (e.g., "en" for "en-US")
        if (culture.Parent != null && culture.Parent != CultureInfo.InvariantCulture)
        {
            if (_resources.TryGetValue(culture.Parent.Name, out var parentResources) &&
                parentResources.TryGetValue(key, out var parentValue))
            {
                return parentValue;
            }
        }

        // Fallback to default culture
        if (_options.FallbackToDefaultCulture)
        {
            if (_resources.TryGetValue(_options.DefaultCulture, out var defaultResources) &&
                defaultResources.TryGetValue(key, out var defaultValue))
            {
                return defaultValue;
            }
        }

        // Return key as fallback
        _logger.LogWarning("Missing translation for key: {Key}, culture: {Culture}", key, cultureName);
        return key;
    }

    public IReadOnlyDictionary<string, string> GetAllStrings()
    {
        return GetAllStrings(_currentCulture);
    }

    public IReadOnlyDictionary<string, string> GetAllStrings(CultureInfo culture)
    {
        if (_resources.TryGetValue(culture.Name, out var resources))
        {
            return resources;
        }

        return new Dictionary<string, string>();
    }

    public IEnumerable<CultureInfo> GetSupportedCultures()
    {
        return _options.SupportedCultures.Select(c => new CultureInfo(c));
    }

    public void SetCulture(CultureInfo culture)
    {
        if (!IsCultureSupported(culture.Name))
        {
            _logger.LogWarning("Culture {Culture} is not supported, using default", culture.Name);
            culture = new CultureInfo(_options.DefaultCulture);
        }

        _currentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        _logger.LogInformation("Culture set to {Culture}", culture.Name);
    }

    public void SetCulture(string cultureName)
    {
        SetCulture(new CultureInfo(cultureName));
    }

    public string FormatDate(DateTime date, string? format = null)
    {
        return date.ToString(format ?? "d", _currentCulture);
    }

    public string FormatNumber(decimal number, int? decimals = null)
    {
        if (decimals.HasValue)
        {
            return number.ToString($"N{decimals}", _currentCulture);
        }
        return number.ToString("N", _currentCulture);
    }

    public string FormatCurrency(decimal amount, string? currencyCode = null)
    {
        if (!string.IsNullOrEmpty(currencyCode))
        {
            var region = new RegionInfo(_currentCulture.Name);
            return $"{currencyCode} {amount.ToString("N2", _currentCulture)}";
        }
        return amount.ToString("C", _currentCulture);
    }

    public bool IsCultureSupported(string cultureName)
    {
        return _options.SupportedCultures.Contains(cultureName, StringComparer.OrdinalIgnoreCase);
    }

    #region Private Methods

    private void LoadResources()
    {
        // English (US) - Default
        _resources["en-US"] = new Dictionary<string, string>
        {
            // Common
            ["common.save"] = "Save",
            ["common.cancel"] = "Cancel",
            ["common.delete"] = "Delete",
            ["common.edit"] = "Edit",
            ["common.add"] = "Add",
            ["common.search"] = "Search",
            ["common.loading"] = "Loading...",
            ["common.noData"] = "No data available",
            ["common.confirm"] = "Confirm",
            ["common.yes"] = "Yes",
            ["common.no"] = "No",

            // Authentication
            ["auth.login"] = "Login",
            ["auth.logout"] = "Logout",
            ["auth.email"] = "Email",
            ["auth.password"] = "Password",
            ["auth.forgotPassword"] = "Forgot Password?",
            ["auth.resetPassword"] = "Reset Password",
            ["auth.invalidCredentials"] = "Invalid email or password",

            // Patients
            ["patient.title"] = "Patients",
            ["patient.add"] = "Add Patient",
            ["patient.name"] = "Patient Name",
            ["patient.dateOfBirth"] = "Date of Birth",
            ["patient.gender"] = "Gender",
            ["patient.phone"] = "Phone Number",
            ["patient.address"] = "Address",

            // Appointments
            ["appointment.title"] = "Appointments",
            ["appointment.schedule"] = "Schedule Appointment",
            ["appointment.date"] = "Date",
            ["appointment.time"] = "Time",
            ["appointment.provider"] = "Provider",
            ["appointment.status"] = "Status",
            ["appointment.confirmed"] = "Confirmed",
            ["appointment.pending"] = "Pending",
            ["appointment.cancelled"] = "Cancelled",

            // Messages
            ["message.saveSuccess"] = "Saved successfully",
            ["message.deleteSuccess"] = "Deleted successfully",
            ["message.error"] = "An error occurred",
            ["message.confirmDelete"] = "Are you sure you want to delete this item?"
        };

        // Arabic (UAE)
        _resources["ar-AE"] = new Dictionary<string, string>
        {
            // Common
            ["common.save"] = "حفظ",
            ["common.cancel"] = "إلغاء",
            ["common.delete"] = "حذف",
            ["common.edit"] = "تعديل",
            ["common.add"] = "إضافة",
            ["common.search"] = "بحث",
            ["common.loading"] = "جاري التحميل...",
            ["common.noData"] = "لا توجد بيانات",
            ["common.confirm"] = "تأكيد",
            ["common.yes"] = "نعم",
            ["common.no"] = "لا",

            // Authentication
            ["auth.login"] = "تسجيل الدخول",
            ["auth.logout"] = "تسجيل الخروج",
            ["auth.email"] = "البريد الإلكتروني",
            ["auth.password"] = "كلمة المرور",
            ["auth.forgotPassword"] = "نسيت كلمة المرور؟",
            ["auth.resetPassword"] = "إعادة تعيين كلمة المرور",
            ["auth.invalidCredentials"] = "البريد الإلكتروني أو كلمة المرور غير صحيحة",

            // Patients
            ["patient.title"] = "المرضى",
            ["patient.add"] = "إضافة مريض",
            ["patient.name"] = "اسم المريض",
            ["patient.dateOfBirth"] = "تاريخ الميلاد",
            ["patient.gender"] = "الجنس",
            ["patient.phone"] = "رقم الهاتف",
            ["patient.address"] = "العنوان",

            // Appointments
            ["appointment.title"] = "المواعيد",
            ["appointment.schedule"] = "حجز موعد",
            ["appointment.date"] = "التاريخ",
            ["appointment.time"] = "الوقت",
            ["appointment.provider"] = "مقدم الخدمة",
            ["appointment.status"] = "الحالة",
            ["appointment.confirmed"] = "مؤكد",
            ["appointment.pending"] = "معلق",
            ["appointment.cancelled"] = "ملغى",

            // Messages
            ["message.saveSuccess"] = "تم الحفظ بنجاح",
            ["message.deleteSuccess"] = "تم الحذف بنجاح",
            ["message.error"] = "حدث خطأ",
            ["message.confirmDelete"] = "هل أنت متأكد من حذف هذا العنصر؟"
        };

        // French
        _resources["fr-FR"] = new Dictionary<string, string>
        {
            // Common
            ["common.save"] = "Enregistrer",
            ["common.cancel"] = "Annuler",
            ["common.delete"] = "Supprimer",
            ["common.edit"] = "Modifier",
            ["common.add"] = "Ajouter",
            ["common.search"] = "Rechercher",
            ["common.loading"] = "Chargement...",
            ["common.noData"] = "Aucune donnée disponible",
            ["common.confirm"] = "Confirmer",
            ["common.yes"] = "Oui",
            ["common.no"] = "Non",

            // Authentication
            ["auth.login"] = "Connexion",
            ["auth.logout"] = "Déconnexion",
            ["auth.email"] = "Email",
            ["auth.password"] = "Mot de passe",
            ["auth.forgotPassword"] = "Mot de passe oublié?",
            ["auth.resetPassword"] = "Réinitialiser le mot de passe",
            ["auth.invalidCredentials"] = "Email ou mot de passe invalide",

            // Patients
            ["patient.title"] = "Patients",
            ["patient.add"] = "Ajouter un patient",
            ["patient.name"] = "Nom du patient",
            ["patient.dateOfBirth"] = "Date de naissance",
            ["patient.gender"] = "Genre",
            ["patient.phone"] = "Téléphone",
            ["patient.address"] = "Adresse",

            // Appointments
            ["appointment.title"] = "Rendez-vous",
            ["appointment.schedule"] = "Planifier un rendez-vous",
            ["appointment.date"] = "Date",
            ["appointment.time"] = "Heure",
            ["appointment.provider"] = "Praticien",
            ["appointment.status"] = "Statut",
            ["appointment.confirmed"] = "Confirmé",
            ["appointment.pending"] = "En attente",
            ["appointment.cancelled"] = "Annulé",

            // Messages
            ["message.saveSuccess"] = "Enregistré avec succès",
            ["message.deleteSuccess"] = "Supprimé avec succès",
            ["message.error"] = "Une erreur s'est produite",
            ["message.confirmDelete"] = "Êtes-vous sûr de vouloir supprimer cet élément?"
        };

        _logger.LogInformation("Loaded {Count} language resources", _resources.Count);
    }

    #endregion
}
