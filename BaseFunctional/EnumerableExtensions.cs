using System.Diagnostics.CodeAnalysis;

namespace BaseFunctional;

public static class EnumerableExtensions
{
    [return: NotNull]
    public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T>? source)
        => source ?? [];
}
