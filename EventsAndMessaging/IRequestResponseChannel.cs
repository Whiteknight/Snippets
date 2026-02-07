using BaseFunctional;

namespace EventsAndMessaging;

// Options are dependent on the internal implementation.
public readonly record struct RequestOptions();

public interface IRequestResponseChannel<TRequest, TResponse>
{
    Task<Result<TResponse, Error>> SendRequest(TRequest message, RequestOptions options);

    IDisposable Listen(IListener<TRequest, TResponse> consumer);
}
