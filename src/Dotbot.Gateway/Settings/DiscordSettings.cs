namespace Dotbot.Gateway.Settings;

public class DiscordSettings
{
    public string BotToken { get; init; } = null!;
    public IDictionary<string, string> Webhooks { get; init; } = null!;
    public ulong? TestGuild { get; init; }
    public string BucketEnvPrefix { get; set; } = null!;
    public bool HealthCheckMode { get; set; }
}