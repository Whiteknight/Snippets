namespace BaseFunctional;

// A representation of values used to look up a value in a data store
// This could be a single primary key (id) or it could be some compound key or even a set of
// search criteria.
// We keep this so we can log "We tried to find an object with values X,Y,Z but not found"
public readonly record struct EntityLookup((string Name, string Value)[] Items)
{
    public static implicit operator EntityLookup((string Name, string Value)[] items) => new EntityLookup(items);

    public static EntityLookup Id(string id) => new EntityLookup([("Id", id)]);

    public static EntityLookup Id(int id) => Id(id.ToString());

    public static EntityLookup Id(Guid id) => Id(id.ToString("D"));

    public static EntityLookup Name(string name) => new EntityLookup([("Name", name)]);

    public static EntityLookup Code(string code) => new EntityLookup([("Code", code)]);

    public override string ToString()
    {
        return Items switch
        {
            [] => string.Empty,
            [..] => Items.Select(i => $"{i.Name}={i.Value}").StringJoin(", ")
        };
    }
}

public abstract record DataError(string Message);

// For connectivity problems in connecting to the database: IOException, NetworkException, etc
// Try to include enough information in subclasses, including the actual exception object, to diagnose
// Notice: TimeoutException could be related to connectivity or query performance.
public abstract record ConnectivityError(string Message, Exception Exception)
    : DataError($"Connectivity problem: {Message}, {Exception.GetType().Name} {Exception.Message} at {Exception.StackTrace}");

// For problems with the query itself, such as a syntax error, index error, constraint error, etc
// The query could not be executed for reasons of syntax or semantics
// Try to include enough information in the Error subclass to diagnose and understand the issue.
public abstract record QueryError(string Message, string Query)
    : DataError($"Query Problem: {Message}. Query: {Query}");

// We tried to load a single entity by id or some other compound key, but it could not be found/loaded
// Notice that EntityNotFound is not an "exceptional error" per se. This is a normal response to
// an unfortunately common occurance of users looking for the wrong thing. We want to tell the user,
// gently, that the item isn't found but we don't want to crash or be dramatic
public record EntityNotFound(string Type, EntityLookup Key)
    : DataError($"Could not find entity of Type {Type} with {Key}");
