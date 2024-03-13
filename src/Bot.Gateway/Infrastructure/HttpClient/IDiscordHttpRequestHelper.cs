using Bot.Gateway.Dto.Responses.Discord;

namespace Bot.Gateway.Infrastructure.HttpClient;

public interface IDiscordHttpRequestHelper
{
    Task SendFollowupMessageAsync(string applicationId, string token, InteractionData data, CancellationToken cancellationToken);
    Task SendWebhookMessageAsync(string webhook, InteractionData data, string? username = null, string? avatarUrl = null);
}