using Dotbot.Database.Entities;
using Dotbot.Database.Repositories;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Common.Services;

public class ChatServerService : IChatServerService
{
    private readonly IChatServerRepository _repository;

    public ChatServerService(IChatServerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ChatServer>> Get(string id)
    {
        return await _repository.GetServerAsync(id);
    }

    public async Task<Result<ChatServer>> Create(string id)
    {
        return await _repository.CreateServerAsync(id);
    }

    public async Task<Result> AddModerator(string serverId, string modId)
    {
        var server = await Get(serverId);
        if (server.IsFailed)
        {
            return Fail(server.Errors);
        }

        await _repository.AddModId(serverId, modId);
        return Ok();
    }

    public async Task<Result> RemoveModerator(string serverId, string modId)
    {
        var server = await Get(serverId);
        if (server.IsFailed)
        {
            return Fail(server.Errors);
        }

        await _repository.RemoveModId(serverId, modId);
        return Ok();
    }

    public async Task<Result<bool>> IsModerator(string serverId, string modId)
    {
        var server = await Get(serverId);
        if (server.IsFailed)
        {
            return Fail(server.Errors);
        }

        return Ok(server.Value.ModeratorIds.Contains(modId));
    }

    public async Task<Result> SetXkcdChannel(string serverId, string channelId)
    {
        var server = await Get(serverId);
        if (server.IsFailed)
        {
            return Fail(server.Errors);
        }

        await _repository.SetXkcdChannelId(serverId, channelId);
        return Ok();
    }

    public async Task<Result<IEnumerable<ChatServer>>> GetXkcdServers()
    {
        //TODO: Probs should write a DB query to do this but I'm too lazy right now and this only gets called every few hours
        var all = await _repository.GetAll();
        if (all.IsFailed)
        {
            return Fail(all.Errors);
        }

        return Ok(all.Value.Where(x => !string.IsNullOrEmpty(x.XkcdChannelId)));
    }
}