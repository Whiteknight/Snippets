namespace EventsAndMessaging;

public class MessageControl
{
    // Unique ID for the message
    public string MessageId { get; }

    // Correlation id for messages in a single transaction
    public string CorrelationId { get; }

    public IMessageBus CorrelationBus { get; }

    public string SubscriptionId { get; }

    public string QueueName { get; }

    public void Retry(TimeSpan delay = default)
    {
        // TODO: Throw some kind of expection to force a message retry.
    }

    public void Fail(string? error = null)
    {
        // TODO: Throw some kind of exception to force the message into deadletter
    }
}
