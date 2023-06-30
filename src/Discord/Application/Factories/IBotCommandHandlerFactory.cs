using Discord.BotCommandHandlers;

namespace Discord.Factories;

public interface IBotCommandHandlerFactory
{
    BotCommandHandler GetCommand(string str, Privilege privilege = Privilege.Base);
}