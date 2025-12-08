using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class QuotationDto
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public QuotationStatus Status { get; set; }

    // Customer
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;

    // Financials
    public decimal Total { get; set; }

    // Additional Info
    public string BranchName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public bool IsExpired { get; set; }
    public bool IsActive { get; set; }
}
