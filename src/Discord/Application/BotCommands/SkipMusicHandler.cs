using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Application.Services;
using Discord.Discord;
using MediatR;

namespace Discord.Application.BotCommands;

public class SkipMusicHandler : IRequestHandler<SkipMusicCommand, bool>
{
    private readonly IAudioService _audioService;

    public SkipMusicHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public async Task<bool> Handle(SkipMusicCommand request, CancellationToken cancellationToken)
    {
        if (request.ServiceContext is not IDiscordChannelMessageContext discordContext) return false;// Fail("Not in discord context");
        
        var guild = discordContext.GetGuild();
        //TODO: Check user is in audio channel
        await _audioService.Skip(guild, discordContext.GetChannel());
        await request.ServiceContext.SendFormattedMessageAsync(FormattedMessage.Info("Skipping track"));
        return true;
    }
}