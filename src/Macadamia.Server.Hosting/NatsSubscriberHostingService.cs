using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Macadamia.Server.Hosting;

internal sealed class NatsSubscriberHostingService(
    NatsSubscribersRegistrar registrar,
    INatsSubscriptionsManager subscriptionsManager,
    MacadamiaServerHostingOptions options,
    ILogger<NatsSubscriberHostingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var subscriptionCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var subscriptionTasks = registrar.SubscriberDescriptors
            .Select(descriptor => MonitorSubscriptionAsync(descriptor, subscriptionCts))
            .ToArray();

        await Task.WhenAll(subscriptionTasks);
    }

    private async Task MonitorSubscriptionAsync(NatsSubscribersRegistrar.SubscriberDescriptor descriptor,
        CancellationTokenSource subscriptionCts)
    {
        try
        {
            await StartSubscriptionAsync(descriptor, subscriptionCts.Token);

            if (!subscriptionCts.IsCancellationRequested)
                throw new NatsSubscriptionException(
                    $"NATS subscription for subject '{descriptor.Subject}' completed unexpectedly.");
        }
        catch (OperationCanceledException) when (subscriptionCts.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "NATS subscription failed for subject {Subject} and message type {MessageType}.",
                descriptor.Subject, descriptor.MessageType);

            if (!options.StopHostOnSubscriptionFailure)
                return;

            await subscriptionCts.CancelAsync();
            throw;
        }
    }

    private Task StartSubscriptionAsync(NatsSubscribersRegistrar.SubscriberDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        var methodDefinition = typeof(INatsSubscriptionsManager)
            .GetMethod(nameof(INatsSubscriptionsManager.SubscribeAsync));
        if (methodDefinition == null)
            throw new InvalidOperationException();

        var constructedMethod = methodDefinition.MakeGenericMethod(descriptor.MessageType);
        var result = constructedMethod.Invoke(subscriptionsManager, [descriptor.Subject, cancellationToken]);
        return (Task)result!;
    }
}
