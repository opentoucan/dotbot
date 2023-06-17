using Dotbot.Discord.Models;
using Dotbot.Discord.Services;
using FluentResults;

namespace Dotbot.Discord.CommandHandlers;

/*
public class XkcdBotCommandHandler: BotCommandHandler
{
    public XkcdBotCommandHandler(IXkcdService xkcdService)
    {
        _xkcdService = xkcdService;
    }
    public override CommandType CommandType => CommandType.Xkcd;
    public override Privilege PrivilegeLevel => Privilege.Base;


    private readonly IXkcdService _xkcdService;
    
    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var strings = content.Split(' ');
        Result<XkcdComic> comic;
        var comicNum = 0;
        var hasComicNum = strings.Length > 1 && int.TryParse(strings[1], out comicNum);
        if (hasComicNum)
        {
            comic = await _xkcdService.GetComic(comicNum);
        }
        else
        {
            comic = await _xkcdService.GetLatestComic();
        }

        if (comic.IsFailed)
        {
            await context.SendFormattedMessageAsync(FormattedMessage.Error("Failed to retrieve latest comic"));
            return Result.Fail("Failed to retrieve latest comic");
        }

        await context.SendFormattedMessageAsync(FormattedMessage.XkcdMessage(comic.Value, !hasComicNum));
        return Result.Ok();
    }
}
*/