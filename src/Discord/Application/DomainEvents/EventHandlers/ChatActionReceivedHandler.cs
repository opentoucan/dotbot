using Discord.Application.DomainEvents.Events;
using MediatR;

namespace Discord.Application.DomainEvents.EventHandlers;

public class ChatActionReceivedHandler : INotificationHandler<DiscordMessageReceivedNotification>
{
    private readonly ILogger _logger;

    public ChatActionReceivedHandler(ILogger<ChatActionReceivedHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(DiscordMessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        if (notification.Message.Content.ToLower() == "lol")
        {
            await notification.Message.AddReactionAsync(new Emoji("🤣"));
        }
    }
}