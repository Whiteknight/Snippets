namespace EventsAndMessaging;

public interface IMessageBus
{
    IPubSubChannel<T> GetPubSubChannel<T>();

    IRequestResponseChannel<TRequest, TResponse> GetRequestResponseChannel<TRequest, TResponse>();
}
