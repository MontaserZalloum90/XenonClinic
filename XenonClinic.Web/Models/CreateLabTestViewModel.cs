using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreateLabTestViewModel
{
    [Required]
    [MaxLength(50)]
    public string TestCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TestName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public TestCategory Category { get; set; }

    public SpecimenType? SpecimenType { get; set; }

    [MaxLength(100)]
    public string? SpecimenVolume { get; set; }

    [Range(1, 720)]
    public int? TurnaroundTimeHours { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    public string? ReferenceRange { get; set; }

    public string? Methodology { get; set; }

    public bool IsActive { get; set; } = true;

    public bool RequiresFasting { get; set; } = false;

    public string? PreparationInstructions { get; set; }

    public int? ExternalLabId { get; set; }
}
