namespace XenonClinic.Core.Enums;

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    CreditCard = 1,     // Alias for Card
    DebitCard = 1,      // Alias for Card
    BankTransfer = 2,
    Insurance = 3,
    Installment = 4,
    Cheque = 5
}
