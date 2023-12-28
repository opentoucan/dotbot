using Bot.Gateway.Model.Responses.Discord;
using MediatR;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class PingCommandHandler : IRequestHandler<PingCommand, InteractionData>
{
    public Task<InteractionData> Handle(PingCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new InteractionData("pong"));
    }
}