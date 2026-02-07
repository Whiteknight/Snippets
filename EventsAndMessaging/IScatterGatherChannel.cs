using BaseFunctional;

namespace EventsAndMessaging;

// Depends on implementation.
// Definitely going to want a timeout and a minimum number of responses to wait for.
public readonly record struct ScatterOptions();

public interface IScatterGatherChannel
{
    public Task<Result<IEnumerable<TResponse>, Error>> ScatterGather<TRequest, TResponse>(TRequest message, ScatterOptions options);

    public IDisposable Listen<TRequest, TResponse>(IListener<TRequest, TResponse> consumer);
}
