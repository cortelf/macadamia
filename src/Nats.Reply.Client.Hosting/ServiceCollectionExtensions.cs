using Microsoft.Extensions.DependencyInjection;

namespace Nats.Reply.Client.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNatsReplyClient(this IServiceCollection services, 
        Action<NatsReplyClientConfigurator>? configuratorAction = null)
    {
        var configurator = new NatsReplyClientConfigurator();
        configuratorAction?.Invoke(configurator);
        configurator.ApplyToServiceCollection(services);
        return services;
    }
}