using Discord.WebSocket;
using MediatR;

namespace Discord.Application.DomainEvents.Events;

public class ButtonActionReceivedNotification : INotification
{
    public SocketMessageComponent SocketMessageComponent;

    public ButtonActionReceivedNotification(SocketMessageComponent socketMessageComponent)
    {
        SocketMessageComponent = socketMessageComponent;
    }
}