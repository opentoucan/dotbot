namespace Bot.Gateway.Settings;

public class DiscordSettings
{
    public string BotToken { get; init; } = null!;
    public IDictionary<string, string> Webhooks { get; init; } = null!;
    public ulong? TestGuild { get; init; }
}