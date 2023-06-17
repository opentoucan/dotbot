using Dotbot.Discord.CommandHandlers;

namespace Dotbot.Discord.Factories;

public interface IBotCommandHandlerFactory
{
    BotCommandHandler GetCommand(string str, Privilege privilege = Privilege.Base);
}