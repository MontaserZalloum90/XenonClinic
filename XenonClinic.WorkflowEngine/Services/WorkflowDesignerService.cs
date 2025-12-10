namespace XenonClinic.WorkflowEngine.Services;

using XenonClinic.WorkflowEngine.Core.Abstractions;
using XenonClinic.WorkflowEngine.Models.Designer;
using XenonClinic.WorkflowEngine.Models.Definitions;
using XenonClinic.WorkflowEngine.Persistence.Abstractions;
using XenonClinic.WorkflowEngine.Serialization;
using XenonClinic.WorkflowEngine.Validation;

/// <summary>
/// Service for managing workflow designs in the visual designer.
/// </summary>
public interface IWorkflowDesignerService
{
    /// <summary>
    /// Gets all available node types for the designer palette
    /// </summary>
    Task<NodeTypeCatalog> GetNodeTypeCatalogAsync(int? tenantId = null);

    /// <summary>
    /// Creates a new workflow design
    /// </summary>
    Task<WorkflowDesignModel> CreateAsync(WorkflowDesignModel design);

    /// <summary>
    /// Gets a workflow design by ID and optional version
    /// </summary>
    Task<WorkflowDesignModel?> GetAsync(string id, int? version = null);

    /// <summary>
    /// Saves a workflow design (creates new version)
    /// </summary>
    Task<WorkflowDesignModel> SaveAsync(WorkflowDesignModel design);

    /// <summary>
    /// Validates a workflow design
    /// </summary>
    Task<WorkflowValidationResult> ValidateAsync(WorkflowDesignModel design);

    /// <summary>
    /// Publishes a workflow design (makes it executable)
    /// </summary>
    Task<WorkflowDesignModel> PublishAsync(string id, int version);

    /// <summary>
    /// Unpublishes a workflow design
    /// </summary>
    Task UnpublishAsync(string id, int version);

    /// <summary>
    /// Deletes a workflow design
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// Lists workflow designs with filtering and pagination
    /// </summary>
    Task<WorkflowDesignListResult> ListAsync(WorkflowDesignQuery query);

    /// <summary>
    /// Gets all versions of a workflow design
    /// </summary>
    Task<IList<WorkflowDesignVersionInfo>> GetVersionsAsync(string id);

    /// <summary>
    /// Clones a workflow design
    /// </summary>
    Task<WorkflowDesignModel> CloneAsync(string id, string newName);

    /// <summary>
    /// Exports a workflow design to JSON
    /// </summary>
    Task<string> ExportAsync(string id, int? version = null);

    /// <summary>
    /// Imports a workflow design from JSON
    /// </summary>
    Task<WorkflowDesignModel> ImportAsync(string json, bool overwrite = false);

    /// <summary>
    /// Gets execution statistics for a workflow
    /// </summary>
    Task<WorkflowStatistics> GetStatisticsAsync(string id);
}

/// <summary>
/// Query for listing workflow designs
/// </summary>
public class WorkflowDesignQuery
{
    public string? SearchTerm { get; init; }
    public string? Category { get; init; }
    public IList<string>? Tags { get; init; }
    public bool? IsDraft { get; init; }
    public int? TenantId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? OrderBy { get; init; }
    public bool OrderDescending { get; init; }
}

/// <summary>
/// Result of listing workflow designs
/// </summary>
public class WorkflowDesignListResult
{
    public IList<WorkflowDesignSummary> Items { get; init; } = new List<WorkflowDesignSummary>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Summary of a workflow design for list views
/// </summary>
public class WorkflowDesignSummary
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Version { get; init; }
    public string? Category { get; init; }
    public List<string> Tags { get; init; } = new();
    public bool IsDraft { get; init; }
    public bool IsActive { get; init; }
    public int NodeCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }
}

/// <summary>
/// Version information for a workflow design
/// </summary>
public class WorkflowDesignVersionInfo
{
    public int Version { get; init; }
    public bool IsDraft { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? Comment { get; init; }
}

/// <summary>
/// Workflow execution statistics
/// </summary>
public class WorkflowStatistics
{
    public string WorkflowId { get; init; } = string.Empty;
    public int TotalInstances { get; init; }
    public int RunningInstances { get; init; }
    public int CompletedInstances { get; init; }
    public int FaultedInstances { get; init; }
    public int CancelledInstances { get; init; }
    public TimeSpan? AverageExecutionTime { get; init; }
    public DateTime? LastExecutedAt { get; init; }
}

/// <summary>
/// Default implementation of the workflow designer service.
/// </summary>
public class WorkflowDesignerService : IWorkflowDesignerService
{
    private readonly IWorkflowDefinitionStore _definitionStore;
    private readonly IWorkflowInstanceStore _instanceStore;
    private readonly IWorkflowValidator _validator;

    public WorkflowDesignerService(
        IWorkflowDefinitionStore definitionStore,
        IWorkflowInstanceStore instanceStore,
        IWorkflowValidator validator)
    {
        _definitionStore = definitionStore;
        _instanceStore = instanceStore;
        _validator = validator;
    }

    public Task<NodeTypeCatalog> GetNodeTypeCatalogAsync(int? tenantId = null)
    {
        // Return the default node types catalog
        // In the future, this could be extended with tenant-specific custom nodes
        return Task.FromResult(DefaultNodeTypes.GetCatalog());
    }

    public async Task<WorkflowDesignModel> CreateAsync(WorkflowDesignModel design)
    {
        design.Id = string.IsNullOrEmpty(design.Id) ? Guid.NewGuid().ToString() : design.Id;
        design.Version = 1;
        design.IsDraft = true;
        design.CreatedAt = DateTime.UtcNow;

        // Add default start and end nodes if empty
        if (design.Nodes.Count == 0)
        {
            design.Nodes.Add(new DesignerNode
            {
                Id = "start_1",
                Type = "start",
                Name = "Start",
                IsStart = true,
                Position = new NodePosition { X = 250, Y = 50 }
            });

            design.Nodes.Add(new DesignerNode
            {
                Id = "end_1",
                Type = "end",
                Name = "End",
                IsEnd = true,
                Position = new NodePosition { X = 250, Y = 250 }
            });

            design.Edges.Add(new DesignerEdge
            {
                Id = "edge_1",
                Source = "start_1",
                Target = "end_1"
            });
        }

        var definition = WorkflowSerializer.ToExecutableDefinition(design);
        await _definitionStore.SaveAsync(definition);

        return design;
    }

    public async Task<WorkflowDesignModel?> GetAsync(string id, int? version = null)
    {
        var definition = await _definitionStore.GetAsync(id, version);
        if (definition == null) return null;

        if (definition is WorkflowDefinitionModel model)
        {
            return WorkflowSerializer.ToDesignModel(model);
        }

        // For other implementations, create a basic design model
        return new WorkflowDesignModel
        {
            Id = definition.Id,
            Name = definition.Name,
            Description = definition.Description,
            Version = definition.Version,
            Category = definition.Category,
            Tags = definition.Tags?.ToList() ?? new List<string>(),
            IsDraft = definition.IsDraft,
            IsActive = definition.IsActive,
            TenantId = definition.TenantId,
            CreatedAt = definition.CreatedAt,
            ModifiedAt = definition.ModifiedAt
        };
    }

    public async Task<WorkflowDesignModel> SaveAsync(WorkflowDesignModel design)
    {
        var existing = await _definitionStore.GetAsync(design.Id);

        if (existing != null)
        {
            // Create new version
            design.Version = existing.Version + 1;
        }
        else
        {
            design.Version = 1;
        }

        design.ModifiedAt = DateTime.UtcNow;
        design.IsDraft = true;

        var definition = WorkflowSerializer.ToExecutableDefinition(design);
        await _definitionStore.SaveAsync(definition);

        return design;
    }

    public Task<WorkflowValidationResult> ValidateAsync(WorkflowDesignModel design)
    {
        return Task.FromResult(_validator.Validate(design));
    }

    public async Task<WorkflowDesignModel> PublishAsync(string id, int version)
    {
        var design = await GetAsync(id, version);
        if (design == null)
        {
            throw new InvalidOperationException($"Workflow design not found: {id} v{version}");
        }

        // Validate before publishing
        var validation = await ValidateAsync(design);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"Cannot publish workflow with validation errors: {string.Join(", ", validation.Errors.Select(e => e.Message))}");
        }

        await _definitionStore.PublishAsync(id, version);

        design.IsDraft = false;
        return design;
    }

    public async Task UnpublishAsync(string id, int version)
    {
        await _definitionStore.UnpublishAsync(id, version);
    }

    public async Task DeleteAsync(string id)
    {
        await _definitionStore.DeleteAsync(id);
    }

    public async Task<WorkflowDesignListResult> ListAsync(WorkflowDesignQuery query)
    {
        var result = await _definitionStore.ListAsync(new WorkflowDefinitionQuery
        {
            SearchTerm = query.SearchTerm,
            Category = query.Category,
            Tags = query.Tags,
            IsDraft = query.IsDraft,
            TenantId = query.TenantId,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            OrderBy = query.OrderBy,
            OrderDescending = query.OrderDescending
        });

        var summaries = result.Items.Select(d => new WorkflowDesignSummary
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            Version = d.Version,
            Category = d.Category,
            Tags = d.Tags?.ToList() ?? new List<string>(),
            IsDraft = d.IsDraft,
            IsActive = d.IsActive,
            NodeCount = d.Activities.Count,
            CreatedAt = d.CreatedAt,
            ModifiedAt = d.ModifiedAt
        }).ToList();

        return new WorkflowDesignListResult
        {
            Items = summaries,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }

    public async Task<IList<WorkflowDesignVersionInfo>> GetVersionsAsync(string id)
    {
        var versions = await _definitionStore.GetVersionsAsync(id);

        return versions.Select(v => new WorkflowDesignVersionInfo
        {
            Version = v.Version,
            IsDraft = v.IsDraft,
            IsActive = v.IsActive,
            CreatedAt = v.CreatedAt
        }).ToList();
    }

    public async Task<WorkflowDesignModel> CloneAsync(string id, string newName)
    {
        var original = await GetAsync(id);
        if (original == null)
        {
            throw new InvalidOperationException($"Workflow design not found: {id}");
        }

        var clone = new WorkflowDesignModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = newName,
            Description = original.Description,
            Category = original.Category,
            Tags = new List<string>(original.Tags),
            TenantId = original.TenantId,
            Nodes = original.Nodes.Select(n => new DesignerNode
            {
                Id = n.Id,
                Type = n.Type,
                Name = n.Name,
                Description = n.Description,
                Position = new NodePosition { X = n.Position.X, Y = n.Position.Y },
                Dimensions = n.Dimensions,
                Style = n.Style,
                Config = new Dictionary<string, object?>(n.Config),
                InputMappings = new Dictionary<string, string>(n.InputMappings),
                OutputMappings = new Dictionary<string, string>(n.OutputMappings),
                IsStart = n.IsStart,
                IsEnd = n.IsEnd
            }).ToList(),
            Edges = original.Edges.Select(e => new DesignerEdge
            {
                Id = e.Id,
                Source = e.Source,
                Target = e.Target,
                SourceHandle = e.SourceHandle,
                TargetHandle = e.TargetHandle,
                Label = e.Label,
                Condition = e.Condition,
                IsDefault = e.IsDefault,
                Priority = e.Priority,
                Type = e.Type
            }).ToList(),
            InputParameters = new List<ParameterDefinition>(original.InputParameters),
            OutputParameters = new List<ParameterDefinition>(original.OutputParameters),
            Variables = new List<VariableDefinition>(original.Variables),
            Triggers = new List<TriggerDefinition>(original.Triggers),
            ErrorHandlers = new List<ErrorHandlerDefinition>(original.ErrorHandlers)
        };

        return await CreateAsync(clone);
    }

    public async Task<string> ExportAsync(string id, int? version = null)
    {
        var design = await GetAsync(id, version);
        if (design == null)
        {
            throw new InvalidOperationException($"Workflow design not found: {id}");
        }

        return WorkflowSerializer.SerializeDesign(design);
    }

    public async Task<WorkflowDesignModel> ImportAsync(string json, bool overwrite = false)
    {
        var design = WorkflowSerializer.DeserializeDesign(json);
        if (design == null)
        {
            throw new InvalidOperationException("Failed to deserialize workflow design");
        }

        if (!overwrite)
        {
            // Generate new ID to avoid conflicts
            design.Id = Guid.NewGuid().ToString();
        }

        return await CreateAsync(design);
    }

    public async Task<WorkflowStatistics> GetStatisticsAsync(string id)
    {
        var query = new WorkflowInstanceQuery
        {
            WorkflowId = id,
            PageSize = 1000 // Get enough for statistics
        };

        var result = await _instanceStore.QueryAsync(query);
        var instances = result.Items;

        return new WorkflowStatistics
        {
            WorkflowId = id,
            TotalInstances = result.TotalCount,
            RunningInstances = instances.Count(i => i.Status is WorkflowStatus.Running or WorkflowStatus.Suspended),
            CompletedInstances = instances.Count(i => i.Status == WorkflowStatus.Completed),
            FaultedInstances = instances.Count(i => i.Status == WorkflowStatus.Faulted),
            CancelledInstances = instances.Count(i => i.Status == WorkflowStatus.Cancelled),
            LastExecutedAt = instances.Where(i => i.StartedAt.HasValue)
                .OrderByDescending(i => i.StartedAt)
                .FirstOrDefault()?.StartedAt
        };
    }
}
