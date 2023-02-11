using Dotbot.Common.CommandHandlers;
using Dotbot.Common.Models;
using Dotbot.Discord.Services;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers;

public class PlayMusicHandler : BotCommandHandler
{
    private readonly IAudioService _audioService;

    public PlayMusicHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public override CommandType CommandType => CommandType.Play;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        if (context is not IDiscordChannelMessageContext discordContext) return Fail("Not in discord context");
        
        var split = content.Split(' ');
        var guild = discordContext.GetGuild();
        var userVoiceState = discordContext.GetUserVoiceState();

        await _audioService.EnqueueAudioThread(guild, userVoiceState.VoiceChannel, discordContext.GetChannel(),
            split[1]);
            
        await context.SendFormattedMessageAsync(FormattedMessage.Info($"Playing {split[1]}"));
        return Ok();

    }
}