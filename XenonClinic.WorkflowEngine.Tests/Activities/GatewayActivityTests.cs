namespace XenonClinic.WorkflowEngine.Tests.Activities;

using FluentAssertions;
using Moq;
using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Core.Activities;
using Xunit;

public class ExclusiveGatewayActivityTests
{
    private readonly Mock<IWorkflowContext> _contextMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public ExclusiveGatewayActivityTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _contextMock.Setup(c => c.ServiceProvider).Returns(_serviceProviderMock.Object);
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>());
        _contextMock.Setup(c => c.Input).Returns(new Dictionary<string, object?>());
    }

    [Fact]
    public async Task ExecuteAsync_FirstMatchingCondition_ReturnsCorrectPath()
    {
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>
        {
            ["amount"] = 150
        });

        var gateway = new ExclusiveGatewayActivity
        {
            Id = "gateway1",
            Name = "Amount Gateway",
            Conditions = new Dictionary<string, string>
            {
                ["high_path"] = "var.amount >= 100",
                ["low_path"] = "var.amount < 100"
            },
            DefaultPath = "default_path"
        };

        var result = await gateway.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.NextActivityId.Should().Be("high_path");
    }

    [Fact]
    public async Task ExecuteAsync_NoMatchingCondition_ReturnsDefaultPath()
    {
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>
        {
            ["status"] = "unknown"
        });

        var gateway = new ExclusiveGatewayActivity
        {
            Id = "gateway1",
            Name = "Status Gateway",
            Conditions = new Dictionary<string, string>
            {
                ["approved_path"] = "var.status == 'approved'",
                ["rejected_path"] = "var.status == 'rejected'"
            },
            DefaultPath = "default_path"
        };

        var result = await gateway.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.NextActivityId.Should().Be("default_path");
    }

    [Fact]
    public async Task ExecuteAsync_NoMatchAndNoDefault_ReturnsFalse()
    {
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>
        {
            ["status"] = "unknown"
        });

        var gateway = new ExclusiveGatewayActivity
        {
            Id = "gateway1",
            Name = "Status Gateway",
            Conditions = new Dictionary<string, string>
            {
                ["approved_path"] = "var.status == 'approved'"
            },
            DefaultPath = null
        };

        var result = await gateway.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("NO_PATH");
    }
}

public class ParallelGatewayActivityTests
{
    private readonly Mock<IWorkflowContext> _contextMock;

    public ParallelGatewayActivityTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>());
    }

    [Fact]
    public async Task ExecuteAsync_SplitDirection_ReturnsParallelPaths()
    {
        var gateway = new ParallelGatewayActivity
        {
            Id = "parallel1",
            Name = "Parallel Split",
            Direction = GatewayDirection.Split,
            OutgoingPaths = new List<string> { "branch1", "branch2", "branch3" }
        };

        var result = await gateway.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.ParallelNextActivityIds.Should().NotBeNull();
        result.ParallelNextActivityIds.Should().HaveCount(3);
        result.ParallelNextActivityIds.Should().Contain(new[] { "branch1", "branch2", "branch3" });
    }

    [Fact]
    public async Task ExecuteAsync_JoinDirection_ReturnsSuccess()
    {
        var gateway = new ParallelGatewayActivity
        {
            Id = "parallel1",
            Name = "Parallel Join",
            Direction = GatewayDirection.Join
        };

        var result = await gateway.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.ParallelNextActivityIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_SplitWithNoPaths_ReturnsSuccess()
    {
        var gateway = new ParallelGatewayActivity
        {
            Id = "parallel1",
            Name = "Parallel Split",
            Direction = GatewayDirection.Split,
            OutgoingPaths = null
        };

        var result = await gateway.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
    }
}

public class InclusiveGatewayActivityTests
{
    private readonly Mock<IWorkflowContext> _contextMock;

    public InclusiveGatewayActivityTests()
    {
        _contextMock = new Mock<IWorkflowContext>();
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>());
        _contextMock.Setup(c => c.Input).Returns(new Dictionary<string, object?>());
    }

    [Fact]
    public async Task ExecuteAsync_MultipleMatchingConditions_ReturnsParallelPaths()
    {
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>
        {
            ["amount"] = 150,
            ["priority"] = "high"
        });

        var gateway = new InclusiveGatewayActivity
        {
            Id = "inclusive1",
            Name = "Inclusive Gateway",
            Conditions = new Dictionary<string, string>
            {
                ["amount_path"] = "var.amount >= 100",
                ["priority_path"] = "var.priority == 'high'",
                ["low_path"] = "var.amount < 50"
            },
            DefaultPath = "default_path"
        };

        var result = await gateway.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.ParallelNextActivityIds.Should().NotBeNull();
        result.ParallelNextActivityIds.Should().HaveCount(2);
        result.ParallelNextActivityIds.Should().Contain("amount_path");
        result.ParallelNextActivityIds.Should().Contain("priority_path");
    }

    [Fact]
    public async Task ExecuteAsync_SingleMatchingCondition_ReturnsSinglePath()
    {
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>
        {
            ["amount"] = 150
        });

        var gateway = new InclusiveGatewayActivity
        {
            Id = "inclusive1",
            Name = "Inclusive Gateway",
            Conditions = new Dictionary<string, string>
            {
                ["amount_path"] = "var.amount >= 100",
                ["low_path"] = "var.amount < 50"
            },
            DefaultPath = "default_path"
        };

        var result = await gateway.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.NextActivityId.Should().Be("amount_path");
    }

    [Fact]
    public async Task ExecuteAsync_NoMatchingConditions_ReturnsDefaultPath()
    {
        _contextMock.Setup(c => c.Variables).Returns(new Dictionary<string, object?>
        {
            ["amount"] = 75 // Between 50 and 100, no match
        });

        var gateway = new InclusiveGatewayActivity
        {
            Id = "inclusive1",
            Name = "Inclusive Gateway",
            Conditions = new Dictionary<string, string>
            {
                ["high_path"] = "var.amount >= 100",
                ["low_path"] = "var.amount < 50"
            },
            DefaultPath = "default_path"
        };

        var result = await gateway.ExecuteAsync(_contextMock.Object);

        result.IsSuccess.Should().BeTrue();
        result.NextActivityId.Should().Be("default_path");
    }
}
