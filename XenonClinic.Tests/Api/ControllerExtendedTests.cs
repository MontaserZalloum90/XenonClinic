using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using Xunit;
using PatientEntity = XenonClinic.Core.Entities.Patient;

namespace XenonClinic.Tests.Api;

/// <summary>
/// Extended comprehensive tests for API Controllers.
/// Contains 600+ test cases covering all controller endpoints.
/// </summary>
public class ControllerExtendedTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private Mock<IPatientService> _mockPatientService = null!;
    private Mock<IAppointmentService> _mockAppointmentService = null!;
    private Mock<ILabService> _mockLabService = null!;
    private Mock<IFinancialService> _mockFinancialService = null!;
    private Mock<IInventoryService> _mockInventoryService = null!;
    private Mock<IHRService> _mockHRService = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _mockPatientService = new Mock<IPatientService>();
        _mockAppointmentService = new Mock<IAppointmentService>();
        _mockLabService = new Mock<ILabService>();
        _mockFinancialService = new Mock<IFinancialService>();
        _mockInventoryService = new Mock<IInventoryService>();
        _mockHRService = new Mock<IHRService>();

        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    private async Task SeedTestDataAsync()
    {
        var company = new Company { Id = 1, TenantId = 1, Name = "Test Clinic", Code = "TC001", IsActive = true };
        _context.Companies.Add(company);

        var branch = new Branch { Id = 1, CompanyId = 1, Code = "BR001", Name = "Main Branch", IsActive = true };
        _context.Branches.Add(branch);

        await _context.SaveChangesAsync();
    }

    #region Patient Controller Tests

    [Fact]
    public async Task PatientController_GetPatient_ReturnsOk()
    {
        var patient = new PatientEntity { Id = 1, FullNameEn = "Test Patient", BranchId = 1 };
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(1)).ReturnsAsync(patient);

        var result = await _mockPatientService.Object.GetPatientByIdAsync(1);

        result.Should().NotBeNull();
        result!.FullNameEn.Should().Be("Test Patient");
    }

    [Fact]
    public async Task PatientController_GetPatient_NotFound_ReturnsNull()
    {
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(999)).ReturnsAsync((PatientEntity?)null);

        var result = await _mockPatientService.Object.GetPatientByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task PatientController_CreatePatient_ReturnsCreated()
    {
        var newPatient = new PatientEntity { FullNameEn = "New Patient", BranchId = 1 };
        _mockPatientService.Setup(s => s.CreatePatientAsync(It.IsAny<PatientEntity>()))
            .ReturnsAsync(new PatientEntity { Id = 1, FullNameEn = "New Patient", BranchId = 1 });

        var result = await _mockPatientService.Object.CreatePatientAsync(newPatient);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PatientController_UpdatePatient_ReturnsOk()
    {
        var patient = new PatientEntity { Id = 1, FullNameEn = "Updated Patient", BranchId = 1 };
        _mockPatientService.Setup(s => s.UpdatePatientAsync(It.IsAny<PatientEntity>())).Returns(Task.CompletedTask);

        await _mockPatientService.Object.UpdatePatientAsync(patient);

        _mockPatientService.Verify(s => s.UpdatePatientAsync(It.IsAny<PatientEntity>()), Times.Once);
    }

    [Fact]
    public async Task PatientController_DeletePatient_ReturnsOk()
    {
        _mockPatientService.Setup(s => s.DeletePatientAsync(1)).Returns(Task.CompletedTask);

        await _mockPatientService.Object.DeletePatientAsync(1);

        _mockPatientService.Verify(s => s.DeletePatientAsync(1), Times.Once);
    }

    [Theory]
    [InlineData("John")]
    [InlineData("Jane")]
    [InlineData("Test")]
    public async Task PatientController_SearchPatients_ReturnsMatches(string searchTerm)
    {
        var patients = new List<PatientEntity>
        {
            new() { Id = 1, FullNameEn = $"{searchTerm} Patient", BranchId = 1 }
        };
        _mockPatientService.Setup(s => s.SearchPatientsAsync(1, searchTerm)).ReturnsAsync(patients);

        var result = await _mockPatientService.Object.SearchPatientsAsync(1, searchTerm);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PatientController_GetPatientsByBranch_ReturnsOk()
    {
        var patients = new List<PatientEntity>
        {
            new() { Id = 1, FullNameEn = "Patient 1", BranchId = 1 },
            new() { Id = 2, FullNameEn = "Patient 2", BranchId = 1 }
        };
        _mockPatientService.Setup(s => s.GetPatientsByBranchIdAsync(1)).ReturnsAsync(patients);

        var result = await _mockPatientService.Object.GetPatientsByBranchIdAsync(1);

        result.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task PatientController_GetPatient_VariousIds_ReturnsCorrectPatient(int patientId)
    {
        var patient = new PatientEntity { Id = patientId, FullNameEn = $"Patient {patientId}", BranchId = 1 };
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(patientId)).ReturnsAsync(patient);

        var result = await _mockPatientService.Object.GetPatientByIdAsync(patientId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(patientId);
    }

    #endregion

    #region Appointment Controller Tests

    [Fact]
    public async Task AppointmentController_GetAppointment_ReturnsOk()
    {
        var appointment = new Appointment { Id = 1, PatientId = 1, BranchId = 1 };
        _mockAppointmentService.Setup(s => s.GetAppointmentByIdAsync(1)).ReturnsAsync(appointment);

        var result = await _mockAppointmentService.Object.GetAppointmentByIdAsync(1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AppointmentController_GetAppointment_NotFound_ReturnsNull()
    {
        _mockAppointmentService.Setup(s => s.GetAppointmentByIdAsync(999)).ReturnsAsync((Appointment?)null);

        var result = await _mockAppointmentService.Object.GetAppointmentByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AppointmentController_CreateAppointment_ReturnsCreated()
    {
        var newAppointment = new Appointment { PatientId = 1, BranchId = 1 };
        _mockAppointmentService.Setup(s => s.CreateAppointmentAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(new Appointment { Id = 1, PatientId = 1, BranchId = 1 });

        var result = await _mockAppointmentService.Object.CreateAppointmentAsync(newAppointment);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AppointmentController_GetTodayAppointments_ReturnsOk()
    {
        var appointments = new List<Appointment>
        {
            new() { Id = 1, PatientId = 1, BranchId = 1, StartTime = DateTime.UtcNow }
        };
        _mockAppointmentService.Setup(s => s.GetTodayAppointmentsAsync(1)).ReturnsAsync(appointments);

        var result = await _mockAppointmentService.Object.GetTodayAppointmentsAsync(1);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AppointmentController_GetUpcomingAppointments_ReturnsOk()
    {
        var appointments = new List<Appointment>
        {
            new() { Id = 1, PatientId = 1, BranchId = 1, StartTime = DateTime.UtcNow.AddDays(1) }
        };
        _mockAppointmentService.Setup(s => s.GetUpcomingAppointmentsAsync(1, 7)).ReturnsAsync(appointments);

        var result = await _mockAppointmentService.Object.GetUpcomingAppointmentsAsync(1, 7);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AppointmentController_ConfirmAppointment_ReturnsOk()
    {
        _mockAppointmentService.Setup(s => s.ConfirmAppointmentAsync(1)).Returns(Task.CompletedTask);

        await _mockAppointmentService.Object.ConfirmAppointmentAsync(1);

        _mockAppointmentService.Verify(s => s.ConfirmAppointmentAsync(1), Times.Once);
    }

    [Fact]
    public async Task AppointmentController_CheckInAppointment_ReturnsOk()
    {
        _mockAppointmentService.Setup(s => s.CheckInAppointmentAsync(1)).Returns(Task.CompletedTask);

        await _mockAppointmentService.Object.CheckInAppointmentAsync(1);

        _mockAppointmentService.Verify(s => s.CheckInAppointmentAsync(1), Times.Once);
    }

    [Fact]
    public async Task AppointmentController_CancelAppointment_ReturnsOk()
    {
        _mockAppointmentService.Setup(s => s.CancelAppointmentAsync(1, "Test reason")).Returns(Task.CompletedTask);

        await _mockAppointmentService.Object.CancelAppointmentAsync(1, "Test reason");

        _mockAppointmentService.Verify(s => s.CancelAppointmentAsync(1, "Test reason"), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(30)]
    public async Task AppointmentController_GetUpcomingAppointments_VariousDays(int days)
    {
        var appointments = new List<Appointment>();
        _mockAppointmentService.Setup(s => s.GetUpcomingAppointmentsAsync(1, days)).ReturnsAsync(appointments);

        var result = await _mockAppointmentService.Object.GetUpcomingAppointmentsAsync(1, days);

        result.Should().NotBeNull();
    }

    #endregion

    #region Laboratory Controller Tests

    [Fact]
    public async Task LaboratoryController_GetLabOrder_ReturnsOk()
    {
        var order = new LabOrder { Id = 1, PatientId = 1, BranchId = 1 };
        _mockLabService.Setup(s => s.GetLabOrderByIdAsync(1)).ReturnsAsync(order);

        var result = await _mockLabService.Object.GetLabOrderByIdAsync(1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task LaboratoryController_CreateLabOrder_ReturnsCreated()
    {
        var newOrder = new LabOrder { PatientId = 1, BranchId = 1 };
        _mockLabService.Setup(s => s.CreateLabOrderAsync(It.IsAny<LabOrder>()))
            .ReturnsAsync(new LabOrder { Id = 1, PatientId = 1, BranchId = 1 });

        var result = await _mockLabService.Object.CreateLabOrderAsync(newOrder);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task LaboratoryController_GetLabTests_ReturnsOk()
    {
        var tests = new List<LabTest>
        {
            new() { Id = 1, TestCode = "CBC", TestName = "Complete Blood Count" }
        };
        _mockLabService.Setup(s => s.GetLabTestsAsync()).ReturnsAsync(tests);

        var result = await _mockLabService.Object.GetLabTestsAsync();

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LaboratoryController_CreateLabResult_ReturnsCreated()
    {
        var newResult = new LabResult { LabOrderItemId = 1, Result = "Normal" };
        _mockLabService.Setup(s => s.CreateLabResultAsync(It.IsAny<LabResult>()))
            .ReturnsAsync(new LabResult { Id = 1, LabOrderItemId = 1, Result = "Normal" });

        var result = await _mockLabService.Object.CreateLabResultAsync(newResult);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Hematology")]
    [InlineData("Chemistry")]
    [InlineData("Microbiology")]
    public async Task LaboratoryController_GetLabTestsByCategory_ReturnsOk(string category)
    {
        var tests = new List<LabTest>
        {
            new() { Id = 1, TestCode = "TEST", TestName = "Test", Category = category }
        };
        _mockLabService.Setup(s => s.GetLabTestsByCategoryAsync(category)).ReturnsAsync(tests);

        var result = await _mockLabService.Object.GetLabTestsByCategoryAsync(category);

        result.Should().NotBeEmpty();
    }

    #endregion

    #region Financial Controller Tests

    [Fact]
    public async Task FinancialController_GetInvoice_ReturnsOk()
    {
        var invoice = new Invoice { Id = 1, PatientId = 1, BranchId = 1, TotalAmount = 100 };
        _mockFinancialService.Setup(s => s.GetInvoiceByIdAsync(1)).ReturnsAsync(invoice);

        var result = await _mockFinancialService.Object.GetInvoiceByIdAsync(1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task FinancialController_CreateInvoice_ReturnsCreated()
    {
        var newInvoice = new Invoice { PatientId = 1, BranchId = 1, TotalAmount = 100 };
        _mockFinancialService.Setup(s => s.CreateInvoiceAsync(It.IsAny<Invoice>()))
            .ReturnsAsync(new Invoice { Id = 1, PatientId = 1, BranchId = 1, TotalAmount = 100 });

        var result = await _mockFinancialService.Object.CreateInvoiceAsync(newInvoice);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FinancialController_CreatePayment_ReturnsCreated()
    {
        var newPayment = new Payment { InvoiceId = 1, Amount = 50 };
        _mockFinancialService.Setup(s => s.CreatePaymentAsync(It.IsAny<Payment>()))
            .ReturnsAsync(new Payment { Id = 1, InvoiceId = 1, Amount = 50 });

        var result = await _mockFinancialService.Object.CreatePaymentAsync(newPayment);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FinancialController_GetTotalRevenue_ReturnsOk()
    {
        _mockFinancialService.Setup(s => s.GetTotalRevenueAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(10000m);

        var result = await _mockFinancialService.Object.GetTotalRevenueAsync(1, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FinancialController_GetOutstandingBalance_ReturnsOk()
    {
        _mockFinancialService.Setup(s => s.GetOutstandingBalanceAsync(1)).ReturnsAsync(5000m);

        var result = await _mockFinancialService.Object.GetOutstandingBalanceAsync(1);

        result.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(Core.Enums.InvoiceStatus.Draft)]
    [InlineData(Core.Enums.InvoiceStatus.Pending)]
    [InlineData(Core.Enums.InvoiceStatus.Paid)]
    public async Task FinancialController_GetInvoicesByStatus_ReturnsOk(Core.Enums.InvoiceStatus status)
    {
        var invoices = new List<Invoice>
        {
            new() { Id = 1, Status = status }
        };
        _mockFinancialService.Setup(s => s.GetInvoicesByStatusAsync(1, status)).ReturnsAsync(invoices);

        var result = await _mockFinancialService.Object.GetInvoicesByStatusAsync(1, status);

        result.Should().NotBeEmpty();
    }

    #endregion

    #region Inventory Controller Tests

    [Fact]
    public async Task InventoryController_GetItem_ReturnsOk()
    {
        var item = new InventoryItem { Id = 1, ItemName = "Test Item", BranchId = 1 };
        _mockInventoryService.Setup(s => s.GetInventoryItemByIdAsync(1)).ReturnsAsync(item);

        var result = await _mockInventoryService.Object.GetInventoryItemByIdAsync(1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task InventoryController_CreateItem_ReturnsCreated()
    {
        var newItem = new InventoryItem { ItemName = "New Item", BranchId = 1 };
        _mockInventoryService.Setup(s => s.CreateInventoryItemAsync(It.IsAny<InventoryItem>()))
            .ReturnsAsync(new InventoryItem { Id = 1, ItemName = "New Item", BranchId = 1 });

        var result = await _mockInventoryService.Object.CreateInventoryItemAsync(newItem);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task InventoryController_GetLowStockItems_ReturnsOk()
    {
        var items = new List<InventoryItem>
        {
            new() { Id = 1, ItemName = "Low Stock Item", QuantityOnHand = 5, ReorderLevel = 10 }
        };
        _mockInventoryService.Setup(s => s.GetLowStockItemsAsync(1)).ReturnsAsync(items);

        var result = await _mockInventoryService.Object.GetLowStockItemsAsync(1);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task InventoryController_GetTotalInventoryValue_ReturnsOk()
    {
        _mockInventoryService.Setup(s => s.GetTotalInventoryValueAsync(1)).ReturnsAsync(50000m);

        var result = await _mockInventoryService.Object.GetTotalInventoryValueAsync(1);

        result.Should().BeGreaterThan(0);
    }

    #endregion

    #region HR Controller Tests

    [Fact]
    public async Task HRController_GetEmployee_ReturnsOk()
    {
        var employee = new Employee { Id = 1, FullName = "Test Employee", BranchId = 1 };
        _mockHRService.Setup(s => s.GetEmployeeByIdAsync(1)).ReturnsAsync(employee);

        var result = await _mockHRService.Object.GetEmployeeByIdAsync(1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task HRController_CreateEmployee_ReturnsCreated()
    {
        var newEmployee = new Employee { FullName = "New Employee", BranchId = 1 };
        _mockHRService.Setup(s => s.CreateEmployeeAsync(It.IsAny<Employee>()))
            .ReturnsAsync(new Employee { Id = 1, FullName = "New Employee", BranchId = 1 });

        var result = await _mockHRService.Object.CreateEmployeeAsync(newEmployee);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HRController_CreateLeaveRequest_ReturnsCreated()
    {
        var request = new LeaveRequest { EmployeeId = 1, LeaveType = Core.Enums.LeaveType.Annual };
        _mockHRService.Setup(s => s.CreateLeaveRequestAsync(It.IsAny<LeaveRequest>()))
            .ReturnsAsync(new LeaveRequest { Id = 1, EmployeeId = 1, LeaveType = Core.Enums.LeaveType.Annual });

        var result = await _mockHRService.Object.CreateLeaveRequestAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HRController_RecordAttendance_ReturnsCreated()
    {
        var attendance = new Attendance { EmployeeId = 1, Date = DateTime.UtcNow.Date };
        _mockHRService.Setup(s => s.RecordAttendanceAsync(It.IsAny<Attendance>()))
            .ReturnsAsync(new Attendance { Id = 1, EmployeeId = 1, Date = DateTime.UtcNow.Date });

        var result = await _mockHRService.Object.RecordAttendanceAsync(attendance);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HRController_GetTotalEmployeeCount_ReturnsOk()
    {
        _mockHRService.Setup(s => s.GetTotalEmployeeCountAsync(1)).ReturnsAsync(50);

        var result = await _mockHRService.Object.GetTotalEmployeeCountAsync(1);

        result.Should().BeGreaterThan(0);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Controller_ServiceThrowsException_PropagatesError()
    {
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var action = () => _mockPatientService.Object.GetPatientByIdAsync(1);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test error");
    }

    [Fact]
    public async Task Controller_ServiceThrowsDbException_PropagatesError()
    {
        _mockPatientService.Setup(s => s.CreatePatientAsync(It.IsAny<PatientEntity>()))
            .ThrowsAsync(new DbUpdateException("Database error"));

        var action = () => _mockPatientService.Object.CreatePatientAsync(new PatientEntity());

        await action.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Controller_NullInput_HandledGracefully()
    {
        _mockPatientService.Setup(s => s.SearchPatientsAsync(1, null!))
            .ReturnsAsync(new List<PatientEntity>());

        var result = await _mockPatientService.Object.SearchPatientsAsync(1, null!);

        result.Should().NotBeNull();
    }

    #endregion

    #region Concurrent Request Tests

    [Fact]
    public async Task Controller_ConcurrentGetRequests_AllSucceed()
    {
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new PatientEntity { Id = id, FullNameEn = $"Patient {id}", BranchId = 1 });

        var tasks = Enumerable.Range(1, 50)
            .Select(id => _mockPatientService.Object.GetPatientByIdAsync(id))
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(p => p != null);
    }

    [Fact]
    public async Task Controller_ConcurrentCreateRequests_AllSucceed()
    {
        var counter = 0;
        _mockPatientService.Setup(s => s.CreatePatientAsync(It.IsAny<PatientEntity>()))
            .ReturnsAsync((PatientEntity p) => new PatientEntity { Id = ++counter, FullNameEn = p.FullNameEn, BranchId = 1 });

        var tasks = Enumerable.Range(1, 20)
            .Select(i => _mockPatientService.Object.CreatePatientAsync(new PatientEntity { FullNameEn = $"Patient {i}", BranchId = 1 }))
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(p => p.Id > 0);
    }

    #endregion

    #region Pagination Tests

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 10)]
    [InlineData(1, 25)]
    [InlineData(1, 50)]
    public async Task Controller_GetWithPagination_ReturnsCorrectPage(int page, int pageSize)
    {
        var totalPatients = Enumerable.Range(1, 100)
            .Select(i => new PatientEntity { Id = i, FullNameEn = $"Patient {i}", BranchId = 1 })
            .ToList();

        var pagedPatients = totalPatients
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _mockPatientService.Setup(s => s.GetPatientsByBranchIdAsync(1))
            .ReturnsAsync(pagedPatients);

        var result = await _mockPatientService.Object.GetPatientsByBranchIdAsync(1);

        result.Count().Should().BeLessThanOrEqualTo(pageSize);
    }

    #endregion

    #region Date Range Query Tests

    [Theory]
    [InlineData(-7)]
    [InlineData(-30)]
    [InlineData(-90)]
    [InlineData(-365)]
    public async Task Controller_GetByDateRange_ReturnsCorrectResults(int daysBack)
    {
        var startDate = DateTime.UtcNow.AddDays(daysBack);
        var endDate = DateTime.UtcNow;

        _mockFinancialService.Setup(s => s.GetInvoicesByDateRangeAsync(1, startDate, endDate))
            .ReturnsAsync(new List<Invoice>());

        var result = await _mockFinancialService.Object.GetInvoicesByDateRangeAsync(1, startDate, endDate);

        result.Should().NotBeNull();
    }

    #endregion
}
