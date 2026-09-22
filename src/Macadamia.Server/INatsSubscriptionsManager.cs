namespace Macadamia.Server;

public interface INatsSubscriptionsManager
{
    public Task SubscribeAsync<TMessageContent>(string subject, CancellationToken cancellationToken)
        where TMessageContent: class;
}
