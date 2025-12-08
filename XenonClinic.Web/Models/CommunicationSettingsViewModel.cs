using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CommunicationSettingsViewModel
{
    public int? TenantId { get; set; }
    public int? CompanyId { get; set; }
    public string ConfigurationLevel { get; set; } = "Tenant"; // "Tenant" or "Company"

    // Email Settings
    [Display(Name = "SMTP Host")]
    public string? SmtpHost { get; set; }

    [Display(Name = "SMTP Port")]
    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535")]
    public int? SmtpPort { get; set; }

    [Display(Name = "SMTP Username")]
    public string? SmtpUsername { get; set; }

    [Display(Name = "SMTP Password")]
    [DataType(DataType.Password)]
    public string? SmtpPassword { get; set; }

    [Display(Name = "Use SSL")]
    public bool? SmtpUseSsl { get; set; }

    [Display(Name = "Default Sender Email")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? DefaultSenderEmail { get; set; }

    [Display(Name = "Default Sender Name")]
    public string? DefaultSenderName { get; set; }

    // WhatsApp Settings
    [Display(Name = "Enable WhatsApp")]
    public bool? EnableWhatsApp { get; set; }

    [Display(Name = "WhatsApp Provider")]
    public string? WhatsAppProvider { get; set; }

    [Display(Name = "Account SID (Twilio)")]
    public string? WhatsAppAccountSid { get; set; }

    [Display(Name = "Auth Token (Twilio)")]
    [DataType(DataType.Password)]
    public string? WhatsAppAuthToken { get; set; }

    [Display(Name = "WhatsApp Phone Number")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? WhatsAppPhoneNumber { get; set; }

    [Display(Name = "Business API Token (WhatsApp Business)")]
    [DataType(DataType.Password)]
    public string? WhatsAppBusinessApiToken { get; set; }

    [Display(Name = "Business Phone Number ID (WhatsApp Business)")]
    public string? WhatsAppBusinessPhoneNumberId { get; set; }

    // Reminder Settings
    [Display(Name = "Send Appointment Reminders")]
    public bool? SendAppointmentReminders { get; set; }

    [Display(Name = "Reminder Hours Before Appointment")]
    [Range(1, 168, ErrorMessage = "Hours must be between 1 and 168 (1 week)")]
    public int? ReminderHoursBeforeAppointment { get; set; }
}
