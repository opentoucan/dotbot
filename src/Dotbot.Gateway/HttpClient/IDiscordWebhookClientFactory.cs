using Discord.Webhook;

namespace Dotbot.Gateway.HttpClient;

public interface IDiscordWebhookClientFactory
{
    DiscordWebhookClient Create(string channel);
}