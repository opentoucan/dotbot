using Dotbot.Database.Repositories;
using FluentResults;
using FuzzySharp;

namespace Dotbot.Discord.Services;

public class BotCommandsService : IBotCommandsService
{
    private readonly IBotCommandRepository _repository;

    public BotCommandsService(IBotCommandRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<(string, int)>>> Search(string serverId, string searchTerm)
    {
        var allNames = await _repository.GetAllNames(serverId);

        if (allNames.IsFailed || allNames.Value.Count == 0)
        {
            return Result.Fail("No matching commands found");
        }

        var matches = Process.ExtractTop(searchTerm, allNames.Value, cutoff: 50, limit: 20);
        
        return Result.Ok(matches.Select(x => (x.Value, x.Score)).ToList());
    }
}