using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.Repositories;
using MassTransit.Internals;
using NetCord.Rest;

namespace Dotbot.Gateway.Services;

public class RegistrationHostedService(IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        var guildRepository = scope.ServiceProvider.GetRequiredService<IGuildRepository>();
        var restClient = scope.ServiceProvider.GetRequiredService<RestClient>();
        var restGuilds = await restClient.GetCurrentUserGuildsAsync().ToListAsync();
        foreach (var registeredGuild in restGuilds)
        {
            var guild = await guildRepository.GetByExternalIdAsync(registeredGuild.Id.ToString());
            if (guild is null)
                guildRepository.Add(new Guild(registeredGuild.Id.ToString(), registeredGuild.Name));
            else
            {
                guild.SetName(registeredGuild.Name);
                guildRepository.Update(guild);
            }
        }
        await guildRepository.UnitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}