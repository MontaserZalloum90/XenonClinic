namespace XenonClinic.Core.Entities;

/// <summary>
/// Represents a linked OAuth account for a user
/// </summary>
public class OAuthLinkedAccount
{
    public int Id { get; set; }

    /// <summary>
    /// The user ID this OAuth account is linked to
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The OAuth provider (Google, Microsoft, GitHub, Apple)
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// The user's ID from the OAuth provider
    /// </summary>
    public string ProviderUserId { get; set; } = string.Empty;

    /// <summary>
    /// The user's email from the OAuth provider
    /// </summary>
    public string? ProviderEmail { get; set; }

    /// <summary>
    /// Encrypted refresh token for token refresh
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// When the refresh token expires
    /// </summary>
    public DateTime? RefreshTokenExpiry { get; set; }

    /// <summary>
    /// Additional data from the provider as JSON
    /// </summary>
    public string? ProviderDataJson { get; set; }

    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
