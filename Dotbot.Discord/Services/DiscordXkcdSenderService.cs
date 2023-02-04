using Discord.WebSocket;
using Dotbot.Common.Models;
using Dotbot.Common.Services;
using Dotbot.Discord.Extensions;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.Services;

public class DiscordXkcdSenderService: IXkcdSenderService
{
    private readonly DiscordSocketClient _discordClient;
    private readonly IChatServerService _chatServerService;

    public DiscordXkcdSenderService(DiscordSocketClient discordClient, IChatServerService chatServerService)
    {
        _discordClient = discordClient;
        _chatServerService = chatServerService;
    }

    public async Task<Result> SendNewComic(XkcdComic comic)
    {
        var servers = await _chatServerService.GetXkcdServers();

        if (servers.IsFailed || !servers.Value.Any())
        {
            return FailIf(servers.IsFailed, "Failed to get servers");
        }

        foreach (var server in servers.Value)
        {
            var socketGuild = _discordClient.GetGuild(ulong.Parse(server.ServiceId));
            var xkcdChannel = socketGuild?.GetChannel(ulong.Parse(server.XkcdChannelId));
            if (xkcdChannel is ISocketMessageChannel msgChannel)
            {
                await msgChannel.SendMessageAsync(embed: FormattedMessage.XkcdMessage(comic, true).Convert());
            }
        }
        
        return Ok();
    }
}