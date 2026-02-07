using BaseFunctional;

namespace EventsAndMessaging;

public interface IListener<TRequest, TResponse>
{
    public Task<Result<TResponse, Error>> Listen(MessageControl control, TRequest request);
}
