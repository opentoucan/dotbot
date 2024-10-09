using Dotbot.Gateway.Dto.Responses.Discord;

namespace Dotbot.Gateway.HttpClient;

public interface IDiscordHttpRequestHelper
{
    Task SendFollowupMessageAsync(string applicationId, string token, InteractionData data, CancellationToken cancellationToken);
    Task SendWebhookMessageAsync(string webhook, InteractionData data, string? username = null, string? avatarUrl = null);
}