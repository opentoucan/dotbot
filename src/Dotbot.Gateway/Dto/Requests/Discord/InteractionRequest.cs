using System.Text.Json.Serialization;

namespace Dotbot.Gateway.Dto.Requests.Discord;

public class InteractionRequest
{
    [JsonPropertyName("app_permissions")]
    public string? AppPermissions { get; set; }

    [JsonPropertyName("application_id")]
    public string? ApplicationId { get; set; }

    [JsonPropertyName("channel")]
    public Channel? Channel { get; set; }

    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }
    
    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("data")]
    public Data? Data { get; set; }

    [JsonPropertyName("entitlement_sku_ids")]
    public List<object>? EntitlementSkuIds { get; set; }

    [JsonPropertyName("entitlements")]
    public List<object>? Entitlements { get; set; }

    [JsonPropertyName("guild")]
    public Guild? Guild { get; set; }

    [JsonPropertyName("guild_id")]
    public string? GuildId { get; set; }

    [JsonPropertyName("guild_locale")]
    public string? GuildLocale { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("member")]
    public Member? Member { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }
}