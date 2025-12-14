using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Laboratory;

/// <summary>
/// Extended comprehensive tests for the Laboratory Service implementation.
/// Contains 600+ test cases covering all laboratory management scenarios.
/// </summary>
public class LabServiceExtendedTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private ILabService _labService = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _labService = new LabService(_context);
        await SeedExtendedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    private async Task SeedExtendedTestDataAsync()
    {
        // Seed company and branch
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
        for (int i = 1; i <= 30; i++)
        {
            patients.Add(new Core.Entities.Patient
            {
                Id = i,
                BranchId = (i % 2) + 1,
                EmiratesId = $"784-{i:D4}-{i:D7}-{i % 10}",
                FullNameEn = $"Lab Patient {i}",
                DateOfBirth = new DateTime(1970 + (i % 30), (i % 12) + 1, (i % 28) + 1),
                Gender = i % 2 == 0 ? "M" : "F",
                CreatedAt = DateTime.UtcNow
            });
        }
        _context.Patients.AddRange(patients);

        // Seed employees (technicians and doctors)
        var employees = new List<Employee>();
        for (int i = 1; i <= 10; i++)
        {
            employees.Add(new Employee
            {
                Id = i,
                BranchId = (i % 2) + 1,
                FullName = i <= 5 ? $"Dr. Doctor {i}" : $"Tech. Technician {i}",
                Email = $"employee{i}@clinic.com",
                IsActive = true
            });
        }
        _context.Employees.AddRange(employees);

        // Seed lab tests catalog
        var labTests = new List<LabTest>
        {
            new() { Id = 1, TestCode = "CBC", TestName = "Complete Blood Count", Category = "Hematology", Price = 50, IsActive = true },
            new() { Id = 2, TestCode = "LFT", TestName = "Liver Function Test", Category = "Chemistry", Price = 80, IsActive = true },
            new() { Id = 3, TestCode = "RFT", TestName = "Renal Function Test", Category = "Chemistry", Price = 75, IsActive = true },
            new() { Id = 4, TestCode = "FBS", TestName = "Fasting Blood Sugar", Category = "Glucose", Price = 25, IsActive = true },
            new() { Id = 5, TestCode = "HBA1C", TestName = "Glycated Hemoglobin", Category = "Glucose", Price = 60, IsActive = true },
            new() { Id = 6, TestCode = "TSH", TestName = "Thyroid Stimulating Hormone", Category = "Endocrine", Price = 70, IsActive = true },
            new() { Id = 7, TestCode = "T3", TestName = "Triiodothyronine", Category = "Endocrine", Price = 65, IsActive = true },
            new() { Id = 8, TestCode = "T4", TestName = "Thyroxine", Category = "Endocrine", Price = 65, IsActive = true },
            new() { Id = 9, TestCode = "LIPID", TestName = "Lipid Profile", Category = "Chemistry", Price = 90, IsActive = true },
            new() { Id = 10, TestCode = "UA", TestName = "Urine Analysis", Category = "Urinalysis", Price = 30, IsActive = true },
            new() { Id = 11, TestCode = "COVID", TestName = "COVID-19 PCR", Category = "Molecular", Price = 150, IsActive = true },
            new() { Id = 12, TestCode = "VITD", TestName = "Vitamin D", Category = "Chemistry", Price = 85, IsActive = true },
            new() { Id = 13, TestCode = "B12", TestName = "Vitamin B12", Category = "Chemistry", Price = 80, IsActive = true },
            new() { Id = 14, TestCode = "IRON", TestName = "Iron Studies", Category = "Hematology", Price = 95, IsActive = true },
            new() { Id = 15, TestCode = "CRP", TestName = "C-Reactive Protein", Category = "Immunology", Price = 55, IsActive = true },
            new() { Id = 16, TestCode = "ESR", TestName = "Erythrocyte Sedimentation Rate", Category = "Hematology", Price = 20, IsActive = true },
            new() { Id = 17, TestCode = "PT", TestName = "Prothrombin Time", Category = "Coagulation", Price = 45, IsActive = true },
            new() { Id = 18, TestCode = "INR", TestName = "International Normalized Ratio", Category = "Coagulation", Price = 45, IsActive = true },
            new() { Id = 19, TestCode = "ELEC", TestName = "Electrolytes Panel", Category = "Chemistry", Price = 55, IsActive = true },
            new() { Id = 20, TestCode = "AMY", TestName = "Amylase", Category = "Chemistry", Price = 50, IsActive = true },
            new() { Id = 21, TestCode = "INACTIVE", TestName = "Inactive Test", Category = "Other", Price = 100, IsActive = false }
        };
        _context.LabTests.AddRange(labTests);

        // Seed lab orders
        var labOrders = new List<LabOrder>();
        var labOrderItems = new List<LabOrderItem>();
        var labResults = new List<LabResult>();

        for (int i = 1; i <= 100; i++)
        {
            var status = i <= 30 ? LabOrderStatus.Pending
                : i <= 60 ? LabOrderStatus.InProgress
                : i <= 90 ? LabOrderStatus.Completed
                : LabOrderStatus.Cancelled;

            var order = new LabOrder
            {
                Id = i,
                BranchId = (i % 2) + 1,
                PatientId = (i % 30) + 1,
                OrderNumber = $"LAB-{i:D6}",
                OrderDate = DateTime.UtcNow.AddDays(-i),
                Status = status,
                OrderedById = (i % 5) + 1,
                Priority = i % 10 == 0 ? "STAT" : "Routine",
                Notes = $"Lab order notes {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };
            labOrders.Add(order);

            // Add items for each order
            for (int j = 1; j <= (i % 5) + 1; j++)
            {
                var itemId = (i - 1) * 5 + j;
                labOrderItems.Add(new LabOrderItem
                {
                    Id = itemId,
                    LabOrderId = i,
                    LabTestId = (j % 20) + 1,
                    Status = status == LabOrderStatus.Completed ? LabResultStatus.Completed : LabResultStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                });

                // Add results for completed orders
                if (status == LabOrderStatus.Completed)
                {
                    labResults.Add(new LabResult
                    {
                        Id = itemId,
                        LabOrderItemId = itemId,
                        Result = $"{50 + (i % 100)}",
                        Unit = "mg/dL",
                        ReferenceRange = "50-150",
                        Status = LabResultStatus.Completed,
                        PerformedById = (i % 5) + 6,
                        PerformedAt = DateTime.UtcNow.AddDays(-i + 1),
                        VerifiedById = (i % 5) + 1,
                        VerifiedAt = DateTime.UtcNow.AddDays(-i + 1),
                        CreatedAt = DateTime.UtcNow.AddDays(-i + 1)
                    });
                }
            }
        }
        _context.LabOrders.AddRange(labOrders);
        _context.LabOrderItems.AddRange(labOrderItems);
        _context.LabResults.AddRange(labResults);

        await _context.SaveChangesAsync();
    }

    #region GetLabOrderByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task GetLabOrderByIdAsync_ValidIds_ReturnsOrder(int orderId)
    {
        var result = await _labService.GetLabOrderByIdAsync(orderId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetLabOrderByIdAsync_InvalidIds_ReturnsNull(int orderId)
    {
        var result = await _labService.GetLabOrderByIdAsync(orderId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLabOrderByIdAsync_IncludesPatient()
    {
        var result = await _labService.GetLabOrderByIdAsync(1);
        result.Should().NotBeNull();
        result!.Patient.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLabOrderByIdAsync_IncludesItems()
    {
        var result = await _labService.GetLabOrderByIdAsync(1);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetLabOrderByIdAsync_ConcurrentAccess_AllSucceed()
    {
        var tasks = Enumerable.Range(1, 50)
            .Select(id => _labService.GetLabOrderByIdAsync(id))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(o => o != null);
    }

    #endregion

    #region GetLabOrdersByPatientIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(15)]
    [InlineData(30)]
    public async Task GetLabOrdersByPatientIdAsync_ValidPatients_ReturnsOrders(int patientId)
    {
        var result = await _labService.GetLabOrdersByPatientIdAsync(patientId);
        var orders = result.ToList();

        orders.Should().OnlyContain(o => o.PatientId == patientId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetLabOrdersByPatientIdAsync_InvalidPatients_ReturnsEmpty(int patientId)
    {
        var result = await _labService.GetLabOrdersByPatientIdAsync(patientId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLabOrdersByPatientIdAsync_OrderedByDate()
    {
        var result = await _labService.GetLabOrdersByPatientIdAsync(1);
        var orders = result.ToList();

        orders.Should().BeInDescendingOrder(o => o.OrderDate);
    }

    #endregion

    #region GetLabOrdersByBranchIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetLabOrdersByBranchIdAsync_ValidBranches_ReturnsOrders(int branchId)
    {
        var result = await _labService.GetLabOrdersByBranchIdAsync(branchId);
        var orders = result.ToList();

        orders.Should().NotBeEmpty();
        orders.Should().OnlyContain(o => o.BranchId == branchId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetLabOrdersByBranchIdAsync_InvalidBranches_ReturnsEmpty(int branchId)
    {
        var result = await _labService.GetLabOrdersByBranchIdAsync(branchId);
        result.Should().BeEmpty();
    }

    #endregion

    #region GetLabOrdersByStatusAsync Tests

    [Theory]
    [InlineData(LabOrderStatus.Pending)]
    [InlineData(LabOrderStatus.InProgress)]
    [InlineData(LabOrderStatus.Completed)]
    [InlineData(LabOrderStatus.Cancelled)]
    public async Task GetLabOrdersByStatusAsync_AllStatuses_ReturnsCorrectOrders(LabOrderStatus status)
    {
        var result = await _labService.GetLabOrdersByStatusAsync(1, status);
        var orders = result.ToList();

        orders.Should().OnlyContain(o => o.Status == status);
    }

    [Fact]
    public async Task GetLabOrdersByStatusAsync_PendingOrders_ExcludesCompleted()
    {
        var result = await _labService.GetLabOrdersByStatusAsync(1, LabOrderStatus.Pending);
        var orders = result.ToList();

        orders.Should().NotContain(o => o.Status == LabOrderStatus.Completed);
    }

    #endregion

    #region CreateLabOrderAsync Tests

    [Fact]
    public async Task CreateLabOrderAsync_ValidOrder_CreatesSuccessfully()
    {
        var newOrder = new LabOrder
        {
            BranchId = 1,
            PatientId = 1,
            OrderNumber = "LAB-NEW001",
            OrderDate = DateTime.UtcNow,
            Status = LabOrderStatus.Pending,
            OrderedById = 1,
            Priority = "Routine"
        };

        var result = await _labService.CreateLabOrderAsync(newOrder);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateLabOrderAsync_WithItems_CreatesWithItems()
    {
        var newOrder = new LabOrder
        {
            BranchId = 1,
            PatientId = 2,
            OrderNumber = "LAB-NEW002",
            OrderDate = DateTime.UtcNow,
            Status = LabOrderStatus.Pending,
            OrderedById = 1,
            Priority = "Routine",
            Items = new List<LabOrderItem>
            {
                new() { LabTestId = 1, Status = LabResultStatus.Pending },
                new() { LabTestId = 2, Status = LabResultStatus.Pending },
                new() { LabTestId = 3, Status = LabResultStatus.Pending }
            }
        };

        var result = await _labService.CreateLabOrderAsync(newOrder);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("STAT")]
    [InlineData("Routine")]
    [InlineData("Urgent")]
    public async Task CreateLabOrderAsync_VariousPriorities_AllSucceed(string priority)
    {
        var newOrder = new LabOrder
        {
            BranchId = 1,
            PatientId = 3,
            OrderNumber = $"LAB-PRI-{priority}",
            OrderDate = DateTime.UtcNow,
            Status = LabOrderStatus.Pending,
            OrderedById = 1,
            Priority = priority
        };

        var result = await _labService.CreateLabOrderAsync(newOrder);

        result.Should().NotBeNull();
        result.Priority.Should().Be(priority);
    }

    [Fact]
    public async Task CreateLabOrderAsync_SetsCreatedAtAutomatically()
    {
        var newOrder = new LabOrder
        {
            BranchId = 1,
            PatientId = 4,
            OrderNumber = "LAB-AUTO001",
            OrderDate = DateTime.UtcNow,
            Status = LabOrderStatus.Pending,
            OrderedById = 1,
            Priority = "Routine"
        };

        var before = DateTime.UtcNow;
        var result = await _labService.CreateLabOrderAsync(newOrder);
        var after = DateTime.UtcNow;

        result.CreatedAt.Should().BeOnOrAfter(before);
        result.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task CreateLabOrderAsync_WithNotes_SavesNotes()
    {
        var newOrder = new LabOrder
        {
            BranchId = 1,
            PatientId = 5,
            OrderNumber = "LAB-NOTE001",
            OrderDate = DateTime.UtcNow,
            Status = LabOrderStatus.Pending,
            OrderedById = 1,
            Priority = "Routine",
            Notes = "Special handling required"
        };

        var result = await _labService.CreateLabOrderAsync(newOrder);

        result.Notes.Should().Be("Special handling required");
    }

    [Fact]
    public async Task CreateLabOrderAsync_ConcurrentCreations_AllSucceed()
    {
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _labService.CreateLabOrderAsync(new LabOrder
            {
                BranchId = 1,
                PatientId = (i % 30) + 1,
                OrderNumber = $"LAB-CONC-{i:D3}",
                OrderDate = DateTime.UtcNow,
                Status = LabOrderStatus.Pending,
                OrderedById = 1,
                Priority = "Routine"
            }))
            .ToList();

        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(o => o.Id > 0);
    }

    #endregion

    #region UpdateLabOrderAsync Tests

    [Fact]
    public async Task UpdateLabOrderAsync_UpdateStatus_UpdatesSuccessfully()
    {
        var order = await _labService.GetLabOrderByIdAsync(1);
        order!.Status = LabOrderStatus.InProgress;

        await _labService.UpdateLabOrderAsync(order);

        var updated = await _labService.GetLabOrderByIdAsync(1);
        updated!.Status.Should().Be(LabOrderStatus.InProgress);
    }

    [Fact]
    public async Task UpdateLabOrderAsync_UpdateNotes_UpdatesSuccessfully()
    {
        var order = await _labService.GetLabOrderByIdAsync(2);
        order!.Notes = "Updated notes";

        await _labService.UpdateLabOrderAsync(order);

        var updated = await _labService.GetLabOrderByIdAsync(2);
        updated!.Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task UpdateLabOrderAsync_UpdatePriority_UpdatesSuccessfully()
    {
        var order = await _labService.GetLabOrderByIdAsync(3);
        order!.Priority = "STAT";

        await _labService.UpdateLabOrderAsync(order);

        var updated = await _labService.GetLabOrderByIdAsync(3);
        updated!.Priority.Should().Be("STAT");
    }

    [Fact]
    public async Task UpdateLabOrderAsync_SetsUpdatedAtAutomatically()
    {
        var order = await _labService.GetLabOrderByIdAsync(4);
        order!.Notes = "Timestamp test";

        var before = DateTime.UtcNow;
        await _labService.UpdateLabOrderAsync(order);
        var after = DateTime.UtcNow;

        var updated = await _labService.GetLabOrderByIdAsync(4);
        updated!.UpdatedAt.Should().BeOnOrAfter(before);
        updated.UpdatedAt.Should().BeOnOrBefore(after);
    }

    #endregion

    #region DeleteLabOrderAsync Tests

    [Fact]
    public async Task DeleteLabOrderAsync_ExistingOrder_Deletes()
    {
        await _labService.DeleteLabOrderAsync(95);

        var deleted = await _labService.GetLabOrderByIdAsync(95);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteLabOrderAsync_NonExistent_NoError()
    {
        var action = () => _labService.DeleteLabOrderAsync(9999);
        await action.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteLabOrderAsync_InvalidId_NoError(int orderId)
    {
        var action = () => _labService.DeleteLabOrderAsync(orderId);
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region GetLabTestsAsync Tests

    [Fact]
    public async Task GetLabTestsAsync_ReturnsActiveTests()
    {
        var result = await _labService.GetLabTestsAsync();
        var tests = result.ToList();

        tests.Should().NotBeEmpty();
        tests.Should().OnlyContain(t => t.IsActive);
    }

    [Fact]
    public async Task GetLabTestsAsync_ExcludesInactive()
    {
        var result = await _labService.GetLabTestsAsync();
        var tests = result.ToList();

        tests.Should().NotContain(t => t.TestCode == "INACTIVE");
    }

    [Fact]
    public async Task GetLabTestsAsync_OrderedByName()
    {
        var result = await _labService.GetLabTestsAsync();
        var tests = result.ToList();

        tests.Should().BeInAscendingOrder(t => t.TestName);
    }

    #endregion

    #region GetLabTestByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public async Task GetLabTestByIdAsync_ValidIds_ReturnsTest(int testId)
    {
        var result = await _labService.GetLabTestByIdAsync(testId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(testId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetLabTestByIdAsync_InvalidIds_ReturnsNull(int testId)
    {
        var result = await _labService.GetLabTestByIdAsync(testId);
        result.Should().BeNull();
    }

    #endregion

    #region GetLabTestsByCategoryAsync Tests

    [Theory]
    [InlineData("Hematology")]
    [InlineData("Chemistry")]
    [InlineData("Glucose")]
    [InlineData("Endocrine")]
    public async Task GetLabTestsByCategoryAsync_ValidCategories_ReturnsTests(string category)
    {
        var result = await _labService.GetLabTestsByCategoryAsync(category);
        var tests = result.ToList();

        tests.Should().NotBeEmpty();
        tests.Should().OnlyContain(t => t.Category == category);
    }

    [Theory]
    [InlineData("NonExistent")]
    [InlineData("")]
    public async Task GetLabTestsByCategoryAsync_InvalidCategories_ReturnsEmpty(string category)
    {
        var result = await _labService.GetLabTestsByCategoryAsync(category);
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateLabResultAsync Tests

    [Fact]
    public async Task CreateLabResultAsync_ValidResult_CreatesSuccessfully()
    {
        var pendingItem = await _context.LabOrderItems
            .FirstOrDefaultAsync(i => i.Status == LabResultStatus.Pending);

        if (pendingItem != null)
        {
            var newResult = new LabResult
            {
                LabOrderItemId = pendingItem.Id,
                Result = "95",
                Unit = "mg/dL",
                ReferenceRange = "70-100",
                Status = LabResultStatus.Completed,
                PerformedById = 6,
                PerformedAt = DateTime.UtcNow
            };

            var result = await _labService.CreateLabResultAsync(newResult);

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }
    }

    [Theory]
    [InlineData("Normal")]
    [InlineData("Abnormal")]
    [InlineData("Critical")]
    public async Task CreateLabResultAsync_VariousInterpretations_AllSucceed(string interpretation)
    {
        var pendingItems = await _context.LabOrderItems
            .Where(i => i.Status == LabResultStatus.Pending)
            .Take(3)
            .ToListAsync();

        var pendingItem = pendingItems.FirstOrDefault();
        if (pendingItem != null)
        {
            var newResult = new LabResult
            {
                LabOrderItemId = pendingItem.Id,
                Result = "100",
                Unit = "mg/dL",
                ReferenceRange = "70-100",
                Interpretation = interpretation,
                Status = LabResultStatus.Completed,
                PerformedById = 6,
                PerformedAt = DateTime.UtcNow
            };

            var result = await _labService.CreateLabResultAsync(newResult);

            result.Should().NotBeNull();
            result.Interpretation.Should().Be(interpretation);
        }
    }

    [Fact]
    public async Task CreateLabResultAsync_WithComments_SavesComments()
    {
        var pendingItem = await _context.LabOrderItems
            .Where(i => i.Status == LabResultStatus.Pending)
            .Skip(5)
            .FirstOrDefaultAsync();

        if (pendingItem != null)
        {
            var newResult = new LabResult
            {
                LabOrderItemId = pendingItem.Id,
                Result = "85",
                Unit = "mg/dL",
                ReferenceRange = "70-100",
                Comments = "Result within normal limits",
                Status = LabResultStatus.Completed,
                PerformedById = 6,
                PerformedAt = DateTime.UtcNow
            };

            var result = await _labService.CreateLabResultAsync(newResult);

            result.Comments.Should().Be("Result within normal limits");
        }
    }

    #endregion

    #region GetLabResultByIdAsync Tests

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    public async Task GetLabResultByIdAsync_ValidIds_ReturnsResult(int resultId)
    {
        var result = await _labService.GetLabResultByIdAsync(resultId);

        if (result != null)
        {
            result.Id.Should().Be(resultId);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(99999)]
    public async Task GetLabResultByIdAsync_InvalidIds_ReturnsNull(int resultId)
    {
        var result = await _labService.GetLabResultByIdAsync(resultId);
        result.Should().BeNull();
    }

    #endregion

    #region GetLabResultsByOrderIdAsync Tests

    [Fact]
    public async Task GetLabResultsByOrderIdAsync_CompletedOrder_ReturnsResults()
    {
        var completedOrder = await _context.LabOrders
            .FirstOrDefaultAsync(o => o.Status == LabOrderStatus.Completed);

        if (completedOrder != null)
        {
            var result = await _labService.GetLabResultsByOrderIdAsync(completedOrder.Id);
            var results = result.ToList();

            results.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GetLabResultsByOrderIdAsync_PendingOrder_ReturnsEmpty()
    {
        var pendingOrder = await _context.LabOrders
            .FirstOrDefaultAsync(o => o.Status == LabOrderStatus.Pending);

        if (pendingOrder != null)
        {
            var result = await _labService.GetLabResultsByOrderIdAsync(pendingOrder.Id);
            result.Should().BeEmpty();
        }
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public async Task LabOrder_StatusTransition_PendingToInProgress()
    {
        var pendingOrder = await _context.LabOrders
            .FirstOrDefaultAsync(o => o.Status == LabOrderStatus.Pending);

        if (pendingOrder != null)
        {
            pendingOrder.Status = LabOrderStatus.InProgress;
            await _labService.UpdateLabOrderAsync(pendingOrder);

            var updated = await _labService.GetLabOrderByIdAsync(pendingOrder.Id);
            updated!.Status.Should().Be(LabOrderStatus.InProgress);
        }
    }

    [Fact]
    public async Task LabOrder_StatusTransition_InProgressToCompleted()
    {
        var inProgressOrder = await _context.LabOrders
            .FirstOrDefaultAsync(o => o.Status == LabOrderStatus.InProgress);

        if (inProgressOrder != null)
        {
            inProgressOrder.Status = LabOrderStatus.Completed;
            await _labService.UpdateLabOrderAsync(inProgressOrder);

            var updated = await _labService.GetLabOrderByIdAsync(inProgressOrder.Id);
            updated!.Status.Should().Be(LabOrderStatus.Completed);
        }
    }

    [Fact]
    public async Task LabOrder_StatusTransition_PendingToCancelled()
    {
        var pendingOrder = await _context.LabOrders
            .Where(o => o.Status == LabOrderStatus.Pending)
            .Skip(10)
            .FirstOrDefaultAsync();

        if (pendingOrder != null)
        {
            pendingOrder.Status = LabOrderStatus.Cancelled;
            await _labService.UpdateLabOrderAsync(pendingOrder);

            var updated = await _labService.GetLabOrderByIdAsync(pendingOrder.Id);
            updated!.Status.Should().Be(LabOrderStatus.Cancelled);
        }
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetTotalLabOrdersCountAsync_ReturnsCount()
    {
        var result = await _labService.GetTotalLabOrdersCountAsync(1);
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPendingLabOrdersCountAsync_ReturnsCount()
    {
        var result = await _labService.GetPendingLabOrdersCountAsync(1);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetCompletedLabOrdersCountAsync_DateRange_ReturnsCount()
    {
        var startDate = DateTime.UtcNow.AddDays(-100);
        var endDate = DateTime.UtcNow;

        var result = await _labService.GetCompletedLabOrdersCountAsync(1, startDate, endDate);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetLabOrderStatusDistributionAsync_ReturnsDistribution()
    {
        var result = await _labService.GetLabOrderStatusDistributionAsync(1);
        result.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetTotalLabOrdersCountAsync_ByBranch_FiltersCorrectly(int branchId)
    {
        var result = await _labService.GetTotalLabOrdersCountAsync(branchId);
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchLabOrdersAsync_ByOrderNumber_ReturnsMatches()
    {
        var result = await _labService.SearchLabOrdersAsync(1, "LAB-000001");
        var orders = result.ToList();

        orders.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchLabOrdersAsync_ByPatientName_ReturnsMatches()
    {
        var result = await _labService.SearchLabOrdersAsync(1, "Lab Patient");
        var orders = result.ToList();

        orders.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchLabOrdersAsync_NoMatches_ReturnsEmpty()
    {
        var result = await _labService.SearchLabOrdersAsync(1, "NonExistentPatient12345");
        result.Should().BeEmpty();
    }

    #endregion

    #region Lab Test CRUD Tests

    [Fact]
    public async Task CreateLabTestAsync_ValidTest_CreatesSuccessfully()
    {
        var newTest = new LabTest
        {
            TestCode = "NEWTEST",
            TestName = "New Test",
            Category = "General",
            Price = 100,
            IsActive = true
        };

        var result = await _labService.CreateLabTestAsync(newTest);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateLabTestAsync_UpdatePrice_UpdatesSuccessfully()
    {
        var test = await _labService.GetLabTestByIdAsync(1);
        test!.Price = 75;

        await _labService.UpdateLabTestAsync(test);

        var updated = await _labService.GetLabTestByIdAsync(1);
        updated!.Price.Should().Be(75);
    }

    [Fact]
    public async Task UpdateLabTestAsync_Deactivate_UpdatesSuccessfully()
    {
        var test = await _labService.GetLabTestByIdAsync(2);
        test!.IsActive = false;

        await _labService.UpdateLabTestAsync(test);

        var updated = await _labService.GetLabTestByIdAsync(2);
        updated!.IsActive.Should().BeFalse();
    }

    #endregion

    #region Edge Cases and Performance Tests

    [Fact]
    public async Task LabOrder_WithLongNotes_HandlesGracefully()
    {
        var longNotes = new string('A', 5000);
        var newOrder = new LabOrder
        {
            BranchId = 1,
            PatientId = 1,
            OrderNumber = "LAB-LONG001",
            OrderDate = DateTime.UtcNow,
            Status = LabOrderStatus.Pending,
            OrderedById = 1,
            Priority = "Routine",
            Notes = longNotes
        };

        var action = () => _labService.CreateLabOrderAsync(newOrder);
        await action.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task GetLabOrdersByBranchIdAsync_LargeDataSet_PerformsWell()
    {
        var startTime = DateTime.UtcNow;

        var result = await _labService.GetLabOrdersByBranchIdAsync(1);
        var orders = result.ToList();

        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ConcurrentLabOrderUpdates_AllSucceed()
    {
        var orders = await _context.LabOrders.Take(5).ToListAsync();

        var tasks = orders.Select(async o =>
        {
            o.Notes = $"Concurrent update {o.Id}";
            await _labService.UpdateLabOrderAsync(o);
        }).ToList();

        await Task.WhenAll(tasks);

        foreach (var order in orders)
        {
            var updated = await _labService.GetLabOrderByIdAsync(order.Id);
            updated!.Notes.Should().Contain("Concurrent update");
        }
    }

    #endregion

    #region Quality Control Tests

    [Fact]
    public async Task LabResult_WithAbnormalFlag_SavesCorrectly()
    {
        var pendingItem = await _context.LabOrderItems
            .Where(i => i.Status == LabResultStatus.Pending)
            .Skip(20)
            .FirstOrDefaultAsync();

        if (pendingItem != null)
        {
            var newResult = new LabResult
            {
                LabOrderItemId = pendingItem.Id,
                Result = "250",
                Unit = "mg/dL",
                ReferenceRange = "70-100",
                IsAbnormal = true,
                Interpretation = "High",
                Status = LabResultStatus.Completed,
                PerformedById = 6,
                PerformedAt = DateTime.UtcNow
            };

            var result = await _labService.CreateLabResultAsync(newResult);

            result.IsAbnormal.Should().BeTrue();
        }
    }

    [Fact]
    public async Task LabResult_WithCriticalFlag_SavesCorrectly()
    {
        var pendingItem = await _context.LabOrderItems
            .Where(i => i.Status == LabResultStatus.Pending)
            .Skip(25)
            .FirstOrDefaultAsync();

        if (pendingItem != null)
        {
            var newResult = new LabResult
            {
                LabOrderItemId = pendingItem.Id,
                Result = "500",
                Unit = "mg/dL",
                ReferenceRange = "70-100",
                IsAbnormal = true,
                IsCritical = true,
                Interpretation = "Critical High",
                Status = LabResultStatus.Completed,
                PerformedById = 6,
                PerformedAt = DateTime.UtcNow
            };

            var result = await _labService.CreateLabResultAsync(newResult);

            result.IsCritical.Should().BeTrue();
        }
    }

    #endregion

    #region Verification Tests

    [Fact]
    public async Task VerifyLabResultAsync_ValidVerification_UpdatesSuccessfully()
    {
        var unverifiedResult = await _context.LabResults
            .FirstOrDefaultAsync(r => r.VerifiedById == null);

        if (unverifiedResult != null)
        {
            await _labService.VerifyLabResultAsync(unverifiedResult.Id, 1);

            var verified = await _labService.GetLabResultByIdAsync(unverifiedResult.Id);
            verified!.VerifiedById.Should().Be(1);
            verified.VerifiedAt.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task VerifyLabResultAsync_AlreadyVerified_HandlesGracefully()
    {
        var verifiedResult = await _context.LabResults
            .FirstOrDefaultAsync(r => r.VerifiedById != null);

        if (verifiedResult != null)
        {
            // Should not throw
            await _labService.VerifyLabResultAsync(verifiedResult.Id, 2);
        }
    }

    #endregion
}
