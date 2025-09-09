using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Dotbot.Api.Discord.HostedServices;
using Dotbot.Api.Discord.SlashCommandApis;
using Dotbot.Api.Settings;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Hosting.AspNetCore;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace Dotbot.Api.Discord.Extensions;

public static class Extensions
{
    private const string InteractionEndpoint = "/interactions";

    public static IHostApplicationBuilder ConfigureDiscordServices(this IHostApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection("Discord");
        var discordSettings = section.Get<DiscordSettings>();
        var botToken = new BotToken(discordSettings!.Token);
        var restClient = new RestClient(botToken);
        builder.Services.AddOptions<DiscordSettings>().Bind(section);
        builder.Services.AddHostedService<SaveDiscordServersHostedService>();
        builder.Services.AddScoped<RestClient>(_ => restClient);

        builder.Services
            .AddDiscordRest()
            .AddHttpApplicationCommands()
            .AddComponentInteractions<ButtonInteraction, HttpButtonInteractionContext>()
            .AddComponentInteractions<StringMenuInteraction, HttpStringMenuInteractionContext>();

        if (builder.Environment.EnvironmentName == "local")
            builder.Services.AddHostedService<DiscordHttpInteractionSetupService>();

        return builder;
    }

    public static void ConfigureDiscordWebApplication(this WebApplication app)
    {
        app.UseHttpInteractions(InteractionEndpoint);
        app.AddSlashCommand("ping", "Welfare check ping", () => "I'm still responding!");
        app.AddSlashCommand("xkcd", "Fetches an XKCD comic", XkcdSlashCommands.FetchXkcdAsync);
        app.AddSlashCommand("avatar", "Gets the avatar of the tagged user.", AvatarSlashCommands.FetchAvatarAsync);
        app.AddSlashCommand("custom", "Retrieves a custom command", CustomCommandSlashCommands.FetchCustomCommandAsync);
        app.AddModules(typeof(Program).Assembly);
    }

    internal class DiscordHttpInteractionSetupService(
        IHttpClientFactory httpClientFactory,
        IOptions<DiscordSettings> settings,
        ILogger<DiscordHttpInteractionSetupService> logger) : IHostedLifecycleService
    {
        public async Task StartedAsync(CancellationToken cancellationToken)
        {
            var httpClient = httpClientFactory.CreateClient();
            var ngrokUrl = "http://localhost:4040/api/tunnels";

            logger.LogInformation("Reading tunnel from ngrok {ngrokUrl}", ngrokUrl);

            var response = await httpClient.GetAsync(ngrokUrl, cancellationToken);

            var ngrokStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var jsonNode = await JsonNode.ParseAsync(ngrokStream, cancellationToken: cancellationToken);
            var publicUrl = jsonNode?["tunnels"]?[0]?["public_url"]?.GetValue<string?>();

            if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(publicUrl))
                throw new Exception(
                    $"Ngrok failed to return a public url ({response.StatusCode}: {response.ReasonPhrase}). Is ngrok running?");

            logger.LogInformation("Ngrok API returned: {publicUrl}", publicUrl);

            var interactionsEndpointUrl = $"{publicUrl}{InteractionEndpoint}";
            var discordInteractionPatch = new JsonObject([
                KeyValuePair.Create<string, JsonNode?>("interactions_endpoint_url", interactionsEndpointUrl)
            ]);

            var content = JsonContent.Create(discordInteractionPatch);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.TryAddWithoutValidation("Authorization", $"Bot {settings.Value.Token}");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", settings.Value.Token);

            logger.LogInformation("Updating Discord bot endpoint with: {interactionEndpoint}", interactionsEndpointUrl);

            var discordApiResponse =
                await httpClient.PatchAsync("https://discord.com/api/applications/@me", content, cancellationToken);

            if (!discordApiResponse.IsSuccessStatusCode)
                throw new Exception(
                    $"Failed to update Discord bot interaction endpoint ({response.StatusCode}: {response.ReasonPhrase}). Is the endpoint publicly accessible and is the token valid?");

            logger.LogInformation("Successfully updated the discord bot interaction endpoint");
        }

        public Task StoppedAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StartingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StoppingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}