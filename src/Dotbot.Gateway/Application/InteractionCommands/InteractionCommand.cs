using Dotbot.Gateway.Dto.Responses.Discord;
using MediatR;

namespace Dotbot.Gateway.Application.InteractionCommands;

public abstract class InteractionCommand : IRequest<InteractionData>
{
    public abstract string InteractionCommandName { get; }
}