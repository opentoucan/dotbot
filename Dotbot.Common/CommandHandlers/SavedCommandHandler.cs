using System.Drawing;
using Dotbot.Common.Models;
using Dotbot.Database.Entities;
using Dotbot.Database.Repositories;
using FluentResults;

namespace Dotbot.Common.CommandHandlers;

public class SavedCommandHandler: BotCommandHandler
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

        var countDbResult = await _botCommandRepository.GetCommandCount();

        var formattedMessage = new FormattedMessage();

        if (countDbResult.IsSuccess && countDbResult.Value != 0 && page >= 0)
        {
            var commandCount = countDbResult.Value;

            var pages = Math.Ceiling((decimal)commandCount / MaxPageSize);

            if (page <= pages)
            {
                formattedMessage.Title = "Saved commands";
                formattedMessage.Description = $"Page {page} of {pages} pages ({commandCount} saved commands)";
                formattedMessage.Color = Color.FromArgb(157, 3, 252);

                var commands = await _botCommandRepository.GetCommands(page - 1, MaxPageSize);

                if (commands.IsSuccess)
                {
                    formattedMessage.Fields.AddRange(commands.Value.Select(x => new FormattedMessage.Field
                    {
                        Name = x.Key,
                        Value = x.Type == BotCommand.CommandType.FILE ? x.FileName : x.Content
                    }).ToList());
                }
            }
            else
            {
                formattedMessage.Title = "Error";
                formattedMessage.Color = Color.Red;
                formattedMessage.Description = "Invalid page";  
            }
        }
        else
        {
            formattedMessage.Title = "Error";
            formattedMessage.Color = Color.Red;
            formattedMessage.Description = page < 0 ? "Invalid page" : "No commands found";  
        }
        
        await context.SendFormattedMessageAsync(formattedMessage);

        return Result.Ok();
    }
}