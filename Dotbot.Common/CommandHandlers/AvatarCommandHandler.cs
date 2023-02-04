using Dotbot.Common.Models;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Common.CommandHandlers;

public class AvatarCommandHandler : BotCommandHandler
{
    public override CommandType CommandType => CommandType.Avatar;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var mentionedIds = await context.GetUserMentionsAsync();

        foreach (var user in mentionedIds)
        {
            await SendAvatarEmbed(user, context);
        }

        return Ok();
    }

    private static async Task SendAvatarEmbed(User user, IServiceContext context)
    {
        await context.SendFormattedMessageAsync(new FormattedMessage
        {
            Title = user.Username, 
            ImageUrl = user.EffectiveAvatarUrl
        });
    }
}