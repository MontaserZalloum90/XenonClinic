using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XenonClinic.WorkflowEngine.Domain.Models;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// Service for BPMN 2.0 import and export operations.
/// </summary>
public interface IBpmnService
{
    /// <summary>
    /// Imports a BPMN 2.0 XML file and creates a process definition.
    /// </summary>
    Task<BpmnImportResult> ImportAsync(BpmnImportRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a process definition to BPMN 2.0 XML format.
    /// </summary>
    Task<BpmnExportResult> ExportAsync(string processDefinitionId, int tenantId, BpmnExportOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates BPMN 2.0 XML without importing.
    /// </summary>
    Task<BpmnValidationResult> ValidateAsync(string bpmnXml, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses BPMN 2.0 XML to a process model.
    /// </summary>
    Task<ProcessModel> ParseAsync(string bpmnXml, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts a process model to BPMN 2.0 XML.
    /// </summary>
    Task<string> SerializeAsync(ProcessModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets diagram information from BPMN XML.
    /// </summary>
    Task<BpmnDiagramInfo> GetDiagramInfoAsync(string bpmnXml, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates diagram coordinates in BPMN XML.
    /// </summary>
    Task<string> UpdateDiagramAsync(string bpmnXml, BpmnDiagramUpdate update, CancellationToken cancellationToken = default);
}

#region Request/Response DTOs

public class BpmnImportRequest
{
    public int TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string BpmnXml { get; set; } = string.Empty;
    public byte[]? BpmnFile { get; set; }
    public string? FileName { get; set; }
    public bool DeployImmediately { get; set; } = false;
    public bool OverwriteExisting { get; set; } = false;
    public string? VersionComment { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class BpmnImportResult
{
    public bool Success { get; set; }
    public string? ProcessDefinitionId { get; set; }
    public string? ProcessDefinitionKey { get; set; }
    public string? ProcessName { get; set; }
    public int Version { get; set; }
    public List<BpmnImportWarning> Warnings { get; set; } = new();
    public List<BpmnImportError> Errors { get; set; } = new();
    public BpmnStatistics Statistics { get; set; } = new();
}

public class BpmnImportWarning
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ElementId { get; set; }
    public int? LineNumber { get; set; }
}

public class BpmnImportError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ElementId { get; set; }
    public int? LineNumber { get; set; }
    public bool IsFatal { get; set; }
}

public class BpmnStatistics
{
    public int TotalElements { get; set; }
    public int StartEvents { get; set; }
    public int EndEvents { get; set; }
    public int UserTasks { get; set; }
    public int ServiceTasks { get; set; }
    public int ScriptTasks { get; set; }
    public int Gateways { get; set; }
    public int SubProcesses { get; set; }
    public int BoundaryEvents { get; set; }
    public int SequenceFlows { get; set; }
}

public class BpmnExportOptions
{
    public bool IncludeDiagram { get; set; } = true;
    public bool IncludeDocumentation { get; set; } = true;
    public bool PrettyPrint { get; set; } = true;
    public string? TargetNamespace { get; set; }
    public BpmnVersion Version { get; set; } = BpmnVersion.Bpmn20;
}

public enum BpmnVersion
{
    Bpmn20,
    Bpmn21
}

public class BpmnExportResult
{
    public bool Success { get; set; }
    public string? BpmnXml { get; set; }
    public byte[]? BpmnFile { get; set; }
    public string? FileName { get; set; }
    public string? ErrorMessage { get; set; }
}

public class BpmnValidationResult
{
    public bool IsValid { get; set; }
    public List<BpmnValidationIssue> Issues { get; set; } = new();
    public BpmnStatistics Statistics { get; set; } = new();
}

public class BpmnValidationIssue
{
    public ValidationSeverity Severity { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ElementId { get; set; }
    public string? ElementType { get; set; }
    public int? LineNumber { get; set; }
    public int? ColumnNumber { get; set; }
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Fatal
}

public class BpmnDiagramInfo
{
    public string DiagramId { get; set; } = string.Empty;
    public string? ProcessId { get; set; }
    public List<BpmnShapeInfo> Shapes { get; set; } = new();
    public List<BpmnEdgeInfo> Edges { get; set; } = new();
    public BpmnBounds Bounds { get; set; } = new();
}

public class BpmnShapeInfo
{
    public string Id { get; set; } = string.Empty;
    public string ElementId { get; set; } = string.Empty;
    public string ElementType { get; set; } = string.Empty;
    public BpmnBounds Bounds { get; set; } = new();
    public bool IsExpanded { get; set; }
    public bool IsHorizontal { get; set; }
}

public class BpmnEdgeInfo
{
    public string Id { get; set; } = string.Empty;
    public string ElementId { get; set; } = string.Empty;
    public string SourceRef { get; set; } = string.Empty;
    public string TargetRef { get; set; } = string.Empty;
    public List<BpmnPoint> Waypoints { get; set; } = new();
}

public class BpmnBounds
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

public class BpmnPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class BpmnDiagramUpdate
{
    public List<ShapeUpdate> ShapeUpdates { get; set; } = new();
    public List<EdgeUpdate> EdgeUpdates { get; set; } = new();
}

public class ShapeUpdate
{
    public string ElementId { get; set; } = string.Empty;
    public BpmnBounds? NewBounds { get; set; }
    public bool? IsExpanded { get; set; }
}

public class EdgeUpdate
{
    public string ElementId { get; set; } = string.Empty;
    public List<BpmnPoint>? NewWaypoints { get; set; }
}

#endregion

#region BPMN Parsing Types

/// <summary>
/// Sequence flow definition for BPMN parsing.
/// </summary>
public class SequenceFlowDefinition
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string SourceRef { get; set; } = string.Empty;
    public string TargetRef { get; set; } = string.Empty;
    public string? ConditionExpression { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Activity types for BPMN elements.
/// </summary>
public enum ActivityType
{
    StartEvent,
    EndEvent,
    UserTask,
    ServiceTask,
    ScriptTask,
    ExclusiveGateway,
    ParallelGateway,
    InclusiveGateway,
    SubProcess,
    CallActivity,
    IntermediateCatchEvent,
    IntermediateThrowEvent,
    BoundaryEvent,
    Task
}

#endregion

#region BPMN Element Types

public static class BpmnElementTypes
{
    // Events
    public const string StartEvent = "startEvent";
    public const string EndEvent = "endEvent";
    public const string IntermediateCatchEvent = "intermediateCatchEvent";
    public const string IntermediateThrowEvent = "intermediateThrowEvent";
    public const string BoundaryEvent = "boundaryEvent";

    // Tasks
    public const string Task = "task";
    public const string UserTask = "userTask";
    public const string ServiceTask = "serviceTask";
    public const string ScriptTask = "scriptTask";
    public const string BusinessRuleTask = "businessRuleTask";
    public const string SendTask = "sendTask";
    public const string ReceiveTask = "receiveTask";
    public const string ManualTask = "manualTask";
    public const string CallActivity = "callActivity";

    // Gateways
    public const string ExclusiveGateway = "exclusiveGateway";
    public const string ParallelGateway = "parallelGateway";
    public const string InclusiveGateway = "inclusiveGateway";
    public const string EventBasedGateway = "eventBasedGateway";
    public const string ComplexGateway = "complexGateway";

    // Containers
    public const string SubProcess = "subProcess";
    public const string Transaction = "transaction";
    public const string AdHocSubProcess = "adHocSubProcess";

    // Flows
    public const string SequenceFlow = "sequenceFlow";
    public const string MessageFlow = "messageFlow";
    public const string Association = "association";

    // Data
    public const string DataObject = "dataObject";
    public const string DataStore = "dataStore";
    public const string DataInput = "dataInput";
    public const string DataOutput = "dataOutput";

    // Event Definitions
    public const string MessageEventDefinition = "messageEventDefinition";
    public const string TimerEventDefinition = "timerEventDefinition";
    public const string ErrorEventDefinition = "errorEventDefinition";
    public const string SignalEventDefinition = "signalEventDefinition";
    public const string ConditionalEventDefinition = "conditionalEventDefinition";
    public const string EscalationEventDefinition = "escalationEventDefinition";
    public const string TerminateEventDefinition = "terminateEventDefinition";
    public const string CompensateEventDefinition = "compensateEventDefinition";
    public const string CancelEventDefinition = "cancelEventDefinition";
}

#endregion
