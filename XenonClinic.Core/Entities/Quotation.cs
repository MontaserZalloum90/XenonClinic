using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a quotation/estimate for potential sales
/// </summary>
public class Quotation
{
    public int Id { get; set; }

    // Quotation Information
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

    // Customer Information
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    // Branch
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    // Financial Details
    public decimal SubTotal { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxPercentage { get; set; } = 5; // UAE VAT default 5%
    public decimal? TaxAmount { get; set; }
    public decimal Total { get; set; }

    // Additional Information
    public string? Notes { get; set; }
    public string? Terms { get; set; }
    public int ValidityDays { get; set; } = 30; // Default 30 days validity

    // Audit fields
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Acceptance/Rejection
    public DateTime? AcceptedDate { get; set; }
    public DateTime? RejectedDate { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation Properties
    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();

    // Computed Properties
    /// <summary>
    /// A quotation is expired if it has passed its expiry date and is still in Draft or Sent status
    /// (not yet accepted, rejected, or already marked as expired)
    /// </summary>
    public bool IsExpired => ExpiryDate.HasValue &&
                             ExpiryDate.Value < DateTime.UtcNow &&
                             (Status == QuotationStatus.Draft || Status == QuotationStatus.Sent);

    /// <summary>
    /// A quotation is active if it's sent, not expired, and hasn't been accepted/rejected
    /// </summary>
    public bool IsActive => Status == QuotationStatus.Sent && !IsExpired;

    /// <summary>
    /// A quotation can be converted to a sale only if it's been accepted
    /// </summary>
    public bool CanConvertToSale => Status == QuotationStatus.Accepted;
}
