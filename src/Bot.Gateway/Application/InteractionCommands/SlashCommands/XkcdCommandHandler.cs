using System.Text.Json;
using Bot.Gateway.Dto.Requests.Discord;
using Bot.Gateway.Dto.Responses.Discord;
using Bot.Gateway.Infrastructure.HttpClient;
using Discord;
using MediatR;
using Xkcd.Sdk;

namespace Bot.Gateway.Application.InteractionCommands.SlashCommands;

public class XkcdCommandHandler(ILogger<XkcdCommandHandler> logger, XkcdService xkcdService)
    : IRequestHandler<XkcdCommand, InteractionData>
{
    public async Task<InteractionData> Handle(XkcdCommand request, CancellationToken cancellationToken)
    {
        var xkcdComic = await xkcdService.GetXkcdComicAsync(request.ComicNumber, cancellationToken);
        logger.LogInformation($"Fetching XKCD: {request.ComicNumber?.ToString() ?? "latest"}");
        
        if (request.ComicNumber is null && xkcdComic is null)
            return new InteractionData("There was an issue fetching the XKCD");
        if (xkcdComic is null)
            return new InteractionData($"XKCD comic #{request.ComicNumber} does not exist");
        
        var comicNumberOrLatestText = (request.ComicNumber is null ? "Latest comic" : "Comic") + $" #{xkcdComic.ComicNumber}";

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
    public override string InteractionCommandName => "xkcd";
    public override void MapFromInteractionRequest(InteractionRequest interactionRequest)
    {
        ComicNumber = ((JsonElement?)interactionRequest.Data!.Options?.FirstOrDefault()?.Value)?.GetInt32();
    }
    
    public int? ComicNumber { get; private set; }
    
    public XkcdCommand(){}
}