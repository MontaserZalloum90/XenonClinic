using System.Security.Claims;
using XenonClinic.Core.Entities;

namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Result of external user mapping operation
/// </summary>
public class ExternalUserMappingResult
{
    public bool Succeeded { get; set; }
    public IApplicationUser? User { get; set; }
    public bool IsNewUser { get; set; }
    public string? ErrorMessage { get; set; }
    public IList<string> Errors { get; set; } = new List<string>();

    public static ExternalUserMappingResult Success(IApplicationUser user, bool isNew)
    {
        return new ExternalUserMappingResult
        {
            Succeeded = true,
            User = user,
            IsNewUser = isNew
        };
    }

    public static ExternalUserMappingResult Failure(string error)
    {
        return new ExternalUserMappingResult
        {
            Succeeded = false,
            ErrorMessage = error,
            Errors = new List<string> { error }
        };
    }

    public static ExternalUserMappingResult Failure(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return new ExternalUserMappingResult
        {
            Succeeded = false,
            ErrorMessage = string.Join("; ", errorList),
            Errors = errorList
        };
    }
}

/// <summary>
/// Service for mapping external identity provider claims to local users
/// Handles auto-provisioning and claim mapping
/// </summary>
public interface IExternalUserMapper
{
    /// <summary>
    /// Maps external claims to a local user, creating or updating as needed
    /// </summary>
    /// <param name="principal">The claims principal from the external IdP</param>
    /// <param name="provider">The identity provider configuration</param>
    /// <param name="companyId">The company ID</param>
    /// <returns>Mapping result with the local user</returns>
    Task<ExternalUserMappingResult> MapExternalUserAsync(
        ClaimsPrincipal principal,
        CompanyIdentityProvider provider,
        int companyId);

    /// <summary>
    /// Finds an existing user by external identifier
    /// </summary>
    /// <param name="providerName">The provider name</param>
    /// <param name="externalUserId">The external user ID</param>
    /// <param name="companyId">The company ID</param>
    /// <returns>The user if found</returns>
    Task<IApplicationUser?> FindUserByExternalIdAsync(
        string providerName,
        string externalUserId,
        int companyId);

    /// <summary>
    /// Finds an existing user by email within a company
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="companyId">The company ID</param>
    /// <returns>The user if found</returns>
    Task<IApplicationUser?> FindUserByEmailAsync(string email, int companyId);

    /// <summary>
    /// Extracts claims from the principal using the provider's claim mappings
    /// </summary>
    /// <param name="principal">The claims principal</param>
    /// <param name="provider">The identity provider configuration</param>
    /// <returns>Dictionary of mapped claim values</returns>
    Dictionary<string, string?> ExtractClaims(ClaimsPrincipal principal, CompanyIdentityProvider provider);

    /// <summary>
    /// Updates the last external login timestamp for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    Task UpdateLastExternalLoginAsync(string userId);
}
