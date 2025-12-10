namespace XenonClinic.WorkflowEngine.Models.Designer;

using System.Text.Json.Serialization;

/// <summary>
/// Complete workflow definition for the visual designer.
/// This model can be serialized to/from JSON for storage and transfer.
/// </summary>
public class WorkflowDesignModel
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Workflow name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Version number
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Category for organization
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Tags for searching
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Whether this is a draft
    /// </summary>
    public bool IsDraft { get; set; } = true;

    /// <summary>
    /// Whether this workflow is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Tenant ID for multi-tenant workflows
    /// </summary>
    public int? TenantId { get; set; }

    /// <summary>
    /// All nodes in the workflow
    /// </summary>
    public List<DesignerNode> Nodes { get; set; } = new();

    /// <summary>
    /// All edges (connections) between nodes
    /// </summary>
    public List<DesignerEdge> Edges { get; set; } = new();

    /// <summary>
    /// Input parameters definition
    /// </summary>
    public List<ParameterDefinition> InputParameters { get; set; } = new();

    /// <summary>
    /// Output parameters definition
    /// </summary>
    public List<ParameterDefinition> OutputParameters { get; set; } = new();

    /// <summary>
    /// Workflow variables
    /// </summary>
    public List<VariableDefinition> Variables { get; set; } = new();

    /// <summary>
    /// Triggers that can start this workflow
    /// </summary>
    public List<TriggerDefinition> Triggers { get; set; } = new();

    /// <summary>
    /// Global error handlers
    /// </summary>
    public List<ErrorHandlerDefinition> ErrorHandlers { get; set; } = new();

    /// <summary>
    /// Designer viewport settings
    /// </summary>
    public ViewportSettings? Viewport { get; set; }

    /// <summary>
    /// Custom metadata
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Modified timestamp
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Created by user ID
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Modified by user ID
    /// </summary>
    public string? ModifiedBy { get; set; }
}

/// <summary>
/// A node in the workflow designer (activity, gateway, event, etc.)
/// </summary>
public class DesignerNode
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Node type (start, end, task, serviceTask, userTask, exclusiveGateway, parallelGateway, timer, signal, etc.)
    /// </summary>
    public string Type { get; set; } = "task";

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Position on canvas
    /// </summary>
    public NodePosition Position { get; set; } = new();

    /// <summary>
    /// Node dimensions
    /// </summary>
    public NodeDimensions? Dimensions { get; set; }

    /// <summary>
    /// Visual styling
    /// </summary>
    public NodeStyle? Style { get; set; }

    /// <summary>
    /// Type-specific configuration
    /// </summary>
    public Dictionary<string, object?> Config { get; set; } = new();

    /// <summary>
    /// Input mappings (parameter name -> expression)
    /// </summary>
    public Dictionary<string, string> InputMappings { get; set; } = new();

    /// <summary>
    /// Output mappings (result key -> variable name)
    /// </summary>
    public Dictionary<string, string> OutputMappings { get; set; } = new();

    /// <summary>
    /// Attached boundary events
    /// </summary>
    public List<BoundaryEventDefinition>? BoundaryEvents { get; set; }

    /// <summary>
    /// Whether this node is the start node
    /// </summary>
    public bool IsStart { get; set; }

    /// <summary>
    /// Whether this node is an end node
    /// </summary>
    public bool IsEnd { get; set; }

    /// <summary>
    /// Custom data for extensions
    /// </summary>
    public Dictionary<string, object?> Data { get; set; } = new();
}

/// <summary>
/// Node position on canvas
/// </summary>
public class NodePosition
{
    public double X { get; set; }
    public double Y { get; set; }
}

/// <summary>
/// Node dimensions
/// </summary>
public class NodeDimensions
{
    public double Width { get; set; } = 180;
    public double Height { get; set; } = 40;
}

/// <summary>
/// Node visual styling
/// </summary>
public class NodeStyle
{
    public string? BackgroundColor { get; set; }
    public string? BorderColor { get; set; }
    public string? TextColor { get; set; }
    public string? IconClass { get; set; }
    public int? BorderWidth { get; set; }
    public int? BorderRadius { get; set; }
}

/// <summary>
/// An edge (connection) between nodes
/// </summary>
public class DesignerEdge
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Source node ID
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Target node ID
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Source handle/port ID
    /// </summary>
    public string? SourceHandle { get; set; }

    /// <summary>
    /// Target handle/port ID
    /// </summary>
    public string? TargetHandle { get; set; }

    /// <summary>
    /// Edge label
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Condition expression (for conditional flows)
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Whether this is the default path
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Priority for evaluation order
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Edge type (default, conditional, default-flow)
    /// </summary>
    public string Type { get; set; } = "default";

    /// <summary>
    /// Visual styling
    /// </summary>
    public EdgeStyle? Style { get; set; }

    /// <summary>
    /// Waypoints for curved edges
    /// </summary>
    public List<NodePosition>? Waypoints { get; set; }

    /// <summary>
    /// Whether the edge is animated
    /// </summary>
    public bool Animated { get; set; }
}

/// <summary>
/// Edge visual styling
/// </summary>
public class EdgeStyle
{
    public string? StrokeColor { get; set; }
    public int? StrokeWidth { get; set; }
    public string? StrokeDasharray { get; set; }
    public string? LabelBackgroundColor { get; set; }
}

/// <summary>
/// Boundary event attached to a node
/// </summary>
public class BoundaryEventDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "error"; // error, timer, signal, message
    public string? Name { get; set; }
    public bool CancelActivity { get; set; } = true;
    public Dictionary<string, object?> Config { get; set; } = new();
}

/// <summary>
/// Parameter definition for inputs/outputs
/// </summary>
public class ParameterDefinition
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = "string"; // string, number, boolean, object, array, date
    public bool IsRequired { get; set; }
    public object? DefaultValue { get; set; }
    public string? ValidationSchema { get; set; } // JSON Schema
    public List<OptionDefinition>? Options { get; set; } // For enum types
}

/// <summary>
/// Variable definition
/// </summary>
public class VariableDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public object? DefaultValue { get; set; }
    public string Scope { get; set; } = "workflow"; // workflow, local
}

/// <summary>
/// Trigger definition
/// </summary>
public class TriggerDefinition
{
    public string Type { get; set; } = "manual"; // manual, scheduled, event, webhook, entityChange
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object?> Config { get; set; } = new();
}

/// <summary>
/// Error handler definition
/// </summary>
public class ErrorHandlerDefinition
{
    public List<string>? ErrorCodes { get; set; }
    public string? HandlerNodeId { get; set; }
    public bool Compensate { get; set; }
    public bool Terminate { get; set; }
    public RetryConfig? Retry { get; set; }
}

/// <summary>
/// Retry configuration
/// </summary>
public class RetryConfig
{
    public int MaxRetries { get; set; } = 3;
    public int InitialDelayMs { get; set; } = 1000;
    public int MaxDelayMs { get; set; } = 300000;
    public double BackoffMultiplier { get; set; } = 2.0;
}

/// <summary>
/// Option for enum/select parameters
/// </summary>
public class OptionDefinition
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Designer viewport settings
/// </summary>
public class ViewportSettings
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Zoom { get; set; } = 1;
}

/// <summary>
/// Node type catalog for the designer palette
/// </summary>
public class NodeTypeCatalog
{
    public List<NodeTypeCategory> Categories { get; set; } = new();
}

/// <summary>
/// Category of node types
/// </summary>
public class NodeTypeCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public int DisplayOrder { get; set; }
    public List<NodeTypeDefinition> NodeTypes { get; set; } = new();
}

/// <summary>
/// Definition of a node type
/// </summary>
public class NodeTypeDefinition
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public string? IconSvg { get; set; }
    public string Category { get; set; } = "general";
    public NodeStyle? DefaultStyle { get; set; }
    public NodeDimensions? DefaultDimensions { get; set; }
    public List<PropertyDefinition> Properties { get; set; } = new();
    public List<PortDefinition>? InputPorts { get; set; }
    public List<PortDefinition>? OutputPorts { get; set; }
    public bool SupportsMultipleInputs { get; set; }
    public bool SupportsMultipleOutputs { get; set; }
    public bool CanHaveBoundaryEvents { get; set; }
}

/// <summary>
/// Property definition for node configuration
/// </summary>
public class PropertyDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "string"; // string, number, boolean, expression, code, select, multiselect, json
    public bool IsRequired { get; set; }
    public object? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public List<OptionDefinition>? Options { get; set; }
    public string? ValidationRegex { get; set; }
    public string? Group { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsAdvanced { get; set; }
    public Dictionary<string, object?>? EditorConfig { get; set; }
}

/// <summary>
/// Port definition for connections
/// </summary>
public class PortDefinition
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string Position { get; set; } = "bottom"; // top, right, bottom, left
    public string Type { get; set; } = "default"; // default, conditional
    public int MaxConnections { get; set; } = -1; // -1 = unlimited
}
