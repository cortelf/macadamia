using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nats.Reply.Core;
using Nats.Reply.Hosting.Shared;

namespace Nats.Reply.Client.Hosting;

public class NatsReplyClientConfigurator
{
    private NatsConnectionConfiguration _connectionConfiguration = new();

    private Func<IServiceProvider, INatsMessageSerializer>? _serializerFactory;
    private Func<IServiceProvider, INatsMessageDeserializer>? _deserializerFactory;

    private Action<NatsConnectionConfiguration, IServiceProvider>? _connectionConfigurationAction;
    
    public NatsReplyClientConfigurator ConfigureConnection(
        Action<NatsConnectionConfiguration, IServiceProvider> optionsAction)
    {
        _connectionConfigurationAction = optionsAction;
        return this;
    }
    
    public NatsReplyClientConfigurator ConfigureConnection(
        Action<NatsConnectionConfiguration> optionsAction)
    {
        return ConfigureConnection((options, _) => optionsAction(options));
    }

    public NatsReplyClientConfigurator ConfigureSerializer(
        Func<IServiceProvider, INatsMessageSerializer> factory)
    {
        _serializerFactory = factory;
        return this;
    }
    
    public NatsReplyClientConfigurator ConfigureSerializer(
        Func<INatsMessageSerializer> factory)
    {
        return ConfigureSerializer(_ => factory());
    }
    
    public NatsReplyClientConfigurator ConfigureDeserializer(
        Func<IServiceProvider, INatsMessageDeserializer> factory)
    {
        _deserializerFactory = factory;
        return this;
    }
    
    public NatsReplyClientConfigurator ConfigureDeserializer(
        Func<INatsMessageDeserializer> factory)
    {
        return ConfigureDeserializer(_ => factory());
    }
    
    public NatsReplyClientConfigurator ConfigureOptionsFromConfigurationSection(
        IConfigurationSection section)
    {
        var connection = section.GetSection("Connection").Get<NatsConnectionConfiguration>();
        _connectionConfiguration = connection ?? new NatsConnectionConfiguration();
        return this;
    }

    internal void ApplyToServiceCollection(IServiceCollection services)
    {
        services.AddNatsReplyNatsNetClient(_connectionConfiguration, _connectionConfigurationAction);
        services.AddNatsReplyNatsSerializers(_serializerFactory, _deserializerFactory);
        
        services.AddSingleton<INatsReplyClient, NatsNetNatsReplyClient>();
    }
}