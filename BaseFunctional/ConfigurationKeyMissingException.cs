namespace BaseFunctional;

public sealed class ConfigurationKeyMissingException : Exception
{
    private ConfigurationKeyMissingException(string message)
        : base(message)
    {
    }

    public static ConfigurationKeyMissingException ConnectionString(string name)
        => new ConfigurationKeyMissingException($"Missing configuration value '{name}' which should be a database connection string");
}
