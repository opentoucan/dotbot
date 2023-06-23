﻿using Dotbot.Discord.Repositories;
using Dotbot.Discord.Services;
using FluentResults;
using static Dotbot.Discord.Models.FormattedMessage;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers.Moderator;

public class SetXkcdChannelCommandHandler: BotCommandHandler
{
    private readonly IDiscordServerRepository _discordServerRepository;

    public SetXkcdChannelCommandHandler(IDiscordServerRepository discordServerRepository)
    {
        _discordServerRepository = discordServerRepository;
    }

    public override CommandType CommandType => CommandType.SetXkcdChannel;
    public override Privilege PrivilegeLevel => Privilege.Moderator;
    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var channelId  = await context.GetChannelId();
        var serverId = await context.GetServerId();
        try
        {
            await _discordServerRepository.SetXkcdChannelId(serverId, channelId);
        }
        catch (Exception)
        {
            var errorMessage = $"Failed to set XKCD channel";
            await context.SendFormattedMessageAsync(Error(errorMessage));
            return Fail(errorMessage);
        }
        
        await context.SendFormattedMessageAsync(Success("Channel set as XKCD channel"));
        return Ok();
    }
}