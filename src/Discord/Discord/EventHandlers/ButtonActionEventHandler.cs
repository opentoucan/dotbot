using Discord.Application.DomainEvents.Events;
using Discord.WebSocket;
using MediatR;

namespace Discord.Discord.EventHandlers;

public class ButtonActionEventHandler
{
    private readonly IMediator _mediator;

    public ButtonActionEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task OnButtonActionAsync(SocketMessageComponent component)
    {
        await _mediator.Publish(new ButtonActionReceivedNotification(component));
    }
}