using Contracts.MessageBus;
using Discord.WebSocket;
using Dotbot.Database.Repositories;
using Dotbot.Discord.CommandHandlers;
using Dotbot.Discord.Extensions;
using Dotbot.Discord.Models;
using MassTransit;

namespace Dotbot.Discord.IntegrationEvents.EventHandlers;

public class XkcdPostedEventHandler : IConsumer<XkcdPostedEvent>
{

    private readonly DiscordSocketClient _discordSocketClient;
    public XkcdPostedEventHandler(DiscordSocketClient discordSocketClient)
    {
        _discordSocketClient = discordSocketClient;
    }

    public async Task Consume(ConsumeContext<XkcdPostedEvent> context)
    {
        var channel = _discordSocketClient.GetChannel(301062316647120896);
        await ((SocketTextChannel)channel).SendMessageAsync(embed: FormattedMessage.XkcdMessage(context.Message, true).Convert());
    }
}