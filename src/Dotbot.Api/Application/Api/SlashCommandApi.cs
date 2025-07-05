using System.Diagnostics;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ServiceDefaults;

namespace Dotbot.Api.Application.Api;

public static class SlashCommandApi
{
    public static WebApplication MapSlashCommandApi(this WebApplication app)
    {
        app.AddSlashCommand("ping", "Welfare check ping", () => "I'm still responding!");
        app.AddSlashCommand("xkcd", "Fetches an XKCD comic", XkcdCommandAsync);
        app.AddSlashCommand("avatar", "Gets the avatar of the tagged user.", AvatarCommandAsync);
        app.AddSlashCommand("custom", "Retrieves a custom command", GetCustomCommandAsync);
        app.AddModules(typeof(Program).Assembly);

        return app;
    }

    public static async Task<InteractionMessageProperties> XkcdCommandAsync(
        IXkcdService xkcdService,
        Instrumentation instrumentation,
        ILoggerFactory loggerFactory,
        HttpApplicationCommandContext context,
        [SlashCommandParameter(Name = "comic", Description = "Comic number to fetch (blank picks latest)")] int? comicNumber = null)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(ActivityKind.Client);

        var logger = loggerFactory.CreateLogger("XkcdCommand");
        var xkcdComic = await xkcdService.GetXkcdComicAsync(comicNumber, CancellationToken.None);

        logger.LogInformation("Fetching XKCD: {ComicNumber}", comicNumber.ToString() ?? "latest");

        if (comicNumber is null && xkcdComic is null)
            return "There was an issue fetching the XKCD";
        if (xkcdComic is null)
            return $"XKCD comic #{comicNumber} does not exist";

        var comicNumberOrLatestText = (comicNumber is null ? "Latest comic" : "Comic") + $" #{xkcdComic.ComicNumber}";

        var displayName = (context.User as GuildUser)?.Nickname ?? context.User.GlobalName ?? context.User.Username;
        var tags = new TagList
        {
            { "guild_id", context.Interaction.GuildId },
            { "interaction_name", "xkcd" },
            { "comicNumber", xkcdComic.ComicNumber },
            { "member_id", context.User.Id },
            { "member_display_name", displayName }
        };

        foreach (var tag in tags)
            activity?.SetTag(tag.Key, tag.Value);
        instrumentation.XkcdCounter.Add(1, tags);

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

    public static InteractionMessageProperties AvatarCommandAsync(
        Instrumentation instrumentation,
        HttpApplicationCommandContext context,
        GuildUser user,
        [SlashCommandParameter(Name = "global", Description = "Optional flag if you want the user's global avatar instead of the server")] bool globalAvatar = false)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("AvatarCommand", ActivityKind.Client);
        var userAvatarUrl = user.GetAvatarUrl();
        var guildAvatarUrl = user.GetGuildAvatarUrl();

        var avatarUrl = globalAvatar ? userAvatarUrl : guildAvatarUrl ?? userAvatarUrl;

        var displayName = user?.Nickname ?? user?.GlobalName ?? user?.Username;
        var tags = new TagList
        {
            { "guild_id", user?.GuildId },
            { "interaction_name", "avatar" },
            { "member_id", user?.Id },
            { "member_display_name", displayName }
        };

        foreach (var tag in tags)
            activity?.SetTag(tag.Key, tag.Value);
        instrumentation.AvatarCounter.Add(1, tags);

        var embed = new EmbedProperties()
            .WithTitle("Avatar")
            .WithImage(new EmbedImageProperties(avatarUrl?.ToString(512)))
            .WithDescription(displayName);

        return avatarUrl is null ? "No avatar set" : new InteractionMessageProperties().AddEmbeds(embed);
    }

    public static async Task GetCustomCommandAsync(
        ICustomCommandService customCommandService,
        Instrumentation instrumentation,
        ILoggerFactory loggerFactory,
        HttpApplicationCommandContext context,
        [SlashCommandParameter(Name = "command", Description = "Name of the custom command",
            AutocompleteProviderType = typeof(CustomCommandAutocompleteProvider))]
        string commandName)
    {
        using var activity = instrumentation.ActivitySource.StartActivity(ActivityKind.Client);

        var guildId = context.Interaction.GuildId?.ToString();
        if (guildId is null)
        {
            await context.Interaction.SendResponseAsync(InteractionCallback.Message("Cannot use this command outside of a server"));
            return;
        }

        await context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());

        var logger = loggerFactory.CreateLogger("SlashCommandApi");
        logger.LogInformation("Member {member} is retrieving custom command {command}", context.User.Username, commandName);

        var customCommandResponse = await customCommandService.GetCustomCommandAsync(guildId, commandName);

        if (!customCommandResponse.IsSuccess)
            await context.Interaction.SendFollowupMessageAsync(string.Join(" ", customCommandResponse.Errors));
        else
        {
            await context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties().WithContent(customCommandResponse.Value.Content).AddAttachments(customCommandResponse.Value.Attachments));

            var displayName = (context.User as GuildUser)?.Nickname ?? context.User.GlobalName ?? context.User.Username;
            var tags = new TagList
            {
                { "guild_id", guildId },
                { "interaction_name", "custom" },
                { "custom_command_name", commandName },
                { "member_id", context.User.Id },
                { "member_display_name", displayName }
            };

            foreach (var tag in tags)
                activity?.SetTag(tag.Key, tag.Value);

            instrumentation.CustomCommandsFetchedCounter.Add(1, tags);
        }
    }
}
