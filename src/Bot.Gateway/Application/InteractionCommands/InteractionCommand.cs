using Bot.Gateway.Dto.Responses.Discord;
using MediatR;

namespace Bot.Gateway.Application.InteractionCommands;

public abstract class InteractionCommand : IRequest<InteractionData>
{
    public abstract string InteractionCommandName { get; }
}