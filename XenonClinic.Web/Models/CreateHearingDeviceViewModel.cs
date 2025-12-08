using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

public class CreateHearingDeviceViewModel
{
    [Required(ErrorMessage = "Patient is required")]
    [Display(Name = "Patient")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Model name is required")]
    [Display(Name = "Model Name")]
    [StringLength(200, ErrorMessage = "Model name cannot exceed 200 characters")]
    public string ModelName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Serial number is required")]
    [Display(Name = "Serial Number")]
    [StringLength(100, ErrorMessage = "Serial number cannot exceed 100 characters")]
    public string SerialNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Purchase date is required")]
    [Display(Name = "Purchase Date")]
    [DataType(DataType.Date)]
    public DateTime PurchaseDate { get; set; }

    [Display(Name = "Warranty Expiry Date")]
    [DataType(DataType.Date)]
    public DateTime? WarrantyExpiryDate { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Notes")]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
