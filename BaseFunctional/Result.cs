using System.Diagnostics.CodeAnalysis;
using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class Result
{
    [DoesNotReturn]
    public static T ThrowResultInvalidException<T>()
        => throw new InvalidOperationException("Result is in an invalid state");

    // Given two separate existing results, Join them together into a single new result.
    // If both input results are success, return a new result with a combined value.e
    // Otherwise if either result is an error, return a failure result with that error.
    public static Result<TOut, TE1, TE2> Combine<T1, TE1, T2, TE2, TOut>(this Result<T1, TE1> r1, Result<T2, TE2> r2, Func<T1, T2, TOut> combine)
        => r1.Match((combine: NotNull(combine), r2),
            static (v1, d1) => d1.r2.Match((v1, d1.combine),
                static (v2, d2) => new Result<TOut, TE1, TE2>(d2.combine(d2.v1, v2), default, default, 0),
                static (e2, _) => new Result<TOut, TE1, TE2>(default, default, e2, 2)),
            static (e1, _) => new Result<TOut, TE1, TE2>(default, e1, default, 1));

    // Given a result and an operation which returns a second result, Join them together into 
    // a single new result. If both input results are success, return a new combined result value.
    // Otherwise if either result is an error, return a failure result with that error.
    public static Result<TOut, TE1, TE2> Combine<T1, TE1, T2, TE2, TOut>(this Result<T1, TE1> r1, Func<Result<T2, TE2>> getR2, Func<T1, T2, TOut> combine)
        => r1.Match((combine: NotNull(combine), getR2),
            static (v1, d1) => d1.getR2().Match((v1, d1.combine),
                static (v2, d2) => new Result<TOut, TE1, TE2>(d2.combine(d2.v1, v2), default, default, 0),
                static (e2, _) => new Result<TOut, TE1, TE2>(default, default, e2, 2)),
            static (e1, _) => new Result<TOut, TE1, TE2>(default, e1, default, 1));

    public static Result<T2, T1> Invert<T1, T2>(this Result<T1, T2> result)
        => result.Match(
            t1 => new Result<T2, T1>(default, t1, 1),
            t2 => new Result<T2, T1>(t2, default, 0));

    public static Result<T, TE1> Create<T, TE1>(T value)
        => new Result<T, TE1>(value, default, 0);

    public static Result<T, TE1> Create<T, TE1>(TE1 error1)
        => new Result<T, TE1>(default, error1, 1);

    public static Result<T, TE1, TE2> Flatten<T, TE1, TE2>(this Result<Result<T, TE1>, TE2> result)
        => result.Match(
            static r1 => r1.Match(
                static v1 => new Result<T, TE1, TE2>(v1, default, default, 0),
                static e1 => new Result<T, TE1, TE2>(default, e1, default, 1)),
            static e2 => new Result<T, TE1, TE2>(default, default, e2, 2));

    public static Result<T, Exception> Try<T>(Func<T> function)
    {
        try
        {
            return function();
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public static Result<T, Exception> Try<T, TData>(TData data, Func<TData, T> function)
    {
        try
        {
            return function(data);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}

public readonly record struct Result<T, TE1> : IEquatable<T>
{
    private readonly int _index;
    private readonly T? _value;
    private readonly TE1? _error;

    public Result(T? value, TE1? error, int index)
    {
        _index = index;
        if (index == 0)
            _value = NotNull(value);
        else if (index == 1)
            _error = NotNull(error);
    }

    public bool IsSuccess => _index == 0 && _value is not null;
    public bool IsError => _index == 1 && _error is not null;
    public bool IsValid => IsSuccess || IsError;

    public static implicit operator Result<T, TE1>(T value)
        => new Result<T, TE1>(value, default, 0);

    public static implicit operator Result<T, TE1>(TE1 error)
        => new Result<T, TE1>(default, error, 1);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<TE1, TOut> onError)
    {
        if (_index == 0 && _value is not null)
            return NotNull(onSuccess)(_value!);
        if (_index == 1 && _error is not null)
            return NotNull(onError)(_error!);
        return Result.ThrowResultInvalidException<TOut>();
    }

    public TOut Match<TOut, TData>(TData data, Func<T, TData, TOut> onSuccess, Func<TE1, TData, TOut> onError)
    {
        if (_index == 0 && _value is not null)
            return NotNull(onSuccess)(_value!, data);
        if (_index == 1 && _error is not null)
            return NotNull(onError)(_error!, data);
        return Result.ThrowResultInvalidException<TOut>();
    }

    public void Switch(Action<T> onSuccess, Action<TE1> onError)
    {
        if (_index == 0 && _value is not null)
        {
            NotNull(onSuccess)(_value!);
            return;
        }
        if (_index == 1 && _error is not null)
        {
            NotNull(onError)(_error!);
            return;
        }
        Result.ThrowResultInvalidException<int>();
    }

    public void Switch<TData>(TData data, Action<T, TData> onSuccess, Action<TE1, TData> onError)
    {
        if (_index == 0 && _value is not null)
        {
            NotNull(onSuccess)(_value!, data);
            return;
        }
        if (_index == 1 && _error is not null)
        {
            NotNull(onError)(_error!, data);
            return;
        }
        Result.ThrowResultInvalidException<int>();
    }

    public Result<TOut, TE1> Bind<TOut>(Func<T, Result<TOut, TE1>> func)
        => Match(NotNull(func), static (v, f) => f(v), static (e, _) => e);

    // Synonym for "Bind", but probably easier to read
    public Result<TOut, TE1> And<TOut>(Func<T, Result<TOut, TE1>> func)
        => Match(NotNull(func), static (v, f) => f(v), static (e, _) => e);

    // Combine two results with different errors, into a single result with two possible errors
    // TODO: Looking for a better name for this. .And() creates ambiguities with the method above
    public Result<TOut, TE1, TE2> AndThen<TOut, TE2>(Func<T, Result<TOut, TE2>> func)
        => Match(
            NotNull(func),
            static (v, f) => f(v).Match(
                static v2 => new Result<TOut, TE1, TE2>(v2, default, default, 0),
                static e2 => new Result<TOut, TE1, TE2>(default, default, e2, 2)),
            static (e1, _) => new Result<TOut, TE1, TE2>(default, e1, default, 1));

    // Combine two results with different errors into a single result with all possible errors
    public Result<TOut, TE1, TE2, TE3> And<TOut, TE2, TE3>(Func<T, Result<TOut, TE2, TE3>> func)
        => Match(
            NotNull(func),
            static (v, f) => f(v).Match(
                static v2 => new Result<TOut, TE1, TE2, TE3>(v2, default, default, default, 0),
                static e2 => new Result<TOut, TE1, TE2, TE3>(default, default, e2, default, 2),
                static e3 => new Result<TOut, TE1, TE2, TE3>(default, default, default, e3, 3)),
            static (e1, _) => new Result<TOut, TE1, TE2, TE3>(default, e1, default, default, 1));

    // If this result fails, take the second result. Use the error type of the second result in
    // either case.
    public Result<T, TError2> Or<TError2>(Func<TE1, Result<T, TError2>> func)
        => Match(static v => new Result<T, TError2>(v, default, 0), func);

    // Map the success result value
    public Result<TOut, TE1> Map<TOut>(Func<T, TOut> map)
        => Match(
            NotNull(map),
            static (v, m) => new Result<TOut, TE1>(m(v), default, 0),
            static (e, _) => new Result<TOut, TE1>(default, e, 1));

    // Map the error result value.
    public Result<T, TErrorOut> MapError<TErrorOut>(Func<TE1, TErrorOut> map)
        => Match(
            NotNull(map),
            static (v, _) => new Result<T, TErrorOut>(v, default, 0),
            static (e, m) => new Result<T, TErrorOut>(default, m(e), 1));

    public Result<T, TE1> OnSuccess(Action<T> onSuccess)
    {
        Switch(onSuccess, static _ => { });
        return this;
    }

    public Result<T, TE1> OnError(Action<TE1> onError)
    {
        Switch(static _ => { }, onError);
        return this;
    }

    public T GetValueOrDefault(T defaultValue)
        => Match(defaultValue, static (t, _) => t, static (_, d) => d);

    public TE1 GetErrorOrDefault(TE1 defaultValue)
        => Match(defaultValue, static (_, d) => d, static (e, _) => e);

    public bool Is(T expected)
        => expected is not null && Match(expected, static (v, e) => v!.Equals(e), static (_, _) => false);

    public bool Is(Func<T, bool> predicate)
        => Match(predicate, static _ => false);

    public bool Equals(T? other)
        => other is not null && Match(
            other,
            static (v, o) => v!.Equals(o),
            static (_, _) => false);
}

public readonly record struct Result<T, TE1, TE2>
{
    private readonly int _index;
    private readonly T? _value;
    private readonly TE1? _e1;
    private readonly TE2? _e2;

    public Result(T? value, TE1? e1, TE2? e2, int index)
    {
        _index = index;
        switch (_index)
        {
            case 0:
                _value = NotNull(value);
                break;
            case 1:
                _e1 = NotNull(e1);
                break;
            case 2:
                _e2 = NotNull(e2);
                break;
            default:
                _index = -1;
                break;
        }
    }

    public bool IsSuccess => _index == 0 && _value is not null;
    public bool IsError1 => _index == 1 && _e1 is not null;
    public bool IsError2 => _index == 2 && _e2 is not null;
    public bool IsValid => IsSuccess || IsError1 || IsError2;

    public static implicit operator Result<T, TE1, TE2>(T value)
        => new Result<T, TE1, TE2>(value, default, default, 0);

    public static implicit operator Result<T, TE1, TE2>(TE1 error)
        => new Result<T, TE1, TE2>(default, error, default, 1);

    public static implicit operator Result<T, TE1, TE2>(TE2 error)
        => new Result<T, TE1, TE2>(default, default, error, 2);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<TE1, TOut> onError1, Func<TE2, TOut> onError2)
    {
        if (_index == 0 && _value is not null)
            return NotNull(onSuccess)(_value!);
        if (_index == 1 && _e1 is not null)
            return NotNull(onError1)(_e1!);
        if (_index == 2 && _e2 is not null)
            return NotNull(onError2)(_e2!);
        return Result.ThrowResultInvalidException<TOut>();
    }

    public TOut Match<TOut, TData>(TData data, Func<T, TData, TOut> onSuccess, Func<TE1, TData, TOut> onError1, Func<TE2, TData, TOut> onError2)
    {
        if (_index == 0 && _value is not null)
            return NotNull(onSuccess)(_value!, data);
        if (_index == 1 && _e1 is not null)
            return NotNull(onError1)(_e1!, data);
        if (_index == 2 && _e2 is not null)
            return NotNull(onError2)(_e2!, data);
        return Result.ThrowResultInvalidException<TOut>();
    }

    public void Switch(Action<T> onSuccess, Action<TE1> onError1, Action<TE2> onError2)
    {
        if (_index == 0 && _value is not null)
        {
            NotNull(onSuccess)(_value!);
            return;
        }
        if (_index == 1 && _e1 is not null)
        {
            NotNull(onError1)(_e1!);
            return;
        }
        if (_index == 2 && _e2 is not null)
        {
            NotNull(onError2)(_e2!);
            return;
        }
        Result.ThrowResultInvalidException<int>();
    }

    public void Switch<TData>(TData data, Action<T, TData> onSuccess, Action<TE1, TData> onError1, Action<TE2, TData> onError2)
    {
        if (_index == 0 && _value is not null)
        {
            NotNull(onSuccess)(_value!, data);
            return;
        }
        if (_index == 1 && _e1 is not null)
        {
            NotNull(onError1)(_e1!, data);
            return;
        }
        if (_index == 2 && _e2 is not null)
        {
            NotNull(onError2)(_e2!, data);
            return;
        }
        Result.ThrowResultInvalidException<int>();
    }

    public Result<TOut, TE1, TE2> Bind<TOut>(Func<T, Result<TOut, TE1, TE2>> func)
        => Match(NotNull(func), static (v, f) => f(v), static (e, _) => e, static (e, _) => e);

    public Result<TOut, TE1, TE2> And<TOut>(Func<T, Result<TOut, TE1, TE2>> func)
        => Match(NotNull(func), static (v, f) => f(v), static (e, _) => e, static (e, _) => e);

    public Result<TOut, TE1, TE2, TE3> And<TOut, TE3>(Func<T, Result<TOut, TE3>> func)
        => Match(
            NotNull(func),
            static (v, f) => f(v).Match(
                static v2 => new Result<TOut, TE1, TE2, TE3>(v2, default, default, default, 0),
                static e3 => new Result<TOut, TE1, TE2, TE3>(default, default, default, e3, 3)),
            static (e1, _) => new Result<TOut, TE1, TE2, TE3>(default, e1, default, default, 1),
            static (e2, _) => new Result<TOut, TE1, TE2, TE3>(default, default, e2, default, 2));

    public Result<TOut, TE1, TE2> Map<TOut>(Func<T, TOut> map)
        => Match(
            NotNull(map),
            static (v, m) => new Result<TOut, TE1, TE2>(m(v), default, default, 0),
            static (e, _) => new Result<TOut, TE1, TE2>(default, e, default, 1),
            static (e, _) => new Result<TOut, TE1, TE2>(default, default, e, 2));

    public Result<T, TErrorOut> MapError<TErrorOut>(Func<TE1, TErrorOut> map1, Func<TE2, TErrorOut> map2)
        => Match(
            (map1, map2),
            static (v, _) => new Result<T, TErrorOut>(v, default, 0),
            static (e, m) => new Result<T, TErrorOut>(default, m.map1(e), 1),
            static (e, m) => new Result<T, TErrorOut>(default, m.map2(e), 1));

    public Result<T, TE1, TE2> OnSuccess(Action<T> onSuccess)
    {
        Switch(onSuccess, static _ => { }, static _ => { });
        return this;
    }

    public Result<T, TE1, TE2> OnError(Action<TE1> onError1, Action<TE2> onError2)
    {
        Switch(static _ => { }, onError1, onError2);
        return this;
    }

    public T GetValueOrDefault(T defaultValue)
        => Match(defaultValue, static (t, _) => t, static (_, d) => d, static (_, d) => d);

    public TE1 GetError1OrDefault(TE1 defaultValue)
        => Match(defaultValue, static (_, d) => d, static (e, _) => e, static (_, d) => d);

    public TE2 GetError2OrDefault(TE2 defaultValue)
        => Match(defaultValue, static (_, d) => d, static (_, d) => d, static (e, _) => e);

    public bool Is(T expected)
        => Match(expected, static (v, e) => v!.Equals(e), static (_, _) => false, static (_, _) => false);

    public bool Is(Func<T, bool> predicate)
        => Match(predicate, static _ => false, static _ => false);
}

public readonly record struct Result<T, TE1, TE2, TE3>
{
    private readonly int _index;
    private readonly T? _value;
    private readonly TE1? _e1;
    private readonly TE2? _e2;
    private readonly TE3? _e3;

    public Result(T? value, TE1? e1, TE2? e2, TE3? e3, int index)
    {
        _index = index;
        switch (_index)
        {
            case 0:
                _value = NotNull(value);
                break;
            case 1:
                _e1 = NotNull(e1);
                break;
            case 2:
                _e2 = NotNull(e2);
                break;
            case 3:
                _e3 = NotNull(e3);
                break;
            default:
                _index = -1;
                break;
        }
    }

    public bool IsSuccess => _index == 0 && _value is not null;
    public bool IsError1 => _index == 1 && _e1 is not null;
    public bool IsError2 => _index == 2 && _e2 is not null;
    public bool IsError3 => _index == 3 && _e3 is not null;
    public bool IsValid => IsSuccess || IsError1 || IsError2 || IsError3;

    public static implicit operator Result<T, TE1, TE2, TE3>(T value)
        => new Result<T, TE1, TE2, TE3>(value, default, default, default, 0);

    public static implicit operator Result<T, TE1, TE2, TE3>(TE1 error)
        => new Result<T, TE1, TE2, TE3>(default, error, default, default, 1);

    public static implicit operator Result<T, TE1, TE2, TE3>(TE2 error)
        => new Result<T, TE1, TE2, TE3>(default, default, error, default, 2);

    public static implicit operator Result<T, TE1, TE2, TE3>(TE3 error)
        => new Result<T, TE1, TE2, TE3>(default, default, default, error, 3);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<TE1, TOut> onError1, Func<TE2, TOut> onError2, Func<TE3, TOut> onError3)
    {
        if (_index == 0 && _value is not null)
            return NotNull(onSuccess)(_value!);
        if (_index == 1 && _e1 is not null)
            return NotNull(onError1)(_e1!);
        if (_index == 2 && _e2 is not null)
            return NotNull(onError2)(_e2!);
        if (_index == 3 && _e3 is not null)
            return NotNull(onError3)(_e3!);
        return Result.ThrowResultInvalidException<TOut>();
    }

    public TOut Match<TOut, TData>(TData data, Func<T, TData, TOut> onSuccess, Func<TE1, TData, TOut> onError1, Func<TE2, TData, TOut> onError2, Func<TE3, TData, TOut> onError3)
    {
        if (_index == 0 && _value is not null)
            return NotNull(onSuccess)(_value!, data);
        if (_index == 1 && _e1 is not null)
            return NotNull(onError1)(_e1!, data);
        if (_index == 2 && _e2 is not null)
            return NotNull(onError2)(_e2!, data);
        if (_index == 3 && _e3 is not null)
            return NotNull(onError3)(_e3!, data);
        return Result.ThrowResultInvalidException<TOut>();
    }

    public void Switch(Action<T> onSuccess, Action<TE1> onError1, Action<TE2> onError2, Action<TE3> onError3)
    {
        if (_index == 0 && _value is not null)
        {
            NotNull(onSuccess)(_value!);
            return;
        }
        if (_index == 1 && _e1 is not null)
        {
            NotNull(onError1)(_e1!);
            return;
        }
        if (_index == 2 && _e2 is not null)
        {
            NotNull(onError2)(_e2!);
            return;
        }
        if (_index == 3 && _e3 is not null)
        {
            NotNull(onError3)(_e3!);
            return;
        }
        Result.ThrowResultInvalidException<int>();
    }

    public void Switch<TData>(TData data, Action<T, TData> onSuccess, Action<TE1, TData> onError1, Action<TE2, TData> onError2, Action<TE3, TData> onError3)
    {
        if (_index == 0 && _value is not null)
        {
            NotNull(onSuccess)(_value!, data);
            return;
        }
        if (_index == 1 && _e1 is not null)
        {
            NotNull(onError1)(_e1!, data);
            return;
        }
        if (_index == 2 && _e2 is not null)
        {
            NotNull(onError2)(_e2!, data);
            return;
        }
        if (_index == 3 && _e3 is not null)
        {
            NotNull(onError3)(_e3!, data);
            return;
        }
        Result.ThrowResultInvalidException<int>();
    }

    public Result<TOut, TE1, TE2, TE3> Bind<TOut>(Func<T, Result<TOut, TE1, TE2, TE3>> func)
        => Match(NotNull(func), static (v, f) => f(v), static (e, _) => e, static (e, _) => e, static (e, _) => e);

    public Result<TOut, TE1, TE2, TE3> And<TOut>(Func<T, Result<TOut, TE1, TE2, TE3>> func)
        => Match(NotNull(func), static (v, f) => f(v), static (e, _) => e, static (e, _) => e, static (e, _) => e);

    public Result<TOut, TE1, TE2, TE3> Map<TOut>(Func<T, TOut> map)
        => Match(
            NotNull(map),
            static (v, m) => new Result<TOut, TE1, TE2, TE3>(m(v), default, default, default, 0),
            static (e, _) => new Result<TOut, TE1, TE2, TE3>(default, e, default, default, 1),
            static (e, _) => new Result<TOut, TE1, TE2, TE3>(default, default, e, default, 2),
            static (e, _) => new Result<TOut, TE1, TE2, TE3>(default, default, default, e, 3));

    public Result<T, TErrorOut> MapError<TErrorOut>(Func<TE1, TErrorOut> map1, Func<TE2, TErrorOut> map2, Func<TE3, TErrorOut> map3)
        => Match(
            (map1, map2, map3),
            static (v, _) => new Result<T, TErrorOut>(v, default, 0),
            static (e, m) => new Result<T, TErrorOut>(default, m.map1(e), 1),
            static (e, m) => new Result<T, TErrorOut>(default, m.map2(e), 1),
            static (e, m) => new Result<T, TErrorOut>(default, m.map3(e), 1));

    public Result<T, TE1, TE2, TE3> OnSuccess(Action<T> onSuccess)
    {
        Switch(onSuccess, static _ => { }, static _ => { }, static _ => { });
        return this;
    }

    public Result<T, TE1, TE2, TE3> OnError(Action<TE1> onError1, Action<TE2> onError2, Action<TE3> onError3)
    {
        Switch(static _ => { }, onError1, onError2, onError3);
        return this;
    }

    public T GetValueOrDefault(T defaultValue)
        => Match(defaultValue, static (t, _) => t, static (_, d) => d, static (_, d) => d, static (_, d) => d);

    public TE1 GetError1OrDefault(TE1 defaultValue)
        => Match(defaultValue, static (_, d) => d, static (e, _) => e, static (_, d) => d, static (_, d) => d);

    public TE2 GetError2OrDefault(TE2 defaultValue)
        => Match(defaultValue, static (_, d) => d, static (_, d) => d, static (e, _) => e, static (_, d) => d);

    public bool Is(T expected)
        => Match(expected, static (v, e) => v!.Equals(e), static (_, _) => false, static (_, _) => false, static (_, _) => false);

    public bool Is(Func<T, bool> predicate)
        => Match(predicate, static _ => false, static _ => false, static _ => false);
}
