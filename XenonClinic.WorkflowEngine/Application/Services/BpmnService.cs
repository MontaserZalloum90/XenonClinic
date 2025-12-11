using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using XenonClinic.WorkflowEngine.Domain.Models;

namespace XenonClinic.WorkflowEngine.Application.Services;

/// <summary>
/// BPMN 2.0 import/export service implementation.
/// </summary>
public class BpmnService : IBpmnService
{
    private readonly ILogger<BpmnService> _logger;
    private readonly IProcessDefinitionService _processDefinitionService;

    // BPMN 2.0 Namespaces
    private static readonly XNamespace BpmnNs = "http://www.omg.org/spec/BPMN/20100524/MODEL";
    private static readonly XNamespace BpmndiNs = "http://www.omg.org/spec/BPMN/20100524/DI";
    private static readonly XNamespace DcNs = "http://www.omg.org/spec/DD/20100524/DC";
    private static readonly XNamespace DiNs = "http://www.omg.org/spec/DD/20100524/DI";

    public BpmnService(ILogger<BpmnService> logger, IProcessDefinitionService processDefinitionService)
    {
        _logger = logger;
        _processDefinitionService = processDefinitionService;
    }

    public async Task<BpmnImportResult> ImportAsync(BpmnImportRequest request, CancellationToken cancellationToken = default)
    {
        var result = new BpmnImportResult();

        try
        {
            // Get XML content
            var bpmnXml = request.BpmnXml;
            if (string.IsNullOrEmpty(bpmnXml) && request.BpmnFile != null)
            {
                bpmnXml = Encoding.UTF8.GetString(request.BpmnFile);
            }

            if (string.IsNullOrEmpty(bpmnXml))
            {
                result.Errors.Add(new BpmnImportError
                {
                    Code = "EMPTY_CONTENT",
                    Message = "No BPMN content provided",
                    IsFatal = true
                });
                return result;
            }

            // Validate first
            var validation = await ValidateAsync(bpmnXml, cancellationToken);
            result.Warnings.AddRange(validation.Issues
                .Where(i => i.Severity == ValidationSeverity.Warning)
                .Select(i => new BpmnImportWarning
                {
                    Code = i.Code,
                    Message = i.Message,
                    ElementId = i.ElementId,
                    LineNumber = i.LineNumber
                }));

            if (!validation.IsValid)
            {
                result.Errors.AddRange(validation.Issues
                    .Where(i => i.Severity >= ValidationSeverity.Error)
                    .Select(i => new BpmnImportError
                    {
                        Code = i.Code,
                        Message = i.Message,
                        ElementId = i.ElementId,
                        LineNumber = i.LineNumber,
                        IsFatal = i.Severity == ValidationSeverity.Fatal
                    }));

                if (validation.Issues.Any(i => i.Severity == ValidationSeverity.Fatal))
                {
                    return result;
                }
            }

            result.Statistics = validation.Statistics;

            // Parse to process model
            var processModel = await ParseAsync(bpmnXml, cancellationToken);

            // Create process definition
            var definition = await _processDefinitionService.CreateAsync(new CreateProcessDefinitionRequest
            {
                TenantId = request.TenantId,
                Key = processModel.ProcessDefinitionKey,
                Name = processModel.Name,
                Description = processModel.Documentation,
                Model = processModel,
                Metadata = request.Metadata
            }, cancellationToken);

            if (request.DeployImmediately)
            {
                await _processDefinitionService.DeployAsync(definition.Id, cancellationToken);
            }

            result.Success = true;
            result.ProcessDefinitionId = definition.Id;
            result.ProcessDefinitionKey = definition.Key;
            result.ProcessName = definition.Name;
            result.Version = definition.Version;

            _logger.LogInformation("Successfully imported BPMN process {ProcessKey} as definition {DefinitionId}",
                processModel.ProcessDefinitionKey, definition.Id);
        }
        catch (Exception ex)
        {
            result.Errors.Add(new BpmnImportError
            {
                Code = "IMPORT_FAILED",
                Message = ex.Message,
                IsFatal = true
            });
            _logger.LogError(ex, "Failed to import BPMN");
        }

        return result;
    }

    public async Task<BpmnExportResult> ExportAsync(string processDefinitionId, BpmnExportOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new BpmnExportOptions();
        var result = new BpmnExportResult();

        try
        {
            var definition = await _processDefinitionService.GetByIdAsync(processDefinitionId, cancellationToken);
            if (definition == null)
            {
                result.ErrorMessage = $"Process definition {processDefinitionId} not found";
                return result;
            }

            var bpmnXml = await SerializeAsync(definition.Model, cancellationToken);

            result.Success = true;
            result.BpmnXml = bpmnXml;
            result.BpmnFile = Encoding.UTF8.GetBytes(bpmnXml);
            result.FileName = $"{definition.Key}_v{definition.Version}.bpmn";

            _logger.LogInformation("Exported process definition {DefinitionId} to BPMN", processDefinitionId);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to export BPMN for definition {DefinitionId}", processDefinitionId);
        }

        return result;
    }

    public Task<BpmnValidationResult> ValidateAsync(string bpmnXml, CancellationToken cancellationToken = default)
    {
        var result = new BpmnValidationResult { IsValid = true };
        var stats = new BpmnStatistics();

        try
        {
            var doc = XDocument.Parse(bpmnXml);
            var root = doc.Root;

            if (root == null || root.Name.LocalName != "definitions")
            {
                result.Issues.Add(new BpmnValidationIssue
                {
                    Severity = ValidationSeverity.Fatal,
                    Code = "INVALID_ROOT",
                    Message = "Document must have 'definitions' as root element"
                });
                result.IsValid = false;
                return Task.FromResult(result);
            }

            // Find process elements
            var processes = root.Elements().Where(e => e.Name.LocalName == "process").ToList();
            if (processes.Count == 0)
            {
                result.Issues.Add(new BpmnValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Code = "NO_PROCESS",
                    Message = "No process element found"
                });
                result.IsValid = false;
            }

            foreach (var process in processes)
            {
                ValidateProcess(process, result, stats);
            }

            result.Statistics = stats;
            result.IsValid = !result.Issues.Any(i => i.Severity >= ValidationSeverity.Error);
        }
        catch (XmlException ex)
        {
            result.Issues.Add(new BpmnValidationIssue
            {
                Severity = ValidationSeverity.Fatal,
                Code = "XML_PARSE_ERROR",
                Message = ex.Message,
                LineNumber = ex.LineNumber
            });
            result.IsValid = false;
        }

        return Task.FromResult(result);
    }

    public Task<ProcessModel> ParseAsync(string bpmnXml, CancellationToken cancellationToken = default)
    {
        var doc = XDocument.Parse(bpmnXml);
        var root = doc.Root!;

        var processElement = root.Elements().FirstOrDefault(e => e.Name.LocalName == "process");
        if (processElement == null)
        {
            throw new InvalidOperationException("No process element found in BPMN");
        }

        var model = new ProcessModel
        {
            ProcessDefinitionKey = processElement.Attribute("id")?.Value ?? "process",
            Name = processElement.Attribute("name")?.Value ?? "Unnamed Process",
            Documentation = processElement.Elements().FirstOrDefault(e => e.Name.LocalName == "documentation")?.Value
        };

        // Parse activities
        foreach (var element in processElement.Elements())
        {
            var localName = element.Name.LocalName;
            var activity = ParseActivity(element, localName);
            if (activity != null)
            {
                model.Activities[activity.Id] = activity;
            }
        }

        // Parse sequence flows
        foreach (var flowElement in processElement.Elements().Where(e => e.Name.LocalName == "sequenceFlow"))
        {
            var flow = ParseSequenceFlow(flowElement);
            model.SequenceFlows[flow.Id] = flow;
        }

        _logger.LogDebug("Parsed BPMN with {ActivityCount} activities and {FlowCount} flows",
            model.Activities.Count, model.SequenceFlows.Count);

        return Task.FromResult(model);
    }

    public Task<string> SerializeAsync(ProcessModel model, CancellationToken cancellationToken = default)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(BpmnNs + "definitions",
                new XAttribute("xmlns", BpmnNs.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "bpmndi", BpmndiNs.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "dc", DcNs.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "di", DiNs.NamespaceName),
                new XAttribute("id", "Definitions_" + Guid.NewGuid().ToString("N")[..8]),
                new XAttribute("targetNamespace", "http://bpmn.io/schema/bpmn"),

                CreateProcessElement(model),
                CreateDiagramElement(model)
            )
        );

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = new UTF8Encoding(false)
        };

        using var stringWriter = new StringWriter();
        using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
        {
            doc.Save(xmlWriter);
        }

        return Task.FromResult(stringWriter.ToString());
    }

    public Task<BpmnDiagramInfo> GetDiagramInfoAsync(string bpmnXml, CancellationToken cancellationToken = default)
    {
        var doc = XDocument.Parse(bpmnXml);
        var info = new BpmnDiagramInfo();

        var diagram = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "BPMNDiagram");
        if (diagram == null)
        {
            return Task.FromResult(info);
        }

        info.DiagramId = diagram.Attribute("id")?.Value ?? "";

        var plane = diagram.Elements().FirstOrDefault(e => e.Name.LocalName == "BPMNPlane");
        if (plane != null)
        {
            info.ProcessId = plane.Attribute("bpmnElement")?.Value;

            // Parse shapes
            foreach (var shape in plane.Elements().Where(e => e.Name.LocalName == "BPMNShape"))
            {
                var bounds = shape.Elements().FirstOrDefault(e => e.Name.LocalName == "Bounds");
                info.Shapes.Add(new BpmnShapeInfo
                {
                    Id = shape.Attribute("id")?.Value ?? "",
                    ElementId = shape.Attribute("bpmnElement")?.Value ?? "",
                    IsExpanded = bool.TryParse(shape.Attribute("isExpanded")?.Value, out var exp) && exp,
                    IsHorizontal = bool.TryParse(shape.Attribute("isHorizontal")?.Value, out var hor) && hor,
                    Bounds = bounds != null ? new BpmnBounds
                    {
                        X = double.Parse(bounds.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
                        Y = double.Parse(bounds.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture),
                        Width = double.Parse(bounds.Attribute("width")?.Value ?? "0", CultureInfo.InvariantCulture),
                        Height = double.Parse(bounds.Attribute("height")?.Value ?? "0", CultureInfo.InvariantCulture)
                    } : new BpmnBounds()
                });
            }

            // Parse edges
            foreach (var edge in plane.Elements().Where(e => e.Name.LocalName == "BPMNEdge"))
            {
                var edgeInfo = new BpmnEdgeInfo
                {
                    Id = edge.Attribute("id")?.Value ?? "",
                    ElementId = edge.Attribute("bpmnElement")?.Value ?? ""
                };

                foreach (var waypoint in edge.Elements().Where(e => e.Name.LocalName == "waypoint"))
                {
                    edgeInfo.Waypoints.Add(new BpmnPoint
                    {
                        X = double.Parse(waypoint.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
                        Y = double.Parse(waypoint.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture)
                    });
                }

                info.Edges.Add(edgeInfo);
            }
        }

        return Task.FromResult(info);
    }

    public async Task<string> UpdateDiagramAsync(string bpmnXml, BpmnDiagramUpdate update, CancellationToken cancellationToken = default)
    {
        var doc = XDocument.Parse(bpmnXml);

        var plane = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "BPMNPlane");
        if (plane == null)
        {
            return bpmnXml;
        }

        // Update shapes
        foreach (var shapeUpdate in update.ShapeUpdates)
        {
            var shape = plane.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "BPMNShape" &&
                                    e.Attribute("bpmnElement")?.Value == shapeUpdate.ElementId);

            if (shape != null && shapeUpdate.NewBounds != null)
            {
                var bounds = shape.Elements().FirstOrDefault(e => e.Name.LocalName == "Bounds");
                if (bounds != null)
                {
                    bounds.SetAttributeValue("x", shapeUpdate.NewBounds.X.ToString(CultureInfo.InvariantCulture));
                    bounds.SetAttributeValue("y", shapeUpdate.NewBounds.Y.ToString(CultureInfo.InvariantCulture));
                    bounds.SetAttributeValue("width", shapeUpdate.NewBounds.Width.ToString(CultureInfo.InvariantCulture));
                    bounds.SetAttributeValue("height", shapeUpdate.NewBounds.Height.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        // Update edges
        foreach (var edgeUpdate in update.EdgeUpdates)
        {
            var edge = plane.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "BPMNEdge" &&
                                    e.Attribute("bpmnElement")?.Value == edgeUpdate.ElementId);

            if (edge != null && edgeUpdate.NewWaypoints != null)
            {
                // Remove old waypoints
                edge.Elements().Where(e => e.Name.LocalName == "waypoint").Remove();

                // Add new waypoints
                foreach (var wp in edgeUpdate.NewWaypoints)
                {
                    edge.Add(new XElement(DiNs + "waypoint",
                        new XAttribute("x", wp.X.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("y", wp.Y.ToString(CultureInfo.InvariantCulture))));
                }
            }
        }

        return doc.ToString();
    }

    #region Private Methods

    private void ValidateProcess(XElement process, BpmnValidationResult result, BpmnStatistics stats)
    {
        var processId = process.Attribute("id")?.Value;
        if (string.IsNullOrEmpty(processId))
        {
            result.Issues.Add(new BpmnValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "MISSING_PROCESS_ID",
                Message = "Process element must have an id attribute"
            });
        }

        var hasStartEvent = false;
        var hasEndEvent = false;

        foreach (var element in process.Elements())
        {
            var localName = element.Name.LocalName;
            var elementId = element.Attribute("id")?.Value;

            stats.TotalElements++;

            switch (localName)
            {
                case BpmnElementTypes.StartEvent:
                    stats.StartEvents++;
                    hasStartEvent = true;
                    break;
                case BpmnElementTypes.EndEvent:
                    stats.EndEvents++;
                    hasEndEvent = true;
                    break;
                case BpmnElementTypes.UserTask:
                    stats.UserTasks++;
                    break;
                case BpmnElementTypes.ServiceTask:
                    stats.ServiceTasks++;
                    break;
                case BpmnElementTypes.ScriptTask:
                    stats.ScriptTasks++;
                    break;
                case BpmnElementTypes.ExclusiveGateway:
                case BpmnElementTypes.ParallelGateway:
                case BpmnElementTypes.InclusiveGateway:
                case BpmnElementTypes.EventBasedGateway:
                    stats.Gateways++;
                    break;
                case BpmnElementTypes.SubProcess:
                    stats.SubProcesses++;
                    break;
                case BpmnElementTypes.BoundaryEvent:
                    stats.BoundaryEvents++;
                    break;
                case BpmnElementTypes.SequenceFlow:
                    stats.SequenceFlows++;
                    ValidateSequenceFlow(element, result);
                    break;
            }

            // Validate element has ID
            if (localName != "documentation" && string.IsNullOrEmpty(elementId))
            {
                result.Issues.Add(new BpmnValidationIssue
                {
                    Severity = ValidationSeverity.Warning,
                    Code = "MISSING_ELEMENT_ID",
                    Message = $"Element of type {localName} is missing id attribute",
                    ElementType = localName
                });
            }
        }

        if (!hasStartEvent)
        {
            result.Issues.Add(new BpmnValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "NO_START_EVENT",
                Message = "Process must have at least one start event",
                ElementId = processId
            });
        }

        if (!hasEndEvent)
        {
            result.Issues.Add(new BpmnValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Code = "NO_END_EVENT",
                Message = "Process should have at least one end event",
                ElementId = processId
            });
        }
    }

    private static void ValidateSequenceFlow(XElement flow, BpmnValidationResult result)
    {
        var flowId = flow.Attribute("id")?.Value;
        var sourceRef = flow.Attribute("sourceRef")?.Value;
        var targetRef = flow.Attribute("targetRef")?.Value;

        if (string.IsNullOrEmpty(sourceRef))
        {
            result.Issues.Add(new BpmnValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "MISSING_SOURCE_REF",
                Message = "Sequence flow is missing sourceRef",
                ElementId = flowId
            });
        }

        if (string.IsNullOrEmpty(targetRef))
        {
            result.Issues.Add(new BpmnValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Code = "MISSING_TARGET_REF",
                Message = "Sequence flow is missing targetRef",
                ElementId = flowId
            });
        }
    }

    private static ActivityDefinition? ParseActivity(XElement element, string elementType)
    {
        var id = element.Attribute("id")?.Value;
        if (string.IsNullOrEmpty(id))
            return null;

        var activityType = elementType switch
        {
            BpmnElementTypes.StartEvent => ActivityType.StartEvent,
            BpmnElementTypes.EndEvent => ActivityType.EndEvent,
            BpmnElementTypes.UserTask => ActivityType.UserTask,
            BpmnElementTypes.ServiceTask => ActivityType.ServiceTask,
            BpmnElementTypes.ScriptTask => ActivityType.ScriptTask,
            BpmnElementTypes.BusinessRuleTask => ActivityType.BusinessRuleTask,
            BpmnElementTypes.SendTask => ActivityType.SendTask,
            BpmnElementTypes.ReceiveTask => ActivityType.ReceiveTask,
            BpmnElementTypes.ExclusiveGateway => ActivityType.ExclusiveGateway,
            BpmnElementTypes.ParallelGateway => ActivityType.ParallelGateway,
            BpmnElementTypes.InclusiveGateway => ActivityType.InclusiveGateway,
            BpmnElementTypes.SubProcess => ActivityType.SubProcess,
            BpmnElementTypes.CallActivity => ActivityType.CallActivity,
            BpmnElementTypes.BoundaryEvent => ActivityType.BoundaryEvent,
            BpmnElementTypes.IntermediateCatchEvent => ActivityType.IntermediateCatchEvent,
            BpmnElementTypes.IntermediateThrowEvent => ActivityType.IntermediateThrowEvent,
            _ => (ActivityType?)null
        };

        if (activityType == null)
            return null;

        var activity = new ActivityDefinition
        {
            Id = id,
            Name = element.Attribute("name")?.Value ?? id,
            Type = activityType.Value,
            Documentation = element.Elements().FirstOrDefault(e => e.Name.LocalName == "documentation")?.Value
        };

        // Parse incoming/outgoing references
        foreach (var incoming in element.Elements().Where(e => e.Name.LocalName == "incoming"))
        {
            activity.Incoming.Add(incoming.Value);
        }
        foreach (var outgoing in element.Elements().Where(e => e.Name.LocalName == "outgoing"))
        {
            activity.Outgoing.Add(outgoing.Value);
        }

        // Parse activity-specific properties
        ParseActivityProperties(element, activity);

        return activity;
    }

    private static void ParseActivityProperties(XElement element, ActivityDefinition activity)
    {
        // Parse user task properties
        if (activity.Type == ActivityType.UserTask)
        {
            activity.Properties["assignee"] = element.Attribute("assignee")?.Value ?? "";
            activity.Properties["candidateUsers"] = element.Attribute("candidateUsers")?.Value ?? "";
            activity.Properties["candidateGroups"] = element.Attribute("candidateGroups")?.Value ?? "";
            activity.Properties["dueDate"] = element.Attribute("dueDate")?.Value ?? "";
            activity.Properties["priority"] = element.Attribute("priority")?.Value ?? "";

            // Parse form key
            var formKey = element.Attribute("formKey")?.Value;
            if (!string.IsNullOrEmpty(formKey))
            {
                activity.Properties["formKey"] = formKey;
            }
        }

        // Parse service task properties
        if (activity.Type == ActivityType.ServiceTask)
        {
            activity.Properties["class"] = element.Attribute("class")?.Value ?? "";
            activity.Properties["delegateExpression"] = element.Attribute("delegateExpression")?.Value ?? "";
            activity.Properties["expression"] = element.Attribute("expression")?.Value ?? "";
            activity.Properties["resultVariable"] = element.Attribute("resultVariable")?.Value ?? "";
        }

        // Parse script task properties
        if (activity.Type == ActivityType.ScriptTask)
        {
            activity.Properties["scriptFormat"] = element.Attribute("scriptFormat")?.Value ?? "javascript";
            var script = element.Elements().FirstOrDefault(e => e.Name.LocalName == "script");
            if (script != null)
            {
                activity.Properties["script"] = script.Value;
            }
        }

        // Parse event definitions
        var eventDef = element.Elements().FirstOrDefault(e => e.Name.LocalName.EndsWith("EventDefinition", StringComparison.Ordinal));
        if (eventDef != null)
        {
            activity.Properties["eventType"] = eventDef.Name.LocalName;
            if (eventDef.Name.LocalName == BpmnElementTypes.TimerEventDefinition)
            {
                var timeDuration = eventDef.Elements().FirstOrDefault(e => e.Name.LocalName == "timeDuration");
                var timeDate = eventDef.Elements().FirstOrDefault(e => e.Name.LocalName == "timeDate");
                var timeCycle = eventDef.Elements().FirstOrDefault(e => e.Name.LocalName == "timeCycle");

                if (timeDuration != null) activity.Properties["timeDuration"] = timeDuration.Value;
                if (timeDate != null) activity.Properties["timeDate"] = timeDate.Value;
                if (timeCycle != null) activity.Properties["timeCycle"] = timeCycle.Value;
            }
        }
    }

    private static SequenceFlowDefinition ParseSequenceFlow(XElement element)
    {
        return new SequenceFlowDefinition
        {
            Id = element.Attribute("id")?.Value ?? "",
            Name = element.Attribute("name")?.Value,
            SourceRef = element.Attribute("sourceRef")?.Value ?? "",
            TargetRef = element.Attribute("targetRef")?.Value ?? "",
            ConditionExpression = element.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "conditionExpression")?.Value
        };
    }

    private static XElement CreateProcessElement(ProcessModel model)
    {
        var process = new XElement(BpmnNs + "process",
            new XAttribute("id", model.ProcessDefinitionKey),
            new XAttribute("name", model.Name ?? ""),
            new XAttribute("isExecutable", "true"));

        if (!string.IsNullOrEmpty(model.Documentation))
        {
            process.Add(new XElement(BpmnNs + "documentation", model.Documentation));
        }

        // Add activities
        foreach (var activity in model.Activities.Values)
        {
            process.Add(CreateActivityElement(activity));
        }

        // Add sequence flows
        foreach (var flow in model.SequenceFlows.Values)
        {
            process.Add(CreateSequenceFlowElement(flow));
        }

        return process;
    }

    private static XElement CreateActivityElement(ActivityDefinition activity)
    {
        var elementName = activity.Type switch
        {
            ActivityType.StartEvent => BpmnElementTypes.StartEvent,
            ActivityType.EndEvent => BpmnElementTypes.EndEvent,
            ActivityType.UserTask => BpmnElementTypes.UserTask,
            ActivityType.ServiceTask => BpmnElementTypes.ServiceTask,
            ActivityType.ScriptTask => BpmnElementTypes.ScriptTask,
            ActivityType.ExclusiveGateway => BpmnElementTypes.ExclusiveGateway,
            ActivityType.ParallelGateway => BpmnElementTypes.ParallelGateway,
            ActivityType.InclusiveGateway => BpmnElementTypes.InclusiveGateway,
            ActivityType.SubProcess => BpmnElementTypes.SubProcess,
            _ => "task"
        };

        var element = new XElement(BpmnNs + elementName,
            new XAttribute("id", activity.Id),
            new XAttribute("name", activity.Name ?? ""));

        // Add incoming/outgoing references
        foreach (var incoming in activity.Incoming)
        {
            element.Add(new XElement(BpmnNs + "incoming", incoming));
        }
        foreach (var outgoing in activity.Outgoing)
        {
            element.Add(new XElement(BpmnNs + "outgoing", outgoing));
        }

        return element;
    }

    private static XElement CreateSequenceFlowElement(SequenceFlowDefinition flow)
    {
        var element = new XElement(BpmnNs + "sequenceFlow",
            new XAttribute("id", flow.Id),
            new XAttribute("sourceRef", flow.SourceRef),
            new XAttribute("targetRef", flow.TargetRef));

        if (!string.IsNullOrEmpty(flow.Name))
        {
            element.Add(new XAttribute("name", flow.Name));
        }

        if (!string.IsNullOrEmpty(flow.ConditionExpression))
        {
            element.Add(new XElement(BpmnNs + "conditionExpression",
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XAttribute("{http://www.w3.org/2001/XMLSchema-instance}type", "bpmn:tFormalExpression"),
                flow.ConditionExpression));
        }

        return element;
    }

    private static XElement CreateDiagramElement(ProcessModel model)
    {
        var diagram = new XElement(BpmndiNs + "BPMNDiagram",
            new XAttribute("id", "BPMNDiagram_1"));

        var plane = new XElement(BpmndiNs + "BPMNPlane",
            new XAttribute("id", "BPMNPlane_1"),
            new XAttribute("bpmnElement", model.ProcessDefinitionKey));

        // Add shapes for activities
        int x = 150;
        int y = 100;
        foreach (var activity in model.Activities.Values)
        {
            var (width, height) = GetShapeSize(activity.Type);
            plane.Add(new XElement(BpmndiNs + "BPMNShape",
                new XAttribute("id", $"{activity.Id}_di"),
                new XAttribute("bpmnElement", activity.Id),
                new XElement(DcNs + "Bounds",
                    new XAttribute("x", x.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("y", y.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("width", width.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("height", height.ToString(CultureInfo.InvariantCulture)))));

            x += width + 100;
            if (x > 800)
            {
                x = 150;
                y += 150;
            }
        }

        // Add edges for flows
        foreach (var flow in model.SequenceFlows.Values)
        {
            plane.Add(new XElement(BpmndiNs + "BPMNEdge",
                new XAttribute("id", $"{flow.Id}_di"),
                new XAttribute("bpmnElement", flow.Id)));
        }

        diagram.Add(plane);
        return diagram;
    }

    private static (int width, int height) GetShapeSize(ActivityType type)
    {
        return type switch
        {
            ActivityType.StartEvent or ActivityType.EndEvent => (36, 36),
            ActivityType.ExclusiveGateway or ActivityType.ParallelGateway or ActivityType.InclusiveGateway => (50, 50),
            _ => (100, 80)
        };
    }

    #endregion
}
