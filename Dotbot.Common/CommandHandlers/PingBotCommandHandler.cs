using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Common.CommandHandlers;

public class PingBotCommandHandler: BotCommandHandler
{
    public override CommandType CommandType => CommandType.Ping;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        await context.ReplyAsync("pong");
        return Ok();
    }
}