namespace BaseFunctional;

public sealed record TaskCancelled() : Error("Task was cancelled.");

public static class ResultCancellationExtensions
{
    public static Result<T, Error> CheckCancellation<T>(this Result<T, Error> result, CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? new TaskCancelled()
            : result;
}
