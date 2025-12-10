namespace XenonClinic.WorkflowEngine.Tests.Validation;

using FluentAssertions;
using XenonClinic.WorkflowEngine.Models.Designer;
using XenonClinic.WorkflowEngine.Validation;
using Xunit;

public class WorkflowValidatorTests
{
    private readonly WorkflowValidator _validator;

    public WorkflowValidatorTests()
    {
        _validator = new WorkflowValidator();
    }

    [Fact]
    public void Validate_ValidWorkflow_ReturnsNoErrors()
    {
        var design = CreateValidWorkflow();

        var result = _validator.Validate(design);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MissingName_ReturnsError()
    {
        var design = CreateValidWorkflow();
        design.Name = "";

        var result = _validator.Validate(design);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "MISSING_NAME");
    }

    [Fact]
    public void Validate_MissingStartNode_ReturnsError()
    {
        var design = new WorkflowDesignModel
        {
            Id = "workflow1",
            Name = "Test Workflow",
            Version = 1,
            Nodes = new List<DesignerNode>
            {
                new()
                {
                    Id = "task1",
                    Type = "task",
                    Name = "Task 1",
                    Position = new NodePosition { X = 100, Y = 100 }
                }
            }
        };

        var result = _validator.Validate(design);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "MISSING_START_NODE");
    }

    [Fact]
    public void Validate_MultipleStartNodes_ReturnsError()
    {
        var design = new WorkflowDesignModel
        {
            Id = "workflow1",
            Name = "Test Workflow",
            Version = 1,
            Nodes = new List<DesignerNode>
            {
                new()
                {
                    Id = "start1",
                    Type = "start",
                    Name = "Start 1",
                    IsStart = true,
                    Position = new NodePosition { X = 100, Y = 100 }
                },
                new()
                {
                    Id = "start2",
                    Type = "start",
                    Name = "Start 2",
                    IsStart = true,
                    Position = new NodePosition { X = 200, Y = 100 }
                }
            }
        };

        var result = _validator.Validate(design);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "MULTIPLE_START_NODES");
    }

    [Fact]
    public void Validate_DuplicateNodeId_ReturnsError()
    {
        var design = new WorkflowDesignModel
        {
            Id = "workflow1",
            Name = "Test Workflow",
            Version = 1,
            Nodes = new List<DesignerNode>
            {
                new()
                {
                    Id = "start",
                    Type = "start",
                    Name = "Start",
                    IsStart = true,
                    Position = new NodePosition { X = 100, Y = 100 }
                },
                new()
                {
                    Id = "start", // Duplicate!
                    Type = "task",
                    Name = "Task",
                    Position = new NodePosition { X = 200, Y = 100 }
                }
            }
        };

        var result = _validator.Validate(design);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == "DUPLICATE_NODE_ID");
    }

    [Fact]
    public void Validate_ServiceTaskWithoutServiceType_ReturnsError()
    {
        var design = CreateValidWorkflow();
        design.Nodes.Add(new DesignerNode
        {
            Id = "service1",
            Type = "serviceTask",
            Name = "Service Task",
            Config = new Dictionary<string, object?>(),
            Position = new NodePosition { X = 200, Y = 100 }
        });

        var result = _validator.Validate(design);

        result.Errors.Should().Contain(e => e.Code == "MISSING_SERVICE_TYPE");
        result.Errors.Should().Contain(e => e.Code == "MISSING_METHOD_NAME");
    }

    [Fact]
    public void Validate_SubProcessWithoutWorkflowId_ReturnsError()
    {
        var design = CreateValidWorkflow();
        design.Nodes.Add(new DesignerNode
        {
            Id = "subprocess1",
            Type = "subProcess",
            Name = "Sub Process",
            Config = new Dictionary<string, object?>(),
            Position = new NodePosition { X = 200, Y = 100 }
        });

        var result = _validator.Validate(design);

        result.Errors.Should().Contain(e => e.Code == "MISSING_SUBPROCESS_ID");
    }

    [Fact]
    public void Validate_SignalWithoutSignalName_ReturnsError()
    {
        var design = CreateValidWorkflow();
        design.Nodes.Add(new DesignerNode
        {
            Id = "signal1",
            Type = "signalReceive",
            Name = "Signal Receive",
            Config = new Dictionary<string, object?>(),
            Position = new NodePosition { X = 200, Y = 100 }
        });

        var result = _validator.Validate(design);

        result.Errors.Should().Contain(e => e.Code == "MISSING_SIGNAL_NAME");
    }

    [Fact]
    public void Validate_InvalidEdgeSource_ReturnsError()
    {
        var design = CreateValidWorkflow();
        design.Edges.Add(new DesignerEdge
        {
            Id = "edge2",
            Source = "nonexistent",
            Target = "end"
        });

        var result = _validator.Validate(design);

        result.Errors.Should().Contain(e => e.Code == "INVALID_EDGE_SOURCE");
    }

    [Fact]
    public void Validate_InvalidEdgeTarget_ReturnsError()
    {
        var design = CreateValidWorkflow();
        design.Edges.Add(new DesignerEdge
        {
            Id = "edge2",
            Source = "start",
            Target = "nonexistent"
        });

        var result = _validator.Validate(design);

        result.Errors.Should().Contain(e => e.Code == "INVALID_EDGE_TARGET");
    }

    [Fact]
    public void Validate_SelfLoop_ReturnsWarning()
    {
        var design = CreateValidWorkflow();
        design.Edges.Add(new DesignerEdge
        {
            Id = "edge2",
            Source = "start",
            Target = "start"
        });

        var result = _validator.Validate(design);

        result.Warnings.Should().Contain(w => w.Code == "SELF_LOOP");
    }

    [Fact]
    public void Validate_UnreachableNode_ReturnsWarning()
    {
        var design = CreateValidWorkflow();
        design.Nodes.Add(new DesignerNode
        {
            Id = "isolated",
            Type = "task",
            Name = "Isolated Task",
            Position = new NodePosition { X = 500, Y = 500 }
        });

        var result = _validator.Validate(design);

        result.Warnings.Should().Contain(w => w.Code == "UNREACHABLE_NODE");
    }

    [Fact]
    public void Validate_DuplicateParameterName_ReturnsError()
    {
        var design = CreateValidWorkflow();
        design.InputParameters = new List<ParameterDefinition>
        {
            new() { Name = "param1", Type = "string" },
            new() { Name = "param1", Type = "int" } // Duplicate
        };

        var result = _validator.Validate(design);

        result.Errors.Should().Contain(e => e.Code == "DUPLICATE_PARAMETER_NAME");
    }

    [Fact]
    public void Validate_ScheduledTriggerWithoutCron_ReturnsError()
    {
        var design = CreateValidWorkflow();
        design.Triggers = new List<TriggerDefinition>
        {
            new()
            {
                Type = "scheduled",
                Name = "Daily Trigger",
                IsEnabled = true,
                Config = new Dictionary<string, object?>()
            }
        };

        var result = _validator.Validate(design);

        result.Errors.Should().Contain(e => e.Code == "MISSING_CRON");
    }

    [Fact]
    public void Validate_WebhookTriggerWithoutPath_ReturnsError()
    {
        var design = CreateValidWorkflow();
        design.Triggers = new List<TriggerDefinition>
        {
            new()
            {
                Type = "webhook",
                Name = "Webhook Trigger",
                IsEnabled = true,
                Config = new Dictionary<string, object?>()
            }
        };

        var result = _validator.Validate(design);

        result.Errors.Should().Contain(e => e.Code == "MISSING_WEBHOOK_PATH");
    }

    [Fact]
    public void Validate_NoEndNode_ReturnsWarning()
    {
        var design = new WorkflowDesignModel
        {
            Id = "workflow1",
            Name = "Test Workflow",
            Version = 1,
            Nodes = new List<DesignerNode>
            {
                new()
                {
                    Id = "start",
                    Type = "start",
                    Name = "Start",
                    IsStart = true,
                    Position = new NodePosition { X = 100, Y = 100 }
                },
                new()
                {
                    Id = "task1",
                    Type = "task",
                    Name = "Task",
                    Position = new NodePosition { X = 200, Y = 100 }
                }
            },
            Edges = new List<DesignerEdge>
            {
                new() { Id = "e1", Source = "start", Target = "task1" }
            }
        };

        var result = _validator.Validate(design);

        result.Warnings.Should().Contain(w => w.Code == "NO_END_NODE");
    }

    [Fact]
    public void Validate_ExclusiveGatewayWithoutConditions_ReturnsWarning()
    {
        var design = new WorkflowDesignModel
        {
            Id = "workflow1",
            Name = "Test Workflow",
            Version = 1,
            Nodes = new List<DesignerNode>
            {
                new()
                {
                    Id = "start",
                    Type = "start",
                    Name = "Start",
                    IsStart = true,
                    Position = new NodePosition { X = 100, Y = 100 }
                },
                new()
                {
                    Id = "gateway",
                    Type = "exclusiveGateway",
                    Name = "Decision",
                    Position = new NodePosition { X = 200, Y = 100 }
                },
                new()
                {
                    Id = "task1",
                    Type = "task",
                    Name = "Task 1",
                    Position = new NodePosition { X = 300, Y = 50 }
                },
                new()
                {
                    Id = "task2",
                    Type = "task",
                    Name = "Task 2",
                    Position = new NodePosition { X = 300, Y = 150 }
                }
            },
            Edges = new List<DesignerEdge>
            {
                new() { Id = "e1", Source = "start", Target = "gateway" },
                new() { Id = "e2", Source = "gateway", Target = "task1" }, // No condition
                new() { Id = "e3", Source = "gateway", Target = "task2" }  // No condition
            }
        };

        var result = _validator.Validate(design);

        result.Warnings.Should().Contain(w => w.Code == "GATEWAY_NO_CONDITIONS");
    }

    private static WorkflowDesignModel CreateValidWorkflow()
    {
        return new WorkflowDesignModel
        {
            Id = "workflow1",
            Name = "Test Workflow",
            Version = 1,
            Nodes = new List<DesignerNode>
            {
                new()
                {
                    Id = "start",
                    Type = "start",
                    Name = "Start",
                    IsStart = true,
                    Position = new NodePosition { X = 100, Y = 100 }
                },
                new()
                {
                    Id = "end",
                    Type = "end",
                    Name = "End",
                    IsEnd = true,
                    Position = new NodePosition { X = 300, Y = 100 }
                }
            },
            Edges = new List<DesignerEdge>
            {
                new()
                {
                    Id = "edge1",
                    Source = "start",
                    Target = "end"
                }
            }
        };
    }
}
