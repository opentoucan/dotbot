using Dotbot.Gateway.Services;
using NetCord;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Dotbot.Gateway.Application.Api;

public static class SlashCommandApi
{
    public static WebApplication MapSlashCommandApi(this WebApplication app)
    {
        app.AddSlashCommand("ping", "Welfare check ping", () => "I'm still alive!");
        app.AddSlashCommand("xkcd", "Fetches an XKCD comic", XkcdCommandAsync);
        app.AddSlashCommand("avatar", "Gets the avatar of the tagged user.", AvatarCommandAsync);
        app.AddSlashCommand("custom", "Retrieves a custom command", GetCustomCommandAsync);
        app.AddModules(typeof(Program).Assembly);

        return app;
    }

    public static async Task<InteractionMessageProperties> XkcdCommandAsync(
        IXkcdService xkcdService, 
        ILoggerFactory loggerFactory, 
        HttpApplicationCommandContext context,
        [SlashCommandParameter(Name = "comic", Description = "Comic number to fetch (blank picks latest)")] int? comicNumber = null)
    {
        var logger = loggerFactory.CreateLogger("XkcdCommand");
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
    
    public static InteractionMessageProperties AvatarCommandAsync(
        GuildUser user,
        [SlashCommandParameter(Name = "global", Description = "Optional flag if you want the user's global avatar instead of the server")] bool globalAvatar = false)
    {
        var userAvatarUrl = user.GetAvatarUrl();
        var guildAvatarUrl = user.GetGuildAvatarUrl();
        var avatarUrl = globalAvatar ? userAvatarUrl : guildAvatarUrl ?? userAvatarUrl;

        var embed = new EmbedProperties()
            .WithTitle("Avatar")
            .WithImage(new EmbedImageProperties(avatarUrl?.ToString(512)))
            .WithDescription(user.Nickname ?? user.Username);
        return avatarUrl is null ? "No avatar set" : new InteractionMessageProperties().AddEmbeds(embed);
    }

    public static async Task GetCustomCommandAsync(
        ICustomCommandService customCommandService,
        HttpApplicationCommandContext context,
        [SlashCommandParameter(Name = "command", Description = "Name of the custom command",
            AutocompleteProviderType = typeof(CustomCommandAutocompleteProvider))]
        string commandName)
    {
        var guildId = context.Interaction.GuildId?.ToString();
        if (guildId is null)
        {
            await context.Interaction.SendResponseAsync(InteractionCallback.Message("Cannot use this command outside of a server") );
            return;
        }
        
        await context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());

        var customCommandResponse = await customCommandService.GetCustomCommandAsync(guildId, commandName);
        if (!customCommandResponse.IsSuccess)
            await context.Interaction.SendFollowupMessageAsync(string.Join(" ", customCommandResponse.Errors));
        else
            await context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties().WithContent(customCommandResponse.Value.Content).AddAttachments(customCommandResponse.Value.Attachments));
        
    }
}