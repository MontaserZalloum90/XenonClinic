namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Password reset service interface.
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Initiate password reset by sending reset link/code.
    /// </summary>
    Task<PasswordResetResult> InitiateResetAsync(string email);

    /// <summary>
    /// Verify reset token/code.
    /// </summary>
    Task<bool> VerifyResetTokenAsync(string email, string token);

    /// <summary>
    /// Complete password reset with new password.
    /// </summary>
    Task<PasswordResetResult> ResetPasswordAsync(string email, string token, string newPassword);

    /// <summary>
    /// Change password (authenticated user).
    /// </summary>
    Task<PasswordResetResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    /// <summary>
    /// Validate password against policy.
    /// </summary>
    PasswordValidationResult ValidatePassword(string password);

    /// <summary>
    /// Check if password has been compromised (HaveIBeenPwned).
    /// </summary>
    Task<bool> IsPasswordCompromisedAsync(string password);

    /// <summary>
    /// Get password reset token expiry.
    /// </summary>
    Task<DateTime?> GetTokenExpiryAsync(string email, string token);

    /// <summary>
    /// Invalidate all reset tokens for user.
    /// </summary>
    Task InvalidateAllTokensAsync(string email);
}

/// <summary>
/// Password reset result.
/// </summary>
public record PasswordResetResult(
    bool Success,
    string? ErrorMessage = null,
    string? Token = null,
    DateTime? ExpiresAt = null
);

/// <summary>
/// Password validation result.
/// </summary>
public record PasswordValidationResult(
    bool IsValid,
    List<string> Errors,
    int Strength // 0-100
);

/// <summary>
/// Password policy configuration.
/// </summary>
public class PasswordPolicy
{
    public int MinimumLength { get; set; } = 8;
    public int MaximumLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;
    public int MinimumUniqueCharacters { get; set; } = 4;
    public bool PreventCommonPasswords { get; set; } = true;
    public bool PreventUserInfoInPassword { get; set; } = true;
    public int PasswordHistoryCount { get; set; } = 5; // Prevent reuse of last N passwords
    public int ResetTokenExpiryMinutes { get; set; } = 60;
    public int MaxResetAttemptsPerHour { get; set; } = 5;
}
