namespace Nats.Reply.Core;

public enum NatsReplyErrorType
{
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    ImATeapot = 418,
    TooManyRequests = 429,
    InternalServerError = 500,
}

public class NatsReplyError
{
    public required NatsReplyErrorType Type { get; init; }
    public string? InternalCode { get; init; }
    public string? Description { get; init; }
}