using Discord.Application.BotCommandHandlers;
using Discord.Application.BotCommands;
using Discord.Application.Models;
using Discord.Discord;
using BotCommand = Discord.Application.BotCommands.BotCommand;

namespace Discord.Application.Factories;

public interface IBotCommandCreatorFactory
{
    BotCommand Create(BotCommandTypes commandType, Privilege userPrivilege, IServiceContext serviceContext, string content);
}