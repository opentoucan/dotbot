using Dotbot.Database.Entities;
using FluentResults;
using MongoDB.Driver;
using static FluentResults.Result;

namespace Dotbot.Database.Repositories;

public class BotCommandRepository : IBotCommandRepository
{
    private readonly DbContext _dbContext;

    public BotCommandRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<BotCommand>> GetCommand(string serverId, string key)
    {
        var cmd = _dbContext.BotCommands.AsQueryable().FirstOrDefault(x => x.ServiceId == serverId && x.Key == key);

        return cmd == null ? Fail("Command not found") : Ok(cmd);
    }

    public async Task<Result> SaveCommand(string serverId, string creatorId, string key, string content,
        bool overwrite = false)
    {
        var command = await GetCommand(serverId, key);
        if (!overwrite && command.IsSuccess)
        {
            return Fail("Command already exists");
        }

        if (!command.IsSuccess)
        {
            await _dbContext.BotCommands.InsertOneAsync(new BotCommand
                { Content = content, Key = key, ServiceId = serverId, Type = BotCommand.CommandType.STRING });
        }
        else
        {
            await _dbContext.BotCommands.FindOneAndReplaceAsync<BotCommand>(Builders<BotCommand>.Filter
                    .Eq(x => x.Id, command.Value.Id),
                new BotCommand
                    { Content = content, Key = key, ServiceId = serverId, Type = BotCommand.CommandType.STRING, CreatorId = creatorId});
        }

        return Ok();
    }

    public async Task<Result> SaveCommand(string serverId, string creatorId, string key, string fileName,
        Stream fileStream, bool overwrite = false)
    {
        var command = await GetCommand(serverId, key);
        if (!overwrite && command.IsSuccess)
        {
            return Fail("Command already exists");
        }

        if (!command.IsSuccess)
        {
            await _dbContext.BotCommands.InsertOneAsync(new BotCommand
                { FileName = fileName, Key = key, ServiceId = serverId, Type = BotCommand.CommandType.FILE });
        }
        else
        {
            await _dbContext.BotCommands.FindOneAndReplaceAsync(
                Builders<BotCommand>.Filter.Eq(x => x.Id, command.Value.Id),
                new BotCommand
                {
                    Id = command.Value.Id, FileName = fileName, Key = key, ServiceId = serverId,
                    Type = BotCommand.CommandType.FILE, CreatorId = creatorId
                });
        }

        return Ok();
    }

    public async Task<Result<List<BotCommand>>> GetCommands(int page, int pageSize)
    {
        if (pageSize < 0)
            return Fail($"Invalid {nameof(pageSize)} {pageSize}");

        var commands = await _dbContext.BotCommands
            .Find(Builders<BotCommand>.Filter.Empty)
            .SortBy(x => x.Key)
            .Skip(page * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        return Ok(commands);
    }

    public async Task<Result<long>> GetCommandCount()
    {
        var count = await _dbContext.BotCommands.CountDocumentsAsync(FilterDefinition<BotCommand>.Empty);
        return Ok(count);
    }
}