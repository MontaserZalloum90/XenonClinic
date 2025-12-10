using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Core.Entities;

public class Supplier
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(100)]
    public string ContactPerson { get; set; } = string.Empty;

    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Mobile { get; set; }

    [MaxLength(20)]
    public string? Fax { get; set; }

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    public string? Website { get; set; }

    [MaxLength(50)]
    public string? TaxNumber { get; set; }

    public int? PaymentTermsDays { get; set; }

    [MaxLength(50)]
    public string Currency { get; set; } = "AED";

    public decimal? CreditLimit { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    // Audit fields
    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }
    [MaxLength(450)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
