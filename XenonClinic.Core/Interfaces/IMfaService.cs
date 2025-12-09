namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Multi-factor authentication service interface.
/// Supports TOTP (authenticator apps), SMS, and Email verification.
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generates a new TOTP secret for a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>Secret key and QR code URI</returns>
    Task<MfaSetupResult> GenerateTotpSecretAsync(string userId);

    /// <summary>
    /// Verifies a TOTP code from an authenticator app.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="code">The 6-digit TOTP code</param>
    /// <returns>True if valid</returns>
    Task<bool> VerifyTotpCodeAsync(string userId, string code);

    /// <summary>
    /// Enables TOTP MFA for a user after verification.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="code">Verification code to confirm setup</param>
    Task<bool> EnableTotpAsync(string userId, string code);

    /// <summary>
    /// Disables MFA for a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    Task DisableMfaAsync(string userId);

    /// <summary>
    /// Sends a verification code via SMS.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="phoneNumber">Phone number to send to</param>
    Task<string> SendSmsCodeAsync(string userId, string phoneNumber);

    /// <summary>
    /// Sends a verification code via email.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="email">Email address to send to</param>
    Task<string> SendEmailCodeAsync(string userId, string email);

    /// <summary>
    /// Verifies an SMS or email code.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="code">The verification code</param>
    /// <param name="purpose">The purpose of the code</param>
    Task<bool> VerifyCodeAsync(string userId, string code, MfaCodePurpose purpose);

    /// <summary>
    /// Generates backup codes for account recovery.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="count">Number of codes to generate</param>
    Task<IEnumerable<string>> GenerateBackupCodesAsync(string userId, int count = 10);

    /// <summary>
    /// Verifies and consumes a backup code.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="code">The backup code</param>
    Task<bool> VerifyBackupCodeAsync(string userId, string code);

    /// <summary>
    /// Gets MFA status for a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    Task<MfaStatus> GetMfaStatusAsync(string userId);
}

/// <summary>
/// Result of MFA setup containing secret and QR code.
/// </summary>
public record MfaSetupResult(
    string Secret,
    string QrCodeUri,
    string ManualEntryKey
);

/// <summary>
/// MFA status for a user.
/// </summary>
public record MfaStatus(
    bool IsEnabled,
    MfaMethod EnabledMethod,
    bool HasBackupCodes,
    int RemainingBackupCodes
);

/// <summary>
/// Supported MFA methods.
/// </summary>
public enum MfaMethod
{
    None = 0,
    Totp = 1,      // Authenticator app
    Sms = 2,       // SMS verification
    Email = 3      // Email verification
}

/// <summary>
/// Purpose of MFA verification code.
/// </summary>
public enum MfaCodePurpose
{
    Login = 1,
    PasswordReset = 2,
    EmailChange = 3,
    PhoneChange = 4,
    EnableMfa = 5,
    DisableMfa = 6
}
