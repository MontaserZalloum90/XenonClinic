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
/// Tests for the AppointmentService implementation.
/// </summary>
public class AppointmentServiceTests : IAsyncLifetime
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

        var patient = new Patient
        {
            Id = 1,
            BranchId = 1,
            EmiratesId = "784-1234-1234567-1",
            FullNameEn = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        _context.Patients.Add(patient);

        var employee = new Employee
        {
            Id = 1,
            BranchId = 1,
            FullName = "Dr. Smith",
            Email = "smith@clinic.com",
            IsActive = true
        };
        _context.Employees.Add(employee);

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);

        // Seed test appointments
        var appointments = new List<Appointment>
        {
            new()
            {
                Id = 1,
                PatientId = 1,
                BranchId = 1,
                ProviderId = 1,
                StartTime = tomorrow.AddHours(9),
                EndTime = tomorrow.AddHours(9).AddMinutes(30),
                Type = AppointmentType.Consultation,
                Status = AppointmentStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                PatientId = 1,
                BranchId = 1,
                ProviderId = 1,
                StartTime = tomorrow.AddHours(10),
                EndTime = tomorrow.AddHours(10).AddMinutes(30),
                Type = AppointmentType.FollowUp,
                Status = AppointmentStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 3,
                PatientId = 1,
                BranchId = 1,
                ProviderId = 1,
                StartTime = DateTime.UtcNow.Date.AddHours(14), // Today
                EndTime = DateTime.UtcNow.Date.AddHours(14).AddMinutes(30),
                Type = AppointmentType.HearingTest,
                Status = AppointmentStatus.CheckedIn,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 4,
                PatientId = 1,
                BranchId = 1,
                ProviderId = 1,
                StartTime = DateTime.UtcNow.AddDays(-1).Date.AddHours(9),
                EndTime = DateTime.UtcNow.AddDays(-1).Date.AddHours(9).AddMinutes(30),
                Type = AppointmentType.Consultation,
                Status = AppointmentStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = 5,
                PatientId = 1,
                BranchId = 1,
                ProviderId = 1,
                StartTime = DateTime.UtcNow.AddDays(-2).Date.AddHours(11),
                EndTime = DateTime.UtcNow.AddDays(-2).Date.AddHours(11).AddMinutes(30),
                Type = AppointmentType.FollowUp,
                Status = AppointmentStatus.Cancelled,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();
    }

    #region GetAppointmentByIdAsync Tests

    [Fact]
    public async Task GetAppointmentByIdAsync_ExistingAppointment_ReturnsAppointment()
    {
        // Act
        var result = await _appointmentService.GetAppointmentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_NonExistent_ReturnsNull()
    {
        // Act
        var result = await _appointmentService.GetAppointmentByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_IncludesRelatedData()
    {
        // Act
        var result = await _appointmentService.GetAppointmentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Patient.Should().NotBeNull();
        result.Provider.Should().NotBeNull();
        result.Branch.Should().NotBeNull();
    }

    #endregion

    #region GetAppointmentsByBranchIdAsync Tests

    [Fact]
    public async Task GetAppointmentsByBranchIdAsync_ReturnsAllBranchAppointments()
    {
        // Act
        var result = await _appointmentService.GetAppointmentsByBranchIdAsync(1);

        // Assert
        var appointments = result.ToList();
        appointments.Should().HaveCount(5);
        appointments.Should().OnlyContain(a => a.BranchId == 1);
    }

    [Fact]
    public async Task GetAppointmentsByBranchIdAsync_NonExistentBranch_ReturnsEmpty()
    {
        // Act
        var result = await _appointmentService.GetAppointmentsByBranchIdAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAppointmentsByDateAsync Tests

    [Fact]
    public async Task GetAppointmentsByDateAsync_ReturnsCorrectAppointments()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);

        // Act
        var result = await _appointmentService.GetAppointmentsByDateAsync(1, tomorrow);

        // Assert
        var appointments = result.ToList();
        appointments.Should().HaveCount(2);
        appointments.Should().OnlyContain(a => a.StartTime.Date == tomorrow);
    }

    [Fact]
    public async Task GetAppointmentsByDateAsync_NoAppointments_ReturnsEmpty()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(30);

        // Act
        var result = await _appointmentService.GetAppointmentsByDateAsync(1, futureDate);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetTodayAppointmentsAsync Tests

    [Fact]
    public async Task GetTodayAppointmentsAsync_ReturnsOnlyTodayAppointments()
    {
        // Act
        var result = await _appointmentService.GetTodayAppointmentsAsync(1);

        // Assert
        var appointments = result.ToList();
        appointments.Should().HaveCount(1);
        appointments.Should().OnlyContain(a => a.StartTime.Date == DateTime.UtcNow.Date);
    }

    #endregion

    #region GetUpcomingAppointmentsAsync Tests

    [Fact]
    public async Task GetUpcomingAppointmentsAsync_ReturnsOnlyFutureAppointments()
    {
        // Act
        var result = await _appointmentService.GetUpcomingAppointmentsAsync(1, 7);

        // Assert
        var appointments = result.ToList();
        appointments.Should().OnlyContain(a => a.StartTime > DateTime.UtcNow);
    }

    #endregion

    #region CreateAppointmentAsync Tests

    [Fact]
    public async Task CreateAppointmentAsync_ValidAppointment_CreatesSuccessfully()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(5).AddHours(15);
        var newAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            ProviderId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(30),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _appointmentService.CreateAppointmentAsync(newAppointment);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var saved = await _appointmentService.GetAppointmentByIdAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAppointmentAsync_EndTimeBeforeStartTime_ThrowsException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(5).AddHours(15);
        var invalidAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            StartTime = futureDate,
            EndTime = futureDate.AddMinutes(-30), // End before start
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled
        };

        // Act
        var action = () => _appointmentService.CreateAppointmentAsync(invalidAppointment);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAppointmentAsync_PastAppointment_ThrowsException()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var pastAppointment = new Appointment
        {
            PatientId = 1,
            BranchId = 1,
            StartTime = pastDate,
            EndTime = pastDate.AddMinutes(30),
            Type = AppointmentType.Consultation,
            Status = AppointmentStatus.Scheduled
        };

        // Act
        var action = () => _appointmentService.CreateAppointmentAsync(pastAppointment);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region UpdateAppointmentAsync Tests

    [Fact]
    public async Task UpdateAppointmentAsync_ValidUpdate_UpdatesSuccessfully()
    {
        // Arrange
        var appointment = await _appointmentService.GetAppointmentByIdAsync(1);
        appointment!.Notes = "Updated notes";
        appointment.Type = AppointmentType.FollowUp;

        // Act
        await _appointmentService.UpdateAppointmentAsync(appointment);

        // Assert
        var updated = await _appointmentService.GetAppointmentByIdAsync(1);
        updated!.Notes.Should().Be("Updated notes");
        updated.Type.Should().Be(AppointmentType.FollowUp);
        updated.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region DeleteAppointmentAsync Tests

    [Fact]
    public async Task DeleteAppointmentAsync_ExistingAppointment_Deletes()
    {
        // Act
        await _appointmentService.DeleteAppointmentAsync(5);

        // Assert
        var deleted = await _appointmentService.GetAppointmentByIdAsync(5);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAppointmentAsync_NonExistent_NoError()
    {
        // Act
        var action = () => _appointmentService.DeleteAppointmentAsync(999);

        // Assert
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public async Task ConfirmAppointmentAsync_ScheduledAppointment_Confirms()
    {
        // Act
        await _appointmentService.ConfirmAppointmentAsync(1);

        // Assert
        var appointment = await _appointmentService.GetAppointmentByIdAsync(1);
        appointment!.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public async Task CheckInAppointmentAsync_ConfirmedAppointment_ChecksIn()
    {
        // Act
        await _appointmentService.CheckInAppointmentAsync(2);

        // Assert
        var appointment = await _appointmentService.GetAppointmentByIdAsync(2);
        appointment!.Status.Should().Be(AppointmentStatus.CheckedIn);
    }

    [Fact]
    public async Task CompleteAppointmentAsync_CheckedInAppointment_Completes()
    {
        // Act
        await _appointmentService.CompleteAppointmentAsync(3);

        // Assert
        var appointment = await _appointmentService.GetAppointmentByIdAsync(3);
        appointment!.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public async Task CancelAppointmentAsync_ActiveAppointment_Cancels()
    {
        // Act
        await _appointmentService.CancelAppointmentAsync(1, "Patient request");

        // Assert
        var appointment = await _appointmentService.GetAppointmentByIdAsync(1);
        appointment!.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task NoShowAppointmentAsync_ConfirmedAppointment_MarksNoShow()
    {
        // Act
        await _appointmentService.NoShowAppointmentAsync(2);

        // Assert
        var appointment = await _appointmentService.GetAppointmentByIdAsync(2);
        appointment!.Status.Should().Be(AppointmentStatus.NoShow);
    }

    #endregion

    #region IsTimeSlotAvailableAsync Tests

    [Fact]
    public async Task IsTimeSlotAvailableAsync_AvailableSlot_ReturnsTrue()
    {
        // Arrange
        var availableSlot = DateTime.UtcNow.Date.AddDays(5).AddHours(16);

        // Act
        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            1, 1, availableSlot, availableSlot.AddMinutes(30));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_ConflictingSlot_ReturnsFalse()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var conflictingSlot = tomorrow.AddHours(9).AddMinutes(15); // Overlaps with appointment 1

        // Act
        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            1, 1, conflictingSlot, conflictingSlot.AddMinutes(30));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_ExcludesSelfWhenRescheduling()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var existingSlot = tomorrow.AddHours(9);

        // Act
        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            1, 1, existingSlot, existingSlot.AddMinutes(30), excludeAppointmentId: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTimeSlotAvailableAsync_CancelledAppointment_SlotAvailable()
    {
        // Arrange - Appointment 5 is cancelled
        var cancelledSlot = DateTime.UtcNow.AddDays(-2).Date.AddHours(11);

        // Act - Check if the cancelled slot is available
        var result = await _appointmentService.IsTimeSlotAvailableAsync(
            1, 1, cancelledSlot, cancelledSlot.AddMinutes(30));

        // Assert - Should be available since original was cancelled
        result.Should().BeTrue();
    }

    #endregion

    #region GetAvailableSlotsAsync Tests

    [Fact]
    public async Task GetAvailableSlotsAsync_ReturnsAvailableSlots()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.Date.AddDays(10);

        // Act
        var result = await _appointmentService.GetAvailableSlotsAsync(1, null, futureDate, 30);

        // Assert
        var slots = result.ToList();
        slots.Should().NotBeEmpty();
        slots.Should().OnlyContain(s => s.Date == futureDate);
    }

    #endregion

    #region RescheduleAppointmentAsync Tests

    [Fact]
    public async Task RescheduleAppointmentAsync_ValidReschedule_Reschedules()
    {
        // Arrange
        var newTime = DateTime.UtcNow.Date.AddDays(7).AddHours(14);

        // Act
        var result = await _appointmentService.RescheduleAppointmentAsync(1, newTime, newTime.AddMinutes(30));

        // Assert
        result.Should().NotBeNull();
        result.StartTime.Should().Be(newTime);
        result.EndTime.Should().Be(newTime.AddMinutes(30));
    }

    [Fact]
    public async Task RescheduleAppointmentAsync_ConflictingTime_ThrowsException()
    {
        // Arrange - Try to reschedule to appointment 2's slot
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var conflictingTime = tomorrow.AddHours(10);

        // Act
        var action = () => _appointmentService.RescheduleAppointmentAsync(1, conflictingTime, conflictingTime.AddMinutes(30));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalAppointmentsCountAsync_ReturnsCorrectCount()
    {
        // Act
        var result = await _appointmentService.GetTotalAppointmentsCountAsync(1);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetTodayAppointmentsCountAsync_ReturnsCorrectCount()
    {
        // Act
        var result = await _appointmentService.GetTodayAppointmentsCountAsync(1);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetAppointmentStatusDistributionAsync_ReturnsDistribution()
    {
        // Act
        var result = await _appointmentService.GetAppointmentStatusDistributionAsync(1);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().ContainKey(AppointmentStatus.Scheduled);
        result.Should().ContainKey(AppointmentStatus.Confirmed);
        result.Should().ContainKey(AppointmentStatus.Completed);
    }

    [Fact]
    public async Task GetAppointmentTypeDistributionAsync_ReturnsDistribution()
    {
        // Act
        var result = await _appointmentService.GetAppointmentTypeDistributionAsync(1);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().ContainKey(AppointmentType.Consultation);
        result.Should().ContainKey(AppointmentType.FollowUp);
    }

    [Fact]
    public async Task GetCompletedAppointmentsCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _appointmentService.GetCompletedAppointmentsCountAsync(1, startDate, endDate);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetCancelledAppointmentsCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _appointmentService.GetCancelledAppointmentsCountAsync(1, startDate, endDate);

        // Assert
        result.Should().Be(1);
    }

    #endregion
}
