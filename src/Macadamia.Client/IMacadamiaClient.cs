using Macadamia.Core;

namespace Macadamia.Client;

public interface IMacadamiaClient
{
    Task<NatsReplyWrapper<TResponse>> RequestAsync<TRequest, TResponse>
        (string subject, TRequest message, Dictionary<string, List<string>>? headers = null,
            CancellationToken cancellationToken = default) 
        where TRequest: class
        where TResponse: class;
}