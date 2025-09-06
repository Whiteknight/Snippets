using BaseFunctional;

namespace DataRelated;

public abstract record EnvironmentVariableError(string Name, string Message) : Error($"Environment Variable '{Name}' Error: {Message}");

public sealed record MissingEnvironmentVariableError(string Name) : EnvironmentVariableError(Name, "Is missing");

public sealed record EmptyEnvironmentVariableError(string Name) : EnvironmentVariableError(Name, "Is missing");

public static class EnvironmentVariables
{
    public static Result<string, Error> Get(string name)
        => Environment.GetEnvironmentVariable(name) switch
        {
            "" => new EmptyEnvironmentVariableError(name),
            string value => Result.New<string, Error>(value!),
            _ => new MissingEnvironmentVariableError(name)
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
