using Bot.Gateway.Model.Requests.Discord;

namespace Bot.Gateway.Application.InteractionCommands.UserCommands;

public class GreetCommand : InteractionCommand
{
    public override BotCommandType CommandType => BotCommandType.Greet;
    public override InteractionRequest Data { get; set; } = null!;
}