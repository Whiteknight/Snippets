namespace EventsAndMessaging;

// Options are dependent on the internal implementation.
public readonly record struct RequestOptions();

public interface IRequestResponseChannel<T>
{
    Task<TResponse> SendRequest<TResponse>(T message, RequestOptions options);

    IDisposable Listen(IListener<T> consumer);
}
