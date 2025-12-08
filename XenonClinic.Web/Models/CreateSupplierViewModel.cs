using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreateSupplierViewModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [Required]
    [MaxLength(100)]
    public string ContactPerson { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? Mobile { get; set; }

    [MaxLength(20)]
    public string? Fax { get; set; }

    [Required]
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

    [Range(0, 365)]
    public int? PaymentTermsDays { get; set; }

    [Required]
    [MaxLength(50)]
    public string Currency { get; set; } = "AED";

    [Range(0, double.MaxValue)]
    public decimal? CreditLimit { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }
}
