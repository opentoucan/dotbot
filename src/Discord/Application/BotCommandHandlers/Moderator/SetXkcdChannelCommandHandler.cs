using Discord.Application.Repositories;
using FluentResults;
using static Discord.Application.Models.FormattedMessage;
using static FluentResults.Result;

namespace Discord.BotCommandHandlers.Moderator;

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