using FluentResults;

namespace Dotbot.Discord.Services;

public interface IBotCommandsService
{
    Task<Result<List<(string,int)>>> Search(string serverId, string searchTerm);
}