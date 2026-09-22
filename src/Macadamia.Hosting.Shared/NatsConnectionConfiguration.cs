namespace Macadamia.Hosting.Shared;

public class NatsConnectionAuthenticationConfiguration
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Token { get; set; }
    public string? Jwt { get; set; }
    public string? Seed { get; set; }
}

public class NatsConnectionConfiguration
{
    public string Url { get; set; } = "nats://localhost:4222";
    public string Name { get; set; } = "macadamia";
    public NatsConnectionAuthenticationConfiguration? Authentication { get; set; }
    public string InboxPrefix { get; set; } = "_INBOX";
    public int RequestTimeoutInSeconds { get; set; } = 5;
}