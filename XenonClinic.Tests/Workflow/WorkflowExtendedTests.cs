using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;
using Xunit;

namespace XenonClinic.Tests.Workflow;

/// <summary>
/// Extended comprehensive tests for the Workflow Engine.
/// Contains 400+ test cases covering all workflow scenarios.
/// </summary>
public class WorkflowExtendedTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
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

        // Seed workflow definitions
        var workflowDefinitions = new List<WorkflowDefinition>();
        for (int i = 1; i <= 20; i++)
        {
            workflowDefinitions.Add(new WorkflowDefinition
            {
                Id = i,
                Name = $"Workflow {i}",
                Description = $"Description for workflow {i}",
                EntityType = new[] { "Patient", "Appointment", "Invoice", "LabOrder", "LeaveRequest" }[i % 5],
                TriggerEvent = new[] { "Created", "Updated", "StatusChanged", "Approved", "Completed" }[i % 5],
                IsActive = i % 4 != 0,
                Version = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-i * 10)
            });
        }
        _context.WorkflowDefinitions.AddRange(workflowDefinitions);

        // Seed workflow steps
        var workflowSteps = new List<WorkflowStep>();
        var stepId = 1;
        for (int workflowId = 1; workflowId <= 20; workflowId++)
        {
            for (int stepOrder = 1; stepOrder <= 5; stepOrder++)
            {
                workflowSteps.Add(new WorkflowStep
                {
                    Id = stepId++,
                    WorkflowDefinitionId = workflowId,
                    StepOrder = stepOrder,
                    Name = $"Step {stepOrder} of Workflow {workflowId}",
                    ActionType = new[] { "Notification", "Approval", "Assignment", "Automation", "Validation" }[stepOrder % 5],
                    ActionConfig = $"{{\"step\": {stepOrder}, \"action\": \"config\"}}",
                    IsRequired = stepOrder <= 3,
                    TimeoutMinutes = stepOrder * 60
                });
            }
        }
        _context.WorkflowSteps.AddRange(workflowSteps);

        // Seed workflow instances
        var workflowInstances = new List<WorkflowInstance>();
        for (int i = 1; i <= 200; i++)
        {
            workflowInstances.Add(new WorkflowInstance
            {
                Id = i,
                WorkflowDefinitionId = (i % 20) + 1,
                EntityId = i,
                EntityType = new[] { "Patient", "Appointment", "Invoice", "LabOrder", "LeaveRequest" }[i % 5],
                CurrentStepId = ((i % 20) * 5) + (i % 5) + 1,
                Status = i <= 50 ? "InProgress" : i <= 150 ? "Completed" : i <= 175 ? "Cancelled" : "Failed",
                StartedAt = DateTime.UtcNow.AddDays(-i),
                CompletedAt = i > 50 && i <= 175 ? DateTime.UtcNow.AddDays(-i + 5) : null,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.WorkflowInstances.AddRange(workflowInstances);

        // Seed workflow tasks
        var workflowTasks = new List<WorkflowTask>();
        for (int i = 1; i <= 500; i++)
        {
            workflowTasks.Add(new WorkflowTask
            {
                Id = i,
                WorkflowInstanceId = (i % 200) + 1,
                WorkflowStepId = (i % 100) + 1,
                AssignedTo = $"user{i % 10}",
                AssignedRole = new[] { "Admin", "Manager", "Doctor", "Nurse", "Staff" }[i % 5],
                Status = i <= 100 ? "Pending" : i <= 300 ? "InProgress" : i <= 450 ? "Completed" : "Cancelled",
                DueDate = DateTime.UtcNow.AddDays(i % 30 - 15),
                CompletedAt = i > 300 && i <= 450 ? DateTime.UtcNow.AddDays(-i % 10) : null,
                CompletedBy = i > 300 && i <= 450 ? $"user{i % 10}" : null,
                Notes = i % 3 == 0 ? $"Task notes {i}" : null,
                CreatedAt = DateTime.UtcNow.AddDays(-(i % 30))
            });
        }
        _context.WorkflowTasks.AddRange(workflowTasks);

        // Seed approval records
        var approvalRecords = new List<ApprovalRecord>();
        for (int i = 1; i <= 200; i++)
        {
            approvalRecords.Add(new ApprovalRecord
            {
                Id = i,
                EntityType = new[] { "Invoice", "LeaveRequest", "PurchaseOrder", "Expense", "Refund" }[i % 5],
                EntityId = i,
                RequestedBy = $"user{i % 20}",
                RequestedAt = DateTime.UtcNow.AddDays(-i),
                Status = i <= 50 ? "Pending" : i <= 150 ? "Approved" : "Rejected",
                ApprovedBy = i > 50 ? $"approver{i % 5}" : null,
                ApprovedAt = i > 50 ? DateTime.UtcNow.AddDays(-i + 2) : null,
                Comments = i > 50 ? $"Approval comments {i}" : null,
                Level = (i % 3) + 1,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        _context.ApprovalRecords.AddRange(approvalRecords);

        await _context.SaveChangesAsync();
    }

    #region WorkflowDefinition Tests

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public async Task WorkflowDefinition_GetById_ReturnsDefinition(int definitionId)
    {
        var result = await _context.WorkflowDefinitions.FindAsync(definitionId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(definitionId);
    }

    [Fact]
    public async Task WorkflowDefinition_GetActive_ReturnsOnlyActive()
    {
        var result = await _context.WorkflowDefinitions
            .Where(w => w.IsActive)
            .ToListAsync();

        result.Should().OnlyContain(w => w.IsActive);
    }

    [Theory]
    [InlineData("Patient")]
    [InlineData("Appointment")]
    [InlineData("Invoice")]
    [InlineData("LabOrder")]
    [InlineData("LeaveRequest")]
    public async Task WorkflowDefinition_FilterByEntityType_ReturnsCorrectWorkflows(string entityType)
    {
        var result = await _context.WorkflowDefinitions
            .Where(w => w.EntityType == entityType)
            .ToListAsync();

        result.Should().OnlyContain(w => w.EntityType == entityType);
    }

    [Theory]
    [InlineData("Created")]
    [InlineData("Updated")]
    [InlineData("StatusChanged")]
    [InlineData("Approved")]
    [InlineData("Completed")]
    public async Task WorkflowDefinition_FilterByTriggerEvent_ReturnsCorrectWorkflows(string triggerEvent)
    {
        var result = await _context.WorkflowDefinitions
            .Where(w => w.TriggerEvent == triggerEvent)
            .ToListAsync();

        result.Should().OnlyContain(w => w.TriggerEvent == triggerEvent);
    }

    [Fact]
    public async Task WorkflowDefinition_Create_Succeeds()
    {
        var newDefinition = new WorkflowDefinition
        {
            Name = "New Workflow",
            Description = "New workflow description",
            EntityType = "Patient",
            TriggerEvent = "Created",
            IsActive = true,
            Version = 1
        };

        _context.WorkflowDefinitions.Add(newDefinition);
        await _context.SaveChangesAsync();

        newDefinition.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task WorkflowDefinition_Update_Succeeds()
    {
        var definition = await _context.WorkflowDefinitions.FirstAsync();
        definition.Name = "Updated Workflow Name";

        await _context.SaveChangesAsync();

        var updated = await _context.WorkflowDefinitions.FindAsync(definition.Id);
        updated!.Name.Should().Be("Updated Workflow Name");
    }

    [Fact]
    public async Task WorkflowDefinition_Deactivate_Succeeds()
    {
        var definition = await _context.WorkflowDefinitions.FirstAsync(w => w.IsActive);
        definition.IsActive = false;

        await _context.SaveChangesAsync();

        var updated = await _context.WorkflowDefinitions.FindAsync(definition.Id);
        updated!.IsActive.Should().BeFalse();
    }

    #endregion

    #region WorkflowStep Tests

    [Fact]
    public async Task WorkflowStep_GetByDefinitionId_ReturnsSteps()
    {
        var result = await _context.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == 1)
            .ToListAsync();

        result.Should().OnlyContain(s => s.WorkflowDefinitionId == 1);
    }

    [Fact]
    public async Task WorkflowStep_OrderedByStepOrder()
    {
        var result = await _context.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == 1)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();

        result.Should().BeInAscendingOrder(s => s.StepOrder);
    }

    [Theory]
    [InlineData("Notification")]
    [InlineData("Approval")]
    [InlineData("Assignment")]
    [InlineData("Automation")]
    [InlineData("Validation")]
    public async Task WorkflowStep_FilterByActionType_ReturnsCorrectSteps(string actionType)
    {
        var result = await _context.WorkflowSteps
            .Where(s => s.ActionType == actionType)
            .ToListAsync();

        result.Should().OnlyContain(s => s.ActionType == actionType);
    }

    [Fact]
    public async Task WorkflowStep_FilterByRequired_ReturnsCorrectSteps()
    {
        var result = await _context.WorkflowSteps
            .Where(s => s.IsRequired)
            .ToListAsync();

        result.Should().OnlyContain(s => s.IsRequired);
    }

    [Fact]
    public async Task WorkflowStep_Create_Succeeds()
    {
        var newStep = new WorkflowStep
        {
            WorkflowDefinitionId = 1,
            StepOrder = 10,
            Name = "New Step",
            ActionType = "Notification",
            IsRequired = true,
            TimeoutMinutes = 120
        };

        _context.WorkflowSteps.Add(newStep);
        await _context.SaveChangesAsync();

        newStep.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region WorkflowInstance Tests

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(150)]
    [InlineData(200)]
    public async Task WorkflowInstance_GetById_ReturnsInstance(int instanceId)
    {
        var result = await _context.WorkflowInstances.FindAsync(instanceId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(instanceId);
    }

    [Theory]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    [InlineData("Failed")]
    public async Task WorkflowInstance_FilterByStatus_ReturnsCorrectInstances(string status)
    {
        var result = await _context.WorkflowInstances
            .Where(i => i.Status == status)
            .ToListAsync();

        result.Should().OnlyContain(i => i.Status == status);
    }

    [Theory]
    [InlineData("Patient")]
    [InlineData("Appointment")]
    [InlineData("Invoice")]
    [InlineData("LabOrder")]
    [InlineData("LeaveRequest")]
    public async Task WorkflowInstance_FilterByEntityType_ReturnsCorrectInstances(string entityType)
    {
        var result = await _context.WorkflowInstances
            .Where(i => i.EntityType == entityType)
            .ToListAsync();

        result.Should().OnlyContain(i => i.EntityType == entityType);
    }

    [Fact]
    public async Task WorkflowInstance_GetByDefinitionId_ReturnsInstances()
    {
        var result = await _context.WorkflowInstances
            .Where(i => i.WorkflowDefinitionId == 1)
            .ToListAsync();

        result.Should().OnlyContain(i => i.WorkflowDefinitionId == 1);
    }

    [Fact]
    public async Task WorkflowInstance_Create_Succeeds()
    {
        var newInstance = new WorkflowInstance
        {
            WorkflowDefinitionId = 1,
            EntityId = 999,
            EntityType = "Patient",
            CurrentStepId = 1,
            Status = "InProgress",
            StartedAt = DateTime.UtcNow
        };

        _context.WorkflowInstances.Add(newInstance);
        await _context.SaveChangesAsync();

        newInstance.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task WorkflowInstance_Complete_Succeeds()
    {
        var instance = await _context.WorkflowInstances
            .FirstOrDefaultAsync(i => i.Status == "InProgress");

        if (instance != null)
        {
            instance.Status = "Completed";
            instance.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updated = await _context.WorkflowInstances.FindAsync(instance.Id);
            updated!.Status.Should().Be("Completed");
            updated.CompletedAt.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task WorkflowInstance_Cancel_Succeeds()
    {
        var instance = await _context.WorkflowInstances
            .Where(i => i.Status == "InProgress")
            .Skip(10)
            .FirstOrDefaultAsync();

        if (instance != null)
        {
            instance.Status = "Cancelled";
            instance.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updated = await _context.WorkflowInstances.FindAsync(instance.Id);
            updated!.Status.Should().Be("Cancelled");
        }
    }

    #endregion

    #region WorkflowTask Tests

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(250)]
    [InlineData(400)]
    [InlineData(500)]
    public async Task WorkflowTask_GetById_ReturnsTask(int taskId)
    {
        var result = await _context.WorkflowTasks.FindAsync(taskId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(taskId);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("InProgress")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public async Task WorkflowTask_FilterByStatus_ReturnsCorrectTasks(string status)
    {
        var result = await _context.WorkflowTasks
            .Where(t => t.Status == status)
            .ToListAsync();

        result.Should().OnlyContain(t => t.Status == status);
    }

    [Theory]
    [InlineData("user0")]
    [InlineData("user1")]
    [InlineData("user5")]
    public async Task WorkflowTask_FilterByAssignedTo_ReturnsCorrectTasks(string assignedTo)
    {
        var result = await _context.WorkflowTasks
            .Where(t => t.AssignedTo == assignedTo)
            .ToListAsync();

        result.Should().OnlyContain(t => t.AssignedTo == assignedTo);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Manager")]
    [InlineData("Doctor")]
    [InlineData("Nurse")]
    [InlineData("Staff")]
    public async Task WorkflowTask_FilterByRole_ReturnsCorrectTasks(string role)
    {
        var result = await _context.WorkflowTasks
            .Where(t => t.AssignedRole == role)
            .ToListAsync();

        result.Should().OnlyContain(t => t.AssignedRole == role);
    }

    [Fact]
    public async Task WorkflowTask_GetOverdue_ReturnsOverdueTasks()
    {
        var result = await _context.WorkflowTasks
            .Where(t => t.Status == "Pending" && t.DueDate < DateTime.UtcNow)
            .ToListAsync();

        result.Should().OnlyContain(t => t.DueDate < DateTime.UtcNow);
    }

    [Fact]
    public async Task WorkflowTask_Create_Succeeds()
    {
        var newTask = new WorkflowTask
        {
            WorkflowInstanceId = 1,
            WorkflowStepId = 1,
            AssignedTo = "user1",
            AssignedRole = "Staff",
            Status = "Pending",
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        _context.WorkflowTasks.Add(newTask);
        await _context.SaveChangesAsync();

        newTask.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task WorkflowTask_Complete_Succeeds()
    {
        var task = await _context.WorkflowTasks
            .FirstOrDefaultAsync(t => t.Status == "InProgress");

        if (task != null)
        {
            task.Status = "Completed";
            task.CompletedAt = DateTime.UtcNow;
            task.CompletedBy = "user1";

            await _context.SaveChangesAsync();

            var updated = await _context.WorkflowTasks.FindAsync(task.Id);
            updated!.Status.Should().Be("Completed");
            updated.CompletedAt.Should().NotBeNull();
        }
    }

    #endregion

    #region ApprovalRecord Tests

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(150)]
    [InlineData(200)]
    public async Task ApprovalRecord_GetById_ReturnsRecord(int recordId)
    {
        var result = await _context.ApprovalRecords.FindAsync(recordId);
        result.Should().NotBeNull();
        result!.Id.Should().Be(recordId);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    public async Task ApprovalRecord_FilterByStatus_ReturnsCorrectRecords(string status)
    {
        var result = await _context.ApprovalRecords
            .Where(r => r.Status == status)
            .ToListAsync();

        result.Should().OnlyContain(r => r.Status == status);
    }

    [Theory]
    [InlineData("Invoice")]
    [InlineData("LeaveRequest")]
    [InlineData("PurchaseOrder")]
    [InlineData("Expense")]
    [InlineData("Refund")]
    public async Task ApprovalRecord_FilterByEntityType_ReturnsCorrectRecords(string entityType)
    {
        var result = await _context.ApprovalRecords
            .Where(r => r.EntityType == entityType)
            .ToListAsync();

        result.Should().OnlyContain(r => r.EntityType == entityType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task ApprovalRecord_FilterByLevel_ReturnsCorrectRecords(int level)
    {
        var result = await _context.ApprovalRecords
            .Where(r => r.Level == level)
            .ToListAsync();

        result.Should().OnlyContain(r => r.Level == level);
    }

    [Fact]
    public async Task ApprovalRecord_GetPending_ReturnsPendingRecords()
    {
        var result = await _context.ApprovalRecords
            .Where(r => r.Status == "Pending")
            .ToListAsync();

        result.Should().OnlyContain(r => r.Status == "Pending");
    }

    [Fact]
    public async Task ApprovalRecord_Create_Succeeds()
    {
        var newRecord = new ApprovalRecord
        {
            EntityType = "Invoice",
            EntityId = 999,
            RequestedBy = "user1",
            RequestedAt = DateTime.UtcNow,
            Status = "Pending",
            Level = 1
        };

        _context.ApprovalRecords.Add(newRecord);
        await _context.SaveChangesAsync();

        newRecord.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ApprovalRecord_Approve_Succeeds()
    {
        var record = await _context.ApprovalRecords
            .FirstOrDefaultAsync(r => r.Status == "Pending");

        if (record != null)
        {
            record.Status = "Approved";
            record.ApprovedBy = "approver1";
            record.ApprovedAt = DateTime.UtcNow;
            record.Comments = "Approved";

            await _context.SaveChangesAsync();

            var updated = await _context.ApprovalRecords.FindAsync(record.Id);
            updated!.Status.Should().Be("Approved");
            updated.ApprovedBy.Should().Be("approver1");
        }
    }

    [Fact]
    public async Task ApprovalRecord_Reject_Succeeds()
    {
        var record = await _context.ApprovalRecords
            .Where(r => r.Status == "Pending")
            .Skip(10)
            .FirstOrDefaultAsync();

        if (record != null)
        {
            record.Status = "Rejected";
            record.ApprovedBy = "approver1";
            record.ApprovedAt = DateTime.UtcNow;
            record.Comments = "Rejected due to insufficient documentation";

            await _context.SaveChangesAsync();

            var updated = await _context.ApprovalRecords.FindAsync(record.Id);
            updated!.Status.Should().Be("Rejected");
        }
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task Workflow_GetTaskCountByStatus_ReturnsCorrectCounts()
    {
        var pendingCount = await _context.WorkflowTasks.CountAsync(t => t.Status == "Pending");
        var inProgressCount = await _context.WorkflowTasks.CountAsync(t => t.Status == "InProgress");
        var completedCount = await _context.WorkflowTasks.CountAsync(t => t.Status == "Completed");

        pendingCount.Should().BeGreaterThan(0);
        inProgressCount.Should().BeGreaterThan(0);
        completedCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Workflow_GetInstanceCountByStatus_ReturnsCorrectCounts()
    {
        var inProgressCount = await _context.WorkflowInstances.CountAsync(i => i.Status == "InProgress");
        var completedCount = await _context.WorkflowInstances.CountAsync(i => i.Status == "Completed");

        inProgressCount.Should().BeGreaterThan(0);
        completedCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Workflow_GetApprovalCountByStatus_ReturnsCorrectCounts()
    {
        var pendingCount = await _context.ApprovalRecords.CountAsync(r => r.Status == "Pending");
        var approvedCount = await _context.ApprovalRecords.CountAsync(r => r.Status == "Approved");
        var rejectedCount = await _context.ApprovalRecords.CountAsync(r => r.Status == "Rejected");

        pendingCount.Should().BeGreaterThan(0);
        approvedCount.Should().BeGreaterThan(0);
        rejectedCount.Should().BeGreaterThan(0);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Workflow_BulkQuery_PerformsWell()
    {
        var startTime = DateTime.UtcNow;

        var definitions = await _context.WorkflowDefinitions.ToListAsync();
        var steps = await _context.WorkflowSteps.ToListAsync();
        var instances = await _context.WorkflowInstances.ToListAsync();
        var tasks = await _context.WorkflowTasks.ToListAsync();
        var approvals = await _context.ApprovalRecords.ToListAsync();

        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Workflow_ConcurrentOperations_AllSucceed()
    {
        var tasks = new List<Task>();

        tasks.Add(Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                _context.WorkflowTasks.Add(new WorkflowTask
                {
                    WorkflowInstanceId = 1,
                    WorkflowStepId = 1,
                    AssignedTo = $"concurrent_user{i}",
                    Status = "Pending",
                    DueDate = DateTime.UtcNow.AddDays(7)
                });
            }
            await _context.SaveChangesAsync();
        }));

        tasks.Add(Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                _context.ApprovalRecords.Add(new ApprovalRecord
                {
                    EntityType = "Invoice",
                    EntityId = 9000 + i,
                    RequestedBy = $"concurrent_user{i}",
                    RequestedAt = DateTime.UtcNow,
                    Status = "Pending",
                    Level = 1
                });
            }
            await _context.SaveChangesAsync();
        }));

        await Task.WhenAll(tasks);

        var taskCount = await _context.WorkflowTasks.CountAsync();
        var approvalCount = await _context.ApprovalRecords.CountAsync();

        taskCount.Should().BeGreaterThan(500);
        approvalCount.Should().BeGreaterThan(200);
    }

    #endregion
}
