namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for resolving configuration settings with company override support.
/// Company settings take precedence over tenant settings.
/// </summary>
public interface IConfigurationResolverService
{
    /// <summary>
    /// Gets the effective email configuration for a company (with fallback to tenant).
    /// </summary>
    Task<EmailConfiguration> GetEmailConfigurationAsync(int companyId);

    /// <summary>
    /// Gets the effective WhatsApp configuration for a company (with fallback to tenant).
    /// </summary>
    Task<WhatsAppConfiguration> GetWhatsAppConfigurationAsync(int companyId);
}

/// <summary>
/// Resolved email configuration.
/// </summary>
public class EmailConfiguration
{
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
    public string? DefaultSenderEmail { get; set; }
    public string? DefaultSenderName { get; set; }
    public bool IsConfigured => !string.IsNullOrEmpty(SmtpHost) && !string.IsNullOrEmpty(SmtpUsername);
}

/// <summary>
/// Resolved WhatsApp configuration.
/// </summary>
public class WhatsAppConfiguration
{
    public bool EnableWhatsApp { get; set; }
    public string? Provider { get; set; } // "Twilio" or "WhatsAppBusiness"
    public string? AccountSid { get; set; } // For Twilio
    public string? AuthToken { get; set; } // For Twilio
    public string? PhoneNumber { get; set; } // Sender phone number
    public string? BusinessApiToken { get; set; } // For WhatsApp Business API
    public string? BusinessPhoneNumberId { get; set; } // For WhatsApp Business API
    public bool IsConfigured => EnableWhatsApp && !string.IsNullOrEmpty(PhoneNumber) &&
                                (!string.IsNullOrEmpty(AuthToken) || !string.IsNullOrEmpty(BusinessApiToken));
}
