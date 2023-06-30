using Discord.WebSocket;

namespace Discord.Extensions;

public static class ChannelExtensions
{
    public static SocketGuildChannel? AsGuildChannel(this ISocketMessageChannel channel)
    {
        if (channel is SocketGuildChannel socketGuildChannel)
        {
            return socketGuildChannel;
        }
        return null;
    }
}