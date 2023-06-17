using Dotbot.Discord.Models;
using Dotbot.Discord.Services;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers;

public class SkipMusicHandler : BotCommandHandler
{
    private readonly IAudioService _audioService;

    public SkipMusicHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public override CommandType CommandType => CommandType.Skip;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        if (context is not IDiscordChannelMessageContext discordContext) return Fail("Not in discord context");
        
        var guild = discordContext.GetGuild();
        //TODO: Check user is in audio channel
        await _audioService.Skip(guild, discordContext.GetChannel());
        await context.SendFormattedMessageAsync(FormattedMessage.Info("Skipping track"));
        return Ok();

    }
}