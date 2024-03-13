using System.Text.Json.Serialization;

namespace Bot.Gateway.Dto.Requests.Discord;

public class Channel
{
    [JsonPropertyName("flags")]
    public int Flags { get; set; }

    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    [JsonPropertyName("icon_emoji")]
    public IconEmoji? IconEmoji { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("last_message_id")]
    public string? LastMessageId { get; set; }

    [JsonPropertyName("last_pin_timestamp")]
    public DateTime LastPinTimestamp { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nsfw")]
    public bool Nsfw { get; set; }

    [JsonPropertyName("parent_id")]
    public string? ParentId { get; set; }

    [JsonPropertyName("permissions")]
    public string? Permissions { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("rate_limit_per_user")]
    public int RateLimitPerUser { get; set; }

    [JsonPropertyName("theme_color")]
    public object? ThemeColor { get; set; }

    [JsonPropertyName("topic")]
    public object? Topic { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }
}