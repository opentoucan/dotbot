using Bot.Gateway.Model.Requests.Discord;
using Bot.Gateway.Model.Responses.Discord;
using MediatR;

namespace Bot.Gateway.Application.InteractionCommands;

public abstract class InteractionCommand : IRequest<InteractionData>
{
    public abstract BotCommandType CommandType { get; }
    public abstract InteractionRequest Data { get; set; }
}