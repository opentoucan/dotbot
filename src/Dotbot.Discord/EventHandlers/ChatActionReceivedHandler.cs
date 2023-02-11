using Discord;
using Dotbot.Discord.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dotbot.Discord.EventHandlers;

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