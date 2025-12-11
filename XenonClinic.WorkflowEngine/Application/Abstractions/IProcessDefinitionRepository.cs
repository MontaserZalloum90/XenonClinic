namespace XenonClinic.WorkflowEngine.Application.Abstractions;

using XenonClinic.WorkflowEngine.Domain.Entities;

/// <summary>
/// Repository for managing process definitions.
/// </summary>
public interface IProcessDefinitionRepository
{
    /// <summary>
    /// Gets a process definition by ID.
    /// </summary>
    Task<ProcessDefinition?> GetByIdAsync(string id, int tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a process definition by key.
    /// </summary>
    Task<ProcessDefinition?> GetByKeyAsync(string key, int tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific version of a process definition.
    /// </summary>
    Task<ProcessVersion?> GetVersionAsync(string definitionId, int version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all versions of a process definition.
    /// </summary>
    Task<IList<ProcessVersion>> GetVersionsAsync(string definitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest published version of a process definition.
    /// </summary>
    Task<ProcessVersion?> GetLatestPublishedVersionAsync(string definitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists process definitions with filtering and pagination.
    /// </summary>
    Task<ProcessDefinitionListResult> ListAsync(ProcessDefinitionQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new process definition.
    /// </summary>
    Task<ProcessDefinition> CreateAsync(ProcessDefinition definition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a process definition.
    /// </summary>
    Task<ProcessDefinition> UpdateAsync(ProcessDefinition definition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new version of a process definition.
    /// </summary>
    Task<ProcessVersion> CreateVersionAsync(ProcessVersion version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a process version.
    /// </summary>
    Task<ProcessVersion> UpdateVersionAsync(ProcessVersion version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a version, making it the active version.
    /// </summary>
    Task PublishVersionAsync(string definitionId, int version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deprecates a version.
    /// </summary>
    Task DeprecateVersionAsync(string definitionId, int version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a process definition (soft delete).
    /// </summary>
    Task DeleteAsync(string id, int tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets process definitions by category.
    /// </summary>
    Task<IList<ProcessDefinition>> GetByCategoryAsync(string category, int tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key is unique within a tenant.
    /// </summary>
    Task<bool> IsKeyUniqueAsync(string key, int tenantId, string? excludeId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query parameters for listing process definitions.
/// </summary>
public class ProcessDefinitionQuery
{
    public int TenantId { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public ProcessDefinitionStatus? Status { get; set; }
    public bool? HasPublishedVersion { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? SortBy { get; set; } = "UpdatedAt";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Result of listing process definitions.
/// </summary>
public class ProcessDefinitionListResult
{
    public IList<ProcessDefinition> Items { get; set; } = new List<ProcessDefinition>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
