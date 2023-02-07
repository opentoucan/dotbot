using Dotbot.Common.CommandHandlers;
using Dotbot.Discord.Services;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers;

public class PlayMusicHandler: BotCommandHandler
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
        if (context is IDiscordChannelMessageContext discordContext)
        {
            var split = content.Split(' ');
            var guild = discordContext.GetGuild();
            var userVoiceState = discordContext.GetUserVoiceState();

            await _audioService.JoinAudio(guild, userVoiceState.VoiceChannel);
            await _audioService.EnqueueAudio(guild, discordContext.GetChannel() , split[1]);
            
            return Ok();
        }
        else
        {
            return Fail("Not in discord context");
        }
    }
}