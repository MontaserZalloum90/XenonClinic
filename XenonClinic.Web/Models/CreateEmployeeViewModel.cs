using System.ComponentModel.DataAnnotations;
using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class CreateEmployeeViewModel
{
    [Required] [StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required] [StringLength(200)]
    public string FullNameEn { get; set; } = string.Empty;

    [Required] [StringLength(200)]
    public string FullNameAr { get; set; } = string.Empty;

    [Required] [StringLength(20)]
    public string EmiratesId { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required] [StringLength(1)]
    public string Gender { get; set; } = string.Empty;

    [Required] [StringLength(100)]
    public string Nationality { get; set; } = string.Empty;

    [StringLength(50)]
    public string? PassportNumber { get; set; }

    [Required] [EmailAddress] [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required] [Phone] [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Phone] [StringLength(20)]
    public string? AlternatePhone { get; set; }

    [Required] [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [StringLength(200)]
    public string? EmergencyContactName { get; set; }

    [Phone] [StringLength(20)]
    public string? EmergencyContactPhone { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int JobPositionId { get; set; }

    [Required]
    public DateTime HireDate { get; set; }

    [Required] [Range(0.01, double.MaxValue)]
    public decimal BasicSalary { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? HousingAllowance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? TransportAllowance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? OtherAllowances { get; set; }

    public TimeOnly? WorkStartTime { get; set; }

    public TimeOnly? WorkEndTime { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
