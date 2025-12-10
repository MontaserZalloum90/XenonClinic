namespace XenonClinic.WorkflowEngine.Models.Designer;

/// <summary>
/// Provides the default node type catalog for the workflow designer.
/// </summary>
public static class DefaultNodeTypes
{
    /// <summary>
    /// Gets the complete node type catalog
    /// </summary>
    public static NodeTypeCatalog GetCatalog()
    {
        return new NodeTypeCatalog
        {
            Categories = new List<NodeTypeCategory>
            {
                GetEventsCategory(),
                GetTasksCategory(),
                GetGatewaysCategory(),
                GetSubProcessCategory(),
                GetDataCategory()
            }
        };
    }

    private static NodeTypeCategory GetEventsCategory()
    {
        return new NodeTypeCategory
        {
            Id = "events",
            Name = "Events",
            Description = "Start, end, and intermediate events",
            IconClass = "fa-bolt",
            DisplayOrder = 1,
            NodeTypes = new List<NodeTypeDefinition>
            {
                new()
                {
                    Type = "start",
                    Name = "Start",
                    Description = "Workflow start point",
                    IconClass = "fa-play-circle",
                    IconSvg = "<circle cx='18' cy='18' r='15' fill='#4CAF50' stroke='#2E7D32' stroke-width='2'/>",
                    Category = "events",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#4CAF50",
                        BorderColor = "#2E7D32",
                        TextColor = "#FFFFFF",
                        BorderRadius = 50
                    },
                    DefaultDimensions = new NodeDimensions { Width = 36, Height = 36 },
                    SupportsMultipleOutputs = false,
                    OutputPorts = new List<PortDefinition>
                    {
                        new() { Id = "out", Position = "bottom" }
                    }
                },
                new()
                {
                    Type = "end",
                    Name = "End",
                    Description = "Workflow end point",
                    IconClass = "fa-stop-circle",
                    Category = "events",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#F44336",
                        BorderColor = "#C62828",
                        TextColor = "#FFFFFF",
                        BorderRadius = 50
                    },
                    DefaultDimensions = new NodeDimensions { Width = 36, Height = 36 },
                    SupportsMultipleInputs = true,
                    InputPorts = new List<PortDefinition>
                    {
                        new() { Id = "in", Position = "top" }
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "outputMappings",
                            DisplayName = "Output Mappings",
                            Type = "json",
                            Description = "Map variables to workflow output"
                        }
                    }
                },
                new()
                {
                    Type = "timer",
                    Name = "Timer",
                    Description = "Wait for a duration or specific time",
                    IconClass = "fa-clock",
                    Category = "events",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#FF9800",
                        BorderColor = "#EF6C00",
                        TextColor = "#FFFFFF"
                    },
                    CanHaveBoundaryEvents = false,
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "timerType",
                            DisplayName = "Timer Type",
                            Type = "select",
                            IsRequired = true,
                            DefaultValue = "duration",
                            Options = new List<OptionDefinition>
                            {
                                new() { Value = "duration", Label = "Duration" },
                                new() { Value = "date", Label = "Specific Date/Time" },
                                new() { Value = "cron", Label = "Cron Expression" }
                            }
                        },
                        new()
                        {
                            Name = "duration",
                            DisplayName = "Duration",
                            Type = "string",
                            Placeholder = "PT1H (ISO 8601 format)",
                            Description = "Duration in ISO 8601 format (e.g., PT1H for 1 hour)"
                        },
                        new()
                        {
                            Name = "dateTime",
                            DisplayName = "Date/Time",
                            Type = "string",
                            Placeholder = "2024-12-31T23:59:59Z"
                        },
                        new()
                        {
                            Name = "cron",
                            DisplayName = "Cron Expression",
                            Type = "string",
                            Placeholder = "0 0 * * *"
                        }
                    }
                },
                new()
                {
                    Type = "signalReceive",
                    Name = "Signal Catch",
                    Description = "Wait for an external signal",
                    IconClass = "fa-satellite-dish",
                    Category = "events",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#9C27B0",
                        BorderColor = "#6A1B9A",
                        TextColor = "#FFFFFF"
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "signalName",
                            DisplayName = "Signal Name",
                            Type = "string",
                            IsRequired = true
                        }
                    }
                },
                new()
                {
                    Type = "signalThrow",
                    Name = "Signal Throw",
                    Description = "Send a signal to other workflows",
                    IconClass = "fa-broadcast-tower",
                    Category = "events",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#9C27B0",
                        BorderColor = "#6A1B9A",
                        TextColor = "#FFFFFF"
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "signalName",
                            DisplayName = "Signal Name",
                            Type = "string",
                            IsRequired = true
                        },
                        new()
                        {
                            Name = "payload",
                            DisplayName = "Payload Expression",
                            Type = "expression"
                        }
                    }
                }
            }
        };
    }

    private static NodeTypeCategory GetTasksCategory()
    {
        return new NodeTypeCategory
        {
            Id = "tasks",
            Name = "Tasks",
            Description = "Various task types",
            IconClass = "fa-tasks",
            DisplayOrder = 2,
            NodeTypes = new List<NodeTypeDefinition>
            {
                new()
                {
                    Type = "task",
                    Name = "Task",
                    Description = "Generic task activity",
                    IconClass = "fa-square",
                    Category = "tasks",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#2196F3",
                        BorderColor = "#1565C0",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4
                    },
                    CanHaveBoundaryEvents = true,
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "taskHandler",
                            DisplayName = "Task Handler",
                            Type = "string",
                            Description = "Fully qualified type name of the task handler"
                        },
                        new()
                        {
                            Name = "parameters",
                            DisplayName = "Parameters",
                            Type = "json",
                            IsAdvanced = true
                        }
                    }
                },
                new()
                {
                    Type = "serviceTask",
                    Name = "Service Task",
                    Description = "Call an external service",
                    IconClass = "fa-cogs",
                    Category = "tasks",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#00BCD4",
                        BorderColor = "#00838F",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4
                    },
                    CanHaveBoundaryEvents = true,
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "serviceType",
                            DisplayName = "Service Type",
                            Type = "string",
                            IsRequired = true,
                            Description = "Fully qualified type name of the service interface"
                        },
                        new()
                        {
                            Name = "methodName",
                            DisplayName = "Method Name",
                            Type = "string",
                            IsRequired = true
                        },
                        new()
                        {
                            Name = "methodParameters",
                            DisplayName = "Method Parameters",
                            Type = "json",
                            Description = "Array of parameter definitions"
                        },
                        new()
                        {
                            Name = "compensationMethod",
                            DisplayName = "Compensation Method",
                            Type = "string",
                            IsAdvanced = true,
                            Description = "Method to call for compensation/rollback"
                        }
                    }
                },
                new()
                {
                    Type = "userTask",
                    Name = "User Task",
                    Description = "Task assigned to a user",
                    IconClass = "fa-user",
                    Category = "tasks",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#E91E63",
                        BorderColor = "#AD1457",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4
                    },
                    CanHaveBoundaryEvents = true,
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "assignee",
                            DisplayName = "Assignee",
                            Type = "expression",
                            Description = "User ID or expression to determine assignee"
                        },
                        new()
                        {
                            Name = "candidateGroups",
                            DisplayName = "Candidate Groups",
                            Type = "string",
                            Description = "Comma-separated list of groups that can claim this task"
                        },
                        new()
                        {
                            Name = "dueDate",
                            DisplayName = "Due Date",
                            Type = "expression"
                        },
                        new()
                        {
                            Name = "priority",
                            DisplayName = "Priority",
                            Type = "select",
                            DefaultValue = "3",
                            Options = new List<OptionDefinition>
                            {
                                new() { Value = "1", Label = "Highest" },
                                new() { Value = "2", Label = "High" },
                                new() { Value = "3", Label = "Normal" },
                                new() { Value = "4", Label = "Low" },
                                new() { Value = "5", Label = "Lowest" }
                            }
                        },
                        new()
                        {
                            Name = "formKey",
                            DisplayName = "Form Key",
                            Type = "string",
                            Description = "Key to identify the form to display"
                        },
                        new()
                        {
                            Name = "formFields",
                            DisplayName = "Form Fields",
                            Type = "json",
                            IsAdvanced = true,
                            Description = "Inline form field definitions"
                        }
                    }
                },
                new()
                {
                    Type = "script",
                    Name = "Script Task",
                    Description = "Execute a script or expression",
                    IconClass = "fa-code",
                    Category = "tasks",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#795548",
                        BorderColor = "#4E342E",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "language",
                            DisplayName = "Language",
                            Type = "select",
                            DefaultValue = "expression",
                            Options = new List<OptionDefinition>
                            {
                                new() { Value = "expression", Label = "Expression" },
                                new() { Value = "csharp", Label = "C#" },
                                new() { Value = "javascript", Label = "JavaScript" }
                            }
                        },
                        new()
                        {
                            Name = "script",
                            DisplayName = "Script",
                            Type = "code",
                            IsRequired = true,
                            EditorConfig = new Dictionary<string, object?>
                            {
                                ["height"] = 200,
                                ["lineNumbers"] = true
                            }
                        }
                    }
                },
                new()
                {
                    Type = "httpTask",
                    Name = "HTTP Task",
                    Description = "Make an HTTP request",
                    IconClass = "fa-globe",
                    Category = "tasks",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#607D8B",
                        BorderColor = "#37474F",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4
                    },
                    CanHaveBoundaryEvents = true,
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "method",
                            DisplayName = "Method",
                            Type = "select",
                            IsRequired = true,
                            DefaultValue = "GET",
                            Options = new List<OptionDefinition>
                            {
                                new() { Value = "GET", Label = "GET" },
                                new() { Value = "POST", Label = "POST" },
                                new() { Value = "PUT", Label = "PUT" },
                                new() { Value = "PATCH", Label = "PATCH" },
                                new() { Value = "DELETE", Label = "DELETE" }
                            }
                        },
                        new()
                        {
                            Name = "url",
                            DisplayName = "URL",
                            Type = "expression",
                            IsRequired = true
                        },
                        new()
                        {
                            Name = "headers",
                            DisplayName = "Headers",
                            Type = "json"
                        },
                        new()
                        {
                            Name = "body",
                            DisplayName = "Request Body",
                            Type = "code"
                        },
                        new()
                        {
                            Name = "timeout",
                            DisplayName = "Timeout (seconds)",
                            Type = "number",
                            DefaultValue = 30
                        }
                    }
                },
                new()
                {
                    Type = "emailTask",
                    Name = "Email Task",
                    Description = "Send an email",
                    IconClass = "fa-envelope",
                    Category = "tasks",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#3F51B5",
                        BorderColor = "#1A237E",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "to",
                            DisplayName = "To",
                            Type = "expression",
                            IsRequired = true
                        },
                        new()
                        {
                            Name = "cc",
                            DisplayName = "CC",
                            Type = "expression"
                        },
                        new()
                        {
                            Name = "subject",
                            DisplayName = "Subject",
                            Type = "expression",
                            IsRequired = true
                        },
                        new()
                        {
                            Name = "body",
                            DisplayName = "Body",
                            Type = "code",
                            IsRequired = true,
                            EditorConfig = new Dictionary<string, object?>
                            {
                                ["height"] = 200
                            }
                        },
                        new()
                        {
                            Name = "isHtml",
                            DisplayName = "Is HTML",
                            Type = "boolean",
                            DefaultValue = true
                        }
                    }
                }
            }
        };
    }

    private static NodeTypeCategory GetGatewaysCategory()
    {
        return new NodeTypeCategory
        {
            Id = "gateways",
            Name = "Gateways",
            Description = "Control flow gateways",
            IconClass = "fa-code-branch",
            DisplayOrder = 3,
            NodeTypes = new List<NodeTypeDefinition>
            {
                new()
                {
                    Type = "exclusiveGateway",
                    Name = "Exclusive Gateway",
                    Description = "Route to one path based on conditions (XOR)",
                    IconClass = "fa-times",
                    Category = "gateways",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#FFC107",
                        BorderColor = "#FFA000",
                        TextColor = "#000000",
                        BorderRadius = 0
                    },
                    DefaultDimensions = new NodeDimensions { Width = 50, Height = 50 },
                    SupportsMultipleInputs = true,
                    SupportsMultipleOutputs = true,
                    InputPorts = new List<PortDefinition>
                    {
                        new() { Id = "in", Position = "left" }
                    },
                    OutputPorts = new List<PortDefinition>
                    {
                        new() { Id = "out", Position = "right", Type = "conditional", MaxConnections = -1 }
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "defaultPath",
                            DisplayName = "Default Path",
                            Type = "string",
                            Description = "Node ID for the default path if no conditions match"
                        }
                    }
                },
                new()
                {
                    Type = "parallelGateway",
                    Name = "Parallel Gateway",
                    Description = "Split into or join from parallel paths",
                    IconClass = "fa-plus",
                    Category = "gateways",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#FFC107",
                        BorderColor = "#FFA000",
                        TextColor = "#000000",
                        BorderRadius = 0
                    },
                    DefaultDimensions = new NodeDimensions { Width = 50, Height = 50 },
                    SupportsMultipleInputs = true,
                    SupportsMultipleOutputs = true,
                    InputPorts = new List<PortDefinition>
                    {
                        new() { Id = "in", Position = "left", MaxConnections = -1 }
                    },
                    OutputPorts = new List<PortDefinition>
                    {
                        new() { Id = "out", Position = "right", MaxConnections = -1 }
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "direction",
                            DisplayName = "Direction",
                            Type = "select",
                            IsRequired = true,
                            DefaultValue = "split",
                            Options = new List<OptionDefinition>
                            {
                                new() { Value = "split", Label = "Split (Fork)" },
                                new() { Value = "join", Label = "Join (Merge)" }
                            }
                        }
                    }
                },
                new()
                {
                    Type = "inclusiveGateway",
                    Name = "Inclusive Gateway",
                    Description = "Route to one or more paths based on conditions (OR)",
                    IconClass = "fa-circle",
                    Category = "gateways",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#FFC107",
                        BorderColor = "#FFA000",
                        TextColor = "#000000",
                        BorderRadius = 0
                    },
                    DefaultDimensions = new NodeDimensions { Width = 50, Height = 50 },
                    SupportsMultipleInputs = true,
                    SupportsMultipleOutputs = true,
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "defaultPath",
                            DisplayName = "Default Path",
                            Type = "string"
                        }
                    }
                }
            }
        };
    }

    private static NodeTypeCategory GetSubProcessCategory()
    {
        return new NodeTypeCategory
        {
            Id = "subprocess",
            Name = "Sub-Processes",
            Description = "Embedded and call activities",
            IconClass = "fa-layer-group",
            DisplayOrder = 4,
            NodeTypes = new List<NodeTypeDefinition>
            {
                new()
                {
                    Type = "subProcess",
                    Name = "Call Activity",
                    Description = "Call another workflow",
                    IconClass = "fa-external-link-alt",
                    Category = "subprocess",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#673AB7",
                        BorderColor = "#4527A0",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4,
                        BorderWidth = 3
                    },
                    CanHaveBoundaryEvents = true,
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "subWorkflowId",
                            DisplayName = "Sub-Workflow",
                            Type = "string",
                            IsRequired = true,
                            Description = "ID of the workflow to call"
                        },
                        new()
                        {
                            Name = "waitForCompletion",
                            DisplayName = "Wait for Completion",
                            Type = "boolean",
                            DefaultValue = true
                        }
                    }
                },
                new()
                {
                    Type = "multiInstance",
                    Name = "Multi-Instance",
                    Description = "Execute an activity multiple times",
                    IconClass = "fa-layer-group",
                    Category = "subprocess",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#009688",
                        BorderColor = "#00695C",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "isSequential",
                            DisplayName = "Sequential",
                            Type = "boolean",
                            DefaultValue = true,
                            Description = "Execute iterations sequentially (true) or in parallel (false)"
                        },
                        new()
                        {
                            Name = "collectionExpression",
                            DisplayName = "Collection",
                            Type = "expression",
                            IsRequired = true,
                            Description = "Expression that evaluates to a collection to iterate"
                        },
                        new()
                        {
                            Name = "itemVariable",
                            DisplayName = "Item Variable",
                            Type = "string",
                            DefaultValue = "item"
                        },
                        new()
                        {
                            Name = "indexVariable",
                            DisplayName = "Index Variable",
                            Type = "string",
                            DefaultValue = "index"
                        },
                        new()
                        {
                            Name = "completionCondition",
                            DisplayName = "Completion Condition",
                            Type = "expression",
                            Description = "Stop early if this condition becomes true"
                        }
                    }
                }
            }
        };
    }

    private static NodeTypeCategory GetDataCategory()
    {
        return new NodeTypeCategory
        {
            Id = "data",
            Name = "Data & Integration",
            Description = "Data manipulation and integration activities",
            IconClass = "fa-database",
            DisplayOrder = 5,
            NodeTypes = new List<NodeTypeDefinition>
            {
                new()
                {
                    Type = "setVariable",
                    Name = "Set Variable",
                    Description = "Set a workflow variable",
                    IconClass = "fa-equals",
                    Category = "data",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#8BC34A",
                        BorderColor = "#558B2F",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "variableName",
                            DisplayName = "Variable Name",
                            Type = "string",
                            IsRequired = true
                        },
                        new()
                        {
                            Name = "valueExpression",
                            DisplayName = "Value",
                            Type = "expression",
                            IsRequired = true
                        }
                    }
                },
                new()
                {
                    Type = "notification",
                    Name = "Notification",
                    Description = "Send a notification",
                    IconClass = "fa-bell",
                    Category = "data",
                    DefaultStyle = new NodeStyle
                    {
                        BackgroundColor = "#FF5722",
                        BorderColor = "#D84315",
                        TextColor = "#FFFFFF",
                        BorderRadius = 4
                    },
                    Properties = new List<PropertyDefinition>
                    {
                        new()
                        {
                            Name = "channel",
                            DisplayName = "Channel",
                            Type = "select",
                            IsRequired = true,
                            DefaultValue = "inApp",
                            Options = new List<OptionDefinition>
                            {
                                new() { Value = "inApp", Label = "In-App" },
                                new() { Value = "email", Label = "Email" },
                                new() { Value = "sms", Label = "SMS" },
                                new() { Value = "push", Label = "Push Notification" }
                            }
                        },
                        new()
                        {
                            Name = "recipient",
                            DisplayName = "Recipient",
                            Type = "expression",
                            IsRequired = true
                        },
                        new()
                        {
                            Name = "title",
                            DisplayName = "Title",
                            Type = "expression",
                            IsRequired = true
                        },
                        new()
                        {
                            Name = "message",
                            DisplayName = "Message",
                            Type = "expression",
                            IsRequired = true
                        }
                    }
                }
            }
        };
    }
}
