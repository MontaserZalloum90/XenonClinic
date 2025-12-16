using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Core.Entities;

public class Employee : IBranchEntity
{
    public int Id { get; set; }

    // Personal Information
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    /// <summary>
    /// First name - computed from FullNameEn for service compatibility
    /// </summary>
    public string FirstName => FullNameEn?.Split(' ').FirstOrDefault() ?? string.Empty;
    /// <summary>
    /// Last name - computed from FullNameEn for service compatibility
    /// </summary>
    public string LastName => FullNameEn?.Split(' ').Skip(1).FirstOrDefault() ??
                              (FullNameEn?.Split(' ').Length > 1 ? string.Join(" ", FullNameEn.Split(' ').Skip(1)) : string.Empty);
    public string EmiratesId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty; // M/F
    public string Nationality { get; set; } = string.Empty;
    public string? PassportNumber { get; set; }

    // Contact Information
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Employment Information
    public int BranchId { get; set; }
    public int DepartmentId { get; set; }
    public int JobPositionId { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;

    // Compensation
    public decimal BasicSalary { get; set; }
    public decimal? HousingAllowance { get; set; }
    public decimal? TransportAllowance { get; set; }
    public decimal? OtherAllowances { get; set; }

    // Banking Information
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? IbanNumber { get; set; }

    // Leave Balances
    public int AnnualLeaveBalance { get; set; } = 30; // Days per year
    public int SickLeaveBalance { get; set; } = 90; // Days per year

    // Work Schedule
    public TimeOnly? WorkStartTime { get; set; }
    public TimeOnly? WorkEndTime { get; set; }

    // System Information
    public string? UserId { get; set; } // Link to ApplicationUser if employee has system access
    public string? ProfilePicturePath { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public JobPosition JobPosition { get; set; } = null!;
    // Note: ApplicationUser navigation removed to avoid circular dependency with Infrastructure
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();

    // Computed properties
    public decimal TotalSalary => BasicSalary +
        (HousingAllowance ?? 0) +
        (TransportAllowance ?? 0) +
        (OtherAllowances ?? 0);

    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year -
        (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

    public int YearsOfService => DateTime.UtcNow.Year - HireDate.Year -
        (DateTime.UtcNow.DayOfYear < HireDate.DayOfYear ? 1 : 0);
}
