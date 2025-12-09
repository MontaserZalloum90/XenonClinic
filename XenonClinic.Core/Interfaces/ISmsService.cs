namespace XenonClinic.Core.Interfaces;

/// <summary>
/// SMS messaging service abstraction.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send an SMS message.
    /// </summary>
    Task SendAsync(string phoneNumber, string message);

    /// <summary>
    /// Send an SMS message using a template.
    /// </summary>
    Task SendTemplateAsync(string phoneNumber, string templateId, Dictionary<string, string> parameters);

    /// <summary>
    /// Send bulk SMS messages.
    /// </summary>
    Task SendBulkAsync(IEnumerable<SmsMessage> messages);

    /// <summary>
    /// Get delivery status of a sent message.
    /// </summary>
    Task<SmsDeliveryStatus> GetDeliveryStatusAsync(string messageId);
}

/// <summary>
/// SMS message details.
/// </summary>
public record SmsMessage(
    string PhoneNumber,
    string Content,
    string? TemplateId = null,
    Dictionary<string, string>? Parameters = null
);

/// <summary>
/// SMS delivery status.
/// </summary>
public record SmsDeliveryStatus(
    string MessageId,
    SmsStatus Status,
    DateTime? DeliveredAt,
    string? ErrorMessage
);

/// <summary>
/// SMS delivery status enumeration.
/// </summary>
public enum SmsStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Expired = 4
}
