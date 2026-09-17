using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Nats.Reply.Server.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNatsReplyServer(this IServiceCollection services, 
        Action<NatsReplyServerConfigurator> configuratorAction)
    { 
        var configurator = new NatsReplyServerConfigurator();
        configuratorAction.Invoke(configurator);
        configurator.ApplyToServiceCollection(services);
        return services;
    }
    
    public static IServiceCollection RegisterNatsMessageHandler<TMessageHandler>(this IServiceCollection services)
    where TMessageHandler: class, INatsMessageHandler
    {
        services.AddScoped<INatsMessageHandler, TMessageHandler>();
        return services;
    }
    
    public static IServiceCollection RegisterNatsMessageHandlersFromAssembly(this IServiceCollection services, 
        Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var types = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && 
                           typeof(INatsMessageHandler).IsAssignableFrom(type));
        foreach (var type in types)
        {
            services.Add(new ServiceDescriptor(typeof(INatsMessageHandler), type, lifetime));
        }
        return services;
    }
}
