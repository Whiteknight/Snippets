namespace EventsAndMessaging;

// Options are dependent on the internal implementation.
public readonly record struct PublishOptions();

public interface IPubSubChannel<T>
{
    Task Publish(T message, PublishOptions options);

    IDisposable Subscribe(IConsumer<T> consumer);
}
