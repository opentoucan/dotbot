using System.Linq.Expressions;
using Discord.Application.Entities;
using FluentResults;

namespace Discord.Application.Repositories;

public interface IDiscordServerRepository : IRepository<DiscordServer>
{
    Task<Result<DiscordServer>> GetServerAsync(string serverId);
    Task<Result<DiscordServer>> CreateServerAsync(string serverId);
    Task AddModId(string serverId, string modId);
    Task RemoveModId(string serverId, string modId);
    Task SetXkcdChannelId(string serverId, string channelId);
    Task UnSetXkcdChannelId(string serverId);
    Task<Result<List<DiscordServer>>> GetAll();
    Task PushToCollection(string serverId, Expression<Func<DiscordServer, IEnumerable<string>>> func, string value);
    Task PullFromCollection(string serverId, Expression<Func<DiscordServer, IEnumerable<string>>> func, string value);
    Task SetField<TField>(string serverId, Expression<Func<DiscordServer, TField>> func, TField value);
    Task UnSetField(string serverId, Expression<Func<DiscordServer, object>> func);
}