using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BaseFunctional;

public static class Assert
{
    [return: NotNull]
    public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value is null)
            throw new ArgumentNullException(name);
        return value!;
    }

    [return: NotNull]
    public static string NotNullOrEmpty([NotNull] string value, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        if (value is null)
            throw new ArgumentNullException(name);
        if (value.Length == 0)
            throw new ArgumentException("String argument cannot be empty", name);
        return value!;
    }
}
