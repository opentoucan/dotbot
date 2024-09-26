using Bot.Gateway.Extensions;
using Discord;
using Bot.Gateway.Settings;
using Discord.Rest;
using Microsoft.Extensions.Options;

namespace Bot.Gateway.Services;

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
        await _discordRestClient.LoginAsync(TokenType.Bot, _discordSettings.BotToken);
        await _discordRestClient.RegisterCommands(isProduction, _discordSettings);
    }
}