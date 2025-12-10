namespace XenonClinic.WorkflowEngine.Tests.Engine;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Core.Activities;
using XenonClinic.WorkflowEngine.Core.Engine;
using XenonClinic.WorkflowEngine.Models.Definitions;
using XenonClinic.WorkflowEngine.Persistence.Abstractions;
using Xunit;

public class WorkflowEngineTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IWorkflowDefinitionStore> _definitionStoreMock;
    private readonly Mock<IWorkflowInstanceStore> _instanceStoreMock;
    private readonly Mock<ILogger<WorkflowEngine>> _loggerMock;
    private readonly WorkflowEngine _engine;

    public WorkflowEngineTests()
    {
        var services = new ServiceCollection();
        _serviceProvider = services.BuildServiceProvider();

        _definitionStoreMock = new Mock<IWorkflowDefinitionStore>();
        _instanceStoreMock = new Mock<IWorkflowInstanceStore>();
        _loggerMock = new Mock<ILogger<WorkflowEngine>>();

        _engine = new WorkflowEngine(
            _definitionStoreMock.Object,
            _instanceStoreMock.Object,
            _serviceProvider,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task CreateInstanceAsync_WithValidDefinition_CreatesInstance()
    {
        var definition = CreateSimpleWorkflowDefinition();
        _definitionStoreMock.Setup(s => s.GetAsync("test-workflow", null))
            .ReturnsAsync(definition);
        _instanceStoreMock.Setup(s => s.SaveAsync(It.IsAny<WorkflowInstanceState>()))
            .Returns(Task.CompletedTask);

        var instance = await _engine.CreateInstanceAsync("test-workflow");

        instance.Should().NotBeNull();
        instance.WorkflowId.Should().Be("test-workflow");
        instance.Status.Should().Be(WorkflowStatus.Pending);
    }

    [Fact]
    public async Task CreateInstanceAsync_WithMissingDefinition_ThrowsException()
    {
        _definitionStoreMock.Setup(s => s.GetAsync("nonexistent", null))
            .ReturnsAsync((WorkflowDefinitionModel?)null);

        var act = () => _engine.CreateInstanceAsync("nonexistent");

        await act.Should().ThrowAsync<WorkflowNotFoundException>();
    }

    [Fact]
    public async Task CreateInstanceAsync_WithMissingRequiredInput_ThrowsException()
    {
        var definition = CreateSimpleWorkflowDefinition();
        definition.InputParameters = new List<WorkflowParameter>
        {
            new() { Name = "requiredParam", IsRequired = true }
        };
        _definitionStoreMock.Setup(s => s.GetAsync("test-workflow", null))
            .ReturnsAsync(definition);

        var act = () => _engine.CreateInstanceAsync("test-workflow", null);

        await act.Should().ThrowAsync<WorkflowValidationException>();
    }

    [Fact]
    public async Task StartAsync_WithPendingInstance_StartsWorkflow()
    {
        var definition = CreateSimpleWorkflowDefinition();
        var state = CreateWorkflowInstanceState();

        _definitionStoreMock.Setup(s => s.GetAsync("test-workflow", 1))
            .ReturnsAsync(definition);
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);
        _instanceStoreMock.Setup(s => s.SaveAsync(It.IsAny<WorkflowInstanceState>()))
            .Returns(Task.CompletedTask);
        _instanceStoreMock.Setup(s => s.AddHistoryAsync(It.IsAny<WorkflowExecutionRecord>()))
            .Returns(Task.CompletedTask);

        var result = await _engine.StartAsync(state.Id);

        result.Should().NotBeNull();
        result.Status.Should().Be(WorkflowStatus.Completed);
    }

    [Fact]
    public async Task StartAsync_WithNonPendingInstance_ThrowsException()
    {
        var state = CreateWorkflowInstanceState();
        state.Status = WorkflowStatus.Running;
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);

        var act = () => _engine.StartAsync(state.Id);

        await act.Should().ThrowAsync<WorkflowInvalidStateException>();
    }

    [Fact]
    public async Task CancelAsync_WithRunningInstance_CancelsWorkflow()
    {
        var state = CreateWorkflowInstanceState();
        state.Status = WorkflowStatus.Running;
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);
        _instanceStoreMock.Setup(s => s.SaveAsync(It.IsAny<WorkflowInstanceState>()))
            .Returns(Task.CompletedTask);

        await _engine.CancelAsync(state.Id, "Test cancellation");

        _instanceStoreMock.Verify(s => s.SaveAsync(It.Is<WorkflowInstanceState>(ws =>
            ws.Status == WorkflowStatus.Cancelled)), Times.Once);
    }

    [Fact]
    public async Task RetryAsync_WithFaultedInstance_RetriesWorkflow()
    {
        var definition = CreateSimpleWorkflowDefinition();
        var state = CreateWorkflowInstanceState();
        state.Status = WorkflowStatus.Faulted;
        state.CurrentActivityId = "start";

        _definitionStoreMock.Setup(s => s.GetAsync("test-workflow", 1))
            .ReturnsAsync(definition);
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);
        _instanceStoreMock.Setup(s => s.SaveAsync(It.IsAny<WorkflowInstanceState>()))
            .Returns(Task.CompletedTask);
        _instanceStoreMock.Setup(s => s.AddHistoryAsync(It.IsAny<WorkflowExecutionRecord>()))
            .Returns(Task.CompletedTask);

        var result = await _engine.RetryAsync(state.Id);

        result.Should().NotBeNull();
        state.FaultCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetInstanceAsync_WithValidId_ReturnsInstance()
    {
        var state = CreateWorkflowInstanceState();
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);

        var result = await _engine.GetInstanceAsync(state.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(state.Id);
    }

    [Fact]
    public async Task TerminateAsync_WithAnyInstance_TerminatesWorkflow()
    {
        var state = CreateWorkflowInstanceState();
        state.Status = WorkflowStatus.Running;
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);
        _instanceStoreMock.Setup(s => s.SaveAsync(It.IsAny<WorkflowInstanceState>()))
            .Returns(Task.CompletedTask);

        await _engine.TerminateAsync(state.Id, "Force termination");

        _instanceStoreMock.Verify(s => s.SaveAsync(It.Is<WorkflowInstanceState>(ws =>
            ws.Status == WorkflowStatus.Faulted)), Times.Once);
    }

    private static WorkflowDefinitionModel CreateSimpleWorkflowDefinition()
    {
        return new WorkflowDefinitionModel
        {
            Id = "test-workflow",
            Name = "Test Workflow",
            Version = 1,
            StartActivityId = "start",
            Activities = new Dictionary<string, IActivity>
            {
                ["start"] = new StartActivity { Id = "start", Name = "Start" },
                ["end"] = new EndActivity { Id = "end", Name = "End" }
            },
            Transitions = new List<TransitionModel>
            {
                new()
                {
                    Id = "t1",
                    SourceActivityId = "start",
                    TargetActivityId = "end"
                }
            }
        };
    }

    private static WorkflowInstanceState CreateWorkflowInstanceState()
    {
        return new WorkflowInstanceState
        {
            Id = Guid.NewGuid(),
            WorkflowId = "test-workflow",
            Version = 1,
            Name = "Test Instance",
            Status = WorkflowStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CurrentActivityId = "start"
        };
    }
}

public class WorkflowEngineWithGatewaysTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IWorkflowDefinitionStore> _definitionStoreMock;
    private readonly Mock<IWorkflowInstanceStore> _instanceStoreMock;
    private readonly Mock<ILogger<WorkflowEngine>> _loggerMock;
    private readonly WorkflowEngine _engine;

    public WorkflowEngineWithGatewaysTests()
    {
        var services = new ServiceCollection();
        _serviceProvider = services.BuildServiceProvider();

        _definitionStoreMock = new Mock<IWorkflowDefinitionStore>();
        _instanceStoreMock = new Mock<IWorkflowInstanceStore>();
        _loggerMock = new Mock<ILogger<WorkflowEngine>>();

        _engine = new WorkflowEngine(
            _definitionStoreMock.Object,
            _instanceStoreMock.Object,
            _serviceProvider,
            _loggerMock.Object);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task StartAsync_WithExclusiveGateway_TakesCorrectPath()
    {
        var definition = CreateWorkflowWithExclusiveGateway();
        var state = new WorkflowInstanceState
        {
            Id = Guid.NewGuid(),
            WorkflowId = "gateway-workflow",
            Version = 1,
            Name = "Test Instance",
            Status = WorkflowStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CurrentActivityId = "start",
            Variables = new Dictionary<string, object?> { ["amount"] = 150 }
        };

        _definitionStoreMock.Setup(s => s.GetAsync("gateway-workflow", 1))
            .ReturnsAsync(definition);
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);
        _instanceStoreMock.Setup(s => s.SaveAsync(It.IsAny<WorkflowInstanceState>()))
            .Returns(Task.CompletedTask);
        _instanceStoreMock.Setup(s => s.AddHistoryAsync(It.IsAny<WorkflowExecutionRecord>()))
            .Returns(Task.CompletedTask);

        var result = await _engine.StartAsync(state.Id);

        result.Status.Should().Be(WorkflowStatus.Completed);
        state.CompletedActivityIds.Should().Contain("high_value_task");
        state.CompletedActivityIds.Should().NotContain("low_value_task");
    }

    private static WorkflowDefinitionModel CreateWorkflowWithExclusiveGateway()
    {
        return new WorkflowDefinitionModel
        {
            Id = "gateway-workflow",
            Name = "Gateway Workflow",
            Version = 1,
            StartActivityId = "start",
            Activities = new Dictionary<string, IActivity>
            {
                ["start"] = new StartActivity { Id = "start", Name = "Start" },
                ["gateway"] = new ExclusiveGatewayActivity
                {
                    Id = "gateway",
                    Name = "Amount Gateway",
                    Conditions = new Dictionary<string, string>
                    {
                        ["high_value_task"] = "var.amount >= 100",
                        ["low_value_task"] = "var.amount < 100"
                    },
                    DefaultPath = "low_value_task"
                },
                ["high_value_task"] = new TaskActivity
                {
                    Id = "high_value_task",
                    Name = "High Value Task"
                },
                ["low_value_task"] = new TaskActivity
                {
                    Id = "low_value_task",
                    Name = "Low Value Task"
                },
                ["end"] = new EndActivity { Id = "end", Name = "End" }
            },
            Transitions = new List<TransitionModel>
            {
                new() { Id = "t1", SourceActivityId = "start", TargetActivityId = "gateway" },
                new() { Id = "t2", SourceActivityId = "gateway", TargetActivityId = "high_value_task", Condition = "var.amount >= 100" },
                new() { Id = "t3", SourceActivityId = "gateway", TargetActivityId = "low_value_task", Condition = "var.amount < 100", IsDefault = true },
                new() { Id = "t4", SourceActivityId = "high_value_task", TargetActivityId = "end" },
                new() { Id = "t5", SourceActivityId = "low_value_task", TargetActivityId = "end" }
            }
        };
    }
}
