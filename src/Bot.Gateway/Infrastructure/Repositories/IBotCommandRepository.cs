using Bot.Gateway.Infrastructure.Entities;

namespace Bot.Gateway.Infrastructure.Repositories;

public interface IBotCommandRepository : IRepository<BotCommand>
{
    BotCommand? GetCommand(string serverId, string key);
    Task SaveCommand(BotCommand botCommand);
    Task<List<BotCommand>> GetCommands(string serverId, int page, int pageSize);
    Task<long> GetCommandCount(string serverId);
    Task<List<string>> GetAllNames(string serverId);

    List<BotCommand> SearchByNameAndServer(string serverId, string? name, int pageSize);
}