using System.Runtime.CompilerServices;
using static BaseFunctional.Assert;

namespace BaseFunctional;

public sealed class NonNullString(string s)
{
    public string Value { get; } = NotNull(s);
}

public sealed class NonEmptyString(string s)
{
    public string Value { get; } = NotNullOrEmpty(s);
}

public readonly struct NonNegativeInteger
{
    public NonNegativeInteger(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Integer value must be greater than or equal to zero.");
        Value = value;
    }

    public int Value { get; }
}

public static class Verify
{
    public static Result<NonNullString, VerifyError> NotNull(string? value, [CallerArgumentExpression(nameof(value))] string? name = null)
        => value is null
            ? new ValueIsNull(name ?? nameof(value))
            : new NonNullString(value);

    public static Result<NonEmptyString, VerifyError> NotNullOrEmpty(string? value, [CallerArgumentExpression(nameof(value))] string? name = null)
        => value switch
        {
            null => new ValueIsNull(name ?? nameof(value)),
            [] => new ValueIsEmpty(name ?? nameof(value)),
            string val => new NonEmptyString(val)
        };

    public static Result<NonNegativeInteger, VerifyError> NotNegative(int value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        return value < 0
            ? new ValueIsOutOfBounds(name ?? nameof(value), "Value cannot be negative")
            : new NonNegativeInteger(value);
    }
}

public abstract record VerifyError(string Message);

public sealed record ValueIsNull(string Name) : VerifyError($"Value {Name} is null");

public sealed record ValueIsEmpty(string Name) : VerifyError("Value {Name} is null or empty");

public sealed record ValueIsOutOfBounds(string Name, string Description) : VerifyError($"Value {Name} is out of bounds: {Description}");
