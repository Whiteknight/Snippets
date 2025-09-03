using static BaseFunctional.Assert;

namespace BaseFunctional;

public readonly record struct Maybe<T> : IEquatable<Maybe<T>>, IEquatable<T>
{
    private readonly bool _hasValue;
    private readonly T? _value;

    public Maybe(T? value, bool hasValue)
    {
        if (hasValue)
        {
            _value = NotNull(value);
            _hasValue = true;
        }
    }

    public static implicit operator Maybe<T>(T value) => new Maybe<T>(value, true);

    public TOut Match<TOut>(Func<T, TOut> onValue, Func<TOut> onNoValue)
    {
        if (_hasValue && _value is not null)
            return NotNull(onValue)(_value!);
        if (!_hasValue)
            return NotNull(onNoValue)();
        throw new InvalidOperationException("Result is in an invalid state");
    }

    public TOut Match<TOut, TData>(TData data, Func<T, TData, TOut> onValue, Func<TData, TOut> onNoValue)
    {
        if (_hasValue && _value is not null)
            return NotNull(onValue)(_value!, data);
        if (!_hasValue)
            return NotNull(onNoValue)(data);
        throw new InvalidOperationException("Result is in an invalid state");
    }

    public Result<T, TError> ToResult<TError>(Func<TError> createError)
        => Match(createError,
            static (t, _) => new Result<T, TError>(t, default, 0),
            static c => new Result<T, TError>(default, c(), 1));

    public T GetValueOrDefault(T defaultValue)
        => Match(
            defaultValue,
            static (t, _) => t,
            static d => d);

    public bool Is(Func<T, bool> predicate)
        => Match(
            predicate,
            static (v, p) => p(v),
            static _ => false);

    public bool Equals(T? other)
        => other is not null && Match(
            other,
            static (v, o) => v.Equals(o),
            static _ => false);
}

public static class MaybeExtensions
{
    public static Maybe<T> Flatten<T>(Maybe<Maybe<T>> maybe)
        => maybe.Match(static m => m, static () => default);
}

public static class DictionaryMaybeExtensions
{
    public static Maybe<TValue> MaybeGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
        => NotNull(dict).TryGetValue(key, out var value)
            ? new Maybe<TValue>(value, true)
            : default;
}

