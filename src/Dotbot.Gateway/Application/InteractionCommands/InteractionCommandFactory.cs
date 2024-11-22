using AutoMapper;
using Dotbot.Gateway.Dto.Requests.Discord;

namespace Dotbot.Gateway.Application.InteractionCommands;

public interface IInteractionCommandFactory
{
    InteractionCommand Create(string interactionCommandName, InteractionRequest interactionRequest);
}

public class InteractionCommandFactory(IEnumerable<InteractionCommand> botCommands) : IInteractionCommandFactory
{
    public InteractionCommand Create(string interactionCommandName, InteractionRequest interactionRequest)
    {
        var botCommand = botCommands.Single(bc => bc.InteractionCommandName.Equals(interactionCommandName, StringComparison.InvariantCultureIgnoreCase));
        botCommand.MapFromInteractionRequest(interactionRequest);
        return botCommand;
    }
}