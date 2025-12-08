namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data like client secrets and passwords
/// </summary>
public interface ISecretEncryptionService
{
    /// <summary>
    /// Encrypts a plaintext secret
    /// </summary>
    /// <param name="plainText">The plaintext to encrypt</param>
    /// <returns>The encrypted string (base64 encoded)</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypts an encrypted secret
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt</param>
    /// <returns>The decrypted plaintext</returns>
    string Decrypt(string encryptedText);

    /// <summary>
    /// Encrypts a plaintext secret, returning null if input is null/empty
    /// </summary>
    /// <param name="plainText">The plaintext to encrypt</param>
    /// <returns>The encrypted string or null</returns>
    string? EncryptIfNotEmpty(string? plainText);

    /// <summary>
    /// Decrypts an encrypted secret, returning null if input is null/empty
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt</param>
    /// <returns>The decrypted plaintext or null</returns>
    string? DecryptIfNotEmpty(string? encryptedText);
}
