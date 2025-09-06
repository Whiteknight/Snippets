namespace BaseFunctional;

#pragma warning disable RCS1194 // Implement exception constructors

// Configuration and env vars being missing is critical and may stop an application
// from starting or executing normally. For this reason, we communicate it as an
// Exception instead of as a structured Error type.
public sealed class ConfigurationKeyMissingException : Exception

{
    private ConfigurationKeyMissingException(string message)
        : base(message)
    {
    }

    public static ConfigurationKeyMissingException ConnectionString(string name)
        => new ConfigurationKeyMissingException($"Missing configuration value '{name}' which should be a database connection string");
}
