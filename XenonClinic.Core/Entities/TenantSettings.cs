namespace XenonClinic.Core.Entities;

/// <summary>
/// Stores tenant-specific configuration settings.
/// </summary>
public class TenantSettings
{
    public int Id { get; set; }
    public int TenantId { get; set; }

    // Regional settings
    public string DefaultLanguage { get; set; } = "en";
    public string DefaultCurrency { get; set; } = "AED";
    public string DefaultTimezone { get; set; } = "Arabian Standard Time";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "HH:mm";

    // Business settings
    public bool EnableMultiCurrency { get; set; } = false;
    public bool EnableMultiLanguage { get; set; } = true;
    public bool RequireApprovalForExpenses { get; set; } = true;
    public decimal ExpenseApprovalThreshold { get; set; } = 1000;

    // Appointment settings
    public int DefaultAppointmentDurationMinutes { get; set; } = 30;
    public bool EnableOnlineBooking { get; set; } = false;
    public bool SendAppointmentReminders { get; set; } = true;
    public int ReminderHoursBeforeAppointment { get; set; } = 24;

    // Invoice settings
    public string InvoicePrefix { get; set; } = "INV";
    public string QuotationPrefix { get; set; } = "QT";
    public string PurchaseOrderPrefix { get; set; } = "PO";
    public string SalePrefix { get; set; } = "SL";
    public int InvoiceStartNumber { get; set; } = 1;
    public decimal DefaultTaxRate { get; set; } = 5; // VAT 5%
    public bool ShowTaxOnInvoice { get; set; } = true;

    // Email settings
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
    public string? DefaultSenderEmail { get; set; }
    public string? DefaultSenderName { get; set; }

    // WhatsApp settings (using Twilio or WhatsApp Business API)
    public bool EnableWhatsApp { get; set; } = false;
    public string? WhatsAppProvider { get; set; } // "Twilio" or "WhatsAppBusiness"
    public string? WhatsAppAccountSid { get; set; } // For Twilio
    public string? WhatsAppAuthToken { get; set; } // For Twilio
    public string? WhatsAppPhoneNumber { get; set; } // Sender phone number (format: +1234567890)
    public string? WhatsAppBusinessApiToken { get; set; } // For WhatsApp Business API
    public string? WhatsAppBusinessPhoneNumberId { get; set; } // For WhatsApp Business API

    // Feature flags
    public bool EnableLabModule { get; set; } = true;
    public bool EnableInventoryModule { get; set; } = true;
    public bool EnableHRModule { get; set; } = true;
    public bool EnableFinanceModule { get; set; } = true;
    public bool EnableProcurementModule { get; set; } = true;
    public bool EnableSalesModule { get; set; } = true;
    public bool EnableAnalyticsModule { get; set; } = true;
    public bool EnablePatientPortal { get; set; } = false;

    // Audit settings
    public bool EnableAuditLogging { get; set; } = true;
    public int AuditLogRetentionDays { get; set; } = 365;

    // Backup settings
    public bool EnableAutoBackup { get; set; } = true;
    public string BackupSchedule { get; set; } = "0 2 * * *"; // Daily at 2 AM (cron format)
    public int BackupRetentionDays { get; set; } = 30;

    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // Navigation property
    public Tenant Tenant { get; set; } = null!;
}
