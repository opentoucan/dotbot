using System.Text.Json.Serialization;

namespace Bot.Gateway.Model.Requests.Discord;

public class Options
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    
    [JsonPropertyName("type")]
    public int Type { get; set; }
    
    [JsonPropertyName("value")]
    public object? Value { get; set; }
    
    [JsonPropertyName("options")]
    public List<Options>? SubOptions { get; set; }
    
    [JsonPropertyName("focused")]
    public bool? Focused { get; set; }
}