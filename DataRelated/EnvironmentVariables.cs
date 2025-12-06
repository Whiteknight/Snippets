using BaseFunctional;

namespace DataRelated;

public abstract record EnvironmentVariableError(string Name, string Message) : Error($"Environment Variable '{Name}' Error: {Message}");

public sealed record MissingEnvironmentVariable(string Name) : EnvironmentVariableError(Name, "Is missing");

public sealed record UnexpectedEmptyEnvironmentVariable(string Name) : EnvironmentVariableError(Name, "Is missing");

public static class EnvironmentVariables
{
    public static Result<string, Error> Get(string name)
        => Environment.GetEnvironmentVariable(name) switch
        {
            "" => new UnexpectedEmptyEnvironmentVariable(name),
            string value => Result.FromValue<string, Error>(value!),
            _ => new MissingEnvironmentVariable(name)
        };

    public static Maybe<string> MaybeGet(string name)
        => Environment.GetEnvironmentVariable(name) switch
        {
            "" => default,
            string value => new Maybe<string>(value!, true),
            _ => default
        };

    public static string GetOrThrow(string name)
        => MaybeGet(name).Match(name,
            (s, _) => s,
            n => throw ConfigurationKeyMissingException.EnvironmentVariable(n));
}
