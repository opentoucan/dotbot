using Dotbot.Database.Entities;
using Dotbot.Database.Repositories;
using Dotbot.Database.Services;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers;

public class DefaultBotCommandHandler : BotCommandHandler
{
    private readonly IBotCommandRepository _botCommandRepository;
    private readonly IFileService _fileService;

    public DefaultBotCommandHandler(IBotCommandRepository botCommandRepository, IFileService fileService)
    {
        _botCommandRepository = botCommandRepository;
        _fileService = fileService;
    }

    public override CommandType CommandType => CommandType.Default;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var messageSplit = content.Split(' ');

        var key = messageSplit[0];
        var command = await _botCommandRepository.GetCommand(await context.GetServerId(), key);

        if (command.IsSuccess)
        {
            return command.Value.Type switch
            {
                BotCommand.CommandType.STRING => await HandleString(command.Value, context),
                BotCommand.CommandType.FILE => await HandleFile(command.Value, context),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        await context.SendMessageAsync($"No command {key} found");

        return Fail($"No command {key} found");
    }

    private async Task<Result> HandleFile(BotCommand command, IServiceContext context)
    {
        if (command.Type != BotCommand.CommandType.FILE) return Fail("Command is not a file");
        var fileStream = await _fileService.GetFile($"{command.ServiceId}:{command.FileName}:{command.Key}");
        if (fileStream.IsFailed)
        {
            await context.SendMessageAsync($"Cannot find file content for {command.Key}");
            return Fail($"Cannot find file content for {command.Key}");
        }

        await context.SendFileAsync(command.FileName, fileStream.Value);
        return Ok();
    }

    private static async Task<Result> HandleString(BotCommand command, IServiceContext context)
    {
        await context.SendMessageAsync(command.Content);
        return Ok();
    }
    
}