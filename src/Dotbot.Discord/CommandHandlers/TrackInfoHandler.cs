using Discord;
using Dotbot.Discord.Extensions;
using Dotbot.Discord.Models;
using Dotbot.Discord.Services;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers;

public class TrackInfoHandler : BotCommandHandler
{
    private readonly IAudioService _audioService;

    public TrackInfoHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public override CommandType CommandType => CommandType.TrackInfo;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        if (context is not IDiscordChannelMessageContext discordContext) return Fail("Not in discord context");

        var trackInfo = await _audioService.GetTrackInfo(discordContext.GetGuild().Id);

        await context.SendFormattedMessageAsync( trackInfo != null ? FormattedMessageExtensions.Youtube(trackInfo) : FormattedMessage.Info("Nothing playing"));
        
        return Ok();

    }
}