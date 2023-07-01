using Discord.Application.BotCommandHandlers;
using Discord.Application.BotCommands;
using Discord.Application.Models;
using Discord.Discord;
using BotCommand = Discord.Application.BotCommands.BotCommand;
using SaveBotCommand = Discord.Application.BotCommands.SaveBotCommand;

namespace Discord.Application.Factories;

public class BotCommandCreatorFactory : IBotCommandCreatorFactory
{
    public BotCommand Create(BotCommandTypes commandType, Privilege userPrivilege, IServiceContext serviceContext, string content)
    {
        BotCommand botCommand = commandType switch
        {
            BotCommandTypes.Default => new DefaultBotCommand(userPrivilege, serviceContext, content),
            BotCommandTypes.Save => new SaveBotCommand(userPrivilege, serviceContext, content),
            BotCommandTypes.Ping => new PingBotCommand(userPrivilege, serviceContext),
            BotCommandTypes.Saved => new SavedCommand(userPrivilege, serviceContext, content),
            BotCommandTypes.Avatar => new AvatarCommand(userPrivilege, serviceContext),
            BotCommandTypes.Xkcd => new XkcdBotCommand(userPrivilege, serviceContext, content),
            BotCommandTypes.SetXkcdChannel => new SetXkcdChannelCommand(userPrivilege, serviceContext, content),
            BotCommandTypes.Info => new InfoCommand(userPrivilege, serviceContext, content),
            BotCommandTypes.Search => new SearchCommand(userPrivilege, serviceContext, content),
            BotCommandTypes.Play => new PlayMusicCommand(userPrivilege, serviceContext, content),
            BotCommandTypes.Skip => new SkipMusicCommand(userPrivilege, serviceContext),
            BotCommandTypes.TrackInfo => new TrackInfoCommand(userPrivilege, serviceContext),
            _ => throw new ArgumentOutOfRangeException(nameof(commandType), commandType, null)
        };

        return botCommand;
    }
}