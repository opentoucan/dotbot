using FluentResults;

namespace Dotbot.Common.CommandHandlers;

public abstract class BotCommandHandler
{
    public abstract CommandType CommandType { get; }
    public abstract Privilege PrivilegeLevel { get; }

    public async Task<Result> HandleAsync(string content, IServiceContext context)
    {
        if (CanExecute() && PrivilegeLevel <= context.GetPrivilege())
        {
            return await ExecuteAsync(content, context);
        }
        return Result.Fail("Handler cannot execute");
    }

    protected abstract Task<Result> ExecuteAsync(string content, IServiceContext context);
    protected virtual bool CanExecute() => true;
}