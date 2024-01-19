using Bot.Gateway.Infrastructure.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Search;

namespace Bot.Gateway.Infrastructure.Repositories;

public class BotCommandRepository : IBotCommandRepository
{
    private readonly DbContext _dbContext;

    public BotCommandRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public BotCommand? GetCommand(string serverId, string key)
    {
        var cmd = _dbContext.BotCommands.AsQueryable().FirstOrDefault(x => x.ServerId == serverId && x.Name == key);

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

    public async Task<List<BotCommand>> GetCommands(string serverId, int index, int pageSize)
    {
        if (pageSize < 0)
            return [];

        var commands = await _dbContext.BotCommands
            .Find(OnServerId(serverId))
            .SortBy(x => x.Name)
            .Skip(index * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        return commands;
    }

    public async Task<long> GetCommandCount(string serverId)
    {
        return await _dbContext.BotCommands.CountDocumentsAsync(Builders<BotCommand>.Filter.Where(x => x.ServerId == serverId));
    }

    public async Task<List<string>> GetAllNames(string serverId)
    {
        return await _dbContext.BotCommands.AsQueryable().Where(x => x.ServerId == serverId).Select(x => x.Name).ToListAsync();
    }

    public List<BotCommand> SearchByNameAndServer(string serverId, string? name, int pageSize)
    {
        if (string.IsNullOrWhiteSpace(name))
            return _dbContext.BotCommands.AsQueryable().Where(x => x.ServerId == serverId).ToList();
        return _dbContext.BotCommands.Aggregate().Search(Builders<BotCommand>.Search.Autocomplete(
            x => x.Name, name, fuzzy: new SearchFuzzyOptions()
            {
                MaxEdits = 1,
                PrefixLength = 1,
                MaxExpansions = 256
            }), indexName: "default")
            .Project<BotCommand>(Builders<BotCommand>.Projection.Include(botCommand => botCommand.Name))
            .Limit(Math.Min(pageSize, 10))
            .ToList();
    }

    private static FilterDefinition<BotCommand> OnServerId(string serverId) =>
        Builders<BotCommand>.Filter.Where(x => x.ServerId == serverId);
}