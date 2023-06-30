using FluentResults;

namespace Discord.BotCommandHandlers;

public abstract class BotCommandHandler
{
    public abstract CommandType CommandType { get; }
    public abstract Privilege PrivilegeLevel { get; }

    public async Task<Result> HandleAsync(string content, IServiceContext context)
    {
        if (PrivilegeLevel <= context.GetPrivilege())
        {
            return await ExecuteAsync(content, context);
        }
        return Result.Fail("Handler cannot execute");
    }

    protected abstract Task<Result> ExecuteAsync(string content, IServiceContext context);
}