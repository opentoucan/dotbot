using System.Text;
using System.Text.Json;
using Discord.Application.Models;
using Discord.Services;
using FluentResults;
using static FluentResults.Result;

namespace Discord.BotCommandHandlers;

public class SaveBotCommandHandler : BotCommandHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFileService _fileService;

    public SaveBotCommandHandler(IHttpClientFactory httpClientFactory, IFileService fileService)
    {
        _httpClientFactory = httpClientFactory;
        _fileService = fileService;
    }

    public override CommandType CommandType => CommandType.Save;
    public override Privilege PrivilegeLevel => Privilege.Base;

    protected override async Task<Result> ExecuteAsync(string content, IServiceContext context)
    {
        var dotbotHttpClient = _httpClientFactory.CreateClient("DotbotApiGateway");
        var httpClient = _httpClientFactory.CreateClient();
        var split = content.Split(' ');
        var key = split[1];
        var serverId = await context.GetServerId();
        string commandContent;
        var botCommandType = BotCommandType.String;

        if (await context.HasAttachments())
        {
            var attachments = (await context.GetAttachments()).First();
            botCommandType = BotCommandType.File;
            commandContent = $"{attachments.Filename}";
            var fileStream = await httpClient.GetStreamAsync(attachments.Url);
            var fileUploadResult = await _fileService.SaveFile(commandContent, fileStream);
            if (fileUploadResult.IsFailed) return Fail(fileUploadResult.Errors);
        }
        else
        {
            if (split.Length < 3)
            {
                await context.SendMessageAsync("No content given");
                return Fail("No content given");
            }

            commandContent = string.Join(" ", split[2..]);
        }

        var saveBotCommand = new SaveBotCommand
        {
            ServiceId = serverId,
            CommandType = botCommandType.Id,
            Content = commandContent,
            CreatorId = (await context.GetAuthorId()).ToString(),
            Name = key
        };
        var stringContent = new StringContent(JsonSerializer.Serialize(saveBotCommand), Encoding.UTF8, "application/json");
        var result = await dotbotHttpClient.PutAsync("save", stringContent);
        if (!result.IsSuccessStatusCode)
        {
            var errorMessage = "Failed to save command";
            await context.SendMessageAsync(errorMessage);
            return Fail(errorMessage);
        }

        await context.SendMessageAsync($"Saved command as {key}");

        return Ok();
    }
}