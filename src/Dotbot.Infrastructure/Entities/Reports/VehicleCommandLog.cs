namespace Dotbot.Infrastructure.Entities.Reports;

public class VehicleCommandLog : Entity
{
    public required string RegistrationPlate { get; set; }
    public DateTimeOffset RequestDate { get; set; }
    public required string UserId { get; set; }
    public required string GuildId { get; set; }
}