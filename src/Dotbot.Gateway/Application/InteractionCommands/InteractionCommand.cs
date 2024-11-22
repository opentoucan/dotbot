using Dotbot.Gateway.Dto.Requests.Discord;
using Dotbot.Gateway.Dto.Responses.Discord;
using MediatR;

namespace Dotbot.Gateway.Application.InteractionCommands;

public abstract class InteractionCommand : IRequest<InteractionData>
{
    public abstract string InteractionCommandName { get; }
    public abstract void MapFromInteractionRequest(InteractionRequest interactionRequest);

}