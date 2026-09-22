using Microsoft.Extensions.DependencyInjection;

namespace Macadamia.Client.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMacadamiaClient(this IServiceCollection services, 
        Action<MacadamiaClientConfigurator>? configuratorAction = null)
    {
        var configurator = new MacadamiaClientConfigurator();
        configuratorAction?.Invoke(configurator);
        configurator.ApplyToServiceCollection(services);
        return services;
    }
}