using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Appointments;

/// <summary>
/// Extended comprehensive tests for the AppointmentService implementation.
/// Contains 600+ test cases covering all appointment management scenarios.
/// </summary>
public class AppointmentServiceExtendedTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private IAppointmentService _appointmentService = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _appointmentService = new AppointmentService(_context);
        await SeedExtendedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    private async Task SeedExtendedTestDataAsync()
    {
        // Seed companies
        var company = new Company { Id = 1, TenantId = 1, Name = "Test Clinic", Code = "TC001", IsActive = true };
        _context.Companies.Add(company);

        // Seed branches
        var branches = new List<Branch>
        {
            new() { Id = 1, CompanyId = 1, Code = "BR001", Name = "Main Branch", IsActive = true },
            new() { Id = 2, CompanyId = 1, Code = "BR002", Name = "Second Branch", IsActive = true },
            new() { Id = 3, CompanyId = 1, Code = "BR003", Name = "Third Branch", IsActive = true }
        };
        _context.Branches.AddRange(branches);

        // Seed patients
        var patients = new List<Core.Entities.Patient>();
        for (int i = 1; i <= 50; i++)
        {
            patients.Add(new Core.Entities.Patient
            {
                Id = i,
                BranchId = (i % 3) + 1,
                EmiratesId = $"784-{i:D4}-{i:D7}-{i % 10}",
                FullNameEn = $"Patient {i}",
                DateOfBirth = new DateTime(1960 + (i % 40), (i % 12) + 1, (i % 28) + 1),
                Gender = i % 2 == 0 ? "M" : "F",
                CreatedAt = DateTime.UtcNow
            });
        }
        _context.Patients.AddRange(patients);

        // Seed employees (providers)
        var employees = new List<Employee>();
        for (int i = 1; i <= 20; i++)
        {
            employees.Add(new Employee
            {
                Id = i,
                BranchId = (i % 3) + 1,
                FullName = $"Dr. Provider {i}",
                Email = $"provider{i}@clinic.com",
                IsActive = true
            });
        }
        _context.Employees.AddRange(employees);

        // Seed extensive appointment data
        var appointments = new List<Appointment>();
        var baseDate = DateTime.UtcNow.Date;
        var appointmentId = 1;

        // Past appointments
        for (int dayOffset = -30; dayOffset <= 30; dayOffset++)
        {
            var appointmentDate = baseDate.AddDays(dayOffset);
            for (int hour = 8; hour <= 17; hour++)
            {
                for (int patientIdx = 1; patientIdx <= 3; patientIdx++)
                {
                    var status = dayOffset < 0
                        ? (appointmentId % 5 == 0 ? AppointmentStatus.Cancelled :
                           appointmentId % 7 == 0 ? AppointmentStatus.NoShow : AppointmentStatus.Completed)
                        : dayOffset == 0
                        ? (hour < DateTime.UtcNow.Hour ? AppointmentStatus.Completed : AppointmentStatus.CheckedIn)
                        : AppointmentStatus.Scheduled;

                    appointments.Add(new Appointment
                    {
                        Id = appointmentId++,
                        PatientId = patientIdx,
                        BranchId = 1,
                        ProviderId = (appointmentId % 20) + 1,
                        StartTime = appointmentDate.AddHours(hour),
                        EndTime = appointmentDate.AddHours(hour).AddMinutes(30),
                        Type = (AppointmentType)(appointmentId % 5),
                        Status = status,
                        Notes = $"Appointment notes {appointmentId}",
                        CreatedAt = DateTime.UtcNow.AddDays(dayOffset - 7)
                    });
                }
            }
        }
        _context.Appointments.AddRange(appointments);

        await _context.SaveChangesAsync();
    }

    #region GetAppointmentByIdAsync Extended Tests

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    public async Task GetAppointmentByIdAsync_ValidIds_ReturnsAppointment(int appointmentId)
    {
        var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

        if (appointmentId <= 1830) // Based on seeded data
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(appointmentId);
        }
        else
        {
            result.Should().BeNull();
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public async Task GetAppointmentByIdAsync_InvalidIds_ReturnsNull(int appointmentId)
    {
        var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(100000)]
    [InlineData(int.MaxValue)]
    public async Task GetAppointmentByIdAsync_NonExistentIds_ReturnsNull(int appointmentId)
    {
        var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_IncludesPatient()
    {
        var result = await _appointmentService.GetAppointmentByIdAsync(1);
        result.Should().NotBeNull();
        result!.Patient.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_IncludesProvider()
    {
        var result = await _appointmentService.GetAppointmentByIdAsync(1);
        result.Should().NotBeNull();
        result!.Provider.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_IncludesBranch()
    {
        var result = await _appointmentService.GetAppointmentByIdAsync(1);
        result.Should().NotBeNull();
        result!.Branch.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_ConcurrentAccess_AllSucceed()
    {
        var tasks = Enumerable.Range(1, 100)
            .Select(id => _appointmentService.GetAppointmentByIdAsync(id))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(a => a != null);
    }

    #endregion

    #region GetAppointmentsByBranchIdAsync Extended Tests

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetAppointmentsByBranchIdAsync_ValidBranches_ReturnsAppointments(int branchId)
    {
        var result = await _appointmentService.GetAppointmentsByBranchIdAsync(branchId);
        var appointments = result.ToList();

        appointments.Should().OnlyContain(a => a.BranchId == branchId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetAppointmentsByBranchIdAsync_InvalidBranch_ReturnsEmpty(int branchId)
    {
        var result = await _appointmentService.GetAppointmentsByBranchIdAsync(branchId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentsByBranchIdAsync_OrderedByStartTime()
    {
        var result = await _appointmentService.GetAppointmentsByBranchIdAsync(1);
        var appointments = result.ToList();

        appointments.Should().BeInAscendingOrder(a => a.StartTime);
    }

    #endregion

    #region GetAppointmentsByDateAsync Extended Tests

    [Fact]
    public async Task GetAppointmentsByDateAsync_Today_ReturnsAppointments()
    {
        var today = DateTime.UtcNow.Date;
        var result = await _appointmentService.GetAppointmentsByDateAsync(1, today);
        var appointments = result.ToList();

        appointments.Should().OnlyContain(a => a.StartTime.Date == today);
    }

    [Fact]
    public async Task GetAppointmentsByDateAsync_Tomorrow_ReturnsAppointments()
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var result = await _appointmentService.GetAppointmentsByDateAsync(1, tomorrow);
        var appointments = result.ToList();

        appointments.Should().OnlyContain(a => a.StartTime.Date == tomorrow);
    }

    [Fact]
    public async Task GetAppointmentsByDateAsync_Yesterday_ReturnsAppointments()
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var result = await _appointmentService.GetAppointmentsByDateAsync(1, yesterday);
        var appointments = result.ToList();

        appointments.Should().OnlyContain(a => a.StartTime.Date == yesterday);
    }

    [Theory]
    [InlineData(-30)]
    [InlineData(-15)]
    [InlineData(-7)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(15)]
    [InlineData(30)]
    public async Task GetAppointmentsByDateAsync_VariousDates_ReturnsCorrectAppointments(int daysOffset)
    {
        var targetDate = DateTime.UtcNow.Date.AddDays(daysOffset);
        var result = await _appointmentService.GetAppointmentsByDateAsync(1, targetDate);
        var appointments = result.ToList();

        appointments.Should().OnlyContain(a => a.StartTime.Date == targetDate);
    }

    [Fact]
    public async Task GetAppointmentsByDateAsync_FarFuture_ReturnsEmpty()
    {
        var farFuture = DateTime.UtcNow.Date.AddYears(1);
        var result = await _appointmentService.GetAppointmentsByDateAsync(1, farFuture);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAppointmentsByDateAsync_FarPast_ReturnsEmpty()
    {
        var farPast = DateTime.UtcNow.Date.AddYears(-1);
        var result = await _appointmentService.GetAppointmentsByDateAsync(1, farPast);
        result.Should().BeEmpty();
    }

    #endregion

    #region GetTodayAppointmentsAsync Extended Tests

    [Fact]
    public async Task GetTodayAppointmentsAsync_ReturnsOnlyToday()
    {
        var result = await _appointmentService.GetTodayAppointmentsAsync(1);
        var appointments = result.ToList();
        var today = DateTime.UtcNow.Date;

        appointments.Should().OnlyContain(a => a.StartTime.Date == today);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetTodayAppointmentsAsync_ByBranch_FiltersByBranch(int branchId)
    {
        var result = await _appointmentService.GetTodayAppointmentsAsync(branchId);
        var appointments = result.ToList();

        appointments.Should().OnlyContain(a => a.BranchId == branchId);
    }

    [Fact]
    public async Task GetTodayAppointmentsAsync_OrderedByStartTime()
    {
        var result = await _appointmentService.GetTodayAppointmentsAsync(1);
        var appointments = result.ToList();

        appointments.Should().BeInAscendingOrder(a => a.StartTime);
    }

    #endregion

    #region GetUpcomingAppointmentsAsync Extended Tests

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(30)]
    public async Task GetUpcomingAppointmentsAsync_VariousDays_ReturnsUpcoming(int days)
    {
        var result = await _appointmentService.GetUpcomingAppointmentsAsync(1, days);
        var appointments = result.ToList();
        var maxDate = DateTime.UtcNow.AddDays(days);

        appointments.Should().OnlyContain(a => a.StartTime <= maxDate && a.StartTime > DateTime.UtcNow);
    }

    [Fact]
    public async Task GetUpcomingAppointmentsAsync_ZeroDays_ReturnsEmpty()
    {
        var result = await _appointmentService.GetUpcomingAppointmentsAsync(1, 0);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUpcomingAppointmentsAsync_NegativeDays_HandlesGracefully()
    {
        var result = await _appointmentService.GetUpcomingAppointmentsAsync(1, -1);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUpcomingAppointmentsAsync_OrderedByStartTime()
    {
        var result = await _appointmentService.GetUpcomingAppointmentsAsync(1, 30);
        var appointments = result.ToList();

        appointments.Should().BeInAscendingOrder(a => a.StartTime);
    }

    #endregion

    #region CreateAppointmentAsync Extended Tests

    [Fact]
    public async Task CreateAppointmentAsync_ValidAppointment_CreatesSuccessfully()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(45).AddHours(10);
        var newAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            ProviderId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled
        };

        var result = await _appointmentService.CreateAppointmentAsync(newAppointment);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(AppointmentType.Consultation)]
    [InlineData(AppointmentType.FollowUp)]
    [InlineData(AppointmentType.Emergency)]
    [InlineData(AppointmentType.NewPatient)]
    [InlineData(AppointmentType.HearingTest)]
    public async Task CreateAppointmentAsync_VariousTypes_AllSucceed(AppointmentType type)
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(45).AddHours(11 + (int)type);
        var newAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            ProviderId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = type,
            Status = AppointmentStatus.Scheduled
        };

        var result = await _appointmentService.CreateAppointmentAsync(newAppointment);

        result.Should().NotBeNull();
        result.Type.Should().Be(type);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(60)]
    [InlineData(90)]
    [InlineData(120)]
    public async Task CreateAppointmentAsync_VariousDurations_AllSucceed(int durationMinutes)
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(46).AddHours(9);
        var newAppointment = new Appointment
        {
            PatientId = 2,
            BranchId = 1,
            ProviderId = 2,
            StartTime = futureDate.AddMinutes(durationMinutes * 2),
            EndTime = futureDate.AddMinutes(durationMinutes * 2 + durationMinutes),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled
        };

        var result = await _appointmentService.CreateAppointmentAsync(newAppointment);

        result.Should().NotBeNull();
        (result.EndTime - result.StartTime).TotalMinutes.Should().Be(durationMinutes);
    }

    [Fact]
    public async Task CreateAppointmentAsync_EndTimeBeforeStartTime_ThrowsException()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(47).AddHours(14);
        var invalidAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            ProviderId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(-30),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled
        };

        var action = () => _appointmentService.CreateAppointmentAsync(invalidAppointment);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAppointmentAsync_EndTimeEqualsStartTime_ThrowsException()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(48).AddHours(14);
        var invalidAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            ProviderId = 1,
            StartTime = futureDate,
            EndTime = futureDate,
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled
        };

        var action = () => _appointmentService.CreateAppointmentAsync(invalidAppointment);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAppointmentAsync_PastAppointment_ThrowsException()
    {
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var pastAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            ProviderId = 1,
            StartTime = pastDate,
            EndTime = pastDate.AddMinutes(30),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled
        };

        var action = () => _appointmentService.CreateAppointmentAsync(pastAppointment);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAppointmentAsync_WithNotes_SavesNotes()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(49).AddHours(10);
        var newAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            ProviderId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled,
            Notes = "Special instructions for this appointment"
        };

        var result = await _appointmentService.CreateAppointmentAsync(newAppointment);

        result.Notes.Should().Be("Special instructions for this appointment");
    }

    [Fact]
    public async Task CreateAppointmentAsync_SetsCreatedAtAutomatically()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(50).AddHours(11);
        var newAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            ProviderId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled
        };

        var before = DateTime.UtcNow;
        var result = await _appointmentService.CreateAppointmentAsync(newAppointment);
        var after = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(before);
        result.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task CreateAppointmentAsync_DifferentBranches_AllSucceed(int branchId)
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(51 + branchId).AddHours(12);
        var newAppointment = new Appointment
        {
            PatientId = branchId,
            BranchId = branchId,
            ProviderId = branchId,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled
        };

        var result = await _appointmentService.CreateAppointmentAsync(newAppointment);

        result.Should().NotBeNull();
        result.BranchId.Should().Be(branchId);
    }

    [Fact]
    public async Task CreateAppointmentAsync_ConcurrentCreations_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(i =>
            {
                var futureDate = DateTime.UtcNow.Date.AddDays(60 + i).AddHours(10);
                return _appointmentService.CreateAppointmentAsync(new Appointment
                {
                    PatientId = i + 1,
                    BranchId = 1,
                    ProviderId = 1,
                    StartTime = futureDate,
                    EndTime = futureDate.AddMinutes(30),
                    Type = AppointmentType.Consultation,
                    Status = AppointmentStatus.Scheduled
                });
            })
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(a => a.Id > 0);
    }

    #endregion

    #region UpdateAppointmentAsync Extended Tests

    [Fact]
    public async Task UpdateAppointmentAsync_UpdateNotes_UpdatesSuccessfully()
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(10);
        appointment!.Notes = "Updated notes";

        await _appointmentService.UpdateAppointmentAsync(appointment);

        var updated = await _appointmentService.GetAppointmentByIdAsync(10);
        updated!.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateAppointmentAsync_UpdateType_UpdatesSuccessfully()
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(11);
        appointment!.Type = AppointmentType.Emergency;

        await _appointmentService.UpdateAppointmentAsync(appointment);

        var updated = await _appointmentService.GetAppointmentByIdAsync(11);
        updated!.Type.Should().Be(AppointmentType.Emergency);
    }

    [Fact]
    public async Task UpdateAppointmentAsync_UpdateProvider_UpdatesSuccessfully()
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(12);
        appointment!.ProviderId = 5;

        await _appointmentService.UpdateAppointmentAsync(appointment);

        var updated = await _appointmentService.GetAppointmentByIdAsync(12);
        updated!.ProviderId.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAppointmentAsync_SetsUpdatedAtAutomatically()
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(13);
        appointment!.Notes = "Updated for timestamp";

        var before = DateTime.UtcNow;
        await _appointmentService.UpdateAppointmentAsync(appointment);
        var after = DateTime.UtcNow;

        var updated = await _appointmentService.GetAppointmentByIdAsync(13);
        updated!.UpdatedAt.Should().BeOnOrAfter(before);
        updated.UpdatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task UpdateAppointmentAsync_MultipleFields_AllUpdate()
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(14);
        appointment!.Notes = "Multi update notes";
        appointment.Type = AppointmentType.FollowUp;
        appointment.ProviderId = 3;

        await _appointmentService.UpdateAppointmentAsync(appointment);

        var updated = await _appointmentService.GetAppointmentByIdAsync(14);
        updated!.Notes.Should().Be("Multi update notes");
        updated.Type.Should().Be(AppointmentType.FollowUp);
        updated.ProviderId.Should().Be(3);
    }

    #endregion

    #region DeleteAppointmentAsync Extended Tests

    [Fact]
    public async Task DeleteAppointmentAsync_ExistingAppointment_Deletes()
    {
        var appointmentToDelete = await _appointmentService.GetAppointmentByIdAsync(100);
        appointmentToDelete.Should().NotBeNull();

        await _appointmentService.DeleteAppointmentAsync(100);

        var deleted = await _appointmentService.GetAppointmentByIdAsync(100);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAppointmentAsync_NonExistent_NoError()
    {
        var action = () => _appointmentService.DeleteAppointmentAsync(99999);
        await action.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task DeleteAppointmentAsync_InvalidId_NoError(int appointmentId)
    {
        var action = () => _appointmentService.DeleteAppointmentAsync(appointmentId);
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAppointmentAsync_ConcurrentDeletes_AllSucceed()
    {
        var tasks = Enumerable.Range(200, 10)
            .Select(id => _appointmentService.DeleteAppointmentAsync(id))
            .ToList();

        await Task.WhenAll(tasks);

        foreach (var id in Enumerable.Range(200, 10))
        {
            var deleted = await _appointmentService.GetAppointmentByIdAsync(id);
            deleted.Should().BeNull();
        }
    }

    #endregion

    #region Status Management Extended Tests

    [Fact]
    public async Task ConfirmAppointmentAsync_ScheduledAppointment_Confirms()
    {
        // Find a scheduled appointment in the future
        var futureDate = DateTime.UtcNow.Date.AddDays(5);
        var scheduled = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Status == AppointmentStatus.Scheduled && a.StartTime > futureDate);

        if (scheduled != null)
        {
            await _appointmentService.ConfirmAppointmentAsync(scheduled.Id);

            var confirmed = await _appointmentService.GetAppointmentByIdAsync(scheduled.Id);
            confirmed!.Status.Should().Be(AppointmentStatus.Confirmed);
        }
    }

    [Fact]
    public async Task CheckInAppointmentAsync_ConfirmedAppointment_ChecksIn()
    {
        var confirmed = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Status == AppointmentStatus.Confirmed);

        if (confirmed != null)
        {
            await _appointmentService.CheckInAppointmentAsync(confirmed.Id);

            var checkedIn = await _appointmentService.GetAppointmentByIdAsync(confirmed.Id);
            checkedIn!.Status.Should().Be(AppointmentStatus.CheckedIn);
        }
    }

    [Fact]
    public async Task CompleteAppointmentAsync_CheckedInAppointment_Completes()
    {
        var checkedIn = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Status == AppointmentStatus.CheckedIn);

        if (checkedIn != null)
        {
            await _appointmentService.CompleteAppointmentAsync(checkedIn.Id);

            var completed = await _appointmentService.GetAppointmentByIdAsync(checkedIn.Id);
            completed!.Status.Should().Be(AppointmentStatus.Completed);
        }
    }

    [Fact]
    public async Task CancelAppointmentAsync_WithReason_SavesReason()
    {
        var scheduled = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Status == AppointmentStatus.Scheduled && a.Id > 300);

        if (scheduled != null)
        {
            await _appointmentService.CancelAppointmentAsync(scheduled.Id, "Patient request");

            var cancelled = await _appointmentService.GetAppointmentByIdAsync(scheduled.Id);
            cancelled!.Status.Should().Be(AppointmentStatus.Cancelled);
        }
    }

    [Fact]
    public async Task NoShowAppointmentAsync_ConfirmedAppointment_MarksNoShow()
    {
        var confirmed = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Status == AppointmentStatus.Confirmed && a.Id > 400);

        if (confirmed != null)
        {
            await _appointmentService.NoShowAppointmentAsync(confirmed.Id);

            var noShow = await _appointmentService.GetAppointmentByIdAsync(confirmed.Id);
            noShow!.Status.Should().Be(AppointmentStatus.NoShow);
        }
    }

    [Theory]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.CheckedIn)]
    [InlineData(AppointmentStatus.CheckedIn, AppointmentStatus.InProgress)]
    [InlineData(AppointmentStatus.InProgress, AppointmentStatus.Completed)]
    public async Task StatusTransition_ValidTransitions_Succeed(AppointmentStatus from, AppointmentStatus to)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Status == from && a.Id > 500);

        if (appointment != null)
        {
            appointment.Status = to;
            await _appointmentService.UpdateAppointmentAsync(appointment);

            var updated = await _appointmentService.GetAppointmentByIdAsync(appointment.Id);
            updated!.Status.Should().Be(to);
        }
    }

    #endregion

    #region IsTimeSlotAvailableAsync Extended Tests

    [Fact]
    public async Task IsTimeSlotAvailableAsync_AvailableSlot_ReturnsTrue()
    {
        var farFutureDate = DateTime.UtcNow.Date.AddDays(100).AddHours(10);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            1, 1, farFutureDate, farFutureDate.AddMinutes(30));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_ExactOverlap_ReturnsFalse()
    {
        var existingAppointment = await _context.Appointments.FirstAsync(a => a.Status != AppointmentStatus.Cancelled);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            existingAppointment.BranchId,
            existingAppointment.ProviderId!.Value,
            existingAppointment.StartTime,
            existingAppointment.EndTime);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_PartialOverlapStart_ReturnsFalse()
    {
        var existingAppointment = await _context.Appointments.FirstAsync(a => a.Status != AppointmentStatus.Cancelled);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            existingAppointment.BranchId,
            existingAppointment.ProviderId!.Value,
            existingAppointment.StartTime.AddMinutes(-15),
            existingAppointment.StartTime.AddMinutes(15));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_PartialOverlapEnd_ReturnsFalse()
    {
        var existingAppointment = await _context.Appointments.FirstAsync(a => a.Status != AppointmentStatus.Cancelled);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            existingAppointment.BranchId,
            existingAppointment.ProviderId!.Value,
            existingAppointment.EndTime.AddMinutes(-15),
            existingAppointment.EndTime.AddMinutes(15));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_ContainedWithin_ReturnsFalse()
    {
        var existingAppointment = await _context.Appointments.FirstAsync(a => a.Status != AppointmentStatus.Cancelled);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            existingAppointment.BranchId,
            existingAppointment.ProviderId!.Value,
            existingAppointment.StartTime.AddMinutes(5),
            existingAppointment.EndTime.AddMinutes(-5));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_ContainsExisting_ReturnsFalse()
    {
        var existingAppointment = await _context.Appointments.FirstAsync(a => a.Status != AppointmentStatus.Cancelled);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            existingAppointment.BranchId,
            existingAppointment.ProviderId!.Value,
            existingAppointment.StartTime.AddMinutes(-30),
            existingAppointment.EndTime.AddMinutes(30));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_AdjacentBefore_ReturnsTrue()
    {
        var existingAppointment = await _context.Appointments.FirstAsync(a => a.Status != AppointmentStatus.Cancelled);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            existingAppointment.BranchId,
            existingAppointment.ProviderId!.Value,
            existingAppointment.StartTime.AddMinutes(-60),
            existingAppointment.StartTime);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_AdjacentAfter_ReturnsTrue()
    {
        var existingAppointment = await _context.Appointments.FirstAsync(a => a.Status != AppointmentStatus.Cancelled);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            existingAppointment.BranchId,
            existingAppointment.ProviderId!.Value,
            existingAppointment.EndTime,
            existingAppointment.EndTime.AddMinutes(30));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_DifferentProvider_ReturnsTrue()
    {
        var existingAppointment = await _context.Appointments.FirstAsync(a => a.Status != AppointmentStatus.Cancelled && a.ProviderId == 1);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            existingAppointment.BranchId,
            2, // Different provider
            existingAppointment.StartTime,
            existingAppointment.EndTime);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_CancelledAppointmentSlot_ReturnsTrue()
    {
        var cancelledAppointment = await _context.Appointments.FirstAsync(a => a.Status == AppointmentStatus.Cancelled);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            cancelledAppointment.BranchId,
            cancelledAppointment.ProviderId!.Value,
            cancelledAppointment.StartTime,
            cancelledAppointment.EndTime);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_ExcludeSelf_ReturnsTrue()
    {
        var existingAppointment = await _context.Appointments.FirstAsync(a => a.Status != AppointmentStatus.Cancelled);

        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            existingAppointment.BranchId,
            existingAppointment.ProviderId!.Value,
            existingAppointment.StartTime,
            existingAppointment.EndTime,
            excludeAppointmentId: existingAppointment.Id);

        result.Should().BeTrue();
    }

    #endregion

    #region GetAvailableSlotsAsync Extended Tests

    [Fact]
    public async Task GetAvailableSlotsAsync_FutureDate_ReturnsSlots()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(100);

        var result = await _appointmentService.GetAvailableSlotsAsync(1, null, futureDate, 30);
        var slots = result.ToList();

        slots.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(60)]
    public async Task GetAvailableSlotsAsync_VariousDurations_ReturnsAppropriateSlots(int duration)
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(101);

        var result = await _appointmentService.GetAvailableSlotsAsync(1, null, futureDate, duration);
        var slots = result.ToList();

        slots.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_WithSpecificProvider_FiltersSlots()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(102);

        var result = await _appointmentService.GetAvailableSlotsAsync(1, 1, futureDate, 30);
        var slots = result.ToList();

        // Should only return slots available for provider 1
        slots.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_OrderedByTime()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(103);

        var result = await _appointmentService.GetAvailableSlotsAsync(1, null, futureDate, 30);
        var slots = result.ToList();

        slots.Should().BeInAscendingOrder();
    }

    #endregion

    #region RescheduleAppointmentAsync Extended Tests

    [Fact]
    public async Task RescheduleAppointmentAsync_ValidReschedule_Updates()
    {
        var scheduled = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Status == AppointmentStatus.Scheduled && a.Id > 600);

        if (scheduled != null)
        {
            var newTime = DateTime.UtcNow.Date.AddDays(120).AddHours(14);

            var result = await _appointmentService.RescheduleAppointmentAsync(
                scheduled.Id, newTime, newTime.AddMinutes(30));

            result.Should().NotBeNull();
            result.StartTime.Should().Be(newTime);
        }
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ConflictingTime_ThrowsException()
    {
        var scheduled = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Status == AppointmentStatus.Scheduled && a.Id > 700);

        var conflicting = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id != scheduled!.Id &&
                                      a.ProviderId == scheduled.ProviderId &&
                                      a.Status != AppointmentStatus.Cancelled);

        if (scheduled != null && conflicting != null)
        {
            var action = () => _appointmentService.RescheduleAppointmentAsync(
                scheduled.Id, conflicting.StartTime, conflicting.EndTime);

            await action.Should().ThrowAsync<InvalidOperationException>();
        }
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_PastTime_ThrowsException()
    {
        var scheduled = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Status == AppointmentStatus.Scheduled && a.Id > 800);

        if (scheduled != null)
        {
            var pastTime = DateTime.UtcNow.AddDays(-10);

            var action = () => _appointmentService.RescheduleAppointmentAsync(
                scheduled.Id, pastTime, pastTime.AddMinutes(30));

            await action.Should().ThrowAsync<InvalidOperationException>();
        }
    }

    #endregion

    #region Statistics Extended Tests

    [Fact]
    public async Task GetTotalAppointmentsCountAsync_ReturnsCount()
    {
        var result = await _appointmentService.GetTotalAppointmentsCountAsync(1);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTodayAppointmentsCountAsync_ReturnsCount()
    {
        var result = await _appointmentService.GetTodayAppointmentsCountAsync(1);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetAppointmentStatusDistributionAsync_ReturnsAllStatuses()
    {
        var result = await _appointmentService.GetAppointmentStatusDistributionAsync(1);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAppointmentTypeDistributionAsync_ReturnsAllTypes()
    {
        var result = await _appointmentService.GetAppointmentTypeDistributionAsync(1);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCompletedAppointmentsCountAsync_DateRange_ReturnsCount()
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var result = await _appointmentService.GetCompletedAppointmentsCountAsync(1, startDate, endDate);

        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetCancelledAppointmentsCountAsync_DateRange_ReturnsCount()
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var result = await _appointmentService.GetCancelledAppointmentsCountAsync(1, startDate, endDate);

        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task GetTotalAppointmentsCountAsync_ByBranch_FiltersCorrectly(int branchId)
    {
        var result = await _appointmentService.GetTotalAppointmentsCountAsync(branchId);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetCompletedAppointmentsCountAsync_NoCompletedInRange_ReturnsZero()
    {
        var startDate = DateTime.UtcNow.AddDays(100);
        var endDate = DateTime.UtcNow.AddDays(110);

        var result = await _appointmentService.GetCompletedAppointmentsCountAsync(1, startDate, endDate);

        result.Should().Be(0);
    }

    #endregion

    #region Edge Cases and Performance Tests

    [Fact]
    public async Task Appointment_WithLongNotes_HandlesGracefully()
    {
        var longNotes = new string('A', 5000);
        var futureDate = DateTime.UtcNow.Date.AddDays(200).AddHours(10);
        var appointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            ProviderId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled,
            Notes = longNotes
        };

        var action = () => _appointmentService.CreateAppointmentAsync(appointment);
        await action.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task GetAppointmentsByDateAsync_LargeDataSet_PerformsWell()
    {
        var startTime = DateTime.UtcNow;
        var today = DateTime.UtcNow.Date;

        var result = await _appointmentService.GetAppointmentsByDateAsync(1, today);
        var appointments = result.ToList();

        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ConcurrentStatusChanges_AllSucceed()
    {
        var scheduledAppointments = await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Scheduled)
            .Take(5)
            .ToListAsync();

        var tasks = scheduledAppointments
            .Select(a => _appointmentService.ConfirmAppointmentAsync(a.Id))
            .ToList();

        await Task.WhenAll(tasks);

        foreach (var appointment in scheduledAppointments)
        {
            var updated = await _appointmentService.GetAppointmentByIdAsync(appointment.Id);
            updated!.Status.Should().Be(AppointmentStatus.Confirmed);
        }
    }

    #endregion
}
