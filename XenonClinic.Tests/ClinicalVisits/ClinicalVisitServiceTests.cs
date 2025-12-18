using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Entities.Dental;
using XenonClinic.Core.Entities.Cardiology;
using XenonClinic.Core.Entities.Ophthalmology;
using XenonClinic.Core.Entities.Physiotherapy;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;
using PatientEntity = XenonClinic.Core.Entities.Patient;

namespace XenonClinic.Tests.ClinicalVisits;

/// <summary>
/// Tests for ClinicalVisitService.
/// </summary>
public class ClinicalVisitServiceTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly ClinicalVisitService _service;
    private readonly int _testBranchId = 1;
    private readonly int _testPatientId = 1;

    public ClinicalVisitServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _service = new ClinicalVisitService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test patient
        var patient = new PatientEntity
        {
            Id = _testPatientId,
            BranchId = _testBranchId,
            FullNameEn = "Test Patient",
            EmiratesId = "784-1234-5678901-1",
            DateOfBirth = new DateTime(1985, 5, 15),
            Gender = "Male"
        };
        _context.Patients.Add(patient);

        // Create test branch
        var branch = new Branch
        {
            Id = _testBranchId,
            CompanyId = 1,
            NameEn = "Test Branch",
            Code = "TB001"
        };
        _context.Branches.Add(branch);

        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Audiology Visit Tests

    [Fact]
    public async Task CreateAudiologyVisitAsync_ValidVisit_CreatesSuccessfully()
    {
        // Arrange
        var visit = new AudiologyVisit
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "Hearing loss",
            Diagnosis = "Sensorineural hearing loss",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateAudiologyVisitAsync(visit);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.PatientId.Should().Be(_testPatientId);
        result.ChiefComplaint.Should().Be("Hearing loss");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetAudiologyVisitByIdAsync_ExistingVisit_ReturnsVisit()
    {
        // Arrange
        var visit = new AudiologyVisit
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "Tinnitus"
        };
        _context.AudiologyVisits.Add(visit);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAudiologyVisitByIdAsync(visit.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ChiefComplaint.Should().Be("Tinnitus");
    }

    [Fact]
    public async Task GetAudiologyVisitByIdAsync_NonExistingVisit_ReturnsNull()
    {
        // Act
        var result = await _service.GetAudiologyVisitByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAudiologyVisitsByPatientAsync_MultipleVisits_ReturnsAllOrderedByDate()
    {
        // Arrange
        var visits = new[]
        {
            new AudiologyVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow.AddDays(-2), ChiefComplaint = "Visit 1" },
            new AudiologyVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Visit 2" },
            new AudiologyVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow.AddDays(-1), ChiefComplaint = "Visit 3" }
        };
        _context.AudiologyVisits.AddRange(visits);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetAudiologyVisitsByPatientAsync(_testPatientId, _testBranchId)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].ChiefComplaint.Should().Be("Visit 2"); // Most recent first
    }

    [Fact]
    public async Task DeleteAudiologyVisitAsync_ExistingVisit_DeletesSuccessfully()
    {
        // Arrange
        var visit = new AudiologyVisit
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "To be deleted"
        };
        _context.AudiologyVisits.Add(visit);
        await _context.SaveChangesAsync();
        var visitId = visit.Id;

        // Act
        await _service.DeleteAudiologyVisitAsync(visitId);

        // Assert
        var deleted = await _context.AudiologyVisits.FindAsync(visitId);
        deleted.Should().BeNull();
    }

    #endregion

    #region Dental Visit Tests

    [Fact]
    public async Task CreateDentalVisitAsync_ValidVisit_CreatesSuccessfully()
    {
        // Arrange
        var visit = new DentalVisit
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "Toothache",
            Diagnosis = "Dental caries",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateDentalVisitAsync(visit);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Diagnosis.Should().Be("Dental caries");
    }

    [Fact]
    public async Task GetDentalVisitsByBranchAsync_WithDateFilter_ReturnsFilteredVisits()
    {
        // Arrange
        var visits = new[]
        {
            new DentalVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow.AddDays(-30), ChiefComplaint = "Old visit" },
            new DentalVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow.AddDays(-5), ChiefComplaint = "Recent visit" },
            new DentalVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Today visit" }
        };
        _context.DentalVisits.AddRange(visits);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetDentalVisitsByBranchAsync(
            _testBranchId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow.AddDays(1))).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(v => v.ChiefComplaint == "Old visit");
    }

    [Fact]
    public async Task AddDentalProcedureAsync_ValidProcedure_AddsSuccessfully()
    {
        // Arrange
        var visit = new DentalVisit
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "Filling needed"
        };
        _context.DentalVisits.Add(visit);
        await _context.SaveChangesAsync();

        var procedure = new DentalProcedure
        {
            DentalVisitId = visit.Id,
            Code = "D2140",
            Name = "Amalgam Filling",
            ToothNumber = "14",
            Cost = 150.00m
        };

        // Act
        await _service.AddDentalProcedureAsync(procedure);

        // Assert
        var savedProcedure = await _context.DentalProcedures.FirstOrDefaultAsync(p => p.DentalVisitId == visit.Id);
        savedProcedure.Should().NotBeNull();
        savedProcedure!.Code.Should().Be("D2140");
    }

    #endregion

    #region Cardiology Visit Tests

    [Fact]
    public async Task CreateCardioVisitAsync_ValidVisit_CreatesSuccessfully()
    {
        // Arrange
        var visit = new CardioVisit
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "Chest pain",
            SystolicBP = 120,
            DiastolicBP = 80,
            HeartRate = 72,
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateCardioVisitAsync(visit);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.SystolicBP.Should().Be(120);
        result.HeartRate.Should().Be(72);
    }

    [Fact]
    public async Task GetCardioVisitsByPatientAsync_ReturnsVisitsForPatient()
    {
        // Arrange
        var visits = new[]
        {
            new CardioVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Checkup" },
            new CardioVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow.AddDays(-7), ChiefComplaint = "Follow-up" },
            new CardioVisit { PatientId = 999, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Other patient" }
        };
        _context.CardioVisits.AddRange(visits);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetCardioVisitsByPatientAsync(_testPatientId, _testBranchId)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(v => v.PatientId == _testPatientId);
    }

    [Fact]
    public async Task CreateECGAsync_ValidECG_CreatesSuccessfully()
    {
        // Arrange
        var ecg = new ECGRecord
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            RecordDate = DateTime.UtcNow,
            HeartRate = 75,
            Interpretation = "Normal sinus rhythm",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateECGAsync(ecg);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.HeartRate.Should().Be(75);
    }

    #endregion

    #region Ophthalmology Visit Tests

    [Fact]
    public async Task CreateOphthalmologyVisitAsync_ValidVisit_CreatesSuccessfully()
    {
        // Arrange
        var visit = new OphthalmologyVisit
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "Blurred vision",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateOphthalmologyVisitAsync(visit);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.ChiefComplaint.Should().Be("Blurred vision");
    }

    [Fact]
    public async Task CreateEyePrescriptionAsync_ValidPrescription_CreatesSuccessfully()
    {
        // Arrange
        var prescription = new EyePrescription
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            PrescriptionDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddYears(2),
            SphereOd = -2.50m,
            SphereOs = -2.25m,
            CylinderOd = -0.75m,
            CylinderOs = -0.50m,
            LensType = "Progressive",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreateEyePrescriptionAsync(prescription);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.SphereOd.Should().Be(-2.50m);
    }

    [Fact]
    public async Task GetEyePrescriptionsByPatientAsync_ReturnsActiveOnly()
    {
        // Arrange
        var prescriptions = new[]
        {
            new EyePrescription { PatientId = _testPatientId, BranchId = _testBranchId, PrescriptionDate = DateTime.UtcNow, IsActive = true, ExpiryDate = DateTime.UtcNow.AddYears(1) },
            new EyePrescription { PatientId = _testPatientId, BranchId = _testBranchId, PrescriptionDate = DateTime.UtcNow.AddYears(-2), IsActive = false, ExpiryDate = DateTime.UtcNow.AddYears(-1) }
        };
        _context.Set<EyePrescription>().AddRange(prescriptions);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetEyePrescriptionsByPatientAsync(_testPatientId, _testBranchId)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].IsActive.Should().BeTrue();
    }

    #endregion

    #region Physiotherapy Session Tests

    [Fact]
    public async Task CreatePhysioSessionAsync_ValidSession_CreatesSuccessfully()
    {
        // Arrange
        var session = new PhysioSession
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            SessionDate = DateTime.UtcNow,
            SessionNumber = 1,
            DurationMinutes = 45,
            PainLevelBefore = 7,
            PainLevelAfter = 4,
            Status = "Completed",
            CreatedBy = "test-user"
        };

        // Act
        var result = await _service.CreatePhysioSessionAsync(session);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.DurationMinutes.Should().Be(45);
        result.PainLevelBefore.Should().Be(7);
    }

    [Fact]
    public async Task GetPhysioSessionsByBranchAsync_WithDateFilter_ReturnsFilteredSessions()
    {
        // Arrange
        var sessions = new[]
        {
            new PhysioSession { PatientId = _testPatientId, BranchId = _testBranchId, SessionDate = DateTime.UtcNow.AddDays(-30), SessionNumber = 1, DurationMinutes = 30 },
            new PhysioSession { PatientId = _testPatientId, BranchId = _testBranchId, SessionDate = DateTime.UtcNow.AddDays(-5), SessionNumber = 2, DurationMinutes = 30 },
            new PhysioSession { PatientId = _testPatientId, BranchId = _testBranchId, SessionDate = DateTime.UtcNow, SessionNumber = 3, DurationMinutes = 30 }
        };
        _context.PhysioSessions.AddRange(sessions);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetPhysioSessionsByBranchAsync(
            _testBranchId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow.AddDays(1))).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdatePhysioSessionAsync_ValidUpdate_UpdatesSuccessfully()
    {
        // Arrange
        var session = new PhysioSession
        {
            PatientId = _testPatientId,
            BranchId = _testBranchId,
            SessionDate = DateTime.UtcNow,
            SessionNumber = 1,
            DurationMinutes = 30,
            PainLevelBefore = 6
        };
        _context.PhysioSessions.Add(session);
        await _context.SaveChangesAsync();
        _context.Entry(session).State = EntityState.Detached;

        // Act
        session.PainLevelAfter = 3;
        session.PatientResponse = "Good improvement";
        await _service.UpdatePhysioSessionAsync(session);

        // Assert
        var updated = await _context.PhysioSessions.FindAsync(session.Id);
        updated!.PainLevelAfter.Should().Be(3);
        updated.PatientResponse.Should().Be("Good improvement");
        updated.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectCounts()
    {
        // Arrange
        _context.AudiologyVisits.Add(new AudiologyVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Test" });
        _context.DentalVisits.Add(new DentalVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Test" });
        _context.DentalVisits.Add(new DentalVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Test 2" });
        _context.CardioVisits.Add(new CardioVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Test" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStatisticsAsync(_testBranchId);

        // Assert
        result.Should().NotBeNull();
        result.VisitsBySpecialty["Audiology"].Should().Be(1);
        result.VisitsBySpecialty["Dental"].Should().Be(2);
        result.VisitsBySpecialty["Cardiology"].Should().Be(1);
        result.TotalVisits.Should().Be(4);
    }

    [Fact]
    public async Task GetTotalVisitsCountAsync_BySpecialty_ReturnsCorrectCount()
    {
        // Arrange
        _context.DentalVisits.AddRange(
            new DentalVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Test 1" },
            new DentalVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Test 2" },
            new DentalVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = DateTime.UtcNow, ChiefComplaint = "Test 3" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTotalVisitsCountAsync(_testBranchId, "dental");

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task GetVisitsBySpecialtyAsync_WithDateRange_ReturnsFilteredCounts()
    {
        // Arrange
        var today = DateTime.UtcNow;
        _context.AudiologyVisits.Add(new AudiologyVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = today, ChiefComplaint = "Recent" });
        _context.AudiologyVisits.Add(new AudiologyVisit { PatientId = _testPatientId, BranchId = _testBranchId, VisitDate = today.AddDays(-60), ChiefComplaint = "Old" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetVisitsBySpecialtyAsync(_testBranchId, today.AddDays(-7), today.AddDays(1));

        // Assert
        result["Audiology"].Should().Be(1);
    }

    #endregion
}
