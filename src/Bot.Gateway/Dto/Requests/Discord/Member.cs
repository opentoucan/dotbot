using System.Text.Json.Serialization;

namespace Bot.Gateway.Dto.Requests.Discord;

public class Member
{
    [JsonPropertyName("user")]
    public User User { get; set; } = null!;

    [JsonPropertyName("avatar")]
    public object? Avatar { get; set; }

    [JsonPropertyName("communication_disabled_until")]
    public object? CommunicationDisabledUntil { get; set; }

    [JsonPropertyName("flags")]
    public int Flags { get; set; }

    [JsonPropertyName("joined_at")]
    public DateTime JoinedAt { get; set; }

    [JsonPropertyName("nick")]
    public string? Nick { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("permissions")]
    public string? Permissions { get; set; }

    [JsonPropertyName("premium_since")]
    public object? PremiumSince { get; set; }

    [JsonPropertyName("roles")]
    public required List<string> Roles { get; set; }

    [JsonPropertyName("unusual_dm_activity_until")]
    public object? UnusualDmActivityUntil { get; set; }
}
