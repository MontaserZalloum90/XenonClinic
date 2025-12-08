using System.ComponentModel.DataAnnotations;

namespace XenonClinic.Web.Models;

/// <summary>
/// Data Transfer Object for Patient entity
/// </summary>
public class PatientDto
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    [Required(ErrorMessage = "Emirates ID is required")]
    [RegularExpression(@"^\d{3}-\d{4}-\d{7}-\d{1}$", ErrorMessage = "Invalid Emirates ID format (XXX-XXXX-XXXXXXX-X)")]
    public string EmiratesId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name in English is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string FullNameEn { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string? FullNameAr { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [RegularExpression("^(M|F)$", ErrorMessage = "Gender must be M or F")]
    public string Gender { get; set; } = "M";

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? HearingLossType { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public string? BranchName { get; set; }
}
