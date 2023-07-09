using System.Text;
using System.Text.Json;
using Discord.Application.BotCommandHandlers;
using Discord.Application.Models;
using Discord.Application.Services;
using FluentResults;
using MediatR;
using static FluentResults.Result;

namespace Discord.Application.BotCommands;

public class SaveBotCommandHandler : IRequestHandler<SaveBotCommand, bool>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IFileService _fileService;

    public SaveBotCommandHandler(IHttpClientFactory httpClientFactory, IFileService fileService)
    {
        _httpClientFactory = httpClientFactory;
        _fileService = fileService;
    }

    public async Task<bool> Handle(SaveBotCommand request, CancellationToken cancellationToken)
    {
        var dotbotHttpClient = _httpClientFactory.CreateClient("DotbotApiGateway");
        var httpClient = _httpClientFactory.CreateClient();
        var split = request.Content.Split(' ');
        var key = split[1];
        var serverId = await request.ServiceContext.GetServerId();
        string commandContent;
        var botCommandType = BotCommandType.String;

        if (await request.ServiceContext.HasAttachments())
        {
            var attachments = (await request.ServiceContext.GetAttachments()).First();
            botCommandType = BotCommandType.File;
            commandContent = $"{attachments.Filename}";
            var fileStream = await httpClient.GetStreamAsync(attachments.Url);
            var fileUploadResult = await _fileService.SaveFile(commandContent, fileStream);
            if (fileUploadResult.IsFailed) return false;
        }
        else
        {
            if (split.Length < 3)
            {
                await request.ServiceContext.SendMessageAsync("No content given");
                return false;
            }

            commandContent = string.Join(" ", split[2..]);
        }

        var saveBotCommand = new Models.SaveBotCommand
        {
            ServiceId = serverId,
            CommandType = botCommandType.Id,
            Content = commandContent,
            CreatorId = (await request.ServiceContext.GetAuthorId()).ToString(),
            Name = key
        };
        var stringContent = new StringContent(JsonSerializer.Serialize(saveBotCommand), Encoding.UTF8, "application/json");
        var result = await dotbotHttpClient.PutAsync("save", stringContent);
        if (!result.IsSuccessStatusCode)
        {
            var errorMessage = "Failed to save command";
            await request.ServiceContext.SendMessageAsync(errorMessage);
            return false;
        }

        await request.ServiceContext.SendMessageAsync($"Saved command as {key}");

        return true;
    }
}