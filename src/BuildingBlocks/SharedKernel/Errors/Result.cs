namespace SharedKernel;

/// <summary>
/// Non-generic result (success/failure with optional error).
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && !error.IsNone)
            throw new InvalidOperationException(
                "A successful result cannot have a non-empty error.");

        if (!isSuccess && error.IsNone)
            throw new InvalidOperationException(
                "A failed result must have a non-empty error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    // Helpers that forward to Result<T>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

/// <summary>
/// Generic result returning a value or an error.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "Cannot access Value when the result is a failure.");

    protected Result(bool isSuccess, T? value, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<T> Success(T value)
        => new(true, value, Error.None);

    public static new Result<T> Failure(Error error)
    => new(false, default, error);

    // Simple map
    public Result<K> Map<K>(Func<T, K> mapper)
        => IsFailure ? Result<K>.Failure(Error) : Result<K>.Success(mapper(Value));

    // Simple bind / flat-map
    public Result<K> Bind<K>(Func<T, Result<K>> binder)
        => IsFailure ? Result<K>.Failure(Error) : binder(Value);

    public async Task<Result<K>> BindAsync<K>(Func<T, Task<Result<K>>> binder)
        => IsFailure ? Result<K>.Failure(Error) : await binder(Value);
}
