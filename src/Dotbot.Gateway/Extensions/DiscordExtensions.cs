using Discord;
using Discord.Interactions;
using Discord.Rest;
using Dotbot.Gateway.Application.InteractionCommands;
using Dotbot.Gateway.Application.InteractionCommands.SlashCommands;
using Dotbot.Gateway.HttpClient;
using Dotbot.Gateway.Settings;

namespace Dotbot.Gateway.Extensions;

public static class DiscordExtensions
{
    public static IHostApplicationBuilder ConfigureDiscordServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IDiscordWebhookClientFactory, DiscordWebhookClientFactory>();
        builder.Services.AddScoped<IInteractionCommandFactory, InteractionCommandFactory>();
        builder.Services.AddScoped<InteractionCommand, PingCommand>();
        builder.Services.AddScoped<InteractionCommand, AvatarCommand>();
        builder.Services.AddScoped<InteractionCommand, SaveCustomCommand>();
        builder.Services.AddScoped<InteractionCommand, RetrieveCustomCommand>();
        builder.Services.AddScoped<InteractionCommand, XkcdCommand>();
        builder.Services.AddScoped<IDiscordClient, DiscordRestClient>();
        
        builder.Services.AddHttpClient(
            "discord",
            client =>
            {
                client.BaseAddress = new Uri("https://discord.com");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
        builder.Services
            .AddSingleton(new DiscordRestConfig())
            .AddSingleton<DiscordRestClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordRestClient>()));

        builder.Services.AddOptions<DiscordSettings>().Bind(builder.Configuration.GetSection("DiscordSettings"));
        builder.Services.AddTransient<IDiscordHttpRequestHelper, DiscordHttpRequestHelper>();
        
        return builder;
    }
    public static async Task RegisterCommands(this DiscordRestClient client, bool isProduction, bool healthCheckMode, DiscordSettings discordSettings)
    {
        if (healthCheckMode)
            return;
        
        if (!isProduction)
            await client.BulkOverwriteGuildCommands(GetInteractionCommands().ToArray(), (ulong)discordSettings.TestGuild!);
        else
            await client.BulkOverwriteGlobalCommands(GetInteractionCommands().ToArray());
    }
    
    public static IEnumerable<ApplicationCommandProperties> GetInteractionCommands()
    {
        yield return new SlashCommandBuilder()
            .WithName("avatar")
            .WithDescription("Gets a user's avatar")
            .AddOption("user", ApplicationCommandOptionType.User, "User", true)
            .Build();
        
        yield return new SlashCommandBuilder()
            .WithName("ping")
            .WithDescription("Pings me")
            .Build();

        yield return new SlashCommandBuilder()
            .WithName("save")
            .WithDescription("Saves a custom bot command")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("text")
                .WithDescription("Text only save command")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("name", ApplicationCommandOptionType.String, "Name of the custom command", true, minLength: 1, maxLength: 50)
                .AddOption("text", ApplicationCommandOptionType.String,"Text input for the command", true, minLength: 1, maxLength: 2000))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("file")
                .WithDescription("File only save command")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("name", ApplicationCommandOptionType.String, "Name of the custom command", true, minLength: 1, maxLength: 50)
                .AddOption("file", ApplicationCommandOptionType.Attachment, "Files for the command", true))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("both")
                .WithDescription("Both text and file save command")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("name", ApplicationCommandOptionType.String, "Name of the custom command", true, minLength: 1, maxLength: 50)
                .AddOption("text", ApplicationCommandOptionType.String,"Text input for the command", true, minLength: 1, maxLength: 2000)
                .AddOption("file", ApplicationCommandOptionType.Attachment, "File for the command", true))
            .Build();

        yield return new SlashCommandBuilder()
            .WithName("custom")
            .WithDescription("Runs a custom bot command")
            .AddOption("command", ApplicationCommandOptionType.String, "Name of the custom command", true, isAutocomplete: true, minLength:1)
            .Build();

        yield return new SlashCommandBuilder()
            .WithName("xkcd")
            .WithDescription("Returns an XKCD comic by comic number")
            .AddOption("number", ApplicationCommandOptionType.Integer, "Comic number", false)
            .Build();
        
        yield return new UserCommandBuilder()
            .WithName("userCmdTest")
            .Build();

        yield return new MessageCommandBuilder()
            .WithName("msgCmdTest")
            .Build();

    }
}