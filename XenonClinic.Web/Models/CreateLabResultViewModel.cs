using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreateLabResultViewModel
{
    [Required]
    public int LabOrderId { get; set; }

    [Required]
    public int LabOrderItemId { get; set; }

    [Required]
    public int LabTestId { get; set; }

    public DateTime? ResultDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? ResultValue { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    public string? ReferenceRange { get; set; }

    public bool IsAbnormal { get; set; } = false;

    public string? Interpretation { get; set; }

    public string? Notes { get; set; }

    public IFormFile? Attachment { get; set; }
}
