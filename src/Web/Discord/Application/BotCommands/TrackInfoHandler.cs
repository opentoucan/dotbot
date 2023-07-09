using Discord.Application.Models;
using Discord.Application.Services;
using Discord.Discord;
using Discord.Extensions;
using MediatR;

namespace Discord.Application.BotCommands;

public class TrackInfoHandler : IRequestHandler<TrackInfoCommand, bool>
{
    private readonly IAudioService _audioService;

    public TrackInfoHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async Task<bool> Handle(TrackInfoCommand request, CancellationToken cancellationToken)
    {
        if (request.ServiceContext is not IDiscordChannelMessageContext discordContext) return false;//return Fail("Not in discord context");

        var trackInfo = await _audioService.GetTrackInfo(discordContext.GetGuild().Id);

        await request.ServiceContext.SendFormattedMessageAsync( trackInfo != null ? FormattedMessageExtensions.Youtube(trackInfo) : FormattedMessage.Info("Nothing playing"));

        return true;
    }
}