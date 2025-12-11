using FluentAssertions;
using FluentValidation.TestHelper;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Validators;
using Xunit;

namespace XenonClinic.Tests.ClinicalVisits;

/// <summary>
/// Tests for clinical visit validators.
/// </summary>
public class ClinicalVisitValidatorTests
{
    #region CreateAudiologyVisitValidator Tests

    private readonly CreateAudiologyVisitValidator _audiologyValidator = new();

    [Fact]
    public void AudiologyValidator_ValidVisit_PassesValidation()
    {
        // Arrange
        var dto = new CreateAudiologyVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Hearing loss",
            HearingLossType = "Sensorineural"
        };

        // Act
        var result = _audiologyValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AudiologyValidator_MissingPatientId_FailsValidation()
    {
        // Arrange
        var dto = new CreateAudiologyVisitDto
        {
            PatientId = 0,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Hearing loss"
        };

        // Act
        var result = _audiologyValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Fact]
    public void AudiologyValidator_FutureVisitDate_FailsValidation()
    {
        // Arrange
        var dto = new CreateAudiologyVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.AddDays(7), // Future date
            ChiefComplaint = "Hearing loss"
        };

        // Act
        var result = _audiologyValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.VisitDate);
    }

    [Fact]
    public void AudiologyValidator_MissingChiefComplaint_FailsValidation()
    {
        // Arrange
        var dto = new CreateAudiologyVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "" // Empty
        };

        // Act
        var result = _audiologyValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ChiefComplaint);
    }

    [Theory]
    [InlineData("Conductive")]
    [InlineData("Sensorineural")]
    [InlineData("Mixed")]
    [InlineData("Neural")]
    [InlineData("Functional")]
    public void AudiologyValidator_ValidHearingLossType_PassesValidation(string hearingLossType)
    {
        // Arrange
        var dto = new CreateAudiologyVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Hearing loss",
            HearingLossType = hearingLossType
        };

        // Act
        var result = _audiologyValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.HearingLossType);
    }

    [Fact]
    public void AudiologyValidator_InvalidHearingLossType_FailsValidation()
    {
        // Arrange
        var dto = new CreateAudiologyVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Hearing loss",
            HearingLossType = "InvalidType"
        };

        // Act
        var result = _audiologyValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HearingLossType);
    }

    [Fact]
    public void AudiologyValidator_HearingAidRecommendedWithoutType_FailsValidation()
    {
        // Arrange
        var dto = new CreateAudiologyVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Hearing loss",
            HearingAidRecommended = true,
            HearingAidType = null // Missing when recommended
        };

        // Act
        var result = _audiologyValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HearingAidType);
    }

    #endregion

    #region AudiogramValidator Tests

    private readonly AudiogramValidator _audiogramValidator = new();

    [Fact]
    public void AudiogramValidator_ValidAudiogram_PassesValidation()
    {
        // Arrange
        var dto = new AudiogramDto
        {
            PatientId = 1,
            TestDate = DateTime.UtcNow.AddDays(-1),
            TestType = "Pure Tone",
            Right500Hz = 25,
            Left500Hz = 30,
            RightSpeechRecognition = 92,
            LeftSpeechRecognition = 88
        };

        // Act
        var result = _audiogramValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-15)] // Below minimum
    [InlineData(125)] // Above maximum
    public void AudiogramValidator_InvalidThreshold_FailsValidation(int threshold)
    {
        // Arrange
        var dto = new AudiogramDto
        {
            PatientId = 1,
            TestDate = DateTime.UtcNow,
            Right500Hz = threshold
        };

        // Act
        var result = _audiogramValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Right500Hz);
    }

    [Theory]
    [InlineData(-5)] // Below 0
    [InlineData(105)] // Above 100
    public void AudiogramValidator_InvalidSpeechRecognition_FailsValidation(int percentage)
    {
        // Arrange
        var dto = new AudiogramDto
        {
            PatientId = 1,
            TestDate = DateTime.UtcNow,
            RightSpeechRecognition = percentage
        };

        // Act
        var result = _audiogramValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RightSpeechRecognition);
    }

    #endregion

    #region CreateDentalVisitValidator Tests

    private readonly CreateDentalVisitValidator _dentalValidator = new();

    [Fact]
    public void DentalValidator_ValidVisit_PassesValidation()
    {
        // Arrange
        var dto = new CreateDentalVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Toothache",
            Diagnosis = "Dental caries"
        };

        // Act
        var result = _dentalValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DentalValidator_InvalidProcedureIds_FailsValidation()
    {
        // Arrange
        var dto = new CreateDentalVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Toothache",
            ProcedureIds = new List<int> { 1, 0, -1 } // Contains invalid IDs
        };

        // Act
        var result = _dentalValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProcedureIds);
    }

    #endregion

    #region DentalProcedureValidator Tests

    private readonly DentalProcedureValidator _procedureValidator = new();

    [Fact]
    public void DentalProcedureValidator_ValidProcedure_PassesValidation()
    {
        // Arrange
        var dto = new DentalProcedureDto
        {
            VisitId = 1,
            Code = "D2140",
            Name = "Amalgam Filling",
            ToothNumber = "14",
            Surface = "MOD",
            Cost = 150.00m,
            Status = "Completed"
        };

        // Act
        var result = _procedureValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("11")]
    [InlineData("18")]
    [InlineData("21")]
    [InlineData("41")]
    [InlineData("51")] // Primary teeth
    public void DentalProcedureValidator_ValidToothNumber_PassesValidation(string toothNumber)
    {
        // Arrange
        var dto = new DentalProcedureDto
        {
            VisitId = 1,
            Code = "D2140",
            Name = "Filling",
            ToothNumber = toothNumber,
            Cost = 100m
        };

        // Act
        var result = _procedureValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ToothNumber);
    }

    [Fact]
    public void DentalProcedureValidator_InvalidToothNumber_FailsValidation()
    {
        // Arrange
        var dto = new DentalProcedureDto
        {
            VisitId = 1,
            Code = "D2140",
            Name = "Filling",
            ToothNumber = "99", // Invalid
            Cost = 100m
        };

        // Act
        var result = _procedureValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ToothNumber);
    }

    [Fact]
    public void DentalProcedureValidator_NegativeCost_FailsValidation()
    {
        // Arrange
        var dto = new DentalProcedureDto
        {
            VisitId = 1,
            Code = "D2140",
            Name = "Filling",
            Cost = -50m
        };

        // Act
        var result = _procedureValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cost);
    }

    #endregion

    #region CreateCardioVisitValidator Tests

    private readonly CreateCardioVisitValidator _cardioValidator = new();

    [Fact]
    public void CardioValidator_ValidVisit_PassesValidation()
    {
        // Arrange
        var dto = new CreateCardioVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Chest pain",
            BloodPressureSystolic = 120,
            BloodPressureDiastolic = 80,
            HeartRate = 72
        };

        // Act
        var result = _cardioValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(50)] // Below minimum
    [InlineData(350)] // Above maximum
    public void CardioValidator_InvalidSystolicBP_FailsValidation(int systolic)
    {
        // Arrange
        var dto = new CreateCardioVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Checkup",
            BloodPressureSystolic = systolic
        };

        // Act
        var result = _cardioValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BloodPressureSystolic);
    }

    [Fact]
    public void CardioValidator_SystolicLessThanDiastolic_FailsValidation()
    {
        // Arrange
        var dto = new CreateCardioVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Checkup",
            BloodPressureSystolic = 80,
            BloodPressureDiastolic = 120 // Higher than systolic
        };

        // Act
        var result = _cardioValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Theory]
    [InlineData(10)] // Below minimum
    [InlineData(350)] // Above maximum
    public void CardioValidator_InvalidHeartRate_FailsValidation(int heartRate)
    {
        // Arrange
        var dto = new CreateCardioVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Checkup",
            HeartRate = heartRate
        };

        // Act
        var result = _cardioValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HeartRate);
    }

    [Theory]
    [InlineData("Regular")]
    [InlineData("Sinus Rhythm")]
    [InlineData("Atrial Fibrillation")]
    public void CardioValidator_ValidRhythmAssessment_PassesValidation(string rhythm)
    {
        // Arrange
        var dto = new CreateCardioVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Checkup",
            RhythmAssessment = rhythm
        };

        // Act
        var result = _cardioValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RhythmAssessment);
    }

    #endregion

    #region ECGRecordValidator Tests

    private readonly ECGRecordValidator _ecgValidator = new();

    [Fact]
    public void ECGValidator_ValidRecord_PassesValidation()
    {
        // Arrange
        var dto = new ECGRecordDto
        {
            PatientId = 1,
            RecordDate = DateTime.UtcNow,
            Rhythm = "Sinus Rhythm",
            HeartRate = 72,
            Interpretation = "Normal ECG"
        };

        // Act
        var result = _ecgValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ECGValidator_FutureRecordDate_FailsValidation()
    {
        // Arrange
        var dto = new ECGRecordDto
        {
            PatientId = 1,
            RecordDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = _ecgValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecordDate);
    }

    #endregion

    #region EchoResultValidator Tests

    private readonly EchoResultValidator _echoValidator = new();

    [Fact]
    public void EchoValidator_ValidResult_PassesValidation()
    {
        // Arrange
        var dto = new EchoResultDto
        {
            PatientId = 1,
            StudyDate = DateTime.UtcNow,
            EjectionFraction = 55m,
            LeftVentricle = "Normal size and function",
            Conclusion = "Normal echocardiogram"
        };

        // Act
        var result = _echoValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-5)] // Below 0
    [InlineData(105)] // Above 100
    public void EchoValidator_InvalidEjectionFraction_FailsValidation(decimal ef)
    {
        // Arrange
        var dto = new EchoResultDto
        {
            PatientId = 1,
            StudyDate = DateTime.UtcNow,
            EjectionFraction = ef
        };

        // Act
        var result = _echoValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EjectionFraction);
    }

    #endregion

    #region CreateOphthalmologyVisitValidator Tests

    private readonly CreateOphthalmologyVisitValidator _ophthalmologyValidator = new();

    [Fact]
    public void OphthalmologyValidator_ValidVisit_PassesValidation()
    {
        // Arrange
        var dto = new CreateOphthalmologyVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Blurred vision",
            VisualAcuityRight = "20/20",
            VisualAcuityLeft = "20/25"
        };

        // Act
        var result = _ophthalmologyValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("20/20")]
    [InlineData("6/6")]
    [InlineData("CF")] // Counting fingers
    [InlineData("HM")] // Hand motion
    [InlineData("LP")] // Light perception
    [InlineData("NLP")] // No light perception
    public void OphthalmologyValidator_ValidVisualAcuity_PassesValidation(string acuity)
    {
        // Arrange
        var dto = new CreateOphthalmologyVisitDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Checkup",
            VisualAcuityRight = acuity
        };

        // Act
        var result = _ophthalmologyValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.VisualAcuityRight);
    }

    #endregion

    #region EyePrescriptionValidator Tests

    private readonly EyePrescriptionValidator _prescriptionValidator = new();

    [Fact]
    public void EyePrescriptionValidator_ValidPrescription_PassesValidation()
    {
        // Arrange
        var dto = new EyePrescriptionDto
        {
            PatientId = 1,
            PrescriptionDate = DateTime.UtcNow,
            RightSphere = -2.50m,
            RightCylinder = -0.75m,
            RightAxis = 90,
            LeftSphere = -2.25m,
            LeftCylinder = -0.50m,
            LeftAxis = 85,
            PupillaryDistance = "62",
            ExpiryDate = DateTime.UtcNow.AddYears(2)
        };

        // Act
        var result = _prescriptionValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-30)] // Below -25
    [InlineData(30)] // Above 25
    public void EyePrescriptionValidator_InvalidSphere_FailsValidation(decimal sphere)
    {
        // Arrange
        var dto = new EyePrescriptionDto
        {
            PatientId = 1,
            PrescriptionDate = DateTime.UtcNow,
            RightSphere = sphere
        };

        // Act
        var result = _prescriptionValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RightSphere);
    }

    [Theory]
    [InlineData(-1)] // Below 0
    [InlineData(185)] // Above 180
    public void EyePrescriptionValidator_InvalidAxis_FailsValidation(int axis)
    {
        // Arrange
        var dto = new EyePrescriptionDto
        {
            PatientId = 1,
            PrescriptionDate = DateTime.UtcNow,
            RightAxis = axis
        };

        // Act
        var result = _prescriptionValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RightAxis);
    }

    [Fact]
    public void EyePrescriptionValidator_ExpiryBeforePrescription_FailsValidation()
    {
        // Arrange
        var dto = new EyePrescriptionDto
        {
            PatientId = 1,
            PrescriptionDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(-1) // Before prescription date
        };

        // Act
        var result = _prescriptionValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryDate);
    }

    #endregion

    #region CreatePhysioSessionValidator Tests

    private readonly CreatePhysioSessionValidator _physioValidator = new();

    [Fact]
    public void PhysioValidator_ValidSession_PassesValidation()
    {
        // Arrange
        var dto = new CreatePhysioSessionDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Lower back pain",
            TreatmentType = "Manual Therapy",
            DurationMinutes = 45,
            PainLevelBefore = 7
        };

        // Act
        var result = _physioValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Manual Therapy")]
    [InlineData("Exercise Therapy")]
    [InlineData("Electrotherapy")]
    [InlineData("Ultrasound")]
    [InlineData("TENS")]
    public void PhysioValidator_ValidTreatmentType_PassesValidation(string treatmentType)
    {
        // Arrange
        var dto = new CreatePhysioSessionDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Pain",
            TreatmentType = treatmentType
        };

        // Act
        var result = _physioValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TreatmentType);
    }

    [Theory]
    [InlineData(2)] // Too short
    [InlineData(200)] // Too long
    public void PhysioValidator_InvalidDuration_FailsValidation(int duration)
    {
        // Arrange
        var dto = new CreatePhysioSessionDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Pain",
            DurationMinutes = duration
        };

        // Act
        var result = _physioValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Theory]
    [InlineData(-1)] // Below 0
    [InlineData(11)] // Above 10
    public void PhysioValidator_InvalidPainLevel_FailsValidation(int painLevel)
    {
        // Arrange
        var dto = new CreatePhysioSessionDto
        {
            PatientId = 1,
            VisitDate = DateTime.UtcNow.Date,
            ChiefComplaint = "Pain",
            PainLevelBefore = painLevel
        };

        // Act
        var result = _physioValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PainLevelBefore);
    }

    #endregion

    #region PhysioAssessmentValidator Tests

    private readonly PhysioAssessmentValidator _assessmentValidator = new();

    [Fact]
    public void PhysioAssessmentValidator_ValidAssessment_PassesValidation()
    {
        // Arrange
        var dto = new PhysioAssessmentDto
        {
            PatientId = 1,
            AssessmentDate = DateTime.UtcNow,
            PresentingComplaint = "Lower back pain for 2 weeks",
            Diagnosis = "Lumbar strain",
            TreatmentPlan = "Manual therapy and exercises",
            PlannedSessions = 12
        };

        // Act
        var result = _assessmentValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PhysioAssessmentValidator_MissingPresentingComplaint_FailsValidation()
    {
        // Arrange
        var dto = new PhysioAssessmentDto
        {
            PatientId = 1,
            AssessmentDate = DateTime.UtcNow,
            PresentingComplaint = ""
        };

        // Act
        var result = _assessmentValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PresentingComplaint);
    }

    [Theory]
    [InlineData(0)] // Below 1
    [InlineData(150)] // Above 100
    public void PhysioAssessmentValidator_InvalidPlannedSessions_FailsValidation(int sessions)
    {
        // Arrange
        var dto = new PhysioAssessmentDto
        {
            PatientId = 1,
            AssessmentDate = DateTime.UtcNow,
            PresentingComplaint = "Pain",
            PlannedSessions = sessions
        };

        // Act
        var result = _assessmentValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlannedSessions);
    }

    #endregion

    #region ClinicalVisitListRequestValidator Tests

    private readonly ClinicalVisitListRequestValidator _listValidator = new();

    [Fact]
    public void ListValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new ClinicalVisitListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = "VisitDate"
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
        var dto = new ClinicalVisitListRequestDto
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
        var dto = new ClinicalVisitListRequestDto
        {
            PageNumber = 1,
            PageSize = 150
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void ListValidator_InvalidDateRange_FailsValidation()
    {
        // Arrange
        var dto = new ClinicalVisitListRequestDto
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

    [Theory]
    [InlineData("VisitDate")]
    [InlineData("PatientName")]
    [InlineData("ProviderName")]
    public void ListValidator_ValidSortField_PassesValidation(string sortBy)
    {
        // Arrange
        var dto = new ClinicalVisitListRequestDto
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
        var dto = new ClinicalVisitListRequestDto
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
}
