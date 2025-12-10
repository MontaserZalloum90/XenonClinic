using XenonClinic.Core.Attributes;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Stores company-specific configuration settings that can override tenant-level settings.
/// </summary>
public class CompanySettings
{
    public int Id { get; set; }
    public int CompanyId { get; set; }

    // Email settings override (null = use tenant settings)
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }

    /// <summary>
    /// SMTP password - should be encrypted at rest
    /// </summary>
    [SensitiveData(DataType = SensitiveDataType.Credential)]
    public string? SmtpPassword { get; set; }

    public bool? SmtpUseSsl { get; set; }
    public string? DefaultSenderEmail { get; set; }
    public string? DefaultSenderName { get; set; }

    // WhatsApp settings override (null = use tenant settings)
    public bool? EnableWhatsApp { get; set; }
    public string? WhatsAppProvider { get; set; } // "Twilio" or "WhatsAppBusiness"
    public string? WhatsAppAccountSid { get; set; } // For Twilio

    /// <summary>
    /// Twilio Auth Token - should be encrypted at rest
    /// </summary>
    [SensitiveData(DataType = SensitiveDataType.Token)]
    public string? WhatsAppAuthToken { get; set; }

    public string? WhatsAppPhoneNumber { get; set; } // Sender phone number (format: +1234567890)

    /// <summary>
    /// WhatsApp Business API Token - should be encrypted at rest
    /// </summary>
    [SensitiveData(DataType = SensitiveDataType.ApiKey)]
    public string? WhatsAppBusinessApiToken { get; set; }

    public string? WhatsAppBusinessPhoneNumberId { get; set; } // For WhatsApp Business API

    // Appointment reminder settings override
    public bool? SendAppointmentReminders { get; set; }
    public int? ReminderHoursBeforeAppointment { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation property
    public Company Company { get; set; } = null!;
}
