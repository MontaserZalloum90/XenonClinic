using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace XenonClinic.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public int? PrimaryBranchId { get; set; }
        public Branch? PrimaryBranch { get; set; }

        public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
    }
}
