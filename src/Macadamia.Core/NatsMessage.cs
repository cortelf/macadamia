namespace Macadamia.Core;

public class NatsMessage
{
    public required string Subject { get; init; }
    public required string ReplyToSubject { get; init; }
    public required object Body { get; init; }
    public required Dictionary<string, List<string>> Headers { get; init; }
}