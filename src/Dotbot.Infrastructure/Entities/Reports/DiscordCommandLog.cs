namespace Dotbot.Infrastructure.Entities.Reports;

public class DiscordCommandLog : Entity
{
    public required string CommandName { get; set; }
    public required string Identifier { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    public required string UserId { get; set; }
    public required string GuildId { get; set; }
}