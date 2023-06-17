using Dotbot.Database.Repositories;
using Dotbot.Database.Services;
using FluentResults;
using static FluentResults.Result;

namespace Dotbot.Discord.CommandHandlers;

public class SaveBotCommandHandler : BotCommandHandler
{
    private readonly IBotCommandRepository _botCommandRepository;
    private readonly HttpClient _httpClient;
    private readonly IFileService _fileService;

    public SaveBotCommandHandler(IBotCommandRepository botCommandRepository, HttpClient httpClient,
        IFileService fileService)
    {
        _botCommandRepository = botCommandRepository;
        _httpClient = httpClient;
        _fileService = fileService;
    }

    public override CommandType CommandType => CommandType.Save;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var split = content.Split(' ');
        var key = split[1];
        if (await context.HasAttachments())
        {
            var attachments = (await context.GetAttachments()).First();

            var fileStream = await _httpClient.GetStreamAsync(attachments.Url);

            var serverId = await context.GetServerId();

            //TODO: This should really be called in the Commands service
            var fileUploadResult = await _fileService.SaveFile($"{serverId}:{attachments.Filename}:{key}", fileStream);
            if (fileUploadResult.IsFailed) return Fail(fileUploadResult.Errors);

            var result = await _botCommandRepository.SaveCommand(serverId, (await context.GetAuthorId()).ToString(),
                key, attachments.Filename, fileStream, true);
            if (result.IsFailed)
            {
                await context.SendMessageAsync("Failed to save command");
            }
        }
        else
        {
            if (split.Length < 3)
            {
                await context.SendMessageAsync("No content given");
                return Fail("No content given");
            }

            var commandContent = string.Join(" ", split[2..]);
            var result = await _botCommandRepository.SaveCommand(await context.GetServerId(),
                (await context.GetAuthorId()).ToString(), key, commandContent, true);
            if (result.IsFailed)
            {
                await context.SendMessageAsync($"Failed to save command:");
            }

            ;
        }

        await context.SendMessageAsync($"Saved command as {key}");

        return Ok();
    }
}