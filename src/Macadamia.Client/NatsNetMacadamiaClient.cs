using Microsoft.Extensions.Primitives;
using NATS.Client.Core;
using Macadamia.Core;

namespace Macadamia.Client;

public class NatsNetMacadamiaClient(INatsClient client, 
    INatsMessageSerializer serializer, INatsMessageDeserializer deserializer): IMacadamiaClient
{
    public async Task<NatsReplyWrapper<TResponse>> RequestAsync<TRequest, TResponse>(
        string subject, TRequest message,  Dictionary<string, List<string>>? headers = null,
        CancellationToken cancellationToken = default) 
        where TRequest: class
        where TResponse: class
    {
        var natsNetSerializer = new GenericNatsNetMessageSerializer<TRequest>(serializer, deserializer);
        var natsNetDeserializer = new GenericNatsNetMessageSerializer<NatsReplyWrapper<TResponse>>(serializer, deserializer);

        var natsNetHeaders = new NatsHeaders(headers?.ToDictionary(
            x => x.Key, x => new StringValues(x.Value.ToArray())));
        var reply = 
            await client.RequestAsync(
                subject, message, natsNetHeaders, natsNetSerializer, natsNetDeserializer, 
                cancellationToken: cancellationToken);
        
        if (reply.Error != null)
            throw reply.Error;
        
        return reply.Data!;
    }
}