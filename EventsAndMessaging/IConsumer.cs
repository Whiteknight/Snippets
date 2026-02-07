using BaseFunctional;
using static BaseFunctional.Assert;

namespace EventsAndMessaging;

public interface IConsumer<T>
{
    public Task<Result<bool, Error>> Consume(MessageControl control, T message, CancellationToken cancellation);
}

public sealed class DelegateConsumer<T, TData> : IConsumer<T>
{
    private readonly TData _data;
    private readonly Func<TData, MessageControl, T, Task<Result<bool, Error>>> _consume;

    public DelegateConsumer(TData data, Func<TData, MessageControl, T, Task<Result<bool, Error>>> consume)
    {
        _data = data;
        _consume = NotNull(consume);
    }

    public Task<Result<bool, Error>> Consume(MessageControl control, T message, CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            return Task.FromCanceled<Result<bool, Error>>(cancellation);
        if (message == null)
            return Task.FromResult((Result<bool, Error>)new MessageNullError());
        return _consume(_data, control, message);
    }
}
