using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nats.Reply.Core;
using Nats.Reply.Hosting.Shared;

namespace Nats.Reply.Server.Hosting;

public class NatsReplyServerConfigurator
{
    private readonly NatsSubscribersRegistrar _registrar = new();
    private NatsReplyServerOptions _serverOptions = new();
    private NatsReplyServerHostingOptions _hostingOptions = new();
    private NatsConnectionConfiguration _connectionConfiguration = new();

    private Func<IServiceProvider, INatsMessageSerializer>? _serializerFactory;
    private Func<IServiceProvider, INatsMessageDeserializer>? _deserializerFactory;
    
    private Action<NatsReplyServerOptions, IServiceProvider>? _serverOptionsAction;
    private Action<NatsReplyServerHostingOptions, IServiceProvider>? _hostingOptionsAction;
    private Action<NatsConnectionConfiguration, IServiceProvider>? _connectionConfigurationAction;

    public NatsReplyServerConfigurator ConfigureServerOptions(
        Action<NatsReplyServerOptions, IServiceProvider> optionsAction)
    {
        _serverOptionsAction = optionsAction;
        return this;
    }
    
    public NatsReplyServerConfigurator ConfigureServerOptions(
        Action<NatsReplyServerOptions> optionsAction)
    {
        return ConfigureServerOptions((options, _) => optionsAction(options));
    }
    
    public NatsReplyServerConfigurator ConfigureHostingOptions(
        Action<NatsReplyServerHostingOptions, IServiceProvider> optionsAction)
    {
        _hostingOptionsAction = optionsAction;
        return this;
    }
    
    public NatsReplyServerConfigurator ConfigureHostingOptions(
        Action<NatsReplyServerHostingOptions> optionsAction)
    {
        return ConfigureHostingOptions((options, _) => optionsAction(options));
    }
    
    public NatsReplyServerConfigurator ConfigureConnection(
        Action<NatsConnectionConfiguration, IServiceProvider> optionsAction)
    {
        _connectionConfigurationAction = optionsAction;
        return this;
    }
    
    public NatsReplyServerConfigurator ConfigureConnection(
        Action<NatsConnectionConfiguration> optionsAction)
    {
        return ConfigureConnection((options, _) => optionsAction(options));
    }

    public NatsReplyServerConfigurator ConfigureSerializer(
        Func<IServiceProvider, INatsMessageSerializer> factory)
    {
        _serializerFactory = factory;
        return this;
    }
    
    public NatsReplyServerConfigurator ConfigureSerializer(
        Func<INatsMessageSerializer> factory)
    {
        return ConfigureSerializer(_ => factory());
    }
    
    public NatsReplyServerConfigurator ConfigureDeserializer(
        Func<IServiceProvider, INatsMessageDeserializer> factory)
    {
        _deserializerFactory = factory;
        return this;
    }
    
    public NatsReplyServerConfigurator ConfigureDeserializer(
        Func<INatsMessageDeserializer> factory)
    {
        return ConfigureDeserializer(_ => factory());
    }
    
    public NatsReplyServerConfigurator ConfigureOptionsFromConfigurationSection(
        IConfigurationSection section)
    {
        var server = section.GetSection("Server").Get<NatsReplyServerOptions>();
        var hosting = section.GetSection("Hosting").Get<NatsReplyServerHostingOptions>();
        var connection = section.GetSection("Connection").Get<NatsConnectionConfiguration>();
        
        _serverOptions = server ?? new NatsReplyServerOptions();
        _hostingOptions = hosting ?? new NatsReplyServerHostingOptions();
        _connectionConfiguration = connection ?? new NatsConnectionConfiguration();
        return this;
    }

    public NatsReplyServerConfigurator Subscribe<TMessageContent>(string subject)
    {
        _registrar.Subscribe<TMessageContent>(subject);
        return this;
    }

    internal void ApplyToServiceCollection(IServiceCollection services)
    {
        services.AddNatsReplyNatsNetClient(_connectionConfiguration, _connectionConfigurationAction);
        services.AddNatsReplyNatsSerializers(_serializerFactory, _deserializerFactory);

        services.AddSingleton(provider =>
        {
            _serverOptionsAction?.Invoke(_serverOptions, provider);
            Validator.ValidateObject(_serverOptions, new ValidationContext(_serverOptions), validateAllProperties: true);
            return _serverOptions;
        });
        
        services.AddSingleton(provider =>
        {
            _hostingOptionsAction?.Invoke(_hostingOptions, provider);
            Validator.ValidateObject(_hostingOptions, new ValidationContext(_hostingOptions), validateAllProperties: true);
            return _hostingOptions;
        });
        
        services.AddSingleton<INatsSubscriptionsManager, NatsSubscriptionsManager>();
        services.AddHostedService<NatsSubscriberHostingService>();
        
        services.AddSingleton(_registrar);
    }
}