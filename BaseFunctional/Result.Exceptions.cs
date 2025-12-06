using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class ResultExceptionExtensions
{
    public static Result<T, Error> MapExceptionToError<T>(this Result<T, Exception> result)
        => result.MapError(ex => (Error)new UnknownException(ex));

    public static Result<T, Error> MapExceptionToError<T>(this Result<T, Exception> result, IMap<Exception>.To<Error> mapper)
        => result.MapError(ex => NotNull(mapper).Map(ex));
}
