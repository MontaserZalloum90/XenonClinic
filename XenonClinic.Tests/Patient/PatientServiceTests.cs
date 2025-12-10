using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Patient;

/// <summary>
/// Tests for the PatientService implementation.
/// </summary>
public class PatientServiceTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private IPatientService _patientService = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _patientService = new PatientService(_context);

        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    private async Task SeedTestDataAsync()
    {
        var company = new Company
        {
            Id = 1,
            TenantId = 1,
            Name = "Test Clinic",
            Code = "TC001",
            IsActive = true
        };
        _context.Companies.Add(company);

        var branch = new Branch
        {
            Id = 1,
            CompanyId = 1,
            Code = "BR001",
            Name = "Main Branch",
            IsActive = true
        };
        _context.Branches.Add(branch);

        var branch2 = new Branch
        {
            Id = 2,
            CompanyId = 1,
            Code = "BR002",
            Name = "Second Branch",
            IsActive = true
        };
        _context.Branches.Add(branch2);

        // Seed test patients
        var patients = new List<Core.Entities.Patient>
        {
            new()
            {
                Id = 1,
                BranchId = 1,
                EmiratesId = "784-1234-1234567-1",
                FullNameEn = "John Doe",
                FullNameAr = "جون دو",
                DateOfBirth = new DateTime(1990, 5, 15),
                Gender = "M",
                PhoneNumber = "+971501234567",
                Email = "john.doe@test.com",
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                IsDeleted = false
            },
            new()
            {
                Id = 2,
                BranchId = 1,
                EmiratesId = "784-2345-2345678-2",
                FullNameEn = "Jane Smith",
                DateOfBirth = new DateTime(1985, 8, 20),
                Gender = "F",
                PhoneNumber = "+971502345678",
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                IsDeleted = false
            },
            new()
            {
                Id = 3,
                BranchId = 1,
                EmiratesId = "784-3456-3456789-3",
                FullNameEn = "Deleted Patient",
                DateOfBirth = new DateTime(1975, 1, 1),
                Gender = "M",
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow.AddDays(-7)
            },
            new()
            {
                Id = 4,
                BranchId = 2,
                EmiratesId = "784-4567-4567890-4",
                FullNameEn = "Other Branch Patient",
                DateOfBirth = new DateTime(2000, 12, 25),
                Gender = "F",
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                IsDeleted = false
            }
        };
        _context.Patients.AddRange(patients);

        // Seed medical history
        var medicalHistory = new PatientMedicalHistory
        {
            Id = 1,
            PatientId = 1,
            ChronicConditions = "Diabetes Type 2",
            Allergies = "Penicillin",
            CurrentMedications = "Metformin 500mg",
            FamilyHistory = "Heart disease",
            IsSmoker = false,
            ConsumesAlcohol = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.PatientMedicalHistories.Add(medicalHistory);

        // Seed documents
        var documents = new List<PatientDocument>
        {
            new()
            {
                Id = 1,
                PatientId = 1,
                DocumentName = "ID Copy",
                DocumentType = "IDCopy",
                FileName = "id_copy.pdf",
                FileExtension = ".pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 1024,
                UploadDate = DateTime.UtcNow.AddDays(-30),
                UploadedBy = "admin",
                IsActive = true
            },
            new()
            {
                Id = 2,
                PatientId = 1,
                DocumentName = "Lab Report",
                DocumentType = "LabReport",
                FileName = "lab_report.pdf",
                FileExtension = ".pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 2048,
                UploadDate = DateTime.UtcNow.AddDays(-10),
                UploadedBy = "doctor1",
                IsActive = true
            }
        };
        _context.PatientDocuments.AddRange(documents);

        await _context.SaveChangesAsync();
    }

    #region GetPatientByIdAsync Tests

    [Fact]
    public async Task GetPatientByIdAsync_ExistingPatient_ReturnsPatient()
    {
        // Act
        var result = await _patientService.GetPatientByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.EmiratesId.Should().Be("784-1234-1234567-1");
        result.FullNameEn.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetPatientByIdAsync_NonExistentPatient_ReturnsNull()
    {
        // Act
        var result = await _patientService.GetPatientByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPatientByIdAsync_DeletedPatient_ReturnsNull()
    {
        // Act
        var result = await _patientService.GetPatientByIdAsync(3);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPatientByIdAsync_IncludesRelatedData()
    {
        // Act
        var result = await _patientService.GetPatientByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Branch.Should().NotBeNull();
        result.MedicalHistory.Should().NotBeNull();
        result.Documents.Should().NotBeEmpty();
    }

    #endregion

    #region GetPatientByEmiratesIdAsync Tests

    [Fact]
    public async Task GetPatientByEmiratesIdAsync_ExistingPatient_ReturnsPatient()
    {
        // Act
        var result = await _patientService.GetPatientByEmiratesIdAsync("784-1234-1234567-1", 1);

        // Assert
        result.Should().NotBeNull();
        result!.FullNameEn.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetPatientByEmiratesIdAsync_WrongBranch_ReturnsNull()
    {
        // Act
        var result = await _patientService.GetPatientByEmiratesIdAsync("784-1234-1234567-1", 2);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPatientByEmiratesIdAsync_NonExistentEmiratesId_ReturnsNull()
    {
        // Act
        var result = await _patientService.GetPatientByEmiratesIdAsync("784-9999-9999999-9", 1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPatientsByBranchIdAsync Tests

    [Fact]
    public async Task GetPatientsByBranchIdAsync_ReturnsActivePatientsOnly()
    {
        // Act
        var result = await _patientService.GetPatientsByBranchIdAsync(1);

        // Assert
        var patients = result.ToList();
        patients.Should().HaveCount(2); // Excludes deleted patient
        patients.Should().OnlyContain(p => !p.IsDeleted);
    }

    [Fact]
    public async Task GetPatientsByBranchIdAsync_ReturnsOnlyBranchPatients()
    {
        // Act
        var result = await _patientService.GetPatientsByBranchIdAsync(2);

        // Assert
        var patients = result.ToList();
        patients.Should().HaveCount(1);
        patients.First().FullNameEn.Should().Be("Other Branch Patient");
    }

    [Fact]
    public async Task GetPatientsByBranchIdAsync_EmptyBranch_ReturnsEmpty()
    {
        // Act
        var result = await _patientService.GetPatientsByBranchIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPatientsByBranchIdAsync_OrderedByName()
    {
        // Act
        var result = await _patientService.GetPatientsByBranchIdAsync(1);

        // Assert
        var patients = result.ToList();
        patients.First().FullNameEn.Should().Be("Jane Smith");
        patients.Last().FullNameEn.Should().Be("John Doe");
    }

    #endregion

    #region SearchPatientsAsync Tests

    [Fact]
    public async Task SearchPatientsAsync_ByName_ReturnsMatches()
    {
        // Act
        var result = await _patientService.SearchPatientsAsync(1, "John");

        // Assert
        var patients = result.ToList();
        patients.Should().HaveCount(1);
        patients.First().FullNameEn.Should().Be("John Doe");
    }

    [Fact]
    public async Task SearchPatientsAsync_ByArabicName_ReturnsMatches()
    {
        // Act
        var result = await _patientService.SearchPatientsAsync(1, "جون");

        // Assert
        var patients = result.ToList();
        patients.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchPatientsAsync_ByEmiratesId_ReturnsMatches()
    {
        // Act
        var result = await _patientService.SearchPatientsAsync(1, "784-1234");

        // Assert
        var patients = result.ToList();
        patients.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchPatientsAsync_ByPhone_ReturnsMatches()
    {
        // Act
        var result = await _patientService.SearchPatientsAsync(1, "501234567");

        // Assert
        var patients = result.ToList();
        patients.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchPatientsAsync_NoMatches_ReturnsEmpty()
    {
        // Act
        var result = await _patientService.SearchPatientsAsync(1, "nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchPatientsAsync_ExcludesDeletedPatients()
    {
        // Act
        var result = await _patientService.SearchPatientsAsync(1, "Deleted");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreatePatientAsync Tests

    [Fact]
    public async Task CreatePatientAsync_ValidPatient_CreatesSuccessfully()
    {
        // Arrange
        var newPatient = new Core.Entities.Patient
        {
            BranchId = 1,
            EmiratesId = "784-5678-5678901-5",
            FullNameEn = "New Patient",
            DateOfBirth = new DateTime(1995, 3, 10),
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _patientService.CreatePatientAsync(newPatient);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var savedPatient = await _patientService.GetPatientByIdAsync(result.Id);
        savedPatient.Should().NotBeNull();
        savedPatient!.EmiratesId.Should().Be("784-5678-5678901-5");
    }

    [Fact]
    public async Task CreatePatientAsync_DuplicateEmiratesId_ThrowsException()
    {
        // Arrange
        var duplicatePatient = new Core.Entities.Patient
        {
            BranchId = 1,
            EmiratesId = "784-1234-1234567-1", // Existing Emirates ID
            FullNameEn = "Duplicate Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M"
        };

        // Act
        var action = () => _patientService.CreatePatientAsync(duplicatePatient);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreatePatientAsync_SameEmiratesIdDifferentBranch_Succeeds()
    {
        // Arrange
        var patient = new Core.Entities.Patient
        {
            BranchId = 2, // Different branch
            EmiratesId = "784-1234-1234567-1", // Same as Branch 1 patient
            FullNameEn = "Same ID Different Branch",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M"
        };

        // Act
        var result = await _patientService.CreatePatientAsync(patient);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region UpdatePatientAsync Tests

    [Fact]
    public async Task UpdatePatientAsync_ValidUpdate_UpdatesSuccessfully()
    {
        // Arrange
        var patient = await _patientService.GetPatientByIdAsync(1);
        patient!.FullNameEn = "John Doe Updated";
        patient.PhoneNumber = "+971509999999";

        // Act
        await _patientService.UpdatePatientAsync(patient);

        // Assert
        var updatedPatient = await _patientService.GetPatientByIdAsync(1);
        updatedPatient!.FullNameEn.Should().Be("John Doe Updated");
        updatedPatient.PhoneNumber.Should().Be("+971509999999");
        updatedPatient.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdatePatientAsync_DuplicateEmiratesId_ThrowsException()
    {
        // Arrange
        var patient = await _patientService.GetPatientByIdAsync(2);
        patient!.EmiratesId = "784-1234-1234567-1"; // Already used by patient 1

        // Act
        var action = () => _patientService.UpdatePatientAsync(patient);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    #endregion

    #region DeletePatientAsync Tests

    [Fact]
    public async Task DeletePatientAsync_ExistingPatient_SoftDeletes()
    {
        // Act
        await _patientService.DeletePatientAsync(2);

        // Assert
        var deletedPatient = await _context.Patients.FindAsync(2);
        deletedPatient.Should().NotBeNull();
        deletedPatient!.IsDeleted.Should().BeTrue();
        deletedPatient.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeletePatientAsync_NonExistentPatient_NoError()
    {
        // Act
        var action = () => _patientService.DeletePatientAsync(999);

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Medical History Tests

    [Fact]
    public async Task GetPatientMedicalHistoryAsync_ExistingHistory_ReturnsHistory()
    {
        // Act
        var result = await _patientService.GetPatientMedicalHistoryAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.ChronicConditions.Should().Be("Diabetes Type 2");
        result.Allergies.Should().Be("Penicillin");
    }

    [Fact]
    public async Task GetPatientMedicalHistoryAsync_NoHistory_ReturnsNull()
    {
        // Act
        var result = await _patientService.GetPatientMedicalHistoryAsync(2);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrUpdateMedicalHistoryAsync_NewHistory_Creates()
    {
        // Arrange
        var newHistory = new PatientMedicalHistory
        {
            PatientId = 2,
            ChronicConditions = "Hypertension",
            Allergies = "None",
            IsSmoker = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _patientService.CreateOrUpdateMedicalHistoryAsync(newHistory);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.ChronicConditions.Should().Be("Hypertension");
    }

    [Fact]
    public async Task CreateOrUpdateMedicalHistoryAsync_ExistingHistory_Updates()
    {
        // Arrange
        var updateHistory = new PatientMedicalHistory
        {
            PatientId = 1,
            ChronicConditions = "Diabetes Type 2, Asthma",
            Allergies = "Penicillin, Sulfa",
            UpdatedBy = "doctor"
        };

        // Act
        var result = await _patientService.CreateOrUpdateMedicalHistoryAsync(updateHistory);

        // Assert
        result.Should().NotBeNull();
        result.ChronicConditions.Should().Be("Diabetes Type 2, Asthma");
        result.Allergies.Should().Be("Penicillin, Sulfa");
        result.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Document Tests

    [Fact]
    public async Task GetDocumentByIdAsync_ExistingDocument_ReturnsDocument()
    {
        // Act
        var result = await _patientService.GetDocumentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.DocumentName.Should().Be("ID Copy");
        result.Patient.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPatientDocumentsAsync_ExistingPatient_ReturnsDocuments()
    {
        // Act
        var result = await _patientService.GetPatientDocumentsAsync(1);

        // Assert
        var documents = result.ToList();
        documents.Should().HaveCount(2);
        // Should be ordered by upload date descending
        documents.First().DocumentName.Should().Be("Lab Report");
    }

    [Fact]
    public async Task GetPatientDocumentsAsync_NoDocuments_ReturnsEmpty()
    {
        // Act
        var result = await _patientService.GetPatientDocumentsAsync(2);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadDocumentAsync_ValidDocument_CreatesSuccessfully()
    {
        // Arrange
        var newDocument = new PatientDocument
        {
            PatientId = 2,
            DocumentName = "New Document",
            DocumentType = "Other",
            FileName = "new_doc.pdf",
            FileExtension = ".pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 512,
            UploadDate = DateTime.UtcNow,
            UploadedBy = "user1",
            IsActive = true
        };

        // Act
        var result = await _patientService.UploadDocumentAsync(newDocument);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ExistingDocument_Deletes()
    {
        // Act
        await _patientService.DeleteDocumentAsync(2);

        // Assert
        var deletedDoc = await _patientService.GetDocumentByIdAsync(2);
        deletedDoc.Should().BeNull();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalPatientsCountAsync_ReturnsActiveCount()
    {
        // Act
        var result = await _patientService.GetTotalPatientsCountAsync(1);

        // Assert
        result.Should().Be(2); // Excludes deleted patient
    }

    [Fact]
    public async Task GetNewPatientsCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-4);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _patientService.GetNewPatientsCountAsync(1, startDate, endDate);

        // Assert
        result.Should().Be(2); // Both patients created within range
    }

    [Fact]
    public async Task GetPatientsByGenderDistributionAsync_ReturnsDistribution()
    {
        // Act
        var result = await _patientService.GetPatientsByGenderDistributionAsync(1);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().ContainKey("M");
        result.Should().ContainKey("F");
        result["M"].Should().Be(1);
        result["F"].Should().Be(1);
    }

    #endregion
}
