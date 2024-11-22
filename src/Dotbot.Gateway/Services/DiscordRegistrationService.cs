using Discord;
using Discord.Rest;
using Dotbot.Gateway.Extensions;
using Dotbot.Gateway.Settings;
using Dotbot.Infrastructure.Entities;
using Dotbot.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.Services;

public class DiscordRegistrationService : BackgroundService
{
    private readonly DiscordRestClient _discordRestClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly Settings.Discord _discord;

    public DiscordRegistrationService(DiscordRestClient discordRestClient, IServiceScopeFactory serviceScopeFactory, IOptions<Settings.Discord> discordSettings)
    {
        _discordRestClient = discordRestClient;
        _serviceScopeFactory = serviceScopeFactory;
        _discord = discordSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        await _discordRestClient.LoginAsync(TokenType.Bot, _discord.BotToken);
        await _discordRestClient.RegisterCommands(isProduction, _discord);
        
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        var guildRepository = scope.ServiceProvider.GetRequiredService<IGuildRepository>();

        var registeredGuilds = await _discordRestClient.GetGuildsAsync();
        foreach (var registeredGuild in registeredGuilds)
        {
            var guild = await guildRepository.GetByExternalIdAsync(registeredGuild.Id.ToString());
            if (guild is null)
                guildRepository.Add(new Guild(registeredGuild.Id.ToString(), registeredGuild.Name));
            else
            {
                guild.SetName(registeredGuild.Name);
                guildRepository.Update(guild);
            }
            await guildRepository.UnitOfWork.SaveChangesAsync(stoppingToken);
        }
        
    }
}