using Macadamia.Core;

namespace Macadamia.Server;

public abstract class NatsMessageHandler<TMessageContent, TResponse>
    : INatsMessageHandler
    where TMessageContent : class
    where TResponse : class
{
    protected NatsMessage Message { get; private set; } = null!;

    public virtual bool CanHandle(NatsMessage message)
    {
        return message.Body is TMessageContent;
    }

    protected abstract Task<NatsReplyWrapper<TResponse>> HandleAsync(TMessageContent message, CancellationToken cancellationToken);

    protected NatsReplyWrapper<TResponse> Ok(TResponse response)
    {
        return new NatsReplyWrapper<TResponse>
        {
            Result = response,
            Error = null,
        };
    }
    
    protected NatsReplyWrapper<TResponse> Error(NatsReplyErrorType  errorType, string? internalCode = null, string? description = null)
    {
        return new NatsReplyWrapper<TResponse>
        {
            Result = null,
            Error = new NatsReplyError
            {
                Type = errorType,
                InternalCode = internalCode,
                Description = description
            },
        };
    }

    public async Task<NatsReplyWrapper> HandleAsync(NatsMessage message, CancellationToken cancellationToken)
    {
        Message = message;
        return await HandleAsync((message.Body as TMessageContent)!, cancellationToken)
               ?? throw new InvalidOperationException("NATS message handler returned a null reply wrapper.");
    }
}
