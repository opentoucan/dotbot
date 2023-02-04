using Dotbot.Database.Entities;
using FluentResults;

namespace Dotbot.Database.Repositories;

public interface IBotCommandRepository : IRepository<BotCommand>
{
    Task<Result<BotCommand>> GetCommand(string serverId, string key);
    Task<Result> SaveCommand(string serverId, string key, string content, bool overwrite = false);
    Task<Result> SaveCommand(string serverId, string key, string fileName, Stream fileStream, bool overwrite = false);
    Task<Result<List<BotCommand>>> GetCommands(int page, int pageSize);
    Task<Result<long>> GetCommandCount();
}