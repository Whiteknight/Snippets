using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class ResultAsyncExtensions
{
    public static async Task<TOut> MatchAsync<T, TError, TOut>(
        this Result<T, TError> result,
        Func<T, CancellationToken, Task<TOut>> onSuccess,
        Func<TError, CancellationToken, Task<TOut>> onError,
        CancellationToken cancellation)
    {
        if (result.IsSuccess)
            return await NotNull(onSuccess)(result.GetValueOrDefault(default!), cancellation);
        if (result.IsError)
            return await NotNull(onError)(result.GetErrorOrDefault(default!), cancellation);
        return Result.ThrowResultInvalidException<TOut>();
    }
}
