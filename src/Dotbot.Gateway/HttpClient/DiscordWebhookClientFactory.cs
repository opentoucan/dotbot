using Discord.Webhook;
using Dotbot.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.HttpClient;

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