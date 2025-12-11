namespace XenonClinic.WorkflowEngine.Application.Services;

using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Domain.Models;

/// <summary>
/// Service for managing process definitions.
/// </summary>
public interface IProcessDefinitionService
{
    /// <summary>
    /// Creates a new process definition.
    /// </summary>
    Task<ProcessDefinitionDetailDto> CreateAsync(
        int tenantId,
        CreateProcessDefinitionRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a process definition by ID.
    /// </summary>
    Task<ProcessDefinitionDetailDto?> GetByIdAsync(
        string id,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a process definition by key.
    /// </summary>
    Task<ProcessDefinitionDetailDto?> GetByKeyAsync(
        string key,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists process definitions with filtering.
    /// </summary>
    Task<PagedResultDto<ProcessDefinitionSummaryDto>> ListAsync(
        int tenantId,
        string? searchTerm,
        string? category,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a process definition metadata.
    /// </summary>
    Task<ProcessDefinitionDetailDto> UpdateAsync(
        string id,
        int tenantId,
        UpdateProcessDefinitionRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a process definition (soft delete).
    /// </summary>
    Task DeleteAsync(
        string id,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new version of a process definition.
    /// </summary>
    Task<ProcessVersionDetailDto> CreateVersionAsync(
        string definitionId,
        int tenantId,
        CreateProcessVersionRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific version.
    /// </summary>
    Task<ProcessVersionDetailDto?> GetVersionAsync(
        string definitionId,
        int version,
        int tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a draft version.
    /// </summary>
    Task<ProcessVersionDetailDto> UpdateVersionAsync(
        string definitionId,
        int version,
        int tenantId,
        UpdateProcessVersionRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a version, making it the active version for new instances.
    /// </summary>
    Task<ProcessVersionDetailDto> PublishVersionAsync(
        string definitionId,
        int version,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deprecates a version.
    /// </summary>
    Task DeprecateVersionAsync(
        string definitionId,
        int version,
        int tenantId,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a process model without saving.
    /// </summary>
    Task<ProcessValidationResultDto> ValidateAsync(
        ProcessModel model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a process definition.
    /// </summary>
    Task<ProcessDefinitionExportDto> ExportAsync(
        string id,
        int tenantId,
        int? version = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports a process definition.
    /// </summary>
    Task<ProcessDefinitionDetailDto> ImportAsync(
        int tenantId,
        ImportProcessDefinitionRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clones a process definition.
    /// </summary>
    Task<ProcessDefinitionDetailDto> CloneAsync(
        string id,
        int tenantId,
        CloneProcessDefinitionRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets categories for a tenant.
    /// </summary>
    Task<IList<string>> GetCategoriesAsync(
        int tenantId,
        CancellationToken cancellationToken = default);
}
