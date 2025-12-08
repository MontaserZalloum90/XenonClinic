using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

public class LookupService : ILookupService
{
    private readonly ClinicDbContext _context;

    public LookupService(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<List<T>> GetLookupsAsync<T>(int? tenantId = null, bool includeInactive = false) where T : SystemLookup
    {
        var query = _context.Set<T>().AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(l => l.TenantId == tenantId.Value || l.TenantId == null);
        }

        if (!includeInactive)
        {
            query = query.Where(l => l.IsActive);
        }

        return await query
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<T?> GetLookupByIdAsync<T>(int id) where T : SystemLookup
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<T?> GetLookupByCodeAsync<T>(string code, int? tenantId = null) where T : SystemLookup
    {
        var query = _context.Set<T>().AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(l => l.TenantId == tenantId.Value || l.TenantId == null);
        }

        return await query.FirstOrDefaultAsync(l => l.Code == code);
    }

    public async Task<T> CreateLookupAsync<T>(T lookup) where T : SystemLookup
    {
        lookup.CreatedAt = DateTime.UtcNow;
        _context.Set<T>().Add(lookup);
        await _context.SaveChangesAsync();
        return lookup;
    }

    public async Task<T> UpdateLookupAsync<T>(T lookup) where T : SystemLookup
    {
        lookup.UpdatedAt = DateTime.UtcNow;
        _context.Set<T>().Update(lookup);
        await _context.SaveChangesAsync();
        return lookup;
    }

    public async Task DeleteLookupAsync<T>(int id) where T : SystemLookup
    {
        var lookup = await GetLookupByIdAsync<T>(id);
        if (lookup == null)
        {
            throw new InvalidOperationException($"Lookup with ID {id} not found.");
        }

        if (lookup.IsSystemDefault)
        {
            throw new InvalidOperationException("Cannot delete system default lookups.");
        }

        _context.Set<T>().Remove(lookup);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CanDeleteLookupAsync<T>(int id) where T : SystemLookup
    {
        var lookup = await GetLookupByIdAsync<T>(id);
        if (lookup == null || lookup.IsSystemDefault)
        {
            return false;
        }

        // Check if lookup is in use (this would need to be enhanced based on actual relationships)
        // For now, just check the IsSystemDefault flag
        return true;
    }

    public async Task<int> GetLookupCountAsync<T>(int? tenantId = null) where T : SystemLookup
    {
        var query = _context.Set<T>().AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(l => l.TenantId == tenantId.Value || l.TenantId == null);
        }

        return await query.CountAsync();
    }

    public async Task ReorderLookupsAsync<T>(Dictionary<int, int> orderMapping) where T : SystemLookup
    {
        foreach (var (id, newOrder) in orderMapping)
        {
            var lookup = await GetLookupByIdAsync<T>(id);
            if (lookup != null)
            {
                lookup.DisplayOrder = newOrder;
                lookup.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasLookupsAsync<T>(int? tenantId = null) where T : SystemLookup
    {
        var query = _context.Set<T>().AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(l => l.TenantId == tenantId.Value || l.TenantId == null);
        }

        return await query.AnyAsync();
    }

    public async Task SeedDefaultLookupsAsync(int? tenantId = null)
    {
        // This method would seed all lookup tables with default values
        // Implementations can be added as needed per lookup type
        await Task.CompletedTask;
    }
}
