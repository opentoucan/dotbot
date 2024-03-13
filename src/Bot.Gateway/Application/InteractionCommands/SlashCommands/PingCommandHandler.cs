using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using Bot.Gateway.Infrastructure.HttpClient;
using MediatR;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

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