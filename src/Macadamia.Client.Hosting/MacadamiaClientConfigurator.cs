using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Macadamia.Core;
using Macadamia.Hosting.Shared;

namespace Macadamia.Client.Hosting;

public class MacadamiaClientConfigurator
{
    private NatsConnectionConfiguration _connectionConfiguration = new();

    private Func<IServiceProvider, INatsMessageSerializer>? _serializerFactory;
    private Func<IServiceProvider, INatsMessageDeserializer>? _deserializerFactory;

    private Action<NatsConnectionConfiguration, IServiceProvider>? _connectionConfigurationAction;
    
    public MacadamiaClientConfigurator ConfigureConnection(
        Action<NatsConnectionConfiguration, IServiceProvider> optionsAction)
    {
        _connectionConfigurationAction = optionsAction;
        return this;
    }
    
    public MacadamiaClientConfigurator ConfigureConnection(
        Action<NatsConnectionConfiguration> optionsAction)
    {
        return ConfigureConnection((options, _) => optionsAction(options));
    }

    public MacadamiaClientConfigurator ConfigureSerializer(
        Func<IServiceProvider, INatsMessageSerializer> factory)
    {
        _serializerFactory = factory;
        return this;
    }
    
    public MacadamiaClientConfigurator ConfigureSerializer(
        Func<INatsMessageSerializer> factory)
    {
        return ConfigureSerializer(_ => factory());
    }
    
    public MacadamiaClientConfigurator ConfigureDeserializer(
        Func<IServiceProvider, INatsMessageDeserializer> factory)
    {
        _deserializerFactory = factory;
        return this;
    }
    
    public MacadamiaClientConfigurator ConfigureDeserializer(
        Func<INatsMessageDeserializer> factory)
    {
        return ConfigureDeserializer(_ => factory());
    }
    
    public MacadamiaClientConfigurator ConfigureOptionsFromConfigurationSection(
        IConfigurationSection section)
    {
        var connection = section.GetSection("Connection").Get<NatsConnectionConfiguration>();
        _connectionConfiguration = connection ?? new NatsConnectionConfiguration();
        return this;
    }

    internal void ApplyToServiceCollection(IServiceCollection services)
    {
        services.AddMacadamiaNatsNetClient(_connectionConfiguration, _connectionConfigurationAction);
        services.AddMacadamiaNatsSerializers(_serializerFactory, _deserializerFactory);
        
        services.AddSingleton<IMacadamiaClient, NatsNetMacadamiaClient>();
    }
}