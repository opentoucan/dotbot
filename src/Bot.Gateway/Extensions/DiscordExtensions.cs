using Bot.Gateway.Application.InteractionCommands;
using Bot.Gateway.Application.InteractionCommands.SlashCommands;
using Bot.Gateway.Application.InteractionCommands.UserCommands;
using Bot.Gateway.Infrastructure.HttpClient;
using Bot.Gateway.Infrastructure.Repositories;
using Bot.Gateway.Settings;
using Discord;
using Discord.Interactions;
using Discord.Rest;

namespace Bot.Gateway.Extensions;

public static class DiscordExtensions
{
    public static IHostApplicationBuilder ConfigureDiscordServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBotCommandRepository, BotCommandRepository>();
        builder.Services.AddSingleton<IDiscordWebhookClientFactory, DiscordWebhookClientFactory>();
        builder.Services.AddSingleton<IBotCommandFactory, SlashCommandFactory>();
        builder.Services.AddSingleton<InteractionCommand, PingCommand>();
        builder.Services.AddSingleton<InteractionCommand, AvatarCommand>();
        builder.Services.AddSingleton<InteractionCommand, GreetCommand>();
        builder.Services.AddSingleton<InteractionCommand, SaveCustomCommand>();
        builder.Services.AddSingleton<InteractionCommand, RetrieveCustomCommand>();
        
        builder.Services.AddHttpClient(
            "discord",
            client =>
            {
                client.BaseAddress = new Uri("https://discord.com");
            });
        builder.Services
            .AddSingleton(new DiscordRestConfig())
            .AddSingleton<DiscordRestClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordRestClient>()));

        builder.Services.AddOptions<DiscordSettings>();
        builder.Services.Configure<DiscordSettings>(builder.Configuration.GetSection("DiscordSettings"));
        builder.Services.AddTransient<IDiscordHttpRequestHelper, DiscordHttpRequestHelper>();
        
        return builder;
    }
    public static async Task RegisterCommands(this DiscordRestClient client, IHostApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
            await client.BulkOverwriteGuildCommands(GetCommands().ToArray(),
                builder.Configuration.GetValue<ulong>("DiscordSettings:TestGuild"));
        else
            await client.BulkOverwriteGlobalCommands(GetCommands().ToArray());
    }
    
    private static IEnumerable<ApplicationCommandProperties> GetCommands()
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

        yield return new UserCommandBuilder()
            .WithName("userCmdTest")
            .Build();

        yield return new MessageCommandBuilder()
            .WithName("msgCmdTest")
            .Build();

    }
}