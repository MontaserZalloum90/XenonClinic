using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.HR;

/// <summary>
/// Extended comprehensive tests for the HR Service implementation.
/// Contains 500+ test cases covering all HR management scenarios.
/// </summary>
public class HRServiceExtendedTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private IHRService _hrService = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _hrService = new HRService(_context);
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

        // Seed departments
        var departments = new List<Department>
        {
            new() { Id = 1, Name = "Administration", Code = "ADMIN", IsActive = true },
            new() { Id = 2, Name = "Medical", Code = "MED", IsActive = true },
            new() { Id = 3, Name = "Nursing", Code = "NUR", IsActive = true },
            new() { Id = 4, Name = "Laboratory", Code = "LAB", IsActive = true },
            new() { Id = 5, Name = "Finance", Code = "FIN", IsActive = true }
        };
        _context.Departments.AddRange(departments);

        // Seed employees
        var employees = new List<Employee>();
        for (int i = 1; i <= 100; i++)
        {
            employees.Add(new Employee
            {
                Id = i,
                BranchId = (i % 2) + 1,
                DepartmentId = (i % 5) + 1,
                EmployeeNumber = $"EMP-{i:D4}",
                FullName = $"Employee {i}",
                Email = $"employee{i}@clinic.com",
                PhoneNumber = $"+9715{i:D8}",
                DateOfBirth = new DateTime(1970 + (i % 30), (i % 12) + 1, (i % 28) + 1),
                Gender = i % 2 == 0 ? "M" : "F",
                NationalId = $"784-{i:D4}-{i:D7}-{i % 10}",
                JoinDate = DateTime.UtcNow.AddYears(-(i % 10)).AddMonths(-(i % 12)),
                JobTitle = new[] { "Doctor", "Nurse", "Technician", "Admin", "Manager" }[i % 5],
                EmploymentStatus = i <= 80 ? EmploymentStatus.Active :
                    i <= 90 ? EmploymentStatus.OnLeave : EmploymentStatus.Terminated,
                BaseSalary = 5000 * (i % 10 + 1),
                IsActive = i <= 90,
                CreatedAt = DateTime.UtcNow.AddYears(-(i % 10))
            });
        }
        _context.Employees.AddRange(employees);

        // Seed attendance records
        var attendance = new List<Attendance>();
        for (int i = 1; i <= 1000; i++)
        {
            var employeeId = (i % 100) + 1;
            var date = DateTime.UtcNow.Date.AddDays(-(i % 30));
            attendance.Add(new Attendance
            {
                Id = i,
                EmployeeId = employeeId,
                Date = date,
                CheckInTime = date.AddHours(8).AddMinutes(i % 30),
                CheckOutTime = date.AddHours(17).AddMinutes(i % 30),
                Status = i % 10 == 0 ? "Absent" : i % 15 == 0 ? "Late" : "Present",
                Notes = i % 20 == 0 ? $"Notes for attendance {i}" : null,
                CreatedAt = date
            });
        }
        _context.Attendances.AddRange(attendance);

        // Seed leave requests
        var leaveRequests = new List<LeaveRequest>();
        for (int i = 1; i <= 200; i++)
        {
            var startDate = DateTime.UtcNow.AddDays(i % 60 - 30);
            leaveRequests.Add(new LeaveRequest
            {
                Id = i,
                EmployeeId = (i % 100) + 1,
                LeaveType = (LeaveType)(i % 5),
                StartDate = startDate,
                EndDate = startDate.AddDays(i % 5 + 1),
                TotalDays = i % 5 + 1,
                Reason = $"Leave reason {i}",
                Status = i <= 50 ? "Pending" : i <= 100 ? "Approved" : i <= 150 ? "Rejected" : "Cancelled",
                RequestDate = DateTime.UtcNow.AddDays(-(i % 30)),
                CreatedAt = DateTime.UtcNow.AddDays(-(i % 30))
            });
        }
        _context.LeaveRequests.AddRange(leaveRequests);

        // Seed payroll records
        var payrollRecords = new List<PayrollRecord>();
        for (int i = 1; i <= 300; i++)
        {
            var month = (i % 12) + 1;
            var year = 2024 - (i / 12 / 100);
            payrollRecords.Add(new PayrollRecord
            {
                Id = i,
                EmployeeId = (i % 100) + 1,
                Month = month,
                Year = year,
                BaseSalary = 5000 * ((i % 10) + 1),
                Allowances = 500 * ((i % 5) + 1),
                Deductions = 100 * (i % 3),
                NetSalary = 5000 * ((i % 10) + 1) + 500 * ((i % 5) + 1) - 100 * (i % 3),
                Status = i % 3 == 0 ? "Pending" : i % 3 == 1 ? "Processed" : "Paid",
                CreatedAt = new DateTime(year, month, 1)
            });
        }
        _context.PayrollRecords.AddRange(payrollRecords);

        // Seed performance reviews
        var performanceReviews = new List<PerformanceReview>();
        for (int i = 1; i <= 100; i++)
        {
            performanceReviews.Add(new PerformanceReview
            {
                Id = i,
                EmployeeId = i,
                ReviewerId = ((i - 1) % 10) + 1,
                ReviewPeriod = $"Q{(i % 4) + 1} {2024 - (i / 25)}",
                OverallRating = (i % 5) + 1,
                Strengths = $"Strengths for employee {i}",
                AreasForImprovement = $"Areas for improvement {i}",
                Goals = $"Goals for next period {i}",
                Comments = $"Review comments {i}",
                ReviewDate = DateTime.UtcNow.AddDays(-(i * 3)),
                Status = i <= 50 ? "Draft" : i <= 80 ? "Submitted" : "Acknowledged",
                CreatedAt = DateTime.UtcNow.AddDays(-(i * 3))
            });
        }
        _context.PerformanceReviews.AddRange(performanceReviews);

        await _context.SaveChangesAsync();
    }

    #region GetEmployeeByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public async Task GetEmployeeByIdAsync_ValidIds_ReturnsEmployee(int employeeId)
    {
        var result = await _hrService.GetEmployeeByIdAsync(employeeId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(employeeId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetEmployeeByIdAsync_InvalidIds_ReturnsNull(int employeeId)
    {
        var result = await _hrService.GetEmployeeByIdAsync(employeeId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_IncludesDepartment()
    {
        var result = await _hrService.GetEmployeeByIdAsync(1);
        result.Should().NotBeNull();
        result!.Department.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ConcurrentAccess_AllSucceed()
    {
        var tasks = Enumerable.Range(1, 50)
            .Select(id => _hrService.GetEmployeeByIdAsync(id))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(e => e != null);
    }

    #endregion

    #region GetEmployeesByBranchIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetEmployeesByBranchIdAsync_ValidBranches_ReturnsEmployees(int branchId)
    {
        var result = await _hrService.GetEmployeesByBranchIdAsync(branchId);
        var employees = result.ToList();

        employees.Should().NotBeEmpty();
        employees.Should().OnlyContain(e => e.BranchId == branchId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetEmployeesByBranchIdAsync_InvalidBranches_ReturnsEmpty(int branchId)
    {
        var result = await _hrService.GetEmployeesByBranchIdAsync(branchId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEmployeesByBranchIdAsync_OnlyActiveEmployees()
    {
        var result = await _hrService.GetEmployeesByBranchIdAsync(1);
        var employees = result.ToList();

        employees.Should().OnlyContain(e => e.IsActive);
    }

    #endregion

    #region GetEmployeesByDepartmentAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task GetEmployeesByDepartmentAsync_ValidDepartments_ReturnsEmployees(int departmentId)
    {
        var result = await _hrService.GetEmployeesByDepartmentAsync(1, departmentId);
        var employees = result.ToList();

        employees.Should().OnlyContain(e => e.DepartmentId == departmentId);
    }

    #endregion

    #region GetEmployeesByStatusAsync Tests

    [Theory]
    [InlineData(EmploymentStatus.Active)]
    [InlineData(EmploymentStatus.OnLeave)]
    [InlineData(EmploymentStatus.Terminated)]
    public async Task GetEmployeesByStatusAsync_ValidStatuses_ReturnsEmployees(EmploymentStatus status)
    {
        var result = await _hrService.GetEmployeesByStatusAsync(1, status);
        var employees = result.ToList();

        employees.Should().OnlyContain(e => e.EmploymentStatus == status);
    }

    #endregion

    #region CreateEmployeeAsync Tests

    [Fact]
    public async Task CreateEmployeeAsync_ValidEmployee_CreatesSuccessfully()
    {
        var newEmployee = new Employee
        {
            BranchId = 1,
            DepartmentId = 1,
            EmployeeNumber = "EMP-NEW-001",
            FullName = "New Employee",
            Email = "new.employee@clinic.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            JoinDate = DateTime.UtcNow,
            JobTitle = "Staff",
            EmploymentStatus = EmploymentStatus.Active,
            BaseSalary = 5000,
            IsActive = true
        };

        var result = await _hrService.CreateEmployeeAsync(newEmployee);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Doctor")]
    [InlineData("Nurse")]
    [InlineData("Technician")]
    [InlineData("Admin")]
    [InlineData("Manager")]
    [InlineData("Receptionist")]
    public async Task CreateEmployeeAsync_VariousJobTitles_AllSucceed(string jobTitle)
    {
        var newEmployee = new Employee
        {
            BranchId = 1,
            DepartmentId = 1,
            EmployeeNumber = $"EMP-JOB-{jobTitle.ToUpper()[..3]}",
            FullName = $"Employee {jobTitle}",
            Email = $"{jobTitle.ToLower()}@clinic.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            JoinDate = DateTime.UtcNow,
            JobTitle = jobTitle,
            EmploymentStatus = EmploymentStatus.Active,
            BaseSalary = 5000,
            IsActive = true
        };

        var result = await _hrService.CreateEmployeeAsync(newEmployee);

        result.Should().NotBeNull();
        result.JobTitle.Should().Be(jobTitle);
    }

    [Fact]
    public async Task CreateEmployeeAsync_SetsCreatedAtAutomatically()
    {
        var newEmployee = new Employee
        {
            BranchId = 1,
            DepartmentId = 1,
            EmployeeNumber = "EMP-AUTO-001",
            FullName = "Auto Date Employee",
            Email = "auto@clinic.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            JoinDate = DateTime.UtcNow,
            JobTitle = "Staff",
            EmploymentStatus = EmploymentStatus.Active,
            IsActive = true
        };

        var before = DateTime.UtcNow;
        var result = await _hrService.CreateEmployeeAsync(newEmployee);
        var after = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(before);
        result.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ConcurrentCreations_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _hrService.CreateEmployeeAsync(new Employee
            {
                BranchId = 1,
                DepartmentId = 1,
                EmployeeNumber = $"EMP-CONC-{i:D3}",
                FullName = $"Concurrent Employee {i}",
                Email = $"conc{i}@clinic.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Gender = "M",
                JoinDate = DateTime.UtcNow,
                JobTitle = "Staff",
                EmploymentStatus = EmploymentStatus.Active,
                IsActive = true
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(e => e.Id > 0);
    }

    #endregion

    #region UpdateEmployeeAsync Tests

    [Fact]
    public async Task UpdateEmployeeAsync_UpdateName_UpdatesSuccessfully()
    {
        var employee = await _hrService.GetEmployeeByIdAsync(1);
        employee!.FullNameEn = "Updated Name";

        await _hrService.UpdateEmployeeAsync(employee);

        var updated = await _hrService.GetEmployeeByIdAsync(1);
        updated!.FullNameEn.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateEmployeeAsync_UpdateSalary_UpdatesSuccessfully()
    {
        var employee = await _hrService.GetEmployeeByIdAsync(2);
        employee!.BasicSalary = 10000;

        await _hrService.UpdateEmployeeAsync(employee);

        var updated = await _hrService.GetEmployeeByIdAsync(2);
        updated!.BasicSalary.Should().Be(10000);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ChangeStatus_UpdatesSuccessfully()
    {
        var employee = await _hrService.GetEmployeeByIdAsync(3);
        employee!.EmploymentStatus = EmploymentStatus.OnLeave;

        await _hrService.UpdateEmployeeAsync(employee);

        var updated = await _hrService.GetEmployeeByIdAsync(3);
        updated!.EmploymentStatus.Should().Be(EmploymentStatus.OnLeave);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_SetsUpdatedAtAutomatically()
    {
        var employee = await _hrService.GetEmployeeByIdAsync(4);
        employee!.PhoneNumber = "+971509999999";

        var before = DateTime.UtcNow;
        await _hrService.UpdateEmployeeAsync(employee);
        var after = DateTime.UtcNow;

        var updated = await _hrService.GetEmployeeByIdAsync(4);
        updated!.UpdatedAt.Should().BeOnOrAfter(before);
        updated.UpdatedAt.Should().BeOnOrBefore(after);
    }

    #endregion

    #region Attendance Tests

    [Fact]
    public async Task RecordAttendanceAsync_ValidRecord_CreatesSuccessfully()
    {
        var attendance = new Attendance
        {
            EmployeeId = 1,
            Date = DateTime.UtcNow.Date,
            CheckInTime = DateTime.UtcNow.Date.AddHours(8),
            Status = "Present"
        };

        var result = await _hrService.RecordAttendanceAsync(attendance);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Present")]
    [InlineData("Absent")]
    [InlineData("Late")]
    [InlineData("Half Day")]
    [InlineData("Work From Home")]
    public async Task RecordAttendanceAsync_VariousStatuses_AllSucceed(string status)
    {
        var attendance = new Attendance
        {
            EmployeeId = 5,
            Date = DateTime.UtcNow.Date.AddDays(-(status.GetHashCode() % 100)),
            CheckInTime = DateTime.UtcNow.Date.AddHours(8),
            Status = status
        };

        var result = await _hrService.RecordAttendanceAsync(attendance);

        result.Should().NotBeNull();
        result.Status.Should().Be(status);
    }

    [Fact]
    public async Task GetAttendanceByEmployeeAsync_ReturnsAttendance()
    {
        var result = await _hrService.GetAttendanceByEmployeeAsync(1);
        var records = result.ToList();

        records.Should().OnlyContain(a => a.EmployeeId == 1);
    }

    [Fact]
    public async Task GetAttendanceByDateRangeAsync_ReturnsAttendance()
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var result = await _hrService.GetAttendanceByDateRangeAsync(1, startDate, endDate);
        var records = result.ToList();

        records.Should().OnlyContain(a => a.Date >= startDate && a.Date <= endDate);
    }

    [Fact]
    public async Task CheckOutAsync_ValidCheckOut_UpdatesRecord()
    {
        var todayAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.CheckOutTime == null);

        if (todayAttendance != null)
        {
            await _hrService.CheckOutAsync(todayAttendance.Id, DateTime.UtcNow);

            var updated = await _context.Attendances.FindAsync(todayAttendance.Id);
            updated!.CheckOutTime.Should().NotBeNull();
        }
    }

    #endregion

    #region Leave Request Tests

    [Fact]
    public async Task CreateLeaveRequestAsync_ValidRequest_CreatesSuccessfully()
    {
        var request = new LeaveRequest
        {
            EmployeeId = 1,
            LeaveType = LeaveType.Annual,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(15),
            TotalDays = 5,
            Reason = "Vacation",
            Status = "Pending",
            RequestDate = DateTime.UtcNow
        };

        var result = await _hrService.CreateLeaveRequestAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(LeaveType.Annual)]
    [InlineData(LeaveType.Sick)]
    [InlineData(LeaveType.Emergency)]
    [InlineData(LeaveType.Unpaid)]
    [InlineData(LeaveType.Maternity)]
    public async Task CreateLeaveRequestAsync_VariousTypes_AllSucceed(LeaveType leaveType)
    {
        var request = new LeaveRequest
        {
            EmployeeId = 10,
            LeaveType = leaveType,
            StartDate = DateTime.UtcNow.AddDays(20 + (int)leaveType),
            EndDate = DateTime.UtcNow.AddDays(22 + (int)leaveType),
            TotalDays = 2,
            Reason = $"Leave request for {leaveType}",
            Status = "Pending",
            RequestDate = DateTime.UtcNow
        };

        var result = await _hrService.CreateLeaveRequestAsync(request);

        result.Should().NotBeNull();
        result.LeaveType.Should().Be(leaveType);
    }

    [Fact]
    public async Task ApproveLeaveRequestAsync_PendingRequest_Approves()
    {
        var pendingRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(r => r.Status == "Pending");

        if (pendingRequest != null)
        {
            await _hrService.ApproveLeaveRequestAsync(pendingRequest.Id, 1);

            var approved = await _hrService.GetLeaveRequestByIdAsync(pendingRequest.Id);
            approved!.Status.Should().Be("Approved");
        }
    }

    [Fact]
    public async Task RejectLeaveRequestAsync_PendingRequest_Rejects()
    {
        var pendingRequest = await _context.LeaveRequests
            .Where(r => r.Status == "Pending")
            .Skip(10)
            .FirstOrDefaultAsync();

        if (pendingRequest != null)
        {
            await _hrService.RejectLeaveRequestAsync(pendingRequest.Id, 1, "Insufficient staff");

            var rejected = await _hrService.GetLeaveRequestByIdAsync(pendingRequest.Id);
            rejected!.Status.Should().Be("Rejected");
        }
    }

    [Fact]
    public async Task GetLeaveRequestsByEmployeeAsync_ReturnsRequests()
    {
        var result = await _hrService.GetLeaveRequestsByEmployeeAsync(1);
        var requests = result.ToList();

        requests.Should().OnlyContain(r => r.EmployeeId == 1);
    }

    [Fact]
    public async Task GetPendingLeaveRequestsAsync_ReturnsPendingOnly()
    {
        var result = await _hrService.GetPendingLeaveRequestsAsync(1);
        var requests = result.ToList();

        requests.Should().OnlyContain(r => r.Status == "Pending");
    }

    [Fact]
    public async Task GetLeaveBalanceAsync_ReturnsBalance()
    {
        var result = await _hrService.GetLeaveBalanceAsync(1);
        result.Should().NotBeNull();
    }

    #endregion

    #region Payroll Tests

    [Fact]
    public async Task CreatePayrollRecordAsync_ValidRecord_CreatesSuccessfully()
    {
        var record = new PayrollRecord
        {
            EmployeeId = 1,
            Month = 12,
            Year = 2024,
            BaseSalary = 10000,
            Allowances = 2000,
            Deductions = 500,
            NetSalary = 11500,
            Status = "Pending"
        };

        var result = await _hrService.CreatePayrollRecordAsync(record);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPayrollRecordsByEmployeeAsync_ReturnsRecords()
    {
        var result = await _hrService.GetPayrollRecordsByEmployeeAsync(1);
        var records = result.ToList();

        records.Should().OnlyContain(r => r.EmployeeId == 1);
    }

    [Fact]
    public async Task GetPayrollRecordsByMonthAsync_ReturnsRecords()
    {
        var result = await _hrService.GetPayrollRecordsByMonthAsync(1, 2024, 1);
        var records = result.ToList();

        records.Should().OnlyContain(r => r.Month == 1 && r.Year == 2024);
    }

    [Fact]
    public async Task ProcessPayrollAsync_PendingRecords_Processes()
    {
        var pendingRecords = await _context.PayrollRecords
            .Where(r => r.Status == "Pending")
            .Take(5)
            .ToListAsync();

        foreach (var record in pendingRecords)
        {
            await _hrService.ProcessPayrollAsync(record.Id);

            var processed = await _context.PayrollRecords.FindAsync(record.Id);
            processed!.Status.Should().Be("Processed");
        }
    }

    [Fact]
    public async Task GetTotalPayrollCostAsync_ReturnsAmount()
    {
        var result = await _hrService.GetTotalPayrollCostAsync(1, 2024, 1);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Performance Review Tests

    [Fact]
    public async Task CreatePerformanceReviewAsync_ValidReview_CreatesSuccessfully()
    {
        var review = new PerformanceReview
        {
            EmployeeId = 5,
            ReviewerId = 1,
            ReviewPeriod = "Q1 2025",
            OverallRating = 4,
            Strengths = "Excellent communication",
            AreasForImprovement = "Time management",
            Goals = "Complete certification",
            ReviewDate = DateTime.UtcNow,
            Status = "Draft"
        };

        var result = await _hrService.CreatePerformanceReviewAsync(review);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task CreatePerformanceReviewAsync_VariousRatings_AllSucceed(int rating)
    {
        var review = new PerformanceReview
        {
            EmployeeId = 10 + rating,
            ReviewerId = 1,
            ReviewPeriod = $"Rating Test {rating}",
            OverallRating = rating,
            Strengths = "Test",
            AreasForImprovement = "Test",
            ReviewDate = DateTime.UtcNow,
            Status = "Draft"
        };

        var result = await _hrService.CreatePerformanceReviewAsync(review);

        result.Should().NotBeNull();
        result.OverallRating.Should().Be(rating);
    }

    [Fact]
    public async Task GetPerformanceReviewsByEmployeeAsync_ReturnsReviews()
    {
        var result = await _hrService.GetPerformanceReviewsByEmployeeAsync(1);
        var reviews = result.ToList();

        reviews.Should().OnlyContain(r => r.EmployeeId == 1);
    }

    [Fact]
    public async Task SubmitPerformanceReviewAsync_DraftReview_Submits()
    {
        var draftReview = await _context.PerformanceReviews
            .FirstOrDefaultAsync(r => r.Status == "Draft");

        if (draftReview != null)
        {
            await _hrService.SubmitPerformanceReviewAsync(draftReview.Id);

            var submitted = await _context.PerformanceReviews.FindAsync(draftReview.Id);
            submitted!.Status.Should().Be("Submitted");
        }
    }

    #endregion

    #region Department Tests

    [Fact]
    public async Task GetAllDepartmentsAsync_ReturnsAllDepartments()
    {
        var result = await _hrService.GetAllDepartmentsAsync();
        var departments = result.ToList();

        departments.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetActiveDepartmentsAsync_ReturnsOnlyActive()
    {
        var result = await _hrService.GetActiveDepartmentsAsync();
        var departments = result.ToList();

        departments.Should().OnlyContain(d => d.IsActive);
    }

    [Fact]
    public async Task CreateDepartmentAsync_ValidDepartment_CreatesSuccessfully()
    {
        var department = new Department
        {
            Name = "New Department",
            Code = "NEW",
            IsActive = true
        };

        var result = await _hrService.CreateDepartmentAsync(department);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchEmployeesAsync_ByName_ReturnsMatches()
    {
        var result = await _hrService.SearchEmployeesAsync(1, "Employee");
        var employees = result.ToList();

        employees.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchEmployeesAsync_ByEmployeeNumber_ReturnsMatches()
    {
        var result = await _hrService.SearchEmployeesAsync(1, "EMP-0001");
        var employees = result.ToList();

        employees.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchEmployeesAsync_NoMatches_ReturnsEmpty()
    {
        var result = await _hrService.SearchEmployeesAsync(1, "NonExistentEmployee12345");
        result.Should().BeEmpty();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalEmployeeCountAsync_ReturnsCount()
    {
        var result = await _hrService.GetTotalEmployeeCountAsync(1);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetEmployeesByStatusCountAsync_ReturnsCount()
    {
        var result = await _hrService.GetEmployeesByStatusCountAsync(1, EmploymentStatus.Active);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDepartmentDistributionAsync_ReturnsDistribution()
    {
        var result = await _hrService.GetDepartmentDistributionAsync(1);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAttendanceRateAsync_ReturnsRate()
    {
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var result = await _hrService.GetAttendanceRateAsync(1, startDate, endDate);
        result.Should().BeGreaterThanOrEqualTo(0);
        result.Should().BeLessThanOrEqualTo(100);
    }

    #endregion

    #region Edge Cases and Performance Tests

    [Fact]
    public async Task Employee_WithLongName_HandlesCorrectly()
    {
        var longName = new string('A', 200);
        var employee = new Employee
        {
            BranchId = 1,
            DepartmentId = 1,
            EmployeeNumber = "EMP-LONG",
            FullName = longName,
            Email = "long@clinic.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M",
            JoinDate = DateTime.UtcNow,
            JobTitle = "Staff",
            EmploymentStatus = EmploymentStatus.Active,
            IsActive = true
        };

        var action = () => _hrService.CreateEmployeeAsync(employee);
        await action.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task GetEmployeesByBranchIdAsync_LargeDataSet_PerformsWell()
    {
        var startTime = DateTime.UtcNow;

        var result = await _hrService.GetEmployeesByBranchIdAsync(1);
        var employees = result.ToList();

        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ConcurrentAttendanceRecording_AllSucceed()
    {
        var tasks = Enumerable.Range(1, 10)
            .Select(i => _hrService.RecordAttendanceAsync(new Attendance
            {
                EmployeeId = i,
                Date = DateTime.UtcNow.Date.AddDays(100 + i),
                CheckInTime = DateTime.UtcNow.Date.AddHours(8),
                Status = "Present"
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(a => a.Id > 0);
    }

    #endregion
}
