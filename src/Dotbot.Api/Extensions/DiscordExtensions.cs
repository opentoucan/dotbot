using Dotbot.Api.Services;
using Dotbot.Api.Settings;
using NetCord;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Dotbot.Api.Extensions;

public static class DiscordExtensions
{
    public static IHostApplicationBuilder ConfigureDiscordServices(this IHostApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection("Discord");
        var discordSettings = section.Get<DiscordSettings>();
        builder.Services.AddOptions<DiscordSettings>().Bind(section);
        builder.Services.AddHostedService<RegistrationHostedService>();
        builder.Services.AddScoped<RestClient>(_ => new RestClient(new BotToken(discordSettings!.Token)));

        builder.Services.AddDiscordRest()
            .AddApplicationCommands<ApplicationCommandInteraction, HttpApplicationCommandContext, AutocompleteInteractionContext>();
        return builder;
    }
}