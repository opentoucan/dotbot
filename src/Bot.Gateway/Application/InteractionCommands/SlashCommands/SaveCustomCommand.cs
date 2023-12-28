using Bot.Gateway.Model.Requests.Discord;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class SaveCustomCommand : InteractionCommand
{
    public override BotCommandType CommandType => BotCommandType.Save;
    public override InteractionRequest Data { get; set; } = null!;
}