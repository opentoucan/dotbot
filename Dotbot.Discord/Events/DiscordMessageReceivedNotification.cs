using Discord.WebSocket;
using MediatR;

namespace Dotbot.Discord.Events;

public class DiscordMessageReceivedNotification : INotification
{
    public DiscordMessageReceivedNotification(SocketMessage message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public SocketMessage Message { get; }
}