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
            // Common Actions
            ["common.save"] = "Save",
            ["common.cancel"] = "Cancel",
            ["common.delete"] = "Delete",
            ["common.edit"] = "Edit",
            ["common.add"] = "Add",
            ["common.remove"] = "Remove",
            ["common.search"] = "Search",
            ["common.filter"] = "Filter",
            ["common.export"] = "Export",
            ["common.import"] = "Import",
            ["common.refresh"] = "Refresh",
            ["common.print"] = "Print",
            ["common.download"] = "Download",
            ["common.upload"] = "Upload",
            ["common.submit"] = "Submit",
            ["common.confirm"] = "Confirm",
            ["common.back"] = "Back",
            ["common.next"] = "Next",
            ["common.previous"] = "Previous",
            ["common.close"] = "Close",
            ["common.view"] = "View",
            ["common.viewDetails"] = "View Details",
            ["common.selectAll"] = "Select All",
            ["common.deselectAll"] = "Deselect All",
            ["common.reset"] = "Reset",
            ["common.clear"] = "Clear",
            ["common.apply"] = "Apply",
            ["common.loading"] = "Loading...",
            ["common.noData"] = "No data available",
            ["common.yes"] = "Yes",
            ["common.no"] = "No",
            ["common.create"] = "Create",

            // Authentication
            ["auth.login"] = "Login",
            ["auth.logout"] = "Logout",
            ["auth.email"] = "Email",
            ["auth.password"] = "Password",
            ["auth.username"] = "Username",
            ["auth.forgotPassword"] = "Forgot Password?",
            ["auth.resetPassword"] = "Reset Password",
            ["auth.rememberMe"] = "Remember me",
            ["auth.signingIn"] = "Signing in...",
            ["auth.invalidCredentials"] = "Invalid email or password",
            ["auth.noAccount"] = "Don't have an account?",
            ["auth.registerHere"] = "Register here",

            // Navigation
            ["nav.dashboard"] = "Dashboard",
            ["nav.appointments"] = "Appointments",
            ["nav.patients"] = "Patients",
            ["nav.laboratory"] = "Laboratory",
            ["nav.pharmacy"] = "Pharmacy",
            ["nav.radiology"] = "Radiology",
            ["nav.audiology"] = "Audiology",
            ["nav.hr"] = "HR",
            ["nav.financial"] = "Financial",
            ["nav.inventory"] = "Inventory",
            ["nav.marketing"] = "Marketing",
            ["nav.admin"] = "Admin",
            ["nav.settings"] = "Settings",
            ["nav.reports"] = "Reports",

            // Entities
            ["entity.patient.singular"] = "Patient",
            ["entity.patient.plural"] = "Patients",
            ["entity.appointment.singular"] = "Appointment",
            ["entity.appointment.plural"] = "Appointments",
            ["entity.encounter.singular"] = "Encounter",
            ["entity.encounter.plural"] = "Encounters",
            ["entity.invoice.singular"] = "Invoice",
            ["entity.invoice.plural"] = "Invoices",
            ["entity.prescription.singular"] = "Prescription",
            ["entity.prescription.plural"] = "Prescriptions",
            ["entity.labTest.singular"] = "Lab Test",
            ["entity.labTest.plural"] = "Lab Tests",
            ["entity.employee.singular"] = "Employee",
            ["entity.employee.plural"] = "Employees",
            ["entity.product.singular"] = "Product",
            ["entity.product.plural"] = "Products",
            ["entity.order.singular"] = "Order",
            ["entity.order.plural"] = "Orders",

            // Patients
            ["patient.title"] = "Patients",
            ["patient.add"] = "Add Patient",
            ["patient.name"] = "Patient Name",
            ["patient.dateOfBirth"] = "Date of Birth",
            ["patient.gender"] = "Gender",
            ["patient.phone"] = "Phone Number",
            ["patient.address"] = "Address",
            ["patient.emiratesId"] = "Emirates ID",
            ["patient.nationalId"] = "National ID",
            ["patient.passportNumber"] = "Passport Number",
            ["patient.nationality"] = "Nationality",

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
            ["appointment.checkedIn"] = "Checked In",
            ["appointment.noShow"] = "No Show",

            // Form Fields
            ["field.firstName"] = "First Name",
            ["field.lastName"] = "Last Name",
            ["field.fullName"] = "Full Name",
            ["field.email"] = "Email",
            ["field.phone"] = "Phone",
            ["field.address"] = "Address",
            ["field.date"] = "Date",
            ["field.time"] = "Time",
            ["field.status"] = "Status",
            ["field.notes"] = "Notes",
            ["field.description"] = "Description",
            ["field.amount"] = "Amount",
            ["field.quantity"] = "Quantity",
            ["field.price"] = "Price",
            ["field.total"] = "Total",
            ["field.discount"] = "Discount",
            ["field.tax"] = "Tax",
            ["field.age"] = "Age",
            ["field.contact"] = "Contact",
            ["field.actions"] = "Actions",

            // Status Values
            ["status.active"] = "Active",
            ["status.inactive"] = "Inactive",
            ["status.pending"] = "Pending",
            ["status.completed"] = "Completed",
            ["status.cancelled"] = "Cancelled",
            ["status.scheduled"] = "Scheduled",
            ["status.inProgress"] = "In Progress",
            ["status.draft"] = "Draft",
            ["status.approved"] = "Approved",
            ["status.rejected"] = "Rejected",
            ["status.paid"] = "Paid",
            ["status.unpaid"] = "Unpaid",
            ["status.overdue"] = "Overdue",

            // Messages
            ["message.saveSuccess"] = "Saved successfully",
            ["message.createSuccess"] = "Created successfully",
            ["message.updateSuccess"] = "Updated successfully",
            ["message.deleteSuccess"] = "Deleted successfully",
            ["message.exportSuccess"] = "Exported successfully",
            ["message.error"] = "An error occurred",
            ["message.notFound"] = "Item not found",
            ["message.unauthorized"] = "Unauthorized access",
            ["message.validationError"] = "Please check the form for errors",
            ["message.networkError"] = "Network error. Please try again.",
            ["message.loadingError"] = "Error loading data",
            ["message.confirmDelete"] = "Are you sure you want to delete this item?",
            ["message.unsavedChanges"] = "You have unsaved changes. Are you sure you want to leave?",
            ["message.noResults"] = "No results found",
            ["message.searchTip"] = "Try adjusting your search criteria"
        };

        // Arabic (UAE)
        _resources["ar-AE"] = new Dictionary<string, string>
        {
            // Common Actions
            ["common.save"] = "حفظ",
            ["common.cancel"] = "إلغاء",
            ["common.delete"] = "حذف",
            ["common.edit"] = "تعديل",
            ["common.add"] = "إضافة",
            ["common.remove"] = "إزالة",
            ["common.search"] = "بحث",
            ["common.filter"] = "تصفية",
            ["common.export"] = "تصدير",
            ["common.import"] = "استيراد",
            ["common.refresh"] = "تحديث",
            ["common.print"] = "طباعة",
            ["common.download"] = "تحميل",
            ["common.upload"] = "رفع",
            ["common.submit"] = "إرسال",
            ["common.confirm"] = "تأكيد",
            ["common.back"] = "رجوع",
            ["common.next"] = "التالي",
            ["common.previous"] = "السابق",
            ["common.close"] = "إغلاق",
            ["common.view"] = "عرض",
            ["common.viewDetails"] = "عرض التفاصيل",
            ["common.selectAll"] = "تحديد الكل",
            ["common.deselectAll"] = "إلغاء تحديد الكل",
            ["common.reset"] = "إعادة تعيين",
            ["common.clear"] = "مسح",
            ["common.apply"] = "تطبيق",
            ["common.loading"] = "جاري التحميل...",
            ["common.noData"] = "لا توجد بيانات",
            ["common.yes"] = "نعم",
            ["common.no"] = "لا",
            ["common.create"] = "إنشاء",

            // Authentication
            ["auth.login"] = "تسجيل الدخول",
            ["auth.logout"] = "تسجيل الخروج",
            ["auth.email"] = "البريد الإلكتروني",
            ["auth.password"] = "كلمة المرور",
            ["auth.username"] = "اسم المستخدم",
            ["auth.forgotPassword"] = "نسيت كلمة المرور؟",
            ["auth.resetPassword"] = "إعادة تعيين كلمة المرور",
            ["auth.rememberMe"] = "تذكرني",
            ["auth.signingIn"] = "جاري تسجيل الدخول...",
            ["auth.invalidCredentials"] = "البريد الإلكتروني أو كلمة المرور غير صحيحة",
            ["auth.noAccount"] = "ليس لديك حساب؟",
            ["auth.registerHere"] = "سجل هنا",

            // Navigation
            ["nav.dashboard"] = "لوحة التحكم",
            ["nav.appointments"] = "المواعيد",
            ["nav.patients"] = "المرضى",
            ["nav.laboratory"] = "المختبر",
            ["nav.pharmacy"] = "الصيدلية",
            ["nav.radiology"] = "الأشعة",
            ["nav.audiology"] = "السمعيات",
            ["nav.hr"] = "الموارد البشرية",
            ["nav.financial"] = "المالية",
            ["nav.inventory"] = "المخزون",
            ["nav.marketing"] = "التسويق",
            ["nav.admin"] = "الإدارة",
            ["nav.settings"] = "الإعدادات",
            ["nav.reports"] = "التقارير",

            // Entities
            ["entity.patient.singular"] = "مريض",
            ["entity.patient.plural"] = "المرضى",
            ["entity.appointment.singular"] = "موعد",
            ["entity.appointment.plural"] = "المواعيد",
            ["entity.encounter.singular"] = "زيارة",
            ["entity.encounter.plural"] = "الزيارات",
            ["entity.invoice.singular"] = "فاتورة",
            ["entity.invoice.plural"] = "الفواتير",
            ["entity.prescription.singular"] = "وصفة طبية",
            ["entity.prescription.plural"] = "الوصفات الطبية",
            ["entity.labTest.singular"] = "فحص مخبري",
            ["entity.labTest.plural"] = "الفحوصات المخبرية",
            ["entity.employee.singular"] = "موظف",
            ["entity.employee.plural"] = "الموظفون",
            ["entity.product.singular"] = "منتج",
            ["entity.product.plural"] = "المنتجات",
            ["entity.order.singular"] = "طلب",
            ["entity.order.plural"] = "الطلبات",

            // Patients
            ["patient.title"] = "المرضى",
            ["patient.add"] = "إضافة مريض",
            ["patient.name"] = "اسم المريض",
            ["patient.dateOfBirth"] = "تاريخ الميلاد",
            ["patient.gender"] = "الجنس",
            ["patient.phone"] = "رقم الهاتف",
            ["patient.address"] = "العنوان",
            ["patient.emiratesId"] = "الهوية الإماراتية",
            ["patient.nationalId"] = "الهوية الوطنية",
            ["patient.passportNumber"] = "رقم جواز السفر",
            ["patient.nationality"] = "الجنسية",

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
            ["appointment.checkedIn"] = "وصل",
            ["appointment.noShow"] = "لم يحضر",

            // Form Fields
            ["field.firstName"] = "الاسم الأول",
            ["field.lastName"] = "اسم العائلة",
            ["field.fullName"] = "الاسم الكامل",
            ["field.email"] = "البريد الإلكتروني",
            ["field.phone"] = "الهاتف",
            ["field.address"] = "العنوان",
            ["field.date"] = "التاريخ",
            ["field.time"] = "الوقت",
            ["field.status"] = "الحالة",
            ["field.notes"] = "ملاحظات",
            ["field.description"] = "الوصف",
            ["field.amount"] = "المبلغ",
            ["field.quantity"] = "الكمية",
            ["field.price"] = "السعر",
            ["field.total"] = "الإجمالي",
            ["field.discount"] = "الخصم",
            ["field.tax"] = "الضريبة",
            ["field.age"] = "العمر",
            ["field.contact"] = "الاتصال",
            ["field.actions"] = "الإجراءات",

            // Status Values
            ["status.active"] = "نشط",
            ["status.inactive"] = "غير نشط",
            ["status.pending"] = "معلق",
            ["status.completed"] = "مكتمل",
            ["status.cancelled"] = "ملغى",
            ["status.scheduled"] = "مجدول",
            ["status.inProgress"] = "قيد التنفيذ",
            ["status.draft"] = "مسودة",
            ["status.approved"] = "موافق عليه",
            ["status.rejected"] = "مرفوض",
            ["status.paid"] = "مدفوع",
            ["status.unpaid"] = "غير مدفوع",
            ["status.overdue"] = "متأخر",

            // Messages
            ["message.saveSuccess"] = "تم الحفظ بنجاح",
            ["message.createSuccess"] = "تم الإنشاء بنجاح",
            ["message.updateSuccess"] = "تم التحديث بنجاح",
            ["message.deleteSuccess"] = "تم الحذف بنجاح",
            ["message.exportSuccess"] = "تم التصدير بنجاح",
            ["message.error"] = "حدث خطأ",
            ["message.notFound"] = "العنصر غير موجود",
            ["message.unauthorized"] = "غير مصرح بالدخول",
            ["message.validationError"] = "يرجى التحقق من الأخطاء في النموذج",
            ["message.networkError"] = "خطأ في الشبكة. يرجى المحاولة مرة أخرى.",
            ["message.loadingError"] = "خطأ في تحميل البيانات",
            ["message.confirmDelete"] = "هل أنت متأكد من حذف هذا العنصر؟",
            ["message.unsavedChanges"] = "لديك تغييرات غير محفوظة. هل أنت متأكد من المغادرة؟",
            ["message.noResults"] = "لم يتم العثور على نتائج",
            ["message.searchTip"] = "حاول تعديل معايير البحث"
        };

        // French
        _resources["fr-FR"] = new Dictionary<string, string>
        {
            // Common Actions
            ["common.save"] = "Enregistrer",
            ["common.cancel"] = "Annuler",
            ["common.delete"] = "Supprimer",
            ["common.edit"] = "Modifier",
            ["common.add"] = "Ajouter",
            ["common.remove"] = "Retirer",
            ["common.search"] = "Rechercher",
            ["common.filter"] = "Filtrer",
            ["common.export"] = "Exporter",
            ["common.import"] = "Importer",
            ["common.refresh"] = "Actualiser",
            ["common.print"] = "Imprimer",
            ["common.download"] = "Télécharger",
            ["common.upload"] = "Téléverser",
            ["common.submit"] = "Soumettre",
            ["common.confirm"] = "Confirmer",
            ["common.back"] = "Retour",
            ["common.next"] = "Suivant",
            ["common.previous"] = "Précédent",
            ["common.close"] = "Fermer",
            ["common.view"] = "Voir",
            ["common.viewDetails"] = "Voir les détails",
            ["common.selectAll"] = "Tout sélectionner",
            ["common.deselectAll"] = "Tout désélectionner",
            ["common.reset"] = "Réinitialiser",
            ["common.clear"] = "Effacer",
            ["common.apply"] = "Appliquer",
            ["common.loading"] = "Chargement...",
            ["common.noData"] = "Aucune donnée disponible",
            ["common.yes"] = "Oui",
            ["common.no"] = "Non",
            ["common.create"] = "Créer",

            // Authentication
            ["auth.login"] = "Connexion",
            ["auth.logout"] = "Déconnexion",
            ["auth.email"] = "Email",
            ["auth.password"] = "Mot de passe",
            ["auth.username"] = "Nom d'utilisateur",
            ["auth.forgotPassword"] = "Mot de passe oublié?",
            ["auth.resetPassword"] = "Réinitialiser le mot de passe",
            ["auth.rememberMe"] = "Se souvenir de moi",
            ["auth.signingIn"] = "Connexion en cours...",
            ["auth.invalidCredentials"] = "Email ou mot de passe invalide",
            ["auth.noAccount"] = "Pas de compte?",
            ["auth.registerHere"] = "Inscrivez-vous ici",

            // Navigation
            ["nav.dashboard"] = "Tableau de bord",
            ["nav.appointments"] = "Rendez-vous",
            ["nav.patients"] = "Patients",
            ["nav.laboratory"] = "Laboratoire",
            ["nav.pharmacy"] = "Pharmacie",
            ["nav.radiology"] = "Radiologie",
            ["nav.audiology"] = "Audiologie",
            ["nav.hr"] = "RH",
            ["nav.financial"] = "Finances",
            ["nav.inventory"] = "Inventaire",
            ["nav.marketing"] = "Marketing",
            ["nav.admin"] = "Admin",
            ["nav.settings"] = "Paramètres",
            ["nav.reports"] = "Rapports",

            // Entities
            ["entity.patient.singular"] = "Patient",
            ["entity.patient.plural"] = "Patients",
            ["entity.appointment.singular"] = "Rendez-vous",
            ["entity.appointment.plural"] = "Rendez-vous",
            ["entity.encounter.singular"] = "Consultation",
            ["entity.encounter.plural"] = "Consultations",
            ["entity.invoice.singular"] = "Facture",
            ["entity.invoice.plural"] = "Factures",
            ["entity.prescription.singular"] = "Ordonnance",
            ["entity.prescription.plural"] = "Ordonnances",
            ["entity.labTest.singular"] = "Analyse",
            ["entity.labTest.plural"] = "Analyses",
            ["entity.employee.singular"] = "Employé",
            ["entity.employee.plural"] = "Employés",
            ["entity.product.singular"] = "Produit",
            ["entity.product.plural"] = "Produits",
            ["entity.order.singular"] = "Commande",
            ["entity.order.plural"] = "Commandes",

            // Patients
            ["patient.title"] = "Patients",
            ["patient.add"] = "Ajouter un patient",
            ["patient.name"] = "Nom du patient",
            ["patient.dateOfBirth"] = "Date de naissance",
            ["patient.gender"] = "Genre",
            ["patient.phone"] = "Téléphone",
            ["patient.address"] = "Adresse",
            ["patient.emiratesId"] = "ID Emirates",
            ["patient.nationalId"] = "ID National",
            ["patient.passportNumber"] = "Numéro de passeport",
            ["patient.nationality"] = "Nationalité",

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
            ["appointment.checkedIn"] = "Enregistré",
            ["appointment.noShow"] = "Absent",

            // Form Fields
            ["field.firstName"] = "Prénom",
            ["field.lastName"] = "Nom",
            ["field.fullName"] = "Nom complet",
            ["field.email"] = "Email",
            ["field.phone"] = "Téléphone",
            ["field.address"] = "Adresse",
            ["field.date"] = "Date",
            ["field.time"] = "Heure",
            ["field.status"] = "Statut",
            ["field.notes"] = "Notes",
            ["field.description"] = "Description",
            ["field.amount"] = "Montant",
            ["field.quantity"] = "Quantité",
            ["field.price"] = "Prix",
            ["field.total"] = "Total",
            ["field.discount"] = "Remise",
            ["field.tax"] = "Taxe",
            ["field.age"] = "Âge",
            ["field.contact"] = "Contact",
            ["field.actions"] = "Actions",

            // Status Values
            ["status.active"] = "Actif",
            ["status.inactive"] = "Inactif",
            ["status.pending"] = "En attente",
            ["status.completed"] = "Terminé",
            ["status.cancelled"] = "Annulé",
            ["status.scheduled"] = "Planifié",
            ["status.inProgress"] = "En cours",
            ["status.draft"] = "Brouillon",
            ["status.approved"] = "Approuvé",
            ["status.rejected"] = "Rejeté",
            ["status.paid"] = "Payé",
            ["status.unpaid"] = "Impayé",
            ["status.overdue"] = "En retard",

            // Messages
            ["message.saveSuccess"] = "Enregistré avec succès",
            ["message.createSuccess"] = "Créé avec succès",
            ["message.updateSuccess"] = "Mis à jour avec succès",
            ["message.deleteSuccess"] = "Supprimé avec succès",
            ["message.exportSuccess"] = "Exporté avec succès",
            ["message.error"] = "Une erreur s'est produite",
            ["message.notFound"] = "Élément non trouvé",
            ["message.unauthorized"] = "Accès non autorisé",
            ["message.validationError"] = "Veuillez vérifier les erreurs du formulaire",
            ["message.networkError"] = "Erreur réseau. Veuillez réessayer.",
            ["message.loadingError"] = "Erreur lors du chargement des données",
            ["message.confirmDelete"] = "Êtes-vous sûr de vouloir supprimer cet élément?",
            ["message.unsavedChanges"] = "Vous avez des modifications non enregistrées. Êtes-vous sûr de vouloir quitter?",
            ["message.noResults"] = "Aucun résultat trouvé",
            ["message.searchTip"] = "Essayez d'ajuster vos critères de recherche"
        };

        _logger.LogInformation("Loaded {Count} language resources", _resources.Count);
    }

    #endregion
}
