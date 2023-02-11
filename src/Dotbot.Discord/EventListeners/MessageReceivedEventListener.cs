using Discord.WebSocket;
using Dotbot.Discord.Events;
using MediatR;

namespace Dotbot.Discord.EventListeners;

public class MessageReceivedEventListener
{
    private readonly IMediator _mediator;

    public MessageReceivedEventListener(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task OnMessageReceivedAsync(SocketMessage arg)
    {
        if(arg.Author.IsBot) return;
        await _mediator.Publish(new DiscordMessageReceivedNotification(arg));
    }
}