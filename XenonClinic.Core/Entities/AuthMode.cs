namespace XenonClinic.Core.Entities;

/// <summary>
/// Authentication mode for a company
/// </summary>
public enum AuthMode
{
    /// <summary>Only local Identity login is allowed</summary>
    LocalOnly = 0,
    /// <summary>Only external SSO login is allowed</summary>
    SsoOnly = 1,
    /// <summary>Both local and SSO login are allowed</summary>
    Hybrid = 2
}
