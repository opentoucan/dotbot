using System.Text.Json.Serialization;

namespace Dotbot.Gateway.Dto.Requests.Discord;

public class Attachment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = null!;

    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
    
    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = null!;
    
    [JsonPropertyName("size")]
    public uint Size { get; set; }
}