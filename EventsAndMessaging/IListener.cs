namespace EventsAndMessaging;

public interface IListener<T>
{
    public void Listen(MessageControl control, T request);
}
