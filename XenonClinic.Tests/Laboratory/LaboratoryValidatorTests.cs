using FluentAssertions;
using FluentValidation.TestHelper;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Validators;
using Xunit;

namespace XenonClinic.Tests.Laboratory;

public class LaboratoryValidatorTests
{
    #region CreateLabTestValidator Tests

    public class CreateLabTestValidatorTests
    {
        private readonly CreateLabTestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_LabTest()
        {
            var dto = CreateValidLabTestDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_TestCode_Is_Empty(string? code)
        {
            var dto = CreateValidLabTestDto();
            dto.TestCode = code!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TestCode)
                .WithErrorMessage(LabValidationMessages.TestCodeRequired);
        }

        [Fact]
        public void Should_Fail_When_TestCode_Exceeds_MaxLength()
        {
            var dto = CreateValidLabTestDto();
            dto.TestCode = new string('A', 51);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TestCode)
                .WithErrorMessage(LabValidationMessages.TestCodeTooLong);
        }

        [Theory]
        [InlineData("TEST@123")]
        [InlineData("TEST 123")]
        [InlineData("TEST#CODE")]
        public void Should_Fail_When_TestCode_Format_Is_Invalid(string code)
        {
            var dto = CreateValidLabTestDto();
            dto.TestCode = code;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TestCode)
                .WithErrorMessage(LabValidationMessages.TestCodeInvalid);
        }

        [Theory]
        [InlineData("CBC")]
        [InlineData("TEST-001")]
        [InlineData("LAB_TEST")]
        public void Should_Pass_When_TestCode_Format_Is_Valid(string code)
        {
            var dto = CreateValidLabTestDto();
            dto.TestCode = code;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.TestCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_TestName_Is_Empty(string? name)
        {
            var dto = CreateValidLabTestDto();
            dto.TestName = name!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TestName)
                .WithErrorMessage(LabValidationMessages.TestNameRequired);
        }

        [Fact]
        public void Should_Fail_When_TestName_Exceeds_MaxLength()
        {
            var dto = CreateValidLabTestDto();
            dto.TestName = new string('A', 201);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TestName)
                .WithErrorMessage(LabValidationMessages.TestNameTooLong);
        }

        [Fact]
        public void Should_Fail_When_Category_Is_Invalid()
        {
            var dto = CreateValidLabTestDto();
            dto.Category = (TestCategory)999;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Category)
                .WithErrorMessage(LabValidationMessages.CategoryInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Should_Fail_When_Price_Is_Invalid(decimal price)
        {
            var dto = CreateValidLabTestDto();
            dto.Price = price;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Price)
                .WithErrorMessage(LabValidationMessages.PriceInvalid);
        }

        [Fact]
        public void Should_Fail_When_TurnaroundTimeHours_Is_Zero()
        {
            var dto = CreateValidLabTestDto();
            dto.TurnaroundTimeHours = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TurnaroundTimeHours)
                .WithErrorMessage(LabValidationMessages.TurnaroundTimeInvalid);
        }

        [Fact]
        public void Should_Pass_When_TurnaroundTimeHours_Is_Null()
        {
            var dto = CreateValidLabTestDto();
            dto.TurnaroundTimeHours = null;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.TurnaroundTimeHours);
        }

        [Fact]
        public void Should_Fail_When_ExternalLabId_Is_Invalid()
        {
            var dto = CreateValidLabTestDto();
            dto.ExternalLabId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExternalLabId);
        }

        private static CreateLabTestDto CreateValidLabTestDto()
        {
            return new CreateLabTestDto
            {
                TestCode = "CBC-001",
                TestName = "Complete Blood Count",
                Description = "Comprehensive blood analysis",
                Category = TestCategory.Hematology,
                SpecimenType = SpecimenType.Blood,
                SpecimenVolume = "5ml",
                TurnaroundTimeHours = 24,
                Price = 150,
                Unit = "count/uL"
            };
        }
    }

    #endregion

    #region UpdateLabTestValidator Tests

    public class UpdateLabTestValidatorTests
    {
        private readonly UpdateLabTestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Update()
        {
            var dto = new UpdateLabTestDto
            {
                Id = 1,
                TestCode = "CBC-001",
                TestName = "Complete Blood Count",
                Category = TestCategory.Hematology,
                Price = 150,
                IsActive = true
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Zero()
        {
            var dto = new UpdateLabTestDto
            {
                Id = 0,
                TestCode = "CBC",
                TestName = "Test",
                Category = TestCategory.Hematology,
                Price = 100
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }

    #endregion

    #region CreateLabOrderValidator Tests

    public class CreateLabOrderValidatorTests
    {
        private readonly CreateLabOrderValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_LabOrder()
        {
            var dto = CreateValidLabOrderDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_PatientId_Is_Zero()
        {
            var dto = CreateValidLabOrderDto();
            dto.PatientId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PatientId)
                .WithErrorMessage(LabValidationMessages.PatientRequired);
        }

        [Fact]
        public void Should_Fail_When_Items_Is_Empty()
        {
            var dto = CreateValidLabOrderDto();
            dto.Items = new List<CreateLabOrderItemDto>();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Items)
                .WithErrorMessage(LabValidationMessages.OrderItemsRequired);
        }

        [Fact]
        public void Should_Fail_When_Items_Exceeds_Maximum()
        {
            var dto = CreateValidLabOrderDto();
            dto.Items = Enumerable.Range(1, 51).Select(i => new CreateLabOrderItemDto { LabTestId = i }).ToList();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Items);
        }

        [Fact]
        public void Should_Fail_When_ExternalLabId_Is_Invalid()
        {
            var dto = CreateValidLabOrderDto();
            dto.ExternalLabId = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ExternalLabId);
        }

        [Fact]
        public void Should_Fail_When_ClinicalNotes_Exceeds_MaxLength()
        {
            var dto = CreateValidLabOrderDto();
            dto.ClinicalNotes = new string('A', 2001);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ClinicalNotes);
        }

        private static CreateLabOrderDto CreateValidLabOrderDto()
        {
            return new CreateLabOrderDto
            {
                PatientId = 1,
                IsUrgent = false,
                ClinicalNotes = "Routine checkup",
                Items = new List<CreateLabOrderItemDto>
                {
                    new() { LabTestId = 1, Notes = "Fasting" }
                }
            };
        }
    }

    #endregion

    #region CreateLabOrderItemValidator Tests

    public class CreateLabOrderItemValidatorTests
    {
        private readonly CreateLabOrderItemValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_OrderItem()
        {
            var dto = new CreateLabOrderItemDto
            {
                LabTestId = 1,
                Notes = "Patient fasting"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_LabTestId_Is_Zero()
        {
            var dto = new CreateLabOrderItemDto
            {
                LabTestId = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LabTestId);
        }

        [Fact]
        public void Should_Fail_When_Notes_Exceeds_MaxLength()
        {
            var dto = new CreateLabOrderItemDto
            {
                LabTestId = 1,
                Notes = new string('A', 501)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    #endregion

    #region UpdateLabOrderStatusValidator Tests

    public class UpdateLabOrderStatusValidatorTests
    {
        private readonly UpdateLabOrderStatusValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_StatusUpdate()
        {
            var dto = new UpdateLabOrderStatusDto
            {
                LabOrderId = 1,
                Status = LabOrderStatus.InProgress
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_LabOrderId_Is_Zero()
        {
            var dto = new UpdateLabOrderStatusDto
            {
                LabOrderId = 0,
                Status = LabOrderStatus.InProgress
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LabOrderId)
                .WithErrorMessage(LabValidationMessages.OrderIdRequired);
        }

        [Fact]
        public void Should_Fail_When_Status_Is_Invalid()
        {
            var dto = new UpdateLabOrderStatusDto
            {
                LabOrderId = 1,
                Status = (LabOrderStatus)999
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage(LabValidationMessages.OrderStatusInvalid);
        }
    }

    #endregion

    #region CollectSamplesValidator Tests

    public class CollectSamplesValidatorTests
    {
        private readonly CollectSamplesValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Collection()
        {
            var dto = new CollectSamplesDto
            {
                LabOrderId = 1,
                CollectionDate = DateTime.UtcNow.AddMinutes(-5)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_LabOrderId_Is_Zero()
        {
            var dto = new CollectSamplesDto
            {
                LabOrderId = 0,
                CollectionDate = DateTime.UtcNow
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LabOrderId)
                .WithErrorMessage(LabValidationMessages.OrderIdRequired);
        }

        [Fact]
        public void Should_Fail_When_CollectionDate_Is_In_Future()
        {
            var dto = new CollectSamplesDto
            {
                LabOrderId = 1,
                CollectionDate = DateTime.UtcNow.AddHours(1)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.CollectionDate)
                .WithErrorMessage(LabValidationMessages.CollectionDateFuture);
        }
    }

    #endregion

    #region LabOrderListRequestValidator Tests

    public class LabOrderListRequestValidatorTests
    {
        private readonly LabOrderListRequestValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var dto = new LabOrderListRequestDto
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
            var dto = new LabOrderListRequestDto
            {
                PageNumber = 0,
                PageSize = 20
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(LabValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Is_Invalid(int pageSize)
        {
            var dto = new LabOrderListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(LabValidationMessages.InvalidPageSize);
        }

        [Fact]
        public void Should_Fail_When_DateTo_Before_DateFrom()
        {
            var dto = new LabOrderListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow.Date,
                DateTo = DateTime.UtcNow.Date.AddDays(-7)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DateTo)
                .WithErrorMessage(LabValidationMessages.DateRangeInvalid);
        }
    }

    #endregion

    #region EnterLabResultValidator Tests

    public class EnterLabResultValidatorTests
    {
        private readonly EnterLabResultValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Result()
        {
            var dto = new EnterLabResultDto
            {
                LabOrderItemId = 1,
                ResultValue = "5.5",
                Unit = "mmol/L",
                ReferenceRange = "4.0-6.0"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_LabOrderItemId_Is_Zero()
        {
            var dto = new EnterLabResultDto
            {
                LabOrderItemId = 0,
                ResultValue = "5.5"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LabOrderItemId)
                .WithErrorMessage(LabValidationMessages.LabOrderItemRequired);
        }

        [Fact]
        public void Should_Fail_When_ResultValue_Exceeds_MaxLength()
        {
            var dto = new EnterLabResultDto
            {
                LabOrderItemId = 1,
                ResultValue = new string('A', 101)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.ResultValue);
        }

        [Fact]
        public void Should_Fail_When_Interpretation_Exceeds_MaxLength()
        {
            var dto = new EnterLabResultDto
            {
                LabOrderItemId = 1,
                Interpretation = new string('A', 2001)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Interpretation);
        }
    }

    #endregion

    #region ReviewLabResultValidator Tests

    public class ReviewLabResultValidatorTests
    {
        private readonly ReviewLabResultValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Review()
        {
            var dto = new ReviewLabResultDto
            {
                LabResultId = 1,
                Interpretation = "Normal results",
                IsAbnormal = false
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_LabResultId_Is_Zero()
        {
            var dto = new ReviewLabResultDto
            {
                LabResultId = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LabResultId)
                .WithErrorMessage(LabValidationMessages.ResultIdRequired);
        }
    }

    #endregion

    #region VerifyLabResultValidator Tests

    public class VerifyLabResultValidatorTests
    {
        private readonly VerifyLabResultValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Verification()
        {
            var dto = new VerifyLabResultDto
            {
                LabResultId = 1,
                Notes = "Verified and approved"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_LabResultId_Is_Zero()
        {
            var dto = new VerifyLabResultDto
            {
                LabResultId = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.LabResultId)
                .WithErrorMessage(LabValidationMessages.ResultIdRequired);
        }

        [Fact]
        public void Should_Fail_When_Notes_Exceeds_MaxLength()
        {
            var dto = new VerifyLabResultDto
            {
                LabResultId = 1,
                Notes = new string('A', 1001)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    #endregion

    #region CreateExternalLabValidator Tests

    public class CreateExternalLabValidatorTests
    {
        private readonly CreateExternalLabValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_ExternalLab()
        {
            var dto = new CreateExternalLabDto
            {
                Name = "Central Diagnostics Lab",
                Code = "CDL001",
                ContactPerson = "Dr. Smith",
                Email = "contact@cdl.com",
                Phone = "+971-4-123-4567"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Fail_When_Name_Is_Empty(string? name)
        {
            var dto = new CreateExternalLabDto
            {
                Name = name!
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(LabValidationMessages.LabNameRequired);
        }

        [Fact]
        public void Should_Fail_When_Name_Exceeds_MaxLength()
        {
            var dto = new CreateExternalLabDto
            {
                Name = new string('A', 101)
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(LabValidationMessages.LabNameTooLong);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        public void Should_Fail_When_Email_Is_Invalid(string email)
        {
            var dto = new CreateExternalLabDto
            {
                Name = "Test Lab",
                Email = email
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage(LabValidationMessages.EmailInvalid);
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("contact@lab.ae")]
        public void Should_Pass_When_Email_Is_Valid(string email)
        {
            var dto = new CreateExternalLabDto
            {
                Name = "Test Lab",
                Email = email
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("phone@")]
        public void Should_Fail_When_Phone_Format_Is_Invalid(string phone)
        {
            var dto = new CreateExternalLabDto
            {
                Name = "Test Lab",
                Phone = phone
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Phone)
                .WithErrorMessage(LabValidationMessages.PhoneInvalid);
        }

        [Theory]
        [InlineData("+971-4-123-4567")]
        [InlineData("04-123-4567")]
        [InlineData("971501234567")]
        public void Should_Pass_When_Phone_Format_Is_Valid(string phone)
        {
            var dto = new CreateExternalLabDto
            {
                Name = "Test Lab",
                Phone = phone
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Phone);
        }

        [Fact]
        public void Should_Fail_When_TurnaroundTimeDays_Is_Zero()
        {
            var dto = new CreateExternalLabDto
            {
                Name = "Test Lab",
                TurnaroundTimeDays = 0
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.TurnaroundTimeDays);
        }
    }

    #endregion

    #region UpdateExternalLabValidator Tests

    public class UpdateExternalLabValidatorTests
    {
        private readonly UpdateExternalLabValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Update()
        {
            var dto = new UpdateExternalLabDto
            {
                Id = 1,
                Name = "Updated Lab Name",
                Email = "updated@lab.com",
                IsActive = true
            };

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Zero()
        {
            var dto = new UpdateExternalLabDto
            {
                Id = 0,
                Name = "Lab"
            };

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }

    #endregion
}
