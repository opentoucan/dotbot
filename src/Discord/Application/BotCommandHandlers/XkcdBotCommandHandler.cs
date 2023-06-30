using System.Text.Json;
using Discord.Application.Models;
using FluentResults;

namespace Discord.BotCommandHandlers;


public class XkcdBotCommandHandler: BotCommandHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    public XkcdBotCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public override CommandType CommandType => CommandType.Xkcd;
    public override Privilege PrivilegeLevel => Privilege.Base;

    
    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var httpClient = _httpClientFactory.CreateClient("DotbotApiGateway");
        
        var strings = content.Split(' ');
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
            await context.SendFormattedMessageAsync(FormattedMessage.Error("Failed to retrieve latest comic"));
            return Result.Fail("Failed to retrieve latest comic");
        }

        await context.SendFormattedMessageAsync(FormattedMessage.XkcdMessage(comic, !hasComicNum));
        return Result.Ok();
    }
 
}  