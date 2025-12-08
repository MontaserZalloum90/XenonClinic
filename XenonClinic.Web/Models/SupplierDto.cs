namespace XenonClinic.Web.Models;

public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Mobile { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public string Currency { get; set; } = "AED";
    public int? PaymentTermsDays { get; set; }
    public decimal? CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public int TotalPurchaseOrders { get; set; }
    public decimal OutstandingBalance { get; set; }
}
