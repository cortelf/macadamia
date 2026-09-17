using System.ComponentModel.DataAnnotations;

namespace Nats.Reply.Server;

public class NatsReplyServerOptions
{
    [MinLength(1)]
    public string? QueueGroup { get; set; } = "nats-reply";

    [Range(1, int.MaxValue)]
    public int MaxDegreeOfParallelism { get; set; } = int.MaxValue;
}
