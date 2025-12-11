using FluentValidation;
using System.Text.RegularExpressions;
using XenonClinic.Core.DTOs;

namespace XenonClinic.Core.Validators;

/// <summary>
/// Validator for CreatePatientDto.
/// </summary>
public class CreatePatientValidator : AbstractValidator<CreatePatientDto>
{
    // UAE Emirates ID format: 784-XXXX-XXXXXXX-X
    private static readonly Regex EmiratesIdPattern = new(
        @"^784-\d{4}-\d{7}-\d$",
        RegexOptions.Compiled);

    // Phone pattern: international format with optional country code
    private static readonly Regex PhonePattern = new(
        @"^\+?[\d\s\-()]{7,20}$",
        RegexOptions.Compiled);

    // Name pattern: letters, spaces, hyphens, apostrophes
    private static readonly Regex NamePattern = new(
        @"^[\p{L}\s\-'\.]+$",
        RegexOptions.Compiled);

    public CreatePatientValidator()
    {
        RuleFor(x => x.EmiratesId)
            .NotEmpty().WithMessage(PatientValidationMessages.EmiratesIdRequired)
            .Must(BeValidEmiratesId).WithMessage(PatientValidationMessages.EmiratesIdInvalid);

        RuleFor(x => x.FullNameEn)
            .NotEmpty().WithMessage(PatientValidationMessages.FullNameRequired)
            .MaximumLength(200).WithMessage(PatientValidationMessages.FullNameTooLong)
            .Must(BeValidName).WithMessage(PatientValidationMessages.FullNameInvalid);

        RuleFor(x => x.FullNameAr)
            .MaximumLength(200).WithMessage(PatientValidationMessages.FullNameTooLong)
            .When(x => !string.IsNullOrEmpty(x.FullNameAr));

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage(PatientValidationMessages.DateOfBirthRequired)
            .LessThanOrEqualTo(DateTime.Today).WithMessage(PatientValidationMessages.DateOfBirthFuture)
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage(PatientValidationMessages.DateOfBirthTooOld);

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage(PatientValidationMessages.GenderRequired)
            .Must(BeValidGender).WithMessage(PatientValidationMessages.GenderInvalid);

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidPhone).WithMessage(PatientValidationMessages.PhoneInvalid)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage(PatientValidationMessages.EmailInvalid)
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.HearingLossType)
            .Must(BeValidHearingLossType).WithMessage("Invalid hearing loss type")
            .When(x => !string.IsNullOrEmpty(x.HearingLossType));

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");

        // Validate nested medical history if provided
        RuleFor(x => x.MedicalHistory)
            .SetValidator(new CreatePatientMedicalHistoryValidator()!)
            .When(x => x.MedicalHistory != null);
    }

    private static bool BeValidEmiratesId(string? emiratesId)
    {
        if (string.IsNullOrEmpty(emiratesId))
            return false;

        return EmiratesIdPattern.IsMatch(emiratesId);
    }

    private static bool BeValidName(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return true; // Empty handled by NotEmpty rule

        return NamePattern.IsMatch(name);
    }

    private static bool BeValidGender(string? gender)
    {
        return gender is "M" or "F" or "O";
    }

    private static bool BeValidPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone))
            return true;

        return PhonePattern.IsMatch(phone);
    }

    private static bool BeValidHearingLossType(string? type)
    {
        if (string.IsNullOrEmpty(type))
            return true;

        var validTypes = new[]
        {
            "Conductive", "Sensorineural", "Mixed", "Central", "Functional", "None"
        };

        return validTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Validator for UpdatePatientDto.
/// </summary>
public class UpdatePatientValidator : AbstractValidator<UpdatePatientDto>
{
    private static readonly Regex EmiratesIdPattern = new(
        @"^784-\d{4}-\d{7}-\d$",
        RegexOptions.Compiled);

    private static readonly Regex PhonePattern = new(
        @"^\+?[\d\s\-()]{7,20}$",
        RegexOptions.Compiled);

    private static readonly Regex NamePattern = new(
        @"^[\p{L}\s\-'\.]+$",
        RegexOptions.Compiled);

    public UpdatePatientValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid patient ID");

        RuleFor(x => x.EmiratesId)
            .NotEmpty().WithMessage(PatientValidationMessages.EmiratesIdRequired)
            .Must(BeValidEmiratesId).WithMessage(PatientValidationMessages.EmiratesIdInvalid);

        RuleFor(x => x.FullNameEn)
            .NotEmpty().WithMessage(PatientValidationMessages.FullNameRequired)
            .MaximumLength(200).WithMessage(PatientValidationMessages.FullNameTooLong)
            .Must(BeValidName).WithMessage(PatientValidationMessages.FullNameInvalid);

        RuleFor(x => x.FullNameAr)
            .MaximumLength(200).WithMessage(PatientValidationMessages.FullNameTooLong)
            .When(x => !string.IsNullOrEmpty(x.FullNameAr));

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage(PatientValidationMessages.DateOfBirthRequired)
            .LessThanOrEqualTo(DateTime.Today).WithMessage(PatientValidationMessages.DateOfBirthFuture)
            .GreaterThan(DateTime.Today.AddYears(-150)).WithMessage(PatientValidationMessages.DateOfBirthTooOld);

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage(PatientValidationMessages.GenderRequired)
            .Must(BeValidGender).WithMessage(PatientValidationMessages.GenderInvalid);

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidPhone).WithMessage(PatientValidationMessages.PhoneInvalid)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage(PatientValidationMessages.EmailInvalid)
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }

    private static bool BeValidEmiratesId(string? emiratesId) =>
        !string.IsNullOrEmpty(emiratesId) && EmiratesIdPattern.IsMatch(emiratesId);

    private static bool BeValidName(string? name) =>
        string.IsNullOrEmpty(name) || NamePattern.IsMatch(name);

    private static bool BeValidGender(string? gender) =>
        gender is "M" or "F" or "O";

    private static bool BeValidPhone(string? phone) =>
        string.IsNullOrEmpty(phone) || PhonePattern.IsMatch(phone);
}

/// <summary>
/// Validator for CreatePatientMedicalHistoryDto.
/// </summary>
public class CreatePatientMedicalHistoryValidator : AbstractValidator<CreatePatientMedicalHistoryDto>
{
    public CreatePatientMedicalHistoryValidator()
    {
        RuleFor(x => x.ChronicConditions)
            .MaximumLength(2000).WithMessage("Chronic conditions cannot exceed 2000 characters");

        RuleFor(x => x.Allergies)
            .MaximumLength(2000).WithMessage("Allergies cannot exceed 2000 characters");

        RuleFor(x => x.CurrentMedications)
            .MaximumLength(2000).WithMessage("Current medications cannot exceed 2000 characters");

        RuleFor(x => x.PastSurgeries)
            .MaximumLength(2000).WithMessage("Past surgeries cannot exceed 2000 characters");

        RuleFor(x => x.FamilyHistory)
            .MaximumLength(2000).WithMessage("Family history cannot exceed 2000 characters");

        RuleFor(x => x.SocialHistory)
            .MaximumLength(2000).WithMessage("Social history cannot exceed 2000 characters");

        RuleFor(x => x.OccupationalExposure)
            .MaximumLength(1000).WithMessage("Occupational exposure cannot exceed 1000 characters");
    }
}

/// <summary>
/// Validator for UploadPatientDocumentDto.
/// </summary>
public class UploadPatientDocumentValidator : AbstractValidator<UploadPatientDocumentDto>
{
    private static readonly string[] AllowedDocumentTypes =
    {
        "ConsentForm", "MedicalRecord", "IDCopy", "InsuranceCard",
        "LabReport", "Prescription", "Other"
    };

    private static readonly string[] AllowedContentTypes =
    {
        "application/pdf",
        "image/jpeg", "image/jpg", "image/png", "image/gif",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public UploadPatientDocumentValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("Invalid patient ID");

        RuleFor(x => x.DocumentName)
            .NotEmpty().WithMessage(PatientValidationMessages.DocumentNameRequired)
            .MaximumLength(200).WithMessage("Document name cannot exceed 200 characters");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage(PatientValidationMessages.DocumentTypeRequired)
            .Must(BeValidDocumentType).WithMessage(PatientValidationMessages.DocumentTypeInvalid);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be in the future")
            .When(x => x.ExpiryDate.HasValue);

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters");

        RuleFor(x => x.FileContent)
            .NotEmpty().WithMessage("File content is required")
            .Must(BeValidBase64).WithMessage("File content must be valid base64")
            .Must(BeWithinSizeLimit).WithMessage(PatientValidationMessages.FileSizeTooLarge)
            .When(x => !string.IsNullOrEmpty(x.FileContent));

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters")
            .Must(BeValidFileName).WithMessage("File name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.FileContent));

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required")
            .Must(BeValidContentType).WithMessage(PatientValidationMessages.FileTypeNotAllowed)
            .When(x => !string.IsNullOrEmpty(x.FileContent));
    }

    private static bool BeValidDocumentType(string? type) =>
        !string.IsNullOrEmpty(type) && AllowedDocumentTypes.Contains(type);

    private static bool BeValidBase64(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return true;

        try
        {
            Convert.FromBase64String(content);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeWithinSizeLimit(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return true;

        try
        {
            var bytes = Convert.FromBase64String(content);
            return bytes.Length <= MaxFileSizeBytes;
        }
        catch
        {
            return false;
        }
    }

    private static bool BeValidFileName(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return true;

        // Check for path traversal attempts
        if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
            return false;

        // Check for invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        return !fileName.Any(c => invalidChars.Contains(c));
    }

    private static bool BeValidContentType(string? contentType) =>
        !string.IsNullOrEmpty(contentType) && AllowedContentTypes.Contains(contentType.ToLowerInvariant());
}

/// <summary>
/// Validator for PatientListRequestDto.
/// </summary>
public class PatientListRequestValidator : AbstractValidator<PatientListRequestDto>
{
    private static readonly string[] ValidSortFields =
    {
        "FullNameEn", "FullNameAr", "EmiratesId", "DateOfBirth", "CreatedAt", "Gender"
    };

    public PatientListRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .Must(NotContainDangerousChars).WithMessage("Search term contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.Gender)
            .Must(BeValidGender).WithMessage("Invalid gender filter")
            .When(x => !string.IsNullOrEmpty(x.Gender));

        RuleFor(x => x.DateOfBirthFrom)
            .LessThanOrEqualTo(x => x.DateOfBirthTo)
            .WithMessage("Start date must be before or equal to end date")
            .When(x => x.DateOfBirthFrom.HasValue && x.DateOfBirthTo.HasValue);

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Invalid sort field")
            .When(x => !string.IsNullOrEmpty(x.SortBy));
    }

    private static bool NotContainDangerousChars(string? term)
    {
        if (string.IsNullOrEmpty(term))
            return true;

        // Block SQL injection and XSS patterns
        var dangerousPatterns = new[] { "--", ";", "<", ">", "script", "drop", "delete" };
        return !dangerousPatterns.Any(p => term.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private static bool BeValidGender(string? gender) =>
        string.IsNullOrEmpty(gender) || gender is "M" or "F" or "O";

    private static bool BeValidSortField(string? field) =>
        string.IsNullOrEmpty(field) || ValidSortFields.Contains(field, StringComparer.OrdinalIgnoreCase);
}
