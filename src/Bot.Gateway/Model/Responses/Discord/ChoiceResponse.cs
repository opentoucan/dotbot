using System.Text.Json.Serialization;

namespace Bot.Gateway.Model.Responses.Discord;

public class Choice
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("value")]
    public object Value { get; set; } = null!;
}