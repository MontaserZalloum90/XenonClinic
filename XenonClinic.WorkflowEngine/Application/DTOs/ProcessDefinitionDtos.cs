namespace XenonClinic.WorkflowEngine.Application.DTOs;

using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Domain.Models;

#region Request DTOs

/// <summary>
/// Request to create a new process definition.
/// </summary>
public class CreateProcessDefinitionRequest
{
    /// <summary>
    /// Unique key for this process (e.g., "leave-request", "invoice-approval").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this process does.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category for organization.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Tags for searching.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// The process model definition.
    /// </summary>
    public ProcessModel? Model { get; set; }

    /// <summary>
    /// Whether to save as draft or publish immediately.
    /// </summary>
    public bool SaveAsDraft { get; set; } = true;
}

/// <summary>
/// Request to update an existing process definition.
/// </summary>
public class UpdateProcessDefinitionRequest
{
    /// <summary>
    /// Display name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Tags.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Status change.
    /// </summary>
    public ProcessDefinitionStatus? Status { get; set; }
}

/// <summary>
/// Request to create a new version of a process definition.
/// </summary>
public class CreateProcessVersionRequest
{
    /// <summary>
    /// The process model definition.
    /// </summary>
    public ProcessModel Model { get; set; } = new();

    /// <summary>
    /// Description of changes in this version.
    /// </summary>
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// Whether to save as draft.
    /// </summary>
    public bool SaveAsDraft { get; set; } = true;
}

/// <summary>
/// Request to update a process version.
/// </summary>
public class UpdateProcessVersionRequest
{
    /// <summary>
    /// The process model definition.
    /// </summary>
    public ProcessModel? Model { get; set; }

    /// <summary>
    /// Description of changes.
    /// </summary>
    public string? ChangeDescription { get; set; }
}

/// <summary>
/// Request to import a process definition.
/// </summary>
public class ImportProcessDefinitionRequest
{
    /// <summary>
    /// The exported process definition JSON.
    /// </summary>
    public string Json { get; set; } = string.Empty;

    /// <summary>
    /// Optional key override.
    /// </summary>
    public string? KeyOverride { get; set; }

    /// <summary>
    /// Whether to create a new definition even if key exists.
    /// </summary>
    public bool CreateNew { get; set; }
}

/// <summary>
/// Request to clone a process definition.
/// </summary>
public class CloneProcessDefinitionRequest
{
    /// <summary>
    /// New key for the cloned process.
    /// </summary>
    public string NewKey { get; set; } = string.Empty;

    /// <summary>
    /// New name for the cloned process.
    /// </summary>
    public string NewName { get; set; } = string.Empty;
}

#endregion

#region Response DTOs

/// <summary>
/// Summary view of a process definition.
/// </summary>
public class ProcessDefinitionSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public ProcessDefinitionStatus Status { get; set; }
    public int LatestVersion { get; set; }
    public int? PublishedVersion { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int InstanceCount { get; set; }
}

/// <summary>
/// Detailed view of a process definition.
/// </summary>
public class ProcessDefinitionDetailDto
{
    public string Id { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public ProcessDefinitionStatus Status { get; set; }
    public int LatestVersion { get; set; }
    public int? PublishedVersion { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProcessVersionSummaryDto> Versions { get; set; } = new();
    public ProcessVersionDetailDto? LatestVersionDetail { get; set; }
}

/// <summary>
/// Summary view of a process version.
/// </summary>
public class ProcessVersionSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public int Version { get; set; }
    public ProcessVersionStatus Status { get; set; }
    public string? ChangeDescription { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// Detailed view of a process version.
/// </summary>
public class ProcessVersionDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public int Version { get; set; }
    public ProcessVersionStatus Status { get; set; }
    public ProcessModel? Model { get; set; }
    public string? ChangeDescription { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? PublishedBy { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Model statistics
    public int ActivityCount { get; set; }
    public int UserTaskCount { get; set; }
    public int ServiceTaskCount { get; set; }
    public int GatewayCount { get; set; }
}

/// <summary>
/// Validation result for a process model.
/// </summary>
public class ProcessValidationResultDto
{
    public bool IsValid { get; set; }
    public List<ProcessValidationErrorDto> Errors { get; set; } = new();
    public List<ProcessValidationWarningDto> Warnings { get; set; } = new();
}

public class ProcessValidationErrorDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActivityId { get; set; }
    public string? PropertyPath { get; set; }
}

public class ProcessValidationWarningDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActivityId { get; set; }
    public string? PropertyPath { get; set; }
}

/// <summary>
/// Export format for a process definition.
/// </summary>
public class ProcessDefinitionExportDto
{
    public string ExportVersion { get; set; } = "1.0";
    public DateTime ExportedAt { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public ProcessModel Model { get; set; } = new();
}

/// <summary>
/// Paginated list response.
/// </summary>
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

#endregion

#region Mapping Extensions

/// <summary>
/// Extension methods for mapping between entities and DTOs.
/// </summary>
public static class ProcessDefinitionMappingExtensions
{
    public static ProcessDefinitionSummaryDto ToSummaryDto(this ProcessDefinition entity, int instanceCount = 0)
    {
        return new ProcessDefinitionSummaryDto
        {
            Id = entity.Id,
            Key = entity.Key,
            Name = entity.Name,
            Description = entity.Description,
            Category = entity.Category,
            Tags = entity.Tags,
            Status = entity.Status,
            LatestVersion = entity.LatestVersion,
            PublishedVersion = entity.PublishedVersion,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedBy = entity.UpdatedBy,
            UpdatedAt = entity.UpdatedAt,
            InstanceCount = instanceCount
        };
    }

    public static ProcessDefinitionDetailDto ToDetailDto(this ProcessDefinition entity)
    {
        return new ProcessDefinitionDetailDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Key = entity.Key,
            Name = entity.Name,
            Description = entity.Description,
            Category = entity.Category,
            Tags = entity.Tags,
            Status = entity.Status,
            LatestVersion = entity.LatestVersion,
            PublishedVersion = entity.PublishedVersion,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedBy = entity.UpdatedBy,
            UpdatedAt = entity.UpdatedAt,
            Versions = entity.Versions?.Select(v => v.ToSummaryDto()).ToList() ?? new()
        };
    }

    public static ProcessVersionSummaryDto ToSummaryDto(this ProcessVersion entity)
    {
        return new ProcessVersionSummaryDto
        {
            Id = entity.Id,
            Version = entity.Version,
            Status = entity.Status,
            ChangeDescription = entity.ChangeDescription,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            PublishedAt = entity.PublishedAt
        };
    }

    public static ProcessVersionDetailDto ToDetailDto(this ProcessVersion entity, ProcessModel? model = null)
    {
        var dto = new ProcessVersionDetailDto
        {
            Id = entity.Id,
            ProcessDefinitionId = entity.ProcessDefinitionId,
            Version = entity.Version,
            Status = entity.Status,
            Model = model,
            ChangeDescription = entity.ChangeDescription,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            PublishedBy = entity.PublishedBy,
            PublishedAt = entity.PublishedAt
        };

        if (model != null)
        {
            dto.ActivityCount = model.Activities?.Count ?? 0;
            dto.UserTaskCount = model.Activities?.Values.Count(a => a is UserTaskDefinition) ?? 0;
            dto.ServiceTaskCount = model.Activities?.Values.Count(a => a is ServiceTaskDefinition) ?? 0;
            dto.GatewayCount = model.Activities?.Values.Count(a => a is ExclusiveGatewayDefinition or ParallelGatewayDefinition or InclusiveGatewayDefinition) ?? 0;
        }

        return dto;
    }

    public static ProcessDefinitionExportDto ToExportDto(this ProcessDefinition entity, ProcessModel model)
    {
        return new ProcessDefinitionExportDto
        {
            ExportVersion = "1.0",
            ExportedAt = DateTime.UtcNow,
            Key = entity.Key,
            Name = entity.Name,
            Description = entity.Description,
            Category = entity.Category,
            Tags = entity.Tags,
            Model = model
        };
    }
}

#endregion
