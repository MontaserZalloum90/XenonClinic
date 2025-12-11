using FluentAssertions;
using FluentValidation.TestHelper;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Validators;
using Xunit;

namespace XenonClinic.Tests.Patient;

/// <summary>
/// Tests for patient DTO validators.
/// </summary>
public class PatientValidatorTests
{
    #region CreatePatientValidator Tests

    private readonly CreatePatientValidator _createValidator = new();

    [Fact]
    public void CreatePatientValidator_ValidPatient_PassesValidation()
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreatePatientValidator_EmptyEmiratesId_FailsValidation()
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmiratesId);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("123-4567-8901234-5")]
    [InlineData("784-123-1234567-1")] // Wrong format in second segment
    [InlineData("784-12345-1234567-1")] // Extra digit
    public void CreatePatientValidator_InvalidEmiratesIdFormat_FailsValidation(string emiratesId)
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = emiratesId,
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmiratesId);
    }

    [Fact]
    public void CreatePatientValidator_EmptyFullName_FailsValidation()
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullNameEn);
    }

    [Fact]
    public void CreatePatientValidator_NameTooLong_FailsValidation()
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = new string('A', 201),
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullNameEn);
    }

    [Fact]
    public void CreatePatientValidator_NameWithInvalidChars_FailsValidation()
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John<script>Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FullNameEn);
    }

    [Fact]
    public void CreatePatientValidator_FutureDateOfBirth_FailsValidation()
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = DateTime.Today.AddDays(1),
            Gender = "M"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void CreatePatientValidator_VeryOldDateOfBirth_FailsValidation()
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = DateTime.Today.AddYears(-151),
            Gender = "M"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("F")]
    [InlineData("O")]
    public void CreatePatientValidator_ValidGender_PassesValidation(string gender)
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = gender
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("Male")]
    [InlineData("")]
    public void CreatePatientValidator_InvalidGender_FailsValidation(string gender)
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = gender
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Theory]
    [InlineData("+971501234567")]
    [InlineData("0501234567")]
    [InlineData("+1 (555) 123-4567")]
    public void CreatePatientValidator_ValidPhone_PassesValidation(string phone)
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M",
            PhoneNumber = phone
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("12345")] // Too short
    [InlineData("abc123456")]
    public void CreatePatientValidator_InvalidPhone_FailsValidation(string phone)
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M",
            PhoneNumber = phone
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void CreatePatientValidator_ValidEmail_PassesValidation()
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M",
            Email = "john.doe@example.com"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void CreatePatientValidator_InvalidEmail_FailsValidation()
    {
        // Arrange
        var dto = new CreatePatientDto
        {
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M",
            Email = "invalid-email"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region UpdatePatientValidator Tests

    private readonly UpdatePatientValidator _updateValidator = new();

    [Fact]
    public void UpdatePatientValidator_ValidPatient_PassesValidation()
    {
        // Arrange
        var dto = new UpdatePatientDto
        {
            Id = 1,
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe Updated",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M"
        };

        // Act
        var result = _updateValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdatePatientValidator_InvalidId_FailsValidation()
    {
        // Arrange
        var dto = new UpdatePatientDto
        {
            Id = 0,
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M"
        };

        // Act
        var result = _updateValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void UpdatePatientValidator_NegativeId_FailsValidation()
    {
        // Arrange
        var dto = new UpdatePatientDto
        {
            Id = -1,
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M"
        };

        // Act
        var result = _updateValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    #endregion

    #region UploadPatientDocumentValidator Tests

    private readonly UploadPatientDocumentValidator _documentValidator = new();

    [Fact]
    public void DocumentValidator_ValidDocument_PassesValidation()
    {
        // Arrange
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 1,
            DocumentName = "Test Document",
            DocumentType = "IDCopy"
        };

        // Act
        var result = _documentValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DocumentValidator_InvalidPatientId_FailsValidation()
    {
        // Arrange
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 0,
            DocumentName = "Test Document",
            DocumentType = "IDCopy"
        };

        // Act
        var result = _documentValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PatientId);
    }

    [Fact]
    public void DocumentValidator_EmptyDocumentName_FailsValidation()
    {
        // Arrange
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 1,
            DocumentName = "",
            DocumentType = "IDCopy"
        };

        // Act
        var result = _documentValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DocumentName);
    }

    [Fact]
    public void DocumentValidator_InvalidDocumentType_FailsValidation()
    {
        // Arrange
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 1,
            DocumentName = "Test Document",
            DocumentType = "InvalidType"
        };

        // Act
        var result = _documentValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DocumentType);
    }

    [Theory]
    [InlineData("ConsentForm")]
    [InlineData("MedicalRecord")]
    [InlineData("IDCopy")]
    [InlineData("InsuranceCard")]
    [InlineData("LabReport")]
    [InlineData("Prescription")]
    [InlineData("Other")]
    public void DocumentValidator_ValidDocumentTypes_PassesValidation(string documentType)
    {
        // Arrange
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 1,
            DocumentName = "Test Document",
            DocumentType = documentType
        };

        // Act
        var result = _documentValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DocumentType);
    }

    [Fact]
    public void DocumentValidator_ValidBase64Content_PassesValidation()
    {
        // Arrange
        var content = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 1,
            DocumentName = "Test Document",
            DocumentType = "IDCopy",
            FileContent = content,
            FileName = "test.pdf",
            ContentType = "application/pdf"
        };

        // Act
        var result = _documentValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FileContent);
    }

    [Fact]
    public void DocumentValidator_InvalidBase64_FailsValidation()
    {
        // Arrange
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 1,
            DocumentName = "Test Document",
            DocumentType = "IDCopy",
            FileContent = "not-valid-base64!!!",
            FileName = "test.pdf",
            ContentType = "application/pdf"
        };

        // Act
        var result = _documentValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileContent);
    }

    [Fact]
    public void DocumentValidator_PathTraversalInFileName_FailsValidation()
    {
        // Arrange
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 1,
            DocumentName = "Test Document",
            DocumentType = "IDCopy",
            FileContent = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
            FileName = "../../../etc/passwd",
            ContentType = "application/pdf"
        };

        // Act
        var result = _documentValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void DocumentValidator_InvalidContentType_FailsValidation()
    {
        // Arrange
        var dto = new UploadPatientDocumentDto
        {
            PatientId = 1,
            DocumentName = "Test Document",
            DocumentType = "IDCopy",
            FileContent = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
            FileName = "test.exe",
            ContentType = "application/x-executable"
        };

        // Act
        var result = _documentValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContentType);
    }

    #endregion

    #region PatientListRequestValidator Tests

    private readonly PatientListRequestValidator _listValidator = new();

    [Fact]
    public void ListValidator_ValidRequest_PassesValidation()
    {
        // Arrange
        var dto = new PatientListRequestDto
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
        var dto = new PatientListRequestDto
        {
            PageNumber = 0,
            PageSize = 20
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void ListValidator_InvalidPageSize_FailsValidation(int pageSize)
    {
        // Arrange
        var dto = new PatientListRequestDto
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData("DROP TABLE")]
    [InlineData("SELECT * FROM")]
    [InlineData("<script>")]
    public void ListValidator_DangerousSearchTerm_FailsValidation(string searchTerm)
    {
        // Arrange
        var dto = new PatientListRequestDto
        {
            PageNumber = 1,
            PageSize = 20,
            SearchTerm = searchTerm
        };

        // Act
        var result = _listValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Fact]
    public void ListValidator_InvalidSortField_FailsValidation()
    {
        // Arrange
        var dto = new PatientListRequestDto
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

    [Theory]
    [InlineData("FullNameEn")]
    [InlineData("EmiratesId")]
    [InlineData("DateOfBirth")]
    [InlineData("CreatedAt")]
    public void ListValidator_ValidSortField_PassesValidation(string sortBy)
    {
        // Arrange
        var dto = new PatientListRequestDto
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

    #endregion
}
