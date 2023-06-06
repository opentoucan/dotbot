using Discord.WebSocket;
using Dotbot.Discord.EventListeners;
using Microsoft.Extensions.DependencyInjection;

namespace Dotbot.Discord.Extensions;

public static class ClientEventRegistrations
{
    public static DiscordSocketClient RegisterClientEvents(this IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        var messageReceivedEventListener = serviceProvider.GetRequiredService<MessageReceivedEventListener>();
        var reactionEventListener = serviceProvider.GetRequiredService<ReactionEventListener>();

        client.MessageReceived += messageReceivedEventListener.OnMessageReceivedAsync;
        client.ReactionAdded += reactionEventListener.OnReactionAdded;
        client.ReactionRemoved += reactionEventListener.OnReactionRemoved;
        //Other client event registrations go here
        return client;
    }
}