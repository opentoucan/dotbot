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
    private readonly IChatServerRepository _chatServerRepository;
    public XkcdPostedEventHandler(DiscordSocketClient discordSocketClient, IChatServerRepository chatServerRepository)
    {
        _discordSocketClient = discordSocketClient;
        _chatServerRepository = chatServerRepository;
    }

    public async Task Consume(ConsumeContext<XkcdPostedEvent> context)
    {

    }
}