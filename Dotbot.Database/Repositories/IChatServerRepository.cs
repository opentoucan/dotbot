using Dotbot.Database.Entities;
using FluentResults;

namespace Dotbot.Database.Repositories;

public interface IChatServerRepository : IRepository<ChatServer>
{
    Task<Result<ChatServer>> GetServerAsync(string serverId);
    Task<Result<ChatServer>> CreateServerAsync(string serverId);
    Task AddModId(string serverId, string modId);
    Task RemoveModId(string serverId, string modId);
    Task SetXkcdChannelId(string serverId, string channelId);
    Task UnSetXkcdChannelId(string serverId);
    Task<Result<List<ChatServer>>> GetAll();
}