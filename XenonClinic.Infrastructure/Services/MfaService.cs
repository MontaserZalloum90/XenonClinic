using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Multi-factor authentication service implementation with database persistence.
/// </summary>
public class MfaService : IMfaService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MfaService> _logger;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ClinicDbContext _dbContext;

    private const string Issuer = "XenonClinic";
    private const int TotpDigits = 6;
    private const int TotpPeriod = 30; // seconds
    private const int CodeExpirationMinutes = 10;

    public MfaService(
        IMemoryCache cache,
        ILogger<MfaService> logger,
        ISmsService smsService,
        IEmailService emailService,
        ClinicDbContext dbContext)
    {
        _cache = cache;
        _logger = logger;
        _smsService = smsService;
        _emailService = emailService;
        _dbContext = dbContext;
    }

    public Task<MfaSetupResult> GenerateTotpSecretAsync(string userId)
    {
        var secret = GenerateSecret();
        var base32Secret = Base32Encode(secret);

        // Store temporarily until verified
        _cache.Set($"mfa_setup_{userId}", base32Secret, TimeSpan.FromMinutes(15));

        var qrCodeUri = GenerateQrCodeUri(userId, base32Secret);
        var manualKey = FormatManualEntryKey(base32Secret);

        _logger.LogInformation("Generated TOTP secret for user {UserId}", userId);

        return Task.FromResult(new MfaSetupResult(base32Secret, qrCodeUri, manualKey));
    }

    public async Task<bool> VerifyTotpCodeAsync(string userId, string code)
    {
        var mfaConfig = await _dbContext.UserMfaConfigurations
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (mfaConfig == null || string.IsNullOrEmpty(mfaConfig.TotpSecret))
        {
            return false;
        }

        var isValid = VerifyTotp(mfaConfig.TotpSecret, code);

        if (isValid)
        {
            _logger.LogInformation("TOTP verification successful for user {UserId}", userId);
        }
        else
        {
            _logger.LogWarning("TOTP verification failed for user {UserId}", userId);
        }

        return isValid;
    }

    public async Task<bool> EnableTotpAsync(string userId, string code)
    {
        var cacheKey = $"mfa_setup_{userId}";
        if (!_cache.TryGetValue(cacheKey, out string? secret) || string.IsNullOrEmpty(secret))
        {
            return false;
        }

        if (!VerifyTotp(secret, code))
        {
            return false;
        }

        // Get or create MFA configuration in database
        var mfaConfig = await _dbContext.UserMfaConfigurations
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (mfaConfig == null)
        {
            mfaConfig = new UserMfaConfiguration
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.UserMfaConfigurations.Add(mfaConfig);
        }

        mfaConfig.TotpSecret = secret;
        mfaConfig.IsEnabled = true;
        mfaConfig.EnabledMethod = (int)MfaMethod.Totp;
        mfaConfig.EnabledAt = DateTime.UtcNow;
        mfaConfig.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _cache.Remove(cacheKey);
        _logger.LogInformation("TOTP MFA enabled for user {UserId}", userId);

        return true;
    }

    public async Task DisableMfaAsync(string userId)
    {
        var mfaConfig = await _dbContext.UserMfaConfigurations
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (mfaConfig != null)
        {
            mfaConfig.IsEnabled = false;
            mfaConfig.EnabledMethod = (int)MfaMethod.None;
            mfaConfig.TotpSecret = null;
            mfaConfig.BackupCodesJson = null;
            mfaConfig.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("MFA disabled for user {UserId}", userId);
    }

    public async Task<string> SendSmsCodeAsync(string userId, string phoneNumber)
    {
        var code = GenerateNumericCode(6);
        var cacheKey = $"mfa_sms_{userId}";

        _cache.Set(cacheKey, code, TimeSpan.FromMinutes(CodeExpirationMinutes));

        await _smsService.SendAsync(phoneNumber, $"Your XenonClinic verification code is: {code}");

        _logger.LogInformation("SMS verification code sent to user {UserId}", userId);
        // Security fix: Do not return the actual code - it should only be sent via SMS
        return "Code sent successfully";
    }

    public async Task<string> SendEmailCodeAsync(string userId, string email)
    {
        var code = GenerateNumericCode(6);
        var cacheKey = $"mfa_email_{userId}";

        _cache.Set(cacheKey, code, TimeSpan.FromMinutes(CodeExpirationMinutes));

        await _emailService.SendAsync(new EmailMessage
        {
            To = email,
            Subject = "XenonClinic Verification Code",
            Body = $"Your verification code is: {code}\n\nThis code expires in {CodeExpirationMinutes} minutes.",
            IsHtml = false
        });

        _logger.LogInformation("Email verification code sent to user {UserId}", userId);
        // Security fix: Do not return the actual code - it should only be sent via email
        return "Code sent successfully";
    }

    public Task<bool> VerifyCodeAsync(string userId, string code, MfaCodePurpose purpose)
    {
        var smsKey = $"mfa_sms_{userId}";
        var emailKey = $"mfa_email_{userId}";

        // SECURITY FIX: Use constant-time comparison to prevent timing attacks
        // Check SMS code
        if (_cache.TryGetValue(smsKey, out string? smsCode) && smsCode != null &&
            CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(smsCode),
                System.Text.Encoding.UTF8.GetBytes(code ?? string.Empty)))
        {
            _cache.Remove(smsKey);
            _logger.LogInformation("SMS code verified for user {UserId}, purpose: {Purpose}", userId, purpose);
            return Task.FromResult(true);
        }

        // Check email code
        if (_cache.TryGetValue(emailKey, out string? emailCode) && emailCode != null &&
            CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(emailCode),
                System.Text.Encoding.UTF8.GetBytes(code ?? string.Empty)))
        {
            _cache.Remove(emailKey);
            _logger.LogInformation("Email code verified for user {UserId}, purpose: {Purpose}", userId, purpose);
            return Task.FromResult(true);
        }

        _logger.LogWarning("Code verification failed for user {UserId}", userId);
        return Task.FromResult(false);
    }

    public async Task<IEnumerable<string>> GenerateBackupCodesAsync(string userId, int count = 10)
    {
        var codes = new List<string>();

        for (int i = 0; i < count; i++)
        {
            codes.Add(GenerateBackupCode());
        }

        // Get or create MFA configuration
        var mfaConfig = await _dbContext.UserMfaConfigurations
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (mfaConfig == null)
        {
            mfaConfig = new UserMfaConfiguration
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.UserMfaConfigurations.Add(mfaConfig);
        }

        // Store hashed backup codes as JSON
        var hashedCodes = codes.Select(HashCode).ToList();
        mfaConfig.BackupCodesJson = JsonSerializer.Serialize(hashedCodes);
        mfaConfig.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Generated {Count} backup codes for user {UserId}", count, userId);

        return codes;
    }

    public async Task<bool> VerifyBackupCodeAsync(string userId, string code)
    {
        var mfaConfig = await _dbContext.UserMfaConfigurations
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (mfaConfig == null || string.IsNullOrEmpty(mfaConfig.BackupCodesJson))
        {
            return false;
        }

        var backupCodes = JsonSerializer.Deserialize<List<string>>(mfaConfig.BackupCodesJson) ?? new List<string>();
        var hashedCode = HashCode(code);
        var matchingCode = backupCodes.FirstOrDefault(c => c == hashedCode);

        if (matchingCode != null)
        {
            backupCodes.Remove(matchingCode);
            mfaConfig.BackupCodesJson = JsonSerializer.Serialize(backupCodes);
            mfaConfig.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Backup code used for user {UserId}, {Remaining} remaining",
                userId, backupCodes.Count);
            return true;
        }

        return false;
    }

    public async Task<MfaStatus> GetMfaStatusAsync(string userId)
    {
        var mfaConfig = await _dbContext.UserMfaConfigurations
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (mfaConfig == null)
        {
            return new MfaStatus(false, MfaMethod.None, false, 0);
        }

        var backupCodeCount = 0;
        if (!string.IsNullOrEmpty(mfaConfig.BackupCodesJson))
        {
            var codes = JsonSerializer.Deserialize<List<string>>(mfaConfig.BackupCodesJson);
            backupCodeCount = codes?.Count ?? 0;
        }

        return new MfaStatus(
            mfaConfig.IsEnabled,
            (MfaMethod)mfaConfig.EnabledMethod,
            backupCodeCount > 0,
            backupCodeCount
        );
    }

    #region Private Helpers

    private static byte[] GenerateSecret(int length = 20)
    {
        var secret = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(secret);
        return secret;
    }

    private static string GenerateNumericCode(int length)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, length);
        return number.ToString().PadLeft(length, '0');
    }

    private static string GenerateBackupCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[5];
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string HashCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        int buffer = 0, bitsLeft = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;

            while (bitsLeft >= 5)
            {
                result.Append(alphabet[(buffer >> (bitsLeft - 5)) & 31]);
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
        {
            result.Append(alphabet[(buffer << (5 - bitsLeft)) & 31]);
        }

        return result.ToString();
    }

    private static string GenerateQrCodeUri(string userId, string secret)
    {
        var label = Uri.EscapeDataString($"{Issuer}:{userId}");
        return $"otpauth://totp/{label}?secret={secret}&issuer={Uri.EscapeDataString(Issuer)}&digits={TotpDigits}&period={TotpPeriod}";
    }

    private static string FormatManualEntryKey(string secret)
    {
        var formatted = new StringBuilder();
        for (int i = 0; i < secret.Length; i += 4)
        {
            if (i > 0) formatted.Append(' ');
            formatted.Append(secret.AsSpan(i, Math.Min(4, secret.Length - i)));
        }
        return formatted.ToString();
    }

    private static bool VerifyTotp(string secret, string code, int toleranceSteps = 1)
    {
        if (string.IsNullOrEmpty(code) || code.Length != TotpDigits)
            return false;

        var secretBytes = Base32Decode(secret);
        var currentStep = GetCurrentTimeStep();

        for (int i = -toleranceSteps; i <= toleranceSteps; i++)
        {
            var expectedCode = GenerateTotpCode(secretBytes, currentStep + i);
            if (expectedCode == code)
                return true;
        }

        return false;
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new List<byte>();
        int buffer = 0, bitsLeft = 0;

        foreach (var c in input.ToUpperInvariant())
        {
            var index = alphabet.IndexOf(c);
            if (index < 0) continue;

            buffer = (buffer << 5) | index;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                output.Add((byte)(buffer >> (bitsLeft - 8)));
                bitsLeft -= 8;
            }
        }

        return output.ToArray();
    }

    private static long GetCurrentTimeStep()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TotpPeriod;
    }

    private static string GenerateTotpCode(byte[] secret, long timeStep)
    {
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);

        var counter = new byte[8];
        Array.Copy(timeBytes, 0, counter, 8 - timeBytes.Length, timeBytes.Length);

        using var hmac = new HMACSHA1(secret);
        var hash = hmac.ComputeHash(counter);

        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24) |
                     ((hash[offset + 1] & 0xFF) << 16) |
                     ((hash[offset + 2] & 0xFF) << 8) |
                     (hash[offset + 3] & 0xFF);

        var otp = binary % (int)Math.Pow(10, TotpDigits);
        return otp.ToString().PadLeft(TotpDigits, '0');
    }

    #endregion
}
