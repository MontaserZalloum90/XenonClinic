namespace XenonClinic.Core.Common;

/// <summary>
/// Represents the result of an operation that may fail
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(string error) => new(default!, false, error);

    public static implicit operator Result(string error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation that returns a value and may fail
/// </summary>
public class Result<T> : Result
{
    private readonly T _value;

    internal Result(T value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    /// <summary>
    /// Returns the value if successful, or the default value if failed
    /// </summary>
    public T? ValueOrDefault => IsSuccess ? _value : default;

    /// <summary>
    /// Maps the result to a new type if successful
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess
            ? Result.Success(mapper(_value))
            : Result.Failure<TNew>(Error!);
    }

    /// <summary>
    /// Binds the result to another operation if successful
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        return IsSuccess
            ? binder(_value)
            : Result.Failure<TNew>(Error!);
    }

    public static implicit operator Result<T>(T value) => new(value, true, null);
    public static implicit operator Result<T>(string error) => new(default!, false, error);
}

/// <summary>
/// Extension methods for Result pattern
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Executes an action if the result is successful
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);
        return result;
    }

    /// <summary>
    /// Executes an action if the result is a failure
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<string> action)
    {
        if (result.IsFailure)
            action(result.Error!);
        return result;
    }

    /// <summary>
    /// Converts a nullable value to a Result
    /// </summary>
    public static Result<T> ToResult<T>(this T? value, string errorIfNull) where T : class
    {
        return value is not null
            ? Result.Success(value)
            : Result.Failure<T>(errorIfNull);
    }

    /// <summary>
    /// Converts a nullable value type to a Result
    /// </summary>
    public static Result<T> ToResult<T>(this T? value, string errorIfNull) where T : struct
    {
        return value.HasValue
            ? Result.Success(value.Value)
            : Result.Failure<T>(errorIfNull);
    }
}
