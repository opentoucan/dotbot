using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Discord;
using MediatR;

namespace Discord.Application.BotCommands;

public class InfoCommand : BotCommand, IRequest<bool>
{
    public InfoCommand(Privilege currentUserPrivilege, IServiceContext serviceContext, string content)
    {
        CurrentUserPrivilege = currentUserPrivilege;
        ServiceContext = serviceContext;
        Content = content;
    }

    public override Privilege PrivilegeLevel => Privilege.Base;
    public override Privilege CurrentUserPrivilege { get; }
    public override IServiceContext ServiceContext { get; }
    public string Content { get; }
}