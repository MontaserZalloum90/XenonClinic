namespace Xenon.Platform.Application;

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

    public static implicit operator Result<T>(T value) => new(value, true, null);
    public static implicit operator Result<T>(string error) => new(default!, false, error);
}
