using Discord.Discord.EventHandlers;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Extensions;

public static class ClientEventRegistrations
{
    public static DiscordSocketClient RegisterClientEvents(this IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<DiscordSocketClient>();
        var messageReceivedEventHandler = serviceProvider.GetRequiredService<MessageReceivedEventHandler>();
        var buttonActionEventHandler = serviceProvider.GetRequiredService<ButtonActionEventHandler>();
        
        client.MessageReceived += messageReceivedEventHandler.OnMessageReceivedAsync;
        client.ButtonExecuted += buttonActionEventHandler.OnButtonActionAsync;
        //Other client event registrations go here
        return client;
    }
}