using Discord;
using Discord.Rest;
using Dotbot.Gateway.Extensions;
using Dotbot.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace Dotbot.Gateway.Services;

public class DiscordRegistrationService : BackgroundService
{
    private readonly DiscordRestClient _discordRestClient;
    private readonly Settings.Discord _discord;

    public DiscordRegistrationService(DiscordRestClient discordRestClient, IOptions<Settings.Discord> discordSettings)
    {
        _discordRestClient = discordRestClient;
        _discord = discordSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
        await _discordRestClient.LoginAsync(TokenType.Bot, _discord.BotToken);
        await _discordRestClient.RegisterCommands(isProduction, _discord);
    }
}