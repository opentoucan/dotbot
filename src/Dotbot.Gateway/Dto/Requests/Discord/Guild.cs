using System.Text.Json.Serialization;

namespace Dotbot.Gateway.Dto.Requests.Discord;

public class Guild
{
    [JsonPropertyName("features")]
    public required List<string> Features { get; set; }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }
}