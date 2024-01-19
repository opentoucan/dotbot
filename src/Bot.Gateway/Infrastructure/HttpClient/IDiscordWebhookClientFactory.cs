using Discord.Webhook;

namespace Bot.Gateway.Infrastructure.HttpClient;

public interface IDiscordWebhookClientFactory
{
    DiscordWebhookClient Create(string channel);
}