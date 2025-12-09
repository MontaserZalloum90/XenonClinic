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
    /// Send an email message.
    /// </summary>
    Task SendAsync(EmailMessage message);

    /// <summary>
    /// Send an email using a template.
    /// </summary>
    Task SendTemplateAsync(string templateId, EmailMessage message, Dictionary<string, object> model);

    /// <summary>
    /// Send bulk emails.
    /// </summary>
    Task SendBulkAsync(IEnumerable<EmailMessage> messages);

    /// <summary>
    /// Queue an email for background sending.
    /// </summary>
    Task<string> QueueAsync(EmailMessage message);

    /// <summary>
    /// Sends an appointment reminder email.
    /// </summary>
    Task<bool> SendAppointmentReminderEmailAsync(int companyId, string toEmail, string patientName, DateTime appointmentTime, string providerName);

    /// <summary>
    /// Checks if email is configured for a company.
    /// </summary>
    Task<bool> IsConfiguredAsync(int companyId);
}

/// <summary>
/// Email message details.
/// </summary>
public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<EmailAttachment> Attachments { get; set; } = new();
    public string? ReplyTo { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
}

/// <summary>
/// Email attachment.
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public bool IsInline { get; set; }
    public string? ContentId { get; set; }
}

/// <summary>
/// Email priority levels.
/// </summary>
public enum EmailPriority
{
    Low = 0,
    Normal = 1,
    High = 2
}
