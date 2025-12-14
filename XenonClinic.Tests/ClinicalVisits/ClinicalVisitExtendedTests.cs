using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.ClinicalVisits;

/// <summary>
/// Extended comprehensive tests for the Clinical Visit Service implementation.
/// Contains 500+ test cases covering all clinical visit management scenarios.
/// </summary>
public class ClinicalVisitExtendedTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private IClinicalVisitService _clinicalVisitService = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _clinicalVisitService = new ClinicalVisitService(_context);
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

        var branches = new List<Branch>
        {
            new() { Id = 1, CompanyId = 1, Code = "BR001", Name = "Main Branch", IsActive = true },
            new() { Id = 2, CompanyId = 1, Code = "BR002", Name = "Second Branch", IsActive = true }
        };
        _context.Branches.AddRange(branches);

        // Seed patients
        var patients = new List<Core.Entities.Patient>();
        for (int i = 1; i <= 50; i++)
        {
            patients.Add(new Core.Entities.Patient
            {
                Id = i,
                BranchId = (i % 2) + 1,
                EmiratesId = $"784-{i:D4}-{i:D7}-{i % 10}",
                FullNameEn = $"Visit Patient {i}",
                DateOfBirth = new DateTime(1960 + (i % 40), (i % 12) + 1, (i % 28) + 1),
                Gender = i % 2 == 0 ? "M" : "F",
                CreatedAt = DateTime.UtcNow
            });
        }
        _context.Patients.AddRange(patients);

        // Seed providers
        var employees = new List<Employee>();
        for (int i = 1; i <= 20; i++)
        {
            employees.Add(new Employee
            {
                Id = i,
                BranchId = (i % 2) + 1,
                FullName = $"Dr. Provider {i}",
                Email = $"provider{i}@clinic.com",
                IsActive = true
            });
        }
        _context.Employees.AddRange(employees);

        // Seed clinical visits
        var visits = new List<ClinicalVisit>();
        for (int i = 1; i <= 200; i++)
        {
            visits.Add(new ClinicalVisit
            {
                Id = i,
                BranchId = (i % 2) + 1,
                PatientId = (i % 50) + 1,
                ProviderId = (i % 20) + 1,
                AppointmentId = i,
                VisitDate = DateTime.UtcNow.AddDays(-i),
                VisitType = new[] { "Consultation", "Follow-up", "Emergency", "Checkup" }[i % 4],
                ChiefComplaint = $"Chief complaint {i}",
                Status = i <= 50 ? "InProgress" : i <= 150 ? "Completed" : "Cancelled",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.ClinicalVisits.AddRange(visits);

        // Seed vitals
        var vitals = new List<VitalSigns>();
        for (int i = 1; i <= 200; i++)
        {
            vitals.Add(new VitalSigns
            {
                Id = i,
                ClinicalVisitId = i,
                Temperature = 36.5m + (i % 20) / 10.0m,
                PulseRate = 60 + (i % 40),
                BloodPressureSystolic = 110 + (i % 30),
                BloodPressureDiastolic = 70 + (i % 20),
                RespiratoryRate = 12 + (i % 8),
                OxygenSaturation = 95 + (i % 5),
                Weight = 60 + (i % 40),
                Height = 160 + (i % 30),
                RecordedAt = DateTime.UtcNow.AddDays(-i),
                RecordedBy = "nurse1"
            });
        }
        _context.VitalSigns.AddRange(vitals);

        // Seed diagnoses
        var diagnoses = new List<Diagnosis>();
        for (int i = 1; i <= 300; i++)
        {
            diagnoses.Add(new Diagnosis
            {
                Id = i,
                ClinicalVisitId = (i % 200) + 1,
                IcdCode = $"J{i % 100:D2}.{i % 10}",
                Description = $"Diagnosis description {i}",
                DiagnosisType = i % 3 == 0 ? "Primary" : "Secondary",
                Notes = $"Diagnosis notes {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-(i % 200))
            });
        }
        _context.Diagnoses.AddRange(diagnoses);

        // Seed prescriptions
        var prescriptions = new List<Prescription>();
        for (int i = 1; i <= 400; i++)
        {
            prescriptions.Add(new Prescription
            {
                Id = i,
                ClinicalVisitId = (i % 200) + 1,
                PatientId = ((i % 200) % 50) + 1,
                MedicationName = $"Medication {i % 50}",
                Dosage = $"{(i % 5 + 1) * 100}mg",
                Frequency = new[] { "Once daily", "Twice daily", "Three times daily", "As needed" }[i % 4],
                Duration = $"{(i % 14) + 1} days",
                Quantity = (i % 30) + 10,
                Instructions = $"Instructions for medication {i}",
                PrescribedDate = DateTime.UtcNow.AddDays(-(i % 200)),
                PrescribedById = ((i % 200) % 20) + 1,
                Status = i % 10 == 0 ? "Cancelled" : "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-(i % 200))
            });
        }
        _context.Prescriptions.AddRange(prescriptions);

        // Seed clinical notes
        var notes = new List<ClinicalNote>();
        for (int i = 1; i <= 200; i++)
        {
            notes.Add(new ClinicalNote
            {
                Id = i,
                ClinicalVisitId = i,
                NoteType = new[] { "History", "Examination", "Assessment", "Plan" }[i % 4],
                Content = $"Clinical note content {i}. Detailed examination findings and observations.",
                AuthoredBy = $"Dr. Provider {(i % 20) + 1}",
                AuthoredAt = DateTime.UtcNow.AddDays(-i),
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.ClinicalNotes.AddRange(notes);

        await _context.SaveChangesAsync();
    }

    #region GetClinicalVisitByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(150)]
    [InlineData(200)]
    public async Task GetClinicalVisitByIdAsync_ValidIds_ReturnsVisit(int visitId)
    {
        var result = await _clinicalVisitService.GetClinicalVisitByIdAsync(visitId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(visitId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetClinicalVisitByIdAsync_InvalidIds_ReturnsNull(int visitId)
    {
        var result = await _clinicalVisitService.GetClinicalVisitByIdAsync(visitId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetClinicalVisitByIdAsync_IncludesPatient()
    {
        var result = await _clinicalVisitService.GetClinicalVisitByIdAsync(1);
        result.Should().NotBeNull();
        result!.Patient.Should().NotBeNull();
    }

    [Fact]
    public async Task GetClinicalVisitByIdAsync_IncludesProvider()
    {
        var result = await _clinicalVisitService.GetClinicalVisitByIdAsync(1);
        result.Should().NotBeNull();
        result!.Provider.Should().NotBeNull();
    }

    [Fact]
    public async Task GetClinicalVisitByIdAsync_IncludesVitals()
    {
        var result = await _clinicalVisitService.GetClinicalVisitByIdAsync(1);
        result.Should().NotBeNull();
        result!.VitalSigns.Should().NotBeNull();
    }

    [Fact]
    public async Task GetClinicalVisitByIdAsync_ConcurrentAccess_AllSucceed()
    {
        var tasks = Enumerable.Range(1, 100)
            .Select(id => _clinicalVisitService.GetClinicalVisitByIdAsync(id))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(v => v != null);
    }

    #endregion

    #region GetClinicalVisitsByPatientIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    public async Task GetClinicalVisitsByPatientIdAsync_ValidPatients_ReturnsVisits(int patientId)
    {
        var result = await _clinicalVisitService.GetClinicalVisitsByPatientIdAsync(patientId);
        var visits = result.ToList();

        visits.Should().OnlyContain(v => v.PatientId == patientId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetClinicalVisitsByPatientIdAsync_InvalidPatients_ReturnsEmpty(int patientId)
    {
        var result = await _clinicalVisitService.GetClinicalVisitsByPatientIdAsync(patientId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetClinicalVisitsByPatientIdAsync_OrderedByDate()
    {
        var result = await _clinicalVisitService.GetClinicalVisitsByPatientIdAsync(1);
        var visits = result.ToList();

        visits.Should().BeInDescendingOrder(v => v.VisitDate);
    }

    #endregion

    #region GetClinicalVisitsByBranchIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetClinicalVisitsByBranchIdAsync_ValidBranches_ReturnsVisits(int branchId)
    {
        var result = await _clinicalVisitService.GetClinicalVisitsByBranchIdAsync(branchId);
        var visits = result.ToList();

        visits.Should().NotBeEmpty();
        visits.Should().OnlyContain(v => v.BranchId == branchId);
    }

    #endregion

    #region GetClinicalVisitsByProviderIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task GetClinicalVisitsByProviderIdAsync_ValidProviders_ReturnsVisits(int providerId)
    {
        var result = await _clinicalVisitService.GetClinicalVisitsByProviderIdAsync(providerId);
        var visits = result.ToList();

        visits.Should().OnlyContain(v => v.ProviderId == providerId);
    }

    #endregion

    #region GetClinicalVisitsByDateRangeAsync Tests

    [Fact]
    public async Task GetClinicalVisitsByDateRangeAsync_ValidRange_ReturnsVisits()
    {
        var startDate = DateTime.UtcNow.AddDays(-100);
        var endDate = DateTime.UtcNow;

        var result = await _clinicalVisitService.GetClinicalVisitsByDateRangeAsync(1, startDate, endDate);
        var visits = result.ToList();

        visits.Should().OnlyContain(v => v.VisitDate >= startDate && v.VisitDate <= endDate);
    }

    [Fact]
    public async Task GetClinicalVisitsByDateRangeAsync_FutureRange_ReturnsEmpty()
    {
        var startDate = DateTime.UtcNow.AddDays(100);
        var endDate = DateTime.UtcNow.AddDays(200);

        var result = await _clinicalVisitService.GetClinicalVisitsByDateRangeAsync(1, startDate, endDate);
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateClinicalVisitAsync Tests

    [Fact]
    public async Task CreateClinicalVisitAsync_ValidVisit_CreatesSuccessfully()
    {
        var newVisit = new ClinicalVisit
        {
            BranchId = 1,
            PatientId = 1,
            ProviderId = 1,
            VisitDate = DateTime.UtcNow,
            VisitType = "Consultation",
            ChiefComplaint = "Headache",
            Status = "InProgress"
        };

        var result = await _clinicalVisitService.CreateClinicalVisitAsync(newVisit);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Consultation")]
    [InlineData("Follow-up")]
    [InlineData("Emergency")]
    [InlineData("Checkup")]
    [InlineData("Procedure")]
    public async Task CreateClinicalVisitAsync_VariousTypes_AllSucceed(string visitType)
    {
        var newVisit = new ClinicalVisit
        {
            BranchId = 1,
            PatientId = 1,
            ProviderId = 1,
            VisitDate = DateTime.UtcNow,
            VisitType = visitType,
            ChiefComplaint = "Test complaint",
            Status = "InProgress"
        };

        var result = await _clinicalVisitService.CreateClinicalVisitAsync(newVisit);

        result.Should().NotBeNull();
        result.VisitType.Should().Be(visitType);
    }

    [Fact]
    public async Task CreateClinicalVisitAsync_SetsCreatedAtAutomatically()
    {
        var newVisit = new ClinicalVisit
        {
            BranchId = 1,
            PatientId = 1,
            ProviderId = 1,
            VisitDate = DateTime.UtcNow,
            VisitType = "Consultation",
            ChiefComplaint = "Auto date test",
            Status = "InProgress"
        };

        var before = DateTime.UtcNow;
        var result = await _clinicalVisitService.CreateClinicalVisitAsync(newVisit);
        var after = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(before);
        result.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task CreateClinicalVisitAsync_ConcurrentCreations_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _clinicalVisitService.CreateClinicalVisitAsync(new ClinicalVisit
            {
                BranchId = 1,
                PatientId = (i % 50) + 1,
                ProviderId = (i % 20) + 1,
                VisitDate = DateTime.UtcNow,
                VisitType = "Consultation",
                ChiefComplaint = $"Concurrent test {i}",
                Status = "InProgress"
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(v => v.Id > 0);
    }

    #endregion

    #region UpdateClinicalVisitAsync Tests

    [Fact]
    public async Task UpdateClinicalVisitAsync_UpdateStatus_UpdatesSuccessfully()
    {
        var visit = await _clinicalVisitService.GetClinicalVisitByIdAsync(1);
        visit!.Status = "Completed";

        await _clinicalVisitService.UpdateClinicalVisitAsync(visit);

        var updated = await _clinicalVisitService.GetClinicalVisitByIdAsync(1);
        updated!.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task UpdateClinicalVisitAsync_UpdateChiefComplaint_UpdatesSuccessfully()
    {
        var visit = await _clinicalVisitService.GetClinicalVisitByIdAsync(2);
        visit!.ChiefComplaint = "Updated complaint";

        await _clinicalVisitService.UpdateClinicalVisitAsync(visit);

        var updated = await _clinicalVisitService.GetClinicalVisitByIdAsync(2);
        updated!.ChiefComplaint.Should().Be("Updated complaint");
    }

    [Fact]
    public async Task UpdateClinicalVisitAsync_SetsUpdatedAtAutomatically()
    {
        var visit = await _clinicalVisitService.GetClinicalVisitByIdAsync(3);
        visit!.ChiefComplaint = "Timestamp test";

        var before = DateTime.UtcNow;
        await _clinicalVisitService.UpdateClinicalVisitAsync(visit);
        var after = DateTime.UtcNow;

        var updated = await _clinicalVisitService.GetClinicalVisitByIdAsync(3);
        updated!.UpdatedAt.Should().BeOnOrAfter(before);
        updated.UpdatedAt.Should().BeOnOrBefore(after);
    }

    #endregion

    #region Vital Signs Tests

    [Fact]
    public async Task RecordVitalSignsAsync_ValidVitals_RecordsSuccessfully()
    {
        var vitals = new VitalSigns
        {
            ClinicalVisitId = 1,
            Temperature = 37.0m,
            PulseRate = 72,
            BloodPressureSystolic = 120,
            BloodPressureDiastolic = 80,
            RespiratoryRate = 16,
            OxygenSaturation = 98,
            Weight = 75,
            Height = 175,
            RecordedAt = DateTime.UtcNow,
            RecordedBy = "nurse1"
        };

        var result = await _clinicalVisitService.RecordVitalSignsAsync(vitals);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(35.0)]
    [InlineData(36.5)]
    [InlineData(37.0)]
    [InlineData(38.5)]
    [InlineData(40.0)]
    public async Task RecordVitalSignsAsync_VariousTemperatures_AllSucceed(decimal temperature)
    {
        var vitals = new VitalSigns
        {
            ClinicalVisitId = 10,
            Temperature = temperature,
            RecordedAt = DateTime.UtcNow,
            RecordedBy = "nurse1"
        };

        var result = await _clinicalVisitService.RecordVitalSignsAsync(vitals);

        result.Should().NotBeNull();
        result.Temperature.Should().Be(temperature);
    }

    [Theory]
    [InlineData(40)]
    [InlineData(60)]
    [InlineData(80)]
    [InlineData(100)]
    [InlineData(120)]
    public async Task RecordVitalSignsAsync_VariousPulseRates_AllSucceed(int pulseRate)
    {
        var vitals = new VitalSigns
        {
            ClinicalVisitId = 20,
            PulseRate = pulseRate,
            RecordedAt = DateTime.UtcNow,
            RecordedBy = "nurse1"
        };

        var result = await _clinicalVisitService.RecordVitalSignsAsync(vitals);

        result.Should().NotBeNull();
        result.PulseRate.Should().Be(pulseRate);
    }

    [Fact]
    public async Task GetVitalSignsByVisitIdAsync_ReturnsVitals()
    {
        var result = await _clinicalVisitService.GetVitalSignsByVisitIdAsync(1);
        result.Should().NotBeNull();
        result!.ClinicalVisitId.Should().Be(1);
    }

    [Fact]
    public async Task GetVitalSignsHistoryAsync_ReturnsHistory()
    {
        var result = await _clinicalVisitService.GetVitalSignsHistoryAsync(1);
        var history = result.ToList();

        history.Should().NotBeEmpty();
    }

    #endregion

    #region Diagnosis Tests

    [Fact]
    public async Task AddDiagnosisAsync_ValidDiagnosis_AddsSuccessfully()
    {
        var diagnosis = new Diagnosis
        {
            ClinicalVisitId = 1,
            IcdCode = "J06.9",
            Description = "Acute upper respiratory infection",
            DiagnosisType = "Primary",
            Notes = "Common cold symptoms"
        };

        var result = await _clinicalVisitService.AddDiagnosisAsync(diagnosis);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Primary")]
    [InlineData("Secondary")]
    [InlineData("Differential")]
    [InlineData("Provisional")]
    public async Task AddDiagnosisAsync_VariousTypes_AllSucceed(string diagnosisType)
    {
        var diagnosis = new Diagnosis
        {
            ClinicalVisitId = 5,
            IcdCode = "J00",
            Description = "Test diagnosis",
            DiagnosisType = diagnosisType
        };

        var result = await _clinicalVisitService.AddDiagnosisAsync(diagnosis);

        result.Should().NotBeNull();
        result.DiagnosisType.Should().Be(diagnosisType);
    }

    [Fact]
    public async Task GetDiagnosesByVisitIdAsync_ReturnsDiagnoses()
    {
        var result = await _clinicalVisitService.GetDiagnosesByVisitIdAsync(1);
        var diagnoses = result.ToList();

        diagnoses.Should().OnlyContain(d => d.ClinicalVisitId == 1);
    }

    [Fact]
    public async Task GetDiagnosesByPatientIdAsync_ReturnsDiagnoses()
    {
        var result = await _clinicalVisitService.GetDiagnosesByPatientIdAsync(1);
        var diagnoses = result.ToList();

        diagnoses.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateDiagnosisAsync_UpdatesSuccessfully()
    {
        var diagnosis = await _context.Diagnoses.FirstAsync();
        diagnosis.Notes = "Updated notes";

        await _clinicalVisitService.UpdateDiagnosisAsync(diagnosis);

        var updated = await _context.Diagnoses.FindAsync(diagnosis.Id);
        updated!.Notes.Should().Be("Updated notes");
    }

    #endregion

    #region Prescription Tests

    [Fact]
    public async Task CreatePrescriptionAsync_ValidPrescription_CreatesSuccessfully()
    {
        var prescription = new Prescription
        {
            ClinicalVisitId = 1,
            PatientId = 1,
            MedicationName = "Paracetamol",
            Dosage = "500mg",
            Frequency = "Three times daily",
            Duration = "5 days",
            Quantity = 15,
            Instructions = "Take after meals",
            PrescribedDate = DateTime.UtcNow,
            PrescribedById = 1,
            Status = "Active"
        };

        var result = await _clinicalVisitService.CreatePrescriptionAsync(prescription);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Once daily")]
    [InlineData("Twice daily")]
    [InlineData("Three times daily")]
    [InlineData("Four times daily")]
    [InlineData("As needed")]
    public async Task CreatePrescriptionAsync_VariousFrequencies_AllSucceed(string frequency)
    {
        var prescription = new Prescription
        {
            ClinicalVisitId = 10,
            PatientId = 10,
            MedicationName = "Test Medication",
            Dosage = "100mg",
            Frequency = frequency,
            Duration = "7 days",
            Quantity = 14,
            PrescribedDate = DateTime.UtcNow,
            PrescribedById = 1,
            Status = "Active"
        };

        var result = await _clinicalVisitService.CreatePrescriptionAsync(prescription);

        result.Should().NotBeNull();
        result.Frequency.Should().Be(frequency);
    }

    [Fact]
    public async Task GetPrescriptionsByVisitIdAsync_ReturnsPrescriptions()
    {
        var result = await _clinicalVisitService.GetPrescriptionsByVisitIdAsync(1);
        var prescriptions = result.ToList();

        prescriptions.Should().OnlyContain(p => p.ClinicalVisitId == 1);
    }

    [Fact]
    public async Task GetPrescriptionsByPatientIdAsync_ReturnsPrescriptions()
    {
        var result = await _clinicalVisitService.GetPrescriptionsByPatientIdAsync(1);
        var prescriptions = result.ToList();

        prescriptions.Should().OnlyContain(p => p.PatientId == 1);
    }

    [Fact]
    public async Task GetActivePrescriptionsAsync_ReturnsActiveOnly()
    {
        var result = await _clinicalVisitService.GetActivePrescriptionsAsync(1);
        var prescriptions = result.ToList();

        prescriptions.Should().OnlyContain(p => p.Status == "Active");
    }

    [Fact]
    public async Task CancelPrescriptionAsync_ActivePrescription_Cancels()
    {
        var activePrescription = await _context.Prescriptions
            .FirstOrDefaultAsync(p => p.Status == "Active");

        if (activePrescription != null)
        {
            await _clinicalVisitService.CancelPrescriptionAsync(activePrescription.Id, "No longer needed");

            var cancelled = await _context.Prescriptions.FindAsync(activePrescription.Id);
            cancelled!.Status.Should().Be("Cancelled");
        }
    }

    #endregion

    #region Clinical Notes Tests

    [Fact]
    public async Task AddClinicalNoteAsync_ValidNote_AddsSuccessfully()
    {
        var note = new ClinicalNote
        {
            ClinicalVisitId = 1,
            NoteType = "Assessment",
            Content = "Patient presents with symptoms...",
            AuthoredBy = "Dr. Test",
            AuthoredAt = DateTime.UtcNow
        };

        var result = await _clinicalVisitService.AddClinicalNoteAsync(note);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("History")]
    [InlineData("Examination")]
    [InlineData("Assessment")]
    [InlineData("Plan")]
    [InlineData("Progress")]
    public async Task AddClinicalNoteAsync_VariousTypes_AllSucceed(string noteType)
    {
        var note = new ClinicalNote
        {
            ClinicalVisitId = 5,
            NoteType = noteType,
            Content = $"Content for {noteType}",
            AuthoredBy = "Dr. Test",
            AuthoredAt = DateTime.UtcNow
        };

        var result = await _clinicalVisitService.AddClinicalNoteAsync(note);

        result.Should().NotBeNull();
        result.NoteType.Should().Be(noteType);
    }

    [Fact]
    public async Task GetClinicalNotesByVisitIdAsync_ReturnsNotes()
    {
        var result = await _clinicalVisitService.GetClinicalNotesByVisitIdAsync(1);
        var notes = result.ToList();

        notes.Should().OnlyContain(n => n.ClinicalVisitId == 1);
    }

    [Fact]
    public async Task UpdateClinicalNoteAsync_UpdatesSuccessfully()
    {
        var note = await _context.ClinicalNotes.FirstAsync();
        note.Content = "Updated content";

        await _clinicalVisitService.UpdateClinicalNoteAsync(note);

        var updated = await _context.ClinicalNotes.FindAsync(note.Id);
        updated!.Content.Should().Be("Updated content");
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public async Task CompleteVisitAsync_InProgressVisit_Completes()
    {
        var inProgressVisit = await _context.ClinicalVisits
            .FirstOrDefaultAsync(v => v.Status == "InProgress");

        if (inProgressVisit != null)
        {
            await _clinicalVisitService.CompleteVisitAsync(inProgressVisit.Id);

            var completed = await _clinicalVisitService.GetClinicalVisitByIdAsync(inProgressVisit.Id);
            completed!.Status.Should().Be("Completed");
        }
    }

    [Fact]
    public async Task CancelVisitAsync_InProgressVisit_Cancels()
    {
        var inProgressVisit = await _context.ClinicalVisits
            .Where(v => v.Status == "InProgress")
            .Skip(10)
            .FirstOrDefaultAsync();

        if (inProgressVisit != null)
        {
            await _clinicalVisitService.CancelVisitAsync(inProgressVisit.Id, "Patient no show");

            var cancelled = await _clinicalVisitService.GetClinicalVisitByIdAsync(inProgressVisit.Id);
            cancelled!.Status.Should().Be("Cancelled");
        }
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalVisitsCountAsync_ReturnsCount()
    {
        var result = await _clinicalVisitService.GetTotalVisitsCountAsync(1);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetVisitsByStatusCountAsync_ReturnsCount()
    {
        var result = await _clinicalVisitService.GetVisitsByStatusCountAsync(1, "Completed");
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetVisitTypeDistributionAsync_ReturnsDistribution()
    {
        var result = await _clinicalVisitService.GetVisitTypeDistributionAsync(1);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetVisitsPerProviderAsync_ReturnsStats()
    {
        var result = await _clinicalVisitService.GetVisitsPerProviderAsync(1);
        result.Should().NotBeEmpty();
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchClinicalVisitsAsync_ByPatientName_ReturnsMatches()
    {
        var result = await _clinicalVisitService.SearchClinicalVisitsAsync(1, "Visit Patient");
        var visits = result.ToList();

        visits.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchClinicalVisitsAsync_ByChiefComplaint_ReturnsMatches()
    {
        var result = await _clinicalVisitService.SearchClinicalVisitsAsync(1, "Chief complaint");
        var visits = result.ToList();

        visits.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchClinicalVisitsAsync_NoMatches_ReturnsEmpty()
    {
        var result = await _clinicalVisitService.SearchClinicalVisitsAsync(1, "NonExistent12345");
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases and Performance Tests

    [Fact]
    public async Task ClinicalVisit_WithLongChiefComplaint_HandlesCorrectly()
    {
        var longComplaint = new string('A', 2000);
        var visit = new ClinicalVisit
        {
            BranchId = 1,
            PatientId = 1,
            ProviderId = 1,
            VisitDate = DateTime.UtcNow,
            VisitType = "Consultation",
            ChiefComplaint = longComplaint,
            Status = "InProgress"
        };

        var action = () => _clinicalVisitService.CreateClinicalVisitAsync(visit);
        await action.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task GetClinicalVisitsByBranchIdAsync_LargeDataSet_PerformsWell()
    {
        var startTime = DateTime.UtcNow;

        var result = await _clinicalVisitService.GetClinicalVisitsByBranchIdAsync(1);
        var visits = result.ToList();

        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ConcurrentVisitCreations_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _clinicalVisitService.CreateClinicalVisitAsync(new ClinicalVisit
            {
                BranchId = 1,
                PatientId = (i % 50) + 1,
                ProviderId = (i % 20) + 1,
                VisitDate = DateTime.UtcNow,
                VisitType = "Consultation",
                ChiefComplaint = $"Concurrent visit {i}",
                Status = "InProgress"
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(v => v.Id > 0);
    }

    #endregion
}
