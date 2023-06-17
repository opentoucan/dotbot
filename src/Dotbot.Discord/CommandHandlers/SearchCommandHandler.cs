using System.Drawing;
using Dotbot.Discord.Models;
using Dotbot.Discord.Services;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers;

public class SearchCommandHandler : BotCommandHandler
{
    public override CommandType CommandType => CommandType.Search;
    public override Privilege PrivilegeLevel => Privilege.Base;
    private readonly IBotCommandsService _botCommandsService;

    public SearchCommandHandler(IBotCommandsService botCommandsService)
    {
        _botCommandsService = botCommandsService;
    }

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var split = content.Split(' ');

        if (split.Length <= 1)
        {
            return Fail("No search term given");
        }

        var result = await _botCommandsService.Search(await context.GetServerId(), split[1]);

        if (result.IsFailed)
        {
            return Fail(result.Errors);
        }
        
        var fm = FormattedMessage
            .Info($"Command - Closeness to {split[1]}")
            .SetTitle("Matched commands")
            .AppendDescription("")
            .SetColor(Color.FromArgb(157, 3, 252));

        foreach (var (value, ratio) in result.Value.OrderByDescending(tuple => tuple.Item2))
        {
            fm.AppendDescription($"{value} - {ratio}%");
        }

        await context.SendFormattedMessageAsync(fm);
        return Ok();
    }
}