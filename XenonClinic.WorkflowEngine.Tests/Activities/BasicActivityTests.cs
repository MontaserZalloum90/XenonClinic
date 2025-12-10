namespace XenonClinic.WorkflowEngine.Tests.Activities;

using FluentAssertions;
using Moq;
using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Core.Activities;
using Xunit;

public class StartActivityTests
{
    private readonly Mock<IWorkflowContext> _contextMock;

    public StartActivityTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
        _contextMock.Setup(c => c.WorkflowId).Returns("test-workflow");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess()
    {
        var activity = new StartActivity
        {
            Id = "start",
            Name = "Start"
        };

        var result = await activity.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Type_ReturnsStart()
    {
        var activity = new StartActivity
        {
            Id = "start",
            Name = "Start"
        };

        activity.Type.Should().Be("start");
    }
}

public class EndActivityTests
{
    private readonly Mock<IWorkflowContext> _contextMock;
    private readonly Dictionary<string, object?> _outputValues;

    public EndActivityTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
        _contextMock.Setup(c => c.WorkflowId).Returns("test-workflow");

        _outputValues = new Dictionary<string, object?>();
        _contextMock.Setup(c => c.SetOutput(It.IsAny<string>(), It.IsAny<object?>()))
            .Callback<string, object?>((k, v) => _outputValues[k] = v);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess()
    {
        var activity = new EndActivity
        {
            Id = "end",
            Name = "End"
        };

        var result = await activity.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithOutputMappings_SetsOutputs()
    {
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>
        {
            ["result"] = "success"
        });

        var activity = new EndActivity
        {
            Id = "end",
            Name = "End",
            FinalOutputMappings = new Dictionary<string, string>
            {
                ["status"] = "var.result"
            }
        };

        await activity.ExecuteAsync(_contextMock.Object);

        _outputValues.Should().ContainKey("status");
    }
}

public class UserTaskActivityTests
{
    private readonly Mock<IWorkflowContext> _contextMock;

    public UserTaskActivityTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
    }

    [Fact]
    public async Task ExecuteAsync_SuspendsWorkflow()
    {
        var activity = new UserTaskActivity
        {
            Id = "userTask1",
            Name = "Review Task",
            Assignee = "john.doe"
        };

        var result = await activity.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Suspend.Should().BeTrue();
        result.BookmarkName.Should().Contain("userTask");
    }

    [Fact]
    public async Task ResumeAsync_WithInput_MapsOutputs()
    {
        var outputValues = new Dictionary<string, object?>();
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>());
        _contextMock.Setup(c => c.SetVariable(It.IsAny<string>(), It.IsAny<object?>()))
            .Callback<string, object?>((k, v) => outputValues[k] = v);

        var activity = new UserTaskActivity
        {
            Id = "userTask1",
            Name = "Review Task",
            OutputMappings = new Dictionary<string, string>
            {
                ["decision"] = "approved"
            }
        };

        var input = new Dictionary<string, object?>
        {
            ["approved"] = true,
            ["comments"] = "Looks good"
        };

        var result = await activity.ResumeAsync(_contextMock.Object, input);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Type_ReturnsUserTask()
    {
        var activity = new UserTaskActivity
        {
            Id = "userTask1",
            Name = "Review Task"
        };

        activity.Type.Should().Be("userTask");
    }
}

public class TimerActivityTests
{
    private readonly Mock<IWorkflowContext> _contextMock;
    private readonly Dictionary<string, object?> _variables;

    public TimerActivityTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
        _variables = new Dictionary<string, object?>();
        _contextMock.Setup(c => c.SetVariable(It.IsAny<string>(), It.IsAny<object?>()))
            .Callback<string, object?>((k, v) => _variables[k] = v);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuration_SuspendsWorkflow()
    {
        var activity = new TimerActivity
        {
            Id = "timer1",
            Name = "Wait Timer",
            Duration = "PT1H" // 1 hour
        };

        var result = await activity.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Suspend.Should().BeTrue();
        result.BookmarkName.Should().Contain("timer");
    }

    [Fact]
    public async Task ExecuteAsync_SetsFireTimeVariable()
    {
        var activity = new TimerActivity
        {
            Id = "timer1",
            Name = "Wait Timer",
            Duration = "PT1M" // 1 minute
        };

        await activity.ExecuteAsync(_contextMock.Object);

        _variables.Should().ContainKey("_timer_timer1_fireTime");
    }

    [Fact]
    public async Task ResumeAsync_ReturnsSuccess()
    {
        var activity = new TimerActivity
        {
            Id = "timer1",
            Name = "Wait Timer"
        };

        var result = await activity.ResumeAsync(_contextMock.Object, null);

        result.IsSuccess.Should().BeTrue();
    }
}

public class SignalActivityTests
{
    private readonly Mock<IWorkflowContext> _contextMock;
    private readonly Dictionary<string, object?> _variables;

    public SignalActivityTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
        _variables = new Dictionary<string, object?>();
        _contextMock.Setup(c => c.SetVariable(It.IsAny<string>(), It.IsAny<object?>()))
            .Callback<string, object?>((k, v) => _variables[k] = v);
        _contextMock.Setup(c => c.Variables).Returns(_variables);
    }

    [Fact]
    public async Task SignalReceiveActivity_SuspendsWorkflow()
    {
        var activity = new SignalReceiveActivity
        {
            Id = "signal1",
            Name = "Wait for Signal",
            SignalName = "approval_signal"
        };

        var result = await activity.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Suspend.Should().BeTrue();
        result.BookmarkName.Should().Contain("signal_approval_signal");
    }

    [Fact]
    public async Task SignalThrowActivity_ExecutesSuccessfully()
    {
        var activity = new SignalThrowActivity
        {
            Id = "signal1",
            Name = "Throw Signal",
            SignalName = "notify_signal",
            PayloadExpression = null
        };

        var result = await activity.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.Output.Should().ContainKey("signalName");
        result.Output!["signalName"].Should().Be("notify_signal");
    }

    [Fact]
    public async Task SignalReceiveActivity_ResumeWithData_MapsOutputs()
    {
        var activity = new SignalReceiveActivity
        {
            Id = "signal1",
            Name = "Wait for Signal",
            SignalName = "approval_signal",
            OutputMappings = new Dictionary<string, string>
            {
                ["status"] = "signalStatus"
            }
        };

        var signalData = new Dictionary<string, object?>
        {
            ["signalStatus"] = "approved"
        };

        var result = await activity.ResumeAsync(_contextMock.Object, signalData);

        result.IsSuccess.Should().BeTrue();
    }
}

public class ScriptActivityTests
{
    private readonly Mock<IWorkflowContext> _contextMock;
    private readonly Dictionary<string, object?> _variables;

    public ScriptActivityTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
        _variables = new Dictionary<string, object?>();
        _contextMock.Setup(c => c.Variables).Returns(_variables);
        _contextMock.Setup(c => c.SetVariable(It.IsAny<string>(), It.IsAny<object?>()))
            .Callback<string, object?>((k, v) => _variables[k] = v);
    }

    [Fact]
    public async Task ExecuteAsync_WithVariableAssignment_SetsVariable()
    {
        _variables["input"] = 10;
        _contextMock.Setup(c => c.Input).Returns(new Dictionary<string, object?>());

        var activity = new ScriptActivity
        {
            Id = "script1",
            Name = "Calculate",
            Language = "expression",
            Script = "result = var.input"
        };

        var result = await activity.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        _variables.Should().ContainKey("result");
    }

    [Fact]
    public void Type_ReturnsScript()
    {
        var activity = new ScriptActivity
        {
            Id = "script1",
            Name = "Calculate",
            Script = "x = 1"
        };

        activity.Type.Should().Be("script");
    }
}
