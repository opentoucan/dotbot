using Dotbot.Discord.Services;
using FluentResults;
using static Dotbot.Discord.Models.FormattedMessage;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers.Moderator;

public class SetXkcdChannelCommandHandler: BotCommandHandler
{
    private readonly IChatServerService _chatServerService;
    public SetXkcdChannelCommandHandler(IChatServerService chatServerService)
    {
        _chatServerService = chatServerService;
    }

    public override CommandType CommandType => CommandType.SetXkcdChannel;
    public override Privilege PrivilegeLevel => Privilege.Moderator;
    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var channelId  = await context.GetChannelId();
        var serverId = await context.GetServerId();
        var result = await _chatServerService.SetXkcdChannel(serverId, channelId);

        if (result.IsSuccess)
        {
            await context.SendFormattedMessageAsync(Success("Channel set as XKCD channel"));
        }
        
        return OkIf(result.IsSuccess, string.Join(", ", result.Errors));
    }
}