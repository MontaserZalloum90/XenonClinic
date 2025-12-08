using Microsoft.AspNetCore.DataProtection;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Implements secret encryption using ASP.NET Core Data Protection API
/// </summary>
public class SecretEncryptionService : ISecretEncryptionService
{
    private readonly IDataProtector _protector;
    private const string Purpose = "XenonClinic.AuthSecrets.v1";

    public SecretEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
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
        catch (Exception)
        {
            // If decryption fails (e.g., data was not encrypted or corrupted), return null
            return null;
        }
    }
}
