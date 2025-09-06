using System.Runtime.CompilerServices;
using static BaseFunctional.Assert;

namespace BaseFunctional;

public interface ICanBeValid
{
    bool IsValid { get; }
}

public sealed class InvalidObject : ICanBeValid
{
    public bool IsValid => false;
}

public sealed class ValidObject : ICanBeValid
{
    public bool IsValid => true;
}

public static class CanBeValidExtensions
{
    public static TOut Match<T, TOut>(this T canBeValid, Func<T, TOut> onValid, Func<TOut> onNotValid)
        where T : ICanBeValid
        => canBeValid?.IsValid == true
            ? NotNull(onValid)(canBeValid!)
            : NotNull(onNotValid)();

    public static void Switch<T>(this T canBeValid, Action<T> onValid, Action onNotValid)
        where T : ICanBeValid
    {
        if (canBeValid?.IsValid == true)
        {
            NotNull(onValid)(canBeValid);
            return;
        }

        NotNull(onNotValid)();
    }

    public static void OnValid<T>(this T canBeValid, Action<T> onValid)
        where T : ICanBeValid
        => canBeValid.Switch(
            NotNull(onValid),
            static () => { });

    public static void OnNotValid<T>(this T canBeValid, Action onNotValid)
        where T : ICanBeValid
        => canBeValid.Switch(
            static _ => { },
            NotNull(onNotValid));

    public static Maybe<T> ToMaybe<T>(this T canBeValid)
        where T : ICanBeValid
        => canBeValid.Match(
            static v => new Maybe<T>(v, true),
            static () => default);

    public static Result<T, ValidationFailure> ToResult<T>(this T canBeValid, [CallerArgumentExpression(nameof(canBeValid))] string? name = null)
        where T : ICanBeValid
        => canBeValid?.IsValid == true
            ? canBeValid
            : ValidationFailure.FromSingleError(name ?? "value", "Value is null or invalid");
}

