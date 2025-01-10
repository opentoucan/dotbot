using System.Text.Json;
using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.Repositories;
using MassTransit.Internals;
using NetCord.Rest;

namespace Dotbot.Gateway.Services;

public interface IRegistrationService
{
    Task Register();
}

public class RegistrationService : IRegistrationService
{
    private readonly RestClient _restClient;
    private readonly IGuildRepository _guildRepository;

    public RegistrationService(RestClient restClient, IGuildRepository guildRepository)
    {
        _restClient = restClient;
        _guildRepository = guildRepository;
    }

    public async Task Register()
    {
        var restGuilds = await _restClient.GetCurrentUserGuildsAsync().ToListAsync();
        foreach (var registeredGuild in restGuilds)
        {
            var guild = await _guildRepository.GetByExternalIdAsync(registeredGuild.Id.ToString());
            if (guild is null)
                _guildRepository.Add(new Guild(registeredGuild.Id.ToString(), registeredGuild.Name));
            else
            {
                guild.SetName(registeredGuild.Name);
                _guildRepository.Update(guild);
            }
        }
        await _guildRepository.UnitOfWork.SaveChangesAsync(CancellationToken.None);
    }
}