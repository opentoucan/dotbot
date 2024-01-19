using Bot.Gateway.Model.Responses.Discord;

namespace Bot.Gateway.Infrastructure.HttpClient;

public interface IDiscordHttpRequestHelper
{
    Task SendFollowupMessageAsync(ulong applicationId, string token, InteractionData data, CancellationToken cancellationToken);
    Task SendWebhookMessageAsync(string webhook, InteractionData data, string? username = null, string? avatarUrl = null);
}