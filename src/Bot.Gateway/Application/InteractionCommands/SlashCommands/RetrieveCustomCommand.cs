using Bot.Gateway.Model.Requests.Discord;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class RetrieveCustomCommand : InteractionCommand
{
    public override BotCommandType CommandType => BotCommandType.Custom;
    public override InteractionRequest Data { get; set; } = null!;
}