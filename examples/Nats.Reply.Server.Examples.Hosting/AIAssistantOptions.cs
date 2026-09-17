namespace Nats.Reply.Server.Examples.Hosting;

public class AIAssistantOptions
{
    public required Uri Endpoint { get; init; }
    public required string Token { get; init; }
    public required string Model { get; init; }
}