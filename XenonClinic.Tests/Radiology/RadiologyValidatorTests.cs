using FluentValidation.TestHelper;
using Xunit;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Validators;

namespace XenonClinic.Tests.Radiology;

/// <summary>
/// Comprehensive unit tests for Radiology module validators.
/// Tests cover imaging studies, radiology orders, imaging results, and workflow operations.
/// </summary>
public class RadiologyValidatorTests
{
    #region CreateImagingStudyValidator Tests

    public class CreateImagingStudyValidatorTests
    {
        private readonly CreateImagingStudyValidator _validator = new();

        [Fact]
        public void Validate_ValidStudy_ShouldPass()
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "CT-001",
                StudyName = "CT Scan - Chest",
                Description = "Computed tomography scan of the chest",
                Modality = ImagingModality.CT,
                BodyPart = "Chest",
                EstimatedDurationMinutes = 30,
                Price = 500.00m,
                ContrastRequired = "IV Contrast",
                PatientPreparation = "NPO 4 hours prior",
                RequiresContrast = true,
                RequiresFasting = true
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyStudyCode_ShouldFail(string? studyCode)
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = studyCode!,
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StudyCode)
                .WithErrorMessage(RadiologyValidationMessages.StudyCodeRequired);
        }

        [Fact]
        public void Validate_StudyCodeTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = new string('x', 51),
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StudyCode)
                .WithErrorMessage(RadiologyValidationMessages.StudyCodeTooLong);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyStudyName_ShouldFail(string? studyName)
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = studyName!,
                Modality = ImagingModality.XRay,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StudyName)
                .WithErrorMessage(RadiologyValidationMessages.StudyNameRequired);
        }

        [Fact]
        public void Validate_StudyNameTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = new string('x', 201),
                Modality = ImagingModality.XRay,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StudyName)
                .WithErrorMessage(RadiologyValidationMessages.StudyNameTooLong);
        }

        [Fact]
        public void Validate_DescriptionTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Description = new string('x', 1001),
                Modality = ImagingModality.XRay,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_InvalidModality_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Modality = (ImagingModality)999,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Modality)
                .WithErrorMessage(RadiologyValidationMessages.ModalityInvalid);
        }

        [Theory]
        [InlineData(ImagingModality.XRay)]
        [InlineData(ImagingModality.CT)]
        [InlineData(ImagingModality.MRI)]
        [InlineData(ImagingModality.Ultrasound)]
        [InlineData(ImagingModality.Mammography)]
        [InlineData(ImagingModality.PET)]
        public void Validate_ValidModality_ShouldPass(ImagingModality modality)
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "IMG-001",
                StudyName = "Imaging Study",
                Modality = modality,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Modality);
        }

        [Fact]
        public void Validate_BodyPartTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                BodyPart = new string('x', 101),
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BodyPart);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(481)]
        [InlineData(600)]
        public void Validate_InvalidDuration_ShouldFail(int duration)
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                EstimatedDurationMinutes = duration,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EstimatedDurationMinutes)
                .WithErrorMessage(RadiologyValidationMessages.DurationInvalid);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(480)]
        public void Validate_ValidDuration_ShouldPass(int duration)
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                EstimatedDurationMinutes = duration,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.EstimatedDurationMinutes);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100.50)]
        public void Validate_NegativePrice_ShouldFail(decimal price)
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                Price = price
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Price)
                .WithErrorMessage(RadiologyValidationMessages.PriceInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(500.50)]
        public void Validate_ValidPrice_ShouldPass(decimal price)
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                Price = price
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Price);
        }

        [Fact]
        public void Validate_PatientPreparationTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                PatientPreparation = new string('x', 1001),
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatientPreparation);
        }

        [Fact]
        public void Validate_ContraindicationsTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingStudyDto
            {
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                Contraindications = new string('x', 1001),
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Contraindications);
        }
    }

    #endregion

    #region UpdateImagingStudyValidator Tests

    public class UpdateImagingStudyValidatorTests
    {
        private readonly UpdateImagingStudyValidator _validator = new();

        [Fact]
        public void Validate_ValidUpdate_ShouldPass()
        {
            // Arrange
            var dto = new UpdateImagingStudyDto
            {
                Id = 1,
                StudyCode = "CT-001",
                StudyName = "CT Scan - Chest Updated",
                Modality = ImagingModality.CT,
                Price = 550.00m,
                IsActive = true
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidId_ShouldFail(int id)
        {
            // Arrange
            var dto = new UpdateImagingStudyDto
            {
                Id = id,
                StudyCode = "XR-001",
                StudyName = "X-Ray",
                Modality = ImagingModality.XRay,
                Price = 100
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(RadiologyValidationMessages.StudyIdRequired);
        }
    }

    #endregion

    #region ImagingStudyListRequestValidator Tests

    public class ImagingStudyListRequestValidatorTests
    {
        private readonly ImagingStudyListRequestValidator _validator = new();

        [Fact]
        public void Validate_ValidRequest_ShouldPass()
        {
            // Arrange
            var dto = new ImagingStudyListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                Modality = ImagingModality.CT,
                BodyPart = "Chest",
                IsActive = true
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPageNumber_ShouldFail(int pageNumber)
        {
            // Arrange
            var dto = new ImagingStudyListRequestDto
            {
                PageNumber = pageNumber,
                PageSize = 20
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(RadiologyValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidPageSize_ShouldFail(int pageSize)
        {
            // Arrange
            var dto = new ImagingStudyListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(RadiologyValidationMessages.InvalidPageSize);
        }

        [Fact]
        public void Validate_InvalidModality_ShouldFail()
        {
            // Arrange
            var dto = new ImagingStudyListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                Modality = (ImagingModality)999
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Modality)
                .WithErrorMessage(RadiologyValidationMessages.ModalityInvalid);
        }

        [Fact]
        public void Validate_SearchTermTooLong_ShouldFail()
        {
            // Arrange
            var dto = new ImagingStudyListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                SearchTerm = new string('x', 101)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
        }
    }

    #endregion

    #region CreateRadiologyOrderValidator Tests

    public class CreateRadiologyOrderValidatorTests
    {
        private readonly CreateRadiologyOrderValidator _validator = new();

        [Fact]
        public void Validate_ValidOrder_ShouldPass()
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                ReferringDoctorId = 10,
                IsUrgent = true,
                ClinicalHistory = "Patient reports chest pain",
                ClinicalIndication = "Rule out pneumonia",
                Items = new List<CreateRadiologyOrderItemDto>
                {
                    new() { ImagingStudyId = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPatientId_ShouldFail(int patientId)
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = patientId,
                Items = new List<CreateRadiologyOrderItemDto>
                {
                    new() { ImagingStudyId = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatientId)
                .WithErrorMessage(RadiologyValidationMessages.PatientRequired);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidAppointmentId_ShouldFail(int appointmentId)
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                AppointmentId = appointmentId,
                Items = new List<CreateRadiologyOrderItemDto>
                {
                    new() { ImagingStudyId = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidReferringDoctorId_ShouldFail(int doctorId)
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                ReferringDoctorId = doctorId,
                Items = new List<CreateRadiologyOrderItemDto>
                {
                    new() { ImagingStudyId = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ReferringDoctorId);
        }

        [Fact]
        public void Validate_ClinicalHistoryTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                ClinicalHistory = new string('x', 2001),
                Items = new List<CreateRadiologyOrderItemDto>
                {
                    new() { ImagingStudyId = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ClinicalHistory);
        }

        [Fact]
        public void Validate_ClinicalIndicationTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                ClinicalIndication = new string('x', 1001),
                Items = new List<CreateRadiologyOrderItemDto>
                {
                    new() { ImagingStudyId = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ClinicalIndication);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                Notes = new string('x', 1001),
                Items = new List<CreateRadiologyOrderItemDto>
                {
                    new() { ImagingStudyId = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(150)]
        public void Validate_InvalidDiscountPercentage_ShouldFail(decimal discount)
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                DiscountPercentage = discount,
                Items = new List<CreateRadiologyOrderItemDto>
                {
                    new() { ImagingStudyId = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage);
        }

        [Fact]
        public void Validate_EmptyItems_ShouldFail()
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                Items = new List<CreateRadiologyOrderItemDto>()
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items)
                .WithErrorMessage(RadiologyValidationMessages.OrderItemsRequired);
        }

        [Fact]
        public void Validate_TooManyItems_ShouldFail()
        {
            // Arrange
            var items = Enumerable.Range(1, 51)
                .Select(i => new CreateRadiologyOrderItemDto { ImagingStudyId = i })
                .ToList();

            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                Items = items
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items)
                .WithErrorMessage("Order cannot have more than 50 studies");
        }

        [Fact]
        public void Validate_InvalidItemInItems_ShouldFail()
        {
            // Arrange
            var dto = new CreateRadiologyOrderDto
            {
                PatientId = 1,
                Items = new List<CreateRadiologyOrderItemDto>
                {
                    new() { ImagingStudyId = 0 } // Invalid study ID
                }
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor("Items[0].ImagingStudyId");
        }
    }

    #endregion

    #region CreateRadiologyOrderItemValidator Tests

    public class CreateRadiologyOrderItemValidatorTests
    {
        private readonly CreateRadiologyOrderItemValidator _validator = new();

        [Fact]
        public void Validate_ValidItem_ShouldPass()
        {
            // Arrange
            var dto = new CreateRadiologyOrderItemDto
            {
                ImagingStudyId = 1,
                DiscountAmount = 50,
                Notes = "Patient anxious - may need sedation",
                SpecialInstructions = "Contact isolation precautions"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidImagingStudyId_ShouldFail(int studyId)
        {
            // Arrange
            var dto = new CreateRadiologyOrderItemDto
            {
                ImagingStudyId = studyId
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ImagingStudyId)
                .WithErrorMessage(RadiologyValidationMessages.StudyIdRequired);
        }

        [Fact]
        public void Validate_NegativeDiscountAmount_ShouldFail()
        {
            // Arrange
            var dto = new CreateRadiologyOrderItemDto
            {
                ImagingStudyId = 1,
                DiscountAmount = -50
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DiscountAmount);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateRadiologyOrderItemDto
            {
                ImagingStudyId = 1,
                Notes = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }

        [Fact]
        public void Validate_SpecialInstructionsTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateRadiologyOrderItemDto
            {
                ImagingStudyId = 1,
                SpecialInstructions = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SpecialInstructions);
        }
    }

    #endregion

    #region UpdateRadiologyOrderValidator Tests

    public class UpdateRadiologyOrderValidatorTests
    {
        private readonly UpdateRadiologyOrderValidator _validator = new();

        [Fact]
        public void Validate_ValidUpdate_ShouldPass()
        {
            // Arrange
            var dto = new UpdateRadiologyOrderDto
            {
                Id = 1,
                ReferringDoctorId = 10,
                IsUrgent = true,
                ClinicalHistory = "Updated clinical history",
                ClinicalIndication = "Updated indication",
                Notes = "Updated notes"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidId_ShouldFail(int id)
        {
            // Arrange
            var dto = new UpdateRadiologyOrderDto { Id = id };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(RadiologyValidationMessages.OrderIdRequired);
        }
    }

    #endregion

    #region RadiologyOrderListRequestValidator Tests

    public class RadiologyOrderListRequestValidatorTests
    {
        private readonly RadiologyOrderListRequestValidator _validator = new();

        [Fact]
        public void Validate_ValidRequest_ShouldPass()
        {
            // Arrange
            var dto = new RadiologyOrderListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                PatientId = 1,
                Status = RadiologyOrderStatus.Pending,
                DateFrom = DateTime.UtcNow.AddDays(-30),
                DateTo = DateTime.UtcNow,
                IsUrgent = true
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPageNumber_ShouldFail(int pageNumber)
        {
            // Arrange
            var dto = new RadiologyOrderListRequestDto
            {
                PageNumber = pageNumber,
                PageSize = 20
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage(RadiologyValidationMessages.InvalidPageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_InvalidPageSize_ShouldFail(int pageSize)
        {
            // Arrange
            var dto = new RadiologyOrderListRequestDto
            {
                PageNumber = 1,
                PageSize = pageSize
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage(RadiologyValidationMessages.InvalidPageSize);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidPatientId_ShouldFail(int patientId)
        {
            // Arrange
            var dto = new RadiologyOrderListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                PatientId = patientId
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatientId)
                .WithErrorMessage(RadiologyValidationMessages.PatientInvalid);
        }

        [Fact]
        public void Validate_InvalidStatus_ShouldFail()
        {
            // Arrange
            var dto = new RadiologyOrderListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                Status = (RadiologyOrderStatus)999
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Status)
                .WithErrorMessage(RadiologyValidationMessages.OrderStatusInvalid);
        }

        [Fact]
        public void Validate_InvalidDateRange_ShouldFail()
        {
            // Arrange
            var dto = new RadiologyOrderListRequestDto
            {
                PageNumber = 1,
                PageSize = 20,
                DateFrom = DateTime.UtcNow,
                DateTo = DateTime.UtcNow.AddDays(-10)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DateTo)
                .WithErrorMessage(RadiologyValidationMessages.DateRangeInvalid);
        }
    }

    #endregion

    #region CreateImagingResultValidator Tests

    public class CreateImagingResultValidatorTests
    {
        private readonly CreateImagingResultValidator _validator = new();

        [Fact]
        public void Validate_ValidResult_ShouldPass()
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                Findings = "No acute abnormality detected",
                Impression = "Normal chest radiograph",
                Recommendation = "No follow-up needed",
                Technique = "PA and Lateral views",
                NumberOfImages = 2
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidRadiologyOrderId_ShouldFail(int orderId)
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = orderId,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RadiologyOrderId)
                .WithErrorMessage(RadiologyValidationMessages.OrderIdRequired);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidRadiologyOrderItemId_ShouldFail(int itemId)
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = itemId,
                ImagingStudyId = 1
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RadiologyOrderItemId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidImagingStudyId_ShouldFail(int studyId)
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = studyId
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ImagingStudyId)
                .WithErrorMessage(RadiologyValidationMessages.StudyIdRequired);
        }

        [Fact]
        public void Validate_FindingsTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                Findings = new string('x', 5001)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Findings);
        }

        [Fact]
        public void Validate_ImpressionTooLong_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                Impression = new string('x', 2001)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Impression);
        }

        [Fact]
        public void Validate_CriticalWithoutCriticalFindings_ShouldFail()
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                IsCritical = true,
                CriticalFindings = "" // Empty when marked critical
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CriticalFindings)
                .WithErrorMessage(RadiologyValidationMessages.CriticalFindingsRequired);
        }

        [Fact]
        public void Validate_CriticalWithCriticalFindings_ShouldPass()
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                IsCritical = true,
                CriticalFindings = "Large pleural effusion with mediastinal shift"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.CriticalFindings);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("1.2.3.abc")]
        [InlineData("!@#$%")]
        public void Validate_InvalidDicomUID_ShouldFail(string dicomUid)
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                DicomStudyUID = dicomUid
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DicomStudyUID)
                .WithErrorMessage(RadiologyValidationMessages.DicomUIDInvalid);
        }

        [Theory]
        [InlineData("1.2.840.10008.1.2")]
        [InlineData("2.16.840.1.113883.3.101")]
        [InlineData("1.2.3.4.5.6.7.8.9.0")]
        public void Validate_ValidDicomUID_ShouldPass(string dicomUid)
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                DicomStudyUID = dicomUid
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.DicomStudyUID);
        }

        [Theory]
        [InlineData("invalid-url")]
        [InlineData("not a url")]
        [InlineData("ftp://example.com")]
        public void Validate_InvalidPacsLink_ShouldFail(string pacsLink)
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                PacsLink = pacsLink
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PacsLink)
                .WithErrorMessage(RadiologyValidationMessages.PacsLinkInvalid);
        }

        [Theory]
        [InlineData("http://pacs.hospital.com/study/123")]
        [InlineData("https://pacs.hospital.com/viewer?studyUID=1.2.3")]
        public void Validate_ValidPacsLink_ShouldPass(string pacsLink)
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                PacsLink = pacsLink
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.PacsLink);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidNumberOfImages_ShouldFail(int numberOfImages)
        {
            // Arrange
            var dto = new CreateImagingResultDto
            {
                RadiologyOrderId = 1,
                RadiologyOrderItemId = 1,
                ImagingStudyId = 1,
                NumberOfImages = numberOfImages
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NumberOfImages);
        }
    }

    #endregion

    #region UpdateImagingResultValidator Tests

    public class UpdateImagingResultValidatorTests
    {
        private readonly UpdateImagingResultValidator _validator = new();

        [Fact]
        public void Validate_ValidUpdate_ShouldPass()
        {
            // Arrange
            var dto = new UpdateImagingResultDto
            {
                Id = 1,
                Findings = "Updated findings",
                Impression = "Updated impression"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidId_ShouldFail(int id)
        {
            // Arrange
            var dto = new UpdateImagingResultDto { Id = id };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage(RadiologyValidationMessages.ResultIdRequired);
        }
    }

    #endregion

    #region Workflow Validator Tests

    public class ReceiveRadiologyOrderValidatorTests
    {
        private readonly ReceiveRadiologyOrderValidator _validator = new();

        [Fact]
        public void Validate_ValidReceive_ShouldPass()
        {
            // Arrange
            var dto = new ReceiveRadiologyOrderDto
            {
                OrderId = 1,
                Notes = "Patient arrived"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidOrderId_ShouldFail(int orderId)
        {
            // Arrange
            var dto = new ReceiveRadiologyOrderDto { OrderId = orderId };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OrderId)
                .WithErrorMessage(RadiologyValidationMessages.OrderIdRequired);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new ReceiveRadiologyOrderDto
            {
                OrderId = 1,
                Notes = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    public class StartImagingValidatorTests
    {
        private readonly StartImagingValidator _validator = new();

        [Fact]
        public void Validate_ValidStart_ShouldPass()
        {
            // Arrange
            var dto = new StartImagingDto
            {
                OrderId = 1,
                Technician = "John Smith",
                Room = "CT-1",
                Equipment = "GE Revolution CT"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidOrderId_ShouldFail(int orderId)
        {
            // Arrange
            var dto = new StartImagingDto { OrderId = orderId };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OrderId)
                .WithErrorMessage(RadiologyValidationMessages.OrderIdRequired);
        }

        [Fact]
        public void Validate_TechnicianTooLong_ShouldFail()
        {
            // Arrange
            var dto = new StartImagingDto
            {
                OrderId = 1,
                Technician = new string('x', 201)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Technician);
        }

        [Fact]
        public void Validate_RoomTooLong_ShouldFail()
        {
            // Arrange
            var dto = new StartImagingDto
            {
                OrderId = 1,
                Room = new string('x', 51)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Room);
        }

        [Fact]
        public void Validate_EquipmentTooLong_ShouldFail()
        {
            // Arrange
            var dto = new StartImagingDto
            {
                OrderId = 1,
                Equipment = new string('x', 101)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Equipment);
        }
    }

    public class CompleteRadiologyOrderValidatorTests
    {
        private readonly CompleteRadiologyOrderValidator _validator = new();

        [Fact]
        public void Validate_ValidComplete_ShouldPass()
        {
            // Arrange
            var dto = new CompleteRadiologyOrderDto
            {
                OrderId = 1,
                CompletedBy = "Tech123",
                Notes = "All images acquired successfully"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidOrderId_ShouldFail(int orderId)
        {
            // Arrange
            var dto = new CompleteRadiologyOrderDto { OrderId = orderId };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OrderId)
                .WithErrorMessage(RadiologyValidationMessages.OrderIdRequired);
        }
    }

    public class ApproveRadiologyOrderValidatorTests
    {
        private readonly ApproveRadiologyOrderValidator _validator = new();

        [Fact]
        public void Validate_ValidApprove_ShouldPass()
        {
            // Arrange
            var dto = new ApproveRadiologyOrderDto
            {
                OrderId = 1,
                ApprovedBy = "Dr. Smith",
                Notes = "Report reviewed and approved"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidOrderId_ShouldFail(int orderId)
        {
            // Arrange
            var dto = new ApproveRadiologyOrderDto { OrderId = orderId };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OrderId)
                .WithErrorMessage(RadiologyValidationMessages.OrderIdRequired);
        }
    }

    public class RejectRadiologyOrderValidatorTests
    {
        private readonly RejectRadiologyOrderValidator _validator = new();

        [Fact]
        public void Validate_ValidReject_ShouldPass()
        {
            // Arrange
            var dto = new RejectRadiologyOrderDto
            {
                OrderId = 1,
                RejectionReason = "Patient not properly prepared"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidOrderId_ShouldFail(int orderId)
        {
            // Arrange
            var dto = new RejectRadiologyOrderDto
            {
                OrderId = orderId,
                RejectionReason = "Some reason"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OrderId)
                .WithErrorMessage(RadiologyValidationMessages.OrderIdRequired);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyRejectionReason_ShouldFail(string? reason)
        {
            // Arrange
            var dto = new RejectRadiologyOrderDto
            {
                OrderId = 1,
                RejectionReason = reason!
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RejectionReason)
                .WithErrorMessage(RadiologyValidationMessages.RejectionReasonRequired);
        }

        [Fact]
        public void Validate_RejectionReasonTooLong_ShouldFail()
        {
            // Arrange
            var dto = new RejectRadiologyOrderDto
            {
                OrderId = 1,
                RejectionReason = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RejectionReason);
        }
    }

    public class AddImagingReportValidatorTests
    {
        private readonly AddImagingReportValidator _validator = new();

        [Fact]
        public void Validate_ValidReport_ShouldPass()
        {
            // Arrange
            var dto = new AddImagingReportDto
            {
                ResultId = 1,
                Findings = "The heart size is normal. The lungs are clear.",
                Impression = "Normal chest radiograph",
                Recommendation = "No follow-up needed",
                Technique = "PA and lateral views obtained"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidResultId_ShouldFail(int resultId)
        {
            // Arrange
            var dto = new AddImagingReportDto
            {
                ResultId = resultId,
                Findings = "Findings",
                Impression = "Impression"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ResultId)
                .WithErrorMessage(RadiologyValidationMessages.ResultIdRequired);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyFindings_ShouldFail(string? findings)
        {
            // Arrange
            var dto = new AddImagingReportDto
            {
                ResultId = 1,
                Findings = findings!,
                Impression = "Impression"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Findings)
                .WithErrorMessage(RadiologyValidationMessages.FindingsRequired);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyImpression_ShouldFail(string? impression)
        {
            // Arrange
            var dto = new AddImagingReportDto
            {
                ResultId = 1,
                Findings = "Findings",
                Impression = impression!
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Impression)
                .WithErrorMessage(RadiologyValidationMessages.ImpressionRequired);
        }

        [Fact]
        public void Validate_CriticalWithoutCriticalFindings_ShouldFail()
        {
            // Arrange
            var dto = new AddImagingReportDto
            {
                ResultId = 1,
                Findings = "Findings",
                Impression = "Impression",
                IsCritical = true,
                CriticalFindings = ""
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CriticalFindings)
                .WithErrorMessage(RadiologyValidationMessages.CriticalFindingsRequired);
        }
    }

    public class VerifyImagingResultValidatorTests
    {
        private readonly VerifyImagingResultValidator _validator = new();

        [Fact]
        public void Validate_ValidVerify_ShouldPass()
        {
            // Arrange
            var dto = new VerifyImagingResultDto
            {
                ResultId = 1,
                VerifiedBy = "Dr. Jones",
                Notes = "Verified and finalized"
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_InvalidResultId_ShouldFail(int resultId)
        {
            // Arrange
            var dto = new VerifyImagingResultDto { ResultId = resultId };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ResultId)
                .WithErrorMessage(RadiologyValidationMessages.ResultIdRequired);
        }

        [Fact]
        public void Validate_VerifiedByTooLong_ShouldFail()
        {
            // Arrange
            var dto = new VerifyImagingResultDto
            {
                ResultId = 1,
                VerifiedBy = new string('x', 201)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.VerifiedBy);
        }

        [Fact]
        public void Validate_NotesTooLong_ShouldFail()
        {
            // Arrange
            var dto = new VerifyImagingResultDto
            {
                ResultId = 1,
                Notes = new string('x', 501)
            };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    #endregion
}
