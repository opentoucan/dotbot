using System.Text.Json;
using Discord.Application.BotCommands;
using Discord.Application.Models;
using Discord.Discord;
using FluentResults;
using MediatR;

namespace Discord.Application.BotCommandHandlers;


public class XkcdBotCommandHandler: IRequestHandler<XkcdBotCommand, bool>
{
    private readonly IHttpClientFactory _httpClientFactory;
    public XkcdBotCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<bool> Handle(XkcdBotCommand request, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("DotbotApiGateway");
        
        var strings = request.Content.Split(' ');
        XkcdComic comic;
        var comicNumber = 0;
        var hasComicNum = strings.Length > 1 && int.TryParse(strings[1], out comicNumber);
        if (hasComicNum)
        {
            comic = await httpClient.GetFromJsonAsync<XkcdComic>($"api/v1/XkcdCommand/{comicNumber}",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        else
        {
            comic = await httpClient.GetFromJsonAsync<XkcdComic>("api/v1/XkcdCommand/latest",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        if (comic == null)
        {
            await request.ServiceContext.SendFormattedMessageAsync(FormattedMessage.Error("Failed to retrieve latest comic"));
            return false;//Result.Fail("Failed to retrieve latest comic");
        }

        await request.ServiceContext.SendFormattedMessageAsync(FormattedMessage.XkcdMessage(comic, !hasComicNum));
        return true;
    }
}  