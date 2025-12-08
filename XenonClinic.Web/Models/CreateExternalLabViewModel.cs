using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreateExternalLabViewModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? Mobile { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    public string? Website { get; set; }

    [MaxLength(50)]
    public string? LicenseNumber { get; set; }

    public DateTime? LicenseExpiryDate { get; set; }

    [Range(1, 365)]
    public int? TurnaroundTimeDays { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }
}
