namespace Dotbot.Api.Settings;

public class DiscordSettings
{
    public string Token { get; init; } = null!;
    public string BucketEnvPrefix { get; set; } = null!;
}