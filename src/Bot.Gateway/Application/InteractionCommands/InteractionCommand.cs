using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using MediatR;

namespace Bot.Gateway.Application.InteractionCommands;

public abstract class InteractionCommand : IRequest<InteractionData>
{
    public abstract string InteractionCommandName { get; }
    public abstract void MapFromInteractionRequest(InteractionRequest interactionRequest);
}