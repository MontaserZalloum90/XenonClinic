using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// SMS service implementation using configurable providers.
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly SmsSettings _settings;
    private readonly HttpClient _httpClient;

    public SmsService(
        ILogger<SmsService> logger,
        IOptions<SmsSettings> settings,
        HttpClient httpClient)
    {
        _logger = logger;
        _settings = settings.Value;
        _httpClient = httpClient;
    }

    public async Task SendAsync(string phoneNumber, string message)
    {
        var normalizedPhone = NormalizePhoneNumber(phoneNumber);

        try
        {
            // Provider-specific implementation
            switch (_settings.Provider.ToLowerInvariant())
            {
                case "twilio":
                    await SendViaTwilioAsync(normalizedPhone, message);
                    break;
                case "messagebird":
                    await SendViaMessageBirdAsync(normalizedPhone, message);
                    break;
                default:
                    _logger.LogWarning("SMS provider {Provider} not configured, logging message", _settings.Provider);
                    _logger.LogInformation("SMS to {Phone}: {Message}", normalizedPhone, message);
                    break;
            }

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}", MaskPhoneNumber(normalizedPhone));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while sending SMS to {PhoneNumber}", MaskPhoneNumber(normalizedPhone));
            throw new InvalidOperationException($"Failed to send SMS: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "SMS request timed out for {PhoneNumber}", MaskPhoneNumber(normalizedPhone));
            throw new InvalidOperationException("SMS request timed out", ex);
        }
    }

    public async Task SendTemplateAsync(string phoneNumber, string templateId, Dictionary<string, string> parameters)
    {
        var template = await GetTemplateAsync(templateId);
        var message = ApplyTemplate(template, parameters);
        await SendAsync(phoneNumber, message);
    }

    public async Task SendBulkAsync(IEnumerable<SmsMessage> messages)
    {
        var tasks = messages.Select(m =>
            string.IsNullOrEmpty(m.TemplateId)
                ? SendAsync(m.PhoneNumber, m.Content)
                : SendTemplateAsync(m.PhoneNumber, m.TemplateId, m.Parameters ?? new Dictionary<string, string>()));

        await Task.WhenAll(tasks);
    }

    public Task<SmsDeliveryStatus> GetDeliveryStatusAsync(string messageId)
    {
        // In production, query the SMS provider's API for delivery status
        return Task.FromResult(new SmsDeliveryStatus(
            messageId,
            SmsStatus.Delivered,
            DateTime.UtcNow,
            null
        ));
    }

    #region Private Methods

    private async Task SendViaTwilioAsync(string phoneNumber, string message)
    {
        using var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("To", phoneNumber),
            new KeyValuePair<string, string>("From", _settings.FromNumber),
            new KeyValuePair<string, string>("Body", message)
        });

        using var response = await _httpClient.PostAsync(
            $"https://api.twilio.com/2010-04-01/Accounts/{_settings.AccountSid}/Messages.json",
            content);

        response.EnsureSuccessStatusCode();
    }

    private async Task SendViaMessageBirdAsync(string phoneNumber, string message)
    {
        using var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("recipients", phoneNumber),
            new KeyValuePair<string, string>("originator", _settings.FromNumber),
            new KeyValuePair<string, string>("body", message)
        });

        using var response = await _httpClient.PostAsync("https://rest.messagebird.com/messages", content);
        response.EnsureSuccessStatusCode();
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove spaces, dashes, and ensure + prefix
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());
        return cleaned.StartsWith('+') ? cleaned : $"+{cleaned}";
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (phoneNumber.Length <= 4) return "****";
        return $"{phoneNumber[..3]}****{phoneNumber[^2..]}";
    }

    private Task<string> GetTemplateAsync(string templateId)
    {
        // In production, load from database or template store
        var templates = new Dictionary<string, string>
        {
            ["verification_code"] = "Your XenonClinic verification code is: {code}",
            ["appointment_reminder"] = "Reminder: You have an appointment at {time} with {provider}",
            ["prescription_ready"] = "Your prescription is ready for pickup at {location}"
        };

        return Task.FromResult(templates.GetValueOrDefault(templateId, ""));
    }

    private static string ApplyTemplate(string template, Dictionary<string, string> parameters)
    {
        var result = template;
        foreach (var param in parameters)
        {
            result = result.Replace($"{{{param.Key}}}", param.Value);
        }
        return result;
    }

    #endregion
}

/// <summary>
/// SMS configuration settings.
/// </summary>
public class SmsSettings
{
    public string Provider { get; set; } = "console";
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
