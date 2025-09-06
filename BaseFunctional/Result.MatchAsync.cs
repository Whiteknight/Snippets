using static BaseFunctional.Assert;

namespace BaseFunctional;

public static class ResultMatchAsync
{
    // Result, async callbacks, no data
    public static async Task<TOut> MatchAsync<T, TError, TOut>(
        this Result<T, TError> result,
        Func<T, CancellationToken, Task<TOut>> onSuccess,
        Func<TError, CancellationToken, Task<TOut>> onError,
        CancellationToken cancellation)
        => await result.Match(
            (onSuccess, onError, cancellation),
            static (v, d) => NotNull(d.onSuccess)(v, d.cancellation),
            static (e, d) => NotNull(d.onError)(e, d.cancellation));

    // Result, async callbacks, data
    public static async Task<TOut> MatchAsync<T, TError, TData, TOut>(
       this Result<T, TError> result,
       TData data,
       Func<T, TData, CancellationToken, Task<TOut>> onSuccess,
       Func<TError, TData, CancellationToken, Task<TOut>> onError,
       CancellationToken cancellation)
       => await result.Match(
           (data, onSuccess, onError, cancellation),
           static (v, d) => NotNull(d.onSuccess)(v, d.data, d.cancellation),
           static (e, d) => NotNull(d.onError)(e, d.data, d.cancellation));

    // Result task, sync callbacks, no data
    public static async Task<TOut> MatchAsync<T, TError, TOut>(
        this Task<Result<T, TError>> result,
        Func<T, TOut> onSuccess,
        Func<TError, TOut> onError)
        => (await result).Match(onSuccess, onError);

    // Result task, sync callbacks, data
    public static async Task<TOut> MatchAsync<T, TError, TData, TOut>(
        this Task<Result<T, TError>> result,
        TData data,
        Func<T, TData, TOut> onSuccess,
        Func<TError, TData, TOut> onError)
        => (await result).Match(data, onSuccess, onError);

    // Result task, async callbacks, no data
    public static async Task<TOut> MatchAsync<T, TError, TOut>(
        this Task<Result<T, TError>> result,
        Func<T, CancellationToken, Task<TOut>> onSuccess,
        Func<TError, CancellationToken, Task<TOut>> onError,
        CancellationToken cancellation)
        => await (await result).MatchAsync(onSuccess, onError, cancellation);

    // Result task, async callbacks, data
    public static async Task<TOut> MatchAsync<T, TError, TData, TOut>(
        this Task<Result<T, TError>> result,
        TData data,
        Func<T, TData, CancellationToken, Task<TOut>> onSuccess,
        Func<TError, TData, CancellationToken, Task<TOut>> onError,
        CancellationToken cancellation)
        => await (await result).MatchAsync(data, onSuccess, onError, cancellation);
}
