using FluentAssertions;
using FluentValidation.TestHelper;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Validators;
using Xunit;

namespace XenonClinic.Tests.Appointments;

/// <summary>
/// Tests for appointment DTO validators.
/// </summary>
public class AppointmentValidatorTests
{
    #region CreateAppointmentValidator Tests

    private readonly CreateAppointmentValidator _createValidator = new();

    [Fact]
    public void CreateValidator_ValidAppointment_PassesValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateValidator_MissingPatientId_FailsValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var dto = new CreateAppointmentDto
        {
            PatientId = 0,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Fact]
    public void CreateValidator_PastStartTime_FailsValidation()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            StartTime = pastDate,
            EndTime = pastDate.AddMinutes(30),
            Type = AppointmentType.Consultation
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void CreateValidator_EndTimeBeforeStartTime_FailsValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(-30), // Before start
            Type = AppointmentType.Consultation
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void CreateValidator_TooShortDuration_FailsValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(2), // Only 2 minutes
            Type = AppointmentType.Consultation
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void CreateValidator_TooLongDuration_FailsValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddHours(10), // 10 hours - exceeds 8 hour limit
            Type = AppointmentType.Consultation
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void CreateValidator_NotesTooLong_FailsValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation,
            Notes = new string('x', 2001)
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void CreateValidator_SameDayAppointment_PassesValidation()
    {
        // Arrange - Allow same day scheduling
        var today = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour + 2);
        var dto = new CreateAppointmentDto
        {
            PatientId = 1,
            StartTime = today,
            EndTime = today.AddMinutes(30),
            Type = AppointmentType.Emergency // Emergency on same day
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
    }

    #endregion

    #region UpdateAppointmentValidator Tests

    private readonly UpdateAppointmentValidator _updateValidator = new();

    [Fact]
    public void UpdateValidator_ValidAppointment_PassesValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var dto = new UpdateAppointmentDto
        {
            Id = 1,
            PatientId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation
        };

        // Act
        var result = _updateValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateValidator_InvalidId_FailsValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var dto = new UpdateAppointmentDto
        {
            Id = 0,
            PatientId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation
        };

        // Act
        var result = _updateValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    #endregion

    #region RescheduleAppointmentValidator Tests

    private readonly RescheduleAppointmentValidator _rescheduleValidator = new();

    [Fact]
    public void RescheduleValidator_ValidReschedule_PassesValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(2).AddHours(14);
        var dto = new RescheduleAppointmentDto
        {
            AppointmentId = 1,
            NewStartTime = futureDate,
            NewEndTime = futureDate.AddMinutes(30),
            Reason = "Patient request"
        };

        // Act
        var result = _rescheduleValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RescheduleValidator_PastTime_FailsValidation()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var dto = new RescheduleAppointmentDto
        {
            AppointmentId = 1,
            NewStartTime = pastDate,
            NewEndTime = pastDate.AddMinutes(30)
        };

        // Act
        var result = _rescheduleValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewStartTime);
    }

    [Fact]
    public void RescheduleValidator_ReasonTooLong_FailsValidation()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(2).AddHours(14);
        var dto = new RescheduleAppointmentDto
        {
            AppointmentId = 1,
            NewStartTime = futureDate,
            NewEndTime = futureDate.AddMinutes(30),
            Reason = new string('x', 501)
        };

        // Act
        var result = _rescheduleValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    #endregion

    #region CancelAppointmentValidator Tests

    private readonly CancelAppointmentValidator _cancelValidator = new();

    [Fact]
    public void CancelValidator_ValidCancel_PassesValidation()
    {
        // Arrange
        var dto = new CancelAppointmentDto
        {
            AppointmentId = 1,
            Reason = "Patient request"
        };

        // Act
        var result = _cancelValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CancelValidator_InvalidId_FailsValidation()
    {
        // Arrange
        var dto = new CancelAppointmentDto
        {
            AppointmentId = 0
        };

        // Act
        var result = _cancelValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    #endregion

    #region AppointmentListRequestValidator Tests

    private readonly AppointmentListRequestValidator _listValidator = new();

    [Fact]
    public void ListValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new AppointmentListRequestDto
        {
            PageNumber = 1,
            PageSize = 20
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ListValidator_InvalidPageNumber_FailsValidation()
    {
        // Arrange
        var dto = new AppointmentListRequestDto
        {
            PageNumber = 0,
            PageSize = 20
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Fact]
    public void ListValidator_PageSizeTooLarge_FailsValidation()
    {
        // Arrange
        var dto = new AppointmentListRequestDto
        {
            PageNumber = 1,
            PageSize = 101
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void ListValidator_DateRangeInvalid_FailsValidation()
    {
        // Arrange
        var dto = new AppointmentListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            DateFrom = DateTime.UtcNow,
            DateTo = DateTime.UtcNow.AddDays(-7) // End before start
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void ListValidator_DateRangeTooLarge_FailsValidation()
    {
        // Arrange
        var dto = new AppointmentListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            DateFrom = DateTime.UtcNow,
            DateTo = DateTime.UtcNow.AddDays(100) // 100 days - exceeds 90 day limit
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void ListValidator_ValidDateRange_PassesValidation()
    {
        // Arrange
        var dto = new AppointmentListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            DateFrom = DateTime.UtcNow,
            DateTo = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationError();
    }

    [Theory]
    [InlineData("StartTime")]
    [InlineData("PatientName")]
    [InlineData("ProviderName")]
    [InlineData("Status")]
    [InlineData("Type")]
    public void ListValidator_ValidSortField_PassesValidation(string sortBy)
    {
        // Arrange
        var dto = new AppointmentListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = sortBy
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Fact]
    public void ListValidator_InvalidSortField_FailsValidation()
    {
        // Arrange
        var dto = new AppointmentListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = "InvalidField"
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    #endregion

    #region AvailabilityRequestValidator Tests

    private readonly AvailabilityRequestValidator _availabilityValidator = new();

    [Fact]
    public void AvailabilityValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new AvailabilityRequestDto
        {
            Date = DateTime.UtcNow.Date.AddDays(1),
            DurationMinutes = 30
        };

        // Act
        var result = _availabilityValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AvailabilityValidator_PastDate_FailsValidation()
    {
        // Arrange
        var dto = new AvailabilityRequestDto
        {
            Date = DateTime.UtcNow.Date.AddDays(-1),
            DurationMinutes = 30
        };

        // Act
        var result = _availabilityValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void AvailabilityValidator_InvalidDuration_FailsValidation()
    {
        // Arrange
        var dto = new AvailabilityRequestDto
        {
            Date = DateTime.UtcNow.Date.AddDays(1),
            DurationMinutes = 2 // Too short
        };

        // Act
        var result = _availabilityValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void AvailabilityValidator_TodayDate_PassesValidation()
    {
        // Arrange - Today should be valid
        var dto = new AvailabilityRequestDto
        {
            Date = DateTime.UtcNow.Date,
            DurationMinutes = 30
        };

        // Act
        var result = _availabilityValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Date);
    }

    #endregion
}
