namespace XenonClinic.WorkflowEngine.Tests.Serialization;

using FluentAssertions;
using XenonClinic.WorkflowEngine.Models.Designer;
using XenonClinic.WorkflowEngine.Serialization;
using XenonClinic.WorkflowEngine.Core.Activities;
using Xunit;

public class WorkflowSerializerTests
{
    [Fact]
    public void ToExecutableDefinition_WithExclusiveGateway_PopulatesConditionsFromEdges()
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
                    Id = "gateway1",
                    Type = "exclusiveGateway",
                    Name = "Decision Gateway",
                    Position = new NodePosition { X = 200, Y = 100 }
                },
                new()
                {
                    Id = "task_high",
                    Type = "task",
                    Name = "High Value Task",
                    Position = new NodePosition { X = 300, Y = 50 }
                },
                new()
                {
                    Id = "task_low",
                    Type = "task",
                    Name = "Low Value Task",
                    Position = new NodePosition { X = 300, Y = 150 }
                },
                new()
                {
                    Id = "end",
                    Type = "end",
                    Name = "End",
                    IsEnd = true,
                    Position = new NodePosition { X = 400, Y = 100 }
                }
            },
            Edges = new List<DesignerEdge>
            {
                new()
                {
                    Id = "edge1",
                    Source = "start",
                    Target = "gateway1"
                },
                new()
                {
                    Id = "edge2",
                    Source = "gateway1",
                    Target = "task_high",
                    Condition = "var.amount >= 100"
                },
                new()
                {
                    Id = "edge3",
                    Source = "gateway1",
                    Target = "task_low",
                    Condition = "var.amount < 100",
                    IsDefault = true
                },
                new()
                {
                    Id = "edge4",
                    Source = "task_high",
                    Target = "end"
                },
                new()
                {
                    Id = "edge5",
                    Source = "task_low",
                    Target = "end"
                }
            }
        };

        var definition = WorkflowSerializer.ToExecutableDefinition(design);

        definition.Activities.Should().ContainKey("gateway1");
        var gateway = definition.Activities["gateway1"] as ExclusiveGatewayActivity;
        gateway.Should().NotBeNull();
        gateway!.Conditions.Should().ContainKey("task_high");
        gateway.Conditions["task_high"].Should().Be("var.amount >= 100");
        gateway.Conditions.Should().ContainKey("task_low");
        gateway.Conditions["task_low"].Should().Be("var.amount < 100");
        gateway.DefaultPath.Should().Be("task_low");
    }

    [Fact]
    public void ToExecutableDefinition_WithParallelGateway_PopulatesOutgoingPaths()
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
                    Id = "parallel_split",
                    Type = "parallelGateway",
                    Name = "Parallel Split",
                    Config = new Dictionary<string, object?> { ["direction"] = "split" },
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
                },
                new()
                {
                    Id = "parallel_join",
                    Type = "parallelGateway",
                    Name = "Parallel Join",
                    Config = new Dictionary<string, object?> { ["direction"] = "join" },
                    Position = new NodePosition { X = 400, Y = 100 }
                }
            },
            Edges = new List<DesignerEdge>
            {
                new() { Id = "e1", Source = "start", Target = "parallel_split" },
                new() { Id = "e2", Source = "parallel_split", Target = "task1" },
                new() { Id = "e3", Source = "parallel_split", Target = "task2" },
                new() { Id = "e4", Source = "task1", Target = "parallel_join" },
                new() { Id = "e5", Source = "task2", Target = "parallel_join" }
            }
        };

        var definition = WorkflowSerializer.ToExecutableDefinition(design);

        var parallelGateway = definition.Activities["parallel_split"] as ParallelGatewayActivity;
        parallelGateway.Should().NotBeNull();
        parallelGateway!.Direction.Should().Be(GatewayDirection.Split);
        parallelGateway.OutgoingPaths.Should().NotBeNull();
        parallelGateway.OutgoingPaths.Should().HaveCount(2);
        parallelGateway.OutgoingPaths.Should().Contain("task1");
        parallelGateway.OutgoingPaths.Should().Contain("task2");
    }

    [Fact]
    public void ToExecutableDefinition_WithInclusiveGateway_PopulatesConditionsFromEdges()
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
                    Id = "gateway1",
                    Type = "inclusiveGateway",
                    Name = "Inclusive Gateway",
                    Position = new NodePosition { X = 200, Y = 100 }
                },
                new()
                {
                    Id = "email_task",
                    Type = "task",
                    Name = "Send Email",
                    Position = new NodePosition { X = 300, Y = 50 }
                },
                new()
                {
                    Id = "sms_task",
                    Type = "task",
                    Name = "Send SMS",
                    Position = new NodePosition { X = 300, Y = 150 }
                }
            },
            Edges = new List<DesignerEdge>
            {
                new() { Id = "e1", Source = "start", Target = "gateway1" },
                new()
                {
                    Id = "e2",
                    Source = "gateway1",
                    Target = "email_task",
                    Condition = "var.sendEmail == true"
                },
                new()
                {
                    Id = "e3",
                    Source = "gateway1",
                    Target = "sms_task",
                    Condition = "var.sendSms == true"
                }
            }
        };

        var definition = WorkflowSerializer.ToExecutableDefinition(design);

        var gateway = definition.Activities["gateway1"] as InclusiveGatewayActivity;
        gateway.Should().NotBeNull();
        gateway!.Conditions.Should().HaveCount(2);
        gateway.Conditions["email_task"].Should().Be("var.sendEmail == true");
        gateway.Conditions["sms_task"].Should().Be("var.sendSms == true");
    }

    [Fact]
    public void SerializeDesign_AndDeserialize_RoundTripsCorrectly()
    {
        var design = new WorkflowDesignModel
        {
            Id = "workflow1",
            Name = "Test Workflow",
            Description = "A test workflow",
            Version = 1,
            Category = "Testing",
            Tags = new List<string> { "test", "demo" },
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
                new() { Id = "e1", Source = "start", Target = "end" }
            },
            CreatedAt = DateTime.UtcNow
        };

        var json = WorkflowSerializer.SerializeDesign(design);
        var deserialized = WorkflowSerializer.DeserializeDesign(json);

        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(design.Id);
        deserialized.Name.Should().Be(design.Name);
        deserialized.Description.Should().Be(design.Description);
        deserialized.Nodes.Should().HaveCount(2);
        deserialized.Edges.Should().HaveCount(1);
    }

    [Fact]
    public void ToExecutableDefinition_WithoutStartNode_ThrowsException()
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
                    Name = "Task",
                    Position = new NodePosition { X = 100, Y = 100 }
                }
            }
        };

        var act = () => WorkflowSerializer.ToExecutableDefinition(design);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*start node*");
    }

    [Fact]
    public void ToExecutableDefinition_WithUserTask_SetsCorrectProperties()
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
                    Id = "userTask1",
                    Type = "userTask",
                    Name = "Review Task",
                    Config = new Dictionary<string, object?>
                    {
                        ["assignee"] = "john.doe",
                        ["priority"] = "5"
                    },
                    Position = new NodePosition { X = 200, Y = 100 }
                }
            },
            Edges = new List<DesignerEdge>
            {
                new() { Id = "e1", Source = "start", Target = "userTask1" }
            }
        };

        var definition = WorkflowSerializer.ToExecutableDefinition(design);

        var userTask = definition.Activities["userTask1"] as UserTaskActivity;
        userTask.Should().NotBeNull();
        userTask!.Assignee.Should().Be("john.doe");
        userTask.Priority.Should().Be(5);
    }
}
