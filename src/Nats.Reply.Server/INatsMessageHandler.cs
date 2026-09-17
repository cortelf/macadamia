using Nats.Reply.Core;

namespace Nats.Reply.Server;

public interface INatsMessageHandler
{
    public bool CanHandle(NatsMessage message);
    public Task<NatsReplyWrapper> HandleAsync(NatsMessage message, CancellationToken cancellationToken);
}
