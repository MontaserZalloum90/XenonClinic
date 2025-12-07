using System.Threading.Tasks;

namespace XenonClinic.Core.Interfaces
{
    public interface ILicenseGuardService
    {
        Task<bool> CanCreateBranchAsync();
        Task<bool> CanCreateUserAsync();
    }
}
