using Microsoft.Extensions.DependencyInjection;
using NATS.Extensions.Microsoft.DependencyInjection;
using Macadamia.Core;
using Macadamia.Serializers.Json;

namespace Macadamia.Hosting.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMacadamiaNatsNetClient(this IServiceCollection services,
        NatsConnectionConfiguration basicConfiguration,
        Action<NatsConnectionConfiguration, IServiceProvider>? configAction = null)
    {
        var configuration = basicConfiguration;
        services.AddSingleton<NatsConnectionConfiguration>(provider =>
        {
            configAction?.Invoke(configuration, provider);
            return configuration;
        });
        services.AddNatsClient(buildAction: builder =>
        {
            builder.ConfigureOptions(optionsBuilder =>
            {
                optionsBuilder.Configure<NatsConnectionConfiguration>((options, configuration) =>
                {
                    options.Opts = configuration.ToNatsNetOpts(options.Opts);
                });
            });
        });
        
        return services;
    }

    public static IServiceCollection AddMacadamiaNatsSerializers(this IServiceCollection services,
        Func<IServiceProvider, INatsMessageSerializer>? serializerFactory = null,
        Func<IServiceProvider, INatsMessageDeserializer>? deserializerFactory = null)
    {
        if (serializerFactory != null)
            services.AddSingleton(serializerFactory);
        else
            services.AddSingleton<INatsMessageSerializer, DefaultNatsMessageJsonSerializer>();
        if (deserializerFactory != null)
            services.AddSingleton(deserializerFactory);
        else
            services.AddSingleton<INatsMessageDeserializer, DefaultNatsMessageJsonSerializer>();
        
        return services;
    }
}
