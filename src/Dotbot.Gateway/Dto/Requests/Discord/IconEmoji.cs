using System.Text.Json.Serialization;

namespace Dotbot.Gateway.Dto.Requests.Discord;

public class IconEmoji
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}