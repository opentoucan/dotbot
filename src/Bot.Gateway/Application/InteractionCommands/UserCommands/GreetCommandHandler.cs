using Bot.Gateway.Model.Responses.Discord;
using MediatR;

namespace Bot.Gateway.Application.InteractionCommands.UserCommands;

public class GreetCommandHandler : IRequestHandler<GreetCommand, InteractionData>
{
    public Task<InteractionData> Handle(GreetCommand request, CancellationToken cancellationToken)
    {
        var interactionData = new InteractionData
        {
            Content = "Hello"
        };
        
        return Task.FromResult(interactionData);
    }
}