namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for sending WhatsApp messages using configured provider (Twilio or WhatsApp Business API).
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Sends a WhatsApp message to a recipient.
    /// </summary>
    /// <param name="companyId">Company ID for configuration resolution</param>
    /// <param name="toPhoneNumber">Recipient phone number (format: +1234567890)</param>
    /// <param name="message">Message content</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<bool> SendMessageAsync(int companyId, string toPhoneNumber, string message);

    /// <summary>
    /// Sends an appointment reminder via WhatsApp.
    /// </summary>
    Task<bool> SendAppointmentReminderAsync(int companyId, string toPhoneNumber, string patientName, DateTime appointmentTime, string providerName);

    /// <summary>
    /// Checks if WhatsApp is configured and enabled for a company.
    /// </summary>
    Task<bool> IsConfiguredAsync(int companyId);
}
