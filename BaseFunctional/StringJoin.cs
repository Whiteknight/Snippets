namespace BaseFunctional;

public static class StringJoinExtensions
{
    // Extension method form of String.Join().
    public static string StringJoin(this IEnumerable<string> parts, char c)
        => string.Join(c, parts.OrEmptyIfNull());

    public static string StringJoin(this IEnumerable<string> parts, string separator)
        => string.Join(separator ?? string.Empty, parts.OrEmptyIfNull());
}
