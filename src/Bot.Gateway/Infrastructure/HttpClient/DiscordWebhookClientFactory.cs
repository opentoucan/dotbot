using Bot.Gateway.Settings;
using Discord.Webhook;
using Microsoft.Extensions.Options;

namespace Bot.Gateway.Infrastructure.HttpClient;

public class DiscordWebhookClientFactory : IDiscordWebhookClientFactory
{
    private readonly DiscordSettings _discordSettings;

    public DiscordWebhookClientFactory(IOptions<DiscordSettings> botSettings)
    {
        _discordSettings = botSettings.Value;
    }

    public DiscordWebhookClient Create(string channel)
    {
        _discordSettings.Webhooks.TryGetValue(channel, out var webhookUrl);
        return new DiscordWebhookClient(webhookUrl);
    }
}