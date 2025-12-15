using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Password reset service implementation.
/// </summary>
public class PasswordResetService : IPasswordResetService
{
    private readonly ILogger<PasswordResetService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;
    private readonly PasswordPolicy _policy;
    private readonly HttpClient _httpClient;

    private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "123456", "12345678", "qwerty", "abc123", "monkey", "1234567",
        "letmein", "trustno1", "dragon", "baseball", "iloveyou", "master", "sunshine",
        "ashley", "bailey", "shadow", "passw0rd", "123456789", "654321", "superman",
        "qazwsx", "michael", "football", "password1", "password123", "welcome",
        "jesus", "ninja", "mustang", "password2", "login", "admin", "administrator"
    };

    public PasswordResetService(
        ILogger<PasswordResetService> logger,
        IMemoryCache cache,
        IEmailService emailService,
        IOptions<PasswordPolicy> policy,
        HttpClient httpClient)
    {
        _logger = logger;
        _cache = cache;
        _emailService = emailService;
        _policy = policy.Value;
        _httpClient = httpClient;
    }

    public async Task<PasswordResetResult> InitiateResetAsync(string email)
    {
        // Rate limiting check
        var rateLimitKey = $"reset_attempts_{email}";
        var attempts = _cache.GetOrCreate(rateLimitKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return 0;
        });

        if (attempts >= _policy.MaxResetAttemptsPerHour)
        {
            _logger.LogWarning("Password reset rate limit exceeded for {Email}", MaskEmail(email));
            return new PasswordResetResult(false, "Too many reset attempts. Please try again later.");
        }

        _cache.Set(rateLimitKey, attempts + 1, TimeSpan.FromHours(1));

        // Generate secure token
        var token = GenerateSecureToken();
        var hashedToken = HashToken(token);
        var expiresAt = DateTime.UtcNow.AddMinutes(_policy.ResetTokenExpiryMinutes);

        // Store hashed token
        var tokenKey = $"reset_token_{email}";
        _cache.Set(tokenKey, new ResetTokenData(hashedToken, expiresAt), TimeSpan.FromMinutes(_policy.ResetTokenExpiryMinutes));

        // Send email with reset link (using configurable base URL)
        var baseUrl = _policy.ResetPasswordBaseUrl.TrimEnd('/');
        var resetLink = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        await _emailService.SendAsync(new EmailMessage
        {
            To = email,
            Subject = "Reset Your Password - XenonClinic",
            Body = GenerateResetEmailBody(resetLink, _policy.ResetTokenExpiryMinutes),
            IsHtml = true
        });

        _logger.LogInformation("Password reset initiated for {Email}", MaskEmail(email));

        return new PasswordResetResult(true, null, null, expiresAt);
    }

    public Task<bool> VerifyResetTokenAsync(string email, string token)
    {
        var tokenKey = $"reset_token_{email}";

        if (!_cache.TryGetValue<ResetTokenData>(tokenKey, out var tokenData))
        {
            return Task.FromResult(false);
        }

        if (tokenData.ExpiresAt < DateTime.UtcNow)
        {
            _cache.Remove(tokenKey);
            return Task.FromResult(false);
        }

        var hashedToken = HashToken(token);
        return Task.FromResult(hashedToken == tokenData.HashedToken);
    }

    public async Task<PasswordResetResult> ResetPasswordAsync(string email, string token, string newPassword)
    {
        // Verify token
        if (!await VerifyResetTokenAsync(email, token))
        {
            return new PasswordResetResult(false, "Invalid or expired reset token.");
        }

        // Validate new password
        var validation = ValidatePassword(newPassword);
        if (!validation.IsValid)
        {
            return new PasswordResetResult(false, string.Join(" ", validation.Errors));
        }

        // Check if password is compromised
        if (await IsPasswordCompromisedAsync(newPassword))
        {
            return new PasswordResetResult(false, "This password has been found in a data breach. Please choose a different password.");
        }

        // In production: Update password in database and invalidate token
        var tokenKey = $"reset_token_{email}";
        _cache.Remove(tokenKey);

        _logger.LogInformation("Password reset completed for {Email}", MaskEmail(email));

        return new PasswordResetResult(true);
    }

    public async Task<PasswordResetResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        // In production: Verify current password against database

        // Validate new password
        var validation = ValidatePassword(newPassword);
        if (!validation.IsValid)
        {
            return new PasswordResetResult(false, string.Join(" ", validation.Errors));
        }

        // Check if password is compromised
        if (await IsPasswordCompromisedAsync(newPassword))
        {
            return new PasswordResetResult(false, "This password has been found in a data breach. Please choose a different password.");
        }

        // In production: Update password in database

        _logger.LogInformation("Password changed for user {UserId}", userId);

        return new PasswordResetResult(true);
    }

    public XenonClinic.Core.Interfaces.PasswordValidationResult ValidatePassword(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            return new XenonClinic.Core.Interfaces.PasswordValidationResult
            {
                IsValid = false,
                Errors = new List<string> { "Password is required." },
                StrengthScore = 0,
                StrengthLabel = "Very Weak"
            };
        }

        if (password.Length < _policy.MinimumLength)
        {
            errors.Add($"Password must be at least {_policy.MinimumLength} characters.");
        }

        if (password.Length > _policy.MaximumLength)
        {
            errors.Add($"Password must not exceed {_policy.MaximumLength} characters.");
        }

        if (_policy.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (_policy.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        if (_policy.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit.");
        }

        if (_policy.RequireSpecialCharacter && !Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            errors.Add("Password must contain at least one special character.");
        }

        if (password.Distinct().Count() < _policy.MinimumUniqueCharacters)
        {
            errors.Add($"Password must contain at least {_policy.MinimumUniqueCharacters} unique characters.");
        }

        if (_policy.PreventCommonPasswords && CommonPasswords.Contains(password))
        {
            errors.Add("This password is too common. Please choose a stronger password.");
        }

        var strength = CalculatePasswordStrength(password);
        var strengthLabel = GetStrengthLabel(strength);

        return new XenonClinic.Core.Interfaces.PasswordValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            StrengthScore = strength,
            StrengthLabel = strengthLabel
        };
    }

    public async Task<bool> IsPasswordCompromisedAsync(string password)
    {
        try
        {
            // Use k-Anonymity model with HaveIBeenPwned API
            var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(password));
            var hash = Convert.ToHexString(sha1);
            var prefix = hash[..5];
            var suffix = hash[5..];

            var response = await _httpClient.GetStringAsync($"https://api.pwnedpasswords.com/range/{prefix}");

            return response.Split('\n')
                .Select(line => line.Trim().Split(':'))
                .Any(parts => parts.Length == 2 && parts[0].Equals(suffix, StringComparison.OrdinalIgnoreCase));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP error checking password against HaveIBeenPwned");
            return false; // Don't block user if service is unavailable
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout checking password against HaveIBeenPwned");
            return false; // Don't block user if service is unavailable
        }
    }

    public Task<DateTime?> GetTokenExpiryAsync(string email, string token)
    {
        var tokenKey = $"reset_token_{email}";

        if (!_cache.TryGetValue<ResetTokenData>(tokenKey, out var tokenData))
        {
            return Task.FromResult<DateTime?>(null);
        }

        var hashedToken = HashToken(token);
        if (hashedToken != tokenData.HashedToken)
        {
            return Task.FromResult<DateTime?>(null);
        }

        return Task.FromResult<DateTime?>(tokenData.ExpiresAt);
    }

    public Task InvalidateAllTokensAsync(string email)
    {
        var tokenKey = $"reset_token_{email}";
        _cache.Remove(tokenKey);

        _logger.LogInformation("All reset tokens invalidated for {Email}", MaskEmail(email));
        return Task.CompletedTask;
    }

    #region Private Methods

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 2) return "***" + email[atIndex..];
        return email[..2] + new string('*', atIndex - 2) + email[atIndex..];
    }

    private static int CalculatePasswordStrength(string password)
    {
        var score = 0;

        // Length contribution
        score += Math.Min(password.Length * 4, 40);

        // Character variety
        if (password.Any(char.IsUpper)) score += 10;
        if (password.Any(char.IsLower)) score += 10;
        if (password.Any(char.IsDigit)) score += 10;
        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) score += 15;

        // Unique characters bonus
        var uniqueRatio = (double)password.Distinct().Count() / password.Length;
        score += (int)(uniqueRatio * 15);

        return Math.Min(score, 100);
    }

    private static string GenerateResetEmailBody(string resetLink, int expiryMinutes)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; }}
        .footer {{ margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Reset Your Password</h2>
        <p>We received a request to reset your password. Click the button below to create a new password:</p>
        <p><a href='{resetLink}' class='button'>Reset Password</a></p>
        <p>This link will expire in {expiryMinutes} minutes.</p>
        <p>If you didn't request a password reset, please ignore this email or contact support if you have concerns.</p>
        <div class='footer'>
            <p>This email was sent by XenonClinic. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GetStrengthLabel(int score)
    {
        return score switch
        {
            >= 80 => "Strong",
            >= 60 => "Good",
            >= 40 => "Fair",
            >= 20 => "Weak",
            _ => "Very Weak"
        };
    }

    #endregion

    private record ResetTokenData(string HashedToken, DateTime ExpiresAt);
}
