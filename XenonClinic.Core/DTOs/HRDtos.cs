using XenonClinic.Core.Enums;

namespace XenonClinic.Core.DTOs;

#region Employee DTOs

/// <summary>
/// DTO for employee data transfer. Used for reading employee information.
/// </summary>
public class EmployeeDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string EmiratesId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age => CalculateAge(DateOfBirth);
    public string Gender { get; set; } = string.Empty;
    public string GenderDisplay => Gender switch
    {
        "M" => "Male",
        "F" => "Female",
        _ => "Other"
    };
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
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int JobPositionId { get; set; }
    public string? JobPositionTitle { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public string EmploymentStatusDisplay => EmploymentStatus switch
    {
        EmploymentStatus.Active => "Active",
        EmploymentStatus.OnLeave => "On Leave",
        EmploymentStatus.Suspended => "Suspended",
        EmploymentStatus.Terminated => "Terminated",
        EmploymentStatus.Resigned => "Resigned",
        _ => "Unknown"
    };

    // Compensation
    public decimal BasicSalary { get; set; }
    public decimal? HousingAllowance { get; set; }
    public decimal? TransportAllowance { get; set; }
    public decimal? OtherAllowances { get; set; }
    public decimal TotalSalary => BasicSalary + (HousingAllowance ?? 0) + (TransportAllowance ?? 0) + (OtherAllowances ?? 0);

    // Leave Balances
    public int AnnualLeaveBalance { get; set; }
    public int SickLeaveBalance { get; set; }

    // Work Schedule
    public TimeOnly? WorkStartTime { get; set; }
    public TimeOnly? WorkEndTime { get; set; }

    // System Information
    public string? UserId { get; set; }
    public string? ProfilePicturePath { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }

    // Computed
    public int YearsOfService => CalculateYearsOfService(HireDate);

    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }

    private static int CalculateYearsOfService(DateTime hireDate)
    {
        var today = DateTime.Today;
        var years = today.Year - hireDate.Year;
        if (hireDate.Date > today.AddYears(-years)) years--;
        return years;
    }
}

/// <summary>
/// DTO for creating a new employee.
/// </summary>
public class CreateEmployeeDto
{
    public string FullNameEn { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string EmiratesId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "M";
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
    public int DepartmentId { get; set; }
    public int JobPositionId { get; set; }
    public DateTime HireDate { get; set; }

    // Compensation
    public decimal BasicSalary { get; set; }
    public decimal? HousingAllowance { get; set; }
    public decimal? TransportAllowance { get; set; }
    public decimal? OtherAllowances { get; set; }

    // Leave Balances
    public int AnnualLeaveBalance { get; set; } = 30;
    public int SickLeaveBalance { get; set; } = 90;

    // Work Schedule
    public TimeOnly? WorkStartTime { get; set; }
    public TimeOnly? WorkEndTime { get; set; }

    // System Information
    public string? UserId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing employee.
/// </summary>
public class UpdateEmployeeDto
{
    public int Id { get; set; }
    public string FullNameEn { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string EmiratesId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "M";
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
    public int DepartmentId { get; set; }
    public int JobPositionId { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public DateTime? TerminationDate { get; set; }

    // Compensation
    public decimal BasicSalary { get; set; }
    public decimal? HousingAllowance { get; set; }
    public decimal? TransportAllowance { get; set; }
    public decimal? OtherAllowances { get; set; }

    // Leave Balances
    public int AnnualLeaveBalance { get; set; }
    public int SickLeaveBalance { get; set; }

    // Work Schedule
    public TimeOnly? WorkStartTime { get; set; }
    public TimeOnly? WorkEndTime { get; set; }

    // System Information
    public string? UserId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for employee search results (lightweight).
/// </summary>
public class EmployeeSearchResultDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobPositionTitle { get; set; }
    public EmploymentStatus EmploymentStatus { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

#endregion

#region Department DTOs

/// <summary>
/// DTO for department data transfer.
/// </summary>
public class DepartmentDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating a department.
/// </summary>
public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerId { get; set; }
}

/// <summary>
/// DTO for updating a department.
/// </summary>
public class UpdateDepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ManagerId { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region Job Position DTOs

/// <summary>
/// DTO for job position data transfer.
/// </summary>
public class JobPositionDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for creating a job position.
/// </summary>
public class CreateJobPositionDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
}

/// <summary>
/// DTO for updating a job position.
/// </summary>
public class UpdateJobPositionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region Attendance DTOs

/// <summary>
/// DTO for attendance data transfer.
/// </summary>
public class AttendanceDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        AttendanceStatus.Present => "Present",
        AttendanceStatus.Absent => "Absent",
        AttendanceStatus.Late => "Late",
        AttendanceStatus.HalfDay => "Half Day",
        AttendanceStatus.OnLeave => "On Leave",
        AttendanceStatus.Holiday => "Holiday",
        _ => "Unknown"
    };
    public bool IsLate { get; set; }
    public int? LateMinutes { get; set; }
    public decimal? WorkedHours { get; set; }
    public decimal? OvertimeHours { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for checking in an employee.
/// </summary>
public class CheckInDto
{
    public int EmployeeId { get; set; }
    public DateTime CheckInTime { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for checking out an employee.
/// </summary>
public class CheckOutDto
{
    public int AttendanceId { get; set; }
    public DateTime CheckOutTime { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating manual attendance record.
/// </summary>
public class CreateAttendanceDto
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating attendance record.
/// </summary>
public class UpdateAttendanceDto
{
    public int Id { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for attendance list request with filters.
/// </summary>
public class AttendanceListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public AttendanceStatus? Status { get; set; }
    public string? SortBy { get; set; } = "Date";
    public bool SortDescending { get; set; } = true;
}

#endregion

#region Leave Request DTOs

/// <summary>
/// DTO for leave request data transfer.
/// </summary>
public class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public string? DepartmentName { get; set; }
    public LeaveType LeaveType { get; set; }
    public string LeaveTypeDisplay => LeaveType switch
    {
        LeaveType.Annual => "Annual Leave",
        LeaveType.Sick => "Sick Leave",
        LeaveType.Emergency => "Emergency Leave",
        LeaveType.Maternity => "Maternity Leave",
        LeaveType.Paternity => "Paternity Leave",
        LeaveType.Unpaid => "Unpaid Leave",
        LeaveType.Study => "Study Leave",
        LeaveType.Hajj => "Hajj Leave",
        LeaveType.Other => "Other",
        _ => "Unknown"
    };
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        LeaveStatus.Pending => "Pending",
        LeaveStatus.Approved => "Approved",
        LeaveStatus.Rejected => "Rejected",
        LeaveStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public string? AttachmentPath { get; set; }
    public DateTime RequestDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for creating a leave request.
/// </summary>
public class CreateLeaveRequestDto
{
    public int EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
}

/// <summary>
/// DTO for updating a leave request.
/// </summary>
public class UpdateLeaveRequestDto
{
    public int Id { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
}

/// <summary>
/// DTO for approving a leave request.
/// </summary>
public class ApproveLeaveRequestDto
{
    public int LeaveRequestId { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// DTO for rejecting a leave request.
/// </summary>
public class RejectLeaveRequestDto
{
    public int LeaveRequestId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// DTO for leave request list with filters.
/// </summary>
public class LeaveRequestListDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public LeaveType? LeaveType { get; set; }
    public LeaveStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; } = "RequestDate";
    public bool SortDescending { get; set; } = true;
}

#endregion

#region HR Statistics DTOs

/// <summary>
/// DTO for HR dashboard statistics.
/// </summary>
public class HRStatisticsDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int OnLeaveEmployees { get; set; }
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
    public int LateToday { get; set; }
    public int PendingLeaveRequests { get; set; }
    public decimal TotalPayroll { get; set; }
    public Dictionary<string, int> EmploymentStatusDistribution { get; set; } = new();
    public Dictionary<string, int> DepartmentDistribution { get; set; } = new();
    public Dictionary<string, int> LeaveTypeDistribution { get; set; } = new();
}

/// <summary>
/// DTO for employee list request with filters.
/// </summary>
public class EmployeeListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public int? DepartmentId { get; set; }
    public int? JobPositionId { get; set; }
    public EmploymentStatus? Status { get; set; }
    public string? SortBy { get; set; } = "FullNameEn";
    public bool SortDescending { get; set; }
}

#endregion

#region Validation Messages

/// <summary>
/// Standard validation error messages for HR operations.
/// </summary>
public static class HRValidationMessages
{
    // Employee messages
    public const string EmployeeNotFound = "Employee not found";
    public const string EmployeeCodeDuplicate = "An employee with this code already exists";
    public const string EmiratesIdRequired = "Emirates ID is required";
    public const string EmiratesIdInvalid = "Emirates ID format is invalid. Expected format: 784-XXXX-XXXXXXX-X";
    public const string EmiratesIdDuplicate = "An employee with this Emirates ID already exists";
    public const string FullNameRequired = "Employee full name (English) is required";
    public const string FullNameTooLong = "Employee name cannot exceed 200 characters";
    public const string DateOfBirthRequired = "Date of birth is required";
    public const string DateOfBirthFuture = "Date of birth cannot be in the future";
    public const string DateOfBirthTooRecent = "Employee must be at least 18 years old";
    public const string GenderRequired = "Gender is required";
    public const string GenderInvalid = "Gender must be M (Male) or F (Female)";
    public const string EmailRequired = "Email address is required";
    public const string EmailInvalid = "Email address format is invalid";
    public const string PhoneRequired = "Phone number is required";
    public const string PhoneInvalid = "Phone number format is invalid";
    public const string DepartmentRequired = "Department is required";
    public const string DepartmentNotFound = "Department not found";
    public const string JobPositionRequired = "Job position is required";
    public const string JobPositionNotFound = "Job position not found";
    public const string HireDateRequired = "Hire date is required";
    public const string HireDateFuture = "Hire date cannot be in the future";
    public const string SalaryInvalid = "Basic salary must be greater than 0";
    public const string SalaryTooHigh = "Salary exceeds maximum allowed value";

    // Department messages
    public const string DepartmentNameRequired = "Department name is required";
    public const string DepartmentNameTooLong = "Department name cannot exceed 100 characters";
    public const string DepartmentNameDuplicate = "A department with this name already exists";

    // Job Position messages
    public const string JobTitleRequired = "Job title is required";
    public const string JobTitleTooLong = "Job title cannot exceed 100 characters";
    public const string MinSalaryInvalid = "Minimum salary must be greater than or equal to 0";
    public const string MaxSalaryInvalid = "Maximum salary must be greater than minimum salary";

    // Attendance messages
    public const string AttendanceNotFound = "Attendance record not found";
    public const string AttendanceAlreadyExists = "Attendance record already exists for this date";
    public const string CheckInTimeRequired = "Check-in time is required";
    public const string CheckOutTimeInvalid = "Check-out time cannot be before check-in time";
    public const string AttendanceDateFuture = "Attendance date cannot be in the future";

    // Leave Request messages
    public const string LeaveRequestNotFound = "Leave request not found";
    public const string LeaveTypeRequired = "Leave type is required";
    public const string LeaveStartDateRequired = "Start date is required";
    public const string LeaveEndDateRequired = "End date is required";
    public const string LeaveEndDateInvalid = "End date cannot be before start date";
    public const string LeaveReasonRequired = "Leave reason is required";
    public const string LeaveReasonTooLong = "Leave reason cannot exceed 500 characters";
    public const string LeaveInsufficientBalance = "Insufficient leave balance for this request";
    public const string LeaveAlreadyApproved = "This leave request has already been approved";
    public const string LeaveAlreadyRejected = "This leave request has already been rejected";
    public const string LeaveOverlapping = "This leave request overlaps with an existing approved leave";
    public const string RejectionReasonRequired = "Rejection reason is required";

    // General messages
    public const string BranchAccessDenied = "You do not have access to this branch";
    public const string InvalidPageNumber = "Page number must be greater than 0";
    public const string InvalidPageSize = "Page size must be between 1 and 100";
}

#endregion
