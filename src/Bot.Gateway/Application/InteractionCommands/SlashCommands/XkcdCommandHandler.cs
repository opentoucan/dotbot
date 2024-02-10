using System.Text.Json;
using Bot.Gateway.Model.Requests.Discord;
using Bot.Gateway.Model.Responses.Discord;
using Discord;
using MediatR;
using Xkcd.Sdk;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class XkcdCommandHandler(ILogger<XkcdCommandHandler> logger, XkcdService xkcdService)
    : IRequestHandler<XkcdCommand, InteractionData>
{
    public async Task<InteractionData> Handle(XkcdCommand request, CancellationToken cancellationToken)
    {
        var comicNumber = ((JsonElement?)request.Data.Data!.Options?.FirstOrDefault()?.Value)?.GetInt32();
        
        var xkcdComic = await xkcdService.GetXkcdComicAsync(comicNumber, cancellationToken);
        logger.LogInformation($"Fetching XKCD: {comicNumber?.ToString() ?? "latest"}");
        
        if (comicNumber is null && xkcdComic is null)
            return new InteractionData("There was an issue fetching the XKCD");
        if (xkcdComic is null)
            return new InteractionData($"XKCD comic #{comicNumber} does not exist");
        
        var comicNumberOrLatestText = (comicNumber is null ? "Latest comic" : "Comic") + $" #{xkcdComic.ComicNumber}";

        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithImageUrl(xkcdComic.ImageUrl);
        embedBuilder.WithTitle(comicNumberOrLatestText);
        embedBuilder.AddField("Title", xkcdComic.Title, true)
                    .AddField("Published", xkcdComic.DatePosted.Date.ToShortDateString(), true)
                    .AddField("Alt text", xkcdComic.AltText, true);
        return new InteractionData(embeds: [embedBuilder.Build()]);
    }
}

public class XkcdCommand : InteractionCommand
{
    public override BotCommandType CommandType => BotCommandType.Xkcd;
    public override InteractionRequest Data { get; set; } = null!;
}