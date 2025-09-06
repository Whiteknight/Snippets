using static BaseFunctional.Assert;

namespace DataTypeRelated;

public static class ExceptionGetInnermostExceptionExtensions
{
    public static Exception GetInnermostException(this Exception exception)
    {
        var ex = NotNull(exception);
        while (ex.InnerException is not null)
            ex = ex.InnerException;
        return ex;
    }
}
