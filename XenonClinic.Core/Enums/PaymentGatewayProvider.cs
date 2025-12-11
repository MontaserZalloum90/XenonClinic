namespace XenonClinic.Core.Enums;

/// <summary>
/// Supported payment gateway providers
/// </summary>
public enum PaymentGatewayProvider
{
    Stripe = 1,
    PayPal = 2,
    Square = 3,
    Authorize = 4,
    Braintree = 5,
    Adyen = 6,
    NetworkInternational = 7,  // Popular in UAE/GCC
    Checkout = 8,
    TapPayments = 9,           // Popular in Middle East
    Telr = 10                  // Popular in GCC
}
