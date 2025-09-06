using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class ResultMatchAsync
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

    public static async Task<TOut> MatchAsync<T, TError, TOut>(
        this Task<Result<T, TError>> result,
        Func<T, TOut> onSuccess,
        Func<TError, TOut> onError)
        => (await result).Match(onSuccess, onError);

    public static async Task<TOut> MatchAsync<T, TError, TOut>(
        this Task<Result<T, TError>> result,
        Func<T, CancellationToken, Task<TOut>> onSuccess,
        Func<TError, CancellationToken, Task<TOut>> onError,
        CancellationToken cancellation)
        => await (await result).MatchAsync(onSuccess, onError, cancellation);
}

public static class ResultBindAsync
{
    public static async Task<Result<TOut, TError>> BindAsync<T, TError, TOut>(
        this Result<T, TError> result,
        Func<T, CancellationToken, Task<Result<TOut, TError>>> onSuccess,
        CancellationToken cancellation)
    {
        if (result.IsSuccess)
            return await NotNull(onSuccess)(result.GetValueOrDefault(default!), cancellation);
        if (result.IsError)
            return Result.Error<TOut, TError>(result.GetErrorOrDefault(default!));
        return Result.ThrowResultInvalidException<TOut>();
    }

    public static async Task<Result<TOut, TError>> BindAsync<T, TError, TOut>(
        this Task<Result<T, TError>> result,
        Func<T, Result<TOut, TError>> onSuccess)
        => (await result).Bind(onSuccess);

    public static async Task<Result<TOut, TError>> BindAsync<T, TError, TOut>(
        this Task<Result<T, TError>> result,
        Func<T, CancellationToken, Task<Result<TOut, TError>>> onSuccess,
        CancellationToken cancellation)
        => await (await result).BindAsync(onSuccess, cancellation);
}