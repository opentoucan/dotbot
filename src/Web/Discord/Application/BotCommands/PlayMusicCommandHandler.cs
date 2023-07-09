using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Application.Services;
using Discord.Discord;
using MediatR;

namespace Discord.Application.BotCommands;

public class PlayMusicCommandHandler : IRequestHandler<PlayMusicCommand, bool>
{
    private readonly IAudioService _audioService;

    public PlayMusicCommandHandler(IAudioService audioService)
    {
        _audioService = audioService;
    }
    
    public async Task<bool> Handle(PlayMusicCommand request, CancellationToken cancellationToken)
    {
        if (request.ServiceContext is not IDiscordChannelMessageContext discordContext) return false;
        
        var split = request.Content.Split(' ');
        var guild = discordContext.GetGuild();
        var userVoiceState = discordContext.GetUserVoiceState();

        await _audioService.EnqueueAudioThread(guild, userVoiceState.VoiceChannel, discordContext.GetChannel(),
            split[1]);
            
        await request.ServiceContext.SendFormattedMessageAsync(FormattedMessage.Info($"Adding {split[1]} to queue"));
        return true;
    }
}