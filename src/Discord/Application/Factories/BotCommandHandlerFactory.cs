using Discord.BotCommandHandlers;

namespace Discord.Factories;

public class BotCommandHandlerFactory : IBotCommandHandlerFactory
{
    private readonly IList<BotCommandHandler> _handlers;

    public BotCommandHandlerFactory(IEnumerable<BotCommandHandler> handlers)
    {
        _handlers = handlers.ToList();
    }

    public BotCommandHandler GetCommand(string str, Privilege privilege = Privilege.Base)
    {
        var commandType = CommandType.FromDisplayName(str);
        return _handlers.First(x => commandType.Equals(x.CommandType) && x.PrivilegeLevel == privilege);
    }
}