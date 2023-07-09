using Discord.Application.BotCommands;
using Discord.Exceptions;
using MediatR;

namespace Discord.Application.Behaviours;

public class BotCommandBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>  where TRequest : BotCommand
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.CurrentUserPrivilege < request.PrivilegeLevel)
            throw new BotCommandPermissionException($"User has insufficient permission level to access this command");
        return await next();
    }
}