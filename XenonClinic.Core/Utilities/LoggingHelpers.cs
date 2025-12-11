namespace XenonClinic.Core.Utilities;

/// <summary>
/// Helper methods for secure logging that mask sensitive PII data.
/// BUG FIX: Prevents email addresses and other PII from being logged in plain text.
/// </summary>
public static class LoggingHelpers
{
    /// <summary>
    /// Masks an email address for logging purposes.
    /// Example: "john.doe@example.com" becomes "jo******@example.com"
    /// </summary>
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return "[empty]";

        var atIndex = email.IndexOf('@');
        if (atIndex < 0)
            return "***[invalid]";

        if (atIndex <= 2)
            return "***" + email[atIndex..];

        return email[..2] + new string('*', Math.Min(atIndex - 2, 6)) + email[atIndex..];
    }

    /// <summary>
    /// Masks a phone number for logging purposes.
    /// Example: "+1234567890" becomes "+12****90"
    /// </summary>
    public static string MaskPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone))
            return "[empty]";

        if (phone.Length <= 4)
            return "****";

        return $"{phone[..3]}****{phone[^2..]}";
    }

    /// <summary>
    /// Masks an IP address for logging purposes (keeps first two octets).
    /// Example: "192.168.1.100" becomes "192.168.***.***"
    /// </summary>
    public static string MaskIpAddress(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return "[empty]";

        var parts = ipAddress.Split('.');
        if (parts.Length == 4)
        {
            return $"{parts[0]}.{parts[1]}.***.***";
        }

        // IPv6 or other formats - mask more aggressively
        if (ipAddress.Length > 8)
        {
            return ipAddress[..8] + "***";
        }

        return "***";
    }
}
