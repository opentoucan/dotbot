using Dotbot.Gateway.Services;
using NetCord;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Dotbot.Gateway.Extensions;

public static class DiscordExtensions
{
    public static IHostApplicationBuilder ConfigureDiscordServices(this IHostApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(nameof(Settings.Discord));
        var discordSettings = section.Get<Settings.Discord>();
        builder.Services.AddOptions<Settings.Discord>().Bind(section);
        builder.Services.AddHostedService<RegistrationHostedService>();
        builder.Services.AddScoped<RestClient>(_ => new RestClient(new BotToken(discordSettings!.Token)));

        builder.Services.AddDiscordRest()
            .AddApplicationCommands<ApplicationCommandInteraction, HttpApplicationCommandContext, AutocompleteInteractionContext>();
        return builder;
    }
}