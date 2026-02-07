using static BaseFunctional.Assert;

namespace EventsAndMessaging;

public interface IConsumer<T>
{
    public Task Consume(MessageControl control, T message, CancellationToken cancellation);
}

public sealed class DelegateConsumer<T, TData> : IConsumer<T>
{
    private readonly TData _data;
    private readonly Func<TData, MessageControl, T, Task> _consume;

    public DelegateConsumer(TData data, Func<TData, MessageControl, T, Task> func)
    {
        _data = data;
        _consume = NotNull(func);
    }

    public Task Consume(MessageControl control, T message, CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            return Task.FromCanceled(cancellation);
        if (message == null)
            return Task.CompletedTask;
        return _consume(_data, control, message);
    }
}
