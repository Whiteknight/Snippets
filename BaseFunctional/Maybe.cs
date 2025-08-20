using static BaseFunctional.Assert;

namespace BaseFunctional;

public readonly record struct Maybe<T>
{
    private readonly bool _isSuccess;
    private readonly T? _value;

    public Maybe(T? value, bool isSuccess)
    {
        if (isSuccess)
        {
            _value = NotNull(value);
            _isSuccess = true;
        }
    }

    public static implicit operator Maybe<T>(T value) => new Maybe<T>(value, true);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<TOut> onError)
    {
        if (_isSuccess && _value is not null)
            return NotNull(onSuccess)(_value!);
        if (!_isSuccess)
            return NotNull(onError)();
        throw new InvalidOperationException("Result is in an invalid state");
    }

    public TOut Match<TOut, TData>(TData data, Func<T, TData, TOut> onSuccess, Func<TData, TOut> onError)
    {
        if (_isSuccess && _value is not null)
            return NotNull(onSuccess)(_value!, data);
        if (!_isSuccess)
            return NotNull(onError)(data);
        throw new InvalidOperationException("Result is in an invalid state");
    }

    public Result<T, TError> MapError<TError>(Func<TError> createError)
        => Match(createError,
            static (t, _) => new Result<T, TError>(t, default, true),
            static c => new Result<T, TError>(default, c(), false));

    public T GetValueOrDefault(T defaultValue)
        => Match(
            defaultValue,
            static (t, _) => t,
            static d => d);
}

public static class MaybeExtensions
{
    public static Maybe<T> Flatten<T>(Maybe<Maybe<T>> maybe)
        => maybe.Match(static m => m, static () => default);
}

