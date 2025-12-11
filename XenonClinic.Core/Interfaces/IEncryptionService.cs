namespace XenonClinic.Core.Interfaces;

/// <summary>
/// Service for encrypting/decrypting PHI data at rest
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypt a string value
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Decrypt a string value
    /// </summary>
    string Decrypt(string cipherText);

    /// <summary>
    /// Encrypt bytes
    /// </summary>
    byte[] EncryptBytes(byte[] data);

    /// <summary>
    /// Decrypt bytes
    /// </summary>
    byte[] DecryptBytes(byte[] encryptedData);

    /// <summary>
    /// Hash a value (one-way, for searching)
    /// </summary>
    string Hash(string value);

    /// <summary>
    /// Generate a secure random key
    /// </summary>
    string GenerateKey(int length = 32);

    /// <summary>
    /// Encrypt for specific patient (patient-specific key derivation)
    /// </summary>
    string EncryptForPatient(string plainText, int patientId);

    /// <summary>
    /// Decrypt patient-specific data
    /// </summary>
    string DecryptForPatient(string cipherText, int patientId);

    /// <summary>
    /// Rotate encryption keys (re-encrypt all data)
    /// </summary>
    Task<int> RotateKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify data integrity
    /// </summary>
    bool VerifyIntegrity(string data, string signature);

    /// <summary>
    /// Generate integrity signature
    /// </summary>
    string GenerateSignature(string data);
}
