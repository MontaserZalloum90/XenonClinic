using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Services;

public class HRServiceTests
{
    private ClinicDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ClinicDbContext(options);
    }

    private Mock<ISequenceGenerator> GetMockSequenceGenerator()
    {
        var mock = new Mock<ISequenceGenerator>();
        mock.Setup(s => s.GenerateEmployeeCodeAsync(It.IsAny<int>()))
            .ReturnsAsync((int branchId) => $"EMP-{branchId}-001");
        return mock;
    }

    private async Task<(ClinicDbContext context, Employee employee)> SetupEmployeeAsync()
    {
        var context = GetInMemoryDbContext();

        // Add required branch and department
        var branch = new Branch { Id = 1, Name = "Main Branch", Code = "MB" };
        var department = new Department { Id = 1, Name = "IT", BranchId = 1 };
        var jobPosition = new JobPosition { Id = 1, Title = "Developer", BranchId = 1, MinSalary = 5000, MaxSalary = 15000 };

        context.Branches.Add(branch);
        context.Departments.Add(department);
        context.JobPositions.Add(jobPosition);
        await context.SaveChangesAsync();

        var employee = new Employee
        {
            Id = 1,
            EmployeeCode = "EMP-001",
            FullNameEn = "John Doe",
            EmiratesId = "784-1234-5678901-1",
            Email = "john@test.com",
            PhoneNumber = "+971501234567",
            BranchId = 1,
            DepartmentId = 1,
            JobPositionId = 1,
            BasicSalary = 10000,
            HireDate = DateTime.UtcNow.AddYears(-1),
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            Nationality = "AE",
            Address = "Dubai",
            WorkStartTime = new TimeOnly(9, 0),
            WorkEndTime = new TimeOnly(18, 0),
            AnnualLeaveBalance = 30,
            SickLeaveBalance = 90
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        return (context, employee);
    }

    #region Employee Management Tests

    [Fact]
    public async Task CreateEmployeeAsync_WithValidData_CreatesEmployee()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var branch = new Branch { Id = 1, Name = "Main Branch", Code = "MB" };
        var department = new Department { Id = 1, Name = "IT", BranchId = 1 };
        var jobPosition = new JobPosition { Id = 1, Title = "Developer", BranchId = 1, MinSalary = 5000, MaxSalary = 15000 };

        context.Branches.Add(branch);
        context.Departments.Add(department);
        context.JobPositions.Add(jobPosition);
        await context.SaveChangesAsync();

        var employee = new Employee
        {
            EmployeeCode = "EMP-001",
            FullNameEn = "John Doe",
            EmiratesId = "784-1234-5678901-1",
            Email = "john@test.com",
            PhoneNumber = "+971501234567",
            BranchId = 1,
            DepartmentId = 1,
            JobPositionId = 1,
            BasicSalary = 10000,
            HireDate = DateTime.UtcNow,
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            Nationality = "AE",
            Address = "Dubai"
        };

        // Act
        var result = await service.CreateEmployeeAsync(employee);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("EMP-001", result.EmployeeCode);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithZeroSalary_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var employee = new Employee
        {
            EmployeeCode = "EMP-001",
            FullNameEn = "John Doe",
            EmiratesId = "784-1234-5678901-1",
            BranchId = 1,
            DepartmentId = 1,
            JobPositionId = 1,
            BasicSalary = 0, // Invalid
            HireDate = DateTime.UtcNow,
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            Nationality = "AE",
            Address = "Dubai",
            Email = "test@test.com",
            PhoneNumber = "123456789"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateEmployeeAsync(employee));
        Assert.Contains("salary must be greater than zero", exception.Message);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithDuplicateEmiratesId_ThrowsException()
    {
        // Arrange
        var (context, existingEmployee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var newEmployee = new Employee
        {
            EmployeeCode = "EMP-002",
            FullNameEn = "Jane Doe",
            EmiratesId = existingEmployee.EmiratesId, // Duplicate
            BranchId = 1,
            DepartmentId = 1,
            JobPositionId = 1,
            BasicSalary = 10000,
            HireDate = DateTime.UtcNow,
            DateOfBirth = new DateTime(1995, 1, 1),
            Gender = "F",
            Nationality = "AE",
            Address = "Abu Dhabi",
            Email = "jane@test.com",
            PhoneNumber = "987654321"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateEmployeeAsync(newEmployee));
        Assert.Contains("Emirates ID", exception.Message);
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task CreateEmployeeAsync_WithDuplicateEmployeeCode_ThrowsException()
    {
        // Arrange
        var (context, existingEmployee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var newEmployee = new Employee
        {
            EmployeeCode = existingEmployee.EmployeeCode, // Duplicate
            FullNameEn = "Jane Doe",
            EmiratesId = "784-9999-9999999-9",
            BranchId = 1,
            DepartmentId = 1,
            JobPositionId = 1,
            BasicSalary = 10000,
            HireDate = DateTime.UtcNow,
            DateOfBirth = new DateTime(1995, 1, 1),
            Gender = "F",
            Nationality = "AE",
            Address = "Abu Dhabi",
            Email = "jane@test.com",
            PhoneNumber = "987654321"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateEmployeeAsync(newEmployee));
        Assert.Contains("code", exception.Message.ToLower());
        Assert.Contains("already exists", exception.Message);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_PerformsSoftDelete()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        // Act
        await service.DeleteEmployeeAsync(employee.Id);

        // Assert
        var deletedEmployee = await context.Employees.FindAsync(employee.Id);
        Assert.NotNull(deletedEmployee);
        Assert.True(deletedEmployee.IsDeleted);
        Assert.NotNull(deletedEmployee.DeletedAt);
        Assert.Equal(EmploymentStatus.Terminated, deletedEmployee.EmploymentStatus);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ExcludesDeletedEmployees()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        // Soft delete the employee
        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetEmployeeByIdAsync(employee.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_WithDuplicateEmiratesId_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        // Add another employee
        var anotherEmployee = new Employee
        {
            EmployeeCode = "EMP-002",
            FullNameEn = "Jane Doe",
            EmiratesId = "784-9999-9999999-9",
            BranchId = 1,
            DepartmentId = 1,
            JobPositionId = 1,
            BasicSalary = 10000,
            HireDate = DateTime.UtcNow,
            DateOfBirth = new DateTime(1995, 1, 1),
            Gender = "F",
            Nationality = "AE",
            Address = "Abu Dhabi",
            Email = "jane@test.com",
            PhoneNumber = "987654321"
        };
        context.Employees.Add(anotherEmployee);
        await context.SaveChangesAsync();

        // Try to update the first employee with the second's Emirates ID
        employee.EmiratesId = anotherEmployee.EmiratesId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateEmployeeAsync(employee));
        Assert.Contains("Emirates ID", exception.Message);
    }

    #endregion

    #region Attendance Management Tests

    [Fact]
    public async Task CheckInAsync_WithValidData_CreatesAttendance()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var checkInTime = DateTime.UtcNow.Date.AddHours(9); // 9:00 AM

        // Act
        var result = await service.CheckInAsync(employee.Id, checkInTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.EmployeeId);
        Assert.Equal(AttendanceStatus.Present, result.Status);
        Assert.False(result.IsLate);
    }

    [Fact]
    public async Task CheckInAsync_WhenLate_MarksAsLate()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var checkInTime = DateTime.UtcNow.Date.AddHours(10); // 10:00 AM (1 hour late)

        // Act
        var result = await service.CheckInAsync(employee.Id, checkInTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AttendanceStatus.Late, result.Status);
        Assert.True(result.IsLate);
        Assert.Equal(60, result.LateMinutes);
    }

    [Fact]
    public async Task CheckInAsync_DuplicateCheckIn_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var checkInTime = DateTime.UtcNow.Date.AddHours(9);

        // First check-in
        await service.CheckInAsync(employee.Id, checkInTime);

        // Act & Assert - Second check-in same day
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CheckInAsync(employee.Id, checkInTime.AddHours(1)));
        Assert.Contains("already checked in", exception.Message);
    }

    [Fact]
    public async Task CheckInAsync_ForDeletedEmployee_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        employee.IsDeleted = true;
        await context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CheckInAsync(employee.Id, DateTime.UtcNow));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task CheckInAsync_ForInactiveEmployee_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        employee.EmploymentStatus = EmploymentStatus.Terminated;
        await context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CheckInAsync(employee.Id, DateTime.UtcNow));
        Assert.Contains("inactive employee", exception.Message);
    }

    [Fact]
    public async Task CheckOutAsync_WithValidData_RecordsCheckOut()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var checkInTime = DateTime.UtcNow.Date.AddHours(9);
        var checkOutTime = DateTime.UtcNow.Date.AddHours(18);

        var attendance = await service.CheckInAsync(employee.Id, checkInTime);

        // Act
        var result = await service.CheckOutAsync(attendance.Id, checkOutTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.CheckOutTime);
        Assert.Equal(9, result.WorkedHours); // 9 hours
    }

    [Fact]
    public async Task CheckOutAsync_BeforeCheckIn_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var checkInTime = DateTime.UtcNow.Date.AddHours(14); // 2:00 PM
        var checkOutTime = DateTime.UtcNow.Date.AddHours(9);  // 9:00 AM

        var attendance = await service.CheckInAsync(employee.Id, checkInTime);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CheckOutAsync(attendance.Id, checkOutTime));
        Assert.Contains("after check in", exception.Message);
    }

    [Fact]
    public async Task CheckOutAsync_WhenAlreadyCheckedOut_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var checkInTime = DateTime.UtcNow.Date.AddHours(9);
        var checkOutTime = DateTime.UtcNow.Date.AddHours(18);

        var attendance = await service.CheckInAsync(employee.Id, checkInTime);
        await service.CheckOutAsync(attendance.Id, checkOutTime);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CheckOutAsync(attendance.Id, checkOutTime.AddHours(1)));
        Assert.Contains("already been recorded", exception.Message);
    }

    [Fact]
    public async Task CheckOutAsync_CalculatesOvertimeCorrectly()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var checkInTime = DateTime.UtcNow.Date.AddHours(9);
        var checkOutTime = DateTime.UtcNow.Date.AddHours(20); // 8 PM (2 hours overtime)

        var attendance = await service.CheckInAsync(employee.Id, checkInTime);

        // Act
        var result = await service.CheckOutAsync(attendance.Id, checkOutTime);

        // Assert
        Assert.Equal(11, result.WorkedHours); // 11 hours total
        Assert.Equal(2, result.OvertimeHours); // 2 hours overtime (9 expected, 11 worked)
    }

    #endregion

    #region Leave Request Management Tests

    [Fact]
    public async Task CreateLeaveRequestAsync_WithValidData_CreatesRequest()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            Reason = "Vacation"
        };

        // Act
        var result = await service.CreateLeaveRequestAsync(leaveRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LeaveStatus.Pending, result.Status);
        Assert.Equal(4, result.TotalDays);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_EndDateBeforeStartDate_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(10),
            EndDate = DateTime.UtcNow.Date.AddDays(5), // Before start
            Reason = "Vacation"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateLeaveRequestAsync(leaveRequest));
        Assert.Contains("End date must be on or after start date", exception.Message);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_PastStartDate_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(-5), // Past date
            EndDate = DateTime.UtcNow.Date.AddDays(-2),
            Reason = "Vacation"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateLeaveRequestAsync(leaveRequest));
        Assert.Contains("past dates", exception.Message);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_InsufficientBalance_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        employee.AnnualLeaveBalance = 5; // Only 5 days
        await context.SaveChangesAsync();

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(20), // 14 days requested
            Reason = "Vacation"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateLeaveRequestAsync(leaveRequest));
        Assert.Contains("Insufficient", exception.Message);
    }

    [Fact]
    public async Task CreateLeaveRequestAsync_OverlappingApprovedLeave_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        // Add existing approved leave
        var existingLeave = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(8),
            EndDate = DateTime.UtcNow.Date.AddDays(12),
            TotalDays = 5,
            Status = LeaveStatus.Approved,
            Reason = "Previous vacation"
        };
        context.LeaveRequests.Add(existingLeave);
        await context.SaveChangesAsync();

        var overlappingRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(10), // Overlaps
            Reason = "New vacation"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateLeaveRequestAsync(overlappingRequest));
        Assert.Contains("overlaps", exception.Message);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_WithValidRequest_ApprovesAndDeductsBalance()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var initialBalance = employee.AnnualLeaveBalance;

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            TotalDays = 4,
            Status = LeaveStatus.Pending,
            Reason = "Vacation"
        };
        context.LeaveRequests.Add(leaveRequest);
        await context.SaveChangesAsync();

        // Act
        await service.ApproveLeaveRequestAsync(leaveRequest.Id, "manager@test.com");

        // Assert
        var updatedRequest = await context.LeaveRequests.FindAsync(leaveRequest.Id);
        var updatedEmployee = await context.Employees.FindAsync(employee.Id);

        Assert.Equal(LeaveStatus.Approved, updatedRequest!.Status);
        Assert.Equal("manager@test.com", updatedRequest.ApprovedBy);
        Assert.Equal(initialBalance - 4, updatedEmployee!.AnnualLeaveBalance);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_AlreadyApproved_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            TotalDays = 4,
            Status = LeaveStatus.Approved, // Already approved
            Reason = "Vacation"
        };
        context.LeaveRequests.Add(leaveRequest);
        await context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ApproveLeaveRequestAsync(leaveRequest.Id, "manager@test.com"));
        Assert.Contains("Only pending requests can be approved", exception.Message);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_InsufficientBalance_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        employee.AnnualLeaveBalance = 2; // Only 2 days
        await context.SaveChangesAsync();

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            TotalDays = 4, // Requesting 4 days
            Status = LeaveStatus.Pending,
            Reason = "Vacation"
        };
        context.LeaveRequests.Add(leaveRequest);
        await context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ApproveLeaveRequestAsync(leaveRequest.Id, "manager@test.com"));
        Assert.Contains("Insufficient annual leave balance", exception.Message);
    }

    [Fact]
    public async Task RejectLeaveRequestAsync_WithValidRequest_RejectsWithReason()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            TotalDays = 4,
            Status = LeaveStatus.Pending,
            Reason = "Vacation"
        };
        context.LeaveRequests.Add(leaveRequest);
        await context.SaveChangesAsync();

        // Act
        await service.RejectLeaveRequestAsync(leaveRequest.Id, "manager@test.com", "Business needs");

        // Assert
        var updatedRequest = await context.LeaveRequests.FindAsync(leaveRequest.Id);
        Assert.Equal(LeaveStatus.Rejected, updatedRequest!.Status);
        Assert.Equal("Business needs", updatedRequest.RejectionReason);
    }

    [Fact]
    public async Task RejectLeaveRequestAsync_AlreadyRejected_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            TotalDays = 4,
            Status = LeaveStatus.Rejected, // Already rejected
            Reason = "Vacation"
        };
        context.LeaveRequests.Add(leaveRequest);
        await context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RejectLeaveRequestAsync(leaveRequest.Id, "manager@test.com", "Another reason"));
        Assert.Contains("Only pending requests can be rejected", exception.Message);
    }

    [Fact]
    public async Task DeleteLeaveRequestAsync_ApprovedRequest_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.Date.AddDays(7),
            EndDate = DateTime.UtcNow.Date.AddDays(10),
            TotalDays = 4,
            Status = LeaveStatus.Approved,
            Reason = "Vacation"
        };
        context.LeaveRequests.Add(leaveRequest);
        await context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteLeaveRequestAsync(leaveRequest.Id));
        Assert.Contains("Cannot delete an approved leave request", exception.Message);
    }

    #endregion

    #region Department Management Tests

    [Fact]
    public async Task DeleteDepartmentAsync_WithEmployees_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteDepartmentAsync(employee.DepartmentId));
        Assert.Contains("assigned employees", exception.Message);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WithJobPositions_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var branch = new Branch { Id = 1, Name = "Main Branch", Code = "MB" };
        var department = new Department { Id = 1, Name = "IT", BranchId = 1 };
        var jobPosition = new JobPosition { Id = 1, Title = "Developer", BranchId = 1, DepartmentId = 1, MinSalary = 5000, MaxSalary = 15000 };

        context.Branches.Add(branch);
        context.Departments.Add(department);
        context.JobPositions.Add(jobPosition);
        await context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteDepartmentAsync(department.Id));
        Assert.Contains("job positions", exception.Message);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_WithNoEmployeesOrPositions_Succeeds()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var branch = new Branch { Id = 1, Name = "Main Branch", Code = "MB" };
        var department = new Department { Id = 1, Name = "Empty Dept", BranchId = 1 };

        context.Branches.Add(branch);
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        // Act
        await service.DeleteDepartmentAsync(department.Id);

        // Assert
        var deleted = await context.Departments.FindAsync(department.Id);
        Assert.Null(deleted);
    }

    #endregion

    #region Job Position Management Tests

    [Fact]
    public async Task DeleteJobPositionAsync_WithEmployees_ThrowsException()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DeleteJobPositionAsync(employee.JobPositionId));
        Assert.Contains("assigned employees", exception.Message);
    }

    [Fact]
    public async Task DeleteJobPositionAsync_WithNoEmployees_Succeeds()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        var branch = new Branch { Id = 1, Name = "Main Branch", Code = "MB" };
        var jobPosition = new JobPosition { Id = 1, Title = "Empty Position", BranchId = 1, MinSalary = 5000, MaxSalary = 15000 };

        context.Branches.Add(branch);
        context.JobPositions.Add(jobPosition);
        await context.SaveChangesAsync();

        // Act
        await service.DeleteJobPositionAsync(jobPosition.Id);

        // Assert
        var deleted = await context.JobPositions.FindAsync(jobPosition.Id);
        Assert.Null(deleted);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalEmployeesCountAsync_ExcludesDeletedEmployees()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        // Add another employee that is deleted
        var deletedEmployee = new Employee
        {
            EmployeeCode = "EMP-002",
            FullNameEn = "Jane Doe",
            EmiratesId = "784-9999-9999999-9",
            BranchId = 1,
            DepartmentId = 1,
            JobPositionId = 1,
            BasicSalary = 10000,
            HireDate = DateTime.UtcNow,
            DateOfBirth = new DateTime(1995, 1, 1),
            Gender = "F",
            Nationality = "AE",
            Address = "Abu Dhabi",
            Email = "jane@test.com",
            PhoneNumber = "987654321",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        context.Employees.Add(deletedEmployee);
        await context.SaveChangesAsync();

        // Act
        var count = await service.GetTotalEmployeesCountAsync(1);

        // Assert
        Assert.Equal(1, count); // Only the non-deleted employee
    }

    [Fact]
    public async Task GetActiveEmployeesCountAsync_ExcludesDeletedAndInactiveEmployees()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        // Add terminated employee
        var terminatedEmployee = new Employee
        {
            EmployeeCode = "EMP-002",
            FullNameEn = "Terminated Employee",
            EmiratesId = "784-8888-8888888-8",
            BranchId = 1,
            DepartmentId = 1,
            JobPositionId = 1,
            BasicSalary = 10000,
            HireDate = DateTime.UtcNow.AddYears(-2),
            DateOfBirth = new DateTime(1985, 1, 1),
            Gender = "M",
            Nationality = "AE",
            Address = "Dubai",
            Email = "terminated@test.com",
            PhoneNumber = "111222333",
            EmploymentStatus = EmploymentStatus.Terminated
        };
        context.Employees.Add(terminatedEmployee);
        await context.SaveChangesAsync();

        // Act
        var count = await service.GetActiveEmployeesCountAsync(1);

        // Assert
        Assert.Equal(1, count); // Only the active, non-deleted employee
    }

    [Fact]
    public async Task ValidateLeaveBalanceAsync_ReturnsCorrectResult()
    {
        // Arrange
        var (context, employee) = await SetupEmployeeAsync();
        var mockSequenceGenerator = GetMockSequenceGenerator();
        var service = new HRService(context, mockSequenceGenerator.Object);

        employee.AnnualLeaveBalance = 10;
        employee.SickLeaveBalance = 5;
        await context.SaveChangesAsync();

        // Act & Assert
        Assert.True(await service.ValidateLeaveBalanceAsync(employee.Id, LeaveType.Annual, 10));
        Assert.False(await service.ValidateLeaveBalanceAsync(employee.Id, LeaveType.Annual, 11));
        Assert.True(await service.ValidateLeaveBalanceAsync(employee.Id, LeaveType.Sick, 5));
        Assert.False(await service.ValidateLeaveBalanceAsync(employee.Id, LeaveType.Sick, 6));
        Assert.True(await service.ValidateLeaveBalanceAsync(employee.Id, LeaveType.Emergency, 100)); // No balance check for emergency
    }

    #endregion
}
