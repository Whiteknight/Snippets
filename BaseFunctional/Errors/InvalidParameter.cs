namespace BaseFunctional.Errors;

// Values provided by a user are either incomplete or invalid.
// This likely turns into an HTTP 400 Bad Request
public sealed record InvalidParameter(string[] Missing, string[] Invalid)
    : Error(FormatMessage(Missing, Invalid))
{
    private static string FormatMessage(string[] missing, string[] invalid)
    {
        if ((missing == null || missing.Length == 0) && (invalid == null || invalid.Length == 0))
            return "Parameters are invalid";
        var items = missing.OrEmptyIfNull().Select(m => $"Missing: {m}")
            .Concat(invalid.OrEmptyIfNull().Select(i => $"Invalid: {i}"));
        return $"Problems with provided parameters:\n{string.Join("\n", items)}";
    }
}
