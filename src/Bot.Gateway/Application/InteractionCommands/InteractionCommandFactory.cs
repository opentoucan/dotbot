using AutoMapper;
using Bot.Gateway.Dto.Requests.Discord;

namespace Bot.Gateway.Application.InteractionCommands;

public interface IInteractionCommandFactory
{
    InteractionCommand Create(string interactionCommandName, InteractionRequest interactionRequest);
}

public class InteractionCommandFactory(IEnumerable<InteractionCommand> botCommands, IMapper mapper) : IInteractionCommandFactory
{
    public InteractionCommand Create(string interactionCommandName, InteractionRequest interactionRequest)
    {
        var botCommand = botCommands.Single(bc => bc.InteractionCommandName.Equals(interactionCommandName, StringComparison.InvariantCultureIgnoreCase));
        mapper.Map(interactionRequest, botCommand);
        return botCommand;
    }
}