using System.Diagnostics;
using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.Repositories;
using NetCord.Rest;
using ServiceDefaults;

namespace Dotbot.Api.Discord.HostedServices;

public class SaveDiscordServersHostedService(IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var guildRepository = scope.ServiceProvider.GetRequiredService<IGuildRepository>();
        var restClient = scope.ServiceProvider.GetRequiredService<RestClient>();
        var instrumentation = scope.ServiceProvider.GetRequiredService<Instrumentation>();
        var restGuilds = restClient.GetCurrentUserGuildsAsync();
        var tagList = new TagList();

        await foreach (var registeredGuild in restGuilds)
        {
            tagList.Clear();
            tagList.Add("guild_id", registeredGuild.Id.ToString());
            tagList.Add("guild_name", registeredGuild.Name);

            var guild = await guildRepository.GetByExternalIdAsync(registeredGuild.Id.ToString());
            if (guild is null)
                guildRepository.Add(new Guild(registeredGuild.Id.ToString(), registeredGuild.Name));
            else
            {
                guild.SetName(registeredGuild.Name);
                guildRepository.Update(guild);
            }

            instrumentation.GuildsCounter.Add(1, tagList);
        }
        await guildRepository.UnitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}