using System.Text.Json.Serialization;

namespace Bot.Gateway.Model.Requests.Discord;

public class Data
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("type")]
    public int? Type { get; set; }

    [JsonPropertyName("resolved")]
    public Resolved? Resolved { get; set; }
    
    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    [JsonPropertyName("target_id")]
    public string? TargetId { get; set; }
    
    [JsonPropertyName("options")]
    public List<Options>? Options { get; set; }
}