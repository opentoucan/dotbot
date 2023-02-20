using System.Drawing;
using Dotbot.Common.Models;
using Dotbot.Database.Entities;
using Dotbot.Database.Repositories;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Common.CommandHandlers;

public class SavedCommandHandler : BotCommandHandler
{
    private const int MaxPageSize = 20;
    private readonly IBotCommandRepository _botCommandRepository;

    public SavedCommandHandler(IBotCommandRepository botCommandRepository)
    {
        _botCommandRepository = botCommandRepository;
    }

    public override CommandType CommandType => CommandType.Saved;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var split = content.Split(' ');
        var page = 1;

        if (split.Length > 1 && !int.TryParse(split[1], out page)) page = 1;

        var serverId = await context.GetServerId();

        var countDbResult = await _botCommandRepository.GetCommandCount(serverId);
        
        if (countDbResult.IsSuccess && countDbResult.Value != 0 && page >= 0)
        {
            var commandCount = countDbResult.Value;

            var pages = Math.Ceiling((decimal)commandCount / MaxPageSize);

            if (page <= pages)
            {
                var formattedMessage = FormattedMessage
                    .Info()
                    .SetTitle("Saved Commands")
                    .SetDescription($"Page {page} of {pages} pages ({commandCount} saved commands)")
                    .SetColor(Color.FromArgb(157, 3, 252));
                
                var commands = await _botCommandRepository.GetCommands(serverId, page - 1, MaxPageSize);

                if (commands.IsSuccess)
                {
                    commands.Value.ForEach(x =>
                        formattedMessage.AddField(x.Key,
                            x.Type == BotCommand.CommandType.FILE ? x.FileName : x.Content.Length > 40 ? x.Content[..40] : x.Content, true));
                }
                await context.SendFormattedMessageAsync(formattedMessage);
            }
            else
            {
                const string error = "Invalid page";
                await context.SendFormattedMessageAsync(FormattedMessage.Error(error));
                return Fail(error);
            }
        }
        else
        {
            var error = page < 0 ? "Invalid page" : "No commands found";
            await context.SendFormattedMessageAsync(FormattedMessage.Error(error));
            return Fail(error);
        }

        return Ok();
    }
}