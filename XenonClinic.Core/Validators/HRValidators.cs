using FluentValidation;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;

namespace XenonClinic.Core.Validators;

#region Employee Validators

/// <summary>
/// Validator for CreateEmployeeDto.
/// </summary>
public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FullNameEn)
            .NotEmpty().WithMessage(HRValidationMessages.FullNameRequired)
            .MaximumLength(200).WithMessage(HRValidationMessages.FullNameTooLong);

        RuleFor(x => x.FullNameAr)
            .MaximumLength(200).WithMessage("Arabic name cannot exceed 200 characters");

        RuleFor(x => x.EmiratesId)
            .NotEmpty().WithMessage(HRValidationMessages.EmiratesIdRequired)
            .Matches(@"^784-\d{4}-\d{7}-\d$").WithMessage(HRValidationMessages.EmiratesIdInvalid);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage(HRValidationMessages.DateOfBirthRequired)
            .LessThan(DateTime.UtcNow).WithMessage(HRValidationMessages.DateOfBirthFuture)
            .Must(BeAtLeast18YearsOld).WithMessage(HRValidationMessages.DateOfBirthTooRecent);

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage(HRValidationMessages.GenderRequired)
            .Must(g => g == "M" || g == "F").WithMessage(HRValidationMessages.GenderInvalid);

        RuleFor(x => x.Nationality)
            .NotEmpty().WithMessage("Nationality is required")
            .MaximumLength(100).WithMessage("Nationality cannot exceed 100 characters");

        RuleFor(x => x.PassportNumber)
            .MaximumLength(50).WithMessage("Passport number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.PassportNumber));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(HRValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(HRValidationMessages.EmailInvalid);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(HRValidationMessages.PhoneRequired)
            .Matches(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$")
            .WithMessage(HRValidationMessages.PhoneInvalid);

        RuleFor(x => x.AlternatePhone)
            .Matches(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$")
            .WithMessage("Alternate phone format is invalid")
            .When(x => !string.IsNullOrEmpty(x.AlternatePhone));

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.EmergencyContactPhone)
            .Matches(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$")
            .WithMessage("Emergency contact phone format is invalid")
            .When(x => !string.IsNullOrEmpty(x.EmergencyContactPhone));

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage(HRValidationMessages.DepartmentRequired);

        RuleFor(x => x.JobPositionId)
            .GreaterThan(0).WithMessage(HRValidationMessages.JobPositionRequired);

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage(HRValidationMessages.HireDateRequired)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(HRValidationMessages.HireDateFuture);

        RuleFor(x => x.BasicSalary)
            .GreaterThan(0).WithMessage(HRValidationMessages.SalaryInvalid)
            .LessThanOrEqualTo(1000000).WithMessage(HRValidationMessages.SalaryTooHigh);

        RuleFor(x => x.HousingAllowance)
            .GreaterThanOrEqualTo(0).WithMessage("Housing allowance cannot be negative")
            .When(x => x.HousingAllowance.HasValue);

        RuleFor(x => x.TransportAllowance)
            .GreaterThanOrEqualTo(0).WithMessage("Transport allowance cannot be negative")
            .When(x => x.TransportAllowance.HasValue);

        RuleFor(x => x.OtherAllowances)
            .GreaterThanOrEqualTo(0).WithMessage("Other allowances cannot be negative")
            .When(x => x.OtherAllowances.HasValue);

        RuleFor(x => x.AnnualLeaveBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Annual leave balance cannot be negative")
            .LessThanOrEqualTo(365).WithMessage("Annual leave balance cannot exceed 365 days");

        RuleFor(x => x.SickLeaveBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Sick leave balance cannot be negative")
            .LessThanOrEqualTo(365).WithMessage("Sick leave balance cannot exceed 365 days");
    }

    private static bool BeAtLeast18YearsOld(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age >= 18;
    }
}

/// <summary>
/// Validator for UpdateEmployeeDto.
/// </summary>
public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeDto>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Employee ID is required");

        RuleFor(x => x.FullNameEn)
            .NotEmpty().WithMessage(HRValidationMessages.FullNameRequired)
            .MaximumLength(200).WithMessage(HRValidationMessages.FullNameTooLong);

        RuleFor(x => x.FullNameAr)
            .MaximumLength(200).WithMessage("Arabic name cannot exceed 200 characters");

        RuleFor(x => x.EmiratesId)
            .NotEmpty().WithMessage(HRValidationMessages.EmiratesIdRequired)
            .Matches(@"^784-\d{4}-\d{7}-\d$").WithMessage(HRValidationMessages.EmiratesIdInvalid);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage(HRValidationMessages.DateOfBirthRequired)
            .LessThan(DateTime.UtcNow).WithMessage(HRValidationMessages.DateOfBirthFuture)
            .Must(BeAtLeast18YearsOld).WithMessage(HRValidationMessages.DateOfBirthTooRecent);

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage(HRValidationMessages.GenderRequired)
            .Must(g => g == "M" || g == "F").WithMessage(HRValidationMessages.GenderInvalid);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(HRValidationMessages.EmailRequired)
            .EmailAddress().WithMessage(HRValidationMessages.EmailInvalid);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(HRValidationMessages.PhoneRequired)
            .Matches(@"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$")
            .WithMessage(HRValidationMessages.PhoneInvalid);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage(HRValidationMessages.DepartmentRequired);

        RuleFor(x => x.JobPositionId)
            .GreaterThan(0).WithMessage(HRValidationMessages.JobPositionRequired);

        RuleFor(x => x.EmploymentStatus)
            .IsInEnum().WithMessage("Invalid employment status");

        RuleFor(x => x.TerminationDate)
            .GreaterThanOrEqualTo(x => DateTime.UtcNow.AddYears(-10))
            .WithMessage("Termination date is too far in the past")
            .When(x => x.TerminationDate.HasValue);

        RuleFor(x => x.BasicSalary)
            .GreaterThan(0).WithMessage(HRValidationMessages.SalaryInvalid)
            .LessThanOrEqualTo(1000000).WithMessage(HRValidationMessages.SalaryTooHigh);
    }

    private static bool BeAtLeast18YearsOld(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age >= 18;
    }
}

/// <summary>
/// Validator for EmployeeListRequestDto.
/// </summary>
public class EmployeeListRequestValidator : AbstractValidator<EmployeeListRequestDto>
{
    public EmployeeListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage(HRValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(HRValidationMessages.InvalidPageSize);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Invalid department ID")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.JobPositionId)
            .GreaterThan(0).WithMessage("Invalid job position ID")
            .When(x => x.JobPositionId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid employment status")
            .When(x => x.Status.HasValue);
    }
}

#endregion

#region Department Validators

/// <summary>
/// Validator for CreateDepartmentDto.
/// </summary>
public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(HRValidationMessages.DepartmentNameRequired)
            .MaximumLength(100).WithMessage(HRValidationMessages.DepartmentNameTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ManagerId)
            .GreaterThan(0).WithMessage("Invalid manager ID")
            .When(x => x.ManagerId.HasValue);
    }
}

/// <summary>
/// Validator for UpdateDepartmentDto.
/// </summary>
public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentDto>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Department ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(HRValidationMessages.DepartmentNameRequired)
            .MaximumLength(100).WithMessage(HRValidationMessages.DepartmentNameTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ManagerId)
            .GreaterThan(0).WithMessage("Invalid manager ID")
            .When(x => x.ManagerId.HasValue);
    }
}

#endregion

#region Job Position Validators

/// <summary>
/// Validator for CreateJobPositionDto.
/// </summary>
public class CreateJobPositionValidator : AbstractValidator<CreateJobPositionDto>
{
    public CreateJobPositionValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(HRValidationMessages.JobTitleRequired)
            .MaximumLength(100).WithMessage(HRValidationMessages.JobTitleTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.MinSalary)
            .GreaterThanOrEqualTo(0).WithMessage(HRValidationMessages.MinSalaryInvalid);

        RuleFor(x => x.MaxSalary)
            .GreaterThan(x => x.MinSalary).WithMessage(HRValidationMessages.MaxSalaryInvalid);
    }
}

/// <summary>
/// Validator for UpdateJobPositionDto.
/// </summary>
public class UpdateJobPositionValidator : AbstractValidator<UpdateJobPositionDto>
{
    public UpdateJobPositionValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Job position ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(HRValidationMessages.JobTitleRequired)
            .MaximumLength(100).WithMessage(HRValidationMessages.JobTitleTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.MinSalary)
            .GreaterThanOrEqualTo(0).WithMessage(HRValidationMessages.MinSalaryInvalid);

        RuleFor(x => x.MaxSalary)
            .GreaterThan(x => x.MinSalary).WithMessage(HRValidationMessages.MaxSalaryInvalid);
    }
}

#endregion

#region Attendance Validators

/// <summary>
/// Validator for CheckInDto.
/// </summary>
public class CheckInValidator : AbstractValidator<CheckInDto>
{
    public CheckInValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("Employee ID is required");

        RuleFor(x => x.CheckInTime)
            .NotEmpty().WithMessage(HRValidationMessages.CheckInTimeRequired)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)).WithMessage("Check-in time cannot be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for CheckOutDto.
/// </summary>
public class CheckOutValidator : AbstractValidator<CheckOutDto>
{
    public CheckOutValidator()
    {
        RuleFor(x => x.AttendanceId)
            .GreaterThan(0).WithMessage("Attendance ID is required");

        RuleFor(x => x.CheckOutTime)
            .NotEmpty().WithMessage("Check-out time is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)).WithMessage("Check-out time cannot be in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for CreateAttendanceDto.
/// </summary>
public class CreateAttendanceValidator : AbstractValidator<CreateAttendanceDto>
{
    public CreateAttendanceValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("Employee ID is required");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Attendance date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(HRValidationMessages.AttendanceDateFuture);

        RuleFor(x => x.CheckOutTime)
            .GreaterThan(x => x.CheckInTime).WithMessage(HRValidationMessages.CheckOutTimeInvalid)
            .When(x => x.CheckInTime.HasValue && x.CheckOutTime.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid attendance status");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for UpdateAttendanceDto.
/// </summary>
public class UpdateAttendanceValidator : AbstractValidator<UpdateAttendanceDto>
{
    public UpdateAttendanceValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Attendance ID is required");

        RuleFor(x => x.CheckOutTime)
            .GreaterThan(x => x.CheckInTime).WithMessage(HRValidationMessages.CheckOutTimeInvalid)
            .When(x => x.CheckInTime.HasValue && x.CheckOutTime.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid attendance status");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Validator for AttendanceListRequestDto.
/// </summary>
public class AttendanceListRequestValidator : AbstractValidator<AttendanceListRequestDto>
{
    public AttendanceListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage(HRValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(HRValidationMessages.InvalidPageSize);

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("Invalid employee ID")
            .When(x => x.EmployeeId.HasValue);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Invalid department ID")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage("End date must be after start date")
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid attendance status")
            .When(x => x.Status.HasValue);
    }
}

#endregion

#region Leave Request Validators

/// <summary>
/// Validator for CreateLeaveRequestDto.
/// </summary>
public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestDto>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("Employee ID is required");

        RuleFor(x => x.LeaveType)
            .IsInEnum().WithMessage(HRValidationMessages.LeaveTypeRequired);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage(HRValidationMessages.LeaveStartDateRequired)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage(HRValidationMessages.LeaveEndDateRequired)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage(HRValidationMessages.LeaveEndDateInvalid);

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(HRValidationMessages.LeaveReasonRequired)
            .MaximumLength(500).WithMessage(HRValidationMessages.LeaveReasonTooLong);

        RuleFor(x => x.AttachmentPath)
            .MaximumLength(500).WithMessage("Attachment path cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.AttachmentPath));

        // Validate medical certificate for sick leave longer than 3 days
        RuleFor(x => x.AttachmentPath)
            .NotEmpty().WithMessage("Medical certificate is required for sick leave longer than 3 days")
            .When(x => x.LeaveType == LeaveType.Sick && (x.EndDate - x.StartDate).Days > 3);
    }
}

/// <summary>
/// Validator for UpdateLeaveRequestDto.
/// </summary>
public class UpdateLeaveRequestValidator : AbstractValidator<UpdateLeaveRequestDto>
{
    public UpdateLeaveRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Leave request ID is required");

        RuleFor(x => x.LeaveType)
            .IsInEnum().WithMessage(HRValidationMessages.LeaveTypeRequired);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage(HRValidationMessages.LeaveStartDateRequired);

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage(HRValidationMessages.LeaveEndDateRequired)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage(HRValidationMessages.LeaveEndDateInvalid);

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(HRValidationMessages.LeaveReasonRequired)
            .MaximumLength(500).WithMessage(HRValidationMessages.LeaveReasonTooLong);
    }
}

/// <summary>
/// Validator for ApproveLeaveRequestDto.
/// </summary>
public class ApproveLeaveRequestValidator : AbstractValidator<ApproveLeaveRequestDto>
{
    public ApproveLeaveRequestValidator()
    {
        RuleFor(x => x.LeaveRequestId)
            .GreaterThan(0).WithMessage("Leave request ID is required");

        RuleFor(x => x.Comments)
            .MaximumLength(500).WithMessage("Comments cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));
    }
}

/// <summary>
/// Validator for RejectLeaveRequestDto.
/// </summary>
public class RejectLeaveRequestValidator : AbstractValidator<RejectLeaveRequestDto>
{
    public RejectLeaveRequestValidator()
    {
        RuleFor(x => x.LeaveRequestId)
            .GreaterThan(0).WithMessage("Leave request ID is required");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage(HRValidationMessages.RejectionReasonRequired)
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");
    }
}

/// <summary>
/// Validator for LeaveRequestListDto.
/// </summary>
public class LeaveRequestListValidator : AbstractValidator<LeaveRequestListDto>
{
    public LeaveRequestListValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage(HRValidationMessages.InvalidPageNumber);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage(HRValidationMessages.InvalidPageSize);

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("Invalid employee ID")
            .When(x => x.EmployeeId.HasValue);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Invalid department ID")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.LeaveType)
            .IsInEnum().WithMessage("Invalid leave type")
            .When(x => x.LeaveType.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid leave status")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom).WithMessage("End date must be after start date")
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue);
    }
}

#endregion
