namespace Nats.Reply.Server.Hosting;

public class NatsSubscribersRegistrar
{
    public record SubscriberDescriptor(Type MessageType, string Subject);
    private readonly HashSet<SubscriberDescriptor> _subscriberDescriptors = [];
    
    internal List<SubscriberDescriptor>  SubscriberDescriptors => _subscriberDescriptors.ToList();

    public NatsSubscribersRegistrar Subscribe<TMessageContent>(string subject)
    {
        _subscriberDescriptors.Add(new SubscriberDescriptor(typeof(TMessageContent), subject));
        return this;
    }
}
