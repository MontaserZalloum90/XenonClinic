using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Implements secret encryption using ASP.NET Core Data Protection API
/// </summary>
public class SecretEncryptionService : ISecretEncryptionService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<SecretEncryptionService> _logger;
    private const string Purpose = "XenonClinic.AuthSecrets.v1";

    public SecretEncryptionService(
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SecretEncryptionService> logger)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
        _logger = logger;
    }

    /// <inheritdoc />
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));

        return _protector.Protect(plainText);
    }

    /// <inheritdoc />
    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentNullException(nameof(encryptedText));

        return _protector.Unprotect(encryptedText);
    }

    /// <inheritdoc />
    public string? EncryptIfNotEmpty(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return null;

        return Encrypt(plainText);
    }

    /// <inheritdoc />
    public string? DecryptIfNotEmpty(string? encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return null;

        try
        {
            return Decrypt(encryptedText);
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            // If decryption fails due to invalid/corrupted data, log and return null
            _logger.LogWarning(ex, "Failed to decrypt text - data may not be encrypted or is corrupted");
            return null;
        }
        catch (FormatException ex)
        {
            // If decryption fails due to invalid format, log and return null
            _logger.LogWarning(ex, "Failed to decrypt text - invalid format");
            return null;
        }
    }
}
