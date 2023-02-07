using Discord;
using Discord.WebSocket;

namespace Dotbot.Discord.Services;

public interface IAudioService
{
    Task JoinAudio(IGuild guild, IVoiceChannel target);
    Task LeaveAudio(IGuild guild);
    Task SendAudioAsync(string guid, IGuild guild, IMessageChannel channel);
    Task Skip(SocketGuild contextGuild, ISocketMessageChannel contextChannel);
    Task EnqueueAudio(IGuild guild, IMessageChannel channel, string url);
}