namespace XenonClinic.WorkflowEngine.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;
using XenonClinic.WorkflowEngine.Models.Designer;
using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Core.Activities;
using XenonClinic.WorkflowEngine.Models.Definitions;

/// <summary>
/// Serializes and deserializes workflow definitions.
/// </summary>
public class WorkflowSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Serializes a workflow design model to JSON
    /// </summary>
    public static string SerializeDesign(WorkflowDesignModel model)
    {
        return JsonSerializer.Serialize(model, _options);
    }

    /// <summary>
    /// Deserializes JSON to a workflow design model
    /// </summary>
    public static WorkflowDesignModel? DeserializeDesign(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<WorkflowDesignModel>(json, _options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a designer model to an executable workflow definition
    /// </summary>
    public static WorkflowDefinitionModel ToExecutableDefinition(WorkflowDesignModel design)
    {
        var definition = new WorkflowDefinitionModel
        {
            Id = design.Id,
            Name = design.Name,
            Description = design.Description,
            Version = design.Version,
            Category = design.Category,
            Tags = design.Tags ?? new List<string>(),
            IsActive = design.IsActive,
            IsDraft = design.IsDraft,
            TenantId = design.TenantId,
            CreatedAt = design.CreatedAt,
            ModifiedAt = design.ModifiedAt,
            Metadata = design.Metadata ?? new Dictionary<string, object?>()
        };

        var nodes = design.Nodes ?? new List<DesignerNode>();
        var edges = design.Edges ?? new List<DesignerEdge>();

        // Find start node
        var startNode = nodes.FirstOrDefault(n => n.IsStart || n.Type == "start");
        if (startNode == null)
        {
            throw new InvalidOperationException("Workflow must have a start node");
        }
        definition.StartActivityId = startNode.Id;

        // Build gateway conditions from edges (edges define the conditions for gateway paths)
        var gatewayConditions = new Dictionary<string, Dictionary<string, string>>();
        var gatewayDefaultPaths = new Dictionary<string, string>();
        var parallelGatewayPaths = new Dictionary<string, List<string>>();

        foreach (var edge in edges)
        {
            var sourceNode = nodes.FirstOrDefault(n => n.Id == edge.Source);
            var sourceNodeType = sourceNode?.Type ?? string.Empty;
            if (sourceNode != null && IsGatewayType(sourceNodeType))
            {
                if (!gatewayConditions.ContainsKey(edge.Source))
                {
                    gatewayConditions[edge.Source] = new Dictionary<string, string>();
                }

                if (!string.IsNullOrEmpty(edge.Condition))
                {
                    gatewayConditions[edge.Source][edge.Target] = edge.Condition;
                }

                if (edge.IsDefault)
                {
                    gatewayDefaultPaths[edge.Source] = edge.Target;
                }

                // Track all paths for parallel gateways
                if (sourceNodeType.Equals("parallelgateway", StringComparison.OrdinalIgnoreCase))
                {
                    if (!parallelGatewayPaths.ContainsKey(edge.Source))
                    {
                        parallelGatewayPaths[edge.Source] = new List<string>();
                    }
                    parallelGatewayPaths[edge.Source].Add(edge.Target);
                }
            }
        }

        // Convert nodes to activities (passing gateway conditions)
        foreach (var node in nodes)
        {
            var conditions = gatewayConditions.GetValueOrDefault(node.Id);
            var defaultPath = gatewayDefaultPaths.GetValueOrDefault(node.Id);
            var parallelPaths = parallelGatewayPaths.GetValueOrDefault(node.Id);
            var activity = ConvertNodeToActivity(node, conditions, defaultPath, parallelPaths);
            definition.Activities[node.Id] = activity;
        }

        // Convert edges to transitions
        foreach (var edge in edges)
        {
            var transition = new TransitionModel
            {
                Id = edge.Id,
                SourceActivityId = edge.Source,
                TargetActivityId = edge.Target,
                Name = edge.Label,
                Condition = edge.Condition,
                IsDefault = edge.IsDefault,
                Priority = edge.Priority
            };
            definition.Transitions.Add(transition);
        }

        // Convert parameters
        definition.InputParameters = (design.InputParameters ?? new List<ParameterDefinition>()).Select(p => new WorkflowParameter
        {
            Name = p.Name,
            DisplayName = p.DisplayName,
            Description = p.Description,
            Type = p.Type,
            IsRequired = p.IsRequired,
            DefaultValue = p.DefaultValue,
            Schema = p.ValidationSchema
        }).ToList();

        definition.OutputParameters = (design.OutputParameters ?? new List<ParameterDefinition>()).Select(p => new WorkflowParameter
        {
            Name = p.Name,
            DisplayName = p.DisplayName,
            Description = p.Description,
            Type = p.Type,
            IsRequired = p.IsRequired,
            DefaultValue = p.DefaultValue,
            Schema = p.ValidationSchema
        }).ToList();

        // Convert variables
        definition.Variables = (design.Variables ?? new List<VariableDefinition>()).Select(v => new WorkflowVariable
        {
            Name = v.Name,
            Type = v.Type,
            DefaultValue = v.DefaultValue,
            Scope = v.Scope == "local" ? VariableScope.Activity : VariableScope.Workflow
        }).ToList();

        // Convert triggers
        definition.Triggers = (design.Triggers ?? new List<TriggerDefinition>()).Select(t => new WorkflowTrigger
        {
            Type = Enum.TryParse<TriggerType>(t.Type ?? "Manual", ignoreCase: true, out var triggerType) ? triggerType : TriggerType.Manual,
            Name = t.Name,
            IsEnabled = t.IsEnabled,
            Configuration = t.Config ?? new Dictionary<string, object?>()
        }).ToList();

        // Convert error handlers
        definition.ErrorHandlers = (design.ErrorHandlers ?? new List<ErrorHandlerDefinition>()).Select(h => new ErrorHandler
        {
            ErrorCodes = h.ErrorCodes ?? new List<string>(),
            HandlerActivityId = h.HandlerNodeId,
            Compensate = h.Compensate,
            Terminate = h.Terminate,
            RetryPolicy = h.Retry != null ? new RetryPolicy
            {
                MaxRetries = h.Retry.MaxRetries,
                InitialDelay = TimeSpan.FromMilliseconds(h.Retry.InitialDelayMs),
                MaxDelay = TimeSpan.FromMilliseconds(h.Retry.MaxDelayMs),
                BackoffMultiplier = h.Retry.BackoffMultiplier
            } : null
        }).ToList();

        return definition;
    }

    /// <summary>
    /// Converts an executable definition back to a designer model
    /// </summary>
    public static WorkflowDesignModel ToDesignModel(WorkflowDefinitionModel definition, Dictionary<string, NodePosition>? positions = null)
    {
        var design = new WorkflowDesignModel
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            Version = definition.Version,
            Category = definition.Category,
            Tags = definition.Tags?.ToList() ?? new List<string>(),
            IsActive = definition.IsActive,
            IsDraft = definition.IsDraft,
            TenantId = definition.TenantId,
            CreatedAt = definition.CreatedAt,
            ModifiedAt = definition.ModifiedAt,
            Metadata = definition.Metadata ?? new Dictionary<string, object?>()
        };

        // Convert activities to nodes
        int row = 0;
        foreach (var (activityId, activity) in definition.Activities)
        {
            var position = positions?.GetValueOrDefault(activityId) ?? new NodePosition
            {
                X = 250,
                Y = 100 + (row++ * 100)
            };

            var node = ConvertActivityToNode(activityId, activity, position);
            if (activityId == definition.StartActivityId)
            {
                node.IsStart = true;
            }
            design.Nodes.Add(node);
        }

        // Convert transitions to edges
        foreach (var transition in definition.Transitions)
        {
            design.Edges.Add(new DesignerEdge
            {
                Id = transition.Id,
                Source = transition.SourceActivityId,
                Target = transition.TargetActivityId,
                Label = transition.Name,
                Condition = transition.Condition,
                IsDefault = transition.IsDefault,
                Priority = transition.Priority
            });
        }

        // Convert parameters
        design.InputParameters = definition.InputParameters?.Select(p => new ParameterDefinition
        {
            Name = p.Name,
            DisplayName = p.DisplayName,
            Description = p.Description,
            Type = p.Type,
            IsRequired = p.IsRequired,
            DefaultValue = p.DefaultValue,
            ValidationSchema = p.Schema
        }).ToList() ?? new List<ParameterDefinition>();

        design.OutputParameters = definition.OutputParameters?.Select(p => new ParameterDefinition
        {
            Name = p.Name,
            DisplayName = p.DisplayName,
            Description = p.Description,
            Type = p.Type,
            IsRequired = p.IsRequired,
            DefaultValue = p.DefaultValue,
            ValidationSchema = p.Schema
        }).ToList() ?? new List<ParameterDefinition>();

        // Convert variables
        design.Variables = definition.Variables?.Select(v => new VariableDefinition
        {
            Name = v.Name,
            Type = v.Type,
            DefaultValue = v.DefaultValue,
            Scope = v.Scope == VariableScope.Activity ? "local" : "workflow"
        }).ToList() ?? new List<VariableDefinition>();

        // Convert triggers
        design.Triggers = definition.Triggers?.Select(t => new TriggerDefinition
        {
            Type = t.Type.ToString().ToLowerInvariant(),
            Name = t.Name,
            IsEnabled = t.IsEnabled,
            Config = t.Configuration ?? new Dictionary<string, object?>()
        }).ToList() ?? new List<TriggerDefinition>();

        return design;
    }

    private static bool IsGatewayType(string? type)
    {
        if (string.IsNullOrEmpty(type)) return false;
        var lowerType = type.ToLowerInvariant();
        return lowerType is "exclusivegateway" or "parallelgateway" or "inclusivegateway";
    }

    private static IActivity ConvertNodeToActivity(
        DesignerNode node,
        Dictionary<string, string>? conditions = null,
        string? defaultPath = null,
        List<string>? parallelPaths = null)
    {
        var nodeType = node.Type ?? "task";
        var config = node.Config ?? new Dictionary<string, object?>();
        var inputMappings = node.InputMappings ?? new Dictionary<string, string>();
        var outputMappings = node.OutputMappings ?? new Dictionary<string, string>();

        return nodeType.ToLowerInvariant() switch
        {
            "start" => new StartActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description
            },
            "end" => new EndActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                FinalOutputMappings = outputMappings.Count > 0 ? outputMappings : null
            },
            "task" => new TaskActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                TaskHandler = config.GetValueOrDefault("taskHandler")?.ToString(),
                Parameters = GetDictionaryConfig(config, "parameters"),
                InputMappings = inputMappings,
                OutputMappings = outputMappings
            },
            "servicetask" => new ServiceTaskActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                ServiceType = config.GetValueOrDefault("serviceType")?.ToString() ?? "",
                MethodName = config.GetValueOrDefault("methodName")?.ToString() ?? "",
                CompensationMethod = config.GetValueOrDefault("compensationMethod")?.ToString(),
                InputMappings = inputMappings,
                OutputMappings = outputMappings
            },
            "usertask" => new UserTaskActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                Assignee = config.GetValueOrDefault("assignee")?.ToString(),
                Priority = int.TryParse(config.GetValueOrDefault("priority")?.ToString(), out var p) ? p : 3,
                InputMappings = inputMappings,
                OutputMappings = outputMappings
            },
            "script" => new ScriptActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                Language = config.GetValueOrDefault("language")?.ToString() ?? "expression",
                Script = config.GetValueOrDefault("script")?.ToString() ?? ""
            },
            "exclusivegateway" => new ExclusiveGatewayActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                Conditions = conditions ?? new Dictionary<string, string>(),
                DefaultPath = defaultPath ?? config.GetValueOrDefault("defaultPath")?.ToString()
            },
            "parallelgateway" => new ParallelGatewayActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                Direction = config.GetValueOrDefault("direction")?.ToString() == "join"
                    ? GatewayDirection.Join
                    : GatewayDirection.Split,
                OutgoingPaths = parallelPaths
            },
            "inclusivegateway" => new InclusiveGatewayActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                Conditions = conditions ?? new Dictionary<string, string>(),
                DefaultPath = defaultPath ?? config.GetValueOrDefault("defaultPath")?.ToString()
            },
            "timer" => new TimerActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                Duration = config.GetValueOrDefault("duration")?.ToString(),
                DateTime = config.GetValueOrDefault("dateTime")?.ToString(),
                Cron = config.GetValueOrDefault("cron")?.ToString()
            },
            "signalreceive" => new SignalReceiveActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                SignalName = config.GetValueOrDefault("signalName")?.ToString() ?? "",
                OutputMappings = outputMappings
            },
            "signalthrow" => new SignalThrowActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                SignalName = config.GetValueOrDefault("signalName")?.ToString() ?? "",
                PayloadExpression = config.GetValueOrDefault("payload")?.ToString()
            },
            "subprocess" => new SubProcessActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                SubWorkflowId = config.GetValueOrDefault("subWorkflowId")?.ToString() ?? "",
                WaitForCompletion = config.GetValueOrDefault("waitForCompletion") is not false,
                InputMappings = inputMappings,
                OutputMappings = outputMappings
            },
            _ => new TaskActivity
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                InputMappings = inputMappings,
                OutputMappings = outputMappings
            }
        };
    }

    private static DesignerNode ConvertActivityToNode(string id, IActivity activity, NodePosition position)
    {
        var node = new DesignerNode
        {
            Id = id,
            Type = activity.Type,
            Name = activity.Name,
            Description = activity.Description,
            Position = position,
            IsEnd = activity.Type == "end"
        };

        // Extract configuration based on activity type
        if (activity is TaskActivity taskActivity)
        {
            if (taskActivity.TaskHandler != null)
                node.Config["taskHandler"] = taskActivity.TaskHandler;
            if (taskActivity.Parameters != null)
                node.Config["parameters"] = taskActivity.Parameters;
            if (taskActivity.InputMappings != null)
                node.InputMappings = new Dictionary<string, string>(taskActivity.InputMappings);
            if (taskActivity.OutputMappings != null)
                node.OutputMappings = new Dictionary<string, string>(taskActivity.OutputMappings);
        }
        else if (activity is ServiceTaskActivity serviceTask)
        {
            node.Config["serviceType"] = serviceTask.ServiceType;
            node.Config["methodName"] = serviceTask.MethodName;
            if (serviceTask.CompensationMethod != null)
                node.Config["compensationMethod"] = serviceTask.CompensationMethod;
        }
        else if (activity is UserTaskActivity userTask)
        {
            if (userTask.Assignee != null)
                node.Config["assignee"] = userTask.Assignee;
            node.Config["priority"] = userTask.Priority;
        }
        else if (activity is TimerActivity timer)
        {
            if (timer.Duration != null) node.Config["duration"] = timer.Duration;
            if (timer.DateTime != null) node.Config["dateTime"] = timer.DateTime;
            if (timer.Cron != null) node.Config["cron"] = timer.Cron;
        }
        else if (activity is ParallelGatewayActivity parallel)
        {
            node.Config["direction"] = parallel.Direction.ToString().ToLowerInvariant();
        }
        else if (activity is SubProcessActivity subprocess)
        {
            node.Config["subWorkflowId"] = subprocess.SubWorkflowId;
            node.Config["waitForCompletion"] = subprocess.WaitForCompletion;
        }

        return node;
    }

    private static IDictionary<string, object?>? GetDictionaryConfig(Dictionary<string, object?> config, string key)
    {
        if (config.TryGetValue(key, out var value) && value is JsonElement element)
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object?>>(element.GetRawText());
            }
            catch (JsonException)
            {
                return null;
            }
        }
        return value as IDictionary<string, object?>;
    }
}
