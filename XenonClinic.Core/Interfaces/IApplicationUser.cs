namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Interface for application user - decouples Core from ASP.NET Identity
/// </summary>
public interface IApplicationUser
{
    string Id { get; }
    string? UserName { get; set; }
    string? Email { get; set; }
    int? TenantId { get; set; }
    int? CompanyId { get; set; }
    int? PrimaryBranchId { get; set; }
    string? FirstName { get; set; }
    string? LastName { get; set; }
    string? DisplayName { get; set; }
    string? AvatarPath { get; set; }
    string? PreferredLanguage { get; set; }
    string? Timezone { get; set; }
    bool IsActive { get; set; }
    bool IsSuperAdmin { get; set; }
    DateTime CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? LastLoginAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }

    // External login tracking
    bool IsExternalUser { get; set; }
    string? ExternalProviderName { get; set; }
    string? ExternalUserId { get; set; }
    DateTime? LastExternalLoginAt { get; set; }
}
