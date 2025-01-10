using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Dotbot.Gateway.Application.Queries;
using Dotbot.Gateway.Services;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Dotbot.Gateway.Application;

public class DiscordCommandsModule(
    IGuildQueries guildQueries,
    IFileUploadService fileUploadService,
    IOptions<Settings.Discord> discordSettings,
    IXkcdService xkcdService,
    ILogger<DiscordCommandsModule> logger)
    : ApplicationCommandModule<HttpApplicationCommandContext>
{
    [SlashCommand("ping", "Pings the bot.")]
    public static string Ping() => "Pong!";

    [SlashCommand("avatar", "Gets the avatar of the tagged user.")]
    public InteractionMessageProperties Avatar(
        GuildUser user,
        [SlashCommandParameter(Name = "global", Description = "Optional flag if you want the user's global avatar instead of the server")] bool globalAvatar = false)
    {
        var avatarUrl = globalAvatar ? user.GetAvatarUrl() : user.GetGuildAvatarUrl();
        return new InteractionMessageProperties()
            .AddEmbeds(new EmbedProperties()
                .WithTitle("Avatar")
                .WithImage(new EmbedImageProperties(avatarUrl?.ToString(512)))
                .WithDescription(user.Nickname ?? user.Username));
    }

    [SlashCommand("custom", "Retrieves a custom command")]
    public async Task CustomCommand(
        [SlashCommandParameter(Name = "command", Description = "Name of the custom command", AutocompleteProviderType = typeof(CustomCommandAutocompleteProvider))] string commandName)
    {
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());

        var guildId = Context.Interaction.GuildId.ToString() ?? string.Empty;
        var customCommandsInServer = await guildQueries.GetAllCustomCommands(guildId);

        var matchingCommand = customCommandsInServer.FirstOrDefault(cc => cc.Name == commandName);
        if (matchingCommand is null)
        {
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties()
                .WithContent($"No custom command exists matching '{commandName}'"));
            return;
        }
        var discordFileAttachments = new List<AttachmentProperties>();
        if (!matchingCommand.Attachments.Any())
        {
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties()
                .WithContent(matchingCommand.Content ?? "No content for this command"));
            return;
        }

        var file = await fileUploadService.GetFile($"{discordSettings.Value.BucketEnvPrefix}-discord-{guildId}", matchingCommand.Attachments.First().Name, CancellationToken.None);
        if (file == null)
        {
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties()
            .WithContent("Failed to retrieve the file for this command"));
            return;
        }

        using var memoryStream = new MemoryStream();
        await file.FileContent.CopyToAsync(memoryStream, CancellationToken.None);

        discordFileAttachments.Add(new AttachmentProperties(file.Filename, new MemoryStream(memoryStream.ToArray())));

        await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties()
            .WithContent(matchingCommand.Content)
            .AddAttachments(discordFileAttachments));
    }

    [SlashCommand("xkcd", "Fetches an XKCD comic")]
    public async Task<InteractionMessageProperties> XkcdCommand(int? comicNumber = null)
    {
        var xkcdComic = await xkcdService.GetXkcdComicAsync(comicNumber, CancellationToken.None);
        logger.LogInformation($"Fetching XKCD: {comicNumber.ToString() ?? "latest"}");

        if (comicNumber is null && xkcdComic is null)
            return "There was an issue fetching the XKCD";
        if (xkcdComic is null)
            return $"XKCD comic #{comicNumber} does not exist";

        var comicNumberOrLatestText = (comicNumber is null ? "Latest comic" : "Comic") + $" #{xkcdComic.ComicNumber}";

        return new InteractionMessageProperties()
            .AddEmbeds(new EmbedProperties()
                .WithTitle(comicNumberOrLatestText)
                .WithImage(new EmbedImageProperties(xkcdComic.ImageUrl))
                .AddFields(new List<EmbedFieldProperties>
                {
                    new()
                    {
                        Name = "Title", Value = xkcdComic.Title, Inline = true
                    },
                    new()
                    {
                        Name = "Published", Value = xkcdComic.DatePosted.Date.ToShortDateString(), Inline = true
                    },
                    new()
                    {
                        Name = "Alt text", Value = xkcdComic.AltText, Inline = true
                    }
                }));
    }

    [MessageCommand("Ad Remover")]
    public string NoAdsCommand(RestMessage message)
    {
        var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var stringBuilder = new StringBuilder();
        var matches = linkParser.Matches(message.Content);
        foreach (Match match in matches)
        {
            stringBuilder.AppendLine($"https://12ft.io/{match.Value}");
        }
        
        return !string.IsNullOrWhiteSpace(stringBuilder.ToString()) ? $"Here's a way to avoid the ad/paywall \n {stringBuilder}"  : "No link in your message";
    }

    [SlashCommand("version", "Gets the version of the bot.", GuildId = 301062316647120896)]
    public string Version()
    {
        var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split("+")[0] ?? "Unknown";
        return version;
    }

}