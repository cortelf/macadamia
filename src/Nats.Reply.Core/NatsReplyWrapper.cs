namespace Nats.Reply.Core;

public abstract class NatsReplyWrapper
{
    public NatsReplyError? Error { get; init; }
}

public class NatsReplyWrapper<TBody> : NatsReplyWrapper
{
    public TBody? Result { get; init; }
}
