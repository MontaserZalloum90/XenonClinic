using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for managing dynamic system lookups.
/// </summary>
public interface ILookupService
{
    // Generic lookup operations
    Task<List<T>> GetLookupsAsync<T>(int? tenantId = null, bool includeInactive = false) where T : SystemLookup;
    Task<T?> GetLookupByIdAsync<T>(int id) where T : SystemLookup;
    Task<T?> GetLookupByCodeAsync<T>(string code, int? tenantId = null) where T : SystemLookup;
    Task<T> CreateLookupAsync<T>(T lookup) where T : SystemLookup;
    Task<T> UpdateLookupAsync<T>(T lookup) where T : SystemLookup;
    Task DeleteLookupAsync<T>(int id) where T : SystemLookup;
    Task<bool> CanDeleteLookupAsync<T>(int id) where T : SystemLookup;

    // Bulk operations
    Task<int> GetLookupCountAsync<T>(int? tenantId = null) where T : SystemLookup;
    Task ReorderLookupsAsync<T>(Dictionary<int, int> orderMapping) where T : SystemLookup;

    // Seed operations
    Task SeedDefaultLookupsAsync(int? tenantId = null);
    Task<bool> HasLookupsAsync<T>(int? tenantId = null) where T : SystemLookup;
}
