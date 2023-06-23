using Discord.WebSocket;
using Dotbot.Discord.CommandHandlers;
using Dotbot.Discord.Extensions;
using Dotbot.Discord.IntegrationEvents.Events;
using Dotbot.Discord.Models;
using Dotbot.Discord.Repositories;
using MassTransit;

namespace Dotbot.Discord.IntegrationEvents.EventHandlers;

public class XkcdPostedEventHandler : IConsumer<XkcdPostedEvent>
{

    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IDiscordServerRepository _discordServerRepository;
    public XkcdPostedEventHandler(DiscordSocketClient discordSocketClient, IDiscordServerRepository discordServerRepository)
    {
        _discordSocketClient = discordSocketClient;
        _discordServerRepository = discordServerRepository;
    }

    public async Task Consume(ConsumeContext<XkcdPostedEvent> context)
    {
        var discordServers = await _discordServerRepository.GetAll();
        foreach (var discordServer in discordServers.ValueOrDefault)
        {
            var channel = _discordSocketClient.GetChannel(ulong.Parse(discordServer.XkcdChannelId));
            var xkcdComic = new XkcdComic
            {
                AltText = context.Message.AltText,
                ComicNumber = context.Message.ComicNumber,
                DatePosted = context.Message.DatePosted,
                ImageUrl = context.Message.ImageUrl,
                Title = context.Message.Title
            };
        
            await ((SocketTextChannel)channel).SendMessageAsync(embed: FormattedMessage.XkcdMessage(xkcdComic, true).Convert());
        }
    }
}