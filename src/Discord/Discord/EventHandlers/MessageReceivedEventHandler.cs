using Discord.Application.DomainEvents.Events;
using Discord.WebSocket;
using MediatR;

namespace Discord.Discord.EventHandlers;

public class MessageReceivedEventHandler
{
    private readonly IMediator _mediator;

    public MessageReceivedEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnMessageReceivedAsync(SocketMessage arg)
    {
        if(arg.Author.IsBot) return;
        await _mediator.Publish(new DiscordMessageReceivedNotification(arg));
    }
}