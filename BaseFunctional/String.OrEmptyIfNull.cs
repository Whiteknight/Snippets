namespace BaseFunctional;

public static class StringOrEmptyIfNullExtensions
{
    public static string OrEmptyIfNull(this string? value)
        => value ?? string.Empty;
}
