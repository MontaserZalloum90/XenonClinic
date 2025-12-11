namespace XenonClinic.WorkflowEngine.Validation;

using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Models.Designer;

/// <summary>
/// Interface for workflow validation.
/// </summary>
public interface IWorkflowValidator
{
    /// <summary>
    /// Validates a workflow design model
    /// </summary>
    WorkflowValidationResult Validate(WorkflowDesignModel design);

    /// <summary>
    /// Validates a workflow definition
    /// </summary>
    ValidationResult Validate(IWorkflowDefinition definition);
}

/// <summary>
/// Result of workflow validation
/// </summary>
public class WorkflowValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<WorkflowValidationError> Errors { get; init; } = new();
    public List<WorkflowValidationWarning> Warnings { get; init; } = new();
}

public class WorkflowValidationError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? NodeId { get; init; }
    public string? EdgeId { get; init; }
    public string? PropertyPath { get; init; }
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;
}

public class WorkflowValidationWarning
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? NodeId { get; init; }
    public string? EdgeId { get; init; }
    public string? PropertyPath { get; init; }
}

public enum ValidationSeverity
{
    Warning,
    Error,
    Critical
}

/// <summary>
/// Default workflow validator implementation.
/// </summary>
public class WorkflowValidator : IWorkflowValidator
{
    public WorkflowValidationResult Validate(WorkflowDesignModel design)
    {
        var errors = new List<WorkflowValidationError>();
        var warnings = new List<WorkflowValidationWarning>();

        // Validate basic properties
        if (string.IsNullOrWhiteSpace(design.Name))
        {
            errors.Add(new WorkflowValidationError
            {
                Code = "MISSING_NAME",
                Message = "Workflow name is required",
                PropertyPath = "name"
            });
        }

        // Validate nodes
        ValidateNodes(design, errors, warnings);

        // Validate edges
        ValidateEdges(design, errors, warnings);

        // Validate flow structure
        ValidateFlowStructure(design, errors, warnings);

        // Validate parameters
        ValidateParameters(design, errors, warnings);

        // Validate triggers
        ValidateTriggers(design, errors, warnings);

        return new WorkflowValidationResult { Errors = errors, Warnings = warnings };
    }

    public ValidationResult Validate(IWorkflowDefinition definition)
    {
        return definition.Validate();
    }

    private void ValidateNodes(WorkflowDesignModel design, List<WorkflowValidationError> errors, List<WorkflowValidationWarning> warnings)
    {
        var nodeIds = new HashSet<string>();
        var nodes = design.Nodes ?? new List<DesignerNode>();

        // Check for start node
        var startNodes = nodes.Where(n => n.IsStart || n.Type == "start").ToList();
        if (startNodes.Count == 0)
        {
            errors.Add(new WorkflowValidationError
            {
                Code = "MISSING_START_NODE",
                Message = "Workflow must have exactly one start node",
                Severity = ValidationSeverity.Critical
            });
        }
        else if (startNodes.Count > 1)
        {
            errors.Add(new WorkflowValidationError
            {
                Code = "MULTIPLE_START_NODES",
                Message = "Workflow can only have one start node",
                Severity = ValidationSeverity.Critical
            });
        }

        // Check for end nodes
        var endNodes = nodes.Where(n => n.IsEnd || n.Type == "end").ToList();
        if (endNodes.Count == 0)
        {
            warnings.Add(new WorkflowValidationWarning
            {
                Code = "NO_END_NODE",
                Message = "Workflow has no explicit end node"
            });
        }

        foreach (var node in nodes)
        {
            // Check for duplicate IDs
            if (!nodeIds.Add(node.Id))
            {
                errors.Add(new WorkflowValidationError
                {
                    Code = "DUPLICATE_NODE_ID",
                    Message = $"Duplicate node ID: {node.Id}",
                    NodeId = node.Id
                });
            }

            // Check node name
            if (string.IsNullOrWhiteSpace(node.Name))
            {
                warnings.Add(new WorkflowValidationWarning
                {
                    Code = "MISSING_NODE_NAME",
                    Message = $"Node '{node.Id}' has no name",
                    NodeId = node.Id
                });
            }

            // Validate type-specific requirements
            ValidateNodeByType(node, errors, warnings);
        }
    }

    private void ValidateNodeByType(DesignerNode node, List<WorkflowValidationError> errors, List<WorkflowValidationWarning> warnings)
    {
        var nodeType = node.Type ?? string.Empty;
        var config = node.Config ?? new Dictionary<string, object?>();

        switch (nodeType.ToLowerInvariant())
        {
            case "servicetask":
                if (!config.ContainsKey("serviceType") || string.IsNullOrEmpty(config["serviceType"]?.ToString()))
                {
                    errors.Add(new WorkflowValidationError
                    {
                        Code = "MISSING_SERVICE_TYPE",
                        Message = "Service task requires a service type",
                        NodeId = node.Id,
                        PropertyPath = "config.serviceType"
                    });
                }
                if (!config.ContainsKey("methodName") || string.IsNullOrEmpty(config["methodName"]?.ToString()))
                {
                    errors.Add(new WorkflowValidationError
                    {
                        Code = "MISSING_METHOD_NAME",
                        Message = "Service task requires a method name",
                        NodeId = node.Id,
                        PropertyPath = "config.methodName"
                    });
                }
                break;

            case "subprocess":
                if (!config.ContainsKey("subWorkflowId") || string.IsNullOrEmpty(config["subWorkflowId"]?.ToString()))
                {
                    errors.Add(new WorkflowValidationError
                    {
                        Code = "MISSING_SUBPROCESS_ID",
                        Message = "Sub-process requires a workflow ID",
                        NodeId = node.Id,
                        PropertyPath = "config.subWorkflowId"
                    });
                }
                break;

            case "signalreceive":
            case "signalthrow":
                if (!config.ContainsKey("signalName") || string.IsNullOrEmpty(config["signalName"]?.ToString()))
                {
                    errors.Add(new WorkflowValidationError
                    {
                        Code = "MISSING_SIGNAL_NAME",
                        Message = "Signal activity requires a signal name",
                        NodeId = node.Id,
                        PropertyPath = "config.signalName"
                    });
                }
                break;

            case "script":
                if (!config.ContainsKey("script") || string.IsNullOrEmpty(config["script"]?.ToString()))
                {
                    errors.Add(new WorkflowValidationError
                    {
                        Code = "MISSING_SCRIPT",
                        Message = "Script task requires script content",
                        NodeId = node.Id,
                        PropertyPath = "config.script"
                    });
                }
                break;
        }
    }

    private void ValidateEdges(WorkflowDesignModel design, List<WorkflowValidationError> errors, List<WorkflowValidationWarning> warnings)
    {
        var edgeIds = new HashSet<string>();
        var nodes = design.Nodes ?? new List<DesignerNode>();
        var edges = design.Edges ?? new List<DesignerEdge>();
        var nodeIds = nodes.Select(n => n.Id).ToHashSet();

        foreach (var edge in edges)
        {
            // Check for duplicate edge IDs
            if (!edgeIds.Add(edge.Id))
            {
                errors.Add(new WorkflowValidationError
                {
                    Code = "DUPLICATE_EDGE_ID",
                    Message = $"Duplicate edge ID: {edge.Id}",
                    EdgeId = edge.Id
                });
            }

            // Check source exists
            if (!nodeIds.Contains(edge.Source))
            {
                errors.Add(new WorkflowValidationError
                {
                    Code = "INVALID_EDGE_SOURCE",
                    Message = $"Edge source node not found: {edge.Source}",
                    EdgeId = edge.Id,
                    PropertyPath = "source"
                });
            }

            // Check target exists
            if (!nodeIds.Contains(edge.Target))
            {
                errors.Add(new WorkflowValidationError
                {
                    Code = "INVALID_EDGE_TARGET",
                    Message = $"Edge target node not found: {edge.Target}",
                    EdgeId = edge.Id,
                    PropertyPath = "target"
                });
            }

            // Check for self-loops
            if (edge.Source == edge.Target)
            {
                warnings.Add(new WorkflowValidationWarning
                {
                    Code = "SELF_LOOP",
                    Message = $"Edge creates a self-loop on node {edge.Source}",
                    EdgeId = edge.Id
                });
            }
        }
    }

    private void ValidateFlowStructure(WorkflowDesignModel design, List<WorkflowValidationError> errors, List<WorkflowValidationWarning> warnings)
    {
        var nodes = design.Nodes ?? new List<DesignerNode>();
        var edges = design.Edges ?? new List<DesignerEdge>();
        var nodeIds = nodes.Select(n => n.Id).ToHashSet();
        var edgesBySource = edges.GroupBy(e => e.Source).ToDictionary(g => g.Key, g => g.ToList());
        var edgesByTarget = edges.GroupBy(e => e.Target).ToDictionary(g => g.Key, g => g.ToList());

        // Find start node
        var startNode = nodes.FirstOrDefault(n => n.IsStart || n.Type == "start");
        if (startNode == null) return;

        // Check connectivity - all nodes should be reachable from start
        var reachable = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(startNode.Id);
        reachable.Add(startNode.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (edgesBySource.TryGetValue(current, out var outgoing))
            {
                foreach (var edge in outgoing)
                {
                    if (reachable.Add(edge.Target))
                    {
                        queue.Enqueue(edge.Target);
                    }
                }
            }
        }

        foreach (var node in nodes)
        {
            if (!reachable.Contains(node.Id) && !node.IsStart && node.Type != "start")
            {
                warnings.Add(new WorkflowValidationWarning
                {
                    Code = "UNREACHABLE_NODE",
                    Message = $"Node '{node.Name}' ({node.Id}) is not reachable from start",
                    NodeId = node.Id
                });
            }
        }

        // Check for nodes with no outgoing edges (except end nodes)
        foreach (var node in nodes)
        {
            if (node.Type != "end" && !node.IsEnd)
            {
                if (!edgesBySource.ContainsKey(node.Id) || edgesBySource[node.Id].Count == 0)
                {
                    warnings.Add(new WorkflowValidationWarning
                    {
                        Code = "DEAD_END_NODE",
                        Message = $"Node '{node.Name}' ({node.Id}) has no outgoing connections",
                        NodeId = node.Id
                    });
                }
            }
        }

        // Validate gateway connections
        foreach (var node in nodes)
        {
            var nodeType = node.Type ?? string.Empty;
            if (nodeType.EndsWith("Gateway", StringComparison.OrdinalIgnoreCase))
            {
                var outgoing = edgesBySource.GetValueOrDefault(node.Id) ?? new List<DesignerEdge>();

                if (nodeType.Equals("exclusiveGateway", StringComparison.OrdinalIgnoreCase))
                {
                    // Exclusive gateway should have conditions or default
                    var hasDefault = outgoing.Any(e => e.IsDefault);
                    var hasConditions = outgoing.Any(e => !string.IsNullOrEmpty(e.Condition));

                    if (outgoing.Count > 1 && !hasConditions && !hasDefault)
                    {
                        warnings.Add(new WorkflowValidationWarning
                        {
                            Code = "GATEWAY_NO_CONDITIONS",
                            Message = $"Exclusive gateway '{node.Name}' has multiple outputs but no conditions defined",
                            NodeId = node.Id
                        });
                    }
                }
            }
        }
    }

    private void ValidateParameters(WorkflowDesignModel design, List<WorkflowValidationError> errors, List<WorkflowValidationWarning> warnings)
    {
        var paramNames = new HashSet<string>();
        var inputParams = design.InputParameters ?? new List<ParameterDefinition>();
        var outputParams = design.OutputParameters ?? new List<ParameterDefinition>();

        foreach (var param in inputParams)
        {
            if (string.IsNullOrWhiteSpace(param.Name))
            {
                errors.Add(new WorkflowValidationError
                {
                    Code = "INVALID_PARAMETER_NAME",
                    Message = "Parameter name cannot be empty",
                    PropertyPath = "inputParameters"
                });
            }
            else if (!paramNames.Add(param.Name))
            {
                errors.Add(new WorkflowValidationError
                {
                    Code = "DUPLICATE_PARAMETER_NAME",
                    Message = $"Duplicate input parameter name: {param.Name}",
                    PropertyPath = $"inputParameters[{param.Name}]"
                });
            }
        }

        paramNames.Clear();
        foreach (var param in outputParams)
        {
            if (string.IsNullOrWhiteSpace(param.Name))
            {
                errors.Add(new WorkflowValidationError
                {
                    Code = "INVALID_PARAMETER_NAME",
                    Message = "Parameter name cannot be empty",
                    PropertyPath = "outputParameters"
                });
            }
            else if (!paramNames.Add(param.Name))
            {
                errors.Add(new WorkflowValidationError
                {
                    Code = "DUPLICATE_PARAMETER_NAME",
                    Message = $"Duplicate output parameter name: {param.Name}",
                    PropertyPath = $"outputParameters[{param.Name}]"
                });
            }
        }
    }

    private void ValidateTriggers(WorkflowDesignModel design, List<WorkflowValidationError> errors, List<WorkflowValidationWarning> warnings)
    {
        var triggers = design.Triggers ?? new List<TriggerDefinition>();

        foreach (var trigger in triggers)
        {
            if (string.IsNullOrWhiteSpace(trigger.Name))
            {
                warnings.Add(new WorkflowValidationWarning
                {
                    Code = "MISSING_TRIGGER_NAME",
                    Message = "Trigger has no name"
                });
            }

            var triggerType = trigger.Type ?? string.Empty;
            var triggerConfig = trigger.Config ?? new Dictionary<string, object?>();

            switch (triggerType.ToLowerInvariant())
            {
                case "scheduled":
                    if (!triggerConfig.ContainsKey("cron") || string.IsNullOrEmpty(triggerConfig["cron"]?.ToString()))
                    {
                        errors.Add(new WorkflowValidationError
                        {
                            Code = "MISSING_CRON",
                            Message = "Scheduled trigger requires a cron expression",
                            PropertyPath = $"triggers[{trigger.Name}].config.cron"
                        });
                    }
                    break;

                case "webhook":
                    if (!triggerConfig.ContainsKey("path") || string.IsNullOrEmpty(triggerConfig["path"]?.ToString()))
                    {
                        errors.Add(new WorkflowValidationError
                        {
                            Code = "MISSING_WEBHOOK_PATH",
                            Message = "Webhook trigger requires a path",
                            PropertyPath = $"triggers[{trigger.Name}].config.path"
                        });
                    }
                    break;

                case "event":
                    if (!triggerConfig.ContainsKey("eventName") || string.IsNullOrEmpty(triggerConfig["eventName"]?.ToString()))
                    {
                        errors.Add(new WorkflowValidationError
                        {
                            Code = "MISSING_EVENT_NAME",
                            Message = "Event trigger requires an event name",
                            PropertyPath = $"triggers[{trigger.Name}].config.eventName"
                        });
                    }
                    break;
            }
        }
    }
}
