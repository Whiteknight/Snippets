using System.Text;
using static BaseFunctional.Assert;

namespace BaseFunctional;

public class DatabaseConnectionString
{
    public DatabaseConnectionString(string connectionString, string? name = null)
    {
        ConnectionString = NotNullOrEmpty(connectionString).Trim();
        Name = name ?? string.Empty;
    }

    public static DatabaseConnectionString Empty { get; } = new DatabaseConnectionString(string.Empty, "empty");

    public string ConnectionString { get; }

    public string Name { get; }

    public static DatabaseConnectionString Create((string Name, string Value)[] parts, char keySeparator = ';', char valueSeparator = '=')
    {
        if (parts is null || parts is [])
            return Empty;

        var dedupes = Deduplicate(parts);
        var str = Stringify(dedupes, keySeparator, valueSeparator);
        return new DatabaseConnectionString(str);
    }

    public static DatabaseConnectionString Create(params (string Name, string Value)[] parts)
        => Create(parts, ';', '=');

    public static DatabaseConnectionString CreateFromEnvironmentVariable(string varName, char keySeparator = ';', char valueSeparator = '=')
    {
        // We throw an exception here because this is typically called from startup/bootstrapper
        // and database connections are pretty fundamental so it's not like we can handle this
        // gracefully and continue.
        var envVar = Environment.GetEnvironmentVariable(varName);
        return string.IsNullOrEmpty(envVar)
            ? throw ConfigurationKeyMissingException.ConnectionString(varName)
            : new DatabaseConnectionString(envVar, varName);
    }

    public static (string Name, string Value)[] Parse(string? cs, char keySeparator = ';', char valueSeparator = '=')
    {
        if (string.IsNullOrEmpty(cs))
            return [];
        return cs.Split(keySeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split([valueSeparator], 2))
            .Select(p => (p[0], p[1]))
            .ToArray();
    }

    public DatabaseConnectionString With((string Name, string Value)[] parts, char keySeparator, char valueSeparator)
    {
        var first = Parse(ConnectionString);
        var dedupes = Deduplicate(first.Concat(parts));
        var str = Stringify(dedupes, keySeparator, valueSeparator);
        return new DatabaseConnectionString(str);
    }

    public DatabaseConnectionString With(params (string Name, string Value)[] parts)
        => With(parts, ';', '=');

    public DatabaseConnectionString Named(string? name)
        => new DatabaseConnectionString(ConnectionString, name);

    private static string Stringify((string Name, string Value)[] parts, char keySeparator, char valueSeparator)
    {
        if (parts.Length == 0)
            return string.Empty;
        if (parts.Length == 1)
            return $"{parts[0].Name}{valueSeparator}{parts[0].Value}";
        var sb = new StringBuilder();
        sb.Append($"{parts[0].Name}{valueSeparator}{parts[0].Value}");
        for (int i = 1; i < parts.Length; i++)
            sb.Append($"{keySeparator}{parts[i].Name}{valueSeparator}{parts[i].Value}");
        return sb.ToString();
    }

    private static (string Name, string Value)[] Deduplicate(IEnumerable<(string Name, string Value)> parts)
    {
        var registry = new Dictionary<string, string>();
        foreach (var part in parts)
            registry.TryAdd(part.Name, part.Value);
        return registry.Select(kvp => (kvp.Key, kvp.Value)).ToArray();
    }
}
