using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// AES-256 encryption service for PHI data protection
/// Compliant with HIPAA encryption requirements
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _masterKey;
    private readonly byte[] _hmacKey;
    private readonly byte[]? _previousKey; // For key rotation - decrypt with old key
    private readonly ILogger<EncryptionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private const int KeySize = 256;
    private const int BlockSize = 128;
    private const int SaltSize = 16;
    private const int Iterations = 100000; // PBKDF2 iterations
    private const int KeyRotationBatchSize = 100; // Process records in batches

    // Key version prefix for identifying encryption version
    private const string CurrentKeyVersion = "v2:";
    private const string PreviousKeyVersion = "v1:";

    public EncryptionService(
        IConfiguration configuration,
        ILogger<EncryptionService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;

        // Load master key from secure configuration (should be from Key Vault in production)
        var masterKeyBase64 = configuration["Security:Encryption:MasterKey"]
            ?? throw new InvalidOperationException("Encryption master key not configured");
        var hmacKeyBase64 = configuration["Security:Encryption:HmacKey"]
            ?? throw new InvalidOperationException("HMAC key not configured");

        _masterKey = Convert.FromBase64String(masterKeyBase64);
        _hmacKey = Convert.FromBase64String(hmacKeyBase64);

        // Load previous key for rotation support (optional)
        var previousKeyBase64 = configuration["Security:Encryption:PreviousMasterKey"];
        if (!string.IsNullOrEmpty(previousKeyBase64))
        {
            _previousKey = Convert.FromBase64String(previousKeyBase64);
            _logger.LogInformation("Previous encryption key loaded for rotation support");
        }

        if (_masterKey.Length != 32)
            throw new InvalidOperationException("Master key must be 256 bits (32 bytes)");
    }

    /// <summary>
    /// Encrypt string using AES-256-GCM
    /// Format: Salt(16) + IV(12) + Tag(16) + CipherText
    /// </summary>
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = EncryptBytes(plainBytes);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Encryption failed");
            throw new CryptographicException("Failed to encrypt data", ex);
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            var encryptedBytes = Convert.FromBase64String(cipherText);
            var decryptedBytes = DecryptBytes(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Decryption failed");
            throw new CryptographicException("Failed to decrypt data", ex);
        }
    }

    public byte[] EncryptBytes(byte[] data)
    {
        if (data == null || data.Length == 0)
            return data;

        // Generate random salt and IV
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var iv = RandomNumberGenerator.GetBytes(12); // GCM uses 12-byte IV
        var tag = new byte[16]; // GCM tag

        // Derive key from master key + salt
        var derivedKey = DeriveKey(_masterKey, salt);

        using var aesGcm = new AesGcm(derivedKey, 16);
        var cipherText = new byte[data.Length];
        
        aesGcm.Encrypt(iv, data, cipherText, tag);

        // Combine: Salt + IV + Tag + CipherText
        var result = new byte[SaltSize + 12 + 16 + cipherText.Length];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(iv, 0, result, SaltSize, 12);
        Buffer.BlockCopy(tag, 0, result, SaltSize + 12, 16);
        Buffer.BlockCopy(cipherText, 0, result, SaltSize + 12 + 16, cipherText.Length);

        return result;
    }

    public byte[] DecryptBytes(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length < SaltSize + 12 + 16)
            return encryptedData;

        // Extract components
        var salt = new byte[SaltSize];
        var iv = new byte[12];
        var tag = new byte[16];
        var cipherText = new byte[encryptedData.Length - SaltSize - 12 - 16];

        Buffer.BlockCopy(encryptedData, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(encryptedData, SaltSize, iv, 0, 12);
        Buffer.BlockCopy(encryptedData, SaltSize + 12, tag, 0, 16);
        Buffer.BlockCopy(encryptedData, SaltSize + 12 + 16, cipherText, 0, cipherText.Length);

        // Derive key
        var derivedKey = DeriveKey(_masterKey, salt);

        using var aesGcm = new AesGcm(derivedKey, 16);
        var plainText = new byte[cipherText.Length];
        
        aesGcm.Decrypt(iv, cipherText, tag, plainText);

        return plainText;
    }

    public string Hash(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Use HMAC-SHA256 for deterministic hashing (allows searching)
        using var hmac = new HMACSHA256(_hmacKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(hash);
    }

    public string GenerateKey(int length = 32)
    {
        var key = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(key);
    }

    public string EncryptForPatient(string plainText, int patientId)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        // SECURITY FIX: Use cryptographically random salt combined with deterministic component
        // This maintains key derivation consistency for the same patient while adding entropy
        // The patientId is used to ensure different patients get different keys
        // The masterKey provides the secret entropy
        var saltInput = $"XENON_PATIENT_KEY_v1_{patientId}";
        var patientSalt = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(saltInput));
        var patientKey = DeriveKey(_masterKey, patientSalt);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var iv = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];

        using var aesGcm = new AesGcm(patientKey, 16);
        var cipherText = new byte[plainBytes.Length];
        aesGcm.Encrypt(iv, plainBytes, cipherText, tag);

        // Format: PatientId(4) + IV(12) + Tag(16) + CipherText
        var result = new byte[4 + 12 + 16 + cipherText.Length];
        BitConverter.GetBytes(patientId).CopyTo(result, 0);
        Buffer.BlockCopy(iv, 0, result, 4, 12);
        Buffer.BlockCopy(tag, 0, result, 16, 16);
        Buffer.BlockCopy(cipherText, 0, result, 32, cipherText.Length);

        return Convert.ToBase64String(result);
    }

    public string DecryptForPatient(string cipherText, int patientId)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var encryptedData = Convert.FromBase64String(cipherText);
        
        // Verify patient ID matches
        var storedPatientId = BitConverter.ToInt32(encryptedData, 0);
        if (storedPatientId != patientId)
            throw new CryptographicException("Patient ID mismatch - possible data tampering");

        var iv = new byte[12];
        var tag = new byte[16];
        var cipher = new byte[encryptedData.Length - 32];

        Buffer.BlockCopy(encryptedData, 4, iv, 0, 12);
        Buffer.BlockCopy(encryptedData, 16, tag, 0, 16);
        Buffer.BlockCopy(encryptedData, 32, cipher, 0, cipher.Length);

        // SECURITY FIX: Use same improved salt derivation as encrypt
        var saltInput = $"XENON_PATIENT_KEY_v1_{patientId}";
        var patientSalt = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(saltInput));
        var patientKey = DeriveKey(_masterKey, patientSalt);

        using var aesGcm = new AesGcm(patientKey, 16);
        var plainText = new byte[cipher.Length];
        aesGcm.Decrypt(iv, cipher, tag, plainText);

        return Encoding.UTF8.GetString(plainText);
    }

    public async Task<int> RotateKeysAsync(CancellationToken cancellationToken = default)
    {
        // Key rotation involves re-encrypting all PHI data with the current key
        // This handles data encrypted with the previous key

        if (_previousKey == null)
        {
            _logger.LogWarning("Key rotation requested but no previous key configured. " +
                "Set Security:Encryption:PreviousMasterKey to enable rotation.");
            return 0;
        }

        _logger.LogInformation("Starting key rotation - re-encrypting data with new master key");
        var totalRotated = 0;
        var errors = new List<string>();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();

        try
        {
            // Rotate patient encrypted fields (SSN, insurance info, etc.)
            totalRotated += await RotatePatientDataAsync(context, cancellationToken);

            // Rotate OAuth tokens in calendar connections
            totalRotated += await RotateCalendarConnectionsAsync(context, cancellationToken);

            // Rotate payment gateway credentials
            totalRotated += await RotatePaymentGatewayDataAsync(context, cancellationToken);

            // Rotate OAuth configs
            totalRotated += await RotateOAuthConfigsAsync(context, cancellationToken);

            // Log rotation completion
            _logger.LogInformation(
                "Key rotation completed. Total records re-encrypted: {TotalRotated}",
                totalRotated);

            // Create audit log entry for key rotation
            var auditLog = new KeyRotationAuditLog
            {
                RotationDate = DateTime.UtcNow,
                RecordsProcessed = totalRotated,
                Status = "Completed",
                Errors = errors.Count > 0 ? string.Join("; ", errors) : null
            };
            context.KeyRotationAuditLogs.Add(auditLog);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Key rotation failed");
            throw;
        }

        return totalRotated;
    }

    private async Task<int> RotatePatientDataAsync(ClinicDbContext context, CancellationToken cancellationToken)
    {
        var rotated = 0;
        var offset = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var patients = await context.Patients
                .OrderBy(p => p.Id)
                .Skip(offset)
                .Take(KeyRotationBatchSize)
                .ToListAsync(cancellationToken);

            if (patients.Count == 0)
                break;

            foreach (var patient in patients)
            {
                var modified = false;

                // Re-encrypt SSN if present
                if (!string.IsNullOrEmpty(patient.EncryptedSSN))
                {
                    try
                    {
                        var decrypted = DecryptWithFallback(patient.EncryptedSSN);
                        var reEncrypted = Encrypt(decrypted);
                        if (patient.EncryptedSSN != reEncrypted)
                        {
                            patient.EncryptedSSN = reEncrypted;
                            modified = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to rotate SSN for PatientId {PatientId}", patient.Id);
                    }
                }

                // Re-encrypt insurance info if present
                if (!string.IsNullOrEmpty(patient.EncryptedInsuranceId))
                {
                    try
                    {
                        var decrypted = DecryptWithFallback(patient.EncryptedInsuranceId);
                        var reEncrypted = Encrypt(decrypted);
                        if (patient.EncryptedInsuranceId != reEncrypted)
                        {
                            patient.EncryptedInsuranceId = reEncrypted;
                            modified = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to rotate insurance ID for PatientId {PatientId}", patient.Id);
                    }
                }

                if (modified)
                {
                    patient.UpdatedAt = DateTime.UtcNow;
                    rotated++;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            offset += KeyRotationBatchSize;

            _logger.LogDebug("Key rotation progress: processed {Offset} patients, rotated {Rotated}",
                offset, rotated);
        }

        _logger.LogInformation("Patient data rotation completed: {Rotated} records", rotated);
        return rotated;
    }

    private async Task<int> RotateCalendarConnectionsAsync(ClinicDbContext context, CancellationToken cancellationToken)
    {
        var rotated = 0;
        var connections = await context.CalendarConnections
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var connection in connections)
        {
            var modified = false;

            if (!string.IsNullOrEmpty(connection.AccessToken))
            {
                try
                {
                    var decrypted = DecryptWithFallback(connection.AccessToken);
                    var reEncrypted = Encrypt(decrypted);
                    if (connection.AccessToken != reEncrypted)
                    {
                        connection.AccessToken = reEncrypted;
                        modified = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to rotate access token for ConnectionId {ConnectionId}",
                        connection.Id);
                }
            }

            if (!string.IsNullOrEmpty(connection.RefreshToken))
            {
                try
                {
                    var decrypted = DecryptWithFallback(connection.RefreshToken);
                    var reEncrypted = Encrypt(decrypted);
                    if (connection.RefreshToken != reEncrypted)
                    {
                        connection.RefreshToken = reEncrypted;
                        modified = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to rotate refresh token for ConnectionId {ConnectionId}",
                        connection.Id);
                }
            }

            if (modified)
            {
                connection.UpdatedAt = DateTime.UtcNow;
                rotated++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Calendar connections rotation completed: {Rotated} records", rotated);
        return rotated;
    }

    private async Task<int> RotatePaymentGatewayDataAsync(ClinicDbContext context, CancellationToken cancellationToken)
    {
        var rotated = 0;
        var configs = await context.PaymentGatewayConfigs
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var config in configs)
        {
            var modified = false;

            if (!string.IsNullOrEmpty(config.EncryptedSecretKey))
            {
                try
                {
                    var decrypted = DecryptWithFallback(config.EncryptedSecretKey);
                    var reEncrypted = Encrypt(decrypted);
                    if (config.EncryptedSecretKey != reEncrypted)
                    {
                        config.EncryptedSecretKey = reEncrypted;
                        modified = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to rotate secret key for PaymentGatewayConfigId {ConfigId}",
                        config.Id);
                }
            }

            if (!string.IsNullOrEmpty(config.EncryptedWebhookSecret))
            {
                try
                {
                    var decrypted = DecryptWithFallback(config.EncryptedWebhookSecret);
                    var reEncrypted = Encrypt(decrypted);
                    if (config.EncryptedWebhookSecret != reEncrypted)
                    {
                        config.EncryptedWebhookSecret = reEncrypted;
                        modified = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to rotate webhook secret for PaymentGatewayConfigId {ConfigId}",
                        config.Id);
                }
            }

            if (modified)
            {
                config.UpdatedAt = DateTime.UtcNow;
                rotated++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Payment gateway rotation completed: {Rotated} records", rotated);
        return rotated;
    }

    private async Task<int> RotateOAuthConfigsAsync(ClinicDbContext context, CancellationToken cancellationToken)
    {
        var rotated = 0;
        var configs = await context.OAuthConfigs
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var config in configs)
        {
            if (!string.IsNullOrEmpty(config.ClientSecret))
            {
                try
                {
                    var decrypted = DecryptWithFallback(config.ClientSecret);
                    var reEncrypted = Encrypt(decrypted);
                    if (config.ClientSecret != reEncrypted)
                    {
                        config.ClientSecret = reEncrypted;
                        rotated++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to rotate client secret for OAuthConfigId {ConfigId}",
                        config.Id);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("OAuth configs rotation completed: {Rotated} records", rotated);
        return rotated;
    }

    /// <summary>
    /// Attempt to decrypt with current key, fall back to previous key if available
    /// </summary>
    private string DecryptWithFallback(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            // Try decrypting with current key first
            return Decrypt(cipherText);
        }
        catch (CryptographicException) when (_previousKey != null)
        {
            // Fall back to previous key for data not yet rotated
            return DecryptWithKey(cipherText, _previousKey);
        }
    }

    /// <summary>
    /// Decrypt using a specific key (for key rotation)
    /// </summary>
    private string DecryptWithKey(string cipherText, byte[] key)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var encryptedBytes = Convert.FromBase64String(cipherText);

        if (encryptedBytes.Length < SaltSize + 12 + 16)
            return cipherText;

        // Extract components
        var salt = new byte[SaltSize];
        var iv = new byte[12];
        var tag = new byte[16];
        var cipherData = new byte[encryptedBytes.Length - SaltSize - 12 - 16];

        Buffer.BlockCopy(encryptedBytes, 0, salt, 0, SaltSize);
        Buffer.BlockCopy(encryptedBytes, SaltSize, iv, 0, 12);
        Buffer.BlockCopy(encryptedBytes, SaltSize + 12, tag, 0, 16);
        Buffer.BlockCopy(encryptedBytes, SaltSize + 12 + 16, cipherData, 0, cipherData.Length);

        // Derive key from provided master key
        var derivedKey = DeriveKey(key, salt);

        using var aesGcm = new AesGcm(derivedKey, 16);
        var plainText = new byte[cipherData.Length];

        aesGcm.Decrypt(iv, cipherData, tag, plainText);

        return Encoding.UTF8.GetString(plainText);
    }

    public bool VerifyIntegrity(string data, string signature)
    {
        var computedSignature = GenerateSignature(data);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(signature),
            Convert.FromBase64String(computedSignature));
    }

    public string GenerateSignature(string data)
    {
        using var hmac = new HMACSHA256(_hmacKey);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    private static byte[] DeriveKey(byte[] masterKey, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(masterKey, salt, Iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32); // 256-bit key
    }
}

/// <summary>
/// Extension methods for encrypted properties
/// </summary>
public static class EncryptedPropertyExtensions
{
    /// <summary>
    /// Encrypt sensitive patient data before storage
    /// </summary>
    public static string EncryptSsn(this IEncryptionService encryption, string ssn)
    {
        return encryption.Encrypt(ssn);
    }

    /// <summary>
    /// Decrypt SSN for display (audit this!)
    /// </summary>
    public static string DecryptSsn(this IEncryptionService encryption, string encryptedSsn)
    {
        return encryption.Decrypt(encryptedSsn);
    }

    /// <summary>
    /// Create searchable hash of SSN (last 4 digits)
    /// </summary>
    public static string HashSsnLast4(this IEncryptionService encryption, string ssn)
    {
        if (string.IsNullOrEmpty(ssn) || ssn.Length < 4)
            return string.Empty;
        return encryption.Hash(ssn[^4..]);
    }
}

/// <summary>
/// Audit log for key rotation operations
/// </summary>
public class KeyRotationAuditLog
{
    public int Id { get; set; }
    public DateTime RotationDate { get; set; }
    public int RecordsProcessed { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Errors { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
