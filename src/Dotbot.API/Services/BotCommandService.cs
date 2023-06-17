using Dotbot.Database.Entities;
using Dotbot.Database.Repositories;
using Dotbot.Database.Services;
using FluentResults;
using FuzzySharp;
using static FluentResults.Result;

namespace Dotbot.Services;

public class BotCommandService : IBotCommandService
{
    private readonly IBotCommandRepository _botCommandRepository;
    private readonly IFileService _fileService;

    public BotCommandService(IBotCommandRepository botCommandRepository, IFileService fileService)
    {
        _botCommandRepository = botCommandRepository;
        _fileService = fileService;
    }

    public async Task<Result<dynamic>> FindBotCommand(string serviceId, string commandName)
    {
        var command = await _botCommandRepository.GetCommand(serviceId, commandName);

        if(command.IsFailed)
            return Fail($"No command {commandName} found");

        if (command.Value.Type != BotCommand.CommandType.FILE) 
            return Ok(command.Value.Content);
        
        var fileStream = await _fileService.GetFile($"{serviceId}:{command.Value.FileName}:{commandName}");
        
        return fileStream.IsSuccess ? Ok(fileStream.Value) : Fail($"Cannot find file content for {commandName}");
    }
    
    public async Task<Result<List<(string, int)>>> Search(string serverId, string searchTerm)
    {
        var allNames = await _botCommandRepository.GetAllNames(serverId);

        if (allNames.IsFailed || allNames.Value.Count == 0)
        {
            return Result.Fail("No matching commands found");
        }

        var matches = Process.ExtractTop(searchTerm, allNames.Value, cutoff: 50, limit: 20);
        
        return Result.Ok(matches.Select(x => (x.Value, x.Score)).ToList());
    }
}