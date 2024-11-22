using Dotbot.Gateway.Dto.Requests.Discord;
using Dotbot.Gateway.Dto.Responses.Discord;
using MediatR;

namespace Dotbot.Gateway.Application.InteractionCommands.SlashCommands;

public class PingCommandHandler : IRequestHandler<PingCommand, InteractionData>
{
    public Task<InteractionData> Handle(PingCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new InteractionData("pong"));
    }
}

public class PingCommand : InteractionCommand
{
    public override string InteractionCommandName => "ping";
    public override void MapFromInteractionRequest(InteractionRequest interactionRequest)
    {
        
    }
}