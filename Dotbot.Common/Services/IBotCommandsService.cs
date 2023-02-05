using FluentResults;

namespace Dotbot.Common.Services;

public interface IBotCommandsService
{
    Task<Result<List<(string,int)>>> Search(string serverId, string searchTerm);
}