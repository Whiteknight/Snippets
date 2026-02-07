using BaseFunctional;

namespace EventsAndMessaging;

public abstract record MessagingError(string Message) : Error(Message);

public sealed record MessageRetryError() : MessagingError("Message should be retried.");

public sealed record MessageFailedError(string Message) : MessagingError(Message);

public sealed record MessageNullError() : MessagingError("Message is null.");
