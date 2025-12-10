using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Xenon.Platform.Infrastructure.Services;

/// <summary>
/// Service for validating passwords against configurable security policies.
/// Implements OWASP password guidelines.
/// </summary>
public interface IPasswordPolicyService
{
    PasswordValidationResult Validate(string password);
    PasswordStrength GetStrength(string password);
    bool IsCommonPassword(string password);
}

public class PasswordPolicyService : IPasswordPolicyService
{
    private readonly PasswordPolicyOptions _options;
    private readonly HashSet<string> _commonPasswords;

    public PasswordPolicyService(IConfiguration configuration)
    {
        _options = new PasswordPolicyOptions();
        configuration.GetSection("PasswordPolicy").Bind(_options);

        // Top 100 most common passwords (subset for efficiency)
        _commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password", "123456", "12345678", "qwerty", "abc123", "monkey", "1234567",
            "letmein", "trustno1", "dragon", "baseball", "iloveyou", "master", "sunshine",
            "ashley", "bailey", "shadow", "123123", "654321", "superman", "qazwsx",
            "michael", "football", "password1", "password123", "welcome", "jesus", "ninja",
            "mustang", "password1!", "admin", "admin123", "root", "toor", "pass", "test",
            "guest", "master123", "changeme", "12345", "1234", "123456789", "0987654321",
            "passw0rd", "p@ssword", "p@ssw0rd", "qwerty123", "administrator", "login",
            "welcome1", "hello123", "password!", "Password1", "Password123", "P@ssword1",
            "letmein123", "welcome123", "admin1234", "password12", "1q2w3e4r", "12341234",
            "secret", "secret123", "access", "access123", "123qwe", "qwe123", "987654321",
            "111111", "000000", "121212", "696969", "batman", "maggie", "whatever",
            "princess", "killer", "soccer", "harley", "charlie", "robert", "thomas",
            "hockey", "ranger", "daniel", "starwars", "klaster", "112233", "654321",
            "jordan23", "jennifer", "hunter", "pepper", "zxcvbn", "zxcvbnm", "asdfgh"
        };
    }

    public PasswordValidationResult Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            return new PasswordValidationResult(false, new[] { "Password is required" });
        }

        if (password.Length < _options.MinLength)
        {
            errors.Add($"Password must be at least {_options.MinLength} characters long");
        }

        if (_options.MaxLength > 0 && password.Length > _options.MaxLength)
        {
            errors.Add($"Password must not exceed {_options.MaxLength} characters");
        }

        if (_options.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter");
        }

        if (_options.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase letter");
        }

        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit");
        }

        if (_options.RequireSpecialCharacter && !Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            errors.Add("Password must contain at least one special character (!@#$%^&*()_+-=[]{}|;':\",./<>?)");
        }

        if (_options.PreventCommonPasswords && IsCommonPassword(password))
        {
            errors.Add("This password is too common. Please choose a more secure password");
        }

        // Check for sequential characters (e.g., "abc", "123")
        if (_options.PreventSequentialCharacters && HasSequentialCharacters(password, 3))
        {
            errors.Add("Password cannot contain 3 or more sequential characters (e.g., 'abc', '123')");
        }

        // Check for repeated characters (e.g., "aaa", "111")
        if (_options.PreventRepeatedCharacters && HasRepeatedCharacters(password, 3))
        {
            errors.Add("Password cannot contain 3 or more repeated characters (e.g., 'aaa', '111')");
        }

        return new PasswordValidationResult(errors.Count == 0, errors);
    }

    public PasswordStrength GetStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordStrength.VeryWeak;

        var score = 0;

        // Length scoring
        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;

        // Character variety
        if (password.Any(char.IsLower)) score++;
        if (password.Any(char.IsUpper)) score++;
        if (password.Any(char.IsDigit)) score++;
        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) score++;

        // Bonus for mixed character types
        var charTypes = 0;
        if (password.Any(char.IsLower)) charTypes++;
        if (password.Any(char.IsUpper)) charTypes++;
        if (password.Any(char.IsDigit)) charTypes++;
        if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) charTypes++;

        if (charTypes >= 4) score += 2;

        // Penalties
        if (IsCommonPassword(password)) score -= 3;
        if (HasSequentialCharacters(password, 3)) score -= 1;
        if (HasRepeatedCharacters(password, 3)) score -= 1;

        return score switch
        {
            <= 2 => PasswordStrength.VeryWeak,
            3 or 4 => PasswordStrength.Weak,
            5 or 6 => PasswordStrength.Fair,
            7 or 8 => PasswordStrength.Strong,
            _ => PasswordStrength.VeryStrong
        };
    }

    public bool IsCommonPassword(string password)
    {
        return _commonPasswords.Contains(password) ||
               _commonPasswords.Contains(password.ToLower());
    }

    private static bool HasSequentialCharacters(string password, int sequenceLength)
    {
        if (password.Length < sequenceLength)
            return false;

        for (int i = 0; i <= password.Length - sequenceLength; i++)
        {
            bool isSequential = true;
            bool isReverseSequential = true;

            for (int j = 1; j < sequenceLength; j++)
            {
                if (password[i + j] != password[i + j - 1] + 1)
                    isSequential = false;
                if (password[i + j] != password[i + j - 1] - 1)
                    isReverseSequential = false;
            }

            if (isSequential || isReverseSequential)
                return true;
        }

        return false;
    }

    private static bool HasRepeatedCharacters(string password, int repeatLength)
    {
        if (password.Length < repeatLength)
            return false;

        for (int i = 0; i <= password.Length - repeatLength; i++)
        {
            if (password.Skip(i).Take(repeatLength).Distinct().Count() == 1)
                return true;
        }

        return false;
    }
}

public class PasswordPolicyOptions
{
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;
    public bool PreventCommonPasswords { get; set; } = true;
    public bool PreventSequentialCharacters { get; set; } = true;
    public bool PreventRepeatedCharacters { get; set; } = true;
    public int PasswordHistoryCount { get; set; } = 5;
    public int PasswordExpiryDays { get; set; } = 90;
}

public record PasswordValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static PasswordValidationResult Success => new(true, Array.Empty<string>());
}

public enum PasswordStrength
{
    VeryWeak = 1,
    Weak = 2,
    Fair = 3,
    Strong = 4,
    VeryStrong = 5
}
