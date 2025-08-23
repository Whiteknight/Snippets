using System.Diagnostics.CodeAnalysis;

namespace BaseFunctional;

public static class OrEmptyIfNullExtensions
{
    // coalesce a null enumerable, such as from an untrusted external provider, to an empty
    // enumerable.
    [return: NotNull]
    public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T>? source)
        => source ?? [];
}
