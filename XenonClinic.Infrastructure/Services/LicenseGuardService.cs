using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

public class LicenseGuardService : ILicenseGuardService
{
    private readonly ClinicDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMemoryCache _cache;
    private const string LicenseCacheKey = "XenonClinic_LicenseConfig";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public LicenseGuardService(
        ClinicDbContext db,
        UserManager<ApplicationUser> userManager,
        IMemoryCache cache)
    {
        _db = db;
        _userManager = userManager;
        _cache = cache;
    }

    public async Task<bool> CanCreateBranchAsync()
    {
        var license = await GetLicenseConfigAsync();
        if (license == null || !license.IsActive)
        {
            return false;
        }

        if (license.ExpiryDate.HasValue && license.ExpiryDate.Value.Date < DateTime.UtcNow.Date)
        {
            return false;
        }

        var branchesCount = await _db.Branches.CountAsync();
        return branchesCount < license.MaxBranches;
    }

    public async Task<bool> CanCreateUserAsync()
    {
        var license = await GetLicenseConfigAsync();
        if (license == null || !license.IsActive)
        {
            return false;
        }

        if (license.ExpiryDate.HasValue && license.ExpiryDate.Value.Date < DateTime.UtcNow.Date)
        {
            return false;
        }

        var usersCount = await _userManager.Users.CountAsync();
        return usersCount < license.MaxUsers;
    }

    private async Task<LicenseConfig?> GetLicenseConfigAsync()
    {
        if (!_cache.TryGetValue(LicenseCacheKey, out LicenseConfig? license))
        {
            license = await _db.LicenseConfigs.AsNoTracking().FirstOrDefaultAsync();

            if (license != null)
            {
                _cache.Set(LicenseCacheKey, license, CacheDuration);
            }
        }

        return license;
    }
}
