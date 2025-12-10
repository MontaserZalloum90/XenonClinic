using FluentAssertions;
using FluentValidation.TestHelper;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Validators;
using Xunit;

namespace XenonClinic.Tests.HR;

public class HRValidatorTests
{
    #region CreateEmployeeValidator Tests

    public class CreateEmployeeValidatorTests
    {
        private readonly CreateEmployeeValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Employee()
        {
            var dto = CreateValidEmployeeDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_FullNameEn_Is_Empty(string? name)
        {
            var dto = CreateValidEmployeeDto();
            dto.FullNameEn = name!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullNameEn)
                .WithErrorMessage(HRValidationMessages.FullNameRequired);
        }

        [Fact]
        public void Should_Fail_When_FullNameEn_Exceeds_MaxLength()
        {
            var dto = CreateValidEmployeeDto();
            dto.FullNameEn = new string('A', 201);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullNameEn)
                .WithErrorMessage(HRValidationMessages.FullNameTooLong);
        }

        [Fact]
        public void Should_Fail_When_EmiratesId_Is_Empty()
        {
            var dto = CreateValidEmployeeDto();
            dto.EmiratesId = "";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EmiratesId)
                .WithErrorMessage(HRValidationMessages.EmiratesIdRequired);
        }

        [Theory]
        [InlineData("123456789")]
        [InlineData("784-1234-12345-1")]
        [InlineData("784-12345-1234567-1")]
        public void Should_Fail_When_EmiratesId_Format_Is_Invalid(string emiratesId)
        {
            var dto = CreateValidEmployeeDto();
            dto.EmiratesId = emiratesId;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EmiratesId)
                .WithErrorMessage(HRValidationMessages.EmiratesIdInvalid);
        }

        [Fact]
        public void Should_Pass_When_EmiratesId_Format_Is_Valid()
        {
            var dto = CreateValidEmployeeDto();
            dto.EmiratesId = "784-1234-1234567-1";

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.EmiratesId);
        }

        [Fact]
        public void Should_Fail_When_DateOfBirth_Is_In_Future()
        {
            var dto = CreateValidEmployeeDto();
            dto.DateOfBirth = DateTime.UtcNow.AddDays(1);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
                .WithErrorMessage(HRValidationMessages.DateOfBirthFuture);
        }

        [Fact]
        public void Should_Fail_When_Employee_Under_18()
        {
            var dto = CreateValidEmployeeDto();
            dto.DateOfBirth = DateTime.UtcNow.AddYears(-17);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
                .WithErrorMessage(HRValidationMessages.DateOfBirthTooRecent);
        }

        [Theory]
        [InlineData("X")]
        [InlineData("Male")]
        [InlineData("")]
        public void Should_Fail_When_Gender_Is_Invalid(string gender)
        {
            var dto = CreateValidEmployeeDto();
            dto.Gender = gender;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Gender);
        }

        [Theory]
        [InlineData("M")]
        [InlineData("F")]
        public void Should_Pass_When_Gender_Is_Valid(string gender)
        {
            var dto = CreateValidEmployeeDto();
            dto.Gender = gender;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Gender);
        }

        [Fact]
        public void Should_Fail_When_Email_Is_Invalid()
        {
            var dto = CreateValidEmployeeDto();
            dto.Email = "invalid-email";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage(HRValidationMessages.EmailInvalid);
        }

        [Fact]
        public void Should_Fail_When_PhoneNumber_Is_Invalid()
        {
            var dto = CreateValidEmployeeDto();
            dto.PhoneNumber = "123";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
                .WithErrorMessage(HRValidationMessages.PhoneInvalid);
        }

        [Fact]
        public void Should_Fail_When_DepartmentId_Is_Zero()
        {
            var dto = CreateValidEmployeeDto();
            dto.DepartmentId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DepartmentId)
                .WithErrorMessage(HRValidationMessages.DepartmentRequired);
        }

        [Fact]
        public void Should_Fail_When_BasicSalary_Is_Zero()
        {
            var dto = CreateValidEmployeeDto();
            dto.BasicSalary = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.BasicSalary)
                .WithErrorMessage(HRValidationMessages.SalaryInvalid);
        }

        [Fact]
        public void Should_Fail_When_BasicSalary_Exceeds_Maximum()
        {
            var dto = CreateValidEmployeeDto();
            dto.BasicSalary = 1000001;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.BasicSalary)
                .WithErrorMessage(HRValidationMessages.SalaryTooHigh);
        }

        [Fact]
        public void Should_Fail_When_HireDate_Is_In_Future()
        {
            var dto = CreateValidEmployeeDto();
            dto.HireDate = DateTime.UtcNow.AddDays(1);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.HireDate)
                .WithErrorMessage(HRValidationMessages.HireDateFuture);
        }

        private static CreateEmployeeDto CreateValidEmployeeDto()
        {
            return new CreateEmployeeDto
            {
                FullNameEn = "John Doe",
                FullNameAr = "جون دو",
                EmiratesId = "784-1234-1234567-1",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                Gender = "M",
                Nationality = "United States",
                Email = "john.doe@example.com",
                PhoneNumber = "050-123-4567",
                Address = "123 Main Street, Dubai",
                DepartmentId = 1,
                JobPositionId = 1,
                HireDate = DateTime.UtcNow.AddMonths(-6),
                BasicSalary = 10000
            };
        }
    }

    #endregion

    #region CreateDepartmentValidator Tests

    public class CreateDepartmentValidatorTests
    {
        private readonly CreateDepartmentValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Department()
        {
            var dto = new CreateDepartmentDto
            {
                Name = "Human Resources",
                Description = "HR Department"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_Name_Is_Empty(string? name)
        {
            var dto = new CreateDepartmentDto { Name = name! };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(HRValidationMessages.DepartmentNameRequired);
        }

        [Fact]
        public void Should_Fail_When_Name_Exceeds_MaxLength()
        {
            var dto = new CreateDepartmentDto { Name = new string('A', 101) };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(HRValidationMessages.DepartmentNameTooLong);
        }

        [Fact]
        public void Should_Fail_When_ManagerId_Is_Invalid()
        {
            var dto = new CreateDepartmentDto
            {
                Name = "HR",
                ManagerId = -1
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ManagerId);
        }
    }

    #endregion

    #region CreateJobPositionValidator Tests

    public class CreateJobPositionValidatorTests
    {
        private readonly CreateJobPositionValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_JobPosition()
        {
            var dto = new CreateJobPositionDto
            {
                Title = "Software Engineer",
                Description = "Develops software",
                MinSalary = 5000,
                MaxSalary = 15000
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_Title_Is_Empty(string? title)
        {
            var dto = new CreateJobPositionDto
            {
                Title = title!,
                MinSalary = 5000,
                MaxSalary = 15000
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                .WithErrorMessage(HRValidationMessages.JobTitleRequired);
        }

        [Fact]
        public void Should_Fail_When_MinSalary_Is_Negative()
        {
            var dto = new CreateJobPositionDto
            {
                Title = "Test",
                MinSalary = -1,
                MaxSalary = 15000
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.MinSalary)
                .WithErrorMessage(HRValidationMessages.MinSalaryInvalid);
        }

        [Fact]
        public void Should_Fail_When_MaxSalary_Less_Than_MinSalary()
        {
            var dto = new CreateJobPositionDto
            {
                Title = "Test",
                MinSalary = 10000,
                MaxSalary = 5000
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.MaxSalary)
                .WithErrorMessage(HRValidationMessages.MaxSalaryInvalid);
        }
    }

    #endregion

    #region CreateLeaveRequestValidator Tests

    public class CreateLeaveRequestValidatorTests
    {
        private readonly CreateLeaveRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_LeaveRequest()
        {
            var dto = new CreateLeaveRequestDto
            {
                EmployeeId = 1,
                LeaveType = LeaveType.Annual,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                Reason = "Vacation"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_EmployeeId_Is_Zero()
        {
            var dto = new CreateLeaveRequestDto
            {
                EmployeeId = 0,
                LeaveType = LeaveType.Annual,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                Reason = "Vacation"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        }

        [Fact]
        public void Should_Fail_When_StartDate_Is_In_Past()
        {
            var dto = new CreateLeaveRequestDto
            {
                EmployeeId = 1,
                LeaveType = LeaveType.Annual,
                StartDate = DateTime.UtcNow.Date.AddDays(-1),
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                Reason = "Vacation"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.StartDate);
        }

        [Fact]
        public void Should_Fail_When_EndDate_Before_StartDate()
        {
            var dto = new CreateLeaveRequestDto
            {
                EmployeeId = 1,
                LeaveType = LeaveType.Annual,
                StartDate = DateTime.UtcNow.Date.AddDays(5),
                EndDate = DateTime.UtcNow.Date.AddDays(1),
                Reason = "Vacation"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EndDate)
                .WithErrorMessage(HRValidationMessages.LeaveEndDateInvalid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_Reason_Is_Empty(string? reason)
        {
            var dto = new CreateLeaveRequestDto
            {
                EmployeeId = 1,
                LeaveType = LeaveType.Annual,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(5),
                Reason = reason!
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Reason)
                .WithErrorMessage(HRValidationMessages.LeaveReasonRequired);
        }

        [Fact]
        public void Should_Fail_When_SickLeave_Over_3_Days_Without_MedicalCertificate()
        {
            var dto = new CreateLeaveRequestDto
            {
                EmployeeId = 1,
                LeaveType = LeaveType.Sick,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(5), // 5 days
                Reason = "Sick",
                AttachmentPath = null
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.AttachmentPath);
        }

        [Fact]
        public void Should_Pass_When_SickLeave_3_Days_Without_MedicalCertificate()
        {
            var dto = new CreateLeaveRequestDto
            {
                EmployeeId = 1,
                LeaveType = LeaveType.Sick,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(3), // 3 days
                Reason = "Sick"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.AttachmentPath);
        }
    }

    #endregion

    #region CheckInValidator Tests

    public class CheckInValidatorTests
    {
        private readonly CheckInValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_CheckIn()
        {
            var dto = new CheckInDto
            {
                EmployeeId = 1,
                CheckInTime = DateTime.UtcNow.AddMinutes(-5)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_EmployeeId_Is_Zero()
        {
            var dto = new CheckInDto
            {
                EmployeeId = 0,
                CheckInTime = DateTime.UtcNow
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        }

        [Fact]
        public void Should_Fail_When_CheckInTime_Is_In_Future()
        {
            var dto = new CheckInDto
            {
                EmployeeId = 1,
                CheckInTime = DateTime.UtcNow.AddHours(1)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CheckInTime);
        }
    }

    #endregion

    #region AttendanceListRequestValidator Tests

    public class AttendanceListRequestValidatorTests
    {
        private readonly AttendanceListRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var dto = new AttendanceListRequestDto
            {
                PageNumber = 1,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_PageNumber_Is_Zero()
        {
            var dto = new AttendanceListRequestDto
            {
                PageNumber = 0,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(HRValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Is_Invalid(int pageSize)
        {
            var dto = new AttendanceListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(HRValidationMessages.InvalidPageSize);
        }

        [Fact]
        public void Should_Fail_When_DateTo_Before_DateFrom()
        {
            var dto = new AttendanceListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow.Date,
                DateTo = DateTime.UtcNow.Date.AddDays(-7)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DateTo);
        }
    }

    #endregion

    #region RejectLeaveRequestValidator Tests

    public class RejectLeaveRequestValidatorTests
    {
        private readonly RejectLeaveRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Rejection()
        {
            var dto = new RejectLeaveRequestDto
            {
                LeaveRequestId = 1,
                RejectionReason = "Budget constraints"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_LeaveRequestId_Is_Zero()
        {
            var dto = new RejectLeaveRequestDto
            {
                LeaveRequestId = 0,
                RejectionReason = "Budget constraints"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LeaveRequestId);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_RejectionReason_Is_Empty(string? reason)
        {
            var dto = new RejectLeaveRequestDto
            {
                LeaveRequestId = 1,
                RejectionReason = reason!
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
                .WithErrorMessage(HRValidationMessages.RejectionReasonRequired);
        }
    }

    #endregion

    #region EmployeeListRequestValidator Tests

    public class EmployeeListRequestValidatorTests
    {
        private readonly EmployeeListRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var dto = new EmployeeListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                SearchTerm = "John"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_SearchTerm_Exceeds_MaxLength()
        {
            var dto = new EmployeeListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                SearchTerm = new string('A', 101)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
        }

        [Fact]
        public void Should_Fail_When_DepartmentId_Is_Negative()
        {
            var dto = new EmployeeListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DepartmentId = -1
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
        }
    }

    #endregion
}
