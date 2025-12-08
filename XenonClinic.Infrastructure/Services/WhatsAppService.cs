using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly IConfigurationResolverService _configResolver;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public WhatsAppService(
        IConfigurationResolverService configResolver,
        ILogger<WhatsAppService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configResolver = configResolver;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> SendMessageAsync(int companyId, string toPhoneNumber, string message)
    {
        try
        {
            var config = await _configResolver.GetWhatsAppConfigurationAsync(companyId);

            if (!config.IsConfigured)
            {
                _logger.LogWarning("WhatsApp not configured for company {CompanyId}", companyId);
                return false;
            }

            if (config.Provider == "Twilio")
            {
                return await SendViaTwilioAsync(config, toPhoneNumber, message);
            }
            else if (config.Provider == "WhatsAppBusiness")
            {
                return await SendViaWhatsAppBusinessApiAsync(config, toPhoneNumber, message);
            }
            else
            {
                _logger.LogError("Unknown WhatsApp provider: {Provider}", config.Provider);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message to {PhoneNumber} for company {CompanyId}",
                toPhoneNumber, companyId);
            return false;
        }
    }

    public async Task<bool> SendAppointmentReminderAsync(int companyId, string toPhoneNumber, string patientName, DateTime appointmentTime, string providerName)
    {
        var message = $"Hello {patientName},\n\n" +
                     $"This is a reminder about your appointment on {appointmentTime:dd MMM yyyy} at {appointmentTime:hh:mm tt}";

        if (!string.IsNullOrEmpty(providerName))
        {
            message += $" with {providerName}";
        }

        message += ".\n\nPlease arrive 10 minutes early. If you need to reschedule, please contact us.\n\n" +
                  "Thank you!";

        return await SendMessageAsync(companyId, toPhoneNumber, message);
    }

    public async Task<bool> IsConfiguredAsync(int companyId)
    {
        var config = await _configResolver.GetWhatsAppConfigurationAsync(companyId);
        return config.IsConfigured;
    }

    private async Task<bool> SendViaTwilioAsync(WhatsAppConfiguration config, string toPhoneNumber, string message)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            // Twilio API endpoint
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{config.AccountSid}/Messages.json";

            // Prepare authentication
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.AccountSid}:{config.AuthToken}"));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {authToken}");

            // Prepare message data
            var formData = new Dictionary<string, string>
            {
                { "From", $"whatsapp:{config.PhoneNumber}" },
                { "To", $"whatsapp:{toPhoneNumber}" },
                { "Body", message }
            };

            var content = new FormUrlEncodedContent(formData);

            // Send request
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WhatsApp message sent successfully via Twilio to {PhoneNumber}", toPhoneNumber);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Twilio API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message via Twilio");
            return false;
        }
    }

    private async Task<bool> SendViaWhatsAppBusinessApiAsync(WhatsAppConfiguration config, string toPhoneNumber, string message)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            // WhatsApp Business API endpoint
            var url = $"https://graph.facebook.com/v17.0/{config.BusinessPhoneNumberId}/messages";

            // Prepare authentication
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.BusinessApiToken}");

            // Prepare message data
            var messageData = new
            {
                messaging_product = "whatsapp",
                to = toPhoneNumber,
                type = "text",
                text = new { body = message }
            };

            var jsonContent = JsonSerializer.Serialize(messageData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send request
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WhatsApp message sent successfully via WhatsApp Business API to {PhoneNumber}", toPhoneNumber);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("WhatsApp Business API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message via WhatsApp Business API");
            return false;
        }
    }
}
