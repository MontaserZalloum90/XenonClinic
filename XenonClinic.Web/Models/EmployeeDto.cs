using XenonClinic.Core.Enums;

namespace XenonClinic.Web.Models;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string EmiratesId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int JobPositionId { get; set; }
    public string JobPositionTitle { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public string EmploymentStatusDisplay { get; set; } = string.Empty;
    public decimal TotalSalary { get; set; }
    public int Age { get; set; }
    public int YearsOfService { get; set; }
    public bool IsActive { get; set; }
}
