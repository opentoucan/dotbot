using Discord.Application.IntegrationEvents.Events;
using Discord.Application.Models;
using Discord.Application.Repositories;
using Discord.Extensions;
using Discord.WebSocket;
using MassTransit;

namespace Discord.Application.IntegrationEvents.EventHandlers;

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