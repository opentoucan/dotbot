using System.Text.Json.Serialization;

namespace Bot.Gateway.Model.Requests.Discord;

public class Resolved
{
    [JsonPropertyName("members")]
    public IDictionary<ulong, Member>? Members { get; set; }

    [JsonPropertyName("users")]
    public IDictionary<ulong, User>? Users { get; set; }
    
    [JsonPropertyName("attachments")]
    public IDictionary<ulong, Attachment>? Attachments { get; set; }
}