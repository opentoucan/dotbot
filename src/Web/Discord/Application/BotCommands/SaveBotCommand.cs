using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Discord;
using MediatR;

namespace Discord.Application.BotCommands;

public class SaveBotCommand: BotCommand, IRequest<bool>
{
    public SaveBotCommand(Privilege currentUserPrivilege, IServiceContext serviceContext, string content)
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