using System.Diagnostics;

namespace BaseFunctional;

public abstract record Error(string Message, string? StackTrace = null)
{
    public static string GetCurrentStackTrace() => new StackTrace(1, true).ToString();
}

public sealed record AggregateError(IReadOnlyList<Error> Errors) : Error(BuildMessage(Errors))
{
    private static string BuildMessage(IReadOnlyList<Error> errors)
    {
        if (errors.Count == 0)
            return "Error not specified";
        if (errors.Count == 1)
            return errors[0].Message;
        return $"{errors.Count} errors: {string.Join("; ", errors.Select(e => e.Message))}";
    }
}

public sealed record UnknownException(Exception Exception) : Error($"Unknown exception: {Exception.GetType().Name} {Exception.Message}", Exception.StackTrace);

public sealed record UnknownError(string Message) : Error($"Unknown error: {Message}");
