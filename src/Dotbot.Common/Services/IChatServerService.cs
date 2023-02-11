using Dotbot.Database.Entities;
using FluentResults;

namespace Dotbot.Common.Services;

public interface IChatServerService
{
    Task<Result<ChatServer>> Get(string id);
    Task<Result<ChatServer>> Create(string id);
    Task<Result> AddModerator(string serverId, string modId);
    Task<Result> RemoveModerator(string serverId, string modId);
    Task<Result<bool>> IsModerator(string serverId, string modId);
    Task<Result> SetXkcdChannel(string serverId, string modId);
    Task<Result<IEnumerable<ChatServer>>> GetXkcdServers();
}