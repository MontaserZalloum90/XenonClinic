using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfigurationResolverService _configResolver;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfigurationResolverService configResolver,
        ILogger<EmailService> logger)
    {
        _configResolver = configResolver;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(int companyId, string toEmail, string subject, string body, bool isHtml = true)
    {
        try
        {
            var config = await _configResolver.GetEmailConfigurationAsync(companyId);

            if (!config.IsConfigured)
            {
                _logger.LogWarning("Email not configured for company {CompanyId}", companyId);
                return false;
            }

            using var smtpClient = new SmtpClient(config.SmtpHost, config.SmtpPort)
            {
                EnableSsl = config.SmtpUseSsl,
                Credentials = new NetworkCredential(config.SmtpUsername, config.SmtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000 // 30 seconds
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(config.DefaultSenderEmail ?? config.SmtpUsername!, config.DefaultSenderName ?? "XenonClinic"),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {Email} for company {CompanyId}", toEmail, companyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email} for company {CompanyId}", toEmail, companyId);
            return false;
        }
    }

    public async Task<bool> SendAppointmentReminderEmailAsync(int companyId, string toEmail, string patientName, DateTime appointmentTime, string providerName)
    {
        var subject = "Appointment Reminder - XenonClinic";

        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #1F6FEB; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .appointment-details {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #1F6FEB; }}
                    .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Appointment Reminder</h1>
                    </div>
                    <div class='content'>
                        <p>Dear {patientName},</p>
                        <p>This is a friendly reminder about your upcoming appointment:</p>
                        <div class='appointment-details'>
                            <p><strong>Date:</strong> {appointmentTime:dddd, dd MMMM yyyy}</p>
                            <p><strong>Time:</strong> {appointmentTime:hh:mm tt}</p>
                            {(!string.IsNullOrEmpty(providerName) ? $"<p><strong>Provider:</strong> {providerName}</p>" : "")}
                        </div>
                        <p>Please arrive 10 minutes early to complete any necessary paperwork.</p>
                        <p>If you need to reschedule or cancel your appointment, please contact us as soon as possible.</p>
                        <p>We look forward to seeing you!</p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message from XenonClinic. Please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(companyId, toEmail, subject, body, isHtml: true);
    }

    public async Task<bool> IsConfiguredAsync(int companyId)
    {
        var config = await _configResolver.GetEmailConfigurationAsync(companyId);
        return config.IsConfigured;
    }
}
