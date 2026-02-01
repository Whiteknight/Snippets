using BaseFunctional;
using DataTypeRelated;

namespace DataRelated;

// A representation of values used to look up a value in a data store
// This could be a single primary key (id) or it could be some compound key or even a set of
// search criteria.
// We keep this so we can log "We tried to find an object with values X,Y,Z but not found"
public readonly record struct EntityKey((string Name, string Value)[] Key) : ICanBeValid
{
    public static implicit operator EntityKey((string Name, string Value)[] items)
        => new EntityKey(items);

    public static implicit operator EntityKey((string Name, string Value) item)
        => new EntityKey([item]);

    public bool IsValid => Key.Length > 0;

    public static EntityKey Id(string id) => new EntityKey([("Id", id)]);

    public static EntityKey Id(int id) => Id(id.ToString());

    public static EntityKey Id(Guid id) => Id(id.ToString("D"));

    public static EntityKey Name(string name) => new EntityKey([("Name", name)]);

    public static EntityKey Code(string code) => new EntityKey([("Code", code)]);

    public override string ToString()
        => Key switch
        {
            null => string.Empty,
            [] => string.Empty,
            [..] => Key.Select(i => $"{i.Name}={i.Value}").StringJoin(" ")
        };
}

public abstract record DataError(string Message) : Error(Message);

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

// We're trying to insert something with a conflicted unique field. For instance a non-primarykey
// unique column like a "code" or "name" may have a duplicate value which causes a conflict
// and returns 409 Conflict.
public sealed record EntityConflict(string Type, EntityKey Key)
    : DataError($"Could not insert/update entity of type {Type} because of conflicted values {Key}");

// We tried to load a single entity by id or some other compound key, but it could not be found/loaded
// Notice that EntityNotFound is not an "exceptional error" per se. This is a normal response to
// an unfortunately common occurance of users looking for the wrong thing. We want to tell the user,
// gently, that the item isn't found but we don't want to crash or be dramatic
public sealed record EntityNotFound(string Type, EntityKey Key)
    : DataError($"Could not find entity of Type {Type} with {Key}");

public abstract record SecurityError(string Message) : Error(Message);

// Access is forbidden (usually HTTP 403). The user IS logged in but does not have permission to
// view the requested resource.
public sealed record AccessForbidden(string Type, EntityKey Key)
    : SecurityError($"Access denied for entity Type={Type} {Key}")
{
    public static AccessForbidden New<T>(int id)
        => new AccessForbidden(typeof(T).Name, EntityKey.Id(id));

    public static AccessForbidden New<T>(Guid id)
        => new AccessForbidden(typeof(T).Name, EntityKey.Id(id));
}

// The user is not authenticated/authorized but is attempting to access restricted resources (HTTP 401)
public sealed record UnauthorizedAccessAttempt(string Message)
    : SecurityError(Message)
{
    public static UnauthorizedAccessAttempt NotLoggedIn()
        => new UnauthorizedAccessAttempt("You are not logged in and cannot access resources in this system");
}
