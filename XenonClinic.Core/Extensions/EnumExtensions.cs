using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace XenonClinic.Core.Extensions;

/// <summary>
/// Extension methods for enum types
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the display name from the Display attribute or uses the enum name
    /// </summary>
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null) return value.ToString();

        var attribute = field.GetCustomAttribute<DisplayAttribute>();
        return attribute?.Name ?? value.ToString();
    }

    /// <summary>
    /// Converts a string to an enum value
    /// </summary>
    public static T? ToEnum<T>(this string value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<T>(value, true, out var result) ? result : null;
    }

    /// <summary>
    /// Converts an enum to its string representation
    /// </summary>
    public static string ToEnumString<T>(this T? value) where T : struct, Enum
    {
        return value?.ToString() ?? string.Empty;
    }
}
