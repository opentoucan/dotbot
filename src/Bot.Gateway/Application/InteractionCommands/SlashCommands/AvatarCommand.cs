using Bot.Gateway.Model.Requests.Discord;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class AvatarCommand : InteractionCommand
{
    public override BotCommandType CommandType => BotCommandType.Avatar;
    public override InteractionRequest Data { get; set; } = null!;
}