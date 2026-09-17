using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NATS.Client.Core;
using Nats.Reply.Core;

namespace Nats.Reply.Server;

public class NatsSubscriptionsManager(INatsConnection connection, IServiceProvider serviceProvider,
    INatsMessageSerializer serializer, INatsMessageDeserializer deserializer,
    NatsReplyServerOptions options, ILogger<NatsSubscriptionsManager> logger)
    : INatsSubscriptionsManager
{
    private const string TraceIdHeader = "traceId";

    public Task SubscribeAsync<TMessageContent>(string subject, CancellationToken cancellationToken)
        where TMessageContent: class
    {
        return SubscribeHandler<TMessageContent>(subject, cancellationToken);
    }

    private async Task SubscribeHandler<TMessageContent>(string subject, CancellationToken cancellationToken)
    where TMessageContent: class
    {
        var natsNetSerializer = new GenericNatsNetMessageSerializer<TMessageContent>
            (serializer, deserializer);
        var natsNetObjectSerializer = new GenericNatsNetMessageSerializer<object>
            (serializer, deserializer);

        await Parallel.ForEachAsync(
            connection.SubscribeAsync(subject, queueGroup: options.QueueGroup, serializer: natsNetSerializer,
                cancellationToken: cancellationToken),
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism
            },
            (message, token) => ProcessMessageAsync(message, natsNetObjectSerializer, token));
    }

    private async ValueTask ProcessMessageAsync<TMessageContent>(NatsMsg<TMessageContent> message,
        GenericNatsNetMessageSerializer<object> natsNetObjectSerializer, CancellationToken cancellationToken)
        where TMessageContent : class
    {
        var traceId = Guid.NewGuid();
        using var loggingScope = logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId
        });

        try
        {
            if (string.IsNullOrWhiteSpace(message.ReplyTo))
            {
                logger.LogWarning(
                    "Ignoring NATS message on subject {Subject} because it has no reply subject.",
                    message.Subject);
                return;
            }

            NatsReplyWrapper response;
            if (message.Error != null)
            {
                logger.LogWarning(message.Error,
                    "Unable to deserialize NATS message on subject {Subject}.", message.Subject);
                response = CreateErrorResponse(NatsReplyErrorType.BadRequest,
                    "request_deserialization_failed", "Unable to deserialize the request body.");
            }
            else if (message.Data is null)
            {
                logger.LogWarning("NATS message on subject {Subject} has an empty body.", message.Subject);
                response = CreateErrorResponse(NatsReplyErrorType.BadRequest,
                    "request_body_is_null", "Request body must not be null.");
            }
            else
            {
                response = await HandleMessageAsync(message.Subject, message.ReplyTo, message.Data,
                    message.Headers, cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
                await message.ReplyAsync(response, serializer: natsNetObjectSerializer,
                    headers: new NatsHeaders(new Dictionary<string, StringValues>
                        {[TraceIdHeader] = traceId.ToString() }),
                    cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {}
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to process or reply to NATS message on subject {Subject}.",
                message.Subject);
        }
    }

    private async Task<NatsReplyWrapper> HandleMessageAsync<TMessageContent>(string subject, string replyTo,
        TMessageContent body, NatsHeaders? headers, CancellationToken cancellationToken)
        where TMessageContent : class
    {
        try
        {
            var natsMessage = new NatsMessage
            {
                Subject = subject,
                Body = body,
                ReplyToSubject = replyTo,
                Headers = headers?.ToDictionary(x => x.Key,
                    x => x.Value.Where(value => value is not null).Select(value => value!).ToList()) ?? []
            };

            using var scope = serviceProvider.CreateScope();
            var endpoints = scope.ServiceProvider.GetRequiredService<IEnumerable<INatsMessageHandler>>();
            var matchingEndpoints = endpoints.Where(x => x.CanHandle(natsMessage)).ToArray();

            if (matchingEndpoints.Length == 0)
            {
                logger.LogWarning("No NATS message handler found for subject {Subject} and body type {BodyType}.",
                    subject, body.GetType());
                return CreateErrorResponse(NatsReplyErrorType.InternalServerError,
                    "message_handler_not_found", "No handler is configured for the registered message type.");
            }

            if (matchingEndpoints.Length > 1)
            {
                logger.LogError(
                    "More than one NATS message handler matched subject {Subject} and body type {BodyType}.",
                    subject, body.GetType());
                return CreateErrorResponse(NatsReplyErrorType.InternalServerError,
                    "ambiguous_message_handler", "More than one handler matched the request.");
            }

            try
            {
                return await matchingEndpoints[0].HandleAsync(natsMessage, cancellationToken)
                       ?? throw new InvalidOperationException("NATS message handler returned a null reply wrapper.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "NATS message handler failed for subject {Subject}.", subject);
                return CreateErrorResponse(NatsReplyErrorType.InternalServerError,
                    "message_handler_failed", "The message handler failed.");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to resolve a NATS message handler for subject {Subject}.", subject);
            return CreateErrorResponse(NatsReplyErrorType.InternalServerError,
                "message_handler_resolution_failed", "Unable to resolve a handler for the request.");
        }
    }

    private static NatsReplyWrapper CreateErrorResponse(NatsReplyErrorType type,
        string internalCode, string description)
    {
        return new NatsReplyWrapper<object>
        {
            Error = new NatsReplyError
            {
                Type = type,
                InternalCode = internalCode,
                Description = description
            }
        };
    }
}
