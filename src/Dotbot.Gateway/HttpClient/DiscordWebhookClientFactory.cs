using Discord.Webhook;
using Dotbot.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.HttpClient;

public class DiscordWebhookClientFactory : IDiscordWebhookClientFactory
{
    private readonly Settings.Discord _discord;

    public DiscordWebhookClientFactory(IOptions<Settings.Discord> botSettings)
    {
        _discord = botSettings.Value;
    }

    public DiscordWebhookClient Create(string channel)
    {
        _discord.Webhooks.TryGetValue(channel, out var webhookUrl);
        return new DiscordWebhookClient(webhookUrl);
    }
}