using Dotbot.Models;
using FluentResults;

namespace Dotbot.Infrastructure.Repositories;

public interface IBotCommandRepository : IRepository<BotCommand>
{
    BotCommand? GetCommand(string serverId, string key);
    Task SaveCommand(BotCommand botCommand);
    Task<Result<List<BotCommand>>> GetCommands(string serverId, int page, int pageSize);
    Task<long> GetCommandCount(string serverId);
    Task<List<string>> GetAllNames(string serverId);
}