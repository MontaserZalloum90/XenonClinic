namespace XenonClinic.Core.DTOs;

/// <summary>
/// DTO for patient data transfer. Used for reading patient information.
/// </summary>
public class PatientDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string EmiratesId { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age => CalculateAge(DateOfBirth);
    public string Gender { get; set; } = "M";  // FIX: Align with Entity default
    public string GenderDisplay => Gender switch
    {
        "M" => "Male",
        "F" => "Female",
        _ => "Other"
    };
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? HearingLossType { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // FIX: Add soft delete fields for audit trail and admin interfaces
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Related data counts (for summary view)
    public int AppointmentsCount { get; set; }
    public int DocumentsCount { get; set; }
    public bool HasMedicalHistory { get; set; }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}

/// <summary>
/// DTO for creating a new patient.
/// NOTE: BranchId can be overridden by service layer from authenticated user context if not provided.
/// </summary>
public class CreatePatientDto
{
    /// <summary>
    /// Branch ID. If not provided, will be set from authenticated user's context.
    /// </summary>
    public int? BranchId { get; set; }

    public string EmiratesId { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "M";
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? HearingLossType { get; set; }
    public string? Notes { get; set; }

    // Optional: Create medical history with patient
    public CreatePatientMedicalHistoryDto? MedicalHistory { get; set; }
}

/// <summary>
/// DTO for updating an existing patient.
/// </summary>
public class UpdatePatientDto
{
    public int Id { get; set; }

    /// <summary>
    /// Branch ID. Allows transferring patient between branches.
    /// </summary>
    public int? BranchId { get; set; }

    public string EmiratesId { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "M";
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? HearingLossType { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for patient search results (lightweight).
/// </summary>
public class PatientSearchResultDto
{
    public int Id { get; set; }
    public string EmiratesId { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for patient medical history.
/// </summary>
public class PatientMedicalHistoryDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }

    // Medical conditions
    public string? ChronicConditions { get; set; }
    public string? Allergies { get; set; }
    public string? AllergyReactions { get; set; }
    public string? CurrentMedications { get; set; }
    public string? PastMedicalHistory { get; set; }
    public string? SurgicalHistory { get; set; }

    // History
    public string? FamilyHistory { get; set; }
    public string? OccupationalExposure { get; set; }

    // Hearing-specific (for audiology)
    public string? NoiseExposureHistory { get; set; }
    public string? PreviousHearingAids { get; set; }
    public string? TinnitusHistory { get; set; }
    public string? BalanceProblems { get; set; }

    // Lifestyle
    public bool IsSmoker { get; set; }
    public bool ConsumesAlcohol { get; set; }

    // Additional
    public string? AdditionalNotes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for creating/updating patient medical history.
/// </summary>
public class CreatePatientMedicalHistoryDto
{
    public string? ChronicConditions { get; set; }
    public string? Allergies { get; set; }
    public string? AllergyReactions { get; set; }
    public string? CurrentMedications { get; set; }
    public string? PastMedicalHistory { get; set; }
    public string? SurgicalHistory { get; set; }
    public string? FamilyHistory { get; set; }
    public string? OccupationalExposure { get; set; }
    public string? NoiseExposureHistory { get; set; }
    public string? PreviousHearingAids { get; set; }
    public string? TinnitusHistory { get; set; }
    public string? BalanceProblems { get; set; }
    public bool IsSmoker { get; set; }
    public bool ConsumesAlcohol { get; set; }
    public string? AdditionalNotes { get; set; }
}

/// <summary>
/// DTO for patient documents.
/// </summary>
public class PatientDocumentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentTypeDisplay => DocumentType switch
    {
        "ConsentForm" => "Consent Form",
        "MedicalRecord" => "Medical Record",
        "IDCopy" => "ID Copy",
        "InsuranceCard" => "Insurance Card",
        "LabReport" => "Lab Report",
        "Prescription" => "Prescription",
        _ => "Other"
    };
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    public string? Tags { get; set; }

    // FIX: Add FilePath for backend/admin use (consider security when exposing)
    public string FilePath { get; set; } = string.Empty;

    // FIX: Add audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for uploading a patient document.
/// </summary>
public class UploadPatientDocumentDto
{
    public int PatientId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Tags { get; set; }

    // File data (base64 encoded for API transfer)
    public string? FileContent { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
}

/// <summary>
/// DTO for patient statistics.
/// </summary>
public class PatientStatisticsDto
{
    public int TotalPatients { get; set; }
    public int NewPatientsThisMonth { get; set; }
    public int NewPatientsThisWeek { get; set; }
    public int ActivePatients { get; set; }
    public Dictionary<string, int> GenderDistribution { get; set; } = new();
    public Dictionary<string, int> AgeGroupDistribution { get; set; } = new();
}

/// <summary>
/// DTO for paginated patient list requests.
/// </summary>
public class PatientListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirthFrom { get; set; }
    public DateTime? DateOfBirthTo { get; set; }
    public string? SortBy { get; set; } = "FullNameEn";
    public bool SortDescending { get; set; }
}

/// <summary>
/// Standard validation error messages for patient operations.
/// </summary>
public static class PatientValidationMessages
{
    public const string EmiratesIdRequired = "Emirates ID is required";
    public const string EmiratesIdInvalid = "Emirates ID format is invalid. Expected format: 784-XXXX-XXXXXXX-X";
    public const string EmiratesIdDuplicate = "A patient with this Emirates ID already exists in this branch";

    public const string FullNameRequired = "Patient full name (English) is required";
    public const string FullNameTooLong = "Patient name cannot exceed 200 characters";
    public const string FullNameInvalid = "Patient name contains invalid characters";

    public const string DateOfBirthRequired = "Date of birth is required";
    public const string DateOfBirthFuture = "Date of birth cannot be in the future";
    public const string DateOfBirthTooOld = "Date of birth cannot be more than 150 years ago";

    public const string GenderRequired = "Gender is required";
    public const string GenderInvalid = "Gender must be M (Male), F (Female), or O (Other)";

    public const string PhoneInvalid = "Phone number format is invalid";
    public const string EmailInvalid = "Email address format is invalid";

    public const string DocumentNameRequired = "Document name is required";
    public const string DocumentTypeRequired = "Document type is required";
    public const string DocumentTypeInvalid = "Invalid document type";
    public const string FileSizeTooLarge = "File size cannot exceed 10 MB";
    public const string FileTypeNotAllowed = "File type is not allowed";

    public const string PatientNotFound = "Patient not found";
    public const string DocumentNotFound = "Document not found";
    public const string BranchAccessDenied = "You do not have access to this branch";
}
