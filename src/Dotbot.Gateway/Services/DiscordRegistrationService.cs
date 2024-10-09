using Discord;
using Discord.Rest;
using Dotbot.Gateway.Extensions;
using Dotbot.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.Services;

public class DiscordRegistrationService : BackgroundService
{
    private readonly DiscordRestClient _discordRestClient;
    private readonly DiscordSettings _discordSettings;

    public DiscordRegistrationService(DiscordRestClient discordRestClient, IOptions<DiscordSettings> discordSettings)
    {
        _discordRestClient = discordRestClient;
        _discordSettings = discordSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        var healthCheckMode = _discordSettings.HealthCheckMode;
        await _discordRestClient.LoginAsync(TokenType.Bot, _discordSettings.BotToken);
        await _discordRestClient.RegisterCommands(isProduction, healthCheckMode, _discordSettings);
    }
}