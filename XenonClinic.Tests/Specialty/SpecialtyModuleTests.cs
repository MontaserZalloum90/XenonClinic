using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Entities.Cardiology;
using XenonClinic.Core.Entities.Dental;
using XenonClinic.Core.Entities.Ophthalmology;
using XenonClinic.Core.Entities.Dermatology;
using XenonClinic.Infrastructure.Data;
using Xunit;
using PatientEntity = XenonClinic.Core.Entities.Patient;

namespace XenonClinic.Tests.Specialty;

/// <summary>
/// Extended comprehensive tests for all Specialty Module implementations.
/// Tests covering Audiology, Dental, Cardiology, Ophthalmology, Dermatology, and more.
/// </summary>
public class SpecialtyModuleTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        await SeedExtendedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    private async Task SeedExtendedTestDataAsync()
    {
        var company = new Company { Id = 1, TenantId = 1, Name = "Test Clinic", Code = "TC001", IsActive = true };
        _context.Companies.Add(company);

        var branch = new Branch { Id = 1, CompanyId = 1, Code = "BR001", Name = "Main Branch", IsActive = true };
        _context.Branches.Add(branch);

        // Seed patients
        var patients = new List<PatientEntity>();
        for (int i = 1; i <= 100; i++)
        {
            patients.Add(new PatientEntity
            {
                Id = i,
                BranchId = 1,
                EmiratesId = $"784-{i:D4}-{i:D7}-{i % 10}",
                FullNameEn = $"Specialty Patient {i}",
                DateOfBirth = new DateTime(1950 + (i % 50), (i % 12) + 1, (i % 28) + 1),
                Gender = i % 2 == 0 ? "M" : "F",
                CreatedAt = DateTime.UtcNow
            });
        }
        _context.Patients.AddRange(patients);

        // Seed Audiology records
        var audiologyRecords = new List<AudiologyVisit>();
        for (int i = 1; i <= 100; i++)
        {
            audiologyRecords.Add(new AudiologyVisit
            {
                Id = i,
                PatientId = (i % 100) + 1,
                BranchId = 1,
                VisitDate = DateTime.UtcNow.AddDays(-i),
                ChiefComplaint = i % 3 == 0 ? "Hearing loss" : i % 3 == 1 ? "Tinnitus" : "Ear pain",
                Diagnosis = $"Audiology diagnosis {i}",
                Plan = $"Treatment plan {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.AudiologyVisits.AddRange(audiologyRecords);

        // Seed Dental records
        var dentalRecords = new List<DentalVisit>();
        for (int i = 1; i <= 100; i++)
        {
            dentalRecords.Add(new DentalVisit
            {
                Id = i,
                PatientId = (i % 100) + 1,
                BranchId = 1,
                VisitDate = DateTime.UtcNow.AddDays(-i),
                ChiefComplaint = i % 4 == 0 ? "Toothache" : i % 4 == 1 ? "Cleaning" : i % 4 == 2 ? "Filling" : "Checkup",
                TreatmentPlan = $"Treatment plan {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.DentalVisits.AddRange(dentalRecords);

        // Seed Dental procedures
        var dentalProcedures = new List<DentalProcedure>();
        for (int i = 1; i <= 200; i++)
        {
            dentalProcedures.Add(new DentalProcedure
            {
                Id = i,
                DentalVisitId = (i % 100) + 1,
                ToothNumber = (i % 32) + 1,
                ProcedureType = new[] { "Filling", "Extraction", "Root Canal", "Crown", "Cleaning", "Whitening" }[i % 6],
                Surface = new[] { "Mesial", "Distal", "Occlusal", "Buccal", "Lingual" }[i % 5],
                Material = i % 3 == 0 ? "Composite" : i % 3 == 1 ? "Amalgam" : "Ceramic",
                Notes = $"Procedure notes {i}",
                PerformedDate = DateTime.UtcNow.AddDays(-(i % 100)),
                CreatedAt = DateTime.UtcNow.AddDays(-(i % 100))
            });
        }
        _context.DentalProcedures.AddRange(dentalProcedures);

        // Seed Cardiology records (using CardioVisit)
        var cardiologyRecords = new List<CardioVisit>();
        for (int i = 1; i <= 100; i++)
        {
            cardiologyRecords.Add(new CardioVisit
            {
                Id = i,
                PatientId = (i % 100) + 1,
                BranchId = 1,
                VisitDate = DateTime.UtcNow.AddDays(-i),
                ChiefComplaint = i % 4 == 0 ? "Chest pain" : i % 4 == 1 ? "Palpitations" : i % 4 == 2 ? "Shortness of breath" : "Routine checkup",
                HeartRate = 60 + (i % 40),
                SystolicBP = 110 + (i % 40),
                DiastolicBP = 70 + (i % 30),
                HeartSounds = i % 6 == 0 ? "Murmur present" : "Normal",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.CardioVisits.AddRange(cardiologyRecords);

        // Seed ECG records
        var ecgRecords = new List<ECGRecord>();
        for (int i = 1; i <= 100; i++)
        {
            ecgRecords.Add(new ECGRecord
            {
                Id = i,
                CardioVisitId = i,
                RecordingDate = DateTime.UtcNow.AddDays(-i),
                HeartRate = 60 + (i % 40),
                PRInterval = 120 + (i % 80),
                QRSDuration = 80 + (i % 40),
                QTInterval = 350 + (i % 100),
                Rhythm = i % 5 == 0 ? "Atrial Fibrillation" : i % 5 == 1 ? "Sinus Tachycardia" : "Normal Sinus",
                Interpretation = $"ECG interpretation {i}",
                IsAbnormal = i % 5 == 0,
                PerformedBy = $"Cardiologist {i % 5}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.ECGRecords.AddRange(ecgRecords);

        // Seed Ophthalmology records
        var ophthalmologyRecords = new List<OphthalmologyVisit>();
        for (int i = 1; i <= 100; i++)
        {
            ophthalmologyRecords.Add(new OphthalmologyVisit
            {
                Id = i,
                PatientId = (i % 100) + 1,
                BranchId = 1,
                VisitDate = DateTime.UtcNow.AddDays(-i),
                ChiefComplaint = i % 4 == 0 ? "Blurred vision" : i % 4 == 1 ? "Eye pain" : i % 4 == 2 ? "Red eye" : "Routine exam",
                RightEyeVisualAcuity = $"20/{20 + (i % 80)}",
                LeftEyeVisualAcuity = $"20/{20 + (i % 80)}",
                RightEyeIOP = 12 + (i % 10),
                LeftEyeIOP = 12 + (i % 10),
                PupilReaction = i % 10 == 0 ? "Sluggish" : "Normal",
                FundusExamination = $"Fundus findings {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.OphthalmologyVisits.AddRange(ophthalmologyRecords);

        // Seed Vision tests
        var visionTests = new List<VisionTest>();
        for (int i = 1; i <= 100; i++)
        {
            visionTests.Add(new VisionTest
            {
                Id = i,
                OphthalmologyVisitId = i,
                TestDate = DateTime.UtcNow.AddDays(-i),
                TestType = new[] { "Visual Acuity", "Color Vision", "Visual Field", "Refraction" }[i % 4],
                RightEyeResult = $"OD: {i % 10 + 1}/{10}",
                LeftEyeResult = $"OS: {i % 10 + 1}/{10}",
                Notes = $"Vision test notes {i}",
                PerformedBy = $"Optometrist {i % 5}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.VisionTests.AddRange(visionTests);

        // Seed Dermatology records
        var dermatologyRecords = new List<DermatologyVisit>();
        for (int i = 1; i <= 100; i++)
        {
            dermatologyRecords.Add(new DermatologyVisit
            {
                Id = i,
                PatientId = (i % 100) + 1,
                BranchId = 1,
                VisitDate = DateTime.UtcNow.AddDays(-i),
                ChiefComplaint = i % 5 == 0 ? "Acne" : i % 5 == 1 ? "Eczema" : i % 5 == 2 ? "Psoriasis" : i % 5 == 3 ? "Rash" : "Mole evaluation",
                AffectedAreas = new[] { "Face", "Arms", "Legs", "Back", "Chest", "Scalp" }[i % 6],
                LesionType = new[] { "Papule", "Macule", "Nodule", "Vesicle", "Plaque" }[i % 5],
                LesionColor = new[] { "Red", "Brown", "White", "Pink", "Purple" }[i % 5],
                LesionSize = $"{i % 5 + 1}cm",
                BiopsyRequired = i % 10 == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.DermatologyVisits.AddRange(dermatologyRecords);

        await _context.SaveChangesAsync();
    }

    #region Audiology Tests

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task AudiologyVisit_GetById_ReturnsVisit(int visitId)
    {
        var result = await _context.AudiologyVisits.FindAsync(visitId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(visitId);
    }

    [Theory]
    [InlineData("Hearing loss")]
    [InlineData("Tinnitus")]
    [InlineData("Ear pain")]
    public async Task AudiologyVisit_FilterByChiefComplaint_ReturnsCorrectVisits(string complaint)
    {
        var result = await _context.AudiologyVisits
            .Where(v => v.ChiefComplaint == complaint)
            .ToListAsync();

        result.Should().OnlyContain(v => v.ChiefComplaint == complaint);
    }

    [Fact]
    public async Task AudiologyVisit_Create_Succeeds()
    {
        var newVisit = new AudiologyVisit
        {
            PatientId = 1,
            BranchId = 1,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "New hearing complaint",
            Diagnosis = "Sensorineural hearing loss"
        };

        _context.AudiologyVisits.Add(newVisit);
        await _context.SaveChangesAsync();

        newVisit.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Dental Tests

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task DentalVisit_GetById_ReturnsVisit(int visitId)
    {
        var result = await _context.DentalVisits.FindAsync(visitId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(visitId);
    }

    [Theory]
    [InlineData("Toothache")]
    [InlineData("Cleaning")]
    [InlineData("Filling")]
    [InlineData("Checkup")]
    public async Task DentalVisit_FilterByComplaint_ReturnsCorrectVisits(string complaint)
    {
        var result = await _context.DentalVisits
            .Where(v => v.ChiefComplaint == complaint)
            .ToListAsync();

        result.Should().OnlyContain(v => v.ChiefComplaint == complaint);
    }

    [Fact]
    public async Task DentalVisit_Create_Succeeds()
    {
        var newVisit = new DentalVisit
        {
            PatientId = 1,
            BranchId = 1,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "New dental complaint",
            TreatmentPlan = "Treatment plan"
        };

        _context.DentalVisits.Add(newVisit);
        await _context.SaveChangesAsync();

        newVisit.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Filling")]
    [InlineData("Extraction")]
    [InlineData("Root Canal")]
    [InlineData("Crown")]
    [InlineData("Cleaning")]
    [InlineData("Whitening")]
    public async Task DentalProcedure_FilterByType_ReturnsCorrectProcedures(string procedureType)
    {
        var result = await _context.DentalProcedures
            .Where(p => p.ProcedureType == procedureType)
            .ToListAsync();

        result.Should().OnlyContain(p => p.ProcedureType == procedureType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(24)]
    [InlineData(32)]
    public async Task DentalProcedure_FilterByToothNumber_ReturnsCorrectProcedures(int toothNumber)
    {
        var result = await _context.DentalProcedures
            .Where(p => p.ToothNumber == toothNumber)
            .ToListAsync();

        result.Should().OnlyContain(p => p.ToothNumber == toothNumber);
    }

    [Theory]
    [InlineData("Mesial")]
    [InlineData("Distal")]
    [InlineData("Occlusal")]
    [InlineData("Buccal")]
    [InlineData("Lingual")]
    public async Task DentalProcedure_FilterBySurface_ReturnsCorrectProcedures(string surface)
    {
        var result = await _context.DentalProcedures
            .Where(p => p.Surface == surface)
            .ToListAsync();

        result.Should().OnlyContain(p => p.Surface == surface);
    }

    [Fact]
    public async Task DentalProcedure_Create_Succeeds()
    {
        var newProcedure = new DentalProcedure
        {
            DentalVisitId = 1,
            ToothNumber = 15,
            ProcedureType = "Filling",
            Surface = "Occlusal",
            Material = "Composite",
            PerformedDate = DateTime.UtcNow
        };

        _context.DentalProcedures.Add(newProcedure);
        await _context.SaveChangesAsync();

        newProcedure.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DentalProcedure_GetByVisitId_ReturnsProcedures()
    {
        var result = await _context.DentalProcedures
            .Where(p => p.DentalVisitId == 1)
            .ToListAsync();

        result.Should().OnlyContain(p => p.DentalVisitId == 1);
    }

    #endregion

    #region Cardiology Tests

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task CardioVisit_GetById_ReturnsVisit(int visitId)
    {
        var result = await _context.CardioVisits.FindAsync(visitId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(visitId);
    }

    [Theory]
    [InlineData("Chest pain")]
    [InlineData("Palpitations")]
    [InlineData("Shortness of breath")]
    [InlineData("Routine checkup")]
    public async Task CardioVisit_FilterByComplaint_ReturnsCorrectVisits(string complaint)
    {
        var result = await _context.CardioVisits
            .Where(v => v.ChiefComplaint == complaint)
            .ToListAsync();

        result.Should().OnlyContain(v => v.ChiefComplaint == complaint);
    }

    [Fact]
    public async Task CardioVisit_Create_Succeeds()
    {
        var newVisit = new CardioVisit
        {
            PatientId = 1,
            BranchId = 1,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "New cardiac complaint",
            HeartRate = 75,
            SystolicBP = 120,
            DiastolicBP = 80
        };

        _context.CardioVisits.Add(newVisit);
        await _context.SaveChangesAsync();

        newVisit.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ECGRecord_GetByVisitId_ReturnsRecords()
    {
        var result = await _context.ECGRecords
            .Where(e => e.CardioVisitId == 1)
            .ToListAsync();

        result.Should().OnlyContain(e => e.CardioVisitId == 1);
    }

    [Fact]
    public async Task ECGRecord_FilterByAbnormal_ReturnsCorrectRecords()
    {
        var result = await _context.ECGRecords
            .Where(e => e.IsAbnormal)
            .ToListAsync();

        result.Should().OnlyContain(e => e.IsAbnormal);
    }

    [Theory]
    [InlineData("Normal Sinus")]
    [InlineData("Atrial Fibrillation")]
    [InlineData("Sinus Tachycardia")]
    public async Task ECGRecord_FilterByRhythm_ReturnsCorrectRecords(string rhythm)
    {
        var result = await _context.ECGRecords
            .Where(e => e.Rhythm == rhythm)
            .ToListAsync();

        result.Should().OnlyContain(e => e.Rhythm == rhythm);
    }

    [Fact]
    public async Task ECGRecord_Create_Succeeds()
    {
        var newECG = new ECGRecord
        {
            CardioVisitId = 1,
            RecordingDate = DateTime.UtcNow,
            HeartRate = 72,
            PRInterval = 160,
            QRSDuration = 90,
            QTInterval = 400,
            Rhythm = "Normal Sinus",
            Interpretation = "Normal ECG",
            IsAbnormal = false,
            PerformedBy = "Cardiologist"
        };

        _context.ECGRecords.Add(newECG);
        await _context.SaveChangesAsync();

        newECG.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Ophthalmology Tests

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task OphthalmologyVisit_GetById_ReturnsVisit(int visitId)
    {
        var result = await _context.OphthalmologyVisits.FindAsync(visitId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(visitId);
    }

    [Theory]
    [InlineData("Blurred vision")]
    [InlineData("Eye pain")]
    [InlineData("Red eye")]
    [InlineData("Routine exam")]
    public async Task OphthalmologyVisit_FilterByComplaint_ReturnsCorrectVisits(string complaint)
    {
        var result = await _context.OphthalmologyVisits
            .Where(v => v.ChiefComplaint == complaint)
            .ToListAsync();

        result.Should().OnlyContain(v => v.ChiefComplaint == complaint);
    }

    [Theory]
    [InlineData("Normal")]
    [InlineData("Sluggish")]
    public async Task OphthalmologyVisit_FilterByPupilReaction_ReturnsCorrectVisits(string reaction)
    {
        var result = await _context.OphthalmologyVisits
            .Where(v => v.PupilReaction == reaction)
            .ToListAsync();

        result.Should().OnlyContain(v => v.PupilReaction == reaction);
    }

    [Fact]
    public async Task OphthalmologyVisit_Create_Succeeds()
    {
        var newVisit = new OphthalmologyVisit
        {
            PatientId = 1,
            BranchId = 1,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "New eye complaint",
            RightEyeVisualAcuity = "20/20",
            LeftEyeVisualAcuity = "20/20",
            RightEyeIOP = 15,
            LeftEyeIOP = 15
        };

        _context.OphthalmologyVisits.Add(newVisit);
        await _context.SaveChangesAsync();

        newVisit.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Visual Acuity")]
    [InlineData("Color Vision")]
    [InlineData("Visual Field")]
    [InlineData("Refraction")]
    public async Task VisionTest_FilterByTestType_ReturnsCorrectTests(string testType)
    {
        var result = await _context.VisionTests
            .Where(t => t.TestType == testType)
            .ToListAsync();

        result.Should().OnlyContain(t => t.TestType == testType);
    }

    [Fact]
    public async Task VisionTest_GetByVisitId_ReturnsTests()
    {
        var result = await _context.VisionTests
            .Where(t => t.OphthalmologyVisitId == 1)
            .ToListAsync();

        result.Should().OnlyContain(t => t.OphthalmologyVisitId == 1);
    }

    [Fact]
    public async Task VisionTest_Create_Succeeds()
    {
        var newTest = new VisionTest
        {
            OphthalmologyVisitId = 1,
            TestDate = DateTime.UtcNow,
            TestType = "Visual Acuity",
            RightEyeResult = "20/20",
            LeftEyeResult = "20/25",
            PerformedBy = "Optometrist"
        };

        _context.VisionTests.Add(newTest);
        await _context.SaveChangesAsync();

        newTest.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Dermatology Tests

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task DermatologyVisit_GetById_ReturnsVisit(int visitId)
    {
        var result = await _context.DermatologyVisits.FindAsync(visitId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(visitId);
    }

    [Theory]
    [InlineData("Acne")]
    [InlineData("Eczema")]
    [InlineData("Psoriasis")]
    [InlineData("Rash")]
    [InlineData("Mole evaluation")]
    public async Task DermatologyVisit_FilterByComplaint_ReturnsCorrectVisits(string complaint)
    {
        var result = await _context.DermatologyVisits
            .Where(v => v.ChiefComplaint == complaint)
            .ToListAsync();

        result.Should().OnlyContain(v => v.ChiefComplaint == complaint);
    }

    [Theory]
    [InlineData("Face")]
    [InlineData("Arms")]
    [InlineData("Legs")]
    [InlineData("Back")]
    [InlineData("Chest")]
    [InlineData("Scalp")]
    public async Task DermatologyVisit_FilterByAffectedArea_ReturnsCorrectVisits(string area)
    {
        var result = await _context.DermatologyVisits
            .Where(v => v.AffectedAreas == area)
            .ToListAsync();

        result.Should().OnlyContain(v => v.AffectedAreas == area);
    }

    [Theory]
    [InlineData("Papule")]
    [InlineData("Macule")]
    [InlineData("Nodule")]
    [InlineData("Vesicle")]
    [InlineData("Plaque")]
    public async Task DermatologyVisit_FilterByLesionType_ReturnsCorrectVisits(string lesionType)
    {
        var result = await _context.DermatologyVisits
            .Where(v => v.LesionType == lesionType)
            .ToListAsync();

        result.Should().OnlyContain(v => v.LesionType == lesionType);
    }

    [Fact]
    public async Task DermatologyVisit_FilterByBiopsyRequired_ReturnsCorrectVisits()
    {
        var result = await _context.DermatologyVisits
            .Where(v => v.BiopsyRequired)
            .ToListAsync();

        result.Should().OnlyContain(v => v.BiopsyRequired);
    }

    [Fact]
    public async Task DermatologyVisit_Create_Succeeds()
    {
        var newVisit = new DermatologyVisit
        {
            PatientId = 1,
            BranchId = 1,
            VisitDate = DateTime.UtcNow,
            ChiefComplaint = "New skin complaint",
            AffectedAreas = "Face",
            LesionType = "Papule",
            LesionColor = "Red",
            LesionSize = "2cm"
        };

        _context.DermatologyVisits.Add(newVisit);
        await _context.SaveChangesAsync();

        newVisit.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Cross-Specialty Tests

    [Fact]
    public async Task Patient_HasMultipleSpecialtyVisits()
    {
        var patientId = 1;

        var audiologyVisits = await _context.AudiologyVisits.CountAsync(v => v.PatientId == patientId);
        var dentalVisits = await _context.DentalVisits.CountAsync(v => v.PatientId == patientId);
        var cardiologyVisits = await _context.CardioVisits.CountAsync(v => v.PatientId == patientId);
        var ophthalmologyVisits = await _context.OphthalmologyVisits.CountAsync(v => v.PatientId == patientId);
        var dermatologyVisits = await _context.DermatologyVisits.CountAsync(v => v.PatientId == patientId);

        var totalVisits = audiologyVisits + dentalVisits + cardiologyVisits + ophthalmologyVisits + dermatologyVisits;
        totalVisits.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AllSpecialties_DateRangeQuery_Works()
    {
        var startDate = DateTime.UtcNow.AddDays(-50);
        var endDate = DateTime.UtcNow;

        var audiologyCount = await _context.AudiologyVisits.CountAsync(v => v.VisitDate >= startDate && v.VisitDate <= endDate);
        var dentalCount = await _context.DentalVisits.CountAsync(v => v.VisitDate >= startDate && v.VisitDate <= endDate);
        var cardiologyCount = await _context.CardioVisits.CountAsync(v => v.VisitDate >= startDate && v.VisitDate <= endDate);
        var ophthalmologyCount = await _context.OphthalmologyVisits.CountAsync(v => v.VisitDate >= startDate && v.VisitDate <= endDate);
        var dermatologyCount = await _context.DermatologyVisits.CountAsync(v => v.VisitDate >= startDate && v.VisitDate <= endDate);

        audiologyCount.Should().BeGreaterThan(0);
        dentalCount.Should().BeGreaterThan(0);
        cardiologyCount.Should().BeGreaterThan(0);
        ophthalmologyCount.Should().BeGreaterThan(0);
        dermatologyCount.Should().BeGreaterThan(0);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task AllSpecialties_BulkQuery_PerformsWell()
    {
        var startTime = DateTime.UtcNow;

        var audiologyVisits = await _context.AudiologyVisits.ToListAsync();
        var dentalVisits = await _context.DentalVisits.ToListAsync();
        var cardiologyVisits = await _context.CardioVisits.ToListAsync();
        var ophthalmologyVisits = await _context.OphthalmologyVisits.ToListAsync();
        var dermatologyVisits = await _context.DermatologyVisits.ToListAsync();

        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    #endregion
}
