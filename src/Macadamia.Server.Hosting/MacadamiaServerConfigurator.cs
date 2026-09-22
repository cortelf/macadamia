using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Macadamia.Core;
using Macadamia.Hosting.Shared;

namespace Macadamia.Server.Hosting;

public class MacadamiaServerConfigurator
{
    private readonly NatsSubscribersRegistrar _registrar = new();
    private MacadamiaServerOptions _serverOptions = new();
    private MacadamiaServerHostingOptions _hostingOptions = new();
    private NatsConnectionConfiguration _connectionConfiguration = new();

    private Func<IServiceProvider, INatsMessageSerializer>? _serializerFactory;
    private Func<IServiceProvider, INatsMessageDeserializer>? _deserializerFactory;
    
    private Action<MacadamiaServerOptions, IServiceProvider>? _serverOptionsAction;
    private Action<MacadamiaServerHostingOptions, IServiceProvider>? _hostingOptionsAction;
    private Action<NatsConnectionConfiguration, IServiceProvider>? _connectionConfigurationAction;

    public MacadamiaServerConfigurator ConfigureServerOptions(
        Action<MacadamiaServerOptions, IServiceProvider> optionsAction)
    {
        _serverOptionsAction = optionsAction;
        return this;
    }
    
    public MacadamiaServerConfigurator ConfigureServerOptions(
        Action<MacadamiaServerOptions> optionsAction)
    {
        return ConfigureServerOptions((options, _) => optionsAction(options));
    }
    
    public MacadamiaServerConfigurator ConfigureHostingOptions(
        Action<MacadamiaServerHostingOptions, IServiceProvider> optionsAction)
    {
        _hostingOptionsAction = optionsAction;
        return this;
    }
    
    public MacadamiaServerConfigurator ConfigureHostingOptions(
        Action<MacadamiaServerHostingOptions> optionsAction)
    {
        return ConfigureHostingOptions((options, _) => optionsAction(options));
    }
    
    public MacadamiaServerConfigurator ConfigureConnection(
        Action<NatsConnectionConfiguration, IServiceProvider> optionsAction)
    {
        _connectionConfigurationAction = optionsAction;
        return this;
    }
    
    public MacadamiaServerConfigurator ConfigureConnection(
        Action<NatsConnectionConfiguration> optionsAction)
    {
        return ConfigureConnection((options, _) => optionsAction(options));
    }

    public MacadamiaServerConfigurator ConfigureSerializer(
        Func<IServiceProvider, INatsMessageSerializer> factory)
    {
        _serializerFactory = factory;
        return this;
    }
    
    public MacadamiaServerConfigurator ConfigureSerializer(
        Func<INatsMessageSerializer> factory)
    {
        return ConfigureSerializer(_ => factory());
    }
    
    public MacadamiaServerConfigurator ConfigureDeserializer(
        Func<IServiceProvider, INatsMessageDeserializer> factory)
    {
        _deserializerFactory = factory;
        return this;
    }
    
    public MacadamiaServerConfigurator ConfigureDeserializer(
        Func<INatsMessageDeserializer> factory)
    {
        return ConfigureDeserializer(_ => factory());
    }
    
    public MacadamiaServerConfigurator ConfigureOptionsFromConfigurationSection(
        IConfigurationSection section)
    {
        var server = section.GetSection("Server").Get<MacadamiaServerOptions>();
        var hosting = section.GetSection("Hosting").Get<MacadamiaServerHostingOptions>();
        var connection = section.GetSection("Connection").Get<NatsConnectionConfiguration>();
        
        _serverOptions = server ?? new MacadamiaServerOptions();
        _hostingOptions = hosting ?? new MacadamiaServerHostingOptions();
        _connectionConfiguration = connection ?? new NatsConnectionConfiguration();
        return this;
    }

    public MacadamiaServerConfigurator Subscribe<TMessageContent>(string subject)
    {
        _registrar.Subscribe<TMessageContent>(subject);
        return this;
    }

    internal void ApplyToServiceCollection(IServiceCollection services)
    {
        services.AddMacadamiaNatsNetClient(_connectionConfiguration, _connectionConfigurationAction);
        services.AddMacadamiaNatsSerializers(_serializerFactory, _deserializerFactory);

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