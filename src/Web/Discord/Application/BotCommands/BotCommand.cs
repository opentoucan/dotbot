using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Discord;

namespace Discord.Application.BotCommands;

public abstract class BotCommand
{
    public abstract Privilege PrivilegeLevel { get; }
    public abstract Privilege CurrentUserPrivilege { get; }

    public abstract IServiceContext ServiceContext { get; }
}