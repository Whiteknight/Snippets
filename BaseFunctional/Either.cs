using System.Diagnostics.CodeAnalysis;
using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class Either
{
    public static Either<TLeft, TRight> FromLeft<TLeft, TRight>(TLeft value)
        => new Either<TLeft, TRight>(value, default, 0);

    public static Either<TLeft, TRight> FromRight<TLeft, TRight>(TRight value)
        => new Either<TLeft, TRight>(default, value, 1);

    [DoesNotReturn]
    public static T ThrowEitherInvalidException<T>()
        => throw new InvalidOperationException("Either is in an invalid state");

    public static Either<T2, T1> Invert<T1, T2>(this Either<T1, T2> Either)
        => Either.Match(
            t1 => new Either<T2, T1>(default, t1, 1),
            t2 => new Either<T2, T1>(t2, default, 0));
}

// Either is one or the other, never both and never neither
public readonly record struct Either<TLeft, TRight>
{
    private readonly int _index;
    private readonly TLeft? _left;
    private readonly TRight? _right;

    public Either(TLeft? left, TRight? right, int index)
    {
        _index = index;
        if (index == 0)
            _left = NotNull(left);
        else if (index == 1)
            _right = NotNull(right);
    }

    public bool IsLeft => _index == 0 && _left is not null;
    public bool IsRight => _index == 1 && _right is not null;
    public bool IsValid => IsLeft || IsRight;

    public static implicit operator Either<TLeft, TRight>(TLeft value)
        => new Either<TLeft, TRight>(value, default, 0);

    public static implicit operator Either<TLeft, TRight>(TRight error)
        => new Either<TLeft, TRight>(default, error, 1);

    public TOut Match<TOut>(Func<TLeft, TOut> onLeft, Func<TRight, TOut> onRight)
    {
        if (_index == 0 && _left is not null)
            return NotNull(onLeft)(_left!);
        if (_index == 1 && _right is not null)
            return NotNull(onRight)(_right!);
        return Either.ThrowEitherInvalidException<TOut>();
    }

    public TOut Match<TOut, TData>(TData data, Func<TLeft, TData, TOut> onLeft, Func<TRight, TData, TOut> onRight)
    {
        if (_index == 0 && _left is not null)
            return NotNull(onLeft)(_left!, data);
        if (_index == 1 && _right is not null)
            return NotNull(onRight)(_right!, data);
        return Either.ThrowEitherInvalidException<TOut>();
    }

    public void Switch(Action<TLeft> onLeft, Action<TRight> onRight)
    {
        if (_index == 0 && _left is not null)
        {
            NotNull(onLeft)(_left!);
            return;
        }
        if (_index == 1 && _right is not null)
        {
            NotNull(onRight)(_right!);
            return;
        }
        Either.ThrowEitherInvalidException<int>();
    }

    public void Switch<TData>(TData data, Action<TLeft, TData> onLeft, Action<TRight, TData> onRight)
    {
        if (_index == 0 && _left is not null)
        {
            NotNull(onLeft)(_left!, data);
            return;
        }
        if (_index == 1 && _right is not null)
        {
            NotNull(onRight)(_right!, data);
            return;
        }
        Either.ThrowEitherInvalidException<int>();
    }

    // Map the left Either value
    public Either<TOut, TRight> MapLeft<TOut>(Func<TLeft, TOut> map)
        => Match(
            NotNull(map),
            static (v, m) => new Either<TOut, TRight>(m(v), default, 0),
            static (e, _) => new Either<TOut, TRight>(default, e, 1));

    // Map the right Either value.
    public Either<TLeft, TOut> MapRight<TOut>(Func<TRight, TOut> map)
        => Match(
            NotNull(map),
            static (v, _) => new Either<TLeft, TOut>(v, default, 0),
            static (e, m) => new Either<TLeft, TOut>(default, m(e), 1));

    public Either<TLeft, TRight> OnLeft(Action<TLeft> onLeft)
    {
        Switch(onLeft, static _ => { });
        return this;
    }

    public Either<TLeft, TRight> OnRight(Action<TRight> onRight)
    {
        Switch(static _ => { }, onRight);
        return this;
    }

    public TLeft GetLeftOrDefault(TLeft defaultValue)
        => Match(defaultValue, static (t, _) => t, static (_, d) => d);

    public TRight GetRightOrDefault(TRight defaultValue)
        => Match(defaultValue, static (_, d) => d, static (e, _) => e);

    public bool LeftIs(TLeft expected)
        => expected is not null && Match(expected, static (v, e) => v!.Equals(e), static (_, _) => false);

    public bool LeftIs(Func<TLeft, bool> predicate)
        => Match(predicate, static _ => false);

    public bool RightIs(TRight expected)
        => expected is not null && Match(expected, static (_, _) => false, static (v, e) => v!.Equals(e));

    public bool RightIs(Func<TRight, bool> predicate)
        => Match(static _ => false, predicate);
}
