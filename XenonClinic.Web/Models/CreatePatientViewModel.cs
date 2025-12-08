using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

/// <summary>
/// ViewModel for creating a new patient
/// </summary>
public class CreatePatientViewModel
{
    [Required(ErrorMessage = "Emirates ID is required")]
    [Display(Name = "Emirates ID")]
    [RegularExpression(@"^\d{3}-\d{4}-\d{7}-\d{1}$", ErrorMessage = "Invalid Emirates ID format (XXX-XXXX-XXXXXXX-X)")]
    public string EmiratesId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name in English is required")]
    [Display(Name = "Full Name (English)")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string FullNameEn { get; set; } = string.Empty;

    [Display(Name = "Full Name (Arabic)")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string? FullNameAr { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.AddYears(-30);

    [Required(ErrorMessage = "Gender is required")]
    [Display(Name = "Gender")]
    public string Gender { get; set; } = "M";

    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Hearing Loss Type")]
    public string? HearingLossType { get; set; }

    [Display(Name = "Notes")]
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public int BranchId { get; set; }
}
