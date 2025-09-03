using System.Runtime.CompilerServices;
using static BaseFunctional.Assert;

namespace BaseFunctional;

public readonly record struct ValidationEntry(string Name, string Description, Maybe<object> Data = default);

public readonly record struct ValidationFailure(ValidationEntry[] Entries)
{
    public static ValidationFailure FromSingleError(string name, string description)
        => new ValidationFailure([new ValidationEntry(name, description)]);
}

public interface IValidator<T>
{
    IEnumerable<ValidationEntry> GetValidationEntries(T value);
}

public static class Validation
{
    public static Result<T, ValidationFailure> Validate<T>(this IValidator<T> validator, T value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value is null)
            return ValidationFailure.FromSingleError(name ?? nameof(value), "Value cannot be null");
        var entries = NotNull(validator).GetValidationEntries(value)
            .OrEmptyIfNull()
            .ToArray();
        return entries switch
        {
            [] => value,
            [..] => new ValidationFailure(entries)
        };
    }

    public static IValidator<T> And<T>(this IValidator<T> first, params IValidator<T>[] rest)
    {
        var list = new[] { first }
            .Concat(rest)
            .SelectMany(validator => validator switch
            {
                NullValidator<T> => [],
                CombinedValidator<T> cv => cv.Validators,
                IValidator<T> v => [v],
                _ => []
            })
            .ToArray();
        return Combine(list);
    }

    public static IValidator<T> Combine<T>(params IValidator<T>[] list)
    {
        return list switch
        {
            [] => new NullValidator<T>(),
            [IValidator<T> v] => v,
            [..] l => new CombinedValidator<T>(l)
        };
    }

    public static IValidator<T> Null<T>() => new NullValidator<T>();
}

public sealed class CombinedValidator<T> : IValidator<T>
{
    private readonly IValidator<T>[] _validators;

    public CombinedValidator(IEnumerable<IValidator<T>> validators)
    {
        _validators = validators.OrEmptyIfNull().ToArray();
    }

    public IEnumerable<IValidator<T>> Validators => _validators;

    public IEnumerable<ValidationEntry> GetValidationEntries(T value)
        => _validators.SelectMany(v => v.GetValidationEntries(value).OrEmptyIfNull());
}

public sealed class NullValidator<T> : IValidator<T>
{
    public IEnumerable<ValidationEntry> GetValidationEntries(T value)
        => [];
}

public sealed class FailValidator<T> : IValidator<T>
{
    private const string _defaultMessage = "Validation Failure";
    private readonly ValidationEntry[] _entry;

    public FailValidator(string? name, string? description)
    {
        _entry = [new ValidationEntry(name ?? string.Empty, description ?? _defaultMessage)];
    }

    public IEnumerable<ValidationEntry> GetValidationEntries(T value)
        => _entry;
}

public sealed class DelegateValidator<T> : IValidator<T>
{
    private readonly Func<T, IEnumerable<ValidationEntry>> _validate;

    public DelegateValidator(Func<T, IEnumerable<ValidationEntry>> validate)
    {
        _validate = NotNull(validate);
    }

    public IEnumerable<ValidationEntry> GetValidationEntries(T value)
        => _validate(value).OrEmptyIfNull();
}
