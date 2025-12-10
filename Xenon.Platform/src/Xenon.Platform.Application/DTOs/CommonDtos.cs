namespace Xenon.Platform.Application.DTOs;

/// <summary>
/// Standard API response wrapper for successful operations
/// </summary>
public record ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public T Data { get; init; } = default!;
    public string? Message { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Data = data, Message = message };
}

/// <summary>
/// Standard API response wrapper for operations without data
/// </summary>
public record ApiResponse
{
    public bool Success { get; init; } = true;
    public string? Message { get; init; }

    public static ApiResponse Ok(string? message = null) =>
        new() { Message = message };
}

/// <summary>
/// Standard API error response
/// </summary>
public record ApiErrorResponse
{
    public bool Success { get; init; } = false;
    public string Error { get; init; } = string.Empty;
    public IDictionary<string, string[]>? ValidationErrors { get; init; }

    public static ApiErrorResponse Fail(string error) =>
        new() { Error = error };

    public static ApiErrorResponse ValidationFail(string error, IDictionary<string, string[]> errors) =>
        new() { Error = error, ValidationErrors = errors };
}
