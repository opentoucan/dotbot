using Bot.Gateway.Model.Requests.Discord;

namespace Bot.Gateway.Application.InteractionCommands;

public interface IBotCommandFactory
{
    InteractionCommand Create(BotCommandType botCommandType, InteractionRequest interactionRequest);
}

public class SlashCommandFactory(IEnumerable<InteractionCommand> botCommands) : IBotCommandFactory
{
    public InteractionCommand Create(BotCommandType botCommandType, InteractionRequest interactionRequest)
    {
        var botCommand = botCommands.Single(bc => bc.CommandType == botCommandType);
        botCommand.Data = interactionRequest;
        return botCommand;
    }
        
}