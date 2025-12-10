using Microsoft.Extensions.Logging;

namespace Xenon.Platform.Infrastructure.Services;

public interface IPasswordHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHashingService : IPasswordHashingService
{
    private readonly ILogger<PasswordHashingService>? _logger;

    public PasswordHashingService(ILogger<PasswordHashingService>? logger = null)
    {
        _logger = logger;
    }

    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException ex)
        {
            // Invalid hash format - this is expected for corrupted/invalid hashes
            _logger?.LogWarning(ex, "Invalid password hash format encountered");
            return false;
        }
        catch (ArgumentException ex)
        {
            // Invalid argument (e.g., invalid hash version)
            _logger?.LogWarning(ex, "Invalid argument during password verification");
            return false;
        }
    }
}
