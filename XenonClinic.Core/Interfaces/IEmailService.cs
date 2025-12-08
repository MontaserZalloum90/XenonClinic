namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for sending emails using configured SMTP settings.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <param name="companyId">Company ID for configuration resolution</param>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML supported)</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<bool> SendEmailAsync(int companyId, string toEmail, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Sends an appointment reminder email.
    /// </summary>
    Task<bool> SendAppointmentReminderEmailAsync(int companyId, string toEmail, string patientName, DateTime appointmentTime, string providerName);

    /// <summary>
    /// Checks if email is configured for a company.
    /// </summary>
    Task<bool> IsConfiguredAsync(int companyId);
}
