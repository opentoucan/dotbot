using Dotbot.Common.Services;
using FluentResults;
using static Dotbot.Common.Models.FormattedMessage;
using static FluentResults.Result;

namespace Dotbot.Common.CommandHandlers.Moderator;

public class RemoveModeratorCommandHandler : BotCommandHandler
{
    private readonly IChatServerService _chatServerService;

    public RemoveModeratorCommandHandler(IChatServerService chatServerService)
    {
        _chatServerService = chatServerService;
    }

    public override CommandType CommandType => CommandType.RemoveModerator;
    public override Privilege PrivilegeLevel => Privilege.Moderator;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var mentions = await context.GetUserMentionsAsync();

        if (mentions.Count == 0)
        {
            await context.SendFormattedMessageAsync(Error("No users provided"));
            return Fail("No users provided");
        }

        var serverId = await context.GetServerId();

        foreach (var mention in mentions)
        {
            var result = await _chatServerService.RemoveModerator(serverId, mention.Id.ToString());
            if (result.IsFailed)
            {
                await context.SendFormattedMessageAsync(Error(result.Errors));
                return Fail(result.Reasons.ToString());
            }
        }

        await context.SendFormattedMessageAsync(Success("Removed moderators"));

        return Ok();
    }
}