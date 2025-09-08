using System.Diagnostics;
using Dotbot.Api.Services;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ServiceDefaults;

namespace Dotbot.Api.Discord.SlashCommandApis;

public static class XkcdSlashCommands
{
    public static async Task<InteractionMessageProperties> FetchXkcdAsync(
        IXkcdService xkcdService,
        Instrumentation instrumentation,
        ILoggerFactory loggerFactory,
        HttpApplicationCommandContext context,
        [SlashCommandParameter(Name = "comic", Description = "Comic number to fetch (blank picks latest)")]
        int? comicNumber = null)
    {
        using var activity = Instrumentation.ActivitySource.StartActivity(ActivityKind.Client);

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
        Instrumentation.XkcdCounter.Add(1, tags);

        return new InteractionMessageProperties()
            .AddEmbeds(new EmbedProperties()
                .WithTitle(comicNumberOrLatestText)
                .WithImage(new EmbedImageProperties(xkcdComic.ImageUrl))
                .AddFields(new()
                {
                    Name = "Title", Value = xkcdComic.Title, Inline = true
                }, new()
                {
                    Name = "Published", Value = xkcdComic.DatePosted.Date.ToShortDateString(), Inline = true
                }, new()
                {
                    Name = "Alt text", Value = xkcdComic.AltText, Inline = true
                }));
    }
}