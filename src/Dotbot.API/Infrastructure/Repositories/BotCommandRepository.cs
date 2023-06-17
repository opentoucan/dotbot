using Dotbot.Database.Entities;
using FluentResults;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using static FluentResults.Result;

namespace Dotbot.Infrastructure.Repositories;

public class BotCommandRepository
{
    public IMongoCollection<BotCommand> BotCommands { get; }

    public async Task<Result<BotCommand>> GetCommand(string serverId, string key)
    {
        var cmd = BotCommands.AsQueryable().FirstOrDefault(x => x.ServiceId == serverId && x.Key == key);

        return cmd == null ? Fail("Command not found") : Ok(cmd);
    }

    public async Task<Result> SaveCommand(string serverId, string creatorId, string key, string content, bool overwrite = false)
    {
        var command = await GetCommand(serverId, key);
        if (!overwrite && command.IsSuccess)
        {
            return Fail("Command already exists");
        }

        if (!command.IsSuccess)
        {
            await BotCommands.InsertOneAsync(new BotCommand
                { Content = content, Key = key, ServiceId = serverId, Type = BotCommand.CommandType.STRING });
        }
        else
        {
            await BotCommands.FindOneAndReplaceAsync<BotCommand>(Builders<BotCommand>.Filter
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
            await BotCommands.InsertOneAsync(new BotCommand
                { FileName = fileName, Key = key, ServiceId = serverId, Type = BotCommand.CommandType.FILE });
        }
        else
        {
            await BotCommands.FindOneAndReplaceAsync(
                Builders<BotCommand>.Filter.Eq(x => x.Id, command.Value.Id),
                new BotCommand
                {
                    Id = command.Value.Id, FileName = fileName, Key = key, ServiceId = serverId,
                    Type = BotCommand.CommandType.FILE, CreatorId = creatorId
                });
        }

        return Ok();
    }

    public async Task<Result<List<BotCommand>>> GetCommands(string serverId, int page, int pageSize)
    {
        if (pageSize < 0)
            return Fail($"Invalid {nameof(pageSize)} {pageSize}");

        var commands = await BotCommands
            .Find(OnServerId(serverId))
            .SortBy(x => x.Key)
            .Skip(page * pageSize)
            .Limit(pageSize)
            .ToListAsync();
        return Ok(commands);
    }

    public async Task<Result<long>> GetCommandCount(string serverId)
    {
        var count = await BotCommands.CountDocumentsAsync(Builders<BotCommand>.Filter.Where(x => x.ServiceId == serverId));
        return Ok(count);
    }

    public async Task<Result<List<string>>> GetAllNames(string serverId)
    {
        return Ok(await BotCommands.AsQueryable().Where(x => x.ServiceId == serverId).Select(x => x.Key).ToListAsync());
    }
    
    private static FilterDefinition<BotCommand> OnServerId(string serverId) =>
        Builders<BotCommand>.Filter.Where(x => x.ServiceId == serverId);
}