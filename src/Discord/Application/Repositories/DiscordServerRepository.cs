using System.Linq.Expressions;
using Discord.Application.Entities;
using FluentResults;
using MongoDB.Driver;
using static FluentResults.Result;

namespace Discord.Application.Repositories;

public class DiscordServerRepository : IDiscordServerRepository
{
    private readonly DbContext _dbContext;

    public DiscordServerRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DiscordServer>> GetServerAsync(string serverId)
    {
        var server = _dbContext.DiscordServers.AsQueryable().FirstOrDefault(x => x.Server == serverId);

        return server == null ? Fail("No such server") : Ok(server);
    }

    public async Task<Result<DiscordServer>> CreateServerAsync(string serverId)
    {
        return await _dbContext.DiscordServers.FindOneAndReplaceAsync(
            Builders<DiscordServer>.Filter.Eq(x => x.Server, serverId),
            new DiscordServer(serverId),
            new FindOneAndReplaceOptions<DiscordServer> { IsUpsert = true });
    }

    public async Task AddModId(string serverId, string modId)
    {
        var filter = Builders<DiscordServer>.Filter.Where(x => x.Server == serverId);
        var update = Builders<DiscordServer>.Update.Push(u => u.ModeratorIds, modId);
        await _dbContext.DiscordServers.UpdateOneAsync(filter, update);
    }

    public async Task RemoveModId(string serverId, string modId)
    {
        var filter = Builders<DiscordServer>.Filter.Where(x => x.Server == serverId);
        var update = Builders<DiscordServer>.Update.Pull(u => u.ModeratorIds, modId);
        await _dbContext.DiscordServers.UpdateOneAsync(filter, update);
    }

    public async Task SetXkcdChannelId(string serverId, string channelId)
    {
        var filter = Builders<DiscordServer>.Filter.Where(x => x.Server == serverId);
        var update = Builders<DiscordServer>.Update.Set(u => u.XkcdChannelId, channelId);
        await _dbContext.DiscordServers.UpdateOneAsync(filter, update);
    }

    public async Task UnSetXkcdChannelId(string serverId)
    {
        var filter = Builders<DiscordServer>.Filter.Where(x => x.Server == serverId);
        var update = Builders<DiscordServer>.Update.Unset(u => u.XkcdChannelId);
        await _dbContext.DiscordServers.UpdateOneAsync(filter, update);
    }
    public async Task PushToCollection(string serverId, Expression<Func<DiscordServer, IEnumerable<string>>> func,
        string value) =>
        await _dbContext.DiscordServers.UpdateOneAsync(OnServerId(serverId), Push(func, value));

    public async Task PullFromCollection(string serverId, Expression<Func<DiscordServer, IEnumerable<string>>> func,
        string value)
        => await _dbContext.DiscordServers.UpdateOneAsync(OnServerId(serverId), Pull(func, value));

    public async Task SetField<TField>(string serverId, Expression<Func<DiscordServer, TField>> func, TField value)
        => await _dbContext.DiscordServers.UpdateOneAsync(OnServerId(serverId), Set(func, value));

    public async Task UnSetField(string serverId, Expression<Func<DiscordServer, object>> func)
        => await _dbContext.DiscordServers.UpdateOneAsync(OnServerId(serverId), UnSet(func));

    public async Task<Result<List<DiscordServer>>> GetAll() => Ok(_dbContext.DiscordServers.AsQueryable().ToList());

    private static FilterDefinition<DiscordServer> OnServerId(string serverId) =>
        Builders<DiscordServer>.Filter.Where(x => x.Server == serverId);
    
    private static UpdateDefinition<DiscordServer> Push(Expression<Func<DiscordServer, IEnumerable<string>>> func,
        string value) => Builders<DiscordServer>.Update.Push(func, value);

    private static UpdateDefinition<DiscordServer> Pull(Expression<Func<DiscordServer, IEnumerable<string>>> func,
        string value) => Builders<DiscordServer>.Update.Pull(func, value);

    private static UpdateDefinition<DiscordServer> Set<TField>(Expression<Func<DiscordServer, TField>> func,
        TField value) => Builders<DiscordServer>.Update.Set(func, value);

    private static UpdateDefinition<DiscordServer> UnSet(Expression<Func<DiscordServer, object>> func) =>
        Builders<DiscordServer>.Update.Unset(func);
}