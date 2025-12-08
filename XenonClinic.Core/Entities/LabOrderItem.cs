using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Core.Entities;

public class LabOrderItem
{
    public int Id { get; set; }

    [Required]
    public int LabOrderId { get; set; }
    public LabOrder? LabOrder { get; set; }

    [Required]
    public int LabTestId { get; set; }
    public LabTest? LabTest { get; set; }

    [Required]
    [MaxLength(50)]
    public string TestCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TestName { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    public string? Notes { get; set; }
}
