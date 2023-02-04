using Dotbot.Common.CommandHandlers;

namespace Dotbot.Common.Factories;

public interface IBotCommandHandlerFactory
{
    BotCommandHandler GetCommand(string str, Privilege privilege = Privilege.Base);
}