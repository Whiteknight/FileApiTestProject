using static TestProject.Api.Assert;

namespace TestProject.Api;

public readonly record struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;
    private readonly bool _isValue;

    public Result(bool isValue, T? value, Error? error)
    {
        _isValue = isValue;
        if (isValue && value is not null)
        {
            _value = value;
            _error = default;
        }
        else if (!isValue && error is not null)
        {
            _error = error;
            _value = default;
        }
        else
        {
            throw new InvalidOperationException("Result<T> must be exactly one of value or error, without nulls");
        }
    }

    public bool IsError => !_isValue && _error is not null;

    public bool IsSuccess => _isValue && _value is not null;

    public static implicit operator Result<T>(T value)
        => new Result<T>(true, value, default);

    public static implicit operator Result<T>(Error error)
        => new Result<T>(false, default, error);

    public T GetValueOrDefault(T defaultValue)
        => IsSuccess
            ? _value!
            : defaultValue;

    public T GetValueOrThrow()
        => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Requested value object but none is available");

    public Error GetErrorOrThrow()
        => IsError
            ? _error!
            : throw new InvalidOperationException("Requested error object but none is available");

    public TOut Match<TOut>(Func<T, TOut> onValue, Func<Error, TOut> onError)
    {
        if (IsSuccess)
            return NotNull(onValue)(_value!);
        if (IsError)
            return NotNull(onError)(_error!);
        throw new InvalidOperationException("Invalid result state");
    }

    public Result<TOut> Then<TOut>(Func<T, Result<TOut>> bind)
    {
        if (IsSuccess)
            return NotNull(bind)(_value!);
        if (IsError)
            return _error!;
        throw new InvalidOperationException("Invalid result state");
    }

    public Result<T> Switch(Action<T> onValue, Action<Error> onError)
    {
        if (IsSuccess)
            NotNull(onValue)(_value!);
        else if (IsError)
            NotNull(onError)(_error!);
        else
            throw new InvalidOperationException("Invalid result state");
        return this;
    }

    public Result<T> OnSuccess(Action<T> onValue)
        => Switch(onValue, static _ => { });

    public Result<T> OnError(Action<Error> onError)
        => Switch(static _ => { }, onError);
}