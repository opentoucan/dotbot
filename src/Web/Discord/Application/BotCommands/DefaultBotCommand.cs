using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Discord;
using Discord.WebSocket;
using MediatR;

namespace Discord.Application.BotCommands;

public class DefaultBotCommand : BotCommand, IRequest<bool>
{
    public override Privilege PrivilegeLevel => Privilege.Base;
    public override Privilege CurrentUserPrivilege { get; }
    public override IServiceContext ServiceContext { get; }
    public string Content { get; }
    public SocketMessageComponent? SocketMessageComponent { get; }

    public DefaultBotCommand(Privilege currentUserPrivilege, IServiceContext serviceContext, string content, SocketMessageComponent? socketMessageComponent = null)
    {
        CurrentUserPrivilege = currentUserPrivilege;
        ServiceContext = serviceContext;
        Content = content;
        SocketMessageComponent = socketMessageComponent;
    }
}