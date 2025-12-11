using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for validating and enforcing tenant data isolation.
/// Provides runtime validation to ensure entities belong to the correct tenant/branch context.
/// </summary>
public interface ITenantIsolationService
{
    /// <summary>
    /// Validates that the specified branch belongs to the current tenant context.
    /// </summary>
    Task<bool> ValidateBranchAccessAsync(int branchId);

    /// <summary>
    /// Validates that the specified company belongs to the current tenant context.
    /// </summary>
    Task<bool> ValidateCompanyAccessAsync(int companyId);

    /// <summary>
    /// Validates that the specified entity belongs to the current branch context.
    /// </summary>
    Task<bool> ValidateEntityBranchAsync<T>(int entityId) where T : class;

    /// <summary>
    /// Gets the tenant ID for a given branch.
    /// </summary>
    Task<int?> GetTenantIdForBranchAsync(int branchId);

    /// <summary>
    /// Gets all accessible branch IDs for the current user.
    /// </summary>
    Task<IReadOnlyList<int>> GetAccessibleBranchIdsAsync(string userId);

    /// <summary>
    /// Validates cross-entity relationships to ensure they belong to the same tenant/branch.
    /// </summary>
    Task<TenantIsolationValidationResult> ValidateCrossEntityRelationshipAsync(
        int sourceBranchId,
        int targetBranchId,
        string relationshipDescription);

    /// <summary>
    /// Performs a comprehensive tenant isolation audit for a specific entity type.
    /// </summary>
    Task<TenantIsolationAuditResult> AuditEntityIsolationAsync<T>() where T : class;
}

public class TenantIsolationService : ITenantIsolationService
{
    private readonly ClinicDbContext _context;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly ILogger<TenantIsolationService> _logger;

    public TenantIsolationService(
        ClinicDbContext context,
        ITenantContextAccessor tenantContextAccessor,
        ILogger<TenantIsolationService> logger)
    {
        _context = context;
        _tenantContextAccessor = tenantContextAccessor;
        _logger = logger;
    }

    public async Task<bool> ValidateBranchAccessAsync(int branchId)
    {
        // Super admins have access to all branches
        if (_tenantContextAccessor.IsSuperAdmin)
            return true;

        var currentTenantId = _tenantContextAccessor.TenantId;
        if (!currentTenantId.HasValue)
        {
            _logger.LogWarning("Tenant context not set when validating branch access for BranchId: {BranchId}", branchId);
            return false;
        }

        var branch = await _context.Branches
            .Include(b => b.Company)
            .FirstOrDefaultAsync(b => b.Id == branchId);

        if (branch == null)
        {
            _logger.LogWarning("Branch not found: {BranchId}", branchId);
            return false;
        }

        var hasAccess = branch.Company?.TenantId == currentTenantId;

        if (!hasAccess)
        {
            _logger.LogWarning(
                "Tenant isolation violation: TenantId {CurrentTenantId} attempted to access BranchId {BranchId} belonging to TenantId {ActualTenantId}",
                currentTenantId, branchId, branch.Company?.TenantId);
        }

        return hasAccess;
    }

    public async Task<bool> ValidateCompanyAccessAsync(int companyId)
    {
        if (_tenantContextAccessor.IsSuperAdmin)
            return true;

        var currentTenantId = _tenantContextAccessor.TenantId;
        if (!currentTenantId.HasValue)
            return false;

        var company = await _context.Companies.FindAsync(companyId);
        if (company == null)
            return false;

        var hasAccess = company.TenantId == currentTenantId;

        if (!hasAccess)
        {
            _logger.LogWarning(
                "Tenant isolation violation: TenantId {CurrentTenantId} attempted to access CompanyId {CompanyId} belonging to TenantId {ActualTenantId}",
                currentTenantId, companyId, company.TenantId);
        }

        return hasAccess;
    }

    public async Task<bool> ValidateEntityBranchAsync<T>(int entityId) where T : class
    {
        if (_tenantContextAccessor.IsSuperAdmin)
            return true;

        var entity = await _context.Set<T>().FindAsync(entityId);
        if (entity == null)
            return false;

        // Use reflection to get BranchId property
        var branchIdProperty = typeof(T).GetProperty("BranchId");
        if (branchIdProperty == null)
        {
            _logger.LogWarning("Entity type {EntityType} does not have BranchId property", typeof(T).Name);
            return true; // Allow if no BranchId property exists
        }

        var branchId = (int?)branchIdProperty.GetValue(entity);
        if (!branchId.HasValue)
            return true;

        return await ValidateBranchAccessAsync(branchId.Value);
    }

    public async Task<int?> GetTenantIdForBranchAsync(int branchId)
    {
        var branch = await _context.Branches
            .Include(b => b.Company)
            .FirstOrDefaultAsync(b => b.Id == branchId);

        return branch?.Company?.TenantId;
    }

    public async Task<IReadOnlyList<int>> GetAccessibleBranchIdsAsync(string userId)
    {
        if (_tenantContextAccessor.IsSuperAdmin)
        {
            return await _context.Branches.Select(b => b.Id).ToListAsync();
        }

        var userBranches = await _context.UserBranches
            .Where(ub => ub.UserId == userId)
            .Select(ub => ub.BranchId)
            .ToListAsync();

        return userBranches;
    }

    public async Task<TenantIsolationValidationResult> ValidateCrossEntityRelationshipAsync(
        int sourceBranchId,
        int targetBranchId,
        string relationshipDescription)
    {
        if (sourceBranchId == targetBranchId)
        {
            return new TenantIsolationValidationResult
            {
                IsValid = true,
                SourceBranchId = sourceBranchId,
                TargetBranchId = targetBranchId
            };
        }

        var sourceTenantId = await GetTenantIdForBranchAsync(sourceBranchId);
        var targetTenantId = await GetTenantIdForBranchAsync(targetBranchId);

        var isValid = sourceTenantId == targetTenantId;

        if (!isValid)
        {
            _logger.LogWarning(
                "Cross-tenant relationship violation: {Description} - SourceBranch {SourceBranchId} (Tenant {SourceTenantId}) -> TargetBranch {TargetBranchId} (Tenant {TargetTenantId})",
                relationshipDescription, sourceBranchId, sourceTenantId, targetBranchId, targetTenantId);
        }

        return new TenantIsolationValidationResult
        {
            IsValid = isValid,
            SourceBranchId = sourceBranchId,
            TargetBranchId = targetBranchId,
            SourceTenantId = sourceTenantId,
            TargetTenantId = targetTenantId,
            ViolationDescription = isValid ? null : $"Cross-tenant relationship not allowed: {relationshipDescription}"
        };
    }

    public async Task<TenantIsolationAuditResult> AuditEntityIsolationAsync<T>() where T : class
    {
        var result = new TenantIsolationAuditResult
        {
            EntityType = typeof(T).Name,
            AuditedAt = DateTime.UtcNow
        };

        // Check if entity has BranchId property
        var branchIdProperty = typeof(T).GetProperty("BranchId");
        if (branchIdProperty == null)
        {
            result.HasBranchId = false;
            result.Notes = "Entity does not have BranchId property - may not require branch-level isolation";
            return result;
        }

        result.HasBranchId = true;

        // Get all entities and validate their branch isolation
        var entities = await _context.Set<T>().ToListAsync();
        result.TotalRecords = entities.Count;

        foreach (var entity in entities)
        {
            var branchId = (int?)branchIdProperty.GetValue(entity);
            if (!branchId.HasValue)
            {
                result.RecordsWithNullBranchId++;
                continue;
            }

            var tenantId = await GetTenantIdForBranchAsync(branchId.Value);
            if (tenantId.HasValue)
            {
                if (!result.TenantDistribution.ContainsKey(tenantId.Value))
                    result.TenantDistribution[tenantId.Value] = 0;
                result.TenantDistribution[tenantId.Value]++;
            }
            else
            {
                result.OrphanedRecords++;
            }
        }

        return result;
    }
}

public class TenantIsolationValidationResult
{
    public bool IsValid { get; set; }
    public int SourceBranchId { get; set; }
    public int TargetBranchId { get; set; }
    public int? SourceTenantId { get; set; }
    public int? TargetTenantId { get; set; }
    public string? ViolationDescription { get; set; }
}

public class TenantIsolationAuditResult
{
    public string EntityType { get; set; } = string.Empty;
    public DateTime AuditedAt { get; set; }
    public bool HasBranchId { get; set; }
    public int TotalRecords { get; set; }
    public int RecordsWithNullBranchId { get; set; }
    public int OrphanedRecords { get; set; }
    public Dictionary<int, int> TenantDistribution { get; set; } = new();
    public string? Notes { get; set; }
}
