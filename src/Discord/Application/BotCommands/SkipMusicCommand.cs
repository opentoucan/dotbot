using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Discord;
using MediatR;

namespace Discord.Application.BotCommands;

public class SkipMusicCommand : BotCommand, IRequest<bool>
{
    public SkipMusicCommand(Privilege currentUserPrivilege, IServiceContext serviceContext)
    {
        CurrentUserPrivilege = currentUserPrivilege;
        ServiceContext = serviceContext;
    }

    public override Privilege PrivilegeLevel => Privilege.Base;
    public override Privilege CurrentUserPrivilege { get; }
    public override IServiceContext ServiceContext { get; }
}