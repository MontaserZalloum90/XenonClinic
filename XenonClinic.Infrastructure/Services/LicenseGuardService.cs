using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services
{
    public class LicenseGuardService : ILicenseGuardService
    {
        private readonly XenonClinicDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public LicenseGuardService(XenonClinicDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<bool> CanCreateBranchAsync()
        {
            var license = await _db.LicenseConfigs.AsNoTracking().FirstOrDefaultAsync();
            if (license == null || !license.IsActive)
            {
                // If there is no license config, allow (demo-friendly)
                return true;
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
            var license = await _db.LicenseConfigs.AsNoTracking().FirstOrDefaultAsync();
            if (license == null || !license.IsActive)
            {
                return true;
            }

            if (license.ExpiryDate.HasValue && license.ExpiryDate.Value.Date < DateTime.UtcNow.Date)
            {
                return false;
            }

            var usersCount = await _userManager.Users.CountAsync();
            return usersCount < license.MaxUsers;
        }
    }
}
