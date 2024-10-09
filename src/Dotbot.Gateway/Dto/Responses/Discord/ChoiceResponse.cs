using System.Text.Json.Serialization;

namespace Dotbot.Gateway.Dto.Responses.Discord;

public class Choice
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("value")]
    public object Value { get; set; } = null!;
}