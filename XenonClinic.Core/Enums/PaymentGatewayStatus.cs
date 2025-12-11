namespace XenonClinic.Core.Enums;

/// <summary>
/// Status of a payment gateway transaction
/// </summary>
public enum PaymentGatewayStatus
{
    Pending = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6,
    PartiallyRefunded = 7,
    Disputed = 8,
    RequiresAction = 9,       // 3D Secure, etc.
    RequiresCapture = 10,     // Authorized but not captured
    Expired = 11
}
