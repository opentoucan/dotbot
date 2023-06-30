using Discord.Application.Models;
using FluentResults;
using static FluentResults.Result;

namespace Discord.BotCommandHandlers;

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
        await context.SendFormattedMessageAsync(FormattedMessage
            .Info()
            .SetTitle(user.Username)
            .SetImage(user.EffectiveAvatarUrl));
    }
}