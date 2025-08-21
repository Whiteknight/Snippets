namespace BaseFunctional;

public static class StringJoinExtensions
{
    public static string StringJoin(this IEnumerable<string> parts, char c)
        => string.Join(c, parts.OrEmptyIfNull());

    public static string StringJoin(this IEnumerable<string> parts, string sep)
        => string.Join(sep ?? string.Empty, parts.OrEmptyIfNull());
}
