using NATS.Client.Core;

namespace Macadamia.Hosting.Shared;

public static class NatsConnectionConfigurationMapper
{
    public static NatsOpts ToNatsNetOpts(
        this NatsConnectionConfiguration cfg, NatsOpts original)
    {
        var authentication = cfg.Authentication;

        return original with
        {
            Url = cfg.Url,
            Name = cfg.Name,
            AuthOpts = authentication is null
                ? NatsAuthOpts.Default
                : new NatsAuthOpts
                {
                    Username = authentication.Username,
                    Password = authentication.Password,
                    Token = authentication.Token,
                    Jwt = authentication.Jwt,
                    Seed = authentication.Seed,
                },
            InboxPrefix =  cfg.InboxPrefix,
            RequestTimeout = TimeSpan.FromSeconds(cfg.RequestTimeoutInSeconds)
        };
    }
}
