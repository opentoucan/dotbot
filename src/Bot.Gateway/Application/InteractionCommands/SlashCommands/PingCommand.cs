using Bot.Gateway.Model.Requests.Discord;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class PingCommand : InteractionCommand
{
    public override BotCommandType CommandType => BotCommandType.Ping;
    public override InteractionRequest Data { get; set; } = null!;
}