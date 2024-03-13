using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Bot.Gateway.Dto.Responses.Discord;
using Discord;

namespace Bot.Gateway.Infrastructure.HttpClient;

public class DiscordHttpRequestHelper(IHttpClientFactory httpClientFactory, IDiscordWebhookClientFactory discordWebhookClientFactory, ILogger<DiscordHttpRequestHelper> logger) 
    : IDiscordHttpRequestHelper
{
    public async Task SendFollowupMessageAsync(string applicationId, string token, InteractionData data, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("discord");
        var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(jsonString, Encoding.UTF8, "application/json"), "payload_json");
        foreach (var item in data.FileAttachments?.Select((value, i) => (value, i))!)
        {
            var byteArrayContent = new ByteArrayContent(item.value.FileContent);
            byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeTypes.GetMimeType(item.value.Filename));
            content.Add(byteArrayContent, $"files[{item.i}]", item.value.Filename);
        }
                        
        var response = await httpClient.PostAsync($"/api/v10/webhooks/{applicationId}/{token}", content, cancellationToken);
        logger.LogInformation("Followup message response - Status Code: <{0}>, Reason Phrase <{1}>", response.StatusCode, response.ReasonPhrase);
    }

    public async Task SendWebhookMessageAsync(string webhook, InteractionData data, string? username = null, string? avatarUrl = null)
    {
        var webhookClient = discordWebhookClientFactory.Create(webhook);
        if (data.FileAttachments?.Count > 0)
        {
            await webhookClient.SendFilesAsync(
                attachments: data.FileAttachments.Select(fa => 
                    new FileAttachment(new MemoryStream(fa.FileContent), fa.Filename, fa.Description, fa.IsSpoiler)),
                text: data.Content, 
                embeds: data.Embeds,
                username: username,
                avatarUrl: avatarUrl);
        }
        else
        {
            await webhookClient.SendMessageAsync(
                text: data.Content, 
                embeds: data.Embeds,
                username: username,
                avatarUrl: avatarUrl);
        }
    }
}