using System.Linq.Expressions;
using Bot.Gateway.Infrastructure.Entities;

namespace Bot.Gateway.Infrastructure.Repositories;

public interface IDiscordServerRepository : IRepository<DiscordServer>
{
    DiscordServer? GetServer(string serverId);
    Task<DiscordServer> CreateServerAsync(string serverId);
    Task AddModId(string serverId, string modId);
    Task RemoveModId(string serverId, string modId);
    Task SetXkcdChannelId(string serverId, string channelId);
    Task UnSetXkcdChannelId(string serverId);
    List<DiscordServer> GetAll();
    Task PushToCollection(string serverId, Expression<Func<DiscordServer, IEnumerable<string>>> func, string value);
    Task PullFromCollection(string serverId, Expression<Func<DiscordServer, IEnumerable<string>>> func, string value);
    Task SetField<TField>(string serverId, Expression<Func<DiscordServer, TField>> func, TField value);
    Task UnSetField(string serverId, Expression<Func<DiscordServer, object>> func);
}