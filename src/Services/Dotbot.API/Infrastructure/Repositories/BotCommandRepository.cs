using Dotbot.Models;
using FluentResults;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using static FluentResults.Result;

namespace Dotbot.Infrastructure.Repositories;

public class BotCommandRepository : IBotCommandRepository
{
    private readonly DbContext _dbContext;

    public BotCommandRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public BotCommand? GetCommand(string serverId, string key)
    {
        var cmd = _dbContext.BotCommands.AsQueryable().FirstOrDefault(x => x.ServiceId == serverId && x.Name == key);

        return cmd;
    }

    public async Task SaveCommand(BotCommand botCommand)
    {
        var existingBotCommand = _dbContext.BotCommands.AsQueryable().FirstOrDefault(x => x.Name == botCommand.Name);
        if (existingBotCommand == null)
        {
            await _dbContext.BotCommands.InsertOneAsync(botCommand);
        }
        else
        {
            botCommand.Id = existingBotCommand.Id;
            await _dbContext.BotCommands.ReplaceOneAsync(Builders<BotCommand>.Filter.Eq(x => x.Name, existingBotCommand.Name), botCommand);
        }

        
        await _dbContext.BotCommands.ReplaceOneAsync(Builders<BotCommand>.Filter
                .Eq(x => x.Name, botCommand.Name),
                botCommand,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task<FluentResults.Result<List<BotCommand>>> GetCommands(string serverId, int index, int pageSize)
    {
        if (pageSize < 0)
            return Fail($"Invalid {nameof(pageSize)} {pageSize}");

        var commands = await _dbContext.BotCommands
            .Find(OnServerId(serverId))
            .SortBy(x => x.Name)
            .Skip(index * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        return Ok(commands);
    }

    public async Task<long> GetCommandCount(string serverId)
    {
        return await _dbContext.BotCommands.CountDocumentsAsync(Builders<BotCommand>.Filter.Where(x => x.ServiceId == serverId));
    }

    public async Task<List<string>> GetAllNames(string serverId)
    {
        return await _dbContext.BotCommands.AsQueryable().Where(x => x.ServiceId == serverId).Select(x => x.Name).ToListAsync();
    }
    
    private static FilterDefinition<BotCommand> OnServerId(string serverId) =>
        Builders<BotCommand>.Filter.Where(x => x.ServiceId == serverId);
}