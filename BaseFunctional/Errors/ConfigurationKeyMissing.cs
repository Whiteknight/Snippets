namespace BaseFunctional.Errors;

public sealed record ConfigurationKeyMissing(string Name, string Description)
    : Error($"Missing configuration value '{Name}'. Expected {Description}")
{
    public static ConfigurationKeyMissing Url(string name)
        => new ConfigurationKeyMissing(name, "URL");

    public static ConfigurationKeyMissing DbConnectionString(string name)
        => new ConfigurationKeyMissing(name, "database connection string");

    public static ConfigurationKeyMissing BooleanFlag(string name)
        => new ConfigurationKeyMissing(name, "boolean flag");

    public static ConfigurationKeyMissing Integer(string name)
        => new ConfigurationKeyMissing(name, "integer");
}

// When configuration is missing, it is often a show-stopper for application startup. In these
// cases we may want to raise an exception and bring the whole application down.
public sealed class ConfigurationKeyMissingException : Exception
{
    public ConfigurationKeyMissingException(string name, string description)
        : base($"Missing configuration value '{name}'. Expected {description}")
    {
    }

    public ConfigurationKeyMissingException(string name, string description, Exception inner)
        : base($"Missing configuration value '{name}'. Expected {description}", inner)
    {
    }

    public static ConfigurationKeyMissingException Url(string name)
        => new ConfigurationKeyMissingException(name, "URL");

    public static ConfigurationKeyMissingException DbConnectionString(string name)
        => new ConfigurationKeyMissingException(name, "database connection string");

    public static ConfigurationKeyMissingException BooleanFlag(string name)
        => new ConfigurationKeyMissingException(name, "boolean flag");

    public static ConfigurationKeyMissing Integer(string name)
        => new ConfigurationKeyMissing(name, "integer");
}

// Similar to a ConfigurationKeyMissing error, but strictly for Environment Variables.
public sealed record EnvironmentVariableMissing(string Name, string Description)
    : Error($"Missing environment variable '{Name}'. Expected {Description}")
{
    public static EnvironmentVariableMissing Url(string name)
        => new EnvironmentVariableMissing(name, "URL");

    public static EnvironmentVariableMissing DbConnectionString(string name)
        => new EnvironmentVariableMissing(name, "database connection string");

    public static EnvironmentVariableMissing BooleanFlag(string name)
        => new EnvironmentVariableMissing(name, "boolean flag");

    public static EnvironmentVariableMissing Integer(string name)
        => new EnvironmentVariableMissing(name, "integer");
}

// Similar to ConfigurationKeyMissingException, but strictly for environment variables.
public sealed class EnvironmentVariableMissingException : Exception
{
    public EnvironmentVariableMissingException(string name, string description)
        : base($"Missing configuration value '{name}'. Expected {description}")
    {
    }

    public EnvironmentVariableMissingException(string name, string description, Exception inner)
        : base($"Missing configuration value '{name}'. Expected {description}", inner)
    {
    }

    public static EnvironmentVariableMissingException Url(string name)
        => new EnvironmentVariableMissingException(name, "URL");

    public static EnvironmentVariableMissingException DbConnectionString(string name)
        => new EnvironmentVariableMissingException(name, "database connection string");

    public static EnvironmentVariableMissingException BooleanFlag(string name)
        => new EnvironmentVariableMissingException(name, "boolean flag");

    public static EnvironmentVariableMissingException Integer(string name)
        => new EnvironmentVariableMissingException(name, "integer");
}
