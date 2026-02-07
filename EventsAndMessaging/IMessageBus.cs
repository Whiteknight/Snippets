namespace EventsAndMessaging;

public interface IMessageBus
{
    IPubSubChannel<T> GetPubSubChannel<T>();

    IRequestResponseChannel<T> GetRequestResponseChannel<T>();
}
