using System.Linq.Expressions;
using Dotbot.Database.Entities;
using FluentResults;
using MongoDB.Driver;
using static FluentResults.Result;

namespace Dotbot.Database.Repositories;

public class ChatServerRepository : IChatServerRepository
{
    private readonly DbContext _dbContext;

    public ChatServerRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ChatServer>> GetServerAsync(string serverId)
    {
        var server = _dbContext.ChatServers.AsQueryable().FirstOrDefault(x => x.ServiceId == serverId);

        return server == null ? Fail("No such server") : Ok(server);
    }

    public async Task<Result<ChatServer>> CreateServerAsync(string serverId)
    {
        if ((await GetServerAsync(serverId)).IsSuccess)
        {
            return Fail("Server already exists");
        }

        await _dbContext.ChatServers.InsertOneAsync(new ChatServer(serverId));
        return await GetServerAsync(serverId);
    }

    public async Task AddModId(string serverId, string modId)
    {
        var filter = Builders<ChatServer>.Filter.Where(x => x.ServiceId == serverId);
        var update = Builders<ChatServer>.Update.Push(u => u.ModeratorIds, modId);
        await _dbContext.ChatServers.UpdateOneAsync(filter, update);
    }

    public async Task RemoveModId(string serverId, string modId)
    {
        var filter = Builders<ChatServer>.Filter.Where(x => x.ServiceId == serverId);
        var update = Builders<ChatServer>.Update.Pull(u => u.ModeratorIds, modId);
        await _dbContext.ChatServers.UpdateOneAsync(filter, update);
    }

    public async Task SetXkcdChannelId(string serverId, string channelId)
    {
        var filter = Builders<ChatServer>.Filter.Where(x => x.ServiceId == serverId);
        var update = Builders<ChatServer>.Update.Set(u => u.XkcdChannelId, channelId);
        await _dbContext.ChatServers.UpdateOneAsync(filter, update);
    }

    public async Task UnSetXkcdChannelId(string serverId)
    {
        var filter = Builders<ChatServer>.Filter.Where(x => x.ServiceId == serverId);
        var update = Builders<ChatServer>.Update.Unset(u => u.XkcdChannelId);
        await _dbContext.ChatServers.UpdateOneAsync(filter, update);
    }
    public async Task PushToCollection(string serverId, Expression<Func<ChatServer, IEnumerable<string>>> func,
        string value) =>
        await _dbContext.ChatServers.UpdateOneAsync(OnServerId(serverId), Push(func, value));

    public async Task PullFromCollection(string serverId, Expression<Func<ChatServer, IEnumerable<string>>> func,
        string value)
        => await _dbContext.ChatServers.UpdateOneAsync(OnServerId(serverId), Pull(func, value));

    public async Task SetField<TField>(string serverId, Expression<Func<ChatServer, TField>> func, TField value)
        => await _dbContext.ChatServers.UpdateOneAsync(OnServerId(serverId), Set(func, value));

    public async Task UnSetField(string serverId, Expression<Func<ChatServer, object>> func)
        => await _dbContext.ChatServers.UpdateOneAsync(OnServerId(serverId), UnSet(func));

    public async Task<Result<List<ChatServer>>> GetAll() => Ok(_dbContext.ChatServers.AsQueryable().ToList());

    private static FilterDefinition<ChatServer> OnServerId(string serverId) =>
        Builders<ChatServer>.Filter.Where(x => x.ServiceId == serverId);
    
    private static UpdateDefinition<ChatServer> Push(Expression<Func<ChatServer, IEnumerable<string>>> func,
        string value) => Builders<ChatServer>.Update.Push(func, value);

    private static UpdateDefinition<ChatServer> Pull(Expression<Func<ChatServer, IEnumerable<string>>> func,
        string value) => Builders<ChatServer>.Update.Pull(func, value);

    private static UpdateDefinition<ChatServer> Set<TField>(Expression<Func<ChatServer, TField>> func,
        TField value) => Builders<ChatServer>.Update.Set(func, value);

    private static UpdateDefinition<ChatServer> UnSet(Expression<Func<ChatServer, object>> func) =>
        Builders<ChatServer>.Update.Unset(func);
}