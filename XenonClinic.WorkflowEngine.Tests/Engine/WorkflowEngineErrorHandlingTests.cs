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

public class WorkflowEngineErrorHandlingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IWorkflowDefinitionStore> _definitionStoreMock;
    private readonly Mock<IWorkflowInstanceStore> _instanceStoreMock;
    private readonly Mock<ILogger<WorkflowEngine>> _loggerMock;
    private readonly WorkflowEngine _engine;

    public WorkflowEngineErrorHandlingTests()
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
    public async Task StartAsync_WithMissingInstance_ThrowsWorkflowNotFoundException()
    {
        _instanceStoreMock.Setup(s => s.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync((WorkflowInstanceState?)null);

        var act = () => _engine.StartAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<WorkflowNotFoundException>();
    }

    [Fact]
    public async Task CancelAsync_WithCompletedInstance_ThrowsInvalidStateException()
    {
        var state = new WorkflowInstanceState
        {
            Id = Guid.NewGuid(),
            WorkflowId = "test",
            Status = WorkflowStatus.Completed
        };
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);

        var act = () => _engine.CancelAsync(state.Id);

        await act.Should().ThrowAsync<WorkflowInvalidStateException>();
    }

    [Fact]
    public async Task RetryAsync_WithNonFaultedInstance_ThrowsInvalidStateException()
    {
        var state = new WorkflowInstanceState
        {
            Id = Guid.NewGuid(),
            WorkflowId = "test",
            Status = WorkflowStatus.Running
        };
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);

        var act = () => _engine.RetryAsync(state.Id);

        await act.Should().ThrowAsync<WorkflowInvalidStateException>();
    }

    [Fact]
    public async Task ResumeAsync_WithMissingBookmark_ThrowsBookmarkNotFoundException()
    {
        var state = new WorkflowInstanceState
        {
            Id = Guid.NewGuid(),
            WorkflowId = "test",
            Status = WorkflowStatus.Suspended,
            Bookmarks = new List<WorkflowBookmark>()
        };
        _instanceStoreMock.Setup(s => s.GetAsync(state.Id))
            .ReturnsAsync(state);

        var act = () => _engine.ResumeAsync(state.Id, "nonexistent_bookmark");

        await act.Should().ThrowAsync<WorkflowBookmarkNotFoundException>();
    }

    [Fact]
    public async Task CreateInstanceAsync_WithInactiveDefinition_CreatesInstance()
    {
        var definition = new WorkflowDefinitionModel
        {
            Id = "test",
            Name = "Test",
            Version = 1,
            IsActive = false,
            StartActivityId = "start",
            Activities = new Dictionary<string, IActivity>
            {
                ["start"] = new StartActivity { Id = "start", Name = "Start" }
            }
        };
        _definitionStoreMock.Setup(s => s.GetAsync("test", null))
            .ReturnsAsync(definition);
        _instanceStoreMock.Setup(s => s.SaveAsync(It.IsAny<WorkflowInstanceState>()))
            .Returns(Task.CompletedTask);

        var instance = await _engine.CreateInstanceAsync("test");

        instance.Should().NotBeNull();
    }

    [Fact]
    public async Task SignalAsync_WithMissingInstance_ThrowsNotFoundException()
    {
        _instanceStoreMock.Setup(s => s.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync((WorkflowInstanceState?)null);

        var act = () => _engine.SignalAsync(Guid.NewGuid(), "test_signal");

        await act.Should().ThrowAsync<WorkflowNotFoundException>();
    }

    [Fact]
    public async Task TerminateAsync_WithMissingInstance_ThrowsNotFoundException()
    {
        _instanceStoreMock.Setup(s => s.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync((WorkflowInstanceState?)null);

        var act = () => _engine.TerminateAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<WorkflowNotFoundException>();
    }
}
