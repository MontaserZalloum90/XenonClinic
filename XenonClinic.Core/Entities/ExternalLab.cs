using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Core.Entities;

public class ExternalLab
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

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

    public int? TurnaroundTimeDays { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    // Audit fields
    [Required]
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; } = string.Empty;

    [MaxLength(450)]
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Navigation properties
    public ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();
    public ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
}
