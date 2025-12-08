using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for mapping external identity provider claims to local users
/// Handles auto-provisioning and claim mapping
/// </summary>
public class ExternalUserMapperService : IExternalUserMapper
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClinicDbContext _dbContext;
    private readonly ICompanyAuthConfigService _authConfigService;
    private readonly ILogger<ExternalUserMapperService> _logger;

    // Standard claim types
    private const string ClaimTypeSubject = "sub";
    private const string ClaimTypeNameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
    private const string ClaimTypeEmail = "email";
    private const string ClaimTypeUpn = "upn";
    private const string ClaimTypeName = "name";
    private const string ClaimTypeGivenName = "given_name";
    private const string ClaimTypeFamilyName = "family_name";

    public ExternalUserMapperService(
        UserManager<ApplicationUser> userManager,
        ClinicDbContext dbContext,
        ICompanyAuthConfigService authConfigService,
        ILogger<ExternalUserMapperService> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _authConfigService = authConfigService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ExternalUserMappingResult> MapExternalUserAsync(
        ClaimsPrincipal principal,
        CompanyIdentityProvider provider,
        int companyId)
    {
        try
        {
            var claims = ExtractClaims(principal, provider);

            // Get external user ID (required)
            var externalUserId = GetExternalUserId(principal);
            if (string.IsNullOrEmpty(externalUserId))
            {
                _logger.LogWarning("No external user ID found in claims for provider {Provider}", provider.Name);
                return ExternalUserMappingResult.Failure("No user identifier found in authentication response");
            }

            // Get email (required for user creation)
            var email = claims.GetValueOrDefault("email");
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No email found in claims for provider {Provider}, user {ExternalId}",
                    provider.Name, externalUserId);
                return ExternalUserMappingResult.Failure("No email address found in authentication response");
            }

            // Try to find existing user by external ID
            var existingUser = await FindUserByExternalIdAsync(provider.Name, externalUserId, companyId);

            if (existingUser != null)
            {
                // Update existing user with latest claims
                await UpdateUserFromClaims(existingUser, claims, provider);
                await UpdateLastExternalLoginAsync(existingUser.Id);

                _logger.LogInformation("Mapped external user {ExternalId} to existing user {UserId}",
                    externalUserId, existingUser.Id);

                return ExternalUserMappingResult.Success(existingUser, false);
            }

            // Try to find existing user by email
            existingUser = await FindUserByEmailAsync(email, companyId);

            if (existingUser != null)
            {
                // Link external identity to existing user
                existingUser.ExternalUserId = externalUserId;
                existingUser.ExternalProviderName = provider.Name;
                existingUser.IsExternalUser = true;

                await UpdateUserFromClaims(existingUser, claims, provider);
                await UpdateLastExternalLoginAsync(existingUser.Id);

                _logger.LogInformation("Linked external user {ExternalId} to existing user {UserId} by email",
                    externalUserId, existingUser.Id);

                return ExternalUserMappingResult.Success(existingUser, false);
            }

            // Check if auto-provisioning is enabled
            var authSettings = await _authConfigService.GetAuthSettingsAsync(companyId);
            if (authSettings?.AutoProvisionUsers != true)
            {
                _logger.LogWarning("Auto-provisioning disabled, no local user for {Email}", email);
                return ExternalUserMappingResult.Failure(
                    "Your account has not been provisioned. Please contact your administrator.");
            }

            // Create new user
            var newUser = await CreateUserFromClaims(claims, provider, companyId, externalUserId, authSettings);

            if (newUser != null)
            {
                _logger.LogInformation("Auto-provisioned new user {UserId} from external provider {Provider}",
                    newUser.Id, provider.Name);

                return ExternalUserMappingResult.Success(newUser, true);
            }

            return ExternalUserMappingResult.Failure("Failed to create user account");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping external user from provider {Provider}", provider.Name);
            return ExternalUserMappingResult.Failure("An error occurred while processing your login");
        }
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> FindUserByExternalIdAsync(
        string providerName,
        string externalUserId,
        int companyId)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.ExternalProviderName == providerName &&
                u.ExternalUserId == externalUserId &&
                u.CompanyId == companyId &&
                u.IsActive);
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> FindUserByEmailAsync(string email, int companyId)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.CompanyId == companyId &&
                u.IsActive);
    }

    /// <inheritdoc />
    public Dictionary<string, string?> ExtractClaims(ClaimsPrincipal principal, CompanyIdentityProvider provider)
    {
        var result = new Dictionary<string, string?>();

        // Extract email using configured mapping or defaults
        result["email"] = GetClaimValue(principal,
            provider.ClaimMappingEmail,
            ClaimTypeEmail,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

        // Extract UPN
        result["upn"] = GetClaimValue(principal,
            provider.ClaimMappingUpn,
            ClaimTypeUpn,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");

        // Extract display name
        result["name"] = GetClaimValue(principal,
            provider.ClaimMappingName,
            ClaimTypeName,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");

        // Extract first name
        result["firstName"] = GetClaimValue(principal,
            provider.ClaimMappingFirstName,
            ClaimTypeGivenName,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");

        // Extract last name
        result["lastName"] = GetClaimValue(principal,
            provider.ClaimMappingLastName,
            ClaimTypeFamilyName,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");

        // Extract groups
        result["groups"] = GetClaimValue(principal,
            provider.ClaimMappingGroups,
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/groups");

        return result;
    }

    /// <inheritdoc />
    public async Task UpdateLastExternalLoginAsync(string userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastExternalLoginAt = DateTime.UtcNow;
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }

    private string? GetExternalUserId(ClaimsPrincipal principal)
    {
        // Try standard claim types for external user ID
        return GetClaimValue(principal,
            ClaimTypeSubject,
            ClaimTypeNameIdentifier,
            "oid", // Azure AD Object ID
            "http://schemas.microsoft.com/identity/claims/objectidentifier");
    }

    private string? GetClaimValue(ClaimsPrincipal principal, params string?[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            if (string.IsNullOrEmpty(claimType))
                continue;

            var claim = principal.FindFirst(claimType);
            if (claim != null && !string.IsNullOrEmpty(claim.Value))
                return claim.Value;
        }

        return null;
    }

    private async Task UpdateUserFromClaims(
        ApplicationUser user,
        Dictionary<string, string?> claims,
        CompanyIdentityProvider provider)
    {
        var updated = false;

        // Update name fields if present in claims
        if (!string.IsNullOrEmpty(claims.GetValueOrDefault("firstName")) &&
            user.FirstName != claims["firstName"])
        {
            user.FirstName = claims["firstName"];
            updated = true;
        }

        if (!string.IsNullOrEmpty(claims.GetValueOrDefault("lastName")) &&
            user.LastName != claims["lastName"])
        {
            user.LastName = claims["lastName"];
            updated = true;
        }

        if (!string.IsNullOrEmpty(claims.GetValueOrDefault("name")))
        {
            var displayName = claims["name"];
            if (user.DisplayName != displayName)
            {
                user.DisplayName = displayName;
                updated = true;
            }
        }

        if (updated)
        {
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = "ExternalAuth";
            await _userManager.UpdateAsync(user);
        }
    }

    private async Task<ApplicationUser?> CreateUserFromClaims(
        Dictionary<string, string?> claims,
        CompanyIdentityProvider provider,
        int companyId,
        string externalUserId,
        CompanyAuthSettings authSettings)
    {
        var email = claims.GetValueOrDefault("email")!;

        // Get company for tenant ID
        var company = await _dbContext.Companies
            .Include(c => c.Branches)
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company == null)
            return null;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true, // Trust the IdP
            TenantId = company.TenantId,
            CompanyId = companyId,
            PrimaryBranchId = company.Branches.FirstOrDefault()?.Id,
            FirstName = claims.GetValueOrDefault("firstName"),
            LastName = claims.GetValueOrDefault("lastName"),
            DisplayName = claims.GetValueOrDefault("name") ??
                          $"{claims.GetValueOrDefault("firstName")} {claims.GetValueOrDefault("lastName")}".Trim(),
            IsActive = true,
            IsExternalUser = true,
            ExternalProviderName = provider.Name,
            ExternalUserId = externalUserId,
            LastExternalLoginAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "ExternalAuth"
        };

        // Create user without password (external users don't use local passwords)
        var result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create user {Email}: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        // Assign default role if configured
        if (!string.IsNullOrEmpty(authSettings.DefaultRoleOnAutoProvision))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, authSettings.DefaultRoleOnAutoProvision);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to assign role {Role} to user {Email}: {Errors}",
                    authSettings.DefaultRoleOnAutoProvision, email,
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        return user;
    }
}
